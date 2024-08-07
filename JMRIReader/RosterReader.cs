using JMRIReader.Classes;
using JMRIReader.Classes.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace JMRIReader
{
    public  class RosterReader
    {
        private string filePath;
        XDocument rosterCFG;
        private List<Locomotive> _locos;
        public RosterReader(string FilePath)
        {
            filePath = FilePath;
            rosterCFG = XDocument.Load(filePath);
            _locos = new List<Locomotive>();
        }

        public List<RosterEntry> GetRoster()
        {
            List<RosterEntry> locolist = new List<RosterEntry>();
            XmlSerializer serial = new XmlSerializer(typeof(Locomotive));
            var locos = rosterCFG.Descendants("locomotive");
            //var transit = config.Descendants("transit")
            foreach (var loco in locos)
            {
                var xLoco = (Locomotive)serial.Deserialize(loco.CreateReader());
                _locos.Add(xLoco);
                var re = new RosterEntry();
                re.Name = xLoco.Id;
                re.ID = xLoco.DccAddress;
                locolist.Add(re);
            }

            return locolist;
        }

        public List<Locomotive> FullRoster
        {
            get
            {
                return _locos;
            }
        }

    }
}
