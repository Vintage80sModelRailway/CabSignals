using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class Memory
    {
        public string type { get; set; }
        public MemoryData data { get; set; }
    }

    public class MemoryData
    {
        public string name { get; set; }
        public string userName { get; set; }
        public object comment { get; set; }
        public object[] properties { get; set; }
        public string value { get; set; }
    }
}
