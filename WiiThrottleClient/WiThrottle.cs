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
        private string _ip;
        private int _port;
        private string _name;
        private Roster _roster;
        private List<Throttle> _throttles;
        public WiThrottle(string ServerAddress, int ServerPort, string ThrottleName)
        {
            _roster = new Roster();
            _roster.RosterList = new List<RosterEntry>();
            _roster.Turnouts = new List<Turnout>();
            _throttles = new List<Throttle>();
            _ip = ServerAddress;
            _port = ServerPort;
            _name = ThrottleName;
            Connect();
        }

        private async void Connect()
        {
            _wi = new TcpClient();
            await _wi.ConnectAsync(_ip,_port);
            NetworkStream stream = _wi.GetStream();

            var message = "N"+_name+"\n";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            await stream.WriteAsync(messageBytes, 0, messageBytes.Count());

            if (_wi.Available > 0)
            {
                var data = new Byte[_wi.Available];
                Int32 bytes = await stream.ReadAsync(data, 0, data.Length);
                var responseData = Encoding.ASCII.GetString(data, 0, bytes); 
                ProcessRoster(responseData);
            }
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

        public async Task<bool> CheckForMessages()
        {
            bool success = false;
            NetworkStream stream = _wi.GetStream();

            if (_wi.Available > 0)
            {
                var data = new Byte[_wi.Available];
                Int32 bytes = await stream.ReadAsync(data, 0, data.Length);
                var responseData = Encoding.ASCII.GetString(data, 0, bytes);
                ProcessMessage(responseData);
                success = true;
            }
            return success;
        }

        public void ProcessMessage(string message)
        {

        }


        public string GetThrottle()
        {
            return "";
        }


    }
}
