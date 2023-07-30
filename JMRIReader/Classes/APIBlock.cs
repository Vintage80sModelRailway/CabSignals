using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class APIBlock
    {
        public string name { get; set; }
        public string userName { get; set; }
        public string comment { get; set; }
        public string[] properties { get; set; }
        public int state { get; set; }
        public string value { get; set; }
        public string sensor { get; set; }
        public string reporter { get; set; }
        public string speed { get; set; }
        public int curvature { get; set; }
        public int direction { get; set; }
        public float length { get; set; }
        public bool permissive { get; set; }
        public float speedLimit { get; set; }
        public string[] denied { get; set; }
    }
}
