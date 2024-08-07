using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
