using JMRIReader.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LayoutMonitor.Classes
{
    public class LiveTrainLog
    {
        public string Name { get; set; }
        public string DCCiD { get; set; }
        public string PreviousBlock { get; set; }
        public string CurrentBlock { get; set; }
        public int AutomatedCurrentBlockIndex { get; set; }
        public List<BlockJourneyLog> AutomatedBlockList { get; set; }
        public string NextBlock { get; set; }
        public string NextNextBlock { get; set; }
        public List<string> History { get; set; }
        public DateTime LastUpdated { get; set; }
        public BlockNavigationLog CurrentBlockBNL { get; set; }
        public BlockNavigationLog NextBlockBNL { get; set; }
        public BlockNavigationLog TwoBlocksBNL { get; set; }
        public List<string> AllocatedBlocks { get; set; }
        public bool IsAutomated { get; set; }

        public bool Terminated { get; set; }
        public string TerminatedReason { get; set; }
        public int ShortBlockRetries { get; set; }
        public DateTime ShortBlockLastQuery { get; set; }
        public bool ProcessingNewBlock { get; set; }

    }
}
