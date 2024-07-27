using JMRIReader;
using JMRIReader.Classes;
using JMRIReader.Classes.DTO;
using LayoutMonitor.Classes;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Media;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;




namespace LayoutMonitor
{
    public partial class LayoutMonitorForm : Form
    {
        private JSONReader webClient;
        private ConfigReader config;
        private List<BlockRootObject> activeBlocks;
        private List<BlockRootObject> allBlocks;
        private bool monitorRuning;
        private List<Alert> alerts;
        private bool ShowProceedMessages;
        private bool TrackAllocation;
        private int CautionNagFrequencySeconds;
        private int DangerNagFrequencySeconds;
        private List<DeOccupiedBlock> DeoccupiedBlocks;
        private String DispatcherFolder;
        private string MQTTServer;
        private string BlockAllocateTopic;
        private string BlockReleaseTopic;
        private string RosterPath;
        private string CabSignalTopic;
        private List<string> AllocatedBlocks;
        private List<LiveTrainLog> Log;
        private int TrainCounter;
        private bool AllocateBlocks;
        private List<string> ActiveAutomatedTrains;
        // Create a MQTT client factory
        private MqttFactory factory = new MqttFactory();
        private List<RosterEntry> Roster;
        //private List<string> NoValueBlocks = new List<string>();
        private string[] shortBlocks = { "UD Station Approach DS" };

        // Create a MQTT client instance
        MqttClient mqttClient;

        public LayoutMonitorForm()
        {
            mqttClient = (MqttClient)factory.CreateMqttClient();
            ActiveAutomatedTrains = new List<string>();
            InitializeComponent();
            lvUpdates.Columns.Add("Status");
            lvUpdates.HeaderStyle = ColumnHeaderStyle.None;

            var cfgFilePath = ConfigurationManager.AppSettings["ConfigFilePath"];
            if (cfgFilePath != null)
            {
                tbConfigLocation.Text = cfgFilePath.ToString();
            }

            var rosterFilePath = ConfigurationManager.AppSettings["RosterFilePath"];
            if (rosterFilePath != null)
            {
                RosterPath = rosterFilePath.ToString();
            }

            var cfgDispatchesFolder = ConfigurationManager.AppSettings["DispatchesFolder"];
            if (cfgDispatchesFolder != null)
            {
                DispatcherFolder = cfgDispatchesFolder.ToString();
            }

            var cfgWebServerIP = ConfigurationManager.AppSettings["WebServerIP"];
            if (cfgWebServerIP != null)
            {
                tbServerIP.Text = cfgWebServerIP.ToString();
            }

            var cfgWebServerPort = ConfigurationManager.AppSettings["WebServerPort"];
            if (cfgWebServerPort != null)
            {
                tbServerPort.Text = cfgWebServerPort.ToString();
            }

            var cfgShowProceed = ConfigurationManager.AppSettings["ShowProceed"];
            if (cfgShowProceed != null)
            {
                var success = bool.TryParse(cfgShowProceed.ToString(), out ShowProceedMessages);
                if (!success) ShowProceedMessages = false;
            }

            var cfgTrackAllocation = ConfigurationManager.AppSettings["TrackBlockAllocation"];
            if (cfgTrackAllocation != null)
            {
                var success = bool.TryParse(cfgTrackAllocation.ToString(), out TrackAllocation);
                if (!success) TrackAllocation = false;
            }

            var cfgAllocateBlocks = ConfigurationManager.AppSettings["AllocateBlocks"];
            if (cfgAllocateBlocks != null)
            {
                var success = bool.TryParse(cfgAllocateBlocks.ToString(), out AllocateBlocks);
                if (!success) AllocateBlocks = false;
            }

            var cfgCautionNagFrequency = ConfigurationManager.AppSettings["CautionNagSeconds"];
            if (cfgCautionNagFrequency != null)
            {
                var success = int.TryParse(cfgCautionNagFrequency.ToString(), out CautionNagFrequencySeconds);
                if (!success) CautionNagFrequencySeconds = -1;
            }

            var cfgDangerFrequency = ConfigurationManager.AppSettings["DangerNagSeconds"];
            if (cfgDangerFrequency != null)
            {
                var success = int.TryParse(cfgDangerFrequency.ToString(), out DangerNagFrequencySeconds);
                if (!success) DangerNagFrequencySeconds = -1;
            }

            var cfgMQTTServer = ConfigurationManager.AppSettings["MQTTServer"];
            if (cfgMQTTServer != null)
            {
                MQTTServer = cfgMQTTServer.ToString();
            }

            var cfgBlockAllocateTopic = ConfigurationManager.AppSettings["BlockAllocateTopic"];
            if (cfgBlockAllocateTopic != null)
            {
                BlockAllocateTopic = cfgBlockAllocateTopic.ToString();
            }

            var cfgBlockReleaseTopic = ConfigurationManager.AppSettings["BlockReleaseTopic"];
            if (cfgBlockReleaseTopic != null)
            {
                BlockReleaseTopic = cfgBlockReleaseTopic.ToString();
            }

            var cfgCabSignalTOpic = ConfigurationManager.AppSettings["CabSignalTopic"];
            if (cfgCabSignalTOpic != null)
            {
                CabSignalTopic = cfgCabSignalTOpic.ToString();
            }


        }

        private async void btnStartMonitoring_Click(object sender, EventArgs e)
        {
            monitorRuning = true;
            DeoccupiedBlocks = new List<DeOccupiedBlock>();
            Log = new List<LiveTrainLog>();
            TrainCounter = 1;
            config = new ConfigReader(tbConfigLocation.Text);
            webClient = new JSONReader("http://" + tbServerIP.Text + ":" + tbServerPort.Text);
            activeBlocks = await webClient.GetOccupiedBlocks();
            allBlocks = await webClient.GetBlocks();

            alerts = new List<Alert>();
            //lbOutput.Items.Add("Monitoring started");
            ListViewItem item = new ListViewItem();
            item.Text = "Monitoring started";
            item.BackColor = Color.LimeGreen;
            lvUpdates.Items.Add(item);
            lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
            lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);


            var rosterCfG = new RosterReader(RosterPath);
            Roster = rosterCfG.GetRoster();

            //await StartAutomationMonitoring();

            while (monitorRuning)
            {
                await MonitorLayout();
                await ProcessAlerts();
                ProcessDeoccupiedBlocks();
                await Task.Delay(200);

                //await MonitorAutomation();
            }
        }

        private async Task<bool> StartAutomationMonitoring()
        {
            if (mqttClient.IsConnected) return true;

            var clientOptions = new MqttClientOptionsBuilder()
                .WithClientId("AutomationMonitor")
                .WithTcpServer(MQTTServer, 1883)
                .Build();

            var connectResult = await mqttClient.ConnectAsync(clientOptions);
            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("Connected to MQTT broker successfully.");

                // Subscribe to a topic
                await mqttClient.SubscribeAsync("layout/automation/#");

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += e => {
                    var topic = e.ApplicationMessage.Topic;
                    var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    XDocument doc;
                    try
                    {
                        var dPath = DispatcherFolder + message;
                        doc = XDocument.Load(dPath);
                        var test = doc.Element("traininfofile");
                        var traininfo = doc.Element("traininfofile").Element("traininfo");
                        var transitName = traininfo.Attribute("transitname").Value;
                        var trainName = traininfo.Attribute("trainname").Value;
                        var startBlockName = traininfo.Attribute("startblockname").Value;
                        var startBlockSeq = traininfo.Attribute("startblockseq").Value;
                        var endBlockName = traininfo.Attribute("endblockname").Value;
                        var endBlockSeq = traininfo.Attribute("endblockseq").Value;
                        var rEntry = Roster.First(f => f.Name == trainName);
                        var newLog = new LiveTrainLog()
                        {
                            Name = trainName,
                            DCCiD = rEntry.ID,
                            IsAutomated = true,
                            CurrentBlock = startBlockName,
                            History = new List<string>(),
                            AllocatedBlocks = new List<string>()
                        };
                        
                        int updatedBlockIndex = HandleNewAutoTrainNotification(rEntry.ID, transitName,startBlockName,startBlockSeq,endBlockName,endBlockSeq);
                        newLog.AutomatedCurrentBlockIndex = updatedBlockIndex;
                        var transit = config.GetTransit(transitName);
                        newLog.AutomatedBlockList = transit.BlocksInOrder;
                        Log.Add(newLog);
                    }

                    catch (Exception ex)
                    {
                        
                    }

                    ActiveAutomatedTrains.Add(message);
                    ChangeUI($"Received message: {message} - {e.ApplicationMessage.Topic}");
                    return Task.CompletedTask;
                };

            }

            return true;
        }
        /// <summary>
        /// just had notification that a new automated train will start in 30 seconds
        /// </summary>
        /// <param name="trainFilename"></param>
        private int HandleNewAutoTrainNotification(string TrainId,string transitName, string startBlockName,string startBlockSeq, string endBlockName, string endBlockSeq)
        {
            int blockIndex = -1;
            var transit = config.GetTransit(transitName);
            int startBlockIndex = Int32.Parse(startBlockSeq);
            int endBlockIndex = Int32.Parse(endBlockSeq);
            var startBlock = transit.BlocksInOrder.ElementAt(startBlockIndex);
            
            for (int i = startBlockIndex+1; i < startBlockIndex +6 && i < endBlockIndex; i++)
            {
                AllocateBlock(transit.BlocksInOrder.ElementAt(i).BlockSystemname, transit.BlocksInOrder.ElementAt(i).BlockUserName, TrainId);
                blockIndex = i;
            }

            return blockIndex;
        }

        private async void AllocateBlock(string blockSystemName, string blockUserName, string allocateValue)
        {
            //await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + blockUserName, blockUserName, false);
            
            await webClient.AllocateBlock(blockSystemName, allocateValue);
            await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + blockUserName, blockUserName, false);
        }

        public void ChangeUI(string text)
        {
            if (InvokeRequired)
                BeginInvoke(new MethodInvoker(
                    delegate
                    {
                        ChangeUI(text);
                    }
                    ));
            else
            {
                lbOutput.Items.Add(text);
            }
        }
        public byte[] ConvertToByteArray(IList<ArraySegment<byte>> list)
        {
            var bytes = new byte[list.Sum(asb => asb.Count)];
            int pos = 0;

            foreach (var asb in list)
            {
                Buffer.BlockCopy(asb.Array, asb.Offset, bytes, pos, asb.Count);
                pos += asb.Count;
            }

            return bytes;
        }

        private async Task<bool> MonitorLayout()
        {
            var newBlockStates = await webClient.GetBlocks();
            var newActiveBlocks = newBlockStates.Where(w => w.data.state == 2).ToList();
            var oldActiveBlocks = allBlocks.Where(w => w.data.state == 2).ToList();

            var activeBlocks = oldActiveBlocks.Union(newActiveBlocks).ToList();

            var newActiveThisTimeBlocks = newActiveBlocks.Where(p => !oldActiveBlocks.Any(p2 => p2.data.name == p.data.name)).ToList();
            List<BlockRootObject> newBlocksToProcess = new List<BlockRootObject>();

            if (newActiveThisTimeBlocks.Count() > 1)
            {
                lbOutput.Items.Add("Multi block");
                List<int> p1Logs = new List<int>();
                foreach (var nnab in newActiveThisTimeBlocks)
                {
                    nnab.MultiBlockLogIndex = -1;
                    var matchingLog = Log.FirstOrDefault(f => f.NextBlock == nnab.data.userName);
                    if (matchingLog != null)
                    {
                        lbOutput.Items.Add("Added p1 "+nnab.data.userName);
                        nnab.MultiBlockLogIndex = Log.IndexOf(matchingLog);
                        nnab.MultiBlockPriority = 1;
                        nnab.HasMultiBlockSuccessor = false;
                        newBlocksToProcess.Add(nnab);
                        p1Logs.Add(nnab.MultiBlockLogIndex);
                    }
                        
                }
                foreach (var nnab in newActiveThisTimeBlocks)
                {
                    //if this block has a path connection to a block already in the processing list, it will be for the same journey
                    //if it's a multi block its existing log will have been seen in the first for each.
                    //it will be the 'next next block' in an existing log
                    var matchingLog = Log.Any(f => f.NextBlock == nnab.data.userName);
                    bool foundConnection = false;

                    if (!matchingLog)
                    {
                        foreach(var el in Log)
                        {
                            int index = Log.IndexOf(el);
                            if (el.NextNextBlock == nnab.data.userName && p1Logs.Contains(index))
                            {
                                var previousBlockAlreadyInProcessingList = newBlocksToProcess.FirstOrDefault(f => f.MultiBlockLogIndex == index && f.MultiBlockPriority == 1);
                                
                                if (previousBlockAlreadyInProcessingList != null)
                                {

                                    var configBlocku = config.GetBlockByUserName(previousBlockAlreadyInProcessingList.data.userName);
                                    var configBlock = config.GetBlockBySystemName(configBlocku.systemName);
                                    if (configBlock != null)
                                    {
                                        foreach (var pathBlock in configBlock.path)
                                        {
                                            if (pathBlock.block == nnab.data.name)
                                            {
                                                foundConnection = true;
                                                lbOutput.Items.Add("matched p2 " + nnab.data.userName + " via path block matching to log " + el.Name);
                                            }
                                        }
                                        if (foundConnection)
                                        {
                                            nnab.MultiBlockLogIndex = index;
                                            var pbIndex = newActiveThisTimeBlocks.IndexOf(previousBlockAlreadyInProcessingList);
                                            newActiveThisTimeBlocks[pbIndex].HasMultiBlockSuccessor = true;
                                        }
                                    }
                                    
                                }
                            }

                        }
                        if (!foundConnection)
                        {
                            lbOutput.Items.Add("Added p2 with no index " + nnab.data.userName);
                        }
                        nnab.MultiBlockPriority = 2;
                        newBlocksToProcess.Add(nnab);
                    }                        
                }
            }
            else
            {
                foreach (var nnab in newActiveThisTimeBlocks)
                {
                    nnab.MultiBlockPriority = 0;
                    nnab.HasMultiBlockSuccessor = false;
                    nnab.MultiBlockLogIndex = -1;
                    newBlocksToProcess.Add(nnab);
                }
            }

            bool foundNewActiveBlock = false;

            //Pick up newly allocated blocks to automated trains and clean them up
            var allocatedBlocks = newBlockStates.Where(w => w.data.value != null && w.data.value.type == "rosterEntry");
            foreach (var ab in allocatedBlocks)
            {
                //lbOutput.Items.Add("New allocated block detected - cleaning up - " + ab.data.value.data.userName);
                await webClient.AllocateBlock(ab.data.name, ab.data.value.data.userName);
            }

            foreach (var nab in newBlocksToProcess.OrderBy(o => o.MultiBlockPriority).ToList())
            {
                try
                {
                    var previousBlockState = allBlocks.FirstOrDefault(f => f.data.name == nab.data.name);

                    var alreadyExists = oldActiveBlocks.Any(a => a.data.name == nab.data.name);
                    var justDeactivated = DeoccupiedBlocks.Any(a => a.BlockName == nab.data.name);
                    if (!alreadyExists && !justDeactivated)
                    {
                        if (nab.data == null) continue;
                        var (success,reason) = await ProcessNewActiveBlock(nab,activeBlocks);
                        if (!success)
                        {
                            foundNewActiveBlock = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lbOutput.Items.Add("New active block processing exception " + ex.Message);
                }
            }

            //Check current journeys for re-routing

            if (!foundNewActiveBlock)
            {
                try
                {
                    var newLogs = new List<LiveTrainLog>();
                    foreach (var log in Log)
                    {
                        //If a train has stopped, remove its log. A new one will be created when it restarts
                        //Stops erroneous alerts when next block of a stopped train becomes active
                        var secondsSinceLastUpdate = DateTime.Now - log.LastUpdated;
                        if (secondsSinceLastUpdate.Seconds > 35 && !log.IsAutomated)
                        {
                            log.TerminatedReason = "Dormant for 60 seconds";
                            //log.Terminated = true;

                        }

                        //Is the block this train is supposed to be in still active?
                        //If this and the next block are inactive it can be terminated
                        /*
                        var thisLiveBlock = newBlockStates.FirstOrDefault(f => f.data.userName == log.CurrentBlock);
                        var nexLivetBlock = newBlockStates.FirstOrDefault(f => f.data.userName == log.NextBlock);
                        if (thisLiveBlock.data.state == 4 && nexLivetBlock.data.state == 4) //4 = innactive
                        {
                            log.Terminated = true;
                            log.TerminatedReason = "No occupancy in currently tracked blocks";
                        }
                        */


                        if (log.CurrentBlockBNL == null || log.NextBlockBNL == null || log.TwoBlocksBNL == null) continue;
                        else if (log.IsAutomated) continue;


                        var issueFoundThisBlock = false;
                        var issueFoundNextBlock = false;
                        var issueFoundTwoBlocks = false;
                        var issueThisBlockDetails = "";
                        var issueNextBlockDetails = "";
                        var issueTwoBlockDetails = "";
                        var allocateNextBlock = false;
                        var allocateTwoBlocks = false;

                        //var newLog = log;
                        //Go back to the start of the block in case we're joining it in the middle
                        var currentBlockReverse = await NavigateThroughBlockItems(log.CurrentBlockBNL.BlockChecked, log.CurrentBlockBNL.PreviousBlock, log.CurrentBlockBNL.EdgeConnectorDirectionConnector, log.CurrentBlockBNL.EdgeConnector, log.CurrentBlockBNL.EdgeConnector);

                        var currentBlockRoute = await NavigateThroughBlockItems(currentBlockReverse.BlockChecked, currentBlockReverse.PreviousBlock, currentBlockReverse.EdgeConnectorDirectionConnector, currentBlockReverse.EdgeConnector, log.CurrentBlockBNL.EdgeConnector);
                        var nextBlock = await NavigateThroughBlockItems(currentBlockRoute.BlockFound, currentBlockRoute.BlockFound, currentBlockRoute.EdgeConnector, currentBlockRoute.EdgeConnectorDirectionConnector, currentBlockRoute.EdgeConnector);
                        var twoBlock = await NavigateThroughBlockItems(nextBlock.BlockFound, nextBlock.BlockChecked, nextBlock.EdgeConnector, nextBlock.EdgeConnectorDirectionConnector, nextBlock.EdgeConnector);
                        if (currentBlockRoute.BlockFound != log.CurrentBlockBNL.BlockFound)
                        {
                            nextBlock = await NavigateThroughBlockItems(currentBlockRoute.BlockFound, currentBlockRoute.BlockFound, currentBlockRoute.EdgeConnector, currentBlockRoute.EdgeConnectorDirectionConnector, currentBlockRoute.EdgeConnector);
                            twoBlock = await NavigateThroughBlockItems(nextBlock.BlockFound, nextBlock.BlockChecked, nextBlock.EdgeConnector, nextBlock.EdgeConnectorDirectionConnector, nextBlock.EdgeConnector);
                            if (AllocateBlocks)
                            {
                                var sysName = config.GetBlockByUserName(log.CurrentBlockBNL.BlockFound);
                                await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                                await webClient.AllocateBlock(sysName.userName, "");
                                sysName = config.GetBlockByUserName(log.NextBlockBNL.BlockFound);
                                await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                                await webClient.AllocateBlock(sysName.userName, "");

                                log.AllocatedBlocks.Remove(log.CurrentBlockBNL.BlockFound);
                                log.AllocatedBlocks.Remove(log.NextBlockBNL.BlockFound);
                            }

                            allocateNextBlock = true;
                            allocateTwoBlocks = true;

                            var alertsForNextBlock = alerts.Where(w => w.BNL.BlockChecked == nextBlock.BlockChecked && w.TrainName == log.Name);
                            foreach (var alert in alertsForNextBlock)
                            {
                                lbOutput.Items.Add("468 deactivate alert");
                                DeactivateAlert(alert, false);
                            }

                            var alertsForTwoBlock = alerts.Where(w => w.BNL.BlockChecked == twoBlock.BlockChecked && w.TrainName == log.Name);
                            foreach (var alert in alertsForTwoBlock)
                            {
                                lbOutput.Items.Add("475 deactivate alert");
                                DeactivateAlert(alert, false);
                            }
                            if (AllocateBlocks)
                            {
                                log.AllocatedBlocks.Add(currentBlockRoute.BlockFound);
                                log.AllocatedBlocks.Add(nextBlock.BlockFound);
                            }
                        }
                        else if (nextBlock.BlockFound != log.NextBlockBNL.BlockFound)
                        {
                            twoBlock = await NavigateThroughBlockItems(nextBlock.BlockFound, nextBlock.BlockChecked, nextBlock.EdgeConnector, nextBlock.EdgeConnectorDirectionConnector, nextBlock.EdgeConnector);
                            if (AllocateBlocks)
                            {
                                var sysName = config.GetBlockByUserName(log.NextBlockBNL.BlockFound);
                                await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                                await webClient.AllocateBlock(sysName.userName, "");
                                log.AllocatedBlocks.Remove(log.NextBlockBNL.BlockFound);
                                log.AllocatedBlocks.Add(nextBlock.BlockFound);
                            }
                            allocateTwoBlocks = true;
                        }

                        issueFoundThisBlock = !string.IsNullOrEmpty(currentBlockRoute.LikelyIssue);
                        issueFoundNextBlock = nextBlock != null && !string.IsNullOrEmpty(nextBlock.LikelyIssue);
                        issueFoundTwoBlocks = twoBlock != null && !string.IsNullOrEmpty(twoBlock.LikelyIssue);
                        issueThisBlockDetails = currentBlockRoute.LikelyIssue;
                        if (nextBlock != null) issueNextBlockDetails = nextBlock.LikelyIssue;
                        if (twoBlock != null) issueTwoBlockDetails = twoBlock.LikelyIssue;

                        var currentBlockcfg = config.GetBlockByUserName(currentBlockRoute.BlockChecked);
                        //this train could be stopped in a station platform that feeds to a mainline
                        //its next block could be through a turnout that's closed against it
                        //so only check live block allocation if the train has an active path to the next block
                        //if it does have an active path, there will be no turnouts closed against it and therefore no alerts from the BNL checks

                        //only check live blocks every second or two?
                        /*
                        var timeSinceLastLiveUpdate = DateTime.Now - newLog.LastUpdated;
                        var liveCurrentBlock = newActiveBlocks.FirstOrDefault(f => f.data.userName == log.CurrentBlock);

                        //if (nextBlock != null && !issueFoundNextBlock && timeSinceLastLiveUpdate.Milliseconds > 500)
                        if (nextBlock != null && !issueFoundNextBlock && liveCurrentBlock != null && liveCurrentBlock.data.state == 2)
                        {
                            var liveNextBlock = newBlockStates.FirstOrDefault(f => f.data.userName == nextBlock.BlockChecked);
                            //var liveNextBlock = await webClient.GetBlock(nextBlock.BlockChecked);
                            if (liveNextBlock != null && liveNextBlock.data.state == 2)
                            {
                                //if (liveNextBlock.data.value == null || liveNextBlock.data.value == null || liveNextBlock.data.value.data.userName != log.DCCiD)
                                //{
                                    if (!NoValueBlocks.Contains(nextBlock.BlockChecked) && !NoValueBlocks.Contains(nextBlock.BlockFound) && !newActiveBlockNames.Contains(liveNextBlock.data.userName) && !newActiveBlockNames.Contains(nextBlock.BlockChecked))
                                    {
                                        issueFoundNextBlock = true;
                                        if (issueNextBlockDetails == "")
                                            issueNextBlockDetails = "Collision ";
                                        else
                                            issueNextBlockDetails = issueNextBlockDetails + " - Collision ";

                                    }

                                //}

                            }
                            else if (liveNextBlock.data.value != null && liveNextBlock.data.value.data.userName != log.DCCiD && TrackAllocation && liveNextBlock.data.state == 2)
                            {
                                issueFoundNextBlock = true;
                                if (issueNextBlockDetails == "")
                                    issueNextBlockDetails = "Allocated to " + liveNextBlock.data.value;
                                else
                                    issueNextBlockDetails = issueNextBlockDetails + " - Allocated to " + liveNextBlock.data.value;
                            }
                        }

                        //if (twoBlock != null && !issueFoundTwoBlocks && timeSinceLastLiveUpdate.Milliseconds > 1000)
                        if (twoBlock != null && !issueFoundTwoBlocks && liveCurrentBlock != null && liveCurrentBlock.data.state == 2)
                        {
                            var liveTwoBlocks = newBlockStates.FirstOrDefault(f => f.data.userName == twoBlock.BlockChecked);
                            //var liveTwoBlocks = await webClient.GetBlock(twoBlock.BlockChecked);
                            if (liveTwoBlocks != null && liveTwoBlocks.data.state == 2)
                            {

                                //if (liveTwoBlocks.data.value == null || liveTwoBlocks.data.value == null || liveTwoBlocks.data.value.data.userName != log.DCCiD)
                                //{
                                    if (!NoValueBlocks.Contains(twoBlock.PreviousBlock) && !NoValueBlocks.Contains(twoBlock.BlockChecked))
                                    {
                                        issueFoundTwoBlocks = true;
                                        if (issueTwoBlockDetails == "")
                                            issueTwoBlockDetails = "Collision ";
                                        else
                                            issueTwoBlockDetails = issueTwoBlockDetails + " - Collision ";

                                    }
                                //}
                            }

                            else if (liveTwoBlocks.data.value != null && liveTwoBlocks.data.value.data.userName != log.DCCiD && TrackAllocation && liveTwoBlocks.data.state == 4)
                            {
                                issueFoundTwoBlocks = true;
                                if (issueTwoBlockDetails == "")
                                    issueTwoBlockDetails = "Allocated to " + liveTwoBlocks.data.value;
                                else
                                    issueTwoBlockDetails = issueTwoBlockDetails + " - Allocated to " + liveTwoBlocks.data.value;
                            }
                            newLog.LastUpdated = DateTime.Now;
                        }
                        */
                        log.CurrentBlock = currentBlockRoute.BlockChecked;
                        if (nextBlock != null) log.NextBlock = nextBlock.BlockChecked;
                        if (twoBlock != null) log.NextNextBlock = twoBlock.BlockChecked;
                        log.CurrentBlockBNL = currentBlockRoute;
                        log.NextBlockBNL = nextBlock;
                        log.TwoBlocksBNL = twoBlock;

                        if (issueFoundThisBlock)
                        {
                            var alertExists = alerts.Any(a => a.AffectedBlockUserName == currentBlockRoute.BlockChecked && a.Severity == AlertSeverity.Extreme);
                            var existingAlert = alerts.FirstOrDefault(a => a.AffectedBlockUserName == currentBlockRoute.BlockChecked && a.Severity == AlertSeverity.Extreme);
                            if (existingAlert == null)
                            {
                                AddAlert(new Alert()
                                {
                                    id = Guid.NewGuid(),
                                    BlockSystemName = currentBlockcfg.systemName,
                                    BlockUserName = currentBlockRoute.BlockChecked,
                                    SignalMastSystemName = "",
                                    SignalMastUserName = "",
                                    Severity = AlertSeverity.Extreme,
                                    PreviousBlockUserName = currentBlockRoute.PreviousBlock,
                                    NextBlockUserName = currentBlockRoute.BlockFound,
                                    LikelyIssue = issueThisBlockDetails,
                                    BNL = currentBlockRoute,
                                    Deactivated = false,
                                    TrainName = log.Name,
                                    TrainId = log.DCCiD
                                });
                            }
                        }

                        if (issueFoundNextBlock)
                        {
                            //Danger alert
                            var alertExists = alerts.Any(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == nextBlock.BlockChecked && a.Severity == AlertSeverity.Danger);
                            var existingAlert = alerts.FirstOrDefault(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == nextBlock.BlockChecked && a.Severity == AlertSeverity.Danger);
                            if (existingAlert == null)
                            {
                                AddAlert(new Alert()
                                {
                                    id = Guid.NewGuid(),
                                    BlockSystemName = currentBlockcfg.systemName,
                                    BlockUserName = currentBlockRoute.BlockChecked,
                                    SignalMastSystemName = "",
                                    SignalMastUserName = "",
                                    Severity = AlertSeverity.Danger,
                                    PreviousBlockUserName = currentBlockRoute.PreviousBlock,
                                    NextBlockUserName = nextBlock.BlockFound,
                                    LikelyIssue = issueNextBlockDetails,
                                    BNL = nextBlock,
                                    Deactivated = false,
                                    TrainName = log.Name,
                                    TrainId = log.DCCiD
                                });
                            }
                        }
                        else
                        {
                            if (allocateNextBlock && AllocateBlocks)
                            {
                                var nbConfig = config.GetBlockByUserName(currentBlockRoute.BlockFound);
                                await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + nbConfig.userName, nbConfig.userName, false);
                                await webClient.AllocateBlock(nbConfig.userName, "");
                                if (!log.AllocatedBlocks.Contains(currentBlockRoute.BlockFound))
                                    log.AllocatedBlocks.Add(currentBlockRoute.BlockFound);

                            }
                        }
                        if (issueFoundTwoBlocks)
                        {
                            //Caution alert
                            var alertExists = alerts.Any(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == twoBlock.BlockChecked && a.Severity == AlertSeverity.Caution);
                            var existingAlert = alerts.FirstOrDefault(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == twoBlock.BlockChecked && a.Severity == AlertSeverity.Caution);
                            var dangerAlertExistsForNextBlock = alerts.Any(a => a.BNL.BlockChecked == nextBlock.BlockChecked && a.Severity == AlertSeverity.Danger && a.TrainName == log.Name);

                            if (existingAlert == null)
                            {
                                if (!dangerAlertExistsForNextBlock)
                                {
                                    AddAlert(new Alert()
                                    {
                                        id = Guid.NewGuid(),
                                        BlockSystemName = currentBlockcfg.systemName,
                                        BlockUserName = currentBlockRoute.BlockChecked,
                                        SignalMastSystemName = "",
                                        SignalMastUserName = "",
                                        Severity = AlertSeverity.Caution,
                                        PreviousBlockUserName = currentBlockRoute.PreviousBlock,
                                        NextBlockUserName = twoBlock.BlockFound,
                                        LikelyIssue = issueTwoBlockDetails,
                                        BNL = twoBlock,
                                        Deactivated = false,
                                        TrainName = log.Name,
                                        TrainId = log.DCCiD

                                    }); ;
                                }
                            }
                        }
                        else
                        {
                            if (allocateTwoBlocks && AllocateBlocks)
                            {
                                var tbConfig = config.GetBlockByUserName(nextBlock.BlockFound);
                                await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + tbConfig.userName, tbConfig.userName, false);
                                await webClient.AllocateBlock(tbConfig.systemName, log.Name);
                                if (!log.AllocatedBlocks.Contains(nextBlock.BlockFound))
                                    log.AllocatedBlocks.Add(nextBlock.BlockFound);
                            }
                        }

                        //check allocation in case a collision alert has been cleared
                        if (AllocateBlocks)
                        {
                            foreach (var allocation in log.AllocatedBlocks)
                            {
                                var liveBlock = newBlockStates.FirstOrDefault(f => f.data.userName == allocation);
                                if (liveBlock != null && liveBlock.data.state == 4)
                                {
                                    if (liveBlock.data.value != null)
                                    {
                                        await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + liveBlock.data.userName, liveBlock.data.userName, false);
                                        await webClient.AllocateBlock(liveBlock.data.name, log.Name);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lbOutput.Items.Add("Existing state block processing exception " + ex.Message);
                }
            }

            foreach (var log in Log.ToList())
            {
                if (log.Terminated)
                {
                    TerminateTrain(log.Name);
                    ListViewItem item = new ListViewItem();
                    item.Text = log.Name + " terminated - " + log.TerminatedReason;
                    item.BackColor = Color.LimeGreen;
                    lvUpdates.Items.Add(item);
                    lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                    lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }

            allBlocks = newBlockStates;
            activeBlocks = newActiveBlocks;
            return true;
        }

        private void btnStopMonitoring_Click(object sender, EventArgs e)
        {
            //lbOutput.Items.Add("Monitoring stopped");
            monitorRuning = false;
            ListViewItem item = new ListViewItem();
            item.Text = "Monitoring stopped";
            item.BackColor = Color.Red;
            lvUpdates.Items.Add(item);
            lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
            foreach (var log in Log.ToList())
            {
                TerminateTrain(log.Name);
            }
            Log.Clear();
            alerts.Clear();
        }

        private async Task<(bool, string)> ProcessNewActiveBlock(BlockRootObject block, List<BlockRootObject> activeBlocks)
        {
            //new block gone occupied

            //lbOutput.Items.Add("New active block " + blockUserName);
            var thisBlock = config.GetBlockBySystemName(block.data.name);
            bool issueFoundThisBlock = false;
            bool issueFoundNextBlock = false;
            bool issueFoundTwoBlocks = false;
            bool noMoreBlocks = false;
            bool handlingNewTrain = false;
            bool oneConnectedBlockUnoccipied = false;
            bool oneConnectedBlockOccupied = false;
            string connectingAnchorPoint = "";
            string likelyIssueThisBlock = "";
            string likelyIssueNextBlock = "";
            string likelyIssueTwoBlocks = "";
            string likelyNextBlock = "";
            string likelyPreviousBlock = "";
            bool determinedPreviousBlockFromAlerts = false;
            BlockNavigationLog BNLThisBlock = new BlockNavigationLog();
            BlockNavigationLog BNLNextBlock = new BlockNavigationLog();
            BlockNavigationLog BNLTwoBlocks = new BlockNavigationLog();
            List<BlockRootObject> nextBlocks = new List<BlockRootObject>();
            var blockLog = new LiveTrainLog();
            blockLog.History = new List<string>();
            blockLog.AllocatedBlocks = new List<string>();
            BlockRootObject previousBlock = new BlockRootObject();

            LiveTrainLog existingLog = null;

            if (block.data.value != null)
            {
                existingLog = Log.FirstOrDefault(f => f.DCCiD == block.data.value.data.userName);
            }

            if (existingLog == null)
            {
                if (block.MultiBlockLogIndex >=0)
                {
                    existingLog = Log.ElementAtOrDefault(block.MultiBlockLogIndex);
                }
            }

            if (existingLog == null)
            {
               existingLog = Log.OrderByDescending(o => o.LastUpdated).FirstOrDefault(f => f.NextBlock == block.data.userName);
            }

            //if (existingLog == null)
            //{
            //    //in exceptional circumstances - short block and late value addition by JMRI, we can end up processing the second of two active blocks - try to link them up anyway
            //    if (block.data.value == null)
            //    {
            //        existingLog = Log.OrderByDescending(o => o.LastUpdated).FirstOrDefault(f => f.NextNextBlock == block.data.userName);
            //    }
            //}

            if (existingLog == null)
            {
                if (block.data.value != null)
                {
                    var re = Roster.FirstOrDefault(f => f.ID == block.data.value.data.userName);
                    if (re != null)
                    {
                        blockLog.Name = re.Name;
                        blockLog.DCCiD = re.ID;
                    }

                    else
                    {
                        //blockLog.Name = "NK";
                        var index = block.data.userName.IndexOf(' ');
                        var prefix = block.data.userName.Substring(0, index);
                        blockLog.Name = prefix;
                    }
                }
                else
                {
                    var index = block.data.userName.IndexOf(' ');
                    var prefix = block.data.userName.Substring(0, index);
                    blockLog.Name = prefix;
                }                

                ListViewItem item = new ListViewItem();
                item.Text = blockLog.Name + " - started tracking";
                item.BackColor = Color.LimeGreen;

                lbOutput.Items.Add("Started new journey tracking for " + blockLog.Name);
                lvUpdates.Items.Add(item);
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                ddlTrainSelector.Items.Add(blockLog.Name);
            }
            else
            {
                blockLog = existingLog;
                blockLog.ProcessingNewBlock = true;
                if (block.data.value != null && (block.data.value.data.userName != blockLog.DCCiD || block.data.value.data.comment != blockLog.Name))
                {
                    lbOutput.Items.Add("Name change on value acquisition - " + blockLog.Name + " & " + blockLog.DCCiD + " - to " + block.data.value.data.comment + " & " + block.data.value.data.userName);
                    if (blockLog != null && blockLog.Name != null && blockLog.Name != "" && ddlTrainSelector.Items.Contains(blockLog.Name))
                    {
                        ddlTrainSelector.Items.Remove(blockLog.Name);
                    }
                    blockLog.Name = block.data.value.data.comment;
                    blockLog.DCCiD = block.data.value.data.userName;
                    ddlTrainSelector.Items.Add(blockLog.Name);
                }
            }

            if (ddlTrainSelector.Text == blockLog.Name)
                lbJourneyLog.Items.Add(block.data.userName);

                //if this is an automated train, extend the allocation to pre-allocation blocks
            if (blockLog.IsAutomated)
            {
                if (blockLog.AutomatedCurrentBlockIndex < blockLog.AutomatedBlockList.Count-1)
                {
                    blockLog.AutomatedCurrentBlockIndex++;
                    AllocateBlock(blockLog.AutomatedBlockList.ElementAt(blockLog.AutomatedCurrentBlockIndex).BlockSystemname, blockLog.AutomatedBlockList.ElementAt(blockLog.AutomatedCurrentBlockIndex).BlockUserName, blockLog.DCCiD);
                }
            }

            if (blockLog.AllocatedBlocks != null && blockLog.AllocatedBlocks.Contains(block.data.userName))
                blockLog.AllocatedBlocks.Remove(block.data.userName);

            string connector1 = "";
            string connector2 = "";
            string previousConnector = "";
            string breadcrumbStart = "";
            var trackSegments = config.GetTracksegmentsForBlock(block.data.userName).OrderBy(o => o.Ident).ToList();
            var ts = trackSegments.FirstOrDefault();
            if (ts != null)
            {
                connector1 = ts.Connect1name;
                connector2 = ts.Connect2name;
                previousConnector = ts.Ident;
                breadcrumbStart = ts.Ident;
            }
            if (ts == null)
            {
                if (blockLog.CurrentBlockBNL == null) return (false, "Current block BNL null");
                //could be a DS or turnout
                //need previous item
                var prev = blockLog.CurrentBlockBNL.EdgeConnectorDirectionConnector;
                var turnouts = config.GetTurnoutsInBlock(block.data.userName);
                var to = turnouts.FirstOrDefault();
                if (to != null)
                {
                    connector1 = to.Connectaname;
                    connector2 = to.Connectbname;
                    previousConnector = prev;
                    breadcrumbStart = to.Ident;
                }
                else
                {
                    var slips = config.GetSlipsInBlock(block.data.userName);
                    var slip = slips.FirstOrDefault();
                    if (slip != null)
                    {
                        connector1 = slip.Ident;
                        connector2 = slip.Connectaname;
                        previousConnector = prev;
                        breadcrumbStart = slip.Ident;
                    }
                }
            }
            if (connector1 == "" || connector2 == "" || previousConnector == "")
                return (false, "No connectors");

            var firstBoundaryFromMiddle = await NavigateThroughBlockItems(block.data.userName, likelyPreviousBlock, connector1, previousConnector, breadcrumbStart);
            if (firstBoundaryFromMiddle == null)
            {
                return (false, "First boundary null");
            }

            //if first boundary from middle has a warning - turnout closed against - we know we've gone the wrong way.
            //need to go the other way
            else if (firstBoundaryFromMiddle.LikelyIssue != null && firstBoundaryFromMiddle.LikelyIssue.Contains("AGAINST"))
            {
                firstBoundaryFromMiddle = await NavigateThroughBlockItems(block.data.userName, likelyPreviousBlock, connector2, previousConnector, firstBoundaryFromMiddle.EdgeConnector);
            }


            var secondBoundaryFromMiddle = await NavigateThroughBlockItems(block.data.userName, likelyPreviousBlock, connector2, previousConnector, firstBoundaryFromMiddle.EdgeConnector);
            if (secondBoundaryFromMiddle == null)
            {
                return (false, "Second boundary from middle null");
            }
            var secondBoundary = await NavigateThroughBlockItems(block.data.userName, likelyPreviousBlock, firstBoundaryFromMiddle.EdgeConnectorDirectionConnector, firstBoundaryFromMiddle.EdgeConnector, firstBoundaryFromMiddle.EdgeConnector);
            if (secondBoundary == null)
            {
                return (false, "Second boundary null");
            }

            var firstBoundary = await NavigateThroughBlockItems(block.data.userName, likelyPreviousBlock, secondBoundary.EdgeConnectorDirectionConnector, secondBoundary.EdgeConnector, secondBoundary.EdgeConnector);
            if (firstBoundary == null)
            {
                return (false, "First boundary null");
            }

            int numberOfOccupiedBlocks = 0;
            List<string> LikelyPreviousBlocks = new List<string>();
            if (!block.HasMultiBlockSuccessor)
            {
                foreach (var connectedBlock in thisBlock.path)
                {
                    var configBlock = config.GetBlockBySystemName(connectedBlock.block);
                    var livePathBlock = activeBlocks.FirstOrDefault(f => f.data.name == connectedBlock.block);
                    if (livePathBlock != null && livePathBlock.data != null && livePathBlock.data.state == 2) //occupied
                    {
                        numberOfOccupiedBlocks++;
                        oneConnectedBlockOccupied = true;
                        if (livePathBlock.data.userName == firstBoundary.BlockFound || livePathBlock.data.userName == secondBoundary.BlockFound)
                        {
                            LikelyPreviousBlocks.Add(livePathBlock.data.userName);
                        }
                    }
                    else
                    {
                        oneConnectedBlockUnoccipied = true;
                    }
                }

                if ((numberOfOccupiedBlocks == thisBlock.path.Count() || !oneConnectedBlockUnoccipied) && !determinedPreviousBlockFromAlerts && handlingNewTrain)
                {
                    issueFoundNextBlock = true;
                    likelyIssueNextBlock = "Collision all surrounding blocks occupied ";
                    BNLNextBlock.PreviousBlock = block.data.userName;
                    BNLNextBlock.BlockChecked = block.data.userName;
                }

                if (!oneConnectedBlockOccupied)
                {
                    lbOutput.Items.Add("No connected active blocks, done nothing for " + block.data.userName);
                    return (false, "No connected occupied blocks");
                }
            }


            //if (!string.IsNullOrEmpty(blockLog.CurrentBlock)) //&& thisBlock.path.Any(a => a.block == blockLog.NextNextBlock))
            //{
            //    likelyPreviousBlock = blockLog.CurrentBlock;
            //}
            //else
            //{
            //get from log?
            if (!string.IsNullOrEmpty(blockLog.CurrentBlock))
            {
                likelyPreviousBlock = blockLog.CurrentBlock;
            }
            else if (LikelyPreviousBlocks.Count == 1)
            {
                likelyPreviousBlock = LikelyPreviousBlocks.First();
            }
            else if (LikelyPreviousBlocks.Count > 1)
            {
                List<string> ActiveCollisionAlertBlocks = alerts.Where(w => w.Severity == AlertSeverity.Caution && w.LikelyIssue.Contains("Collision")).Select(s => s.BNL.BlockChecked).ToList();
                foreach (var lpb in LikelyPreviousBlocks)
                {
                    if (!ActiveCollisionAlertBlocks.Contains(lpb))
                    {
                        likelyPreviousBlock = lpb;
                        determinedPreviousBlockFromAlerts = true;
                    }
                }
            }
            //}


            if (likelyPreviousBlock == "")
            {
                likelyPreviousBlock = blockLog.CurrentBlock;
            }

            blockLog.PreviousBlock = likelyPreviousBlock;
            blockLog.History.Add(block.data.userName);
            blockLog.CurrentBlock = block.data.userName;

            if (blockLog.PreviousBlock == blockLog.CurrentBlock)
            {
                return (false, "Previous log block = current log block");
            }


            if (!issueFoundNextBlock)
            {
                var bnl = await NavigateThroughBlockItems(block.data.userName, likelyPreviousBlock, firstBoundary.EdgeConnectorDirectionConnector, firstBoundary.EdgeConnector, firstBoundary.EdgeConnector);
                if (bnl.BlockFound == likelyPreviousBlock)
                {
                    BNLThisBlock = await NavigateThroughBlockItems(block.data.userName, likelyPreviousBlock, bnl.EdgeConnectorDirectionConnector, bnl.EdgeConnector, bnl.EdgeConnector);
                }
                else
                {
                    BNLThisBlock = bnl;
                }

                likelyNextBlock = BNLThisBlock.BlockFound;

                //don't process if newly active block is surrounded by active blocks - most likely a detection issue
                var nextBlockLive = await webClient.GetBlock(likelyNextBlock);
                var previousBlockLive = await webClient.GetBlock(likelyPreviousBlock);
                if (nextBlockLive != null && previousBlockLive != null)
                {
                    if (nextBlockLive.data.state == 2 && previousBlockLive.data.state == 2) return (false,"Surrounded by active blocks");
                }

                blockLog.NextBlock = likelyNextBlock;
                blockLog.CurrentBlockBNL = bnl;
                BNLThisBlock.BlockCheckedSystemName = block.data.userName;

                if (BNLThisBlock.EdgeConnectorDirectionConnector.StartsWith("A"))
                {
                    connectingAnchorPoint = BNLThisBlock.EdgeConnectorDirectionConnector;
                }
                if (!string.IsNullOrEmpty(BNLThisBlock.LikelyIssue))
                {
                    likelyIssueThisBlock = BNLThisBlock.LikelyIssue;
                    issueFoundThisBlock = true;
                }
                blockLog.CurrentBlockBNL = BNLThisBlock;
                if (BNLThisBlock.NoMoreBlocksFound)
                {
                    noMoreBlocks = true;
                }

                BNLNextBlock = await NavigateThroughBlockItems(likelyNextBlock, BNLThisBlock.BlockChecked, BNLThisBlock.EdgeConnector, BNLThisBlock.EdgeConnectorDirectionConnector, BNLThisBlock.EdgeConnector);
                if (BNLNextBlock != null && !noMoreBlocks)
                {
                    blockLog.NextNextBlock = BNLNextBlock.BlockFound;
                    blockLog.NextBlockBNL = BNLNextBlock;

                    if (!String.IsNullOrEmpty(BNLNextBlock.LikelyIssue))
                    {
                        issueFoundNextBlock = true;
                        likelyIssueNextBlock = BNLNextBlock.LikelyIssue + " ";
                    }
                    var liveNextBlock = await webClient.GetBlock(likelyNextBlock);
                    if (liveNextBlock != null && liveNextBlock.data != null)
                    {
                        BNLNextBlock.BlockCheckedSystemName = liveNextBlock.data.name;
                        if ((liveNextBlock.data.value == null || liveNextBlock.data.value == null || liveNextBlock.data.value.data.userName != blockLog.DCCiD) && liveNextBlock.data.state == 2)//occupied
                        {
                            issueFoundNextBlock = true;
                            likelyIssueNextBlock += "Collision ";// in " + liveNextBlock.data.userName;
                        }
                        if (liveNextBlock.data.value != null && TrackAllocation && liveNextBlock.data.value.data.userName != blockLog.DCCiD && TrackAllocation && liveNextBlock.data.state == 4)
                        {
                            issueFoundNextBlock = true;
                            BNLNextBlock.BlockCheckedAllocatedTo = liveNextBlock.data.value.data.userName;
                            likelyIssueNextBlock += "Allocated to " + liveNextBlock.data.value + " ";
                        }
                    }

                    if (!blockLog.AllocatedBlocks.Contains(liveNextBlock.data.name))
                        blockLog.AllocatedBlocks.Add(liveNextBlock.data.userName);

                    BNLTwoBlocks = await NavigateThroughBlockItems(BNLNextBlock.BlockFound, BNLNextBlock.BlockChecked, BNLNextBlock.EdgeConnector, BNLNextBlock.EdgeConnectorDirectionConnector, BNLNextBlock.EdgeConnector);
                    if (BNLTwoBlocks != null)
                    {
                        blockLog.TwoBlocksBNL = BNLTwoBlocks;
                        if (!String.IsNullOrEmpty(BNLTwoBlocks.LikelyIssue))
                        {
                            issueFoundTwoBlocks = true;
                            likelyIssueTwoBlocks = BNLTwoBlocks.LikelyIssue + " ";
                        }
                        var twoBlocksLiveBlock = await (webClient.GetBlock(BNLNextBlock.BlockFound));
                        if (twoBlocksLiveBlock != null && twoBlocksLiveBlock.data != null)
                        {
                            BNLTwoBlocks.BlockCheckedSystemName = twoBlocksLiveBlock.data.name;
                            if ((twoBlocksLiveBlock.data.value == null || twoBlocksLiveBlock.data.value == null || twoBlocksLiveBlock.data.value.data.userName != blockLog.DCCiD) && twoBlocksLiveBlock.data.state == 2)//occupied
                            {
                                issueFoundTwoBlocks = true;
                                likelyIssueTwoBlocks = "Collision ";// in "+twoBlocksLiveBlock.data.userName;
                            }
                            if (twoBlocksLiveBlock.data.value != null && TrackAllocation && twoBlocksLiveBlock.data.value.data.userName != blockLog.DCCiD && TrackAllocation && twoBlocksLiveBlock.data.state == 4)
                            {
                                issueFoundTwoBlocks = true;
                                likelyIssueTwoBlocks += "Allocated to " + twoBlocksLiveBlock.data.value + " ";
                                BNLTwoBlocks.BlockCheckedAllocatedTo = twoBlocksLiveBlock.data.value.data.userName;
                            }
                        }

                        if (!blockLog.AllocatedBlocks.Contains(twoBlocksLiveBlock.data.userName))
                            blockLog.AllocatedBlocks.Add(twoBlocksLiveBlock.data.userName);
                    }
                }
            }
            if (handlingNewTrain)
            {
                //if future blocks assigned to non-manual block value then this is an auto train
                bool isAutomated = true;
                if (BNLNextBlock.BlockCheckedAllocatedTo != null)
                {
                    if (BNLNextBlock.BlockCheckedAllocatedTo.Contains("Manual"))
                    {
                        isAutomated = false;
                    }
                    else
                    {
                        if (BNLTwoBlocks.BlockCheckedAllocatedTo == null)
                        {
                            isAutomated = false;
                        }
                        else
                        {
                            if (BNLTwoBlocks.BlockCheckedAllocatedTo.Contains("Manual"))
                            {
                                isAutomated = false;
                            }
                        }
                    }
                }
                else
                {
                    isAutomated = false;
                }
                if (isAutomated)
                {
                    blockLog.Name = "Automated train " + TrainCounter.ToString();
                    blockLog.Name = BNLNextBlock.BlockCheckedAllocatedTo + " (A)";
                    blockLog.IsAutomated = true;
                }
                else
                {
                    blockLog.IsAutomated = false;
                }

                ddlTrainSelector.Items.Add(blockLog.Name);
                TrainCounter++;
            }

            if (issueFoundThisBlock)
            {
                var alertExists = alerts.Any(a => a.BlockSystemName == block.data.name && a.Severity == AlertSeverity.Extreme);
                if (!alertExists)
                {
                    AddAlert(new Alert()
                    {
                        id = Guid.NewGuid(),
                        BlockSystemName = block.data.name,
                        BlockUserName = block.data.userName,
                        SignalMastSystemName = "",
                        SignalMastUserName = "",
                        Severity = AlertSeverity.Extreme,
                        PreviousBlockUserName = previousBlock.data.userName,
                        NextBlockUserName = likelyNextBlock,
                        LikelyIssue = likelyIssueThisBlock,
                        BNL = BNLThisBlock,
                        Deactivated = false,
                        TrainName = blockLog.Name,
                        TrainId = blockLog.DCCiD
                    });
                }
            }

            if (issueFoundNextBlock)
            {
                //Danger alert
                var alertExists = alerts.Any(a => a.BlockSystemName == block.data.name && a.Severity == AlertSeverity.Danger);
                if (!alertExists)
                {
                    AddAlert(new Alert()
                    {
                        id = Guid.NewGuid(),
                        BlockSystemName = block.data.name,
                        BlockUserName = block.data.userName,
                        SignalMastSystemName = "",
                        SignalMastUserName = "",
                        Severity = AlertSeverity.Danger,
                        PreviousBlockUserName = previousBlock != null && previousBlock.data != null ? previousBlock.data.userName :"",
                        NextBlockUserName = likelyNextBlock,
                        LikelyIssue = likelyIssueNextBlock,
                        BNL = BNLNextBlock,
                        Deactivated = false,
                        TrainName = blockLog.Name,
                        TrainId = blockLog.DCCiD
                    });
                }
            }
            if (issueFoundTwoBlocks)
            {
                //Caution alert
                var alertExists = alerts.Any(a => a.BlockSystemName == block.data.name && a.Severity == AlertSeverity.Caution);
                var dangerAlertExistsForNextBlock = alerts.Any(a => a.BNL.BlockChecked == BNLNextBlock.BlockChecked && a.Severity == AlertSeverity.Danger);

                if (!alertExists && !dangerAlertExistsForNextBlock)
                {
                    AddAlert(new Alert()
                    {
                        id = Guid.NewGuid(),
                        BlockSystemName = block.data.name,
                        BlockUserName = block.data.userName,
                        SignalMastSystemName = "",
                        SignalMastUserName = "",
                        Severity = AlertSeverity.Caution,
                        PreviousBlockUserName = previousBlock != null && previousBlock.data != null ? previousBlock.data.userName : "",
                        NextBlockUserName = likelyNextBlock,
                        LikelyIssue = likelyIssueTwoBlocks,
                        BNL = BNLTwoBlocks,
                        Deactivated = false,
                        TrainName = blockLog.Name,
                        TrainId = blockLog.DCCiD
                    });
                }
            }

            if (noMoreBlocks)
            {
                blockLog.Terminated = true;
                blockLog.TerminatedReason = "No more blocks";
                blockLog.LastUpdated = DateTime.Now;
                //TerminateTrain(blockLog.Name);
                return (false, "No more blocks");
            }

            if (!issueFoundThisBlock && !issueFoundNextBlock && !issueFoundTwoBlocks)
            {
                //lbOutput.Items.Add(("Proceed " + blockUserName));
                // lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                if (ShowProceedMessages)
                {
                    ListViewItem item = new ListViewItem();
                    if (BNLTwoBlocks != null)
                    {
                        item.Text = "Proceed " + block.data.userName + " to " + likelyNextBlock + " to " + BNLTwoBlocks.BlockChecked;
                    }
                    else
                    {
                        item.Text = "Proceed " + block.data.userName + " to " + likelyNextBlock + " then possible end of blocks";
                    }
                    item.BackColor = Color.LimeGreen;

                        lvUpdates.Items.Add(item);
                        lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                        lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                    

                }

                var topic = CabSignalTopic + blockLog.DCCiD;

                    //lbOutput.Items.Add("MQtt " + topic + " - Proceed");

                
                await MQTTClient.SendMQTTMessage(MQTTServer, topic, "Proceed", false);

                var alertsForThisBlock = alerts.Where(w => w.BlockSystemName == block.data.name && !w.Deactivated);
                foreach (var alert in alertsForThisBlock)
                {
                    //lbOutput.Items.Add("1162 deactivate alert");
                    DeactivateAlert(alert,true);
                }
            }

            if (!issueFoundNextBlock && !blockLog.IsAutomated && AllocateBlocks)
            {
                await webClient.AllocateBlock(BNLNextBlock.BlockCheckedSystemName, blockLog.Name);
                await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + BNLNextBlock.BlockCheckedSystemName, BNLNextBlock.BlockChecked, false);
            }
            if (!issueFoundTwoBlocks && !blockLog.IsAutomated && AllocateBlocks)
            {
                await webClient.AllocateBlock(BNLTwoBlocks.BlockCheckedSystemName, blockLog.Name);
                await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + BNLTwoBlocks.BlockCheckedSystemName, BNLTwoBlocks.BlockChecked, false);
            }

            blockLog.LastUpdated = DateTime.Now;
            blockLog.ProcessingNewBlock = false;
            Log.Remove(existingLog);
            Log.Add(blockLog);
            return (true, "Success");
        }

        private async Task<BlockNavigationLog> NavigateThroughBlockItems(string currentBlock, string previousBlock, string LayoutItem, string previousLayoutItem, string breadcrumbStart)
        {
            if (string.IsNullOrEmpty(LayoutItem) || string.IsNullOrEmpty(currentBlock) || string.IsNullOrEmpty(previousLayoutItem))
                return null;

            var bnl = new BlockNavigationLog();
            bnl.BlockChecked = currentBlock;
            bnl.StartItem = LayoutItem;
            bnl.StartPreviousItem = previousLayoutItem;
            bnl.PreviousBlock = previousBlock;
            if (breadcrumbStart != "") bnl.Breadcrumb += breadcrumbStart+";";
            if (LayoutItem.Substring(0, 2) == "TO")
            {
                //turnout
                var to = config.GetLayuoutTurnout(LayoutItem);
                var configTurnout = config.GetTurnoutByUserName(to.Turnoutname);
                var liveTurnout = await webClient.GetTurnout(configTurnout.systemName);
                var derivedXoverBlockName = "";
                if (to.Type.Contains("XOVER"))
                {
                    if (to.Connectaname == previousLayoutItem)
                    {
                        derivedXoverBlockName = to.Blockname;
                    }
                    else if (to.Connectbname == previousLayoutItem)
                    {
                        derivedXoverBlockName = to.Blockname;
                    }
                    else if (to.Connectcname == previousLayoutItem)
                    {
                        derivedXoverBlockName = to.Blockcname;
                    }
                    else if (to.Connectdname == previousLayoutItem)
                    {
                        derivedXoverBlockName = to.Blockdname;
                    }

                }
                else derivedXoverBlockName = to.Blockname;

                if (derivedXoverBlockName != currentBlock && derivedXoverBlockName != previousBlock && to.Connectaname != breadcrumbStart && to.Connectbname != breadcrumbStart && to.Connectcname != breadcrumbStart && to.Connectdname != breadcrumbStart)
                {
                    bnl.EdgeConnector = to.Ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    bnl.BlockFound = to.Blockname;
                    if (to.Connectbname == previousLayoutItem || to.Connectcname == previousLayoutItem)
                    {
                        bnl.NextBlockEdgeConnector = to.Connectaname;
                    }
                    else if (liveTurnout.data.state == 2)
                    {
                        bnl.NextBlockEdgeConnector = to.Connectbname;
                    }
                    else
                    {
                        bnl.NextBlockEdgeConnector = to.Connectcname;
                    }
                }
                else
                {
                    //turnout still in same block, keep going
                    string nextItemIdent = "";
                    if (to.Type.Contains("XOVER"))
                    {
                        if (to.Connectaname == previousLayoutItem)
                        {
                            if (liveTurnout.data.state == 2)
                            {
                                //closed
                                nextItemIdent = to.Connectbname;
                            }
                            else
                            {
                                if (to.Type.StartsWith("LH"))
                                {
                                    //approaching A on a thrown LH XOver - short imminent
                                    bnl.LikelyIssue = liveTurnout.data.userName + " THROWN AGAINST";
                                    nextItemIdent = to.Connectbname;
                                }
                                else
                                {
                                    nextItemIdent = to.Connectcname;
                                }                                
                            }
                        }
                        else if (to.Connectbname == previousLayoutItem)
                        {
                            if (liveTurnout.data.state == 2)
                            {
                                nextItemIdent = to.Connectaname;
                            }
                            else
                            {
                                if (to.Type.StartsWith("RH"))
                                {
                                    //approaching B on a RH Xover when it's open - short imminent - assign a SM that should be red
                                    bnl.LikelyIssue = liveTurnout.data.userName + " THROWN AGAINST";
                                    nextItemIdent = to.Connectaname;
                                }
                                else
                                {
                                    nextItemIdent = to.Connectdname;
                                }                                
                            }
                        }
                        else if (to.Connectcname == previousLayoutItem)
                        {
                            if (liveTurnout.data.state == 2)
                            {
                                //closed
                                nextItemIdent = to.Connectdname;
                            }
                            else
                            {
                                if (to.Type.StartsWith("LH"))
                                {
                                    //approaching C on a LH Xover when it's thrown - short imminent
                                    bnl.LikelyIssue = liveTurnout.data.userName + " THROWN AGAINST";
                                    nextItemIdent = to.Connectdname;
                                }
                                else
                                {
                                    nextItemIdent = to.Connectaname;
                                }
                            }
                        }
                        else if (to.Connectdname == previousLayoutItem)
                        {
                            if (liveTurnout.data.state == 2)
                            {
                                //closed
                                nextItemIdent = to.Connectcname;
                            }
                            else
                            {
                                if (to.Type.StartsWith("RH"))
                                {
                                    //approaching D on a RH Xover when it's thrown - short imminent
                                    bnl.LikelyIssue = liveTurnout.data.userName + " THROWN AGAINST";
                                    nextItemIdent = to.Connectcname;
                                }
                                else
                                {
                                    nextItemIdent = to.Connectbname;
                                }
                                
                            }
                        }
                        bnl.Breadcrumb += nextItemIdent + ";";
                        var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItemIdent, to.Ident, "");
                        bnl.Breadcrumb += newbnl.Breadcrumb;
                        bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                        if (newbnl.BlockFound == null)
                        {
                            bnl.NoMoreBlocksFound = true;
                        }
                        else
                        {
                            bnl.BlockFound = newbnl.BlockFound;
                        }
                        bnl.EdgeConnector = newbnl.EdgeConnector;
                        bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                        bnl.NextBlockEdgeConnector = newbnl.NextBlockEdgeConnector;
                        if (!String.IsNullOrEmpty(newbnl.LikelyIssue))
                        {
                            if (!String.IsNullOrEmpty(bnl.LikelyIssue))
                            {
                                bnl.LikelyIssue += "; " + newbnl.LikelyIssue;
                            }
                            else
                            {
                                bnl.LikelyIssue = newbnl.LikelyIssue;
                            }
                        }
                    }
                    else
                    {
                        //turnout
                        if (liveTurnout.data.state == 4)
                        {
                            //thrown                        
                            //need to determine direction of travel. If one of the C or B connectors matches the previousLayout Item, we're traversing head on.
                            if (to.Connectbname == previousLayoutItem || to.Connectcname == previousLayoutItem)
                            {
                                nextItemIdent = to.Connectaname;
                                if (to.Connectbname == previousLayoutItem)
                                {
                                    bnl.LikelyIssue = liveTurnout.data.userName + " THROWN AGAINST";
                                }
                            }
                            else
                            {
                                var thrownConnector = to.Connectcname;
                                if (thrownConnector != previousLayoutItem)
                                {
                                    nextItemIdent = thrownConnector;
                                }
                                else
                                {
                                    nextItemIdent = to.Connectaname;
                                }
                            }                         
                        }
                        else
                        {
                            if (to.Connectbname == previousLayoutItem || to.Connectcname == previousLayoutItem)
                            {
                                nextItemIdent = to.Connectaname;
                                if (to.Connectcname == previousLayoutItem)
                                {
                                    bnl.LikelyIssue = liveTurnout.data.userName + " CLOSED AGAINST";
                                }
                            }
                            else
                            {
                                var closedConnector = to.Connectbname;
                                if (closedConnector != previousLayoutItem)
                                {
                                    nextItemIdent = closedConnector;
                                }
                                else
                                {
                                    nextItemIdent = to.Connectaname;
                                }
                            }
                        }
                        bnl.Breadcrumb += nextItemIdent + ";";
                        var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItemIdent, to.Ident, "");
                        bnl.Breadcrumb += newbnl.Breadcrumb;
                        bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                        if (newbnl.BlockFound == null)
                        {
                            bnl.NoMoreBlocksFound = true;
                        }
                        else
                        {
                            bnl.BlockFound = newbnl.BlockFound;
                        }
                        bnl.EdgeConnector = newbnl.EdgeConnector;
                        bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                        bnl.NextBlockEdgeConnector = newbnl.NextBlockEdgeConnector;
                        if (!String.IsNullOrEmpty(newbnl.LikelyIssue))
                        {
                            if (!String.IsNullOrEmpty(bnl.LikelyIssue))
                            {
                                bnl.LikelyIssue += "; " + newbnl.LikelyIssue;
                            }
                            else
                            {
                                bnl.LikelyIssue = newbnl.LikelyIssue;
                            }
                        }
                    }
                }
            }
            else if (LayoutItem.Substring(0, 1) == "T")
            {
                //track
                var ts = config.GetLayoutTracksegment(LayoutItem);
                if (ts.Blockname != currentBlock)
                {
                    bnl.EdgeConnector = ts.Ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    bnl.BlockFound = ts.Blockname;
                    if (ts.Connect1name != previousLayoutItem)
                    {
                        bnl.NextBlockEdgeConnector = ts.Connect1name;
                    }
                    else
                    {
                        bnl.NextBlockEdgeConnector = ts.Connect2name;
                    }
                }
                else
                {
                    var nextItem = ts.Connect2name;
                    if (ts.Connect2name == previousLayoutItem) nextItem = ts.Connect1name;
                    bnl.Breadcrumb += nextItem + ";";
                    var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItem, ts.Ident,"");
                    bnl.Breadcrumb += newbnl.Breadcrumb;
                    bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                    if (newbnl.BlockFound == null)
                    {
                        bnl.NoMoreBlocksFound = true;
                    }
                    else
                    {
                        bnl.BlockFound = newbnl.BlockFound;
                    }
                    bnl.EdgeConnector = newbnl.EdgeConnector;
                    bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                    bnl.NextBlockEdgeConnector = newbnl.NextBlockEdgeConnector;
                    if (!String.IsNullOrEmpty(newbnl.LikelyIssue))
                    {
                        if (!String.IsNullOrEmpty(bnl.LikelyIssue))
                        {
                            bnl.LikelyIssue += "; " + newbnl.LikelyIssue;
                        }
                        else
                        {
                            bnl.LikelyIssue = newbnl.LikelyIssue;
                        }
                    }
                }
            }
            else if (LayoutItem.Substring(0, 1) == "A")
            {
                //anchor
                var a = config.GetTrackLayoutAnchorPoint(LayoutItem);
                var nextItem = a.Connect2name;
                if (a.Connect2name == previousLayoutItem) nextItem = a.Connect1name;
                bnl.Breadcrumb += nextItem + ";";
                var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItem, a.Ident,"");
                bnl.Breadcrumb += newbnl.Breadcrumb;
                bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                if (newbnl.BlockFound == null)
                {
                    bnl.NoMoreBlocksFound = true;
                }
                else
                {
                    bnl.BlockFound = newbnl.BlockFound;
                }
                bnl.EdgeConnector = newbnl.EdgeConnector;
                bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                bnl.NextBlockEdgeConnector = newbnl.NextBlockEdgeConnector;
                if (!String.IsNullOrEmpty(newbnl.LikelyIssue))
                {
                    if (!String.IsNullOrEmpty(bnl.LikelyIssue))
                    {
                        bnl.LikelyIssue += "; " + newbnl.LikelyIssue;
                    }
                    else
                    {
                        bnl.LikelyIssue = newbnl.LikelyIssue;
                    }
                }
            }
            else if(LayoutItem.Substring(0,2) == "SL")
            {
                //slip
                var slip = config.GetSlip(LayoutItem);
                var cfgTurnoutA = config.GetTurnoutByUserName(slip.Turnout);
                var cfgTurnoutB = config.GetTurnoutByUserName(slip.TurnoutB);
                var liveTA = await webClient.GetTurnout(cfgTurnoutA.systemName);
                var liveTB = await webClient.GetTurnout(cfgTurnoutB.systemName);
                var nextItem = "";
                var issueFound = "";
                string astate = liveTA.data.state.ToString();
                string bstate = liveTB.data.state.ToString();

                //Approaching from...
                if (slip.Connectaname == previousLayoutItem)
                {
                    if (slip.States.AC.Turnout == astate && slip.States.AC.TurnoutB == bstate)
                    {
                        nextItem = slip.Connectcname;
                    }
                    else if (slip.States.AD.Turnout == astate && slip.States.AD.TurnoutB == bstate)
                    {
                        nextItem = slip.Connectdname;
                    }
                    else
                    {                        
                        string state = " THROWN";
                        if (liveTA.data.state == 2) state = " CLOSED";
                        if (slip.States.AC.Turnout == astate)
                        {
                            nextItem = slip.Connectcname;
                        }
                        else
                        {
                            nextItem = slip.Connectdname;
                        }
                        issueFound = liveTA.data.userName + state + " AGAINST";
                    }
                }
                else if (slip.Connectbname == previousLayoutItem)
                {
                    if (slip.States.BC.Turnout == astate && slip.States.BC.TurnoutB == bstate)
                    {
                        nextItem = slip.Connectcname;
                    }
                    else if (slip.States.BD.Turnout == astate && slip.States.BD.TurnoutB == bstate)
                    {
                        nextItem = slip.Connectdname;
                    }
                    else
                    {
                        string state = " THROWN";
                        if (liveTA.data.state == 2) state = " CLOSED";
                        if (slip.States.BC.Turnout == astate)
                        {
                            nextItem = slip.Connectcname;
                        }
                        else
                        {
                            nextItem = slip.Connectdname;
                        }
                        issueFound = liveTA.data.userName + state + " AGAINST";
                    }
                }
                else if (slip.Connectcname == previousLayoutItem)
                {
                    if (slip.States.AC.Turnout == astate && slip.States.AC.TurnoutB == bstate)
                    {
                        nextItem = slip.Connectaname;
                    }
                    else if (slip.States.BC.Turnout == astate && slip.States.BC.TurnoutB == bstate)
                    {
                        nextItem = slip.Connectbname;
                    }
                    else
                    {
                        string state = " THROWN";
                        if (liveTB.data.state == 2) state = " CLOSED";
                        if (slip.States.AC.TurnoutB == bstate)
                        {
                            nextItem = slip.Connectaname;
                        }
                        else
                        {
                            nextItem = slip.Connectbname;
                        }
                        issueFound = liveTB.data.userName + state + " AGAINST";
                    }
                }
                else if (slip.Connectdname == previousLayoutItem)
                {
                    if (slip.States.AD.Turnout == astate && slip.States.AD.TurnoutB == bstate)
                    {
                        nextItem = slip.Connectaname;
                    }
                    else if (slip.States.BD.Turnout == astate && slip.States.BD.TurnoutB == bstate)
                    {
                        nextItem = slip.Connectbname;
                    }
                    else
                    {
                        string state = " THROWN";
                        if (liveTB.data.state == 2) state = " CLOSED";
                        if (slip.States.AD.TurnoutB == bstate)
                        {
                            nextItem = slip.Connectaname;
                        }
                        else
                        {
                            nextItem = slip.Connectbname;
                        }
                        issueFound = liveTB.data.userName + state + " AGAINST";
                    }
                }

                if(slip.Blockname != currentBlock)
                {
                    bnl.EdgeConnector = slip.Ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    bnl.BlockFound = slip.Blockname; 
                    bnl.NextBlockEdgeConnector = nextItem;
                }
                else
                {
                    bnl.LikelyIssue = issueFound;
                    bnl.Breadcrumb += nextItem + ";";
                    var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItem, slip.Ident, "");
                    bnl.Breadcrumb += newbnl.Breadcrumb;
                    bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                    if (newbnl.BlockFound == null)
                    {
                        bnl.NoMoreBlocksFound = true;
                    } 
                    else
                    {
                        bnl.BlockFound = newbnl.BlockFound;
                    }
                    bnl.EdgeConnector = newbnl.EdgeConnector;
                    bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                    bnl.NextBlockEdgeConnector = newbnl.NextBlockEdgeConnector;
                    if (!String.IsNullOrEmpty(newbnl.LikelyIssue))
                    {
                        if (!String.IsNullOrEmpty(bnl.LikelyIssue))
                        {
                            bnl.LikelyIssue += "; " + newbnl.LikelyIssue;
                        }
                        else
                        {
                            bnl.LikelyIssue = newbnl.LikelyIssue;
                        }
                    }
                }
            }
            return bnl;
        }

        private void btnAcknowledgeAlert_Click(object sender, EventArgs e)
        {
            if (lblBlockWarning.Text == "" || lblBlockContainingDanger.Text == "") return;
            var ackBlockName = lblBlockWarning.Text;
            var ackBlockChecked = lblBlockContainingDanger.Text.Substring(3);
            var ackAlerts = alerts.Where(f => f.BlockUserName == ackBlockName && f.BNL.BlockChecked == ackBlockChecked);

            var ackText = "";

            foreach (var ackAlert in ackAlerts)
            {
                ackText = "Acknowledged " + ackAlert.BlockUserName;
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                ackAlert.Acknowledged = true;
                //ackAlert.Deactivated = true;
                //ackAlert.DeactivatedTime = DateTime.Now;
                if (lblBlockWarning.Text == ackAlert.BlockUserName)
                {
                    lblBlockWarning.Text = "";
                    lblBlockContainingDanger.Text = "";
                    lblLikelyIssue.Text = "";
                    lblTrainName.Text = "";
                }
            }
            ListViewItem item = new ListViewItem();
            item.Text = ackText;
            item.BackColor = Color.LimeGreen;
            lvUpdates.Items.Add(item);
        }

        private async Task<bool> ProcessAlerts()
        {
            if (alerts.Count == 0)
            {
                lblBlockWarning.Text = "";
                lblLikelyIssue.Text = "";
                lblBlockContainingDanger.Text = "";
                lblTrainName.Text = "";
            }

            var currentVisibleAlert = alerts.FirstOrDefault(f => f.Visible == true);
            List<Alert> alertsToRemove = new List<Alert>();

            try
            {
                foreach (var alert in alerts.OrderBy(o => o.Severity).ToList())
                {
                    if (alert.Deactivated) continue;
                    if (alert.Acknowledged) continue;

                    bool hasPrecedingAlert = false;
                    if (string.IsNullOrEmpty(alert.BNL.BlockFound) && !alert.BNL.NoMoreBlocksFound && !alert.Deactivated)
                    {
                        if (!alert.Visible)
                        {
                            DisplayAlert(alert);
                            alert.Visible = true;
                            alert.AlertStart = DateTime.Now;
                            var colour = alert.Severity.ToString();
                            SoundPlayer signalBeep = new SoundPlayer("./Assets/" + colour + ".wav");
                            signalBeep.Play();
                            continue;
                        }
                        continue;
                    }
                    var checkAlert = new BlockNavigationLog();

                    var log = Log.FirstOrDefault(f => f.Name == alert.TrainName);
                    bool alertWasFromADifferentPath = false;
                    if (log != null)
                    {
                        
                        if (log.Terminated)
                        {
                            lbOutput.Items.Add("2345 deactivate alert");
                            DeactivateAlert(alert, false);
                        }
                        else
                        {
                            if (log.CurrentBlockBNL.BlockChecked == alert.BNL.BlockChecked)
                                checkAlert = log.CurrentBlockBNL;
                            else if (log.NextBlockBNL.BlockChecked == alert.BNL.BlockChecked)
                                checkAlert = log.NextBlockBNL;
                            else if (log.TwoBlocksBNL.BlockChecked == alert.BNL.BlockChecked)
                                checkAlert = log.TwoBlocksBNL;
                            else if (log.CurrentBlockBNL.LikelyIssue == null && log.NextBlockBNL.LikelyIssue == null && log.NextBlockBNL.LikelyIssue == null)
                            {
                                alertWasFromADifferentPath = true;
                                checkAlert = log.CurrentBlockBNL;
                            }                                
                        }
                    }
                    if (string.IsNullOrEmpty(checkAlert.BlockChecked))
                    {
                        alert.Deactivated = true;
                        alert.DeactivatedTime = DateTime.Now;
                        lbOutput.Items.Add("Alert deactivated - no issue or BNL found - 1982 - "+alert.LikelyIssue);
                    }

                    if (!string.IsNullOrEmpty(checkAlert.LikelyIssue))
                    {
                        if (!alert.LikelyIssue.Contains("Collision"))
                        {
                            alert.LikelyIssue = checkAlert.LikelyIssue;
                        }
                        else
                        {
                            if (!alert.LikelyIssue.Contains(checkAlert.LikelyIssue))
                                alert.LikelyIssue += checkAlert.LikelyIssue;
                        }
                    }
                    if (!alertWasFromADifferentPath)
                    { }
                        alert.BNL = checkAlert;
                    
                    var checkAlertLiveBlock = await webClient.GetBlock(alert.BNL.BlockChecked);
                    var alertStillActive = false;
                    if (!string.IsNullOrEmpty(checkAlert.LikelyIssue)) alertStillActive = true;
                    if (checkAlertLiveBlock.data.state == 2) alertStillActive = true;
                    if (TrackAllocation && checkAlertLiveBlock.data.value != null && log != null && checkAlertLiveBlock.data.value.data.userName != log.Name) alertStillActive = true;
                    if (log.CurrentBlockBNL.LikelyIssue == null && log.NextBlockBNL.LikelyIssue == null && log.NextBlockBNL.LikelyIssue == null)
                    {
                        alertStillActive = false;
                    }

                    if (!alertStillActive)
                    {
                        lbOutput.Items.Add("2282 deactivate alert");
                        DeactivateAlert(alert,true); 
                        continue;
                    }

                    if (alert.Severity == AlertSeverity.Danger)
                    {
                        var existingCuationAlert = alerts.FirstOrDefault(a => a.Severity == AlertSeverity.Caution && a.BlockUserName == alert.PreviousBlockUserName);
                        var existingCautionsForAffectedBlock = alerts.Where(w => w.Severity == AlertSeverity.Caution && w.BNL.BlockChecked == alert.BNL.BlockChecked);
                        foreach (var existingCaution in existingCautionsForAffectedBlock)
                        {
                            existingCaution.Acknowledged = false;
                            existingCaution.Superceded = true;
                            existingCaution.Deactivated = true;
                            existingCaution.DeactivatedTime = DateTime.Now;
                        }
                    }
                    else if (alert.Severity == AlertSeverity.Caution)
                    {
                        var blockAffected = alert.BNL.BlockChecked;
                        hasPrecedingAlert = alerts.Any(a => a.BNL.BlockFound == blockAffected && a.Severity == AlertSeverity.Danger);
                    }

                    TimeSpan timeDiff = DateTime.Now - alert.AlertStart;
                    bool showAlert = false;

                    if (alertStillActive && !hasPrecedingAlert && !alert.Deactivated)
                    {
                        if (alert.Severity == AlertSeverity.Caution)
                        {
                            if (timeDiff.TotalSeconds >= CautionNagFrequencySeconds) showAlert = true;
                        }
                        else if (alert.Severity == AlertSeverity.Danger)
                        {
                            if (timeDiff.TotalSeconds > DangerNagFrequencySeconds) showAlert = true;
                        }
                    }

                    if (showAlert)
                    {
                        DisplayAlert(alert);
                        alert.Visible = true;
                        alert.AlertStart = DateTime.Now;
                        var colour = alert.Severity.ToString();
                        SoundPlayer signalBeep = new SoundPlayer("./Assets/" + colour + ".wav");
                        signalBeep.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                lbOutput.Items.Add("Alert processing exception "+ex.Message);
            }

            try
            {
                var alertsEmptied = false;
                var deactivatedAlerts = alerts.Where(w => w.Deactivated).ToList();
                foreach (var da in deactivatedAlerts)
                {
                    TimeSpan diff = DateTime.Now - da.DeactivatedTime;
                    if (diff.TotalSeconds > 30)
                    {
                        alerts.Remove(da);
                        alertsEmptied = true;
                    }
                }

                int numberOfActiveAlerts = alerts.Where(w => !w.Deactivated).Count();
                if (alertsEmptied && numberOfActiveAlerts == 0)
                {
                    var latestAlert = lvUpdates.Items[lvUpdates.Items.Count - 1];
                    lvUpdates.Items.Clear();
                    lvUpdates.Items.Add(latestAlert);
                    lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
            catch (Exception ex)
            {
                lbOutput.Items.Add("Expired alert processing excpeption " + ex.Message);
            }

            return true;
        }

        private async void AddAlert(Alert alert)
        {
            alerts.Add(alert);
            var topic = CabSignalTopic + alert.TrainId;
            //lbOutput.Items.Add("MQtt " + topic + " - " + alert.Severity.ToString());
            await MQTTClient.SendMQTTMessage(MQTTServer, topic, alert.Severity.ToString(), false);
        }

        private async void DeactivateAlert(Alert alert, bool displayListViewItem)
        {
            if (displayListViewItem)
            {
                ListViewItem item = new ListViewItem();
                item.Text = "Proceed " + alert.TrainName + " - " + alert.BlockUserName + " cleared";
                item.BackColor = Color.LimeGreen;
                lvUpdates.Items.Add(item);
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }

            alert.Deactivated = true;
            alert.DeactivatedTime = DateTime.Now;
            if (lblBlockWarning.Text == alert.BlockUserName)
            {
                lblBlockWarning.Text = "";
                lblBlockContainingDanger.Text = "";
                lblLikelyIssue.Text = "";
                lblTrainName.Text = "";
            }
            var topic = CabSignalTopic + alert.TrainId;
           // lbOutput.Items.Add("MQTT " + topic + " - Proceed");
            await MQTTClient.SendMQTTMessage(MQTTServer,topic , "Proceed", false);
        }

        private void DisplayAlert(Alert alert)
        {
            lblBlockWarning.Text = alert.BlockUserName;
            lblTrainName.Text = alert.TrainName +" ("+alert.TrainId+")";
            lblBlockWarning.ForeColor = Color.White;
            lblLikelyIssue.Text = alert.LikelyIssue;
            lblLikelyIssue.ForeColor = Color.White;
            lblTrainName.ForeColor = Color.White;
            lblBlockContainingDanger.ForeColor = Color.White;
            lblBlockContainingDanger.Text = "In "+ alert.BNL.BlockChecked;
            if (alert.Severity == AlertSeverity.Danger)
            {
                lblBlockWarning.BackColor = Color.Red;
                lblLikelyIssue.BackColor = Color.Red;
                lblBlockContainingDanger.BackColor = Color.Red;
                lblTrainName.BackColor = Color.Red;
                //lbOutput.Items.Add(alert.BlockUserName+" DANGER "+ alerts.Count.ToString());
                //lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                ListViewItem item = new ListViewItem();
                item.Name = alert.id.ToString();
                item.Text = alert.TrainName+" - "+ alert.BlockUserName+" - "+alert.LikelyIssue+"- in "+alert.BNL.BlockChecked;
                item.BackColor = Color.Red;
                lvUpdates.Items.Add(item);
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
            }
            else if (alert.Severity == AlertSeverity.Caution)
            {
                lblBlockWarning.BackColor = Color.Orange;
                lblLikelyIssue.BackColor = Color.Orange;
                lblBlockContainingDanger.BackColor = Color.Orange;
                lblTrainName.BackColor = Color.Orange;
                //lbOutput.Items.Add(alert.BlockUserName + " CAUTION " + alerts.Count.ToString());
                //lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                ListViewItem item = new ListViewItem();
                item.Name = alert.id.ToString();
                item.Text = alert.TrainName + " - " + alert.BlockUserName + " - " + alert.LikelyIssue + "- in " + alert.BNL.BlockChecked;
                item.BackColor = Color.Orange;
                lvUpdates.Items.Add(item);
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
            }

            lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void ProcessDeoccupiedBlocks()
        {
            try
            {
                List<DeOccupiedBlock> toRemove = new List<DeOccupiedBlock>();
                foreach (var dob in DeoccupiedBlocks)
                {
                    TimeSpan diff = DateTime.Now - dob.DeactivatedTime;
                    if (diff.TotalSeconds > 2) toRemove.Add(dob);
                }
                foreach(var dob2r in toRemove)
                {
                    DeoccupiedBlocks.Remove(dob2r);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void lvUpdates_SelectedIndexChanged(object sender, EventArgs e)
        {
            return;
            ListViewItem item = (ListViewItem)sender;
            var alertId = item.Name;
            var alert = alerts.FirstOrDefault(f => f.id.ToString() == alertId);
            if (alert != null)
            {
                item.Text = "Acknowledged " + alert.BlockUserName;
                item.BackColor = Color.LimeGreen;
                lvUpdates.Items.Add(item);
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                alerts.Remove(alert);
            }
        }

        private void btnTerminateTrain_Click(object sender, EventArgs e)
        {
            var trainName = (string)ddlTrainSelector.SelectedItem;
            TerminateTrain(trainName);
        }

        private async void TerminateTrain(string trainName)
        {
            var log = Log.FirstOrDefault(f => f.Name == trainName);
            if (log == null) return;
            var alertsToDeactivate = alerts.Where(w => w.TrainName == trainName);
            foreach (var atd in alertsToDeactivate)
            {
                atd.Deactivated = true;
                atd.DeactivatedTime = DateTime.Now;
            }
            if (Log.Count > 0 && AllocateBlocks)
            {
                foreach (var blockToUnallocate in log.AllocatedBlocks)
                {
                    await webClient.AllocateBlock(blockToUnallocate, "");
                    var sysName = config.GetBlockByUserName(blockToUnallocate);
                    await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                }
            }
            Log.Remove(log);

            var topic = CabSignalTopic + log.DCCiD;
            //lbOutput.Items.Add("MQtt " + topic + " - Inactive");
            await MQTTClient.SendMQTTMessage(MQTTServer, topic, "Inactive", false);

            ddlTrainSelector.Items.Clear();
            foreach (var remainingLog in Log)
            {
                ddlTrainSelector.Items.Add(remainingLog.Name);
            }
        }

        private async void btnCancelAllocations_Click(object sender, EventArgs e)
        {
            if (!AllocateBlocks) return;
            if (webClient == null || config == null)
            {
                config = new ConfigReader(tbConfigLocation.Text);
                webClient = new JSONReader("http://" + tbServerIP.Text + ":" + tbServerPort.Text);
            }

            var blocks = await webClient.GetBlocks();
            var allocatedBlocks = blocks.Where(w => w.data.value != null);
            foreach (var alloc in allocatedBlocks)
            {
                if (alloc.data.value != null)
                {
                    await webClient.AllocateBlock(alloc.data.userName, "");
                    var sysName = config.GetBlockByUserName(alloc.data.userName);
                    await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                }
            }
        }

        private void btnRosterTest_Click(object sender, EventArgs e)
        {
            var roster = new RosterReader(RosterPath);
            var r = roster.GetRoster();

            var test = AlertSeverity.Caution.ToString();
        }

        private void btnClearOutputLog_Click(object sender, EventArgs e)
        {
            lbOutput.Items.Clear();
        }

        private void btnClearJourneyListBox_Click(object sender, EventArgs e)
        {
            lbJourneyLog.Items.Clear();
        }

        private void ddlTrainSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbJourneyLog.Items.Clear();
            var log = Log.FirstOrDefault(f => f.Name == ddlTrainSelector.Text);
            if (log != null)
            {
                foreach (var h in log.History)
                {
                    lbJourneyLog.Items.Add(h);
                }
            }
        }
    }
}
