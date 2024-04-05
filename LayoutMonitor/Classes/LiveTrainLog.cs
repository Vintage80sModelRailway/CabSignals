using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutMonitor.Classes
{
    public class LiveTrainLog
    {
        public string Name { get; set; }
        public string TrainName { get; set; }
        public string PreviousBlock { get; set; }
        public string CurrentBlock { get; set; }
        public string NextBlock { get; set; }
        public string NextNextBlock { get; set; }
        public List<string> History { get; set; }
        public DateTime LastUpdated { get; set; }

    }
}
