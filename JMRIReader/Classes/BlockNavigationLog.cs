using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public  class BlockNavigationLog
    {
        public string BlockFound { get; set; }
        public string BlockChecked { get; set; }
        public bool BlockCheckedIsOccupied { get; set; }
        public bool BlockCheckedIsAllocated { get; set; }
        public string BlockCheckedSystemName { get; set; }
        public string PreviousBlock { get; set; }
        public string StartItem { get; set; }
        public string StartPreviousItem { get; set; }
        public string EdgeConnector { get; set; }
        public string EdgeConnectorDirectionConnector { get; set; }
        public string Breadcrumb { get; set; }
        public string LikelyIssue { get; set; }
        public string OccupiedBy { get; set; }
        public string AllocatedTo { get; set; }
        public string NextBlockEdgeConnector { get; set; }
        public string OriginalAlertBlock { get; set; }
        public bool NoMoreBlocksFound { get; set; }
        public string BlockCheckedAllocatedTo { get; set; }
        public List<BNLTurnout> BNLTurnouts { get; set; }

        public string UsedEdgeConnector { get; set; }
        public string UsedEdgeConnectorDirectionConnector { get; set; }

    }
}
