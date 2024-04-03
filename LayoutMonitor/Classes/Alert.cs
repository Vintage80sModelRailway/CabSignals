using JMRIReader.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutMonitor.Classes
{
    public class Alert
    {
        public Guid id { get; set; }
        public string BlockSystemName { get; set; }
        public string BlockUserName { get; set; }
        public string SignalMastSystemName { get; set; }
        public string SignalMastUserName { get; set; }
        public DateTime AlertStart { get; set; }
        public AlertSeverity Severity { get; set; }
        public bool Visible { get; set; }
        public string PreviousBlockUserName { get; set; }
        public string AffectedBlockUserName { get; set; }
        public string NextBlockUserName { get; set; }
        public string LikelyIssue { get; set; }
        public BlockNavigationLog BNL { get; set; }
        public bool Acknowledged { get; set; }
        public bool Superceded { get; set; }

    }
}
