using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JMRIReader.Classes.Enums;

namespace JMRIReader.Classes
{
    public class BlockJourneyLog
    {
        public string BlockSystemname { get; set; }
        public string BlockUserName { get; set; }
        public bool Traversed { get; set; }
        public bool Assigned { get; set; }
        public DateTime LastAllocationTime { get; set; }
        public int Sequence { get; set; }
        public bool HasAlternate { get; set; }
        public bool PossibleAlternate { get; set; }
        public int SectionSequenceId { get; set; }
        public Guid SectionId { get; set; }
        public DateTime TimeTrainEnteredBlock { get; set; }
        public JourneySequenceState SequenceState { get; set; }
        public decimal BlockLengthMM { get; set; }
        public List<SpeedStepLog> SpeedLog { get; set; }
        public AutomatedTrainRunningSpeed SpeedLimit { get; set; }
        public decimal mmCovered { get; set; }
        public bool PreviousBlockExited { get; set; }
        public List<BlockTrigger> BlockTriggers { get; set; }
        public string ForwardStoppingSensor { get; set; }
        public string reverseStoppingSensor { get; set; }
        public string derivedStoppingSensor { get; set; }

        public string OccupationSensorSystemName { get; set; }
        public bool EarlyExitBlock { get; set; }
        public bool StorageBlock { get; set; }
        public bool EmergencyStopOnly { get; set; }

        public BlockJourneyLog Clone()
        {
            return (BlockJourneyLog)MemberwiseClone();
        }

    }
}
