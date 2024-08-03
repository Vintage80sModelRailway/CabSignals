using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WiThrottleClient.Classes;

namespace WiThrottleClient
{
    public class WiThrottle
    {
        private TcpClient _wi;
        private NetworkStream _stream;
        private string _ip;
        private int _port;
        private string _name;
        private Roster _roster;
        private List<Throttle> _throttles;
        private int _throttleIndex;
        public WiThrottle(string ServerAddress, int ServerPort, string ThrottleName)
        {
            _roster = new Roster();

            _roster.RosterList = new List<RosterEntry>();
            _roster.Turnouts = new List<Turnout>();
            _throttles = new List<Throttle>();
            _ip = ServerAddress;
            _port = ServerPort;
            _name = ThrottleName;
            _throttleIndex = 0;
            Connect();
        }

        private async void Connect()
        {
            _wi = new TcpClient();
            await _wi.ConnectAsync(_ip,_port);
            _stream = _wi.GetStream();

            var message = "N"+_name+"\n";
            WriteToStream(message);

            if (_wi.Available > 0)
            {
                var data = new Byte[_wi.Available];
                Int32 bytes = await _stream.ReadAsync(data, 0, data.Length);
                var responseData = Encoding.ASCII.GetString(data, 0, bytes); 
                ProcessRoster(responseData);
            }
        }

        private async void WriteToStream(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            if (_stream == null) _stream = _wi.GetStream();
            await _stream.WriteAsync(messageBytes, 0, messageBytes.Count());
        }

        private void ProcessRoster(string rosterData)
        {            
            string receivedLine = "";
            string delimeter = "";
            var lines = rosterData.Split('\n');
            foreach (var line in lines)
            {
                if (line == "\r") continue;

                string prefix = "";
                foreach (var c in line)
                {
                    prefix += c;
                    if (prefix == "RL")
                    {
                        ProcessRosterEntries(line);
                        break;
                    }
                    if (prefix == "PTL")
                    {
                        ProcessTurnouts(line);
                        break;
                    }

                }

                var delSplit = line.Split(' ');
            }

        }

        private void ProcessRosterEntries(string line)
        {
            var entries = line.Split(new string[] { "]\\[" }, StringSplitOptions.None);
            foreach (var entry in entries)
            {
                var comps = entry.Split(new string[] { "}|{" }, StringSplitOptions.None);
                if (comps != null && comps.Count() == 3)
                {
                    var re = new RosterEntry();
                    re.ID = comps[1];
                    re.Name = comps[0];
                    re.IDType = comps[2];
                    re.FullID = comps[2] != null && comps[2].Length > 0 ? re.ID+comps[2].Substring(0, 1) : re.ID + comps[2];
                    _roster.RosterList.Add(re);
                }
            }
        }

        private void ProcessTurnouts(string line)
        {
            var entries = line.Split(new string[] { "]\\[" }, StringSplitOptions.None);
            foreach (var entry in entries)
            {
                var comps = entry.Split(new string[] { "}|{" }, StringSplitOptions.None);
                if (comps != null && comps.Count() == 3)
                {
                    var t = new Turnout();
                    t.ID = comps[0];
                    t.Name = comps[1];
                    t.State = comps[2] != null && comps[2].Length > 0 ? comps[2].Substring(0, 1) : comps[2];
                    _roster.Turnouts.Add(t);
                }
            }
        }

        private void ProcessThrottleChange(string line)
        {
            var sides = line.Split(new string[] { "<;>" }, StringSplitOptions.None);
            if (sides == null || sides.Count() < 2) return;
            var value = sides[1];
            if (value == null || value.Length < 2) return;
            var valueHeader = value.Substring(0, 1);
            var mtIndex = line.Substring(1, 1);
            var throttle = _throttles.FirstOrDefault(f => f.mtIndex == mtIndex);
            var suffix = value.Substring(1);
            switch (valueHeader)
            {
                case "s":
                    var testSS = "";
                    break;
                case "R":
                    var testDir = "";
                    throttle.Direction = suffix;
                    break;
                case "V":
                    var testSpeed = "";
                    int speed = -1;
                    var success = int.TryParse(suffix, out speed);
                    if (success)
                    {
                        if (speed < 0) speed = 0;
                        throttle.Speed = speed;
                    }
                        
                    break;
            }
        }

        public async Task<bool> CheckForMessages()
        {
            bool success = false;
            if (_roster.RosterList.Count == 0 || _roster.Turnouts.Count == 0) return false;

            if (_wi.Available > 0)
            {
                var data = new Byte[_wi.Available];
                if (_stream == null) _stream = _wi.GetStream();
                Int32 bytes = await _stream.ReadAsync(data, 0, data.Length);
                var responseData = Encoding.ASCII.GetString(data, 0, bytes);
                ProcessMessage(responseData);
                success = true;
            }
            return success;
        }

        public void ProcessMessage(string message)
        {
            if (message.Length <= 0) return;
            var messages = message.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);
            foreach (var update in messages)
            {
                if (update == null || update.Length < 3) continue;

                if (update.Substring(0, 3) == "PTA")
                {
                    //turnout state update
                    //PTA4MT1015\r\n\r\n

                    string state = update.Substring(3, 1);

                    var turnoutName = update.Substring(4);
                    var existingTurnout = _roster.Turnouts.FirstOrDefault(f => f.ID == turnoutName);
                    if (existingTurnout != null)
                    {
                        existingTurnout.State = state;
                    }

                }
                if (update.Substring(0, 1) == "M")
                {
                    ProcessThrottleChange(update);
                }
            }


        }

        public List<RosterEntry> Roster
        {
            get
            {
                return _roster.RosterList;
            }
        }


        public string GetThrottle(int rosterIndex)
        {
            var alreadyExists = _throttles.FirstOrDefault(f => f.RosterIndex == rosterIndex);
            if (alreadyExists != null)
            {
                return alreadyExists.mtIndex;
            }

            var selectedRosterEntry = _roster.RosterList.ElementAtOrDefault(rosterIndex);
            if (selectedRosterEntry == null)
            {
                return "Not found";
            }

            var nt = new Throttle();
            nt.Name = selectedRosterEntry.Name;
            nt.ID = selectedRosterEntry.ID;
            nt.RosterIndex = rosterIndex;
            nt.mtIndex = _throttleIndex.ToString();
            
            _throttles.Add(nt);
            _throttleIndex++;

            string assign = "M" + nt.mtIndex + "+" + selectedRosterEntry.IDType + selectedRosterEntry.ID + "<;>" + selectedRosterEntry.IDType + selectedRosterEntry.ID + "\n";
            WriteToStream(assign);

            return "";
        }


    }
}
