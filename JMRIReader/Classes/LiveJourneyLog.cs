using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JMRIReader.Classes.Enums;

namespace JMRIReader.Classes
{
    public class LiveJourneyLog
    {
        public string Name { get; set; }
        public string OriginalName { get; set; }

        public Guid LogId { get; set; }
        public string DCCiD { get; set; }
        public string OriginalDCCiD { get; set; }
        public string TransitName { get; set; }
        public string PreviousBlock { get; set; }
        public int PreviousBlockIndex { get; set; }
        public string CurrentBlock { get; set; }
        public int CurrentBlockIndex { get; set; }
        public int AutomatedCurrentBlockIndex { get; set; }
        public int CurrentSpeedStep { get; set; }
        public bool AutomatedTrainActive { get; set; }
        public List<BlockJourneyLog> AutomatedBlockList { get; set; }
        public List<BlockJourneyLog> AutomatedAlternativeBlockList { get; set; }
        public int AutomatedCurrentSectionIndex { get; set; }
        public List<SectionJourneyLog> AutomatedSectionList { get; set; }
        public List<SectionJourneyLog> AutomatedAlternateSectionList { get; set; }
        public int CurrentAlternateIndex { get; set; }
        public SignalAspect SignalAspect { get; set; }
        public AllocationStatus NextSectionAllocationStatus { get; set; }
        public string NextSectionAllocationStatusReason { get; set; }
        public string SignalAspectReason { get; set; }
        public AutomatedTrainRunningStatus AutomatedTrainRunningStatus { get; set; }
        public string AutomatedTrainStatusReason { get; set; }
        public AutomatedTrainRunningSpeed AutomatedTrainRunningSpeed { get; set; }
        public string AutomatedTrainSpeedReason { get; set; }
        public DateTime StatusLastChanged { get; set; }
        public string NextBlock { get; set; }
        public string NextNextBlock { get; set; }
        public List<string> History { get; set; }
        public DateTime LastUpdated { get; set; }
        public BlockNavigationLog PreviousBlockBNL { get; set; }
        public BlockNavigationLog CurrentBlockBNL { get; set; }
        public BlockNavigationLog NextBlockBNL { get; set; }
        public BlockNavigationLog TwoBlocksBNL { get; set; }
        public List<string> AllocatedBlocks { get; set; }
        public bool IsAutomated { get; set; }
        public bool Terminated { get; set; }
        public string TerminatedReason { get; set; }
        public bool ProcessingNewBlock { get; set; }     
        public DateTime TimeStarted { get; set; }
        public TrainMotionConfig TrainMotionCfg { get; set; }

        public int TrainLengthMM { get; set; }
        public DateTime StartTime { get; set; }
        public bool ReverseWhenDone { get; set; }
        public int ReverseRestartDelaySeconds { get; set; }
        public bool RestartWhenDone { get; set; }
        public string NextTransit { get; set; }
        public TrainDirection NextTransitDirection { get; set; }
        public int NextTransitDelayMS { get; set; }
        public int NextTransitAdditionalDelayMS { get; set; }
        public bool HasSpeedProfile { get; set; }
        public TransitType TransitType { get; set; }
        public int NumberOfSectionsAheadToAllocate { get; set; }

    }
}
