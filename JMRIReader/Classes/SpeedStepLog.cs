using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JMRIReader.Classes.Enums;

namespace JMRIReader.Classes
{
    public  class SpeedStepLog
    {
        public int SpeedStep { get; set; }
        public decimal SpeedMMS { get; set; }
        public DateTime start { get; set; }
        public TrainDirection Direction { get; set; }
    }
}
