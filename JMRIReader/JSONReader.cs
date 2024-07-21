using JMRIReader.Classes;
using JMRIReader.Classes.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

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
                ICollection<BlockRootObjectInitial>  blockRoot = JsonConvert.DeserializeObject<ICollection<BlockRootObjectInitial>>(jsonResponse);
                foreach (var b in blockRoot)
                {
                    var pb = InitialBlockToAPIBlock(b);
                    blocks.Add(pb);
                }
               // blocks = blockRoot.ToList();
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }
            return blocks;
        }

        public BlockRootObject InitialBlockToAPIBlock(BlockRootObjectInitial b)
        {
            var pb = new APIBlock();
            var d = b.data;
            pb.name = d.name;
            pb.curvature = d.curvature;
            pb.properties = d.properties;
            pb.sensor = d.sensor;
            pb.state = d.state;
            pb.comment = d.comment;
            pb.denied = d.denied;
            pb.direction = d.direction;
            pb.length = d.length;
            pb.permissive = d.permissive;
            pb.reporter = d.reporter;
            pb.speed = d.speed;
            pb.speedLimit = d.speedLimit;
            pb.userName = d.userName;

            if (b.data.value != null)
            {
                var vType = b.data.value.GetType();

                if (vType == typeof(String))
                {
                    var newVal = new BlockValue();
                    newVal.data = new BlockValueData();
                    newVal.data.userName = b.data.value.ToString();
                    newVal.type = "Manual";
                    pb.value = newVal;
                }
                else
                {
                    var valString = b.data.value.ToString();
                    var val = JsonConvert.DeserializeObject<BlockValue>(valString);
                    if (val.type == "rosterEntry")
                    {
                        var newVal = new BlockValue();
                        newVal.data = new BlockValueData();
                        var re = JsonConvert.DeserializeObject<BlockRootValueRosterEntry>(valString);
                        newVal.data.userName = re.data.address;
                        newVal.data.comment = re.data.name;
                        newVal.type = val.type;
                        pb.value = newVal;
                    }
                    else
                        pb.value = val;


                }
            }
            var bro = new BlockRootObject();
            bro.data = pb;
            return bro;
        }

        public async Task AllocateBlock(string systemName, string allocatedValue)
        {
            APIAllocationBlock block = new APIAllocationBlock();
            block.value = allocatedValue;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(jmriServer+"/json/block/"+systemName);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
           // string blockString = new JavaScriptSerializer().Serialize(block);

            var blockString = JsonConvert.SerializeObject(block, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    await streamWriter.WriteAsync(blockString);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }



        }

        public void AllocateBlockOld(string systemName, string allocatedValue)
        {
            APIAllocationBlock block = new APIAllocationBlock();
            block.value = allocatedValue;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(jmriServer + "/json/block/" + systemName);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            // string blockString = new JavaScriptSerializer().Serialize(block);

            var blockString = JsonConvert.SerializeObject(block, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.WriteAsync(blockString);
                }
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }

            //var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            //{
            //    var result = streamReader.ReadToEnd();
            //}


        }

        public async Task<BlockRootObject> GetBlock(string UserName)
        {
            BlockRootObject block = new BlockRootObject();

            var response = await client.GetAsync("/json/block/"+UserName);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            try
            {
                var initBlock = JsonConvert.DeserializeObject<BlockRootObjectInitial>(jsonResponse);
                block = InitialBlockToAPIBlock(initBlock);
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }
            return block;
        }

        public async Task<List<BlockRootObject>> GetAssignedBlocks(string TrainName, bool includeOccupied = false)
        {
            List<BlockRootObjectInitial> initBlocks = new List<BlockRootObjectInitial>();
            List<BlockRootObject> blocks = new List<BlockRootObject>();
            var response = await client.GetAsync("/json/block");
            var success = response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            try
            {
                ICollection<BlockRootObjectInitial> blockRoot = JsonConvert.DeserializeObject<ICollection<BlockRootObjectInitial>>(jsonResponse);
                foreach (var b in blockRoot)
                {
                    var pb = InitialBlockToAPIBlock(b);
                    blocks.Add(pb);
                }
                if (includeOccupied)
                {
                    blocks = blocks.Where(w => w.data.value != null && w.data.value.data.userName == TrainName).ToList();
                }
                else
                {
                    blocks = blocks.Where(w => w.data.value != null && w.data.value.data.userName == TrainName && w.data.state == 4).ToList();
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
            List<BlockRootObjectInitial> initBlocks = new List<BlockRootObjectInitial>();
            List<BlockRootObject> blocks = new List<BlockRootObject>();
            var response = await client.GetAsync("/json/block");
            var success = response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            try
            {
                ICollection<BlockRootObjectInitial> blockRoot = JsonConvert.DeserializeObject<ICollection<BlockRootObjectInitial>>(jsonResponse);
                foreach (var b in blockRoot)
                {
                    var pb = InitialBlockToAPIBlock(b);
                    blocks.Add(pb);
                }
                blocks = blocks.Where(w => w.data.state == 2).ToList();
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
                turnout = JsonConvert.DeserializeObject<TurnoutRootobject>(jsonResponse);

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
                sm = JsonConvert.DeserializeObject<SMRootobject>(jsonResponse);

            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }

            return sm;
        }
    }
}
