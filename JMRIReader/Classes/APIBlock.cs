using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class APIBlockInitial
    {
        public string name { get; set; }
        public string userName { get; set; }
        public string comment { get; set; }
        public string[] properties { get; set; }
        public int state { get; set; }
        public object value { get; set; }
        public string sensor { get; set; }
        public string reporter { get; set; }
        public string speed { get; set; }
        public int curvature { get; set; }
        public int direction { get; set; }
        public float length { get; set; }
        public bool permissive { get; set; }
        public float speedLimit { get; set; }
        public string[] denied { get; set; }
    }
    public class APIBlock
    {
        public string name { get; set; }
        public string userName { get; set; }
        public string comment { get; set; }
        public string[] properties { get; set; }
        public int state { get; set; }
        public BlockValue value { get; set; }
        public string sensor { get; set; }
        public string reporter { get; set; }
        public string speed { get; set; }
        public int curvature { get; set; }
        public int direction { get; set; }
        public float length { get; set; }
        public bool permissive { get; set; }
        public float speedLimit { get; set; }
        public string[] denied { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class BlockValueData
    {
        public string name { get; set; }
        public string userName { get; set; }
        public string comment { get; set; }
        public List<Property> properties { get; set; }
    }

    public class BlockValueRosterData
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    public class Property
    {
        public string content { get; set; }
    }

    public class BlockValue
    {
        public string type { get; set; }
        public BlockValueData data { get; set; }
    }


}
