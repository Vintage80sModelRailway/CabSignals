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
using WiThrottleClient;
using WiThrottleClient.Classes;
using static JMRIReader.Classes.Enums;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using RosterEntry = JMRIReader.Classes.DTO.RosterEntry;

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
        private string SensorHoldTopic;
        private string RosterPath;
        private string CabSignalTopic;
        private List<string> AllocatedBlocks;
        private List<LiveJourneyLog> Log;
        private int TrainCounter;
        private bool AllocateBlocks;
        private List<string> ActiveAutomatedTrains;
        // Create a MQTT client factory
        private MqttFactory factory = new MqttFactory();
        private List<RosterEntry> Roster;
        //private List<string> NoValueBlocks = new List<string>();
        private string[] shortBlocks = { "UD Station Approach DS" };
        private string memoryAllocatedAutoTrainsName;
        private string memoryAllocatedManualTrainsName;
        private WiThrottle wt;
        private int _WiThrottlePort;
        private int DefaultCautionMMS;
        private int DefaultCrawlMMS;
        private int DefaultFullSpeedMMS;
        private Queue<MQTTMessage> MQTTMessages = new Queue<MQTTMessage>();
        private DateTime LastMQTTMessageProcessed = DateTime.MinValue;
        private bool usingMQTT = false;

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

            var cfgWiThrottleServerPort = ConfigurationManager.AppSettings["WiTHrottlePort"];
            if (cfgWiThrottleServerPort != null)
            {
                var sPort = cfgWiThrottleServerPort.ToString();
                _WiThrottlePort = int.Parse(sPort);
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

            var cfgMemName = ConfigurationManager.AppSettings["MemoryAllocatedAutoTrainsName"];
            if (cfgMemName != null)
            {
                memoryAllocatedAutoTrainsName = cfgMemName.ToString();
            }

            var cfgManualMemName = ConfigurationManager.AppSettings["MemoryAllocatedManualTrainsName"];
            if (cfgManualMemName != null)
            {
                memoryAllocatedManualTrainsName = cfgManualMemName.ToString();
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
                usingMQTT = true;
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

            var cfgSensorHoldTopic = ConfigurationManager.AppSettings["SensorHoldTopic"];
            if (cfgSensorHoldTopic != null)
            {
                SensorHoldTopic = cfgSensorHoldTopic;
            }

            var cfgCabSignalTOpic = ConfigurationManager.AppSettings["CabSignalTopic"];
            if (cfgCabSignalTOpic != null)
            {
                CabSignalTopic = cfgCabSignalTOpic.ToString();
            }

            var cfgdefaultCrawlMMS = ConfigurationManager.AppSettings["DefaultCrawlMMS"];
            if (cfgdefaultCrawlMMS != null)
            {
                var dcmms = cfgdefaultCrawlMMS.ToString();
                DefaultCrawlMMS = int.Parse(dcmms);
            }

            var cfgDefaultCautionMMS = ConfigurationManager.AppSettings["DefaultCautionMMS"];
            if (cfgDefaultCautionMMS != null)
            {
                var dcams = cfgDefaultCautionMMS.ToString();
                DefaultCautionMMS = int.Parse(dcams);
            }

            var cfgFullSpeedMMS = ConfigurationManager.AppSettings["DefaultFullSpeedMMS"];
            if (cfgFullSpeedMMS != null)
            {
                var dfs = cfgFullSpeedMMS.ToString();
                DefaultFullSpeedMMS = int.Parse(dfs);
            }
        }

        private async void btnStartMonitoring_Click(object sender, EventArgs e)
        {
            monitorRuning = true;
            DeoccupiedBlocks = new List<DeOccupiedBlock>();
            Log = new List<LiveJourneyLog>();
            TrainCounter = 1;
            config = new ConfigReader(tbConfigLocation.Text);
            webClient = new JSONReader("http://" + tbServerIP.Text + ":" + tbServerPort.Text);
            activeBlocks = await webClient.GetOccupiedBlocks();
            allBlocks = await webClient.GetBlocks();
            await webClient.UpdateMemory(memoryAllocatedManualTrainsName, "");

            alerts = new List<Alert>();
            //lbOutput.Items.Add("Monitoring started");
            ListViewItem item = new ListViewItem();
            item.Text = "Monitoring started";
            item.BackColor = Color.LimeGreen;
            lvUpdates.Items.Add(item);
            lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
            lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            var rosterCfG = new RosterReader(RosterPath);
            Roster = rosterCfG.LocoList;

            wt = new WiThrottle(tbServerIP.Text, _WiThrottlePort, "Monitor");

            while (monitorRuning)
            {
                await MonitorLayout();
                await ProcessAlerts();
                ProcessDeoccupiedBlocks();
                await wt.CheckForMessages();
                await Task.Delay(100);
                if (usingMQTT)
                    ProcessMQTTMessageQueue();
            }
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
                // lbOutput.Items.Add("Multi block");
                List<int> p1Logs = new List<int>();
                foreach (var nnab in newActiveThisTimeBlocks)
                {
                    if (nnab.data.value != null && nnab.data.value.data.comment == "Automated")
                        continue;
                    nnab.MultiBlockLogIndex = -1;
                    var matchingLog = Log.FirstOrDefault(f => f.NextBlock == nnab.data.userName);
                    if (matchingLog != null)
                    {
                        //lbOutput.Items.Add("Added p1 "+nnab.data.userName);
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
                        foreach (var el in Log)
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
                                                //lbOutput.Items.Add("matched p2 " + nnab.data.userName + " via path block matching to log " + el.Name);
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
                            //lbOutput.Items.Add("Added p2 with no index " + nnab.data.userName);
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

            foreach (var nab in newBlocksToProcess.OrderBy(o => o.MultiBlockPriority).ToList())
            {
                try
                {
                if (nab.data.value != null && nab.data.value.data.comment == "Automated")
                {
                    continue;
                }
                var previousBlockState = allBlocks.FirstOrDefault(f => f.data.name == nab.data.name);

                var alreadyExists = oldActiveBlocks.Any(a => a.data.name == nab.data.name);
                var justDeactivated = DeoccupiedBlocks.Any(a => a.BlockName == nab.data.name);
                if (!alreadyExists && !justDeactivated)
                {
                    //if (nab.data == null) continue;
                    var (success, reason) = await ProcessNewActiveBlock(nab, activeBlocks);
                    foundNewActiveBlock = true;
                    if (!success)
                    {

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
                    var newLogs = new List<LiveJourneyLog>();
                    for (int i = 0; i < Log.Count; i++)
                    {
                        var log = Log.ElementAt(i);

                        //If a train has stopped, remove its log. A new one will be created when it restarts
                        //Stops erroneous alerts when next block of a stopped train becomes active
                        var secondsSinceLastUpdate = DateTime.Now - log.LastUpdated;
                        if (secondsSinceLastUpdate.TotalSeconds > 240 && !log.IsAutomated && log.SignalAspect != SignalAspect.Danger)
                        {
                            log.TerminatedReason = "Dormant for 240 seconds";
                            log.Terminated = true;

                        }

                        //Try to get train length from roster config if it's currently 0
                        if (log.TrainLengthMM <= 0)
                        {
                            //var rosterCfG = new RosterReader(RosterPath);
                            //var roster = rosterCfG.LocoList;
                            var fullInfo = log.fullRosterInfo;
                            //var fullInfo = rosterCfG.FullRoster.FirstOrDefault(f => f.DccAddress == log.DCCiD);
                            if (fullInfo != null)
                            {
                                var trainLength = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerTrainLengthMM");
                                int trainLengthMM = 0;

                                if (trainLength != null)
                                {
                                    int.TryParse(trainLength.Value, out trainLengthMM);
                                }
                                log.TrainLengthMM = trainLengthMM;
                            }
                        }

                        //see if speed has changed
                        var throttle = wt.GetThrottleInfoByDCCID(log.DCCiD);
                        if (throttle == null)
                        {
                            var rosterEntry = wt.Roster.FirstOrDefault(f => f.ID == log.DCCiD);
                            var indexOfRE = wt.Roster.IndexOf(rosterEntry);
                            var mtIndex = wt.GetThrottle(indexOfRE);
                            throttle = wt.GetThrottleInfoByDCCID(log.DCCiD);
                        }
                        if (throttle != null)
                        {
                            if (log.CurrentSpeedStep != throttle.Speed)
                            {
                                var ssl = new SpeedStepLog();
                                ssl.start = DateTime.Now;
                                ssl.SpeedStep = throttle.Speed;

                                TrainDirection dir = TrainDirection.Forward;
                                if (throttle.Direction != "1") dir = TrainDirection.Reverse;

                                if (log.fullRosterInfo != null)
                                {
                                    var mms = GetMMSFromSpeedStep(log.fullRosterInfo, throttle.Speed, log.DCCiD, dir);
                                    ssl.SpeedMMS = mms;
                                }
                                var currentBlock = log.AutomatedBlockList.LastOrDefault();
                                if (currentBlock != null)
                                {
                                    currentBlock.SpeedLog.Add(ssl);
                                    //lbOutput.Items.Add("Speed change detected for " + log.DCCiD+" prev "+log.CurrentSpeedStep.ToString()+" now "+throttle.Speed.ToString()+" dir "+dir.ToString());
                                }
                                log.CurrentSpeedStep = throttle.Speed;
                            }
                        }

                        if (log.TrainLengthMM > 0 && log.HasSpeedProfile)
                        {
                            var previousBlocksStillOccupied = log.AutomatedBlockList.Where(w => w.SequenceState == JourneySequenceState.EnteredNextBlock);

                            foreach (var pbso in previousBlocksStillOccupied)
                            {
                                var indexOfpbso = log.AutomatedBlockList.IndexOf(pbso);
                                var debugDiff = log.AutomatedBlockList.Count - 1 - indexOfpbso;
                                bool debugOutput = false;
                                if (debugDiff > 1)
                                {
                                    debugOutput = true;
                                }
                                decimal totalMMCoveredSinceExitingPBSO = 0M;
                                if (indexOfpbso != -1)
                                {
                                    var speedLogList = new List<SpeedStepLog>();
                                    for (int p = indexOfpbso + 1; p < log.AutomatedBlockList.Count; p++)
                                    {
                                        var thisLogBlock = log.AutomatedBlockList.ElementAtOrDefault(p);
                                        if (thisLogBlock != null)
                                        {
                                            speedLogList.AddRange(thisLogBlock.SpeedLog);
                                        }
                                    }

                                    for (int b = 0; b < speedLogList.Count; b++)
                                    {
                                        var dateTimeTo = DateTime.Now;
                                        if (b + 1 < speedLogList.Count)
                                        {
                                            dateTimeTo = speedLogList.ElementAt(b + 1).start;
                                        }

                                        var timeDiff = dateTimeTo - speedLogList.ElementAt(b).start;

                                        totalMMCoveredSinceExitingPBSO += speedLogList.ElementAt(b).SpeedMMS * (decimal)timeDiff.TotalSeconds;
                                        if (debugOutput)
                                        {
                                            //lbOutput.Items.Add("b = " + b.ToString() + " MM " + totalMMCoveredSinceExitingPBSO.ToString()+" pbso "+pbso.BlockUserName);
                                        }
                                    }

                                    if (totalMMCoveredSinceExitingPBSO > log.TrainLengthMM)
                                    {
                                        var liveBlock = allBlocks.FirstOrDefault(f => f.data.name == pbso.BlockSystemname);
                                        if (liveBlock != null)
                                        {
                                            var sensorName = liveBlock.data.sensor.Substring(2);
                                            lbOutput.Items.Add(DateTime.Now.ToString() + " Loco " + log.DCCiD + " calculated exit of block " + pbso.BlockUserName + " senspr " + sensorName + " train length " + log.TrainLengthMM.ToString() + " distance calculated " + totalMMCoveredSinceExitingPBSO.ToString());
                                            pbso.SequenceState = JourneySequenceState.Traversed;
                                            if (usingMQTT)
                                            {
                                                MQTTMessages.Enqueue(new MQTTMessage()
                                                {
                                                    Topic = SensorHoldTopic + "/" + sensorName,
                                                    Payload = "0",
                                                    Retain = false
                                                });
                                            }

                                            //await MQTTClient.SendMQTTMessage(MQTTServer, SensorHoldTopic + "/" + sensorName, "0", false);
                                        }
                                        else
                                        {
                                            lbOutput.Items.Add("Sensor release failure - couldn't get live block for " + pbso.BlockUserName);
                                        }

                                    }
                                    else
                                    {
                                        //lbOutput.Items.Add("totalMM " + totalMMCoveredSinceExitingPBSO.ToString()+" - length"+log.TrainLengthMM.ToString());
                                    }
                                }
                            }
                        }
                        else
                        {
                            //lbOutput.Items.Add("Train length issue - " + log.TrainLengthMM.ToString());
                        }

                        if (log.CurrentBlockBNL == null || log.NextBlockBNL == null || log.TwoBlocksBNL == null) continue;
                        else if (log.IsAutomated) continue;

                        //Check to see if current block value has changed since the block went active - could happen if a train that started with a random ID has just gone over an RFID reader
                        var liveThisBlock = newBlockStates.FirstOrDefault(f => f.data.userName == log.CurrentBlock);
                        if (liveThisBlock != null && liveThisBlock.data != null)
                        {
                            if (liveThisBlock.data.state == 2)
                            {
                                if (liveThisBlock.data.value != null && liveThisBlock.data.value.data != null)
                                {
                                    if (liveThisBlock.data.value.data.userName != log.DCCiD)
                                    {
                                        var test = "changed alert";
                                        var newTrainLogName = "M-"+liveThisBlock.data.value.data.userName;
                                        if (!string.IsNullOrEmpty(liveThisBlock.data.value.data.name))
                                        {
                                            newTrainLogName = liveThisBlock.data.value.data.name;
                                        }
                                        log = await UpdateTrainNameAndIDInLog(log,newTrainLogName, log.DCCiD, liveThisBlock.data.value.data.userName);
                                    }
                                }
                            }
                        }


                        var issueFoundThisBlock = false;
                        var issueFoundNextBlock = false;
                        var issueFoundTwoBlocks = false;
                        var issueThisBlockDetails = "";
                        var issueNextBlockDetails = "";
                        var issueTwoBlockDetails = "";
                        var allocateNextBlock = false;
                        var allocateTwoBlocks = false;

                        //Go back to the start of the block in case we're joining it in the middle
                        var currentBlockRoute = await NavigateThroughBlockItems(log.CurrentBlockBNL.BlockChecked, log.CurrentBlockBNL.PreviousBlock, log.CurrentBlockBNL.EdgeConnector, log.CurrentBlockBNL.EdgeConnectorDirectionConnector, log.CurrentBlockBNL.EdgeConnector);
                        var nextBlock = await NavigateThroughBlockItems(currentBlockRoute.BlockFound, currentBlockRoute.BlockFound, currentBlockRoute.EdgeConnector, currentBlockRoute.EdgeConnectorDirectionConnector, currentBlockRoute.EdgeConnector);
                        var twoBlock = await NavigateThroughBlockItems(nextBlock.BlockFound, nextBlock.BlockChecked, nextBlock.EdgeConnector, nextBlock.EdgeConnectorDirectionConnector, nextBlock.EdgeConnector);
                        if (currentBlockRoute.BlockFound != log.CurrentBlockBNL.BlockFound)
                        {
                            var oldNextBlock = nextBlock.BlockChecked;
                            nextBlock = await NavigateThroughBlockItems(currentBlockRoute.BlockFound, currentBlockRoute.BlockFound, currentBlockRoute.EdgeConnector, currentBlockRoute.EdgeConnectorDirectionConnector, currentBlockRoute.EdgeConnector);
                            twoBlock = await NavigateThroughBlockItems(nextBlock.BlockFound, nextBlock.BlockChecked, nextBlock.EdgeConnector, nextBlock.EdgeConnectorDirectionConnector, nextBlock.EdgeConnector);

                            lbOutput.Items.Add("Route change detected from next block " + oldNextBlock + " to " + nextBlock.BlockChecked);

                            if (AllocateBlocks)
                            {
                                var sysName = config.GetBlockByUserName(log.CurrentBlockBNL.BlockFound);
                                if (usingMQTT)
                                {
                                    MQTTMessages.Enqueue(new MQTTMessage()
                                    {
                                        Topic = BlockReleaseTopic + "/" + sysName.userName,
                                        Payload = sysName.userName,
                                        Retain = false
                                    });
                                }

                                //await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                                await webClient.AllocateBlock(sysName.userName, "");
                                sysName = config.GetBlockByUserName(log.NextBlockBNL.BlockFound);
                                if (usingMQTT)
                                {
                                    MQTTMessages.Enqueue(new MQTTMessage()
                                    {
                                        Topic = BlockReleaseTopic + "/" + sysName.userName,
                                        Payload = sysName.userName,
                                        Retain = false
                                    });
                                }
                                //await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                                await webClient.AllocateBlock(sysName.userName, "");

                                log.AllocatedBlocks.Remove(log.CurrentBlockBNL.BlockFound);
                                log.AllocatedBlocks.Remove(log.NextBlockBNL.BlockFound);
                            }

                            allocateNextBlock = true;
                            allocateTwoBlocks = true;

                            var alertsForNextBlock = alerts.Where(w => w.BNL.BlockChecked == nextBlock.BlockChecked && w.TrainName == log.Name);
                            foreach (var alert in alertsForNextBlock)
                            {
                                lbOutput.Items.Add("468 deactivate alert (route change)");
                                DeactivateAlert(alert, false);
                            }

                            var alertsForTwoBlock = alerts.Where(w => w.BNL.BlockChecked == twoBlock.BlockChecked && w.TrainName == log.Name);
                            foreach (var alert in alertsForTwoBlock)
                            {
                                lbOutput.Items.Add("475 deactivate alert (route change)");
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
                            var oldTwoBlock = twoBlock.BlockChecked;
                            twoBlock = await NavigateThroughBlockItems(nextBlock.BlockFound, nextBlock.BlockChecked, nextBlock.EdgeConnector, nextBlock.EdgeConnectorDirectionConnector, nextBlock.EdgeConnector);
                            lbOutput.Items.Add("Route change detected from two block " + oldTwoBlock + " to " + twoBlock.BlockChecked);
                            if (AllocateBlocks)
                            {
                                var sysName = config.GetBlockByUserName(log.NextBlockBNL.BlockFound);
                                if (usingMQTT)
                                    MQTTMessages.Enqueue(new MQTTMessage() { Topic = BlockReleaseTopic + "/" + sysName.userName, Payload = sysName.userName, Retain = false });
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
                        string nextBlockCurrentAllocation = string.Empty;
                        string twoBlockCurrentAllocation = string.Empty;

                        bool nextBlockAvailable = true;
                        bool twoBlockAvailable = true;

                        bool nextBlockAlreadyAllocated = false;
                        bool twoBlockAlreadyAllocated = false;

                        var currentBlockcfg = config.GetBlockByUserName(currentBlockRoute.BlockChecked);
                        if (nextBlock != null)
                        {
                            var liveNextBlock = newBlockStates.FirstOrDefault(f => f.data.userName == nextBlock.BlockChecked);
                            if (liveNextBlock != null)
                            {
                                if (liveNextBlock.data.state == 2)
                                {
                                    issueFoundNextBlock = true;
                                    if (liveNextBlock.data.value != null)
                                    {
                                        issueNextBlockDetails += "; Occupied by " + liveNextBlock.data.value.data.userName;
                                    }
                                    else
                                    {
                                        issueNextBlockDetails += "; Occupied";
                                    }
                                    nextBlockAvailable = false;
                                }
                                else
                                {
                                    if (liveNextBlock.data.value != null && !string.IsNullOrEmpty(liveNextBlock.data.value.data.userName))
                                    {
                                        nextBlockCurrentAllocation = liveNextBlock.data.value.data.userName;
                                        if (nextBlockCurrentAllocation != log.DCCiD && nextBlockCurrentAllocation != log.OriginalDCCiD)
                                        {
                                            issueFoundNextBlock = true;
                                            issueNextBlockDetails += "; Allocated to " + liveNextBlock.data.value.data.userName;
                                            nextBlockAvailable = false;
                                        }
                                        else
                                        {
                                            nextBlockAlreadyAllocated = true;
                                        }
                                    }
                                    else
                                    {
                                        allocateNextBlock = true;
                                    }
                                }
                            }
                        }


                        if (twoBlock != null)
                        {
                            var liveTwoBlocks = newBlockStates.FirstOrDefault(f => f.data.userName == twoBlock.BlockChecked);
                            if (liveTwoBlocks != null)
                            {
                                if (liveTwoBlocks.data.state == 2)
                                {
                                    issueFoundTwoBlocks = true;
                                    twoBlockAvailable = false;
                                    if (liveTwoBlocks.data.value != null)
                                    {
                                        issueTwoBlockDetails += "; Occupied by " + liveTwoBlocks.data.value.data.userName;
                                    }
                                    else
                                    {
                                        issueTwoBlockDetails += "; Occupied";
                                    }

                                }
                                else
                                {
                                    if (liveTwoBlocks.data.value != null && !string.IsNullOrEmpty(liveTwoBlocks.data.value.data.userName))
                                    {
                                        twoBlockCurrentAllocation = liveTwoBlocks.data.value.data.userName;
                                        if (twoBlockCurrentAllocation != log.DCCiD && twoBlockCurrentAllocation != log.OriginalDCCiD)
                                        {
                                            issueFoundTwoBlocks = true;
                                            issueTwoBlockDetails += "; Allocated to " + liveTwoBlocks.data.value.data.userName;
                                            twoBlockAvailable = false;
                                        }
                                        else
                                        {
                                            twoBlockAlreadyAllocated = true;
                                        }
                                    }
                                    else
                                    {
                                        allocateTwoBlocks = true;
                                    }
                                }
                            }
                        }

                        log.CurrentBlock = currentBlockRoute.BlockChecked;
                        if (nextBlock != null) log.NextBlock = nextBlock.BlockChecked;
                        if (twoBlock != null) log.NextNextBlock = twoBlock.BlockChecked;
                        log.CurrentBlockBNL = currentBlockRoute;
                        log.NextBlockBNL = nextBlock;
                        log.TwoBlocksBNL = twoBlock;

                        if (issueFoundThisBlock)
                        {
                            var alertExists = alerts.Any(a => a.AffectedBlockUserName == currentBlockRoute.BlockChecked && a.Severity == AlertSeverity.Extreme && a.Deactivated == false);
                            var existingAlert = alerts.FirstOrDefault(a => a.AffectedBlockUserName == currentBlockRoute.BlockChecked && a.Severity == AlertSeverity.Extreme && a.Deactivated == false && a.Deactivated == false);
                            log.SignalAspect = SignalAspect.Danger;
                            log.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Stop;
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
                            log.SignalAspect = SignalAspect.Danger;
                            log.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                            var alertExists = alerts.Any(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == nextBlock.BlockChecked && a.Severity == AlertSeverity.Danger && a.Deactivated == false);
                            var existingAlert = alerts.FirstOrDefault(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == nextBlock.BlockChecked && a.Severity == AlertSeverity.Danger && a.Deactivated == false);
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

                        if (issueFoundTwoBlocks)
                        {
                            //Caution alert
                            var alertExists = alerts.Any(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == twoBlock.BlockChecked && a.Severity == AlertSeverity.Caution && a.Deactivated == false);
                            var existingAlert = alerts.FirstOrDefault(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == twoBlock.BlockChecked && a.Severity == AlertSeverity.Caution && a.Deactivated == false);
                            var dangerAlertExistsForNextBlock = alerts.Any(a => a.BNL.BlockChecked == nextBlock.BlockChecked && a.Severity == AlertSeverity.Danger && a.TrainName == log.Name);
                            log.SignalAspect = SignalAspect.Caution;
                            log.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Caution;
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

                                    });
                                }
                            }
                        }

                        if (allocateNextBlock && nextBlockAvailable && AllocateBlocks && !nextBlockAlreadyAllocated && !issueFoundTwoBlocks)
                        {
                            nextBlockAlreadyAllocated = true;
                            lbOutput.Items.Add("Dynamic allocation of next block " + currentBlockRoute.BlockFound + " to " + log.DCCiD);
                            var nbConfig = config.GetBlockByUserName(currentBlockRoute.BlockFound);
                            if (usingMQTT)
                            {
                                MQTTMessages.Enqueue(new MQTTMessage()
                                {
                                    Topic = BlockAllocateTopic + "/" + nbConfig.userName,
                                    Payload = nbConfig.userName,
                                    Retain = false
                                });
                            }                                

                           // await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + nbConfig.userName, nbConfig.userName, false);
                            await webClient.AllocateBlock(nbConfig.userName, log.DCCiD);
                            if (!log.AllocatedBlocks.Contains(currentBlockRoute.BlockFound))
                                log.AllocatedBlocks.Add(currentBlockRoute.BlockFound);
                        }

                        if (allocateTwoBlocks && nextBlockAvailable && twoBlockAvailable && AllocateBlocks && nextBlockAlreadyAllocated && !twoBlockAlreadyAllocated)
                        {
                            lbOutput.Items.Add("Dynamic allocation of two block" + nextBlock.BlockFound + " to " + log.DCCiD);
                            var tbConfig = config.GetBlockByUserName(nextBlock.BlockFound);
                            if ( usingMQTT)
                            {
                                MQTTMessages.Enqueue(new MQTTMessage()
                                {
                                    Topic = BlockAllocateTopic + "/" + tbConfig.userName,
                                    Payload = tbConfig.userName,
                                    Retain = false
                                });
                            }

                            //await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + tbConfig.userName, tbConfig.userName, false);
                            await webClient.AllocateBlock(tbConfig.systemName, log.DCCiD);

                            if (!log.AllocatedBlocks.Contains(nextBlock.BlockFound))
                                log.AllocatedBlocks.Add(nextBlock.BlockFound);
                        }

                        if (!issueFoundThisBlock && !issueFoundNextBlock && !issueFoundTwoBlocks)
                        {
                            if (log.SignalAspect != SignalAspect.Proceed)
                            {
                                //resuming?
                                log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Resuming;
                                lbOutput.Items.Add("Resume mid-block detected for " + log.Name);
                                SoundPlayer signalBeep = new SoundPlayer("./Assets/Proceed.wav");
                                signalBeep.Play();
                            }
                            else
                            {
                                log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Running;
                            }
                            log.SignalAspect = SignalAspect.Proceed;
                            log.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Full;
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
                    var logName = log.Name;
                    var reason = log.TerminatedReason;
                    TerminateTrain(log.Name);
                    ListViewItem item = new ListViewItem();
                    item.Text = logName + " terminated - " + reason;
                    item.BackColor = Color.LimeGreen;
                    lvUpdates.Items.Add(item);
                    lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                    lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    var thisThrottle = wt.GetThrottleInfoByDCCID(log.DCCiD);
                    if (thisThrottle != null)
                    {
                        wt.ReleaseThrottle(thisThrottle.RosterIndex);
                    }
                }
            }

            allBlocks = newBlockStates;
            activeBlocks = newActiveBlocks;
            return true;
        }

        private TrainMotionConfig GetTrainMotionConfig(Locomotive fullInfo, string trainName, string DCCId)
        {
            TrainMotionConfig config = new TrainMotionConfig();
            //var rosterCfG = new RosterReader(RosterPath);
            //var roster = rosterCfG.LocoList;
            //var fullInfo = rosterCfG.FullRoster.FirstOrDefault(f => f.DccAddress == DCCId);

            if (fullInfo == null)
            {
                return null;
            }

            var fullSpeed = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerFullMMS");
            var cautionSpeed = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerCautionMMS");
            var crawlSpeed = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerCrawlMMS");
            var rampDownInterval = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerRampDownIntervalMS");
            var rampUpInterval = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerRampUpIntervalMS");
            var rampDownStep = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerRampDownStep");
            var rampUpStep = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerRampUpStep");
            var trainLength = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerTrainLengthMM");

            int fulSpeedMMS = DefaultFullSpeedMMS;
            int cautionMMS = DefaultCautionMMS;
            int crawlMMS = DefaultCrawlMMS;

            if (fullSpeed != null)
            {
                int.TryParse(fullSpeed.Value, out fulSpeedMMS);
            }
            if (cautionSpeed != null)
            {
                int.TryParse(cautionSpeed.Value, out cautionMMS);
            }
            if (crawlSpeed != null)
            {
                int.TryParse(crawlSpeed.Value, out crawlMMS);
            }

            int trainLengthMM = 0;

            if (trainLength != null)
            {
                int.TryParse(trainLength.Value, out trainLengthMM);
            }

            config.Name = trainName;
            config.DCCID = DCCId;

            if (fullInfo != null && fullInfo.Speedprofile != null)
            {
                var firstSpeed = fullInfo.Speedprofile.Speeds.Speed.FirstOrDefault();
                if (firstSpeed != null)
                {
                    decimal prevForwardSpeed = 0.0M;
                    decimal prevReverseSpeed = 0.0M;
                    decimal prevStep = 0.0M;

                    foreach (var step in fullInfo.Speedprofile.Speeds.Speed)
                    {
                        var tStep = step.Step;
                        var speed = step.Forward;
                        decimal dForward = 0.0M;
                        decimal dReverse = 0.0M;
                        decimal dStep = 0.0M;

                        bool fSuccess = decimal.TryParse(step.Forward, out dForward);
                        bool rSuccess = decimal.TryParse(step.Reverse, out dReverse);
                        bool dSuccess = decimal.TryParse(step.Step, out dStep);


                        if (fSuccess && rSuccess)
                        {
                            if ((crawlMMS < dForward && crawlMMS > prevForwardSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(crawlMMS, prevForwardSpeed, dForward);
                                var calculatedMMS = prevForwardSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                config.ForwardCrawlSpeedStep = calculatedSpeedStep;
                                config.ForwardCrawlMMS = calculatedMMS;
                            }
                            if ((crawlMMS < dReverse && crawlMMS > prevReverseSpeed))
                            {

                                var calc = GetRelativeSpeedPercentage(crawlMMS, prevReverseSpeed, dReverse);
                                var calculatedMMS = prevReverseSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                config.ReverseCrawlSpeedStep = calculatedSpeedStep;
                                config.ReverseCrawlMMS = calculatedMMS;
                            }


                            if ((cautionMMS < dForward && cautionMMS > prevForwardSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(cautionMMS, prevForwardSpeed, dForward);
                                var calculatedMMS = prevForwardSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                config.ForwardCautionSpeedStep = calculatedSpeedStep;
                                config.ForwardCautionMMS = calculatedMMS;
                            }
                            if ((cautionMMS < dReverse && cautionMMS > prevReverseSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(cautionMMS, prevReverseSpeed, dReverse);
                                var calculatedMMS = prevReverseSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                config.ReverseCautionSpeedStep = calculatedSpeedStep;
                                config.ReverseCautionMMS = calculatedMMS;
                            }

                            if (fulSpeedMMS < dForward && fulSpeedMMS > prevForwardSpeed)
                            {
                                var calc = GetRelativeSpeedPercentage(fulSpeedMMS, prevForwardSpeed, dForward);
                                var calculatedMMS = prevForwardSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                config.ForwardFullSpeedStep = calculatedSpeedStep;
                                config.ForwardFullSpeedMMS = calculatedMMS;
                            }
                            if ((fulSpeedMMS < dReverse && fulSpeedMMS > prevReverseSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(fulSpeedMMS, prevReverseSpeed, dReverse);
                                var calculatedMMS = prevReverseSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                config.ReverseFullSpeedStep = calculatedSpeedStep;
                                config.ReverseFullSpeedMMS = calculatedMMS;
                            }

                        }
                        prevForwardSpeed = dForward;
                        prevReverseSpeed = dReverse;
                        prevStep = dStep;
                    }

                }
            }
            if (fullInfo == null || fullInfo.Speedprofile == null || config.ForwardCrawlSpeedStep == 0 || config.ReverseCrawlSpeedStep == 0 || config.ForwardCautionSpeedStep == 0
                || config.ReverseCrawlSpeedStep == 0 || config.ForwardFullSpeedStep == 0 || config.ReverseFullSpeedStep == 0)
            {
                //WriteToLog("Speed profile not complete for this train - can't run it");
                return null;
            }

            int rampUpStepIncrease = 3;
            int rampUpIntervalMS = 200;
            int rampDownStepDecrease = 3;
            int rampDownIntervalMS = 200;

            if (rampDownStep != null)
            {
                int.TryParse(rampDownStep.Value, out rampDownStepDecrease);
            }

            if (rampDownInterval != null)
            {
                int.TryParse(rampDownInterval.Value, out rampDownIntervalMS);
            }

            if (rampUpStep != null)
            {
                int.TryParse(rampUpStep.Value, out rampUpStepIncrease);
            }

            if (rampUpInterval != null)
            {
                int.TryParse(rampUpInterval.Value, out rampUpIntervalMS);
            }

            config.RampUpSpeedStepIncrease = rampUpStepIncrease;
            config.RampUpIntervalMS = rampUpIntervalMS;
            config.RampDownIntervalMS = rampDownIntervalMS;
            config.RampDownSpeedStepDecrease = rampDownStepDecrease;

            config.CurrentSpeedStep = 0;
            config.TargetSpeedStep = 0;
            config.IsActive = true;
            return config;
        }

        private (decimal percentage, decimal speed) GetRelativeSpeedPercentage(int targetMMS, decimal prevForwardSpeed, decimal thisForwardSpeed)
        {
            decimal a = targetMMS - prevForwardSpeed;
            decimal b = thisForwardSpeed - prevForwardSpeed;
            decimal frac = 0.0M;
            if (b == 0)
            {
                frac = 100;
            }
            else
                frac = (a / b) * 100;

            //frac now equals percentage
            //get that percentage of the full forward speed for relative speed
            var scale = thisForwardSpeed - prevForwardSpeed;
            decimal perc = 0.0M;
            if (scale == 0)
                perc = scale;
            else
                perc = (frac / 100) * scale;
            return (frac, perc);
        }

        private int GetRelativeSpeedStep(decimal percentage, decimal prevStep, decimal thisStep)
        {
            //work out percentage position between prevStep and Step
            decimal pos = 0.0M;

            var scale = thisStep - prevStep;
            if (scale == 0)
                pos = scale;
            else
                pos = (percentage / 100) * scale;

            //then add the pos to the ... step?
            var requiredStep = prevStep + pos;
            var asPerc1 = requiredStep / 1000;
            var beforeRound = asPerc1 * 128;
            int dSS = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
            return dSS;
        }

        private void CheckRunningTrainsForUnattendedSpeedChanges()
        {

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
            var blockLog = new LiveJourneyLog();
            blockLog.History = new List<string>();
            blockLog.AllocatedBlocks = new List<string>();
            BlockRootObject previousBlock = new BlockRootObject();
            bool okToRenameLog = true;

            LiveJourneyLog existingLog = null;
            string prevBlockAllocatedId = string.Empty;

            var potentialLogs = Log.Where(w => w.NextBlock == block.data.userName);
            var prevBlockState = allBlocks.FirstOrDefault(f => f.data.name == block.data.name);

            var automatedIDs = new List<string>();

            var automatedMem = await webClient.GetMemory(memoryAllocatedAutoTrainsName);
            if (automatedMem != null)
            {
                var currentVal = automatedMem.data.value;
                if (currentVal != null)
                {
                    automatedIDs = automatedMem.data.value.Split(';').ToList();
                }
            }

            if (block.data.value != null)
            {
                existingLog = Log.FirstOrDefault(f => f.DCCiD == block.data.value.data.userName && f.NextBlock == block.data.userName);
                if (existingLog != null)
                {
                    lbOutput.Items.Add("New block " + block.data.userName + " found simplest way for DCC ID " + existingLog.DCCiD);
                }
                else
                {
                    //Handle possible incorrect block value assignment by JMRI?
                    if (prevBlockState != null && prevBlockState.data.value != null)
                    {
                        existingLog = Log.FirstOrDefault(f => f.DCCiD == prevBlockState.data.value.data.userName && f.NextBlock == block.data.userName);
                        if (existingLog != null)
                        {
                            lbOutput.Items.Add("New block " + block.data.userName + " found by matching up allocation for DCC ID " + existingLog.DCCiD);
                            block.data.value = prevBlockState.data.value;
                            await webClient.AllocateBlock(block.data.name, existingLog.DCCiD);
                        }
                    }

                }
            }
            else
            {
                if (prevBlockState != null && prevBlockState.data.value != null)
                {
                    existingLog = Log.FirstOrDefault(f => f.DCCiD == prevBlockState.data.value.data.userName && f.NextBlock == block.data.userName);
                    if (existingLog != null)
                    {
                        lbOutput.Items.Add(block.data.userName + " matched to log but nab appears to have no value for DCC ID " + existingLog.DCCiD);
                        block.data.value = prevBlockState.data.value;
                        await webClient.AllocateBlock(block.data.name, existingLog.DCCiD);
                    }
                    else
                    {
                        return (false, "No block value");
                    }
                }
                else
                {
                    return (false, "No block value");
                }
            }

            if (existingLog == null && block.data.value != null)
            {
                existingLog = Log.FirstOrDefault(f => f.DCCiD == block.data.value.data.userName);
                if (existingLog != null)
                    lbOutput.Items.Add("ID matched by pure ID - maybe an automated train " + block.data.userName + " to " + existingLog.DCCiD);
            }

            if (existingLog == null)
            {
                handlingNewTrain = true;
                if (block.data.value != null && !string.IsNullOrEmpty(block.data.value.data.userName))
                {
                    var re = Roster.FirstOrDefault(f => f.ID == block.data.value.data.userName);
                    if (re != null)
                    {
                        blockLog.Name = re.Name;
                        blockLog.DCCiD = re.ID;
                        blockLog.OriginalName = re.Name;
                        blockLog.OriginalDCCiD = re.ID;
                    }
                    else
                    {
                        blockLog.Name = block.data.value.data.userName;
                        blockLog.DCCiD = block.data.value.data.comment;
                        blockLog.OriginalName = block.data.value.data.userName;
                        blockLog.OriginalDCCiD = block.data.value.data.comment;
                    }
                }
                else
                {
                    //return (false,"No usable ID yet");
                    var index = block.data.userName.IndexOf(' ');
                    var prefix = block.data.userName.Substring(0, index);
                    var newTrainRandomId = "";
                    while (newTrainRandomId == "" || Log.Any(a => a.DCCiD == newTrainRandomId) || automatedIDs.Contains(newTrainRandomId))
                    {
                        var guid = Guid.NewGuid().ToString();
                        newTrainRandomId = guid.Substring(0, 3);
                    }

                    blockLog.Name = "M-" + newTrainRandomId;
                    blockLog.DCCiD = newTrainRandomId;
                    blockLog.OriginalName = "M-" + newTrainRandomId;
                    blockLog.OriginalDCCiD = newTrainRandomId;

                    //Populate ID in current block
                    await webClient.AllocateBlock(block.data.name, blockLog.DCCiD, false);
                }

                if (automatedIDs.Contains(blockLog.DCCiD))
                {
                    blockLog.IsAutomated = true;
                    ListViewItem aItem = new ListViewItem();
                    aItem.Text = blockLog.Name + " - automated train - not tracking";
                    aItem.BackColor = Color.LimeGreen;

                    lvUpdates.Items.Add(aItem);
                    lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                    lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                }

                if (!blockLog.IsAutomated)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = blockLog.Name + " - started tracking";
                    item.BackColor = Color.LimeGreen;

                    lbOutput.Items.Add("Started new journey tracking for " + blockLog.Name + " ID " + blockLog.DCCiD);
                    lvUpdates.Items.Add(item);
                    lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                    lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    ddlTrainSelector.Items.Add(blockLog.Name);
                }
                //blockLog.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Running;


                blockLog.AutomatedBlockList = new List<BlockJourneyLog>();
                
                blockLog.HasSpeedProfile = false;

                var rosterCfG = new RosterReader(RosterPath);
                var roster = rosterCfG.LocoList;
                var fullInfo = rosterCfG.FullRoster.FirstOrDefault(f => f.DccAddress == blockLog.DCCiD);

                if (fullInfo != null)
                {
                    blockLog.TrainMotionCfg = GetTrainMotionConfig(fullInfo, blockLog.Name,blockLog.DCCiD);
                    blockLog.fullRosterInfo = fullInfo;
                    var trainLength = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerTrainLengthMM");
                    int trainLengthMM = 0;

                    if (trainLength != null)
                    {
                        int.TryParse(trainLength.Value, out trainLengthMM);
                    }

                    blockLog.TrainLengthMM = trainLengthMM;
                    lbOutput.Items.Add("Train length " + blockLog.TrainLengthMM.ToString());
                    if (fullInfo.Speedprofile != null && fullInfo.Speedprofile.Speeds.Speed.Count > 0)
                    {
                        blockLog.HasSpeedProfile = true;
                        lbOutput.Items.Add("Speed profile found - steps " + fullInfo.Speedprofile.Speeds.Speed.Count.ToString());
                    }
                }

                var currentMem = await webClient.GetMemory(memoryAllocatedManualTrainsName);
                if (currentMem != null)
                {
                    string updateVal = string.Empty;
                    var currentVal = currentMem.data.value;
                    if (currentVal != null)
                    {
                        var currentList = currentVal.Split(';').ToList();
                        if (!currentList.Contains(blockLog.DCCiD))
                        {
                            updateVal = currentVal + ";" + blockLog.DCCiD;
                        }
                        else
                        {
                            updateVal = currentVal;
                        }
                    }
                    else
                    {
                        updateVal = blockLog.DCCiD + ";";
                    }
                    await webClient.UpdateMemory(memoryAllocatedManualTrainsName, updateVal);
                }
                else
                {
                    var updateVal = blockLog.DCCiD + ";";
                    await webClient.UpdateMemory(memoryAllocatedManualTrainsName, updateVal);
                }
            }
            else
            {
                handlingNewTrain = false;
                blockLog = existingLog;
                blockLog.ProcessingNewBlock = true;
                if (block.data.value != null && !string.IsNullOrEmpty(block.data.value.data.userName) && !string.IsNullOrEmpty(block.data.value.data.comment) &&
                    (block.data.value.data.userName != blockLog.DCCiD || block.data.value.data.comment != blockLog.Name))
                {
                    if (okToRenameLog)
                    {
                        lbOutput.Items.Add("Name change on value acquisition - " + blockLog.Name + " & " + blockLog.DCCiD + " - to " + block.data.value.data.comment + " & " + block.data.value.data.userName);
                        if (blockLog != null && blockLog.Name != null && blockLog.Name != "" && ddlTrainSelector.Items.Contains(blockLog.Name))
                        {
                            ddlTrainSelector.Items.Remove(blockLog.Name);
                        }
                        if (!string.IsNullOrEmpty(block.data.value.data.comment))
                            blockLog.Name = block.data.value.data.comment;
                        else
                            blockLog.Name = block.data.value.data.userName;

                        blockLog.DCCiD = block.data.value.data.userName;
                        ddlTrainSelector.Items.Add(blockLog.Name);
                    }
                    else
                    {
                        lbOutput.Items.Add("Ignored rename, bad block value? Sending " + existingLog.DCCiD + " to replace it");
                        await webClient.AllocateBlock(block.data.name, existingLog.DCCiD, false);
                    }
                }
            }

            var existingThrottle = wt.GetThrottleInfoByDCCID(blockLog.DCCiD);
            if (existingThrottle == null)
            {
                var rosterEntry = wt.Roster.FirstOrDefault(f => f.ID == blockLog.DCCiD);
                var indexOfRE = wt.Roster.IndexOf(rosterEntry);
                var mtIndex = wt.GetThrottle(indexOfRE);
                existingThrottle = wt.GetThrottleInfoByDCCID(blockLog.DCCiD);
            }

            if (ddlTrainSelector.Text == blockLog.Name)
                lbJourneyLog.Items.Add(block.data.userName);

            if (!blockLog.IsAutomated && blockLog.AllocatedBlocks != null && blockLog.AllocatedBlocks.Contains(block.data.userName))
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
            if (!blockLog.IsAutomated && (connector1 == "" || connector2 == "" || previousConnector == ""))
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

            if (likelyPreviousBlock == "")
            {
                likelyPreviousBlock = blockLog.CurrentBlock;
            }

            blockLog.PreviousBlock = likelyPreviousBlock;
            blockLog.History.Add(block.data.userName);
            blockLog.CurrentBlock = block.data.userName;
            blockLog.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Caution;

            BlockJourneyLog bjl = new BlockJourneyLog();
            bjl.BlockSystemname = block.data.name;
            bjl.BlockLengthMM = new decimal(block.data.length);
            bjl.BlockUserName = block.data.userName;
            bjl.SequenceState = JourneySequenceState.Active;
            bjl.OccupationSensorSystemName = block.data.sensor.Substring(2); ;
            //lbOutput.Items.Add("Sensor for new block " + bjl.OccupationSensorSystemName);
            bjl.SpeedLog = new List<SpeedStepLog>();


            //var throttle = wt.GetThrottleInfoByDCCID(blockLog.DCCiD);
            if (existingThrottle != null)
            {
                var ssl = new SpeedStepLog();
                ssl.start = DateTime.Now;
                ssl.SpeedStep = existingThrottle.Speed;

                TrainDirection dir = TrainDirection.Forward;
                if (existingThrottle.Direction != "1") dir = TrainDirection.Reverse;

                var mms = GetMMSFromSpeedStep(blockLog.fullRosterInfo, existingThrottle.Speed, blockLog.DCCiD, dir);
                ssl.SpeedMMS = mms;
                bjl.SpeedLog.Add(ssl);
                blockLog.CurrentSpeedStep = existingThrottle.Speed;
                //lbOutput.Items.Add("Got throttle and initial speed step, direction " + dir.ToString()+" speed MM "+mms.ToString());
            }

            blockLog.AutomatedBlockList.Add(bjl);
            var indexOfBJL = blockLog.AutomatedBlockList.IndexOf(bjl);
            if (indexOfBJL > 0 && blockLog.TrainLengthMM > 0 && blockLog.HasSpeedProfile)
            {
                var previousBlockLog = blockLog.AutomatedBlockList.ElementAtOrDefault(indexOfBJL - 1);

                if (previousBlockLog != null && blockLog.TrainLengthMM > 0)
                {
                    previousBlockLog.SequenceState = JourneySequenceState.EnteredNextBlock;

                    var previousLiveBlock = allBlocks.FirstOrDefault(f => f.data.name == previousBlockLog.BlockSystemname);
                    if (previousLiveBlock != null)
                    {
                        var sensorName = previousLiveBlock.data.sensor.Substring(2);
                        lbOutput.Items.Add(existingLog.DCCiD + " Sensor hold for " + sensorName);
                        if (usingMQTT)
                            MQTTMessages.Enqueue( new MQTTMessage() {Topic = SensorHoldTopic + "/" + sensorName, Payload = "1", Retain = false });
                        //await MQTTClient.SendMQTTMessage(MQTTServer, SensorHoldTopic + "/" + sensorName, "1", false);
                    }
                    else
                    {
                        lbOutput.Items.Add("Sensor hold failure - couldn't find log block for " + previousBlockLog.BlockUserName);
                    }
                }
            }

            if (blockLog.PreviousBlock == blockLog.CurrentBlock)
            {
                return (false, "Previous log block = current log block");
            }


            if (!issueFoundNextBlock)
            {
                //var bnl = await NavigateThroughBlockItems(block.data.userName, likelyPreviousBlock, firstBoundary.EdgeConnectorDirectionConnector, firstBoundary.EdgeConnector, firstBoundary.EdgeConnector);
                var bnl = await NavigateThroughBlockItems(block.data.userName, likelyPreviousBlock, firstBoundary.EdgeConnector, firstBoundary.EdgeConnectorDirectionConnector, firstBoundary.EdgeConnector);
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
                if (handlingNewTrain)
                {
                    var nextBlockLive = await webClient.GetBlock(likelyNextBlock);
                    var previousBlockLive = await webClient.GetBlock(likelyPreviousBlock);
                    if (nextBlockLive != null && previousBlockLive != null)
                    {
                        if (nextBlockLive.data.state == 2 && previousBlockLive.data.state == 2) return (false, "Surrounded by active blocks");
                    }
                }


                blockLog.NextBlock = likelyNextBlock;
                blockLog.CurrentBlockBNL = bnl;
                BNLThisBlock.BlockCheckedSystemName = block.data.userName;

                //Don't need to process alerts etc for auto train, but needed to record next block for log matching when new blocks go active
                //That's done now so can exit here for automated trains
                if (blockLog.IsAutomated)
                {
                    if (handlingNewTrain)
                    {
                        Log.Add(blockLog);
                    }

                    if (BNLThisBlock.NoMoreBlocksFound)
                    {
                        lbOutput.Items.Add("No more blocks found - " + existingLog.DCCiD);
                        existingLog.Terminated = true;
                        existingLog.TerminatedReason = "No more blocks found for automated train";
                    }
                    return (true, "Automated processing successful");
                }

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
                    BNLThisBlock.LikelyIssue = "End of line";
                    issueFoundThisBlock = true;
                }

                BNLNextBlock = await NavigateThroughBlockItems(likelyNextBlock, BNLThisBlock.BlockChecked, BNLThisBlock.EdgeConnector, BNLThisBlock.EdgeConnectorDirectionConnector, BNLThisBlock.EdgeConnector);
                if (BNLNextBlock != null && !noMoreBlocks)
                {
                    blockLog.NextNextBlock = BNLNextBlock.BlockFound;

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
                            BNLNextBlock.LikelyIssue = likelyIssueNextBlock;
                        }
                        if (liveNextBlock.data.value != null && TrackAllocation && !string.IsNullOrEmpty(liveNextBlock.data.value.data.userName)
                            && liveNextBlock.data.value.data.userName != blockLog.DCCiD && liveNextBlock.data.value.data.userName != blockLog.OriginalDCCiD && TrackAllocation && liveNextBlock.data.state == 4)
                        {
                            issueFoundNextBlock = true;
                            BNLNextBlock.BlockCheckedAllocatedTo = liveNextBlock.data.value.data.userName;
                            likelyIssueNextBlock += "Allocated to " + liveNextBlock.data.value + " ";
                            BNLNextBlock.LikelyIssue = likelyIssueNextBlock;
                        }
                    }

                    if (issueFoundNextBlock)
                    {
                        //lbOutput.Items.Add("New block check - issue next block - " + likelyIssueNextBlock);
                    }

                    if (BNLNextBlock.NoMoreBlocksFound)
                    {
                        noMoreBlocks = true;
                        BNLNextBlock.LikelyIssue = "End of line";
                        issueFoundNextBlock = true;
                    }

                    blockLog.NextBlockBNL = BNLNextBlock;

                    if (!blockLog.AllocatedBlocks.Contains(liveNextBlock.data.name))
                        blockLog.AllocatedBlocks.Add(liveNextBlock.data.userName);

                    BNLTwoBlocks = await NavigateThroughBlockItems(BNLNextBlock.BlockFound, BNLNextBlock.BlockChecked, BNLNextBlock.EdgeConnector, BNLNextBlock.EdgeConnectorDirectionConnector, BNLNextBlock.EdgeConnector);
                    if (BNLTwoBlocks != null)
                    {

                        if (!String.IsNullOrEmpty(BNLTwoBlocks.LikelyIssue))
                        {
                            issueFoundTwoBlocks = true;
                            likelyIssueTwoBlocks = BNLTwoBlocks.LikelyIssue + "; ";
                        }
                        var twoBlocksLiveBlock = await (webClient.GetBlock(BNLNextBlock.BlockFound));
                        if (twoBlocksLiveBlock != null && twoBlocksLiveBlock.data != null)
                        {
                            BNLTwoBlocks.BlockCheckedSystemName = twoBlocksLiveBlock.data.name;
                            if ((twoBlocksLiveBlock.data.value == null || twoBlocksLiveBlock.data.value == null || twoBlocksLiveBlock.data.value.data.userName != blockLog.DCCiD) && twoBlocksLiveBlock.data.state == 2)//occupied
                            {
                                issueFoundTwoBlocks = true;
                                likelyIssueTwoBlocks += "Collision ";// in "+twoBlocksLiveBlock.data.userName;
                                BNLTwoBlocks.LikelyIssue = likelyIssueTwoBlocks;
                            }
                            if (twoBlocksLiveBlock.data.value != null && TrackAllocation && !string.IsNullOrEmpty(twoBlocksLiveBlock.data.value.data.userName)
                                && twoBlocksLiveBlock.data.value.data.userName != blockLog.DCCiD && twoBlocksLiveBlock.data.value.data.userName != blockLog.OriginalDCCiD && TrackAllocation && twoBlocksLiveBlock.data.state == 4)
                            {
                                issueFoundTwoBlocks = true;
                                likelyIssueTwoBlocks += "Allocated to " + twoBlocksLiveBlock.data.value + " ";
                                BNLTwoBlocks.BlockCheckedAllocatedTo = twoBlocksLiveBlock.data.value.data.userName;
                                BNLTwoBlocks.LikelyIssue = likelyIssueTwoBlocks;
                            }
                        }

                        if (issueFoundTwoBlocks)
                        {
                            //lbOutput.Items.Add("New block check - issue two blocks - " + likelyIssueTwoBlocks);
                        }

                        if (BNLTwoBlocks.NoMoreBlocksFound)
                        {
                            noMoreBlocks = true;
                            BNLTwoBlocks.LikelyIssue = "End of line";
                            issueFoundTwoBlocks = true;
                        }

                        blockLog.TwoBlocksBNL = BNLTwoBlocks;

                        if (!blockLog.AllocatedBlocks.Contains(twoBlocksLiveBlock.data.userName))
                            blockLog.AllocatedBlocks.Add(twoBlocksLiveBlock.data.userName);
                    }
                }
            }

            if (issueFoundThisBlock)
            {
                var alertExists = alerts.Any(a => a.BlockSystemName == block.data.name && a.Severity == AlertSeverity.Extreme);
                blockLog.SignalAspect = SignalAspect.Danger;
                blockLog.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                if (!alertExists)
                {
                    //lbOutput.Items.Add("Add alert caution");
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
                blockLog.SignalAspect = SignalAspect.Danger;
                blockLog.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                //lbOutput.Items.Add("Add alert danger pre-check");
                if (!alertExists)
                {
                    //lbOutput.Items.Add("Add alert danger");
                    AddAlert(new Alert()
                    {
                        id = Guid.NewGuid(),
                        BlockSystemName = block.data.name,
                        BlockUserName = block.data.userName,
                        SignalMastSystemName = "",
                        SignalMastUserName = "",
                        Severity = AlertSeverity.Danger,
                        PreviousBlockUserName = previousBlock != null && previousBlock.data != null ? previousBlock.data.userName : "",
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
                blockLog.SignalAspect = SignalAspect.Caution;
                blockLog.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Caution;
                //lbOutput.Items.Add("Add alert caution pre-check");

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
                TerminateTrain(blockLog.Name);
                return (false, "No more blocks");
            }

            if (!issueFoundThisBlock && !issueFoundNextBlock && !issueFoundTwoBlocks)
            {
                //lbOutput.Items.Add(("Proceed " + blockUserName));
                // lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                if (blockLog.SignalAspect != SignalAspect.Proceed)
                {
                    //resuming?
                    blockLog.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Resuming;
                    lbOutput.Items.Add("Resume detected for " + blockLog.Name);
                    SoundPlayer signalBeep = new SoundPlayer("./Assets/Proceed.wav");
                    signalBeep.Play();
                }
                else
                {
                    blockLog.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Running;
                }
                blockLog.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Full;
                blockLog.SignalAspect = SignalAspect.Proceed;

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
                if (usingMQTT)
                    MQTTMessages.Enqueue(new MQTTMessage() { Topic = topic, Payload = "Proceed", Retain = false });
                //await MQTTClient.SendMQTTMessage(MQTTServer, topic, "Proceed", false);

                var alertsForThisBlock = alerts.Where(w => w.BlockSystemName == block.data.name && !w.Deactivated);
                foreach (var alert in alertsForThisBlock)
                {
                    //lbOutput.Items.Add("1162 deactivate alert");
                    DeactivateAlert(alert, true);
                }
            }

            if (!issueFoundThisBlock && !issueFoundNextBlock && !blockLog.IsAutomated && AllocateBlocks)
            {
                await webClient.AllocateBlock(BNLNextBlock.BlockCheckedSystemName, blockLog.DCCiD);
                if (usingMQTT)
                    MQTTMessages.Enqueue(new MQTTMessage() { Topic = BlockAllocateTopic + "/" + BNLNextBlock.BlockCheckedSystemName, Payload = BNLNextBlock.BlockChecked, Retain = false });
                //await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + BNLNextBlock.BlockCheckedSystemName, BNLNextBlock.BlockChecked, false);
                if (!issueFoundTwoBlocks && !blockLog.IsAutomated && AllocateBlocks)
                {
                    await webClient.AllocateBlock(BNLTwoBlocks.BlockCheckedSystemName, blockLog.DCCiD);
                    if (usingMQTT)
                        MQTTMessages.Enqueue(new MQTTMessage() { Topic = BlockAllocateTopic + "/" + BNLTwoBlocks.BlockCheckedSystemName, Payload = BNLTwoBlocks.BlockChecked, Retain = false });
                    //await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + BNLTwoBlocks.BlockCheckedSystemName, BNLTwoBlocks.BlockChecked, false);
                }
            }

            blockLog.LastUpdated = DateTime.Now;
            blockLog.ProcessingNewBlock = false;

            if (blockLog.AllocatedBlocks.Contains(block.data.userName))
                blockLog.AllocatedBlocks.Remove(block.data.userName);
            //Log.Remove(existingLog);
            if (handlingNewTrain)
                Log.Add(blockLog);
            return (true, "Success");
        }

        private void UpdateTrainDetailsFromRoster(ref BlockJourneyLog blockLog)
        {

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
            if (breadcrumbStart != "") bnl.Breadcrumb += breadcrumbStart + ";";
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
                    var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItem, ts.Ident, "");
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
                var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItem, a.Ident, "");
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
            else if (LayoutItem.Substring(0, 2) == "SL")
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

                if (slip.Blockname != currentBlock)
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

            foreach (var alert in alerts.OrderBy(o => o.Severity).ToList())
            {
                try
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
                        lbOutput.Items.Add("Alert deactivated - no issue or BNL found - 1982 - " + alert.LikelyIssue);
                    }

                    if (!alertWasFromADifferentPath)
                    {
                        alert.BNL = checkAlert;
                    }

                    var checkAlertLiveBlock = await webClient.GetBlock(alert.BNL.BlockChecked);
                    string currentAlertReason = string.Empty;

                    var alertStillActive = false;
                    if (!string.IsNullOrEmpty(checkAlert.LikelyIssue))
                    {
                        currentAlertReason = checkAlert.LikelyIssue;
                        alertStillActive = true;
                    }
                    if (checkAlertLiveBlock.data.state == 2)
                    {
                        if (checkAlertLiveBlock.data.value != null && !string.IsNullOrEmpty(checkAlertLiveBlock.data.value.data.userName))
                        {
                            if (checkAlertLiveBlock.data.value.data.userName != log.DCCiD)
                            {
                                if (!currentAlertReason.Contains("occupied"))
                                    currentAlertReason += "; " + checkAlertLiveBlock.data.userName + " occupied by " + checkAlertLiveBlock.data.value.data.userName;
                                alertStillActive = true;
                            }
                        }
                        else
                        {
                            if (!currentAlertReason.Contains("occupied"))
                                currentAlertReason += "; " + checkAlertLiveBlock.data.userName + " occupied";
                            alertStillActive = true;
                        }
                    }

                    if (TrackAllocation && checkAlertLiveBlock.data.value != null && log != null && checkAlertLiveBlock.data.value.data.userName != log.DCCiD)
                    {
                        if (!currentAlertReason.Contains("allocated"))
                            currentAlertReason += "; " + checkAlertLiveBlock.data.userName + " allocated to " + checkAlertLiveBlock.data.value.data.userName;
                        alertStillActive = true;
                    }

                    if (!string.IsNullOrEmpty(log.CurrentBlockBNL.LikelyIssue) && !currentAlertReason.Contains(log.CurrentBlockBNL.LikelyIssue))
                    {
                        currentAlertReason += "; " + log.CurrentBlockBNL.LikelyIssue;
                        alertStillActive = true;
                    }
                    if (!string.IsNullOrEmpty(log.NextBlockBNL.LikelyIssue) && !currentAlertReason.Contains(log.NextBlockBNL.LikelyIssue))
                    {
                        currentAlertReason += "; " + log.NextBlockBNL.LikelyIssue;
                        alertStillActive = true;
                    }
                    if (!string.IsNullOrEmpty(log.TwoBlocksBNL.LikelyIssue) && !currentAlertReason.Contains(log.TwoBlocksBNL.LikelyIssue))
                    {
                        currentAlertReason += "; " + log.TwoBlocksBNL.LikelyIssue;
                        alertStillActive = true;
                    }

                    if (!alertStillActive && !alert.Deactivated)
                    {
                        lbOutput.Items.Add("2282 deactivate alert - no longer active - " + log.DCCiD + " - " + alert.LikelyIssue);
                        DeactivateAlert(alert, true);
                        continue;
                    }

                    alert.LikelyIssue = currentAlertReason;

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
                catch (Exception ex)
                {
                    //lbOutput.Items.Add("Alert processing exception "+ex.Message);
                    alert.Deactivated = true;
                }
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
            if (usingMQTT)
                MQTTMessages.Enqueue(new MQTTMessage { Topic = topic, Payload = alert.Severity.ToString(), Retain = false });
            //await MQTTClient.SendMQTTMessage(MQTTServer, topic, alert.Severity.ToString(), false);
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
            if (usingMQTT)
                MQTTMessages.Enqueue(new MQTTMessage { Topic = topic, Payload = "Proceed", Retain = false });            
            //await MQTTClient.SendMQTTMessage(MQTTServer, topic, "Proceed", false);
        }

        private void DisplayAlert(Alert alert)
        {
            lblBlockWarning.Text = alert.BlockUserName;
            lblTrainName.Text = alert.TrainName + " (" + alert.TrainId + ")";
            lblBlockWarning.ForeColor = Color.White;
            lblLikelyIssue.Text = alert.LikelyIssue;
            lblLikelyIssue.ForeColor = Color.White;
            lblTrainName.ForeColor = Color.White;
            lblBlockContainingDanger.ForeColor = Color.White;
            lblBlockContainingDanger.Text = "In " + alert.BNL.BlockChecked;
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
                item.Text = alert.TrainName + " - " + alert.BlockUserName + " - " + alert.LikelyIssue + "- in " + alert.BNL.BlockChecked;
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
                foreach (var dob2r in toRemove)
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
            var trainName = (string)ddlTrainSelector.Text;
            TerminateTrain(trainName);
        }

        private async void TerminateTrain(string trainName)
        {
            var log = Log.FirstOrDefault(f => f.Name == trainName);
            if (log == null) return;

            var heldBlocks = log.AutomatedBlockList.Where(w => w.SequenceState == JourneySequenceState.EnteredNextBlock || w.SequenceState == JourneySequenceState.Active);
            if (log.HasSpeedProfile && log.TrainLengthMM > 0)
            {
                foreach (var hb in heldBlocks)
                {
                    if (usingMQTT)
                        MQTTMessages.Enqueue(new MQTTMessage { Topic = SensorHoldTopic + "/" + hb.OccupationSensorSystemName, Payload = "0", Retain = false });
                    //await MQTTClient.SendMQTTMessage(MQTTServer, SensorHoldTopic + "/" + hb.OccupationSensorSystemName, "0", false);
                }
            }

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
                    var blockState = allBlocks.FirstOrDefault(f => f.data.userName == blockToUnallocate);
                    if (blockState != null && blockState.data != null)
                    {
                        if (blockState.data.state != 2)
                        {
                            await webClient.AllocateBlock(blockToUnallocate, "");
                            var sysName = config.GetBlockByUserName(blockToUnallocate);
                            if (usingMQTT)
                                MQTTMessages.Enqueue(new MQTTMessage { Topic = BlockReleaseTopic + "/" + sysName.userName, Payload = sysName.userName, Retain = false });
                            //await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                        }
                    }
                }
            }
            log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Cancelled;
            Log.Remove(log);

            var topic = CabSignalTopic + log.DCCiD;
            //lbOutput.Items.Add("MQtt " + topic + " - Inactive");
            if (usingMQTT)
                MQTTMessages.Enqueue(new MQTTMessage { Topic = topic, Payload = "Inactive", Retain = false });
            //await MQTTClient.SendMQTTMessage(MQTTServer, topic, "Inactive", false);

            var mem = await webClient.GetMemory(memoryAllocatedManualTrainsName);
            if (mem != null)
            {
                var idList = mem.data.value.Split(';').ToList();
                var instances = idList.Where(f => f == log.DCCiD).ToList();
                foreach (var instance in instances)
                {
                    idList.Remove(instance);
                }

                var updateString = "";
                foreach (var id in idList)
                {
                    if (!string.IsNullOrEmpty(id))
                        updateString += id + ";";
                }
                await webClient.UpdateMemory(memoryAllocatedManualTrainsName, updateString);
            }

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
                    if (usingMQTT)
                        MQTTMessages.Enqueue(new MQTTMessage { Topic = BlockReleaseTopic + "/" + sysName.userName, Payload = sysName.userName, Retain = false });
                    //await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                }
            }
        }

        private void btnRosterTest_Click(object sender, EventArgs e)
        {
            var roster = new RosterReader(RosterPath);
            var r = roster.LocoList;

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
                tbTrainDCCID.Text = log.DCCiD;
                tbTrainName.Text = log.Name;
                tbPrevDCCID.Text = log.DCCiD;
            }
        }

        private decimal GetMMSFromSpeedStep(Locomotive fullInfo, int speedStep, string dccId, TrainDirection dir)
        {
            var re = wt.Roster.FirstOrDefault(f => f.ID == dccId);
            var rosterIndex = wt.Roster.IndexOf(re);
            var mmPerSecond = 0.0M;

            //get relative position of new speed step
            //var rosterCfG = new RosterReader(RosterPath);
            //var roster = rosterCfG.LocoList;
            //var fullInfo = rosterCfG.FullRoster.FirstOrDefault(f => f.DccAddress == dccId);

            if (fullInfo != null && fullInfo.Speedprofile != null)
            {
                decimal prevForwardSpeed = 0.0M;
                decimal prevReverseSpeed = 0.0M;
                decimal prevStep = 0.0M;


                foreach (var step in fullInfo.Speedprofile.Speeds.Speed)
                {
                    var tStep = step.Step;
                    var speed = step.Forward;
                    decimal dForward = 0.0M;
                    decimal dReverse = 0.0M;
                    decimal dStep = 0.0M;

                    bool fSuccess = decimal.TryParse(step.Forward, out dForward);
                    bool rSuccess = decimal.TryParse(step.Reverse, out dReverse);
                    bool dSuccess = decimal.TryParse(step.Step, out dStep);
                    var asPerc1 = dStep / 1000;
                    var beforeRound = asPerc1 * 128;
                    int dSS = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
                    var actualSpeedStep = dSS;

                    if (fSuccess && rSuccess)
                    {
                        if ((speedStep < actualSpeedStep && speedStep >= prevStep))
                        {
                            var percent = GetRelativeSpeedStepPosition(speedStep, prevStep, actualSpeedStep);
                            if (dir == TrainDirection.Forward)
                            {
                                mmPerSecond = GetRelativeSpeedMM(percent, dForward, prevForwardSpeed);
                            }
                            else
                            {
                                mmPerSecond = GetRelativeSpeedMM(percent, dReverse, prevReverseSpeed);
                            }
                            break;
                        }
                    }
                    prevStep = actualSpeedStep;
                    prevReverseSpeed = dReverse;
                    prevForwardSpeed = dForward;
                }
            }
            return mmPerSecond;
        }
        private decimal GetRelativeSpeedMM(decimal percent, decimal highSpeed, decimal lowSpeed)
        {
            //work out percentage position between prevStep and Step
            decimal pos = 0.0M;

            var scale = highSpeed - lowSpeed;
            if (scale == 0)
                pos = scale;
            else
                pos = (percent / 100) * scale;

            //then add the pos to the base speed
            var requiredSpeedMM = lowSpeed + pos;
            return requiredSpeedMM;
        }

        private decimal GetRelativeSpeedStepPosition(int speedStep, decimal prevStep, decimal thisStep)
        {
            decimal a = speedStep - prevStep;
            decimal b = thisStep - prevStep;
            decimal frac = 0.0M;
            if (b == 0)
            {
                frac = 100;
            }
            else
                frac = (a / b) * 100;

            return frac;

        }

        private async Task<LiveJourneyLog> UpdateTrainNameAndIDInLog(LiveJourneyLog log, string newTrainName, string prevDCCID, string newDCCID)
        {
            if (log == null) return null;

            log.Name = newTrainName;
            log.DCCiD = newDCCID;

            var rosterCfG = new RosterReader(RosterPath);
            var roster = rosterCfG.LocoList;
            var fullInfo = rosterCfG.FullRoster.FirstOrDefault(f => f.DccAddress == newDCCID);

            if (fullInfo != null)
            {
                log.TrainMotionCfg = GetTrainMotionConfig(fullInfo, log.Name, log.DCCiD);
                log.fullRosterInfo = fullInfo;
                var trainLength = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerTrainLengthMM");
                int trainLengthMM = 0;

                if (trainLength != null)
                {
                    int.TryParse(trainLength.Value, out trainLengthMM);
                }

                log.TrainLengthMM = trainLengthMM;
                lbOutput.Items.Add("Train length " + log.TrainLengthMM.ToString());
                if (fullInfo.Speedprofile != null && fullInfo.Speedprofile.Speeds.Speed.Count > 0)
                {
                    log.HasSpeedProfile = true;
                    lbOutput.Items.Add("Speed profile found - steps " + fullInfo.Speedprofile.Speeds.Speed.Count.ToString());
                }
            }

            var currentMem = await webClient.GetMemory(memoryAllocatedManualTrainsName);
            if (currentMem != null)
            {
                var idList = currentMem.data.value.Split(';').ToList();
                var instances = idList.Where(f => f == prevDCCID).ToList();
                foreach (var instance in instances)
                {
                    idList.Remove(instance);
                }

                idList.Add(newDCCID);
                var updateString = "";
                foreach (var id in idList)
                {
                    if (!string.IsNullOrEmpty(id))
                        updateString += id + ";";
                }

                await webClient.UpdateMemory(memoryAllocatedManualTrainsName, updateString);
            }
            else
            {
                var updateVal = log.DCCiD + ";";
                await webClient.UpdateMemory(memoryAllocatedManualTrainsName, updateVal);
            }

            var topic = CabSignalTopic + prevDCCID;
            //lbOutput.Items.Add("MQtt " + topic + " - Inactive");
            if (usingMQTT)
                MQTTMessages.Enqueue(new MQTTMessage { Topic = topic, Payload = "Inactive", Retain = false });
            //await MQTTClient.SendMQTTMessage(MQTTServer, topic, "Inactive", false);


            ddlTrainSelector.Items.Clear();
            ddlTrainSelector.Text = log.Name;
            ddlTrainSelector.SelectedIndex = -1;
            foreach (var remainingLog in Log)
            {
                ddlTrainSelector.Items.Add(remainingLog.Name);
            }

            //get current block
            var nbConfig = config.GetBlockByUserName(log.CurrentBlock);
            if (usingMQTT)
                MQTTMessages.Enqueue(new MQTTMessage { Topic = BlockAllocateTopic + "/" + nbConfig.userName, Payload = nbConfig.userName, Retain = false });
            //await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + nbConfig.userName, nbConfig.userName, false);
            await webClient.AllocateBlock(nbConfig.userName, log.DCCiD);

            //get assigned blocks
            foreach (var block in log.AllocatedBlocks)
            {
                var bConfig = config.GetBlockByUserName(block);
                if (usingMQTT)
                    MQTTMessages.Enqueue(new MQTTMessage { Topic = BlockAllocateTopic + "/" + bConfig.userName, Payload = bConfig.userName, Retain = false });
                //await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + bConfig.userName, bConfig.userName, false);
                await webClient.AllocateBlock(bConfig.userName, log.DCCiD);
            }

            //any active alerts for the train
            var activeAlerts = alerts.Where(f => f.TrainId == prevDCCID && !f.Deactivated).ToList();
            foreach (var alert in activeAlerts)
            {
                alert.TrainId = log.DCCiD;
                alert.TrainName = log.Name;
            }

            return log;
        }

        private async void btnUpdateTrainIDAndName_Click(object sender, EventArgs e)
        {
            var trainName = tbTrainName.Text;
            var newDCCID = tbTrainDCCID.Text;
            var prevDCCID = tbPrevDCCID.Text;

            if (string.IsNullOrEmpty(trainName) || string.IsNullOrEmpty(prevDCCID) || string.IsNullOrEmpty(newDCCID)) return;
            var log = Log.FirstOrDefault(f => f.DCCiD == prevDCCID);
            if (log != null)
            log = await UpdateTrainNameAndIDInLog(log,trainName, prevDCCID, newDCCID);
            var test = Log;
 
        }

        private async void ProcessMQTTMessageQueue()
        {
            if (DateTime.Now - LastMQTTMessageProcessed < TimeSpan.FromMilliseconds(100)) return;
            while (MQTTMessages.Count > 0)
            {
                var message = MQTTMessages.Dequeue();
                try
                {
                    await MQTTClient.SendMQTTMessage(MQTTServer, message.Topic, message.Payload, message.Retain);
                }
                catch (Exception ex)
                {
                    lbOutput.Items.Add("MQTT send exception " + ex.Message);
                }
            }
        }
    }
}
