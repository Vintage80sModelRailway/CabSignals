using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class BlockJourneyLog
    {
        public string BlockSystemname { get; set; }
        public string BlockUserName { get; set; }
        public bool Traversed { get; set; }
        public int Sequence { get; set; }
        public bool HasAlternate { get; set; }
        public bool PossibleAlternate { get; set; }

    }
}
