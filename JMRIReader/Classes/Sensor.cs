using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class SensorRootobject
    {
        public SensorData data { get; set; }
        public string type { get; set; }
    }

    public class SensorData
    {
        public object comment { get; set; }
        public bool inverted { get; set; }
        public string name { get; set; }
        public object[] properties { get; set; }
        public int state { get; set; }
        public string userName { get; set; }
    }

}
