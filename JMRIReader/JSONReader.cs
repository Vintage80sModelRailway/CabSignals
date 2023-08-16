using JMRIReader.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JMRIReader
{    
    public class JSONReader
    {
        private string jmriServer;
        private HttpClient client;
        public JSONReader(string ServerIPAddress)
        {
            jmriServer = ServerIPAddress;
            client = new HttpClient();
            client.BaseAddress = new Uri(jmriServer);
        }

        public async Task<List<BlockRootObject>> GetBlocks()
        {
            List<BlockRootObject> blocks = new List<BlockRootObject>();
            var response = await client.GetAsync("/json/block");
            var success = response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            try
            {
                ICollection<BlockRootObject>  blockRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<ICollection<BlockRootObject>>(jsonResponse);
                blocks = blockRoot.ToList();
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }
            return blocks;
        }

        public async Task<BlockRootObject> GetBlock(string UserName)
        {
            BlockRootObject block = new BlockRootObject();
            var response = await client.GetAsync("/json/block/"+UserName);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            try
            {
                block = Newtonsoft.Json.JsonConvert.DeserializeObject<BlockRootObject>(jsonResponse);
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }
            return block;
        }

        public async Task<List<BlockRootObject>> GetAssignedBlocks(string TrainName, bool includeOccupied = false)
        {
            List<BlockRootObject> blocks = new List<BlockRootObject>();
            var response = await client.GetAsync("/json/block");
            var success = response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            try
            {
                ICollection<BlockRootObject> blockRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<ICollection<BlockRootObject>>(jsonResponse);
                if (includeOccupied)
                {
                    blocks = blockRoot.Where(w => w.data.value != null && w.data.value == TrainName).ToList();
                }
                else
                {
                    blocks = blockRoot.Where(w => w.data.value != null && w.data.value == TrainName && w.data.state == 4).ToList();
                }
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }
            return blocks;
        }

        public async Task<List<BlockRootObject>> GetOccupiedBlocks()
        {
            List<BlockRootObject> blocks = new List<BlockRootObject>();
            var response = await client.GetAsync("/json/block");
            var success = response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            try
            {
                ICollection<BlockRootObject> blockRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<ICollection<BlockRootObject>>(jsonResponse);
                blocks = blockRoot.Where(w => w.data.state == 2).ToList();
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }
            return blocks;
        }

        public async Task<TurnoutRootobject> GetTurnout(string SystemName)
        {
            TurnoutRootobject turnout = new TurnoutRootobject();
            var response = await client.GetAsync("/json/turnout/" + SystemName);
            var success = response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            try
            {
                turnout = Newtonsoft.Json.JsonConvert.DeserializeObject<TurnoutRootobject>(jsonResponse);

            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }

            return turnout;
        }

        public async Task<SMRootobject> GetSignalMast(string SignalMastName)
        {
            SMRootobject sm = new SMRootobject();
            var response = await client.GetAsync("/json/signalMast/" + SignalMastName);
            var success = response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            try
            {
                sm = Newtonsoft.Json.JsonConvert.DeserializeObject<SMRootobject>(jsonResponse);

            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }

            return sm;
        }
    }
}
