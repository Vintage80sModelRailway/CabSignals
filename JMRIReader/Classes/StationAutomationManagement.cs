using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class StationAutomationManagement
    {
        public bool StationManagementACRunning { get; set; }
        public bool StationManagementCWRunning { get; set; }
        public int LastACLaunchAttemptSectionIndex { get; set; }
        public int LastCWLaunchAttemptSectionIndex { get; set; }
        public DateTime LastACSensorCheck { get; set; }
        public DateTime LastCWSensorCheck { get; set; }
    }
}
