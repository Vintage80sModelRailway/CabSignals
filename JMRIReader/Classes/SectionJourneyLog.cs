using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JMRIReader.Classes.Enums;

namespace JMRIReader.Classes
{
    public class SectionJourneyLog
    {
        public string SectionSystemname { get; set; }
        public string SectionkUserName { get; set; }
        public bool Traversed { get; set; }
        public int Sequence { get; set; }
        public bool HasAlternate { get; set; }
        public bool PossibleAlternate { get; set; }
        public int AllocationFailureCount { get; set; }
        public Guid SectionID { get; set; }
        public AllocationStatus AllocationStatus { get; set; }
        public string AllocationStatusReason { get; set; }
        public List<block> Blocks { get; set; }
        public section Section { get; set; }
        public transitTransitsection TransitSection { get; set; }
        public bool IsAllocated { get; set; }
        public bool AwaitingAllocateion { get; set; }
        public string SignalAspectReason { get; set; }
        public List<BlockNavigationLog> BlockBNLs { get; set; }
        public bool IsTraversed { get; set; }

    }
}
