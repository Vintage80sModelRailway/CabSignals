using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiThrottleClient;

namespace Shuttler
{
    public partial class Shuttler : Form
    {
        public string _JMRIServerIP;
        public string _MQTTServerIP;
        public int _JMRIServerPort;
        public int _MQTTServerPort;
        public int _WiThrottlePort;
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
            WiThrottle c = new WiThrottle(_JMRIServerIP, _WiThrottlePort,"Shuttler");

            /*
            TcpClient wi = new TcpClient();
            await wi.ConnectAsync(_JMRIServerIP, _WiThrottlePort);
            NetworkStream stream =  wi.GetStream();

            var message = "NShuttler\n";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            await stream.WriteAsync(messageBytes, 0, messageBytes.Count());
            // Buffer to store the response bytes.


            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            if (wi.Available > 0)
            {
                var data = new Byte[wi.Available];
                Int32 bytes = await stream.ReadAsync(data, 0, data.Length); //(**This receives the data using the byte method**)
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes); //(**This converts it to string**)
            }
            */
        }


    }
}
