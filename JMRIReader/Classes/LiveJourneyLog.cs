using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class LiveJourneyLog
    {
        public string Name { get; set; }
        public string DCCiD { get; set; }
        public string PreviousBlock { get; set; }
        public string CurrentBlock { get; set; }
        public int AutomatedCurrentBlockIndex { get; set; }
        public string AutomatedTrainDirection { get; set; }
        public bool AutomatedTrainActive { get; set; }
        public List<BlockJourneyLog> AutomatedBlockList { get; set; }
        public int AutomatedCurrentSectionIndex { get; set; }
        public List<SectionJourneyLog> AutomatedSectionList { get; set; }
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

        public bool ProcessingNewBlock { get; set; }
    }
}
