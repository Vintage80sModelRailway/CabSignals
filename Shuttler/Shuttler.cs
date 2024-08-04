using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiThrottleClient;
using WiThrottleClient.Classes;

namespace Shuttler
{
    public partial class Shuttler : Form
    {
        private string _JMRIServerIP;
        private int _JMRIServerPort;
        private int _WiThrottlePort;
        private List<Throttle> _throttles;
        WiThrottle c;

        private bool _isRunning;
        public Shuttler()
        {
            InitializeComponent();
            var cfgWebServerIP = ConfigurationManager.AppSettings["JMRIServerIP"];
            if (cfgWebServerIP != null)
            {
                _JMRIServerIP = cfgWebServerIP.ToString();
            }

            var cfgWiThrottleServerPort = ConfigurationManager.AppSettings["WiTHrottlePort"];
            if (cfgWiThrottleServerPort != null)
            {
                var sPort = cfgWiThrottleServerPort.ToString();
                _WiThrottlePort = int.Parse(sPort);
            }
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            
        }

        private async void RunShuttles()
        {
            c = new WiThrottle(_JMRIServerIP, _WiThrottlePort, "Shuttler");
            lbRoster.Items.Clear();

            while (_isRunning)
            {
                if (lbRoster.Items.Count == 0 && c.Roster.Count > 0)
                {
                    foreach (var t in c.Roster)
                    {
                        lbRoster.Items.Add(t.Name + " (" + t.ID + ")");
                    }
                }
                await c.CheckForMessages();
                await Task.Delay(500);
            }

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _isRunning = true;
            RunShuttles();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _isRunning = false;
        }

        private void lbRoster_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (c == null) return;

            var index = lbRoster.SelectedIndex;
            c.GetThrottle(index);
        }
    }
}
