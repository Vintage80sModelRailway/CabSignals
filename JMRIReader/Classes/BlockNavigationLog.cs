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
        public string EdgeConnector { get; set; }
        public string EdgeConnectorDirectionConnector { get; set; }
        public string Breadcrumb { get; set; }
    }
}
