using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class ViableRouteList
    {
        public List<ViableRouteBlock> Blocks { get; set; }
        public int NumberOfUnavailableBlocks { get; set; }

    }
}
