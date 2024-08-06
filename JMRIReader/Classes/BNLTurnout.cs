using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class BNLTurnout
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string CurrentState { get; set; }
        public string RequiredState { get; set; }

        public int NumberOfRetries { get; set; }
    }
}
