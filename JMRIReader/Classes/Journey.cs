using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader.Classes
{
    public class Journey
    {
        private string transitName;
        private string trainName;
        private string startBlockName;
        private string endBlockName;
        private string configFilePath;

        private transit thisTransit;
        private ConfigReader config;


        public Journey(string ConfigFilePath, string TransitName, string TrainName, string StartBlockName, string EndBlockName)
        {
            configFilePath = ConfigFilePath;
            transitName = TransitName;
            trainName = TrainName;
            startBlockName = StartBlockName;
            endBlockName = EndBlockName;

            config = new ConfigReader(configFilePath);

            thisTransit = GetTransit();
        }

        public transit GetTransit()
        {
            var tr = config.GetTransit(transitName);


            return tr;
        }


    }
}
