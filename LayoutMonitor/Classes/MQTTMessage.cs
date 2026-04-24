using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutMonitor.Classes
{
    public class MQTTMessage
    {
        public string Topic { get; set; }
        public string Payload { get; set; }

        public bool Retain { get; set; }
    }
}
