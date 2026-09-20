using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class BlockRootObject
    {
        public string type { get; set; }
        public APIBlock data { get; set; }
        public int MultiBlockLogIndex { get; set; }
        public int MultiBlockPriority { get; set; }
        public bool HasMultiBlockSuccessor { get; set; }

        public int ProcessingOrder { get; set; }

        public bool RetrievalError { get; set; }
    }

    public class BlockRootObjectInitial
    {
        public string type { get; set; }
        public APIBlockInitial data { get; set; }
    }

    public class BlockRootValueRosterEntry
    {
        public string type { get; set; }
        public BlockValueRosterData data { get; set; }
    }
}
