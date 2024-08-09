using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class BlockToDecorate
    {
        public string BlockUserName { get; set; }
        public bool SetToAlternate { get; set; }
        public int Position { get; set; }
        public bool Processed { get; set; }

    }
}
