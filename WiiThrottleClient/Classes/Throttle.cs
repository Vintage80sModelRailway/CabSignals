using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiThrottleClient.Classes
{
    public class Throttle
    {
        public string mtIndex { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public int Speed { get; set; }
        public string Direction { get; set; }
        public List<string> Functions { get; set; }

        public int RosterIndex { get; set; }
    }
}
