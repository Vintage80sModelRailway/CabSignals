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
        public string StartItem { get; set; }
        public string StartPreviousItem { get; set; }
        public string EdgeConnector { get; set; }
        public string EdgeConnectorDirectionConnector { get; set; }
        public string Breadcrumb { get; set; }
        public string LikelyIssue { get; set; }

        public string NextBlockEdgeConnector { get; set; }
    }
}
