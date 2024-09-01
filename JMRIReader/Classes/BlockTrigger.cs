using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JMRIReader.Classes.Enums;

namespace JMRIReader.Classes
{
    public class BlockTrigger
    {
        public transitsectionwhen WhenCode { get; set; }
        public transitsectionwhat WhatCode { get; set; }
        public string WhenData { get; set; }
        public string WhatString { get; set; }
        public string WhenString { get; set; }
        public int DelayMilliseconds { get; set; }
        public string TriggerBlock { get; set; }
        public string TransitName { get; set; }
        public TrainDirection TrainsitTrainDirection { get; set; }
        public bool Fired { get; set; }
    }
}
