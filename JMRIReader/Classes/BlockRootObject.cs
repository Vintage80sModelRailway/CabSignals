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
        public bool ReadyToProcess { get; set; }
        public bool Processed { get; set; }
        public int LogIndex { get; set; }

        public int position { get; set; }

        public BlockRootObject PreviousBlock { get; set; }
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
