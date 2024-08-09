using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JMRIReader.Classes.Enums;

namespace JMRIReader.Classes
{
    public class TrainMotionConfig
    {
        public string Name { get; set; }
        public string DCCID { get; set; }
        public int ForwardCrawlSpeedStep { get; set; }
        public int ForwardCautionSpeedStep { get; set; }
        public int ForwardFullSpeedStep { get; set; }
        public int ReverseCrawlSpeedStep { get; set; }
        public int ReverseCautionSpeedStep { get; set; }
        public int ReverseFullSpeedStep { get; set; }
        public int RampUpSpeedStepIncrease { get; set; }
        public int RampUpIntervalMS { get; set; }
        public int RampDownSpeedStepDecrease { get;  set; }
        public int RampDownIntervalMS { get; set; }
        public bool InRampUp { get; set; }
        public bool InRampDown { get; set; }
        public int CurrentSpeedStep { get; set; }
        public int RequiredSpeedStep { get; set; }
        public TrainDirection TrainDirection { get; set; }
        public int TargetSpeedStep { get; set; }
        public DateTime RampSpeedLastSet { get; set; }
        public DateTime CurrentSpeedLastSet { get; set; }
        public DateTime CurrentBlockEntryTime { get; set; }
        public bool IsActive { get; set; }

    }
}
