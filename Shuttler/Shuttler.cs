using JMRIReader;
using JMRIReader.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiThrottleClient;
using WiThrottleClient.Classes;
using static JMRIReader.Classes.Enums;

namespace Shuttler
{
    public partial class Shuttler : Form
    {
        private string _JMRIServerIP;
        private int _WiThrottlePort;
        private string _cfgFilePath;
        private string RosterPath;
        private string DispatcherPath;
        private ConfigReader config;
        private JSONReader webClient;
        private RosterReader rosterConfig;
        private List<BlockRootObject> _startBlocks;
        private List<transit> _transits;
        private int _sectionsAhead = 3;
        private List<BlockRootObject> _allBlocks;
        private List<LiveJourneyLog> _logs;
        private string MQTTServer;
        private string BlockAllocateTopic;
        private string BlockReleaseTopic;
        private int DefaultCautionMMS;
        private int DefaultCrawlMMS;
        private int DefaultFullSpeedMMS;
        private int cautiomBlockPercentToBeginRampDown;
        private int dangerBlockPercentToBeginRampDown;
        private int shortBlockThresholdMM;
        private int shortTrainThresholdMM = 1200;
        private string memoryAllocatedTrainsName;
        private string SensorHoldTopic;
        private DateTime LastTimeYardWasCheckedForShuffle;
        private StationAutomationManagement sam;
        private const string ACSAYardTransit = "SA AC Yard Exit to AC Platform";
        private const string CWSAYardTransit = "SA CW Yard Exit to CW Platform";
        private const string CWSAYard5Transit = "SA CW Yard 5 to CW Platform";
        private const string ACSAFreightTransit = "SA AC Freight run";
        private const string CWSAYardFreightTransit = "SA CW Yard Exit Freight run";
        private const string CWSAYard5FreightTransit = "SA CW Yard 5 Freight run";
        private const int SensorHoldBufferPercent = 35;
        private string LastLogLine = "";
        private DateTime LastRogueBlockValueCheck = DateTime.Now;

        List<ViableRouteList> ViableRoutes = new List<ViableRouteList>();
        int routeIndex = -1;
        private List<BlockToDecorate> blocksToDecorate = new List<BlockToDecorate>();

        WiThrottle c;

        private bool _isRunning;
        public Shuttler()
        {
            InitializeComponent();
            var cfgFilePath = ConfigurationManager.AppSettings["ConfigFilePath"];
            if (cfgFilePath != null)
            {
                _cfgFilePath = cfgFilePath.ToString();
            }

            var rosterFilePath = ConfigurationManager.AppSettings["RosterFilePath"];
            if (rosterFilePath != null)
            {
                RosterPath = rosterFilePath.ToString();
            }

            var dispatcherFolderPath = ConfigurationManager.AppSettings["DispatchesFolder"];
            if (dispatcherFolderPath != null)
            {
                DispatcherPath = dispatcherFolderPath.ToString();
            }

            var cfgWebServerIP = ConfigurationManager.AppSettings["JMRIServerIP"];
            if (cfgWebServerIP != null)
            {
                _JMRIServerIP = cfgWebServerIP.ToString();
            }

            var cfgWiThrottleServerPort = ConfigurationManager.AppSettings["WiTHrottlePort"];
            if (cfgWiThrottleServerPort != null)
            {
                var sPort = cfgWiThrottleServerPort.ToString();
                _WiThrottlePort = int.Parse(sPort);
            }

            var cfgMQTTServer = ConfigurationManager.AppSettings["MQTTServerIP"];
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

            var cfgSensorHoldTopic = ConfigurationManager.AppSettings["SensorHoldTopic"];
            if (cfgSensorHoldTopic != null)
            {
                SensorHoldTopic = cfgSensorHoldTopic;
            }

            var cfgMemName = ConfigurationManager.AppSettings["MemoryAllocatedTrainsName"];
            if (cfgMemName != null)
            {
                memoryAllocatedTrainsName = cfgMemName.ToString();
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

            lbRunningTransits.ValueMember = "Value";
            lbRunningTransits.DisplayMember = "Name";
            cautiomBlockPercentToBeginRampDown = 50;
            dangerBlockPercentToBeginRampDown = 60;
            shortBlockThresholdMM = 320;

            pbBlockProgress.Maximum = 100;
            pbBlockProgress.Step = 1;
            pbBlockProgress.Value = 0;
        }

        private async void InitialiseClients()
        {
            if (lbRoster.Items.Count == 0 && c.Roster.Count > 0)
            {
                foreach (var t in c.Roster)
                {
                    lbRoster.Items.Add(new
                    {
                        Name = t.Name + " (" + t.ID + ")",
                        DCCID = t.ID
                    });
                }
            }
            if (webClient == null && c != null && c.WebServerPort > -1)
            {
                var serverAddress = "http://" + _JMRIServerIP + ":" + c.WebServerPort.ToString();
                webClient = new JSONReader(serverAddress);
                await webClient.UpdateMemory(memoryAllocatedTrainsName, "");
                LoadStartBlocks();
                _allBlocks = await webClient.GetBlocks();
            }
        }

        private async void RunShuttles()
        {
            c = new WiThrottle(_JMRIServerIP, _WiThrottlePort, "Shuttler");
            lbRoster.Items.Clear();
            lbRoster.DisplayMember = "Name";
            lbRoster.ValueMember = "DCCID";

            while (_isRunning)
            {
                InitialiseClients();
                await c.CheckForMessages();
                await Task.Delay(100);
                var newBlockStates = await CheckForNewActiveBlocks();

                CheckRunningTrains(newBlockStates);
                CalculateSpeedForTrains();
                SetTrainSpeeds();
                await Task.Delay(100);
                ProcessBlocksToDecorate();
                await ManageExitedBlockHolds();

                if ((DateTime.Now - LastTimeYardWasCheckedForShuffle).TotalSeconds > 5)
                {
                    ManageYardLines(newBlockStates);
                    LastTimeYardWasCheckedForShuffle = DateTime.Now;
                }

                UpdateLogPanel();
                CleanUpListBoxes();
                await Task.Delay(100);

                if (sam != null && (sam.StationManagementACRunning || sam.StationManagementCWRunning))
                {
                    ManageStationAutomation(newBlockStates);
                }

                if ((DateTime.Now - LastRogueBlockValueCheck).TotalSeconds > 10)
                {
                    PurgeLateBlockValues(newBlockStates);
                }

                _allBlocks = newBlockStates;
                await Task.Delay(100);
            }
        }

        private async Task<List<BlockRootObject>> CheckForNewActiveBlocks()
        {
            if (webClient == null || c == null || _allBlocks == null)
                return null;

            var newBlockStates = await webClient.GetBlocks();
            var newActiveBlocks = newBlockStates.Where(w => w.data.state == 2).ToList();
            var oldActiveBlocks = _allBlocks.Where(w => w.data.state == 2).ToList();

            var activeBlocks = oldActiveBlocks.Union(newActiveBlocks).ToList();
            var newActiveThisTimeBlocks = newActiveBlocks.Where(p => !oldActiveBlocks.Any(p2 => p2.data.name == p.data.name)).ToList();

            var goneInactiveBlocks = oldActiveBlocks.Where(x => !newActiveBlocks.Select(i => i.data.name).Contains(x.data.name));

            foreach (var gib in goneInactiveBlocks.ToList())
            {
                if (gib == null) continue;
                if (gib.data.value == null) continue;
                var relatedLog = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete). FirstOrDefault(f => f.DCCiD == gib.data.value.data.userName);
                if (relatedLog == null) continue;
                var sequenceBlock = relatedLog.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == gib.data.name && f.SequenceState == JourneySequenceState.Active);
                if (sequenceBlock != null)
                {
                    //sequenceBlock.SequenceState = JourneySequenceState.Traversed;
                    //WriteToLog("Block " + sequenceBlock.BlockUserName + " exited");
                    var posInSequence = relatedLog.AutomatedBlockList.IndexOf(sequenceBlock);
                    if (posInSequence > -1)
                    {
                        var affectedBlock = relatedLog.AutomatedBlockList.ElementAtOrDefault(posInSequence + 1);
                        if (affectedBlock != null)
                        {
                            WriteToLog("GIB " + gib.data.userName + " ID " + relatedLog.DCCiD + " so set " + affectedBlock.BlockUserName + " to previous block exited");
                            affectedBlock.PreviousBlockExited = true;
                        }
                    }
                }
            }

            //just in case 2 blocks go active very quickly alongside each other and they're picked up by the same query - try to control the order they get processed in
            if (newActiveThisTimeBlocks.Count > 1)
            {
                foreach (var nab in newActiveThisTimeBlocks)
                {
                    if (_logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete).Any(a => a.NextBlock == nab.data.userName) || newActiveThisTimeBlocks.Count == 1)
                    {
                        nab.ProcessingOrder = 1;
                    }
                    else
                    {
                        nab.ProcessingOrder = 2;
                    }
                }
            }

            foreach (var nab in newActiveThisTimeBlocks.OrderBy( o => o.ProcessingOrder))
            {
                try
                {
                    var nextBlockName = "";
                    var allocatedTo = "";
                    var newBlockValue = "";
                    bool matchingLogFound = false;
                    var previousBlockState = _allBlocks.FirstOrDefault(f => f.data.name == nab.data.name);
                    if (previousBlockState != null && previousBlockState.data.value != null)
                    {
                        allocatedTo = previousBlockState.data.value.data.userName;
                        newBlockValue = previousBlockState.data.value.data.userName;
                    }

                    WriteToLog("NAB " + nab.data.userName+" processing order "+nab.ProcessingOrder.ToString());
                    var logsByNextBlock = _logs.Where(w =>w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete && w.NextBlock == nab.data.userName).ToList();
                    var existingLog = new LiveJourneyLog();
                    existingLog.DCCiD = "XX";

                    if (logsByNextBlock.Count == 1)
                    {
                        var logByNextBlock = logsByNextBlock.First();
                        if (logByNextBlock.DCCiD == newBlockValue)
                        {
                            WriteToLog("Matched NAB to log via log next block, matching ID " + newBlockValue);
                            matchingLogFound = true;
                            existingLog = logByNextBlock;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(newBlockValue))
                            {
                                WriteToLog("New value empty - could be a late entry, expecting "+logByNextBlock.DCCiD);
                            }
                            else if (newBlockValue != logByNextBlock.DCCiD)
                            {
                                WriteToLog("Possibly incorrect block value - expecting " + logByNextBlock.DCCiD + " but got " + newBlockValue);
                            }

                            var includedInAllocation = logByNextBlock.AllocatedBlocks.Any(a => a == newBlockValue);
                            WriteToLog("NAB Value mismatch when only one log matched via next block - log ID " + logByNextBlock.DCCiD + " but block value " + newBlockValue+" - included in allocation = "+includedInAllocation.ToString());
                            if (includedInAllocation)
                            {
                                //probably need to correct a block value here

                                var responseBlock = await webClient.AllocateBlock(nab.data.name, existingLog.DCCiD, true);

                            }
                        }                        
                    }

                    if (!matchingLogFound && logsByNextBlock.Count > 1)
                    {
                        var IDs = "";
                        var idOfMovingTrain = "";
                        LiveJourneyLog logToProcess = new LiveJourneyLog();
                        var foundASingleMatchingLog = false;

                        foreach (var lbnb in logsByNextBlock)
                        {
                            IDs += lbnb.DCCiD + "; ";
                        }

                        WriteToLog("More than one matching log by next block found for " + nab.data.userName + " - " + IDs);

                        var logsWithMovingTrains = logsByNextBlock.Where(w => w.TrainMotionCfg.CurrentSpeedStep > 0).ToList();
                        WriteToLog("Number of found logs with moving train: " + logsWithMovingTrains.Count.ToString());
                        if (logsWithMovingTrains.Count == 1)
                        {
                            var logWithMovingTrain = logsWithMovingTrains.First();
                            logToProcess = logWithMovingTrain;
                            WriteToLog("Only " + logWithMovingTrain.DCCiD + " had a moving train though");

                            foundASingleMatchingLog = true;
                        }

                        //try to recover from a short?
                        if (!foundASingleMatchingLog)
                        {
                            var logsWithThisBlockAsCurrerntBlock = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete && w.CurrentBlock == nab.data.userName);
                            if (logsWithThisBlockAsCurrerntBlock.Count() == 1)
                            {
                                var log = logsWithThisBlockAsCurrerntBlock.First();
                                WriteToLog("Recoved from a short here? Found " + log.DCCiD + " with this block as current block - " + nab.data.userName);
                                existingLog = log;
                                if (string.IsNullOrWhiteSpace(newBlockValue))
                                {
                                    WriteToLog("New value empty - could be a late entry, expecting " + log.DCCiD);
                                }
                                else if (newBlockValue != log.DCCiD)
                                {
                                    WriteToLog("Possibly incorrect block value - expecting " + log.DCCiD + " but got " + newBlockValue);
                                }

                                var includedInAllocation = log.AllocatedBlocks.Any(a => a == newBlockValue);
                                WriteToLog("NAB Value mismatch when only one log matched via current block - log ID " + log.DCCiD + " but block value " + newBlockValue + " - included in allocation = " + includedInAllocation.ToString());
                                if (includedInAllocation)
                                {
                                    //probably need to correct a block value here

                                    var responseBlock = await webClient.AllocateBlock(nab.data.name, existingLog.DCCiD, true);

                                }
                            }
                        }

                        if (!foundASingleMatchingLog)
                        {
                            var logsWithThisBlockAllocated = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete && w.AllocatedBlocks.Contains(nab.data.userName)).ToList();
                            if (logsWithThisBlockAllocated.Any())
                            {
                                WriteToLog("Number of found logs with this block allocated: " + logsWithThisBlockAllocated.Count.ToString());
                                if (logsWithThisBlockAllocated.Count == 1)
                                {
                                    logToProcess = logsWithThisBlockAllocated.First();
                                    foundASingleMatchingLog = true;
                                }
                                else
                                {
                                    var logsWithThisBlockAllocatedAndAMovingTrain = logsWithThisBlockAllocated.Where(w => w.TrainMotionCfg.CurrentSpeedStep > 0).ToList();
                                    WriteToLog("Number of found logs with this block allocated and are moving: " + logsWithThisBlockAllocatedAndAMovingTrain.Count.ToString());
                                    if (logsWithThisBlockAllocatedAndAMovingTrain.Count == 1)
                                    {
                                        var logWithAllocationAndMovingTrain = logsWithThisBlockAllocatedAndAMovingTrain.First();
                                        logToProcess = logWithAllocationAndMovingTrain;
                                        WriteToLog("Only " + logToProcess.DCCiD + " had a moving train though");

                                        foundASingleMatchingLog = true;
                                    }
                                }
                            }
                        }

                        if (foundASingleMatchingLog)
                        {
                            existingLog = logToProcess;
                            matchingLogFound = true;
                            if (logToProcess.DCCiD == newBlockValue)
                            {
                                WriteToLog("Matched NAB to log via allocation or speed, matching ID " + newBlockValue);
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(newBlockValue))
                                {
                                    WriteToLog("New block value empty - could be a late entry, expecting " + logToProcess.DCCiD);
                                }
                                else if (newBlockValue != logToProcess.DCCiD)
                                {
                                    WriteToLog("Possibly incorrect block value - expecting " + logToProcess.DCCiD + " but got " + newBlockValue);
                                }

                                var includedInAllocation = logToProcess.AllocatedBlocks.Any(a => a == newBlockValue);
                                WriteToLog("NAB Value mismatch when only one log matched via next block - log ID " + logToProcess.DCCiD + " but block value " + newBlockValue + " - included in allocation = " + includedInAllocation.ToString());
                                if (includedInAllocation)
                                {
                                    //probably need to correct a block value here
                                    var responseBlock = await webClient.AllocateBlock(nab.data.name, existingLog.DCCiD, true);
                                }
                            }
                        }
                    }

                    if (!matchingLogFound)
                    {
                        var logsWithThisBlockAllocated = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete && w.AllocatedBlocks != null && w.AllocatedBlocks.Contains(nab.data.userName)).ToList();
                        WriteToLog("No matching log yet, so checking for abnormal order - logs with this block allocated = " + logsWithThisBlockAllocated.Count.ToString());
                        if (logsWithThisBlockAllocated.Count == 1)
                        {
                            var potLog = logsWithThisBlockAllocated.First();
                            var indexOfBooking = potLog.AllocatedBlocks.IndexOf(newBlockValue);
                        }
                        WriteToLog("Log lookup failure for block " + nab.data.userName);
                        continue;
                    }
                    var activeSection = existingLog.AutomatedSectionList.ElementAtOrDefault(existingLog.AutomatedCurrentSectionIndex);

                    //if new block is in current section
                    var activeBlock = activeSection.Blocks.FirstOrDefault(f => f.userName == nab.data.userName);
                    if (activeBlock == null)
                    {
                        //entered new active section
                        activeSection = existingLog.AutomatedSectionList.ElementAtOrDefault(existingLog.AutomatedCurrentSectionIndex + 1);
                        if (activeSection != null)
                        {
                            existingLog.AutomatedCurrentSectionIndex++;
                            activeSection.IsTraversed = true;
                            activeBlock = activeSection.Blocks.FirstOrDefault(f => f.userName == nab.data.userName);
                            WriteToLog("New active section " + activeSection.SectionkUserName + " - block " + activeBlock.userName + " section index now at " + existingLog.AutomatedCurrentSectionIndex.ToString() + " storage " + activeSection.IsStorage.ToString());
                        }
                    }

                    if (activeBlock != null)
                    {
                        //should be 0 but hey ho
                        var abIndex = activeSection.Blocks.IndexOf(activeBlock);
                        if (abIndex > -1)
                        {
                            if (abIndex + 1 < activeSection.Blocks.Count)
                            {
                                var nextBlock = activeSection.Blocks.ElementAtOrDefault(abIndex + 1);
                                if (nextBlock != null)
                                {
                                    //nextBlockName = nextBlock.userName;
                                    WriteToLog("New block " + nab.data.userName + " comes from existing section " + activeSection.SectionkUserName);
                                }
                                else
                                {
                                    WriteToLog("Next block not found in current section check " + existingLog.DCCiD + " " + activeSection.SectionkUserName);
                                }
                            }
                            else
                            {
                                //get from next section
                                var nextSection = existingLog.AutomatedSectionList.ElementAtOrDefault(existingLog.AutomatedCurrentSectionIndex + 1);
                                if (nextSection != null)
                                {
                                    var nextBlock = nextSection.Blocks.FirstOrDefault();
                                    if (nextBlock != null)
                                    {
                                        //nextBlockName = nextBlock.userName;
                                        WriteToLog("New block " + nab.data.userName + " comes from new section " + nextSection.SectionkUserName);
                                    }
                                    else
                                    {
                                        WriteToLog("Next block not found in next section check " + existingLog.DCCiD + " " + nextSection.SectionkUserName);
                                    }
                                }
                            }
                        }

                        existingLog.AutomatedTrainRunningSpeed = activeBlock.BlockSpeed;
                        existingLog.AutomatedTrainSpeedReason = activeBlock.AutomatedSpeedReason;
                    }
                    else
                    {
                        WriteToLog("Active section block not found for " + nab.data.userName + " - ID " + existingLog.DCCiD);
                    }

                    var logBlock = existingLog.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == nab.data.name && f.SectionSequenceId == activeSection.Sequence);

                    if (logBlock == null)
                    {
                        logBlock = existingLog.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == nab.data.name && f.SectionSequenceId == existingLog.AutomatedCurrentSectionIndex + 1);
                        if (logBlock.SequenceState != JourneySequenceState.Queued)
                        {
                            //flickering? This block has already been processed as a new block.
                            WriteToLog(nab.data.userName + " detected as new block but already processed - ignored as it's probably flickering");
                            continue;
                        }
                    }

                    if (logBlock == null)
                    {
                        //last section? shouldn't get here but just in case
                        logBlock = existingLog.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == nab.data.name && f.SequenceState == JourneySequenceState.Queued);
                    }

                    try
                    {
                        logBlock.TimeTrainEnteredBlock = DateTime.Now;
                        logBlock.mmCovered = 0;
                        logBlock.SequenceState = JourneySequenceState.Active;
                        logBlock.SpeedLog = new List<SpeedStepLog>();

                        //WriteToLog("Config block speed " + activeBlock.BlockSpeed.ToString() + " - " + activeBlock.AutomatedSpeedReason);
                        //WriteToLog("Log block speed " + logBlock.SpeedLimit.ToString());
                        if (logBlock.EarlyExitBlock)
                            WriteToLog(nab.data.userName + " is an early exit block train may stop prematurely in this block (if a stop is required) if it also has thrown turnouts; ID" + existingLog.DCCiD);

                        if (existingLog.AutomatedBlockList.ElementAt(0).SequenceState == JourneySequenceState.Queued)
                            existingLog.AutomatedBlockList.ElementAt(0).SequenceState = JourneySequenceState.Active;
                    }
                    catch (Exception ex)
                    {
                        WriteToLog("NAB Block state exception - " + ex.Message);
                    }



                    var previousLogBlock = existingLog.AutomatedBlockList.ElementAtOrDefault(existingLog.AutomatedCurrentBlockIndex);
                    existingLog.AutomatedCurrentBlockIndex = existingLog.AutomatedBlockList.IndexOf(logBlock);
                    var nextLogBlock = existingLog.AutomatedBlockList.ElementAtOrDefault(existingLog.AutomatedCurrentBlockIndex + 1);
                    if (nextLogBlock != null)
                    {
                        nextBlockName = nextLogBlock.BlockUserName;
                    }
                    else
                    {
                        nextBlockName = "";
                    }

                    //log the speed the train was going at as it entered the block - for mm covered so far calculations

                    if (previousLogBlock != null)
                    {
                        var lastSpeedLogEntry = previousLogBlock.SpeedLog.LastOrDefault();
                        if (lastSpeedLogEntry != null)
                        {
                            lastSpeedLogEntry.start = DateTime.Now;
                            logBlock.SpeedLog.Add(lastSpeedLogEntry);
                        }
                        else
                        {
                            //WriteToLog(logBlock.BlockUserName + " prev block " + previousLogBlock.BlockUserName + " last speed log entry null count " + previousLogBlock.SpeedLog.Count.ToString());
                        }

                        previousLogBlock.SequenceState = JourneySequenceState.EnteredNextBlock;
                        if (existingLog.TrainLengthMM > 0)
                        {
                            var previousLiveBlock = _allBlocks.FirstOrDefault(f => f.data.name == previousLogBlock.BlockSystemname);
                            if (previousLiveBlock != null)
                            {
                                var sensorName = previousLiveBlock.data.sensor.Substring(2);
                                WriteToLog(existingLog.DCCiD + " Sensor hold for " + sensorName);
                                await MQTTClient.SendMQTTMessage(MQTTServer, SensorHoldTopic + "/" + sensorName, "1", false);
                            }
                            else
                            {
                                WriteToLog("Sensor hold failure - couldn't find log block for " + previousLogBlock.BlockUserName);
                            }
                        }
                    }
                    else
                    {
                        WriteToLog(logBlock.BlockUserName + " previous log block null");
                    }

                    if (existingLog != null && existingLog.AllocatedBlocks != null && existingLog.AllocatedBlocks.Contains(nab.data.userName))
                    {
                        existingLog.AllocatedBlocks.Remove(nab.data.userName);
                    }

                    var newBlockBNL = new BlockNavigationLog();
                    if (existingLog.CurrentBlockBNL == null)
                    {
                        WriteToLog("Log current block BNL is null " + existingLog.DCCiD);
                        if (activeBlock != null)
                        {
                            if (activeBlock.BNL != null)
                            {
                                WriteToLog("Using active section block for hooks " + activeBlock.userName);
                                newBlockBNL = NavigateThroughBlockItems(nab.data.userName, nextBlockName, activeBlock.BNL.UsedEdgeConnector, activeBlock.BNL.UsedEdgeConnectorDirectionConnector, "");
                            }
                            else
                                WriteToLog("Active section block " + activeBlock.userName + " used but BNL is null");
                        }
                        else
                            WriteToLog("Couldn't get from active section block as it's null");
                    }
                    else
                    {
                        newBlockBNL = NavigateThroughBlockItems(nab.data.userName, nextBlockName, existingLog.CurrentBlockBNL.EdgeConnector, existingLog.CurrentBlockBNL.EdgeConnectorDirectionConnector, "");
                    }


                    //WriteToLog("BNL check next block original find " + newBlockBNL.BlockFound);
                    //var altNewBlockBNL = NavigateThroughBlockItems(nab.data.userName, nextBlockName, existingLog.CurrentBlockBNL.NextBlockEdgeConnector, existingLog.CurrentBlockBNL.EdgeConnectorDirectionConnector, "");

                    existingLog.CurrentBlock = nab.data.userName;
                    existingLog.NextBlock = newBlockBNL.BlockFound;
                    existingLog.PreviousBlockBNL = existingLog.CurrentBlockBNL;
                    existingLog.CurrentBlockBNL = newBlockBNL;

                    WriteToLog("NAB " + nab.data.userName + " next block in log " + existingLog.NextBlock + " ID " + existingLog.DCCiD + " index " + existingLog.AutomatedCurrentBlockIndex.ToString());
                }
                catch (Exception ex)
                {
                    WriteToLog("Exception processing " + nab.data.name + " - " + ex.Message);
                }
 

                //WriteToLog("New block " + nab.data.userName + " block index "+existingLog.AutomatedCurrentBlockIndex.ToString()+ " for train " + existingLog.Name + " next block " + existingLog.NextBlock 
                //    + " speed "+existingLog.AutomatedTrainRunningSpeed.ToString()+" - reason "+existingLog.AutomatedTrainSpeedReason);
            }
            return newBlockStates;

        }


        private async Task ManageExitedBlockHolds()
        {
            try
            {
                var activeLogs = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete).ToList();
                for (int l = 0; l < activeLogs.Count; l++)
                {
                    var log = activeLogs[l];
                    if (log == null)
                        continue;

                    var previousBlocksStillOccupied = log.AutomatedBlockList.Where(w => w.SequenceState == JourneySequenceState.EnteredNextBlock).ToList();
                    foreach (var pbso in previousBlocksStillOccupied)
                    {
                        var indexOfpbso = log.AutomatedBlockList.IndexOf(pbso);
                        if (indexOfpbso != -1)
                        {
                            decimal totalMMCoveredSinceExitingPBSO = 0M;
                            for (int i = indexOfpbso + 1; i <= log.AutomatedCurrentBlockIndex; i++)
                            {
                                decimal mmCoveredSoFarThisBlock = 0.0M;
                                var thisLogBlock = log.AutomatedBlockList.ElementAtOrDefault(i);
                                if (thisLogBlock != null)
                                {
                                    for (int b = 0; b < thisLogBlock.SpeedLog.Count; b++)
                                    {
                                        decimal mmCoveredAtStartOfLoop = thisLogBlock.mmCovered;
                                        var thisSpeedLog = thisLogBlock.SpeedLog.ElementAtOrDefault(b);
                                        if (thisSpeedLog != null && thisSpeedLog.SpeedStep > 0)
                                        {
                                            var dateTimeTo = DateTime.Now;
                                            if (b + 1 < thisLogBlock.SpeedLog.Count)
                                            {
                                                dateTimeTo = thisLogBlock.SpeedLog.ElementAt(b + 1).start;
                                            }

                                            var timeDiff = dateTimeTo - thisLogBlock.SpeedLog.ElementAt(b).start;
                                            mmCoveredSoFarThisBlock += thisLogBlock.SpeedLog.ElementAt(b).SpeedMMS * (decimal)timeDiff.TotalSeconds;
                                            thisLogBlock.mmCovered = mmCoveredSoFarThisBlock;
                                            totalMMCoveredSinceExitingPBSO += mmCoveredSoFarThisBlock;
                                        }
                                        else
                                        {
                                            //WriteToLog(log.DCCiD + " block " + pbso.BlockUserName + " zero SS MM covered " + totalMMCoveredSinceExitingPBSO.ToString("#.##")+" train len "+log.TrainLengthMM.ToString());
                                        }
                                    }
                                }

                            }
                            if (totalMMCoveredSinceExitingPBSO > log.TrainLengthMM)
                            {
                                var additionalPercent = (totalMMCoveredSinceExitingPBSO / 100) * SensorHoldBufferPercent;

                                //locos always move slower than their speed profiled speed when turning corners, going up hills, dragging coaches etc., so add a little buffer
                                if (totalMMCoveredSinceExitingPBSO + additionalPercent > log.TrainLengthMM)
                                {
                                    //only reliable place to get system name of occupancy sensor is API - names seem inconsistent in the config
                                    var liveBlock = _allBlocks.FirstOrDefault(f => f.data.name == pbso.BlockSystemname);
                                    if (liveBlock != null)
                                    {
                                        var sensorName = liveBlock.data.sensor.Substring(2);
                                        WriteToLog("Loco " + log.DCCiD + " calculated exit of block " + pbso.BlockUserName + " sensor " + sensorName + " train length " + log.TrainLengthMM.ToString() + " distance calculated " + totalMMCoveredSinceExitingPBSO.ToString());
                                        await MQTTClient.SendMQTTMessage(MQTTServer, SensorHoldTopic + "/" + sensorName, "0", false);
                                        var logToUpdate = _logs.FirstOrDefault(f => f.LogId == log.LogId && f.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete);
                                        if (logToUpdate != null)
                                        {
                                            var blockToUpdate = logToUpdate.AutomatedBlockList.ElementAtOrDefault(indexOfpbso);
                                            if (blockToUpdate != null)
                                            {
                                                logToUpdate.AutomatedBlockList.ElementAt(indexOfpbso).SequenceState = JourneySequenceState.Traversed;
                                            }
                                            else
                                            {
                                                WriteToLog(logToUpdate.DCCiD + " block " + pbso.BlockUserName + " can't find to update to traversed - current state " + pbso.SequenceState.ToString());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        WriteToLog("Sensor hold failure - couldn't get live block for " + pbso.BlockUserName);
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Exited block holds exception - " + ex.Message);
            }

            /*
            foreach (var log in _logs.ToList())
            {

            }
            */
        }
        private async void ManageYardLines(List<BlockRootObject> LiveBlocks)
        {
            if (!cbManageYard.Checked) return;
            var yardSections = config.GetYardSections();
            
            foreach (var ys in yardSections)
            {
                //override second sensors for long trains if they haven't detected a coach / truck
                //get first block and see if it contains a long train
                var previousBlockTrainId = "";
                bool longTrainInPreviousBlock = false;
                for (int i = ys.blockentry.Count() - 1; i >= 0; i--)
                {
                    var yardbBock = ys.blockentry[i];
                    var liveBlock = LiveBlocks.FirstOrDefault(f => f.data.name == yardbBock.sName);
                    if (liveBlock == null) break;
                    //Some storage sections start with the yard entrance block which isn't storage, so skip those
                    if (liveBlock.data.comment != null && !liveBlock.data.comment.Contains("Storage"))
                        continue;

                    if (longTrainInPreviousBlock)
                    {
                        if (liveBlock.data.state == 4) //unoccupied
                        {
                            var locoInMotion = _logs.Any(a => a.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete && a.DCCiD == previousBlockTrainId);
                            if (!locoInMotion)
                            {
                                WriteToLog("Yard sensor override for " + liveBlock.data.userName + " ID " + previousBlockTrainId);
                                var sensorToHack = liveBlock.data.sensor;
                                await webClient.AllocateBlock(liveBlock.data.name, previousBlockTrainId);
                                await webClient.SetSensor(sensorToHack, "2");
                            }                        
                        }

                        longTrainInPreviousBlock = false;
                        break;
                    }


                    if (liveBlock.data.state == 2 && i == ys.blockentry.Count() - 1)
                    {
                        //first block in line occupied
                        if (liveBlock.data.value != null && !string.IsNullOrEmpty(liveBlock.data.value.data.userName))
                        {
                            var rosterCfG = new RosterReader(RosterPath);
                            var fullInfo = rosterCfG.FullRoster.FirstOrDefault(f => f.DccAddress == liveBlock.data.value.data.userName);
                            if (fullInfo != null && fullInfo.Attributepairs != null)
                            {
                                var trainLength = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerTrainLengthMM");
                                if (trainLength != null)
                                {
                                    int trainLengthMM = 0;
                                    var success = int.TryParse(trainLength.Value, out trainLengthMM);
                                    if (success)
                                    {
                                        if (trainLengthMM > shortTrainThresholdMM)
                                        {
                                            longTrainInPreviousBlock = true;
                                            previousBlockTrainId = liveBlock.data.value.data.userName;
                                        }
                                    }
                                }
                            }                            
                        }
                    }
                }

                var blocksForTransitSection = new List<ViableRouteBlock>();

                for (int i = ys.blockentry.Count() - 1; i >= 0; i--)
                {
                    var yardbBock = ys.blockentry[i];
                    var liveBlock = LiveBlocks.FirstOrDefault(f => f.data.name == yardbBock.sName);
                    if (liveBlock == null) break;
                    //Some storage sections start with the yard entrance block which isn't storage, so skip those
                    if (liveBlock.data.comment != null && !liveBlock.data.comment.Contains("Storage"))
                        continue;
                    if (liveBlock.data.state == 2 && i == ys.blockentry.Count() - 1)
                    {
                        //first block in line occupied
                        break;
                    }
                    if (liveBlock.data.value != null && !string.IsNullOrEmpty(liveBlock.data.value.data.userName) && i == ys.blockentry.Count() - 1) break; //first block allocated - likely this line has already been processed and transit has started
                    if (liveBlock.data.state == 4)
                    {
                        blocksForTransitSection.Add(new ViableRouteBlock()
                        {
                            IsAvailable = true,
                            Blockname = liveBlock.data.userName,
                            Displayname = liveBlock.data.userName
                        });
                    }
                    if (liveBlock.data.state == 2 && blocksForTransitSection.Count > 0)
                    {
                        //there were empty blocks previously but now found an occupied one
                        //if it has a value and the occupant can be determined, shove it forward
                        if (liveBlock.data.value != null && !string.IsNullOrEmpty(liveBlock.data.value.data.userName))
                        {
                            var locoInMotion = _logs.Any(a => a.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete && a.DCCiD == liveBlock.data.value.data.userName);
                            if (!locoInMotion)
                            {
                                blocksForTransitSection.Add(new ViableRouteBlock()
                                {
                                    IsAvailable = true,
                                    Blockname = liveBlock.data.userName,
                                    Displayname = liveBlock.data.userName
                                });

                                blocksForTransitSection.Reverse();

                                var transit = config.BuildTransitFromBlockList(blocksForTransitSection);
                                transit.Sections.First().IsStorage = true;
                                transit.Sections.First().StorageSlotAllocated = true;
                                transit.Type = TransitType.YardShuffle;

                                //just set it as forward - the StartAutoTrain will retrieve the train's default direction from the roster and update this value
                                TrainDirection dir = TrainDirection.Forward;

                                StartAutoTrain(transit, dir, DateTime.Now);
                            }

                            break;
                        }
                    }
                }
            }
        }

        private void CleanUpListBoxes()
        {
            var completeLogs = _logs.Where(w => w.TrainMotionCfg.IsActive == false && w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete 
                && (w.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Cancelled || w.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Complete)).ToList();
 
            if (completeLogs != null && completeLogs.Count > 0)
            {
                foreach (var done in completeLogs)
                {
                    WriteToLog("Processing completion for " + done.DCCiD+" next transit "+done.NextTransit);
                    done.LastUpdated = DateTime.Now;
                    done.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.ReadyToDelete;
                    
                    if (!string.IsNullOrEmpty(done.NextTransit))
                    {
                        if (done.AutomatedCurrentBlockIndex < done.AutomatedBlockList.Count - 1)
                        {
                            WriteToLog("Not triggering new transit - " + done.NextTransit + " - detected that previous transit was cancelled");
                        }

                        else if (done.TerminatedReason == "Manually cancelled")
                        {
                            WriteToLog("Not triggering new transit - " + done.NextTransit + " - detected that previous transit was not completed (block index)");
                        }
                        else
                        {
                            var newTransit = config.GetTransit(done.NextTransit, DispatcherPath);
                            newTransit.NextTransitAdditionalDelayMS = done.NextTransitAdditionalDelayMS;
                            newTransit.Type = TransitType.Triggered;

                            var fullDelay = done.NextTransitDelayMS + done.NextTransitAdditionalDelayMS;
                            WriteToLog("Triggering new transit - " + done.NextTransit + " - " + done.NextTransitDirection.ToString() + " - delay " + fullDelay.ToString());
                            StartAutoTrain(newTransit, done.NextTransitDirection, DateTime.Now.AddMilliseconds(fullDelay));
                        }
                    }

                    var re = c.Roster.FirstOrDefault(f => f.ID == done.DCCiD);
                    var rosterIndex = c.Roster.IndexOf(re);
                    c.ReleaseThrottle(rosterIndex);
                }

                lbRunningTransits.Items.Clear();
                foreach (var log in _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete).OrderBy(o => o.TimeStarted).ToList())
                {
                    lbRunningTransits.Items.Add(new
                    {
                        Name = log.DCCiD+" "+ log.TransitName,
                        Value = log.DCCiD
                    });
                }
            }

        }

        private void CalculateSpeedForTrains()
        {
            foreach (var log in _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete).ToList())
            {
                if (log.TrainMotionCfg == null) continue;

                if (log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Scheduled) continue;

                if (!log.TrainMotionCfg.IsActive && log.TrainMotionCfg.CurrentSpeedStep > 0)
                {
                    log.TrainMotionCfg.TargetSpeedStep = 0;
                    log.TrainMotionCfg.RequiredSpeedStep = 0;
                    log.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                    log.AutomatedTrainSpeedReason = "Manually cancelled";
                    log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Cancelled;
                    log.StatusLastChanged = DateTime.Now;
                    continue;
                }

                if (log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Starting || log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Resuming)
                {
                    //Don't set the trains off straight away, let the points get set and early route settled
                    var timeSinceStarted = DateTime.Now - log.StatusLastChanged;
                    if (timeSinceStarted.TotalSeconds < 10)
                    {
                        log.TrainMotionCfg.CurrentSpeedStep = 0;
                        log.TrainMotionCfg.TargetSpeedStep = 0;
                        //WriteToLog("Start delay " + log.Name + " - " + timeSinceStarted.TotalSeconds.ToString()+" - "+log.AutomatedTrainRunningStatus.ToString());
                        continue;
                    }
                    else
                    {
                        log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Running;
                        log.StatusLastChanged = DateTime.Now;

                        var rosterEntry = c.Roster.FirstOrDefault(f => f.ID == log.DCCiD);
                        var rosterIndex = c.Roster.IndexOf(rosterEntry);

                        string mtIndex = c.GetThrottle(rosterIndex);
                        var newThrottle = new Throttle();
                        newThrottle.mtIndex = mtIndex;
                        newThrottle.RosterIndex = rosterIndex;
                        newThrottle.ID = log.DCCiD;

                        c.SetThrottleDirection(rosterIndex, ((int)log.TrainMotionCfg.TrainDirection).ToString());
                    }
                }

                if (log.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.Running)
                    continue;

                bool emergencyStopRequired = false;

                int targetSpeedRequired = 0;
                switch (log.AutomatedTrainRunningSpeed)
                {
                    case AutomatedTrainRunningSpeed.EmergencyStop:
                        targetSpeedRequired = 0;
                        emergencyStopRequired = true;
                        break;
                    case AutomatedTrainRunningSpeed.Stop:
                        targetSpeedRequired = 0;
                        break;
                    case AutomatedTrainRunningSpeed.Crawl:
                        targetSpeedRequired = log.TrainMotionCfg.TrainDirection == TrainDirection.Forward
                            ? log.TrainMotionCfg.ForwardCrawlSpeedStep : log.TrainMotionCfg.ReverseCrawlSpeedStep;
                        break;
                    case AutomatedTrainRunningSpeed.Caution:
                        targetSpeedRequired = log.TrainMotionCfg.TrainDirection == TrainDirection.Forward
                            ? log.TrainMotionCfg.ForwardCautionSpeedStep : log.TrainMotionCfg.ReverseCautionSpeedStep;
                        break;
                    default:
                        targetSpeedRequired = log.TrainMotionCfg.TrainDirection == TrainDirection.Forward
                            ? log.TrainMotionCfg.ForwardFullSpeedStep : log.TrainMotionCfg.ReverseFullSpeedStep;
                        break;                                          
                }

                if (targetSpeedRequired > log.TrainMotionCfg.CurrentSpeedStep)
                {
                    log.TrainMotionCfg.InRampDown = false;
                    log.TrainMotionCfg.InRampUp = true;
                }
                else if (targetSpeedRequired < log.TrainMotionCfg.CurrentSpeedStep && !emergencyStopRequired)
                {
                    log.TrainMotionCfg.InRampDown = true;
                    log.TrainMotionCfg.InRampUp = false;
                }


                log.TrainMotionCfg.TargetSpeedStep = targetSpeedRequired;

                int actualSpeedRequired = log.TrainMotionCfg.CurrentSpeedStep;

                if (targetSpeedRequired != log.TrainMotionCfg.CurrentSpeedStep)
                {
                    var timeSinceLastChange = DateTime.Now - log.TrainMotionCfg.RampSpeedLastSet;
                    if (log.TrainMotionCfg.InRampUp)
                    {
                        if (timeSinceLastChange.TotalMilliseconds > log.TrainMotionCfg.RampUpIntervalMS)
                        {
                            //WriteToLog(log.Name + " ramp up from " + actualSpeedRequired.ToString() +" step "+ log.TrainMotionCfg.RampUpSpeedStepIncrease.ToString());
                            actualSpeedRequired = log.TrainMotionCfg.CurrentSpeedStep + log.TrainMotionCfg.RampUpSpeedStepIncrease;
                            if (actualSpeedRequired >= targetSpeedRequired)
                            {
                                actualSpeedRequired = targetSpeedRequired;
                                log.TrainMotionCfg.InRampUp = false;
                            }
                            log.TrainMotionCfg.RampSpeedLastSet = DateTime.Now;
                        }
                        log.TrainMotionCfg.InRampDown = false;
                    }

                    else if (log.TrainMotionCfg.InRampDown)
                    {
                        if (timeSinceLastChange.TotalMilliseconds > log.TrainMotionCfg.RampDownIntervalMS)
                        {
                            actualSpeedRequired = log.TrainMotionCfg.CurrentSpeedStep - log.TrainMotionCfg.RampDownSpeedStepDecrease;
                           // WriteToLog(log.Name + " ramp down from " + actualSpeedRequired.ToString() + " step " + log.TrainMotionCfg.RampUpSpeedStepIncrease.ToString());
                            if (actualSpeedRequired < 0)
                                actualSpeedRequired = 0;
                            if (actualSpeedRequired <= targetSpeedRequired)
                            {
                                actualSpeedRequired = targetSpeedRequired;
                                log.TrainMotionCfg.InRampDown = false;
                                WriteToLog("Ramp down complete speed = "+actualSpeedRequired.ToString()+" ID "+log.DCCiD);
                            }
                            log.TrainMotionCfg.RampSpeedLastSet = DateTime.Now;
                        }
                        log.TrainMotionCfg.InRampUp = false;
                    }
                }

                log.TrainMotionCfg.TargetSpeedStep = targetSpeedRequired;
                log.TrainMotionCfg.RequiredSpeedStep = actualSpeedRequired;

                if (emergencyStopRequired && (log.TrainMotionCfg.TargetSpeedStep == 0 || log.TrainMotionCfg.RequiredSpeedStep == 0))
                {
                    log.TrainMotionCfg.TargetSpeedStep = 0;
                    log.TrainMotionCfg.RequiredSpeedStep = 0;
                    log.TrainMotionCfg.InRampDown = false;
                    log.TrainMotionCfg.InRampUp = false;
                    //WriteToLog("Emergency stop executed for " + log.Name);
                }

                if (log.TrainMotionCfg.TargetSpeedStep == 0 && log.TrainMotionCfg.RequiredSpeedStep == 0)
                {
                    if (log.AutomatedCurrentBlockIndex == log.AutomatedBlockList.Count - 1)
                    {
                        log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Complete;
                        log.StatusLastChanged = DateTime.Now;
                    }
                    else if (log.SignalAspect != SignalAspect.Proceed)
                    {
                        {
                            log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Waiting;
                            log.StatusLastChanged = DateTime.Now;
                        }
                    }

                }
            }
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

        private decimal GetRelativeSpeedStepPosition(int speedStep, decimal prevStep, decimal thisStep )
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

        private void SetTrainSpeeds()
        {
            //if current speed < target speed and not ramping up, set ramp up
            foreach (var log in _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete).ToList())
            {

                if (log.TrainMotionCfg.RequiredSpeedStep != log.TrainMotionCfg.CurrentSpeedStep)
                {
                    var re = c.Roster.FirstOrDefault(f => f.ID == log.DCCiD);
                    var rosterIndex = c.Roster.IndexOf(re);
                    c.SetThrottleSpeedStep(rosterIndex, log.TrainMotionCfg.RequiredSpeedStep);
                    log.TrainMotionCfg.CurrentSpeedStep = log.TrainMotionCfg.RequiredSpeedStep;

                    //get relative position of new speed step
                    var rosterCfG = new RosterReader(RosterPath);
                    var fullInfo = rosterCfG.FullRoster.FirstOrDefault(f => f.DccAddress == log.DCCiD);

                    if (fullInfo != null && fullInfo.Speedprofile != null)
                    {
                        decimal prevForwardSpeed = 0.0M;
                        decimal prevReverseSpeed = 0.0M;
                        decimal prevStep = 0.0M;
                        var mmPerSecond = 0.0M;

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
                                if ((log.TrainMotionCfg.CurrentSpeedStep < actualSpeedStep && log.TrainMotionCfg.CurrentSpeedStep >= prevStep))
                                {
                                    var percent = GetRelativeSpeedStepPosition(log.TrainMotionCfg.CurrentSpeedStep, prevStep, actualSpeedStep);
                                    if (log.TrainMotionCfg.TrainDirection == TrainDirection.Forward)
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

                        log.TrainMotionCfg.CurrentSpeedMMS = mmPerSecond;
                        var speedStep = log.TrainMotionCfg.RequiredSpeedStep;

                        var activeBlock = log.AutomatedBlockList.ElementAtOrDefault(log.AutomatedCurrentBlockIndex);
                        if (activeBlock != null)
                        {
                            activeBlock.SpeedLog.Add(new SpeedStepLog()
                            {
                                SpeedMMS = mmPerSecond,
                                start = DateTime.Now,
                                SpeedStep = speedStep
                            });
                        }
                        else
                        {
                            WriteToLog("Unable to create speed log entry - active Block lookup null");
                        }
                    }
                }
            }
        }

        private async void CheckRunningTrains(List<BlockRootObject> LiveBlocks)
        {
            var activeLogs = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete).ToList();
            for (int li = 0; li < activeLogs.Count; li++)
            {
                var log = activeLogs[li];
                if (log == null || !log.TrainMotionCfg.IsActive || log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.ReadyToDelete)
                {
                    continue;
                }

                try
                {
                    if (log.StartTime > DateTime.Now && log.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.Scheduled)
                    {
                        log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Scheduled;
                        log.StatusLastChanged = DateTime.Now;
                        WriteToLog("Not yet scheduled " + log.Name);
                        continue;
                    }
                    else if (log.StartTime < DateTime.Now && log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Scheduled)
                    {
                        log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Starting;
                        log.StatusLastChanged = DateTime.Now;
                    }

                    if (log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Scheduled)
                        continue;

                    if (log.AutomatedCurrentBlockIndex == log.AutomatedBlockList.Count - 1 && log.TrainMotionCfg.InRampDown == false
                        && (log.AutomatedTrainRunningSpeed == AutomatedTrainRunningSpeed.Stop || log.AutomatedTrainRunningSpeed == AutomatedTrainRunningSpeed.EmergencyStop)
                        && log.TrainMotionCfg.CurrentSpeedStep == 0 && log.TrainMotionCfg.RequiredSpeedStep == 0)
                    {
                        log.TrainMotionCfg.IsActive = false;
                        log.AutomatedTrainSpeedReason = "Journey complete";
                        log.StatusLastChanged = DateTime.Now;
                        log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Complete;
                        var re = c.Roster.FirstOrDefault(f => f.ID == log.DCCiD);
                        var rosterIndex = c.Roster.IndexOf(re);
                        c.SetThrottleSpeedStep(rosterIndex, 0);
                        log.Terminated = true;

                        var mem = await webClient.GetMemory(memoryAllocatedTrainsName);
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
                            await webClient.UpdateMemory(memoryAllocatedTrainsName, updateString);
                        }

                        WriteToLog("Terminated train " + log.Name);
                        continue;
                    }

                    if (log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.PauseBeforeResume)
                    {
                        var timeSincePause = DateTime.Now - log.StatusLastChanged;
                        if (timeSincePause.TotalSeconds > 5)
                        {
                            log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Resuming;
                            //still continue though so that points etc can be set and sections reserved ahead of resume
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteToLog("CRT initial checks exception " + ex.Message);
                }

                try
                {
                    int sectionCounter = 0;

                    //check sections for allocation and turnout setting                    
                    int sectionBlockCounter = 0;
                    var nextSectionAllocationFailureReason = "";
                    for (int i = log.AutomatedCurrentSectionIndex; i <= log.AutomatedCurrentSectionIndex + log.NumberOfSectionsAheadToAllocate; i++)
                    {
                        sectionCounter++;
                        var thisSectionFailureAllocationReason = ""; 
                        var section = log.AutomatedSectionList.ElementAtOrDefault(i);
                        if (section == null) continue;

                        bool previousSectionAllocated = false;
                        var previousSection = log.AutomatedSectionList.ElementAtOrDefault(i - 1);
                        if (previousSection == null && i == 0)
                        {
                            previousSectionAllocated = true;
                        }
                        else
                            previousSectionAllocated = previousSection.IsAllocated;

                        bool errorDuringBlockChecking = false;
                        bool sectionContainsUnallocatableBlock = true;
                        bool currentlyProcessingNextSection = i == log.AutomatedCurrentSectionIndex + 1 ? true : false;

                        //only do block checking and allocation for storage sections if they've been processed and a slot allocated
                        //otherwise, if the first storage section is available, it'll just get fully allocated without measurement checking
                        if (!section.IsStorage || (section.IsStorage && section.StorageSlotAllocated))
                        {
                            foreach (var block in section.Blocks)
                            {
                                var sequenceBlock = log.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == block.systemName && f.SectionSequenceId == section.Sequence);
                                var indexOfSequenceBlock = log.AutomatedBlockList.IndexOf(sequenceBlock);
                                var nextBlockInSequence = log.AutomatedBlockList.ElementAtOrDefault(indexOfSequenceBlock + 1);

                                var nextBlockName = block.BNL.BlockFound;
                                if (nextBlockInSequence != null && nextBlockInSequence.BlockUserName != block.BNL.BlockFound)
                                {
                                    //section.LastBlockRerouteAndCheckRequired = true;
                                    nextBlockName = nextBlockInSequence.BlockUserName;
                                }

                                if ((int)sequenceBlock.SequenceState > (int)JourneySequenceState.Queued && indexOfSequenceBlock >= log.AutomatedCurrentBlockIndex)
                                {
                                    //this block is already active or traversed - just set the flags to not cause any trouble as it won't need to be allocated
                                    //WriteToLog("Block " + block.userName + " deemed active or traversed for " + log.DCCiD + " seq state " + sequenceBlock.SequenceState.ToString());
                                    block.ClearToAllocate = true;
                                    block.AllocationIssue = "";
                                    sectionBlockCounter++;
                                    continue;
                                }

                                if (block.BNL != null)
                                {
                                    block.BNL = NavigateThroughBlockItems(block.userName, nextBlockName, block.BNL.UsedEdgeConnector, block.BNL.UsedEdgeConnectorDirectionConnector, "");
                                }
                                else
                                {
                                    WriteToLog("BNL null for block " + block.userName + " ID " + log.DCCiD);
                                    continue;
                                }

                                string issue = "";
                                bool allocationIssueFound = false;

                                if (!section.IsAllocated)
                                {
                                    if (log.AllocatedBlocks.Contains(block.userName))
                                    {
                                        var allocatedInPreviousBlock = false;
                                        var instancesOfThisBlockInThisTransit = log.AutomatedBlockList.Where(w => w.BlockSystemname == block.systemName).ToList();
                                        foreach (var instance in instancesOfThisBlockInThisTransit)
                                        {
                                            if (instance.Sequence < sequenceBlock.Sequence && (int)instance.SequenceState < (int)JourneySequenceState.Traversed)
                                            {
                                                allocatedInPreviousBlock = true;
                                                break;
                                            }
                                        }
                                        if (allocatedInPreviousBlock)
                                        {
                                            issue = block.userName + " allocated to this train earlier in the transit";
                                            allocationIssueFound = true;
                                        }
                                    }

                                    if (!allocationIssueFound)
                                    {
                                        var allocatedBlocksByLog = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete && w.DCCiD != log.DCCiD);
                                        foreach (var alog in allocatedBlocksByLog)
                                        {
                                            foreach (var alloc in alog.AllocatedBlocks)
                                            {
                                                if (alloc == block.userName)
                                                {
                                                    //WriteToLog("Log check can't allocate " + block.userName + " to " + log.DCCiD + " as it's allocated to " + alog.DCCiD);
                                                    issue = block.userName + " allocated to another log - " + alog.DCCiD;
                                                    allocationIssueFound = true;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!allocationIssueFound)
                                {
                                    //var liveStateBlock = LiveBlocks.FirstOrDefault(f => f.data.name == block.systemName);
                                    var liveStateBlock = await webClient.GetBlock(block.userName);
                                    if (liveStateBlock != null)
                                    {
                                        var state = liveStateBlock.data.state;
                                        var value = liveStateBlock.data.value != null ? liveStateBlock.data.value.data.userName : "";

                                        if (!section.IsAllocated)
                                        {
                                            //need to handle if a block in this section is already allocated to this train, from an earlier section in the transit
                                            //under these circumstances this section should not be allowed to be allocated, as a block within it is already allocated to this train on an earlier part of its journey
                                            //this section should not be allocated until the previous section has been exited

                                            if (state == (int)BlockState.Occupied && (int)sequenceBlock.SequenceState < (int)JourneySequenceState.Active && indexOfSequenceBlock > 0) //occupied
                                            {
                                                //this block could be active and occupied by this train in an earlier section in this transit
                                                //in this case the block should not be available to allocate in the later section

                                                if (value != log.DCCiD)
                                                {
                                                    issue = block.userName + " occupied";
                                                    if (value.Length > 0) issue += " by " + value;
                                                    allocationIssueFound = true;
                                                }
                                                else
                                                {
                                                    var occupiedInPreviousBlock = false;
                                                    var instancesOfThisBlockInThisTransit = log.AutomatedBlockList.Where(w => w.BlockSystemname == block.systemName).ToList();
                                                    foreach (var instance in instancesOfThisBlockInThisTransit)
                                                    {
                                                        if (instance.Sequence < sequenceBlock.Sequence && (int)instance.SequenceState >= (int)JourneySequenceState.Active)
                                                        {
                                                            occupiedInPreviousBlock = true;
                                                        }
                                                    }
                                                    if (occupiedInPreviousBlock)
                                                    {
                                                        issue = block.userName + " occupied by this train earlier in the transit";
                                                        allocationIssueFound = true;
                                                    }
                                                }
                                            }

                                            if (!allocationIssueFound)
                                            {
                                                //check for allocation 
                                                if (value.Length > 0)
                                                {
                                                    if (value != log.DCCiD)
                                                    {
                                                        //block is allocated to another train                         
                                                        issue = block.userName + " allocated to " + value;
                                                        //WriteToLog("Live block can't allocate " + block.userName + " to " + log.DCCiD + " because it's allocated to " + value);
                                                        allocationIssueFound = true;
                                                    }
                                                    else
                                                    {
                                                        //block is booked to this train, but possibly from an earlier section which may not yet be traversed
                                                        var allocatedInPreviousBlock = false;
                                                        var instancesOfThisBlockInThisTransit = log.AutomatedBlockList.Where(w => w.BlockSystemname == block.systemName).ToList();
                                                        foreach (var instance in instancesOfThisBlockInThisTransit)
                                                        {
                                                            if (instance.Sequence < sequenceBlock.Sequence && (int)instance.SequenceState < (int)JourneySequenceState.Active)
                                                            {
                                                                allocatedInPreviousBlock = true;
                                                            }
                                                        }
                                                        if (allocatedInPreviousBlock)
                                                        {
                                                            issue = block.userName + " allocated to this train earlier in the transit";
                                                            //WriteToLog("Live block can't allocate " + block.userName + " to " + log.DCCiD + " because it's allocated to this train earlier in the transit");
                                                            allocationIssueFound = true;
                                                        }
                                                    }
                                                }

                                            }


                                        }
                                        //else if (state == 4 && string.IsNullOrEmpty(value) && sequenceBlock.SequenceState == JourneySequenceState.Queued && indexOfSequenceBlock > 0 )
                                        else if (state == (int)BlockState.Unoccupied && string.IsNullOrEmpty(value) && sequenceBlock.SequenceState == JourneySequenceState.Queued)
                                        {
                                            var currentJourneyBlockSequenceNo = log.AutomatedCurrentBlockIndex;
                                            var sequenceNumberOfCheckedBlock = indexOfSequenceBlock;

                                            if (sequenceNumberOfCheckedBlock > currentJourneyBlockSequenceNo)
                                            {
                                                WriteToLog(block.userName + " not allocated but probably should be - setting section back to unallocated");
                                                section.IsAllocated = false;
                                                section.AllocationStatus = AllocationStatus.LostAllocation;
                                            }
                                        }
                                    }
                                    else
                                        errorDuringBlockChecking = true;
                                }
                                if (!allocationIssueFound)
                                {
                                    block.ClearToAllocate = true;
                                    block.AllocationIssue = "";
                                }
                                else
                                {
                                    block.ClearToAllocate = false;
                                    block.AllocationIssue = issue;
                                }
                                sectionBlockCounter++;
                            }

                            sectionContainsUnallocatableBlock = section.Blocks.Any(a => !a.ClearToAllocate);
                            if (sectionContainsUnallocatableBlock)
                                thisSectionFailureAllocationReason += "; unallocatable block";

                            //handle incline queue - if incline is available it will just get allocated straight away which is OK
                            //if it's not, the 'queueingForIncline' value will get set later in the method and a time set for when it entered the queue
                            //so when we get back here the second time 

                            if (section.IsInclineSection && !section.IsAllocated && !log.QueueingForIncline && currentlyProcessingNextSection)
                            {
                                log.QueueingForIncline = true;
                                log.TimeEnteredInclineQueue = DateTime.Now;
                                WriteToLog("Section " + section.SectionkUserName + " entered incline queue for "+log.DCCiD);
                            }

                            var logsInInclineQueue = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete && w.QueueingForIncline).OrderBy(o => o.TimeEnteredInclineQueue).ToList();
                            
                            var logNextInInclineQueue = true;
                            if (log.QueueingForIncline && currentlyProcessingNextSection)
                            {
                                var indexOfThisLogInInclineQueue = logsInInclineQueue.IndexOf(log);
                                if (indexOfThisLogInInclineQueue > 0)
                                {
                                    logNextInInclineQueue = false;
                                }
                            }

                            if (!sectionContainsUnallocatableBlock && !errorDuringBlockChecking && previousSectionAllocated && logNextInInclineQueue)
                            {
                                //this is where all blocks for the section get allocated
                                //WriteToLog("Starting allocation for section " + section.SectionkUserName + " ID " + log.DCCiD);
                                bool allocateFailedAnywhere = false;

                                foreach (var block in section.Blocks)
                                {
                                    //first block of a transit has to be treated differently as it's already active, so is never triggered as a new block and never gets processed as one
                                    bool processingFirstBlock = false;
                                    var sequenceBlock = log.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == block.systemName && f.SectionSequenceId == section.Sequence);

                                    //Don't try to manage blocks in the current section that have already been exited
                                    if ((int)sequenceBlock.SequenceState >= (int)JourneySequenceState.Traversed)
                                        continue;

                                    var indexOfSequenceBlock = log.AutomatedBlockList.IndexOf(sequenceBlock);
                                    if ((int)sequenceBlock.SequenceState > (int)JourneySequenceState.Queued || indexOfSequenceBlock == 0)
                                    {
                                        //block is already active or traversed, so it's fine that it's not allocated - do nothing
                                        processingFirstBlock = true;
                                        //sectionBlockCounter++;
                                        //continue;
                                    }

                                    if (!section.IsAllocated && !processingFirstBlock)
                                    {
                                        try
                                        {
                                            var correspondingLogBlock = log.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == block.systemName && f.SectionSequenceId == section.Sequence);
                                            blocksToDecorate.Add(new BlockToDecorate
                                            {
                                                SetToAlternate = true,
                                                BlockUserName = block.userName,
                                                Position = log.AutomatedBlockList.IndexOf(correspondingLogBlock)
                                            });
                                            var responseBlock = await webClient.AllocateBlock(block.systemName, log.DCCiD, true);
                                            if (responseBlock == null || responseBlock.data == null || responseBlock.data.value == null || responseBlock.data.value.data.userName != log.DCCiD)
                                            {
                                                allocateFailedAnywhere = true;
                                                WriteToLog("Allocation failure for block " + block.userName + " - 1827 no response");
                                            }

                                            //WriteToLog("Allocated block " + block.userName + " to " + log.Name);
                                            else
                                            {
                                                WriteToLog("Successful allocation for block " + block.userName + " value " + responseBlock.data.value.data.userName);
                                                if (!log.AllocatedBlocks.Contains(block.userName))
                                                    log.AllocatedBlocks.Add(block.userName);
                                                correspondingLogBlock.LastAllocationTime = DateTime.Now;                                                    
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            WriteToLog("Allocation Error - " + ex.Message);
                                            allocateFailedAnywhere = true;
                                        }

                                    }

                                    if (section.IsAllocated)
                                    {
                                        //not occupied and not allocated - set turnouts
                                        //Check all turnouts even if allocated - one may have been set by human error

                                        if (block.BNL != null && block.BNL.BNLTurnouts != null)
                                        {
                                            var tos = block.BNL.BNLTurnouts.ToList();
                                            var blockName = block.userName;
                                            foreach (var to in block.BNL.BNLTurnouts)
                                            {
                                                try
                                                {
                                                    var liveTO = await webClient.GetTurnout(to.ID);
                                                    if (liveTO != null)
                                                    {
                                                        to.CurrentState = liveTO.data.state.ToString();
                                                        var storedTO = c.Turnouts.FirstOrDefault(f => f.ID == to.ID);
                                                        if (storedTO != null)
                                                        {
                                                            storedTO.State = liveTO.data.state.ToString();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        WriteToLog("No response to request for live turnout state " + to.Name);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    WriteToLog("Error encountered checking live turnout " + to.Name + " - ex - " + ex.Message);
                                                }

                                                if (to.RequiredState != null && (to.CurrentState == null || to.CurrentState != to.RequiredState))
                                                {
                                                    to.NumberOfRetries++;
                                                    c.SetTurnout(to.ID, int.Parse(to.RequiredState));
                                                    WriteToLog("Set turnout " + to.Name + " to required state " + to.RequiredState + " for " + log.DCCiD);

                                                }
                                            }
                                        }

                                        //block values seem to be getting lost after the train has activated the block
                                        var liveStateBlock = LiveBlocks.FirstOrDefault(f => f.data.name == block.systemName);

                                        if (liveStateBlock != null && liveStateBlock.data.state == (int)BlockState.Occupied && indexOfSequenceBlock == log.AutomatedCurrentBlockIndex
                                            && sequenceBlock.SequenceState == JourneySequenceState.Active)
                                        {
                                            if (liveStateBlock.data.value == null || string.IsNullOrEmpty(liveStateBlock.data.value.data.userName))
                                            {
                                                WriteToLog("Lost block value - " + block.userName + " ID " + log.DCCiD);
                                                await webClient.AllocateBlock(block.systemName, log.DCCiD);
                                            }
                                            else if (liveStateBlock.data.value.data.userName != log.DCCiD)
                                            {
                                                WriteToLog("Incorrect block value in occupied block - " + block.userName + " ID " + log.DCCiD);
                                                await webClient.AllocateBlock(block.systemName, log.DCCiD);
                                            }
                                        }
                                    }
                                    sectionBlockCounter++;
                                }
                                if (!allocateFailedAnywhere)
                                {
                                    if (!section.IsAllocated && section.IsInclineSection && log.QueueingForIncline)
                                    {
                                        log.QueueingForIncline = false;
                                        WriteToLog(section.SectionkUserName + " is an incline section so removed from queue now it's allocated to "+log.DCCiD);
                                    }

                                    section.IsAllocated = true;
                                    section.AllocationStatus = AllocationStatus.Allocated;
                                    if (currentlyProcessingNextSection)
                                    {
                                        log.NextSectionAllocationStatus = AllocationStatus.Allocated;
                                        log.NextSection = section.SectionkUserName;
                                    }
                                        
                                }

                                else
                                {
                                    section.AllocationStatus = AllocationStatus.NotAllocated;
                                    if (currentlyProcessingNextSection)
                                    {
                                        log.NextSection = section.SectionkUserName;
                                        log.NextSectionAllocationStatus = AllocationStatus.NotAllocated;
                                    }
                                }
                            }
                            else
                            {
                                if (currentlyProcessingNextSection)
                                {
                                    log.NextSection = section.SectionkUserName;
                                    log.NextSectionAllocationStatus = AllocationStatus.NotAllocated;
                                }
                            }
                            if (sectionContainsUnallocatableBlock)
                            {
                                thisSectionFailureAllocationReason += "; unallocatable block";
                            }
                            if (errorDuringBlockChecking)
                            {
                                thisSectionFailureAllocationReason += "; block check error";
                            }
                            if (!logNextInInclineQueue)
                            {
                                thisSectionFailureAllocationReason += "; in incline queue";
                            }
                        }

                        if (currentlyProcessingNextSection)
                            log.NextSectionAllocationStatusReason = thisSectionFailureAllocationReason;

                        //look for alternates
                        if (sectionContainsUnallocatableBlock && previousSectionAllocated && log.AutomatedAlternateSectionList != null)
                        {                            
                            if (previousSection != null && previousSection.AllocationStatus == AllocationStatus.Allocated)
                            {
                                var alternatesExistOnThisTransit = log.AutomatedAlternateSectionList.Any();

                                if (alternatesExistOnThisTransit)
                                {
                                    var alternateSectionWasAllocated = false;
                                    var shuffleUpSpaceWasAllocated = false;
                                    var alternates = log.AutomatedAlternateSectionList.Where(w => w.Sequence == section.Sequence);
                                    //if it's a storage section let the subsequent code that tries to fit trains into the smallest available gap deal with it
                                    //this section will always put a train at the front of the first available line which isn't always ideal
                                    if (alternates != null && alternates.Count() > 0 && !section.IsStorage)
                                    {
                                        //WriteToLog("Alternates found for section " + section.SectionkUserName);

                                        log.CurrentAlternateIndex++;
                                        if (log.CurrentAlternateIndex >= alternates.Count())
                                            log.CurrentAlternateIndex = 0;
                                        var possibleAlt = alternates.ElementAtOrDefault(log.CurrentAlternateIndex);

                                        //entire section has to be unallocated and empty if it's going to be used
                                        foreach (var potentialAlt in alternates)
                                        {
                                            var sectionIsAvaileble = true;

                                            if (potentialAlt.SectionSystemname == section.SectionSystemname)
                                            {
                                                sectionIsAvaileble = false;
                                                continue;
                                            }

                                            foreach (var altBlock in potentialAlt.Blocks)
                                            {
                                                var liveBlock = LiveBlocks.FirstOrDefault(f => f.data.name == altBlock.systemName);
                                                if (liveBlock == null || liveBlock.data == null)
                                                {
                                                    sectionIsAvaileble = false;
                                                    break;
                                                }

                                                if (liveBlock.data.value != null && !string.IsNullOrEmpty(liveBlock.data.value.data.userName) && liveBlock.data.value.data.userName != log.DCCiD)
                                                {
                                                    sectionIsAvaileble = false;
                                                    break;
                                                }

                                                if (liveBlock.data.state != 4)
                                                {
                                                    sectionIsAvaileble = false;
                                                    break;
                                                }
                                            }

                                            if (sectionIsAvaileble)
                                            {
                                                section = potentialAlt;
                                                log.AutomatedSectionList[i] = section;
                                                log.CurrentAlternateIndex = log.CurrentAlternateIndex;

                                                var sectionAltBlocks = log.AutomatedAlternativeBlockList.Where(w => w.SectionId == section.SectionID);
                                                foreach (var altBlock in sectionAltBlocks)
                                                {
                                                    var existingBlock = log.AutomatedBlockList.FirstOrDefault(f => f.Sequence == altBlock.Sequence);
                                                    if (existingBlock != null)
                                                    {
                                                        var index = log.AutomatedBlockList.IndexOf(existingBlock);
                                                        log.AutomatedBlockList[index] = altBlock;
                                                    }
                                                }
                                                alternateSectionWasAllocated = true;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        section.AllocationStatus = AllocationStatus.NotAllocated;
                                        section.AllocationStatusReason = "Section contains unallocatable block";
                                    }

                                    if (!alternateSectionWasAllocated)
                                    {
                                        if (section.IsStorage && !section.StorageSlotAllocated)
                                        {
                                            //WriteToLog("Attempt to find storage space in yard now train length " + log.TrainLengthMM.ToString());
                                            //go through each alt section
                                            //for each section start calculating the length available by adding length of available blocks together starting with the closest
                                            //if enough space for the train is found in the back of the yard line, remove any subsequent blocks from the section, starting from the block after the block that completes the available space
                                            //then assign this section and its reduces blocks to the transit / log and the train should part on the end of the line

                                            //var sectionHasSpace = false;
                                            //int indexOfLastBlockNeeded = -1;
                                            decimal lowestDifferenceInSpace = 10000; // just needs to be a high number for the start of comparisons later
                                            int indexOfBestFitBlock = -1;
                                            int indexOfBestFitSection = -1;
                                            var storageSpaceFound = false;
                                            var sectionNameFound = "";
                                            List<block> FirstNonStorageBlocksInSection = new List<block>();

                                            for (int si = 0; si < log.AutomatedAlternateSectionList.Count; si++)
                                            {
                                                var altSec = log.AutomatedAlternateSectionList.ElementAtOrDefault(si);
                                                if (altSec != null)
                                                {
                                                    var availableSpaceMM = 0M;


                                                    //go through the blocks in the section to see if there's space
                                                    //Go right to the end of the line, because if we stop when enough space is found, there's a risk that it will leave an empty space at the start of the line
                                                    for (int bi = 0; bi < altSec.Blocks.Count; bi++)
                                                    {
                                                        var storageBlock = altSec.Blocks[bi];
                                                        if (!storageBlock.IsStorageBlock)
                                                        {
                                                            FirstNonStorageBlocksInSection.Add(storageBlock);
                                                            continue;
                                                        }

                                                        var nextBlockIsOccupied = false;
                                                        var blockIsLastInSection = false;
                                                        var possibleNextBlock = altSec.Blocks.ElementAtOrDefault(bi + 1);
                                                        if (possibleNextBlock != null)
                                                        {
                                                            var nextPossibleLiveStateBlock = LiveBlocks.FirstOrDefault(f => f.data.name == possibleNextBlock.systemName);
                                                            if (nextPossibleLiveStateBlock != null && nextPossibleLiveStateBlock.data.state == 2)
                                                            {
                                                                nextBlockIsOccupied = true;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            blockIsLastInSection = true;
                                                        }

                                                        var liveStateBlock = LiveBlocks.FirstOrDefault(f => f.data.name == storageBlock.systemName);
                                                        if (liveStateBlock != null && liveStateBlock.data.state == 4)
                                                        {
                                                            availableSpaceMM += storageBlock.length;
                                                            var blockPositionInSection = altSec.Blocks.Count - bi;
                                                            //1 = front; 2 = middle; 3 = back

                                                            if (log.TrainLengthMM < shortTrainThresholdMM)
                                                            {
                                                                if (blockPositionInSection == 1)
                                                                {
                                                                    //if it's a short train at the front make sure it won't run over into the second block
                                                                    if (log.TrainLengthMM > storageBlock.length)
                                                                    {
                                                                        WriteToLog("Short train - " + log.DCCiD + " - BP " + blockPositionInSection.ToString() + " - too long for front storage block, ignoring " + altSec.SectionkUserName);
                                                                        break;
                                                                    }
                                                                }
                                                                if (blockPositionInSection == 2 && nextBlockIsOccupied)
                                                                {
                                                                    //don't let a short train pull up behind another short train
                                                                    if (log.TrainLengthMM < shortTrainThresholdMM)
                                                                    {
                                                                        WriteToLog("Short train - " + log.DCCiD + " - BP " + blockPositionInSection.ToString() + " - can't park in block 2, ignoring " + altSec.SectionkUserName);
                                                                        break;
                                                                    }
                                                                }
                                                                //if it's found space on the end of the line for a shorty, make sure the shorty will also fit into the front block when it gets there
                                                                if (blockPositionInSection == 3 && availableSpaceMM > log.TrainLengthMM && nextBlockIsOccupied)
                                                                {
                                                                    var sectionFrontBlock = altSec.Blocks.Last();
                                                                    if (sectionFrontBlock.length < log.TrainLengthMM)
                                                                    {
                                                                        WriteToLog("Short train - " + log.DCCiD + " - BP " + blockPositionInSection.ToString() + " - found space in block 3, but block 1 is too short - ignoring " + altSec.SectionkUserName);
                                                                        break;
                                                                    }
                                                                }
                                                            }

                                                            if (availableSpaceMM > log.TrainLengthMM && (nextBlockIsOccupied || blockIsLastInSection))
                                                            {
                                                                //WriteToLog("Looks like there's space for "+log.DCCiD+" in " + altSec.SectionkUserName);                                                            
                                                                decimal thisSpaceDiff = availableSpaceMM - log.TrainLengthMM;
                                                                if (thisSpaceDiff < lowestDifferenceInSpace)
                                                                {
                                                                    lowestDifferenceInSpace = thisSpaceDiff;
                                                                    indexOfBestFitBlock = bi;
                                                                    indexOfBestFitSection = si;
                                                                    storageSpaceFound = true;
                                                                    sectionNameFound = altSec.SectionkUserName;
                                                                    section.StorageSlotAllocated = true;
                                                                    WriteToLog("Best fit space so far for " + log.DCCiD + "  in " + altSec.SectionkUserName + " diff MM " + lowestDifferenceInSpace.ToString("#.##") + " si " + si.ToString() + " bi " + bi.ToString());
                                                                }
                                                            }
                                                        }

                                                        else
                                                            break;
                                                    }
                                                }
                                            }

                                            try
                                            {
                                                if (storageSpaceFound)
                                                {
                                                    var sectionToUse = log.AutomatedAlternateSectionList.ElementAtOrDefault(indexOfBestFitSection);
                                                    if (sectionToUse != null)
                                                    {
                                                        var altBlocksToUse = log.AutomatedAlternativeBlockList.Where(w => w.SectionId == sectionToUse.SectionID);

                                                        var currentSectionToReplace = log.AutomatedSectionList.FirstOrDefault(f => f.Sequence == sectionToUse.Sequence);
                                                        //WriteToLog("Transit section " + currentSectionToReplace.SectionkUserName + " to be replaced by " + sectionToUse.SectionkUserName);
                                                        var secIndex = log.AutomatedSectionList.IndexOf(currentSectionToReplace);

                                                        foreach (var rb in altBlocksToUse)
                                                        {
                                                            //WriteToLog("RB " + rb.BlockUserName);
                                                            var blockWithSpace = sectionToUse.Blocks.FirstOrDefault(f => f.systemName == rb.BlockSystemname);
                                                            if (blockWithSpace != null)
                                                            {
                                                                shuffleUpSpaceWasAllocated = true;
                                                                var indexInSection = sectionToUse.Blocks.IndexOf(blockWithSpace);
                                                                var scriptBlock = log.AutomatedBlockList.FirstOrDefault(f => f.Sequence == rb.Sequence && f.SectionId == currentSectionToReplace.SectionID);
                                                                var replacementIndex = log.AutomatedBlockList.IndexOf(scriptBlock);
                                                                if (scriptBlock != null && replacementIndex >= 0)
                                                                {
                                                                    //WriteToLog("Replacement index " + replacementIndex.ToString());
                                                                    if (indexInSection <= indexOfBestFitBlock)
                                                                    {
                                                                        log.AutomatedBlockList[replacementIndex] = rb;
                                                                    }
                                                                    else
                                                                    {
                                                                        //remove the block from the script
                                                                        if (log.AutomatedBlockList.Count - 1 >= replacementIndex)
                                                                            log.AutomatedBlockList.RemoveAt(replacementIndex);
                                                                        if (sectionToUse.Blocks.Count - 1 >= indexInSection)
                                                                            sectionToUse.Blocks.RemoveAt(indexInSection);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    //WriteToLog("Script block lookup error seq " + rb.Sequence.ToString() + " replacement index " + replacementIndex.ToString());
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //WriteToLog("Couldn't find block " + rb.BlockUserName + " in section " + sectionToUse.SectionkUserName);
                                                            }
                                                        }

                                                        sectionToUse.AllocationStatus = AllocationStatus.NotAllocated;
                                                        sectionToUse.AllocationStatusReason = "Found suitable space in storage yard";
                                                        log.AutomatedSectionList[secIndex] = sectionToUse;
                                                        // WriteToLog("Sections and blocks updated for " + log.DCCiD + " now using " + sectionToUse.SectionkUserName);
                                                    }
                                                    else
                                                    {
                                                        //WriteToLog("SectionToUse " + indexOfBestFitSection.ToString() + " index not found for " + log.DCCiD);
                                                    }

                                                }
                                                if (!alternateSectionWasAllocated && !shuffleUpSpaceWasAllocated)
                                                {
                                                    //WriteToLog("No room at the inn for " + log.DCCiD);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                var dccId = "";
                                                if (log != null)
                                                    dccId = log.DCCiD;
                                                WriteToLog("Storage space allocation exception ID " + dccId + " - " + ex.Message);
                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    //WriteToLog("Not looking for alts as previous section is not yet allocated");
                                }
                            }
                            else
                            {
                                /*
                                if (previousSection != null)
                                    WriteToLog("Not processing " + section.SectionkUserName + " for storage fit as previous section " + previousSection.SectionkUserName + " appears unallocated");
                                else
                                    WriteToLog("Not processing " + section.SectionkUserName + " for storage fit as previous section null. First section?");
                                */
                            }



                            //WriteToLog("Section " + section.SectionkUserName + " unallocatable due to unallocatable block");
                        }
                        if (errorDuringBlockChecking)
                        {
                            section.AllocationStatus = AllocationStatus.NotAllocated;
                            section.AllocationStatusReason = "Error during block checking";
                            //WriteToLog("Section " + section.SectionkUserName + " unallocatable due to error during block checking");
                        }
                        if (!previousSectionAllocated)
                        {
                            section.AllocationStatus = AllocationStatus.NotAllocated;
                            section.AllocationStatusReason = "Previous section unallocated";
                            //WriteToLog("Section " + section.SectionkUserName + " unallocatable due to previous section being unallocated");
                        }

                        //Go down to caution in penultimate section
                        var position = log.AutomatedSectionList.IndexOf(section);
                        var positionRelative = log.AutomatedSectionList.Count - position;
                    }
                }
                catch (Exception ex)
                {
                    WriteToLog("Section and blocks processing exception " + ex.Message);
                }

                try
                {

                    //check blocks for issues
                    List<block> CheckedBlocks = new List<block>();

                    if (!string.IsNullOrEmpty(log.PreviousBlock))
                    {
                        var livePreviousBlock = LiveBlocks.FirstOrDefault(f => f.data.userName == log.PreviousBlock);
                    }

                    //This definition is for the 'for' loop. Want the for loop to run through the next 2 blocks, or fewer if fewer than 2 blocks remain
                    //Don't want to check further ahead, so if there are more than 2 remaining, limit what we search through
                    int blocksRemainingIncludingCurrent = log.AutomatedBlockList.Count - log.AutomatedCurrentBlockIndex;
                    if (blocksRemainingIncludingCurrent > 4) blocksRemainingIncludingCurrent = 4;

                    var currentOccupiedLogSectionBlock = new block();
                    var currentBlockLog = new BlockJourneyLog();

                    int blockCounter = 0;

                    for (int i = log.AutomatedCurrentBlockIndex; i < log.AutomatedCurrentBlockIndex + blocksRemainingIncludingCurrent; i++)
                    {
                        bool requiresCustomSpeedValue = false;
                        blockCounter++;

                        var thisLogBlock = log.AutomatedBlockList.ElementAtOrDefault(i);
                        if (thisLogBlock == null)
                        {
                            WriteToLog("Exception caught, this log block null " + log.DCCiD);
                            continue;
                        }
                        var thisLogSection = log.AutomatedSectionList.ElementAtOrDefault(thisLogBlock.SectionSequenceId);
                        if (thisLogBlock == null || thisLogSection == null)
                        {
                            WriteToLog("Exception caught, this log section null " + log.DCCiD);
                            continue;
                        }
                        var thisLogSectionBlock = thisLogSection.Blocks.FirstOrDefault(f => f.systemName == thisLogBlock.BlockSystemname);

                        var nextBlock = log.AutomatedBlockList.ElementAtOrDefault(i + 1);

                        /*
                        if (thisLogSectionBlock.BNL == null)
                        {
                            if (nextBlock != null)
                            {
                                thisLogSectionBlock.BNL = NavigateThroughBlockItems(thisLogBlock.BlockUserName, nextBlock.BlockUserName, previousBlockBNL.EdgeConnector, previousBlockBNL.EdgeConnectorDirectionConnector, "");
                            }
                        }
                        */
                        //mm covered - only check for current active block

                        if (i == log.AutomatedCurrentBlockIndex)
                        {
                            try
                            {
                                decimal mmCoveredSoFar = 0.0M;
                                for (int b = 0; b < thisLogBlock.SpeedLog.Count; b++)
                                {
                                    var thisSpeedLog = thisLogBlock.SpeedLog.ElementAtOrDefault(b);
                                    if (thisSpeedLog == null)
                                        continue;
                                    if (thisLogBlock.SpeedLog.ElementAt(b).SpeedStep == 0)
                                        continue;

                                    var dateTimeTo = DateTime.Now;
                                    if (b + 1 < thisLogBlock.SpeedLog.Count)
                                    {
                                        dateTimeTo = thisLogBlock.SpeedLog.ElementAt(b + 1).start;
                                    }

                                    var timeDiff = dateTimeTo - thisLogBlock.SpeedLog.ElementAt(b).start;
                                    mmCoveredSoFar += thisLogBlock.SpeedLog.ElementAt(b).SpeedMMS * (decimal)timeDiff.TotalSeconds;
                                    thisLogBlock.mmCovered = mmCoveredSoFar;

                                    //WriteToLog("Block " + thisLogBlock.BlockUserName + " mm covered " + mmCoveredSoFar.ToString() + " for " + log.DCCiD);
                                }
                            }
                            catch (Exception ex)
                            {
                                var logDCCId = "";
                                if (log != null)
                                    logDCCId = log.DCCiD;
                                WriteToLog("mm covered exception id " + logDCCId + " - " + ex.Message);
                            }

                            if (thisLogBlock.BlockTriggers != null && thisLogBlock.BlockTriggers.Count > 0)
                            {
                                var timeInBlock = DateTime.Now - thisLogBlock.TimeTrainEnteredBlock;
                                foreach (var bt in thisLogBlock.BlockTriggers)
                                {
                                    switch (bt.WhatCode)
                                    {
                                        case transitsectionwhat.LOADTRAININFO:

                                            if (!bt.Fired && bt.WhenCode == transitsectionwhen.BLOCKENTRY)
                                            {
                                                WriteToLog("Found block trigger block " + thisLogBlock.BlockUserName + " transit " + bt.TransitName);
                                                var transit = _transits.FirstOrDefault(f => f.userName == bt.TransitName);
                                                var newTransit = config.GetTransit(bt.TransitName, DispatcherPath);
                                                newTransit.Type = TransitType.Triggered;
                                                //newTransit.NextTransitDelayMS = log.NextTransitDelayMS;
                                                newTransit.NextTransitAdditionalDelayMS = log.NextTransitAdditionalDelayMS;
                                                WriteToLog("Starting new triggered BLOCKENTRY transit passing on delay " + (newTransit.NextTransitAdditionalDelayMS + newTransit.NextTransitDelayMS).ToString());
                                                StartAutoTrain(newTransit, bt.TrainsitTrainDirection, DateTime.Now);
                                                bt.Fired = true;
                                            }
                                            break;
                                        case transitsectionwhat.SETSENSORACTIVE:
                                            if (!bt.Fired && bt.WhenCode == transitsectionwhen.BLOCKENTRY)
                                            {
                                                await webClient.SetSensor(bt.WhatString, "2");
                                                bt.Fired = true;
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        string issue = "";

                        //previous speed restrictions first as they should be superceded by current block speed restrictions
                        var previousBlocksStillActive = log.AutomatedBlockList.Where(w => w.SequenceState == JourneySequenceState.Active && w.Sequence < thisLogBlock.Sequence);

                        var previousActiveBlockHasSpeedRestriction = false;
                        var previousActiveSpeedRestrictionReason = "";
                        foreach (var pb in previousBlocksStillActive)
                        {
                            if (thisLogBlock.Sequence - pb.Sequence > 1)
                                continue;
                            var bSection = log.AutomatedSectionList.FirstOrDefault(f => f.Sequence == pb.SectionSequenceId);
                            if (bSection == null) continue;
                            var cBlock = bSection.Blocks.FirstOrDefault(f => f.systemName == pb.BlockSystemname);
                            if (cBlock == null) continue;

                            var liveBlock = _allBlocks.First(f => f.data.name == cBlock.systemName);
                            if (liveBlock.data.state == 4)
                            {
                                foreach (var to in cBlock.BNL.BNLTurnouts)
                                {
                                    if (to.RequiredState == "4")
                                    {
                                        previousActiveBlockHasSpeedRestriction = true;
                                        previousActiveSpeedRestrictionReason = "Previous block " + cBlock.userName + " still active and has thrown turnout " + to.Name;
                                    }
                                }
                            }
                        }

                        if (previousActiveBlockHasSpeedRestriction)
                        {
                            requiresCustomSpeedValue = true;
                            if ((int)thisLogSectionBlock.BlockSpeed >= (int)AutomatedTrainRunningSpeed.Caution)
                            {
                                thisLogSectionBlock.BlockSpeed = AutomatedTrainRunningSpeed.Caution;
                                thisLogSectionBlock.AutomatedSpeedReason = previousActiveSpeedRestrictionReason;
                            }
                        }

                        foreach (var to in thisLogSectionBlock.BNL.BNLTurnouts)
                        {
                            if (to.RequiredState != to.CurrentState)
                            {
                                //this would be an issue if this train was blocked by another train's allocation / occupancy
                                //It would need to stop a train that was blocked, but not prevent the allocation if this and other blocks are otherwise clear
                                issue += "; " + to.Name + " set against";
                            }
                            if (to.RequiredState == "4")
                            {
                                if ((int)thisLogSectionBlock.BlockSpeed >= (int)AutomatedTrainRunningSpeed.Caution || thisLogSectionBlock.BlockSpeed == 0)
                                {
                                    thisLogSectionBlock.BlockSpeed = AutomatedTrainRunningSpeed.Caution;
                                    thisLogSectionBlock.AutomatedSpeedReason = "Thrown turnout in path (" + to.Name + ")";
                                    requiresCustomSpeedValue = true;
                                }
                            }
                        }

                        //Don't check occupancy for the block the train is already in - it'll always be occupied
                        if (i > log.AutomatedCurrentBlockIndex)
                        {
                            var liveStateBlock = LiveBlocks.FirstOrDefault(f => f.data.name == thisLogBlock.BlockSystemname);
                            if (liveStateBlock != null)
                            {
                                var state = liveStateBlock.data.state;
                                /*
                                if (liveStateBlock.data.value == null)
                                {
                                    WriteToLog("Block value reported null for " + thisLogBlock.BlockUserName+" live block returned was "+liveStateBlock.data.userName);
                                }
                                */
                                var value = liveStateBlock.data.value != null ? liveStateBlock.data.value.data.userName : "";
                                if (state == 2 && value != log.DCCiD) //occupied
                                {
                                    issue += "; Occupied";
                                    if (value.Length > 0) issue += " by " + value;
                                }
                                else
                                {
                                    if (value.Length > 0 && value != log.DCCiD)
                                    {
                                        //check for allocation                              
                                        issue += "; Allocated to " + value;
                                    }
                                    else if (value.Length == 0)
                                    {
                                        if (liveStateBlock.data.value == null)
                                        {
                                            issue += "; " + liveStateBlock.data.userName + " not allocated - null value";
                                        }
                                        else
                                        {
                                            issue += "; " + liveStateBlock.data.userName + " not allocated - empty value";
                                        }

                                        var allocationIssue = false;
                                        if (thisLogBlock.LastAllocationTime != null)
                                        {
                                            var diff = DateTime.Now - thisLogBlock.LastAllocationTime;
                                            if (diff.TotalMilliseconds > 1000)
                                            {
                                                allocationIssue = true;
                                            }
                                        }

                                        if (thisLogSection.IsAllocated && allocationIssue)
                                        {
                                            //Or just try to reallocate this block?
                                            //If all other blocks have values it might cause an issue
                                            if ((DateTime.Now - thisLogBlock.LastAllocationTime).Milliseconds > 2000)
                                            {
                                                WriteToLog("Potentially a lost allocation case, resetting allocation status for block " + liveStateBlock.data.userName + " section " + thisLogSection.SectionkUserName);
                                                var responseBlock = await webClient.AllocateBlock(liveStateBlock.data.name, log.DCCiD, false);
                                                if (responseBlock == null || responseBlock.data == null || responseBlock.data.value == null || responseBlock.data.value.data.userName != log.DCCiD)
                                                {
                                                    WriteToLog("Allocation failure for block " + liveStateBlock.data.userName + " - 1542 no response");
                                                }
                                                else
                                                {
                                                    thisLogBlock.LastAllocationTime = DateTime.Now;
                                                }
                                            }


                                            //thisLogSection.IsAllocated = false;
                                        }
                                    }
                                }
                            }
                        }


                        if (nextBlock != null)
                        {
                            var nextBlockSection = log.AutomatedSectionList.ElementAtOrDefault(nextBlock.SectionSequenceId);
                            var nextBlockSectionBlock = nextBlockSection.Blocks.FirstOrDefault(f => f.systemName == nextBlock.BlockSystemname);
                        }

                        if (i == log.AutomatedBlockList.Count - 2)
                        {
                            if ((int)thisLogSectionBlock.BlockSpeed >= (int)AutomatedTrainRunningSpeed.Caution || thisLogSectionBlock.BlockSpeed == 0)
                            {
                                thisLogSectionBlock.BlockSpeed = AutomatedTrainRunningSpeed.Caution;
                                thisLogSectionBlock.AutomatedSpeedReason = "Penultimate block";
                                requiresCustomSpeedValue = true;
                            }
                        }

                        if (!string.IsNullOrEmpty(issue))
                        {
                            thisLogSectionBlock.BlockContainsDanger = true;
                            thisLogSectionBlock.DangerReason = issue;
                        }
                        else
                        {
                            thisLogSectionBlock.BlockContainsDanger = false;
                            thisLogSectionBlock.DangerReason = "";
                        }

                        if (log.AutomatedCurrentBlockIndex == 0)
                        {
                            if ((int)thisLogSectionBlock.BlockSpeed >= (int)AutomatedTrainRunningSpeed.Caution || thisLogSectionBlock.BlockSpeed == 0)
                            {
                                thisLogSectionBlock.BlockSpeed = AutomatedTrainRunningSpeed.Caution;
                                thisLogSectionBlock.AutomatedSpeedReason = "First block";
                                requiresCustomSpeedValue = true;
                            }
                        }

                        thisLogSectionBlock.CheckSequence = blockCounter;
                        CheckedBlocks.Add(thisLogSectionBlock);
                        //previousBlockBNL = thisLogSectionBlock.BNL;

                        if (!requiresCustomSpeedValue)
                        {
                            thisLogSectionBlock.BlockSpeed = thisLogSectionBlock.DefaultBlockSpeed;
                            thisLogSectionBlock.AutomatedSpeedReason = thisLogSectionBlock.DefaultSpeedReason;
                        }

                        if (i == log.AutomatedCurrentBlockIndex)
                        {
                            currentOccupiedLogSectionBlock = thisLogSectionBlock;
                            currentBlockLog = thisLogBlock;
                        }
                    }

                    decimal nextNlockLengthMM = 0;

                    var newAspect = log.SignalAspect;
                    string newAspectReason = string.Empty;
                    bool shortBlockEarlyCautionRequired = false;

                    //calculate aspect here?
                    //if next next block contains danger then caution


                    if (CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(3) != null && CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(3).BlockContainsDanger)
                    {
                        //caution next block - if next block is also a short block then ramp to caution early
                        var nextLogBlock = log.AutomatedBlockList.ElementAtOrDefault(log.AutomatedCurrentBlockIndex + 1);
                        if (nextLogBlock != null)
                        {
                            nextNlockLengthMM = nextLogBlock.BlockLengthMM;
                            if (nextNlockLengthMM < shortBlockThresholdMM)
                            {
                                shortBlockEarlyCautionRequired = true;
                            }
                        }
                    }

                    //Checked blocks contain more blocks than we're interested in here for danger / caution because they've been checked to the end of the section
                    //Just grab the ones we need for danger checking
                    var immediateDangerBlocks = CheckedBlocks.OrderBy(o => o.CheckSequence).Take(3);

                    if (immediateDangerBlocks.ElementAtOrDefault(2) != null && immediateDangerBlocks.ElementAtOrDefault(2).BlockContainsDanger)
                    {
                        //caution
                        newAspect = SignalAspect.Caution;
                        newAspectReason = immediateDangerBlocks.ElementAtOrDefault(2).DangerReason;
                    }

                    if (immediateDangerBlocks.ElementAtOrDefault(1) != null && immediateDangerBlocks.ElementAtOrDefault(1).BlockContainsDanger)
                    {
                        //danger
                        newAspect = SignalAspect.Danger;
                        newAspectReason = immediateDangerBlocks.ElementAtOrDefault(1).DangerReason;
                    }

                    if (immediateDangerBlocks.ElementAtOrDefault(0) != null && immediateDangerBlocks.ElementAtOrDefault(0).BlockContainsDanger)
                    {
                        //stop
                        newAspect = SignalAspect.Stop;
                        newAspectReason = immediateDangerBlocks.ElementAtOrDefault(0).DangerReason;
                    }

                    var anyDanger = immediateDangerBlocks.Any(a => a.BlockContainsDanger);

                    //if (anyDanger && string.IsNullOrEmpty(newAspectReason) && !signalHasChanged)
                    //{
                    //    newAspectReason = "Danger block found but potentially in next section.";
                    //    newAspect = SignalAspect.Proceed;
                    //}

                    if (!anyDanger)
                    {
                        newAspect = SignalAspect.Proceed;
                        newAspectReason = "No danger ahead";
                    }

                    if (newAspect != log.SignalAspect)
                    {
                        if (log.SignalAspect == SignalAspect.Stop || log.SignalAspect == SignalAspect.Danger && log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Waiting
                           && (int)newAspect < (int)SignalAspect.Danger)
                        {
                            log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.PauseBeforeResume;
                            log.StatusLastChanged = DateTime.Now;
                        }
                        log.SignalAspect = newAspect;
                        log.SignalAspectReason = newAspectReason;

                        WriteToLog(log.SignalAspect.ToString() + " " + newAspectReason);
                    }

                    var newRunningSpeed = currentOccupiedLogSectionBlock.BlockSpeed;
                    var newRunningSpeedReason = currentOccupiedLogSectionBlock.AutomatedSpeedReason;

                    int numberOfBlocksRemaining = (log.AutomatedBlockList.Count - 1) - log.AutomatedCurrentBlockIndex;

                    bool inStorageLine = false;
                    bool currentBlockIsEmergencyStopOnly = false;

                    var currentSection = log.AutomatedSectionList.ElementAtOrDefault(log.AutomatedCurrentSectionIndex);
                    if (currentSection != null)
                    {
                        if (currentSection.IsStorage && numberOfBlocksRemaining == 0)
                        {
                            inStorageLine = true;
                            // WriteToLog("In storage line");
                        }
                    }

                    if (currentBlockLog.EmergencyStopOnly)
                        currentBlockIsEmergencyStopOnly = true;

                    //if the current block contains a thrown turnout and the train is going to come to a stop in it, we can't trust the block length
                    //So if there is a thrown turnout in this block and the train is coming to a stop, stop as soon as the block goes active

                    var thisBlockCheck = immediateDangerBlocks.ElementAtOrDefault(0);
                    var currentBlockContainsThrownTurnout = false;
                    if (thisBlockCheck != null)
                    {

                        if (thisBlockCheck.BNL != null)
                        {
                            if (thisBlockCheck.BNL.BNLTurnouts != null)
                            {
                                foreach (var to in thisBlockCheck.BNL.BNLTurnouts)
                                {
                                    if (to.RequiredState == "4")
                                    {
                                        currentBlockContainsThrownTurnout = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //get speed setting and set that first - this will be superceded by signal based speed
                    switch (log.SignalAspect)
                    {
                        case SignalAspect.Stop:
                            newRunningSpeed = AutomatedTrainRunningSpeed.EmergencyStop;
                            newRunningSpeedReason = "Danger in current block";
                            break;

                        case SignalAspect.Danger:
                            if (((int)log.AutomatedTrainRunningSpeed >= (int)AutomatedTrainRunningSpeed.Crawl || log.AutomatedTrainRunningSpeed == 0) && log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Running)
                            {
                                newRunningSpeed = AutomatedTrainRunningSpeed.Crawl;
                                newRunningSpeedReason = "Signal aspect set to danger";
                            }
                            else if (log.AutomatedTrainRunningStatus < AutomatedTrainRunningStatus.Running)
                            {
                                newRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                                newRunningSpeedReason = "Signal danger, waiting to resume";
                            }
                            else
                            {
                                //WriteToLog("Danger acknowledged but current speed is lower");
                                newRunningSpeedReason = "Danger acknowledged but respecting current speed of " + currentOccupiedLogSectionBlock.BlockSpeed.ToString();
                            }

                            break;
                        case SignalAspect.Caution:
                            if ((int)log.AutomatedTrainRunningSpeed >= (int)AutomatedTrainRunningSpeed.Caution || log.AutomatedTrainRunningSpeed == 0 || log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Waiting)
                            {
                                newRunningSpeed = AutomatedTrainRunningSpeed.Caution;
                                newRunningSpeedReason = "Signal set to caution";
                            }
                            else
                            {
                                //WriteToLog("Caution acknowledged but current speed is lower");
                                newRunningSpeedReason = "Caution acknowledged but respecting current speed of " + currentOccupiedLogSectionBlock.BlockSpeed.ToString();
                            }

                            break;
                    }


                    if (blocksRemainingIncludingCurrent == 2)
                    {
                        //ensure caution to slow towards end
                        if ((int)newRunningSpeed >= (int)AutomatedTrainRunningSpeed.Caution)
                        {
                            newRunningSpeed = AutomatedTrainRunningSpeed.Caution;
                            newRunningSpeedReason += "; approaching end of journey";
                        }
                    }
                    else if (blocksRemainingIncludingCurrent <= 1)
                    {
                        if ((int)newRunningSpeed >= (int)AutomatedTrainRunningSpeed.Crawl)
                        {
                            newRunningSpeed = AutomatedTrainRunningSpeed.Crawl;
                            newRunningSpeedReason += "; penulatimate or last block";
                        }
                    }

                    var lengthMM = currentBlockLog.BlockLengthMM;
                    var traversedSoFarMM = currentBlockLog.mmCovered;
                    decimal percentageOfBlockTraversed = 100.0M;
                    var mmRemaining = lengthMM - traversedSoFarMM;
                    bool previousBlockExited = false;
                    bool stopBlockHasStoppingSensor = false;

                    //there can't be a short stopping sensor without a forward one
                    //using JMRI section config 'reverse stopping sensor' as short stopping sensor
                    if (!string.IsNullOrEmpty(currentBlockLog.ForwardStoppingSensor))
                    {
                        stopBlockHasStoppingSensor = true;
                        if (string.IsNullOrEmpty(currentBlockLog.reverseStoppingSensor) || log.TrainLengthMM > shortTrainThresholdMM)
                        {
                            currentBlockLog.derivedStoppingSensor = currentBlockLog.ForwardStoppingSensor;
                        }
                        else
                            currentBlockLog.derivedStoppingSensor = currentBlockLog.reverseStoppingSensor;

                        //WriteToLog("Derived stopping sensor " + currentBlockLog.derivedStoppingSensor);
                    }

                    var previousBlock = log.AutomatedBlockList.ElementAtOrDefault(log.AutomatedCurrentBlockIndex - 1);
                    if (previousBlock != null)
                    {
                        var prevLiveStateBlock = LiveBlocks.FirstOrDefault(f => f.data.name == previousBlock.BlockSystemname);
                        if (prevLiveStateBlock != null)
                        {
                            if (prevLiveStateBlock.data.state == 4)
                            {
                                previousBlockExited = true;
                            }
                        }
                    }

                    if (lengthMM != null && lengthMM > 0 && traversedSoFarMM > 0)
                    {
                        percentageOfBlockTraversed = (traversedSoFarMM / lengthMM) * 100;
                    }

                    //debug
                    /*
                    if (log.SignalAspect == SignalAspect.Danger && currentBlockContainsThrownTurnout && currentBlockLog.EarlyExitBlock)
                    {
                       WriteToLog("Should be early stopping - thrown turnouts and early exit " + currentBlockLog.BlockUserName + " ID " + log.DCCiD);
                    }
                    else if (log.SignalAspect == SignalAspect.Danger && currentBlockLog.EarlyExitBlock)
                    {
                        WriteToLog("Regular stopping - just early exit " + currentBlockLog.BlockUserName + " ID " + log.DCCiD);
                    }
                    */

                    if (stopBlockHasStoppingSensor && (numberOfBlocksRemaining == 0 || log.SignalAspect == SignalAspect.Stop || log.SignalAspect == SignalAspect.Danger))
                    {
                        var sensor = await webClient.GetSensor(currentBlockLog.derivedStoppingSensor);
                        if (sensor != null)
                        {
                            if (sensor.data.state == 2) //active
                            {
                                newRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                                newRunningSpeedReason = "Last block or danger and stopping sensor activated - stopping";
                            }
                        }
                    }

                    else if (numberOfBlocksRemaining == 0 && inStorageLine)
                    {
                        newRunningSpeed = AutomatedTrainRunningSpeed.EmergencyStop;
                        newRunningSpeedReason = "End of journey - storage line - emergency stopping immediately";
                    }
                    else if ((lengthMM < shortBlockThresholdMM || currentBlockIsEmergencyStopOnly) && !stopBlockHasStoppingSensor && (log.SignalAspect == SignalAspect.Stop || log.SignalAspect == SignalAspect.Danger))
                    {
                        newRunningSpeed = AutomatedTrainRunningSpeed.EmergencyStop;
                        newRunningSpeedReason = "Short block or ES only block, emergency stop ASAP - " + currentOccupiedLogSectionBlock.userName + " - " + log.SignalAspect.ToString() + " - " + lengthMM.ToString();
                    }

                    else if (mmRemaining > 0 && mmRemaining < 500 && (log.SignalAspect == SignalAspect.Stop || log.SignalAspect == SignalAspect.Danger))
                    {
                        newRunningSpeed = AutomatedTrainRunningSpeed.EmergencyStop;
                        newRunningSpeedReason = "Dangerously close to end of block - " + currentOccupiedLogSectionBlock.userName + " - " + log.SignalAspect.ToString() + " - " + mmRemaining.ToString();
                    }

                    else if (numberOfBlocksRemaining == 0 && lengthMM < shortBlockThresholdMM && !stopBlockHasStoppingSensor)
                    {
                        newRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                        newRunningSpeedReason = "End of journey - short block - stopping";
                    }

                    else if (numberOfBlocksRemaining == 0 && percentageOfBlockTraversed > dangerBlockPercentToBeginRampDown && !stopBlockHasStoppingSensor && (log.TrainLengthMM <= 0 || log.TrainLengthMM > currentBlockLog.BlockLengthMM))
                    {
                        newRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                        newRunningSpeedReason = "End of journey - train longer than last block - stopping";
                    }

                    else if (numberOfBlocksRemaining == 0 && percentageOfBlockTraversed > dangerBlockPercentToBeginRampDown && !stopBlockHasStoppingSensor && lengthMM > 0 && log.TrainLengthMM < lengthMM && previousBlockExited)
                    {
                        newRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                        newRunningSpeedReason = "End of journey - train wholly in last block - stopping";
                    }

                    else if (percentageOfBlockTraversed > dangerBlockPercentToBeginRampDown && !stopBlockHasStoppingSensor && lengthMM > 0 && (log.TrainLengthMM <= 0 || log.TrainLengthMM > currentBlockLog.BlockLengthMM)
                            && (log.SignalAspect == SignalAspect.Stop || log.SignalAspect == SignalAspect.Danger))
                    {
                        newRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                        newRunningSpeedReason = "Danger block, no train length data, time to ramp to stop";
                    }

                    else if (percentageOfBlockTraversed > dangerBlockPercentToBeginRampDown && !stopBlockHasStoppingSensor && lengthMM > 0 && log.TrainLengthMM < currentBlockLog.BlockLengthMM && previousBlockExited
                            && (log.SignalAspect == SignalAspect.Stop || log.SignalAspect == SignalAspect.Danger))
                    {
                        newRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                        newRunningSpeedReason = "Danger block, train wholly in block, time to ramp to stop";
                    }
                    else if (currentBlockContainsThrownTurnout && currentBlockLog.EarlyExitBlock && (log.SignalAspect == SignalAspect.Danger || log.SignalAspect == SignalAspect.Stop))
                    {
                        newRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                        newRunningSpeedReason = "Danger block, thrown turnout detected and early exit block, so can't trust block length - stop now";
                    }

                    else if (percentageOfBlockTraversed > cautiomBlockPercentToBeginRampDown && log.SignalAspect == SignalAspect.Caution && lengthMM > 0)
                    {
                        if ((int)newRunningSpeed >= (int)AutomatedTrainRunningSpeed.Crawl || log.AutomatedTrainRunningSpeed == 0)
                        {
                            newRunningSpeed = AutomatedTrainRunningSpeed.Crawl;
                            newRunningSpeedReason = "Towards end of caution block and approaching danger";
                            //WriteToLog("Dropping from caution to crawl as approaching danger block for " + log.Name);
                        }
                    }


                    else if (shortBlockEarlyCautionRequired && log.SignalAspect == SignalAspect.Proceed && percentageOfBlockTraversed > 30)
                    {
                        newRunningSpeed = AutomatedTrainRunningSpeed.Caution;
                        newRunningSpeedReason = "Short caution block approaching";
                    }

                    if (log.AutomatedTrainRunningSpeed != newRunningSpeed)
                    {
                        WriteToLog("Speed change required for " + log.Name + " from " + log.AutomatedTrainRunningSpeed.ToString() + " to " + newRunningSpeed.ToString() + " - " + newRunningSpeedReason);
                        log.AutomatedTrainRunningSpeed = newRunningSpeed;
                        log.AutomatedTrainSpeedReason = newRunningSpeedReason;
                    }
                }
                catch (Exception ex)
                {
                    WriteToLog("Signal and speed processing exception " + ex.Message);
                }                  
            }
        }

        private void LoadConfig()
        {
            if (!File.Exists(_cfgFilePath))
            {
                return;
            }
            config = new ConfigReader(_cfgFilePath);
            LoadPreferredBlocks();
            LoadAvailableTransits();
            rosterConfig = new RosterReader(RosterPath);
        }

        private void LoadAvailableTransits()
        {
            if (_startBlocks == null) return;

            var transits = config.GetTransits(DispatcherPath).OrderBy(o => o.userName);

            _transits = transits.Where(w => lbStartBlocks.Items.Contains(w.StartBlock)).ToList();
            _transits = transits.Where(w => _startBlocks.Any(a => a.data.userName == w.StartBlock)).ToList();
            cbAvailableTransits.Items.Clear();
            cbAvailableTransits.DisplayMember = "Name";
            cbAvailableTransits.ValueMember = "Value";
            foreach ( var t in _transits.OrderBy(o => o.userName))
            {
                cbAvailableTransits.Items.Add(new
                {
                    Name = t.userName,
                    Value = t.systemName
                });
            }
        }

        private void LoadPreferredBlocks()
        {
            var cBlocks = config.GetPreferredDestinationBlocks();
            lbDestinationBlocks.Items.Clear();
            foreach (var block in cBlocks.OrderBy(o => o.userName))
            {
                lbDestinationBlocks.Items.Add(block.userName);
            }
        }

        private async void LoadStartBlocks()
        {
            var blocks = await webClient.GetBlocks();
            _startBlocks = blocks.Where(w => w.data.value != null && w.data.state == 2).ToList();
            lbStartBlocks.DisplayMember = "Name";
            lbStartBlocks.ValueMember = "Value";
            lbStartBlocks.Items.Clear();
            foreach (var sb in _startBlocks)
            {
                lbStartBlocks.Items.Add(new
                {
                    Name = sb.data.userName + " (" + sb.data.value.data.userName + ")",
                    Value = sb.data.name
                });
            }
            LoadAvailableTransits();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _isRunning = true;
            _logs = new List<LiveJourneyLog>();
            LastTimeYardWasCheckedForShuffle = DateTime.Now;

            WriteToLog("Starting");

            sam = new StationAutomationManagement();
            sam.StationManagementACRunning = false;
            sam.StationManagementCWRunning = false;

            RunShuttles();
            LoadConfig();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _isRunning = false;
        }

        private void lbRoster_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (c == null) return;
        }

        private void btnReloadStartBlocks_Click(object sender, EventArgs e)
        {
            LoadStartBlocks();

        }

        private void btnReloadDestBlocks_Click(object sender, EventArgs e)
        {
            LoadPreferredBlocks();
        }

        private void btnTransitsReload_Click(object sender, EventArgs e)
        {
            LoadAvailableTransits();
        }

        private void cbAvailableTransits_SelectedIndexChanged(object sender, EventArgs e)
        {
            dynamic transitItem = cbAvailableTransits.SelectedItem;
            var tName = transitItem.Name;
            var tSysName = transitItem.Value;

            var transit = _transits.FirstOrDefault(f => f.systemName == tSysName);
            if (transit != null)
            {
                var fb = transit.StartBlock;
                var startBlock = _startBlocks.FirstOrDefault(f => f.data.userName == fb);
                if (startBlock != null)
                {
                    var sbi = new
                    {
                        Name = startBlock.data.userName + " (" + startBlock.data.value.data.userName + ")",
                        Value = startBlock.data.name
                    };

                    var sbIndex = lbStartBlocks.Items.IndexOf(sbi);
                    if (sbIndex > -1)
                    {
                        lbStartBlocks.SelectedIndex = sbIndex;
                    }

                    var eb = transit.EndBlock;
                    var dbi = lbDestinationBlocks.Items.IndexOf(eb);
                    if (dbi > -1)
                    {
                        lbDestinationBlocks.SelectedIndex = dbi;
                    }
                    var dccId = startBlock.data.value.data.userName;
                    var re = c.Roster.FirstOrDefault(f => f.ID == dccId);
                    if (re != null)
                    {
                        var robject = new
                        {
                            Name = re.Name + " (" + re.ID + ")",
                            DCCID = re.ID
                        };
                        var rIndex = lbRoster.Items.IndexOf(robject);
                        if (rIndex > -1)
                        {
                            lbRoster.SelectedIndex = rIndex;
                        }
                    }
                }
            }
        }

        private async void StartAutoTrain(transit transit, TrainDirection direction, DateTime StartTime)
        {
            LiveJourneyLog trainLog = new LiveJourneyLog();
            trainLog.TrainMotionCfg = new TrainMotionConfig();
            trainLog.AllocatedBlocks = new List<string>();
            trainLog.AutomatedSectionList = transit.Sections.ToList();
            trainLog.AutomatedAlternateSectionList = transit.AlternateSections;
            trainLog.AutomatedCurrentSectionIndex = 0;
            trainLog.TrainMotionCfg.TrainDirection = direction;
            trainLog.StartTime = StartTime;
            trainLog.NextTransit = transit.NextTransit;
            trainLog.NextTransitDirection = transit.NextTransitDirection;
            trainLog.NextTransitDelayMS = transit.NextTransitDelayMS;
            trainLog.NextTransitAdditionalDelayMS = transit.NextTransitAdditionalDelayMS;
            trainLog.TransitType = transit.Type;
            WriteToLog("Next transit delay " + trainLog.NextTransitDelayMS.ToString()+" - additional "+trainLog.NextTransitAdditionalDelayMS.ToString());

            var startBlock = transit.StartBlock;

            var thisLiveStartBlock = _allBlocks.FirstOrDefault(f => f.data.userName == startBlock);
            if (thisLiveStartBlock == null || thisLiveStartBlock.data.value == null) return;

            var thisTrainAlreadyRunning = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete).Any(a => a.DCCiD == thisLiveStartBlock.data.value.data.userName);
            if (thisTrainAlreadyRunning)
            {
                WriteToLog("Train " + thisLiveStartBlock.data.value.data.userName + " already associated with an existing journey so can't run now");
                return;
            }

            trainLog.DCCiD = thisLiveStartBlock.data.value.data.userName;
            var rosterEntry = c.Roster.FirstOrDefault(f => f.ID == trainLog.DCCiD);
            if (rosterEntry == null) return;
            trainLog.Name = rosterEntry.Name;
            var rosterIndex = c.Roster.IndexOf(rosterEntry);

            //determine panel direction
            var nextBlock = transit.BlocksInOrder[1].BlockUserName;
            if (nextBlock == null) return;

            trainLog.CurrentBlock = transit.StartBlock;
            trainLog.NextBlock = nextBlock;

            trainLog.AutomatedBlockList = transit.BlocksInOrder.ToList();
            trainLog.AutomatedAlternativeBlockList = transit.AlternateBlocks;

            var firstBlockBNL = GetFirstBNL(trainLog.CurrentBlock, trainLog.NextBlock);
            if (firstBlockBNL == null) return;
            var prevBNL = firstBlockBNL;
            foreach (var section in transit.Sections)
            {
                foreach (var block in section.Blocks)
                {
                    var blockInSequence = transit.BlocksInOrder.FirstOrDefault(f => f.BlockSystemname == block.systemName && f.SectionSequenceId == section.Sequence);
                    if (blockInSequence == null) continue;
                    var seqIndex = transit.BlocksInOrder.IndexOf(blockInSequence);
                    if (seqIndex == -1) continue;
                    if (seqIndex == 0)
                    {
                        block.BNL = firstBlockBNL;
                    }
                    else
                    {
                        var nextBlockInSequence = transit.BlocksInOrder.ElementAtOrDefault(seqIndex + 1);
                        if (nextBlockInSequence != null)
                            block.BNL = NavigateThroughBlockItems(block.userName, nextBlockInSequence.BlockUserName, prevBNL.EdgeConnector, prevBNL.EdgeConnectorDirectionConnector, "");
                        else
                            block.BNL = NavigateThroughBlockItems(block.userName, "", prevBNL.EdgeConnector, prevBNL.EdgeConnectorDirectionConnector, "");
                        
                    }
                    prevBNL = block.BNL;
                }
            }

            var originalSections = transit.Sections.ToList();
            var originalBlocksInOrder = transit.BlocksInOrder.ToList();

            if (transit.AlternateSections != null)
            {
                foreach (var altSection in transit.AlternateSections)
                {
                    var primarySection = transit.Sections.FirstOrDefault(f => f.Sequence == altSection.Sequence);
                    if (primarySection == null || primarySection.SectionSystemname == altSection.SectionSystemname)
                        continue;
                    if (primarySection != null)
                    {
                        var index = transit.Sections.IndexOf(primarySection);
                        if (index == -1) continue;
                        transit.Sections[index] = altSection;

                        var altBlocks = transit.AlternateBlocks.Where(w => w.SectionId == altSection.SectionID);
                        foreach (var altBlock in altBlocks)
                        {
                            var existingBlock = transit.BlocksInOrder.FirstOrDefault(f => f.Sequence == altBlock.Sequence);
                            if (existingBlock != null)
                            {
                                var bIndex = transit.BlocksInOrder.IndexOf(existingBlock);
                                transit.BlocksInOrder[bIndex] = altBlock;
                            }
                        }

                        prevBNL = firstBlockBNL;
                        foreach (var section in transit.Sections)
                        {
                            foreach (var block in section.Blocks)
                            {
                                var blockInSequence = transit.BlocksInOrder.FirstOrDefault(f => f.BlockSystemname == block.systemName && f.SectionSequenceId == section.Sequence);
                                if (blockInSequence == null) continue;
                                var seqIndex = transit.BlocksInOrder.IndexOf(blockInSequence);
                                if (seqIndex == -1) continue;
                                if (seqIndex == 0)
                                {
                                    block.BNL = firstBlockBNL;
                                }
                                else
                                {
                                    var nextBlockInSequence = transit.BlocksInOrder.ElementAtOrDefault(seqIndex + 1);
                                    if (nextBlockInSequence != null)
                                        block.BNL = NavigateThroughBlockItems(block.userName, nextBlockInSequence.BlockUserName, prevBNL.EdgeConnector, prevBNL.EdgeConnectorDirectionConnector, "");
                                    else
                                        block.BNL = NavigateThroughBlockItems(block.userName, "", prevBNL.EdgeConnector, prevBNL.EdgeConnectorDirectionConnector, "");
                                }

                                prevBNL = block.BNL;

                            }
                            var originalAltSection = transit.AlternateSections.FirstOrDefault(f => f.SectionSystemname == section.SectionSystemname);
                            if (originalAltSection != null)
                            {
                                originalAltSection.Blocks = section.Blocks;
                            }
                        }
                    }
                }
            }

            transit.Sections = originalSections;
            transit.BlocksInOrder = originalBlocksInOrder;

            trainLog.AutomatedTrainActive = true;
            trainLog.LastUpdated = DateTime.Now;
            trainLog.AutomatedCurrentSectionIndex = 0;
            trainLog.AutomatedCurrentBlockIndex = 0;
            trainLog.CurrentBlockBNL = firstBlockBNL;
            trainLog.StatusLastChanged = DateTime.Now;
            if (StartTime > DateTime.Now)
                trainLog.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Scheduled;
            else
                trainLog.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Starting;

            //get train roster entry
            var fullInfo = rosterConfig.FullRoster.FirstOrDefault(f => f.DccAddress == trainLog.DCCiD);

            var fullSpeed = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerFullMMS");
            var cautionSpeed = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerCautionMMS");
            var crawlSpeed = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerCrawlMMS");
            var rampDownInterval = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerRampDownIntervalMS");
            var rampUpInterval = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerRampUpIntervalMS");
            var rampDownStep = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerRampDownStep");
            var rampUpStep = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerRampUpStep");
            var trainLength = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerTrainLengthMM");
            var defaultDirection = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerDefaultDirection");
            var sectionsAhead = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerSectionsAheadToAllocate");

            int fulSpeedMMS = DefaultFullSpeedMMS;
            int cautionMMS = DefaultCautionMMS;
            int crawlMMS = DefaultCrawlMMS;
            int numberOfSectionsAhead =_sectionsAhead;

            if (sectionsAhead != null)
            {
                int.TryParse(sectionsAhead.Value, out numberOfSectionsAhead);
            }

            trainLog.NumberOfSectionsAheadToAllocate = numberOfSectionsAhead;

            if ((transit.Type == TransitType.Triggered || transit.Type == TransitType.YardShuffle || transit.Type == TransitType.StationAutomation) && defaultDirection != null)
            {
                var textDir = defaultDirection.Value;
                if (textDir == "Forward")
                    trainLog.TrainMotionCfg.TrainDirection = TrainDirection.Forward;
                else if (textDir == "Reverse")
                    trainLog.TrainMotionCfg.TrainDirection = TrainDirection.Reverse;
                WriteToLog("Train direction override from roster - now " + trainLog.TrainMotionCfg.TrainDirection.ToString());
            }

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

            trainLog.TrainMotionCfg.Name = trainLog.Name;
            trainLog.TrainMotionCfg.DCCID = trainLog.DCCiD;
            trainLog.TrainLengthMM = trainLengthMM;

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

                                trainLog.TrainMotionCfg.ForwardCrawlSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ForwardCrawlMMS = calculatedMMS;
                            }
                            if ((crawlMMS < dReverse && crawlMMS > prevReverseSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(crawlMMS, prevReverseSpeed, dReverse);
                                var calculatedMMS = prevReverseSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                trainLog.TrainMotionCfg.ReverseCrawlSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ReverseCrawlMMS = calculatedMMS;
                            }


                            if ((cautionMMS < dForward && cautionMMS > prevForwardSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(cautionMMS, prevForwardSpeed, dForward);
                                var calculatedMMS = prevForwardSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                trainLog.TrainMotionCfg.ForwardCautionSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ForwardCautionMMS = calculatedMMS;
                            }
                            if ((cautionMMS < dReverse && cautionMMS > prevReverseSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(cautionMMS, prevReverseSpeed, dReverse);
                                var calculatedMMS = prevReverseSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                trainLog.TrainMotionCfg.ReverseCautionSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ReverseCautionMMS = calculatedMMS;
                            }

                            if (fulSpeedMMS < dForward && fulSpeedMMS > prevForwardSpeed)
                            {
                                var calc = GetRelativeSpeedPercentage(fulSpeedMMS, prevForwardSpeed, dForward);
                                var calculatedMMS = prevForwardSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                trainLog.TrainMotionCfg.ForwardFullSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ForwardFullSpeedMMS = calculatedMMS;
                            }
                            if ((fulSpeedMMS < dReverse && fulSpeedMMS > prevReverseSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(fulSpeedMMS, prevReverseSpeed, dReverse);
                                var calculatedMMS = prevReverseSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                trainLog.TrainMotionCfg.ReverseFullSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ReverseFullSpeedMMS = calculatedMMS;
                            }

                        }
                        prevForwardSpeed = dForward;
                        prevReverseSpeed = dReverse;
                        prevStep = dStep;
                    }

                }
            }
            if (fullInfo == null || fullInfo.Speedprofile == null || trainLog.TrainMotionCfg.ForwardCrawlSpeedStep == 0 || trainLog.TrainMotionCfg.ReverseCrawlSpeedStep == 0 || trainLog.TrainMotionCfg.ForwardCautionSpeedStep == 0 
                || trainLog.TrainMotionCfg.ReverseCrawlSpeedStep == 0 || trainLog.TrainMotionCfg.ForwardFullSpeedStep == 0 || trainLog.TrainMotionCfg.ReverseFullSpeedStep == 0)
            {
                /*
                if (fullInfo != null && fullInfo.Speedprofile != null)
                {
                    var firstStep = fullInfo.Speedprofile.Speeds.Speed.First();
                    if (trainLog.TrainMotionCfg.ForwardCrawlSpeedStep == 0)
                    {
                        int ss = 0;
                        var success = int.TryParse(firstStep.Step, out ss);
                        if (success)
                            trainLog.TrainMotionCfg.ForwardCrawlSpeedStep = ss;
                    }
                    if (trainLog.TrainMotionCfg.ReverseCrawlSpeedStep == 0)
                    {
                        int ss = 0;
                        var success = int.TryParse(firstStep.Step, out ss);
                        if (success)
                            trainLog.TrainMotionCfg.ReverseCrawlSpeedStep = ss;
                    }
                }
                */
                WriteToLog("Speed profile not complete for" + trainLog.DCCiD+ "- can't run it");
                return;
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

            trainLog.TrainMotionCfg.RampUpSpeedStepIncrease = rampUpStepIncrease;
            trainLog.TrainMotionCfg.RampUpIntervalMS = rampUpIntervalMS;
            trainLog.TrainMotionCfg.RampDownIntervalMS = rampDownIntervalMS;
            trainLog.TrainMotionCfg.RampDownSpeedStepDecrease = rampDownStepDecrease;

            trainLog.TrainMotionCfg.CurrentSpeedStep = 0;
            trainLog.TrainMotionCfg.TargetSpeedStep = 0;
            trainLog.TrainMotionCfg.IsActive = true;

            trainLog.TimeStarted = DateTime.Now;
            trainLog.StatusLastChanged = DateTime.Now;

            trainLog.TransitName = transit.userName;
            trainLog.LogId = Guid.NewGuid();

            _logs.Add(trainLog);

            lbRunningTransits.Items.Add(new
            {
                Name = transit.userName + " (" + trainLog.DCCiD + ")",
                Value = trainLog.DCCiD
            });

            var currentMem = await webClient.GetMemory(memoryAllocatedTrainsName);
            if (currentMem != null)
            {
                string updateVal = string.Empty;
                var currentVal = currentMem.data.value;
                if (currentVal != null)
                {
                    var currentList = currentVal.Split(';').ToList();
                    if (!currentList.Contains(trainLog.DCCiD))
                    {
                        updateVal = currentVal + ";" + trainLog.DCCiD;
                    }
                    else
                    {
                        updateVal = currentVal;
                    }
                }
                else
                {
                    updateVal = trainLog.DCCiD+";";
                }
                await webClient.UpdateMemory(memoryAllocatedTrainsName, updateVal);
            }
            else
            {
                var updateVal = trainLog.DCCiD + ";";
                await webClient.UpdateMemory(memoryAllocatedTrainsName, updateVal);
            }

            var lastSection = trainLog.AutomatedSectionList.LastOrDefault();
            string finishesInStorageLine = "";
            if (lastSection != null)
            {
                finishesInStorageLine = " last section is storage " + lastSection.SectionkUserName+" ";
            }
            WriteToLog("Started transit " + transit.userName + " for train " + trainLog.Name + " - " + Enum.GetName(typeof(TrainDirection), trainLog.TrainMotionCfg.TrainDirection) +" - "+transit.Type.ToString()+" - "
                +" secs ahead "+trainLog.NumberOfSectionsAheadToAllocate.ToString() +" - starting at "+trainLog.StartTime.TimeOfDay.ToString());
        }

        private void btnStartTransit_Click(object sender, EventArgs e)
        {
            dynamic transitItem = cbAvailableTransits.SelectedItem;
            if (transitItem == null) return;
            var tName = transitItem.Name;
            var tSysName = transitItem.Value;
            var additionalDelayMS = 0;
            if (!string.IsNullOrEmpty(tbAdditionalTriggerDelay.Text))
            {
                var success = int.TryParse(tbAdditionalTriggerDelay.Text, out additionalDelayMS);
            }

            var transit = _transits.FirstOrDefault(f => f.systemName == tSysName);
            var newTransit = config.GetTransit(transit.userName, DispatcherPath);

            newTransit.Type = TransitType.UserSelected;
            //newTransit.NextTransitDelayMS = newTransit.NextTransitDelayMS + additionalDelayMS;
            newTransit.NextTransitAdditionalDelayMS = additionalDelayMS * 1000;

            TrainDirection dir = TrainDirection.Forward;
            if (cbTransitTrainDirection.Text == "Reverse")
                dir = TrainDirection.Reverse;

            StartAutoTrain(newTransit, dir, DateTime.Now);
            tbAdditionalTriggerDelay.Text = "";
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
            return (frac,perc) ;
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
            var requiredStep =  prevStep + pos;
            var asPerc1 = requiredStep / 1000;
            var beforeRound = asPerc1 * 128;
            int dSS = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
            if (thisStep > 0 && percentage > 0 && dSS == 0)
                dSS = 1;
            return dSS;
        }

        private BlockNavigationLog GetFirstBNL(string firstBlock, string secondBlock)
        {
            string connector1 = "";
            string connector2 = "";
            string previousConnector = "";
            string breadcrumbStart = "";
            var trackSegments = config.GetTracksegmentsForBlock(firstBlock).OrderBy(o => o.Ident).ToList();
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
                 //could be a DS or turnout
                //need previous item

                var turnouts = config.GetTurnoutsInBlock(firstBlock);
                var to = turnouts.FirstOrDefault();
                if (to != null)
                {
                    connector1 = to.Connectaname;
                    connector2 = to.Connectbname;
                    breadcrumbStart = to.Ident;
                }
                else
                {
                    var slips = config.GetSlipsInBlock(firstBlock);
                    var slip = slips.FirstOrDefault();
                    if (slip != null)
                    {
                        connector1 = slip.Ident;
                        connector2 = slip.Connectaname;
                        breadcrumbStart = slip.Ident;
                    }
                }
            }
            if (connector1 == "" || connector2 == "" || previousConnector == "")
                return null;

            var firstBoundaryFromMiddle = NavigateThroughBlockItems(firstBlock, secondBlock, connector1, previousConnector, breadcrumbStart);
            if (firstBoundaryFromMiddle == null)
            {
                return null;
            }

            //if first boundary from middle has a warning - turnout closed against - we know we've gone the wrong way.
            //need to go the other way
            else if (firstBoundaryFromMiddle.LikelyIssue != null && firstBoundaryFromMiddle.LikelyIssue.Contains("AGAINST"))
            {
                firstBoundaryFromMiddle = NavigateThroughBlockItems(firstBlock, secondBlock, connector2, previousConnector, firstBoundaryFromMiddle.EdgeConnector);
            }


            var secondBoundaryFromMiddle = NavigateThroughBlockItems(firstBlock, secondBlock, connector2, previousConnector, firstBoundaryFromMiddle.EdgeConnector);
            if (secondBoundaryFromMiddle == null)
            {
                return null;
            }
            var secondBoundary = NavigateThroughBlockItems(firstBlock, secondBlock, firstBoundaryFromMiddle.EdgeConnectorDirectionConnector, firstBoundaryFromMiddle.EdgeConnector, firstBoundaryFromMiddle.EdgeConnector);
            if (secondBoundary == null)
            {
                return null;
            }

            var firstBoundary = NavigateThroughBlockItems(firstBlock, secondBlock, secondBoundary.EdgeConnectorDirectionConnector, secondBoundary.EdgeConnector, secondBoundary.EdgeConnector);
            if (firstBoundary == null)
            {
                return null;
            }

            if (firstBoundary.BlockFound == secondBlock)
                return firstBoundary;
            else if (secondBoundary.BlockFound == secondBlock)
                return secondBoundary;
            return null;
        }

        private BlockNavigationLog NavigateThroughBlockItems(string currentBlock, string nextBlock, string LayoutItem, string previousLayoutItem, string breadcrumbStart)
        {
            if (string.IsNullOrEmpty(LayoutItem) || string.IsNullOrEmpty(currentBlock) || string.IsNullOrEmpty(previousLayoutItem))
                return null;

            var bnl = new BlockNavigationLog();
            bnl.BNLTurnouts = new List<BNLTurnout>();
            bnl.BlockChecked = currentBlock;
            bnl.StartItem = LayoutItem;
            bnl.StartPreviousItem = previousLayoutItem;
            bnl.UsedEdgeConnector = LayoutItem;
            bnl.UsedEdgeConnectorDirectionConnector = previousLayoutItem;
            if (breadcrumbStart != "") bnl.Breadcrumb += breadcrumbStart + ";";
            if (LayoutItem.Substring(0, 2) == "TO")
            {
                //turnout
                var to = config.GetLayuoutTurnout(LayoutItem);
                var configTurnout = config.GetTurnoutByUserName(to.Turnoutname);
                var liveTurnout = c.Turnouts.FirstOrDefault(f => f.ID == configTurnout.systemName);// await webClient.GetTurnout(configTurnout.systemName);
                var derivedXoverBlockName = "";
                BNLTurnout bnlto = new BNLTurnout();
                bnlto.ID = configTurnout.systemName;
                bnlto.Name = configTurnout.userName;
                bnlto.CurrentState = liveTurnout.State;

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

                if (derivedXoverBlockName != currentBlock && (derivedXoverBlockName == nextBlock || nextBlock == "") && to.Connectaname != breadcrumbStart && to.Connectbname != breadcrumbStart && to.Connectcname != breadcrumbStart && to.Connectdname != breadcrumbStart)
                {
                    bnl.EdgeConnector = to.Ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    if (derivedXoverBlockName == nextBlock)
                        bnl.BlockFound = to.Blockname;
                    else
                    {
                        bnl.BlockFound = "";
                        bnl.NoMoreBlocksFound = true;
                        bnl.LikelyIssue = "End of journey";
                    }    
                    if (to.Connectbname == previousLayoutItem || to.Connectcname == previousLayoutItem)
                    {
                        bnl.NextBlockEdgeConnector = to.Connectaname;
                    }
                    else if (liveTurnout.State == "2")
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
                        string blockFoundOnTurnoutSearch = "";
                        if (to.Type.StartsWith("LH"))
                        {
                            //On a LH - connections B and D approach at turnout start - A and C approach at V
                            //if approaching on B, test A and D
                            //if approaching on D, test B and C
                            //if approaching from A or C, need closed

                            if (to.Connectaname == previousLayoutItem || to.Connectcname == previousLayoutItem)
                            {
                                bnlto.RequiredState = "2";
                                nextItemIdent = to.Connectaname == previousLayoutItem ? to.Connectbname : to.Connectdname;
                            }
                            else if (to.Connectbname == previousLayoutItem)
                            {
                                var testbnlA = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectaname, to.Ident, "");
                                var testbnlD = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectdname, to.Ident, "");

                                if (testbnlA.BlockFound == nextBlock)
                                {
                                    nextItemIdent = to.Connectaname;
                                    bnlto.RequiredState = "2";

                                }
                                else if (testbnlD.BlockFound == nextBlock)
                                {
                                    nextItemIdent = to.Connectdname;
                                    bnlto.RequiredState = "4";
                                }
                                else
                                {
                                    blockFoundOnTurnoutSearch = testbnlA.BlockFound;
                                }
                            }
                            else if (to.Connectdname == previousLayoutItem)
                            {
                                var testbnlB = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectbname, to.Ident, "");
                                var testbnlC = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectcname, to.Ident, "");
                                if (testbnlB.BlockFound == nextBlock)
                                {
                                    nextItemIdent = to.Connectbname;
                                    bnlto.RequiredState = "4";

                                }
                                else if (testbnlC.BlockFound == nextBlock)
                                {
                                    nextItemIdent = to.Connectcname;
                                    bnlto.RequiredState = "2";
                                }
                                else
                                {
                                    blockFoundOnTurnoutSearch = testbnlB.BlockFound;
                                }
                            }
                        }
                        else if (to.Type.StartsWith("RH"))
                        {
                            //on a RH - connections A and C approach at turnout start - B and D approach at V
                            //if approaching from A, test B and C
                            //if approaching from C, test D and A

                            //if approaching from D or B, need closed
                            if (to.Connectbname == previousLayoutItem || to.Connectdname == previousLayoutItem)
                            {
                                bnlto.RequiredState = "2";
                                nextItemIdent = to.Connectbname == previousLayoutItem ? to.Connectaname : to.Connectcname;
                            }
                            else if (to.Connectaname == previousLayoutItem)
                            {
                                var testbnlB = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectbname, to.Ident, "");
                                var testbnlC = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectcname, to.Ident, "");
                                if (testbnlB.BlockFound == nextBlock)
                                {
                                    nextItemIdent = to.Connectbname;
                                    bnlto.RequiredState = "2";

                                }
                                else if (testbnlC.BlockFound == nextBlock)
                                {
                                    nextItemIdent = to.Connectcname;
                                    bnlto.RequiredState = "4";
                                }
                                else
                                {
                                    blockFoundOnTurnoutSearch = testbnlB.BlockFound;
                                }
                            }
                            else if (to.Connectcname == previousLayoutItem)
                            {
                                var testbnlA = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectaname, to.Ident, "");
                                var testbnlD = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectdname, to.Ident, "");

                                if (testbnlA.BlockFound == nextBlock)
                                {
                                    nextItemIdent = to.Connectaname;
                                    bnlto.RequiredState = "4";

                                }
                                else if (testbnlD.BlockFound == nextBlock)
                                {
                                    nextItemIdent = to.Connectdname;
                                    bnlto.RequiredState = "2";
                                }
                                else
                                {
                                    blockFoundOnTurnoutSearch = testbnlA.BlockFound;
                                }
                            }
                        }

                        bnl.BNLTurnouts.Add(bnlto);
                        bnl.Breadcrumb += nextItemIdent + ";";
                        var newbnl = NavigateThroughBlockItems(currentBlock, nextBlock, nextItemIdent, to.Ident, "");
                        bnl.Breadcrumb += newbnl.Breadcrumb;
                        bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                        bnl.BNLTurnouts.AddRange(newbnl.BNLTurnouts);

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
                        string blockFoundOnTurnoutSearch = "";
                        if (to.Connectbname == previousLayoutItem || to.Connectcname == previousLayoutItem)
                        {
                            nextItemIdent = to.Connectaname;
                            //Arriving at V of turnout
                            if (liveTurnout.State == "4")
                            {
                                //thrown
                                if (to.Connectbname == previousLayoutItem)
                                {
                                    bnl.LikelyIssue = liveTurnout.Name + " THROWN AGAINST";
                                    bnlto.RequiredState = "2";
                                }
                                else bnlto.RequiredState = "4";
                            }
                            else if (liveTurnout.State == "2")
                            {
                                //closed
                                if (to.Connectcname == previousLayoutItem)
                                {
                                    bnl.LikelyIssue = liveTurnout.Name + " CLOSED AGAINST";
                                    bnlto.RequiredState = "4";
                                }
                                else bnlto.RequiredState = "2";
                            }
                        }
                        else
                        {
                            //arriving at front
                            var testbnlB = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectbname, to.Ident, "");
                            var testbnlC = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectcname, to.Ident, "");

                            if (testbnlB.BlockFound == nextBlock)
                            {
                                nextItemIdent = to.Connectbname;
                                bnlto.RequiredState = "2";
                                
                            }
                            else if (testbnlC.BlockFound == nextBlock)
                            {
                                nextItemIdent = to.Connectcname;
                                bnlto.RequiredState = "4";
                            }
                            else
                            {
                                if (liveTurnout.State == "2")
                                {
                                    blockFoundOnTurnoutSearch = testbnlB.BlockFound;
                                    nextItemIdent = to.Connectbname;
                                }
                                else if (liveTurnout.State == "4")
                                {
                                    blockFoundOnTurnoutSearch = testbnlC.BlockFound;
                                    nextItemIdent = to.Connectcname;
                                }

                            }
                        }

                        //it's possible that neither BNL check above returned a matching block if we're already in a block search branch - the wrong one
                        //Need to be able to go back down the chain so an alternative branch can be searched
                        //We don't really need to return anything from here but adding one of the incorrectly found blocks would probably help
                        bnl.Breadcrumb += nextItemIdent + ";";
                        bnl.BNLTurnouts.Add(bnlto);

                        if (!string.IsNullOrEmpty(nextItemIdent))
                        {
                            var newbnl = NavigateThroughBlockItems(currentBlock, nextBlock, nextItemIdent, to.Ident, "");
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
                            bnl.BNLTurnouts.AddRange(newbnl.BNLTurnouts);
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
                            bnl.BlockFound = blockFoundOnTurnoutSearch;
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
                    if (nextBlock == "")
                    {
                        bnl.BlockFound = "";
                        bnl.NoMoreBlocksFound = true;
                        bnl.LikelyIssue = "End of journey";
                    }
                    else
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
                    var newbnl = NavigateThroughBlockItems(currentBlock, nextBlock, nextItem, ts.Ident, "");
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
                    bnl.BNLTurnouts.AddRange(newbnl.BNLTurnouts);
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
                var newbnl = NavigateThroughBlockItems(currentBlock, nextBlock, nextItem, a.Ident, "");
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
                bnl.BNLTurnouts.AddRange(newbnl.BNLTurnouts);
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
                var liveTA = c.Turnouts.FirstOrDefault(f => f.ID == cfgTurnoutA.systemName);
                var liveTB = c.Turnouts.FirstOrDefault(f => f.ID == cfgTurnoutB.systemName);
                var nextItem = "";
                var issueFound = "";
                string astate = liveTA.State;
                string bstate = liveTB.State;

                BNLTurnout bnltoA = new BNLTurnout();
                bnltoA.ID = cfgTurnoutA.systemName;
                bnltoA.Name = cfgTurnoutA.userName;
                bnltoA.CurrentState = liveTA.State;

                BNLTurnout bnltoB = new BNLTurnout();
                bnltoB.ID = cfgTurnoutB.systemName;
                bnltoB.Name = cfgTurnoutB.userName;
                bnltoB.CurrentState = liveTB.State;

                string blockFoundOnTurnoutSearch = "";

                if (slip.Blockname != currentBlock)
                {
                    bnl.EdgeConnector = slip.Ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    if (nextBlock == "")
                    {
                        bnl.BlockFound = "";
                        bnl.NoMoreBlocksFound = true;
                        bnl.LikelyIssue = "End of journey";
                    }
                    else
                        bnl.BlockFound = slip.Blockname;

                    bnl.NextBlockEdgeConnector = nextItem;
                }
                else
                {
                    if (slip.Connectaname == previousLayoutItem || slip.Connectbname == previousLayoutItem)
                    {
                        //Approaching from...

                        //Route is through C or D
                        var testbnlC = NavigateThroughBlockItems(currentBlock, nextBlock, slip.Connectcname, slip.Ident, "");
                        var testbnlD = NavigateThroughBlockItems(currentBlock, nextBlock, slip.Connectdname, slip.Ident, "");

                        if (testbnlC.BlockFound == nextBlock)
                        {
                            nextItem = slip.Connectcname;
                            if (slip.Connectaname == previousLayoutItem)
                            {
                                //AC
                                bnltoA.RequiredState = slip.States.AC.Turnout;
                                bnltoB.RequiredState = slip.States.AC.TurnoutB;
                            }
                            else
                            {
                                //BC
                                bnltoA.RequiredState = slip.States.BC.Turnout;
                                bnltoB.RequiredState = slip.States.BC.TurnoutB;
                            }
                        }

                        else if (testbnlD.BlockFound == nextBlock)
                        {
                            nextItem = slip.Connectdname;
                            if (slip.Connectaname == previousLayoutItem)
                            {
                                //AD
                                bnltoA.RequiredState = slip.States.AD.Turnout;
                                bnltoB.RequiredState = slip.States.AD.TurnoutB;
                            }
                            else
                            {
                                //BD
                                bnltoA.RequiredState = slip.States.BD.Turnout;
                                bnltoB.RequiredState = slip.States.BD.TurnoutB;
                            }
                        }
                        else
                        {
                            blockFoundOnTurnoutSearch = testbnlC.BlockFound;
                        }
                    }
                    else
                    {
                        var testbnlA = NavigateThroughBlockItems(currentBlock, nextBlock, slip.Connectaname, slip.Ident, "");
                        var testbnlB = NavigateThroughBlockItems(currentBlock, nextBlock, slip.Connectbname, slip.Ident, "");
                        if (testbnlA.BlockFound == nextBlock)
                        {
                            nextItem = slip.Connectaname;
                            if (slip.Connectcname == previousLayoutItem)
                            {
                                //AC
                                bnltoA.RequiredState = slip.States.AC.Turnout;
                                bnltoB.RequiredState = slip.States.AC.TurnoutB;
                            }
                            else
                            {
                                //AD
                                bnltoA.RequiredState = slip.States.AD.Turnout;
                                bnltoB.RequiredState = slip.States.AD.TurnoutB;
                            }
                        }
                        else if (testbnlB.BlockFound == nextBlock)
                        {
                            nextItem = slip.Connectbname;
                            if (slip.Connectcname == previousLayoutItem)
                            {
                                //BC
                                bnltoA.RequiredState = slip.States.BC.Turnout;
                                bnltoB.RequiredState = slip.States.BC.TurnoutB;
                            }
                            else
                            {
                                //BD
                                bnltoA.RequiredState = slip.States.BD.Turnout;
                                bnltoB.RequiredState = slip.States.BD.TurnoutB;
                            }
                        }
                        else
                        {
                            blockFoundOnTurnoutSearch = testbnlA.BlockFound;
                        }
                    }

                    bnl.BNLTurnouts.Add(bnltoA);
                    bnl.BNLTurnouts.Add(bnltoB);

                    if (string.IsNullOrEmpty(blockFoundOnTurnoutSearch))
                    {
                        bnl.LikelyIssue = issueFound;
                        bnl.Breadcrumb += nextItem + ";";
                        var newbnl = NavigateThroughBlockItems(currentBlock, nextBlock, nextItem, slip.Ident, "");
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
                        bnl.BNLTurnouts.AddRange(newbnl.BNLTurnouts);
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
                        bnl.BlockFound = blockFoundOnTurnoutSearch;
                    }

                }
            }
            else if (LayoutItem.Substring(0,2) == "EB")
            {
                //end bumper - end of the line
                bnl.NoMoreBlocksFound = true;

                //can only go backwards from here
                bnl.EdgeConnector = LayoutItem;
                bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                bnl.BlockFound = "";
                bnl.LikelyIssue = "No more blocks";
            }
            return bnl;
        }
        private void WriteToLog(string message)
        {
            if (message == LastLogLine) return;

            var fullMess = DateTime.Now.TimeOfDay.ToString() + " - " + message;
            lbOutput.Items.Add(fullMess);
            lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
            LastLogLine = message;
        }

        private void UpdateLogPanel()
        {
            if (lbRunningTransits.SelectedIndex >=0)
            {
                dynamic rt = lbRunningTransits.SelectedItem ;
                if (rt == null) return;
                string dccId = rt.Value;
                var log = _logs.FirstOrDefault(f => f.DCCiD == dccId && f.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete);
                if (log == null) return;

                lblActiveTransitID.Text = log.DCCiD;
                lblActiveTransitName.Text = log.Name;
                lblSignalAspect.Text = Enum.GetName(typeof(SignalAspect), log.SignalAspect);
                lblSignalReason.Text = log.SignalAspectReason;
                lblSpeed.Text = log.AutomatedTrainRunningSpeed.ToString();
                lblSpeedReason.Text = log.AutomatedTrainSpeedReason;
                lblSpeedStep.Text = log.TrainMotionCfg.CurrentSpeedStep.ToString()+" / "+log.TrainMotionCfg.TargetSpeedStep.ToString();
                lblSpeedMMS.Text = decimal.Round(log.TrainMotionCfg.CurrentSpeedMMS, 0, MidpointRounding.AwayFromZero).ToString();

                var nextSectionText = log.NextSection+" - "+log.NextSectionAllocationStatus.ToString();
                if (log.NextSectionAllocationStatus != AllocationStatus.Allocated)
                    nextSectionText += " - "+ log.NextSectionAllocationStatusReason;
                lblNextSectionInfo.Text = nextSectionText;

                if (log.AutomatedTrainRunningStatus == AutomatedTrainRunningStatus.Scheduled)
                {
                    var timeUntilStart = log.StartTime - DateTime.Now;
                    lblTrainStatus.Text = "Starting in " + timeUntilStart.TotalSeconds.ToString("#") + " seconds";
                }
                else
                    lblTrainStatus.Text = Enum.GetName(typeof(AutomatedTrainRunningStatus), log.AutomatedTrainRunningStatus);

                var logBlock = log.AutomatedBlockList.ElementAtOrDefault(log.AutomatedCurrentBlockIndex);
                if (logBlock != null)
                {
                    lblBlockLength.Text = logBlock.BlockLengthMM.ToString();
                    lblCurrentBlock.Text = logBlock.BlockUserName;
                    lblMmCoveredThisBlock.Text = decimal.Round(logBlock.mmCovered,0,MidpointRounding.AwayFromZero).ToString();
                    if (logBlock.BlockLengthMM > 0)
                    {
                        var perc = (logBlock.mmCovered / logBlock.BlockLengthMM) * 100;
                        var percRounded = decimal.Round(perc, 0, MidpointRounding.AwayFromZero);
                        lblmmCoveredPercent.Text = percRounded.ToString();
                        int pbVal = (int)percRounded;
                        if (pbVal > 100) pbVal = 100;
                        if (pbVal < 0) pbVal = 0;
                        pbBlockProgress.Value = pbVal;
                    }
                    else
                    {
                        lblmmCoveredPercent.Text = "0";
                    }
                }
            }

            var inclineQueue = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete && w.QueueingForIncline).OrderBy(o => o.TimeEnteredInclineQueue).ToList();
            List<Keyvaluepair> queueItems = new List<Keyvaluepair>();
            bool lbUpdateNeeded = false;

            if (inclineQueue.Count != lbInclineQueue.Items.Count)
            {
                lbUpdateNeeded = true;
            }
            else
            {
                var lbItemsList = lbInclineQueue.Items.Cast<Keyvaluepair>().ToList();
                for (int i = 0; i < inclineQueue.Count; i++)
                {
                    var iqLog = inclineQueue[i];
                    var correspondingCurrentItem = lbItemsList.ElementAtOrDefault(i) as Keyvaluepair;
                    if (correspondingCurrentItem.Key != iqLog.DCCiD)
                    {
                        lbUpdateNeeded = true;
                    }
                }
            }

            if (lbUpdateNeeded)
            {
                foreach (var iqLog in inclineQueue)
                {
                    var diff = DateTime.Now - iqLog.TimeEnteredInclineQueue;
                    var kvp = new Keyvaluepair()
                    {
                        Key = iqLog.DCCiD,
                        Value = iqLog.DCCiD + " (" + diff.TotalSeconds.ToString() + ")"
                    };
                    queueItems.Add(kvp);
                }

                lbInclineQueue.Items.Clear();
                lbInclineQueue.ValueMember = "Key";
                lbInclineQueue.DisplayMember = "Value";
                foreach (var kvp in queueItems)
                {
                    lbInclineQueue.Items.Add(kvp);
                }
            }
        }

        private void lbRunningTransits_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private BlockNavigationLog SearchForBlock(string currentBlock, string targetBlock, List<List<string>> traversedBlocks,  string LayoutItem, string previousLayoutItem, string breadcrumbStart, int branchLevel)
        {
            if (string.IsNullOrEmpty(LayoutItem) || string.IsNullOrEmpty(currentBlock) || string.IsNullOrEmpty(previousLayoutItem))
                return null;

            var bnl = new BlockNavigationLog();
            bnl.BNLTurnouts = new List<BNLTurnout>();
            bnl.BlockChecked = currentBlock;
            bnl.StartItem = LayoutItem;
            bnl.StartPreviousItem = previousLayoutItem;
            bnl.UsedEdgeConnector = LayoutItem;
            bnl.ValidBlockPath = new List<string>();
            bnl.UsedEdgeConnectorDirectionConnector = previousLayoutItem;
            bnl.BranchBlockLog = new List<string>();
            bnl.TargetFound = false;
            bnl.ViablePaths = new List<List<string>>();

            if (breadcrumbStart != "") bnl.Breadcrumb += breadcrumbStart + ";";

            var testTrav = traversedBlocks.ElementAtOrDefault(branchLevel);
            if (testTrav == null)
            {
                traversedBlocks.Add(new List<string>());
            }
            if (LayoutItem.Substring(0, 2) == "TO")
            {
                //turnout
                var to = config.GetLayuoutTurnout(LayoutItem);
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

                //&& to.Connectaname != breadcrumbStart && to.Connectbname != breadcrumbStart && to.Connectcname != breadcrumbStart && to.Connectdname != breadcrumbStart)
                if (derivedXoverBlockName != currentBlock)
                {
                    traversedBlocks[branchLevel].Add(currentBlock);
                    currentBlock = derivedXoverBlockName;
                }

                var blockLog = FlattenLog(traversedBlocks, branchLevel - 1);
                if (derivedXoverBlockName == targetBlock)
                {
                    bnl.TargetFound = true;
                    bnl.EdgeConnector = to.Ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    bnl.BlockFound = to.Blockname;
                    traversedBlocks[branchLevel].Add(to.Blockname);
                    bnl.ValidBlockPath = traversedBlocks[branchLevel];
                    bnl.ViablePaths.Add(FlattenLog(traversedBlocks, branchLevel));
                }
                else if (blockLog.Contains(derivedXoverBlockName))
                {
                    //come across a duplicate, go back
                    bnl.TargetFound = false;
                }
                else
                {
                    string nextItemIdent = "";
                    if (to.Type.Contains("XOVER"))
                    {
                        if (to.Type.StartsWith("LH"))
                        {
                            //On a LH - connections B and D approach at turnout start - A and C approach at V
                            //if approaching on B, test A and D
                            //if approaching on D, test B and C
                            //if approaching from A or C, need closed

                            if (to.Connectaname == previousLayoutItem || to.Connectcname == previousLayoutItem)
                            {
                                nextItemIdent = to.Connectaname == previousLayoutItem ? to.Connectbname : to.Connectdname;
                                bnl.Breadcrumb += nextItemIdent + ";";

                                var newbnl = SearchForBlock(currentBlock, targetBlock, traversedBlocks, nextItemIdent, to.Ident, "", branchLevel);
                                bnl.Breadcrumb += newbnl.Breadcrumb;
                                bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                                bnl.TargetFound = newbnl.TargetFound;
                                bnl.EdgeConnector = newbnl.EdgeConnector;
                                bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                                bnl.NextBlockEdgeConnector = newbnl.NextBlockEdgeConnector;
                                bnl.ValidBlockPath.AddRange(newbnl.ValidBlockPath);
                                bnl.ViablePaths.AddRange(newbnl.ViablePaths);
                            }
                            else if (to.Connectbname == previousLayoutItem)
                            {
                                var testbnlA = SearchForBlock(currentBlock, targetBlock, traversedBlocks, to.Connectaname, to.Ident, "", branchLevel+1);
                                if (testbnlA.TargetFound)
                                {
                                    var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                                    bnl.ViablePaths.AddRange(testbnlA.ViablePaths);
                                    bnl.TargetFound = true;
                                }
                                traversedBlocks.RemoveAt(branchLevel + 1);
                                var testbnlD = SearchForBlock(currentBlock, targetBlock, traversedBlocks, to.Connectdname, to.Ident, "", branchLevel + 1);
                                if (testbnlD.TargetFound)
                                {
                                    var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                                    bnl.ViablePaths.AddRange(testbnlD.ViablePaths);
                                    bnl.TargetFound = true;
                                }
                                traversedBlocks.RemoveAt(branchLevel + 1);

                            }
                            else if (to.Connectdname == previousLayoutItem)
                            {
                                var testbnlB = SearchForBlock(currentBlock, targetBlock, traversedBlocks, to.Connectbname, to.Ident, "", branchLevel + 1);
                                if (testbnlB.TargetFound)
                                {
                                    var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                                    bnl.ViablePaths.AddRange(testbnlB.ViablePaths);
                                    bnl.TargetFound = true;
                                }
                                traversedBlocks.RemoveAt(branchLevel + 1);
                                var testbnlC = SearchForBlock(currentBlock, targetBlock, traversedBlocks, to.Connectcname, to.Ident, "", branchLevel+1);
                                if (testbnlC.TargetFound)
                                {
                                    var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                                    bnl.ViablePaths.AddRange(testbnlC.ViablePaths);
                                    bnl.TargetFound = true;
                                }
                                traversedBlocks.RemoveAt(branchLevel + 1);

                            }
                        }
                        else if (to.Type.StartsWith("RH"))
                        {
                            //on a RH - connections A and C approach at turnout start - B and D approach at V
                            //if approaching from A, test B and C
                            //if approaching from C, test D and A

                            //if approaching from D or B, need closed
                            if (to.Connectbname == previousLayoutItem || to.Connectdname == previousLayoutItem)
                            {
                                nextItemIdent = to.Connectbname == previousLayoutItem ? to.Connectaname : to.Connectcname;

                                bnl.Breadcrumb += nextItemIdent + ";";

                                var newbnl = SearchForBlock(currentBlock, targetBlock, traversedBlocks, nextItemIdent, to.Ident, "", branchLevel);
                                bnl.Breadcrumb += newbnl.Breadcrumb;
                                bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                                bnl.EdgeConnector = newbnl.EdgeConnector;
                                bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                                bnl.NextBlockEdgeConnector = newbnl.NextBlockEdgeConnector;
                                bnl.ValidBlockPath.AddRange(newbnl.ValidBlockPath);
                                bnl.TargetFound = newbnl.TargetFound;
                                bnl.ViablePaths.AddRange(newbnl.ViablePaths);
                            }
                            else if (to.Connectaname == previousLayoutItem)
                            {
                                var testbnlB = SearchForBlock(currentBlock, targetBlock, traversedBlocks, to.Connectbname, to.Ident, "", branchLevel + 1);
                                if (testbnlB.TargetFound)
                                {
                                    var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                                    bnl.ViablePaths.AddRange(testbnlB.ViablePaths);
                                    bnl.TargetFound = true;
                                }
                                traversedBlocks.RemoveAt(branchLevel + 1);
                                var testbnlC = SearchForBlock(currentBlock, targetBlock, traversedBlocks, to.Connectcname, to.Ident, "", branchLevel+1);
                                if (testbnlC.TargetFound)
                                {
                                    var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                                    bnl.ViablePaths.AddRange(testbnlC.ViablePaths);
                                    bnl.TargetFound = true;
                                }
                                traversedBlocks.RemoveAt(branchLevel + 1);


                            }
                            else if (to.Connectcname == previousLayoutItem)
                            {
                                var testbnlA = SearchForBlock(currentBlock, targetBlock, traversedBlocks, to.Connectaname, to.Ident, "", branchLevel + 1);
                                if (testbnlA.TargetFound)
                                {
                                    var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                                    bnl.ViablePaths.AddRange(testbnlA.ViablePaths);
                                    bnl.TargetFound = true;
                                }
                                traversedBlocks.RemoveAt(branchLevel + 1);
                                var testbnlD = SearchForBlock(currentBlock, targetBlock, traversedBlocks, to.Connectdname, to.Ident, "", branchLevel+1);
                                if (testbnlD.TargetFound)
                                {
                                    var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                                    bnl.ViablePaths.AddRange(testbnlD.ViablePaths);
                                    bnl.TargetFound = true;
                                }
                                traversedBlocks.RemoveAt(branchLevel + 1);

                            }
                        }

                        bnl.Breadcrumb += nextItemIdent + ";";

                    }
                    else
                    {
                        //turnout
                        if (to.Connectbname == previousLayoutItem || to.Connectcname == previousLayoutItem)
                        {
                            nextItemIdent = to.Connectaname;
                            //Arriving at V of turnout
                            bnl.Breadcrumb += nextItemIdent + ";";

                            var newbnl = SearchForBlock(currentBlock, targetBlock, traversedBlocks, nextItemIdent, to.Ident, "", branchLevel);
                            bnl.Breadcrumb += newbnl.Breadcrumb;
                            bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                            bnl.EdgeConnector = newbnl.EdgeConnector;
                            bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                            bnl.NextBlockEdgeConnector = newbnl.NextBlockEdgeConnector;
                            bnl.ValidBlockPath.AddRange(newbnl.ValidBlockPath);
                            bnl.TargetFound = newbnl.TargetFound;
                            bnl.ViablePaths.AddRange(newbnl.ViablePaths);
                        }
                        else
                        {
                            //arriving at front
                            var testbnlB = SearchForBlock(currentBlock, targetBlock, traversedBlocks, to.Connectbname, to.Ident, "", branchLevel + 1);
                            if (testbnlB.TargetFound)
                            {
                                var viablePath = FlattenLog(traversedBlocks, branchLevel+1);
                                bnl.ViablePaths.AddRange(testbnlB.ViablePaths);
                                bnl.TargetFound = true;
                            }
                            traversedBlocks.RemoveAt(branchLevel + 1);
                            var testbnlC = SearchForBlock(currentBlock, targetBlock, traversedBlocks, to.Connectcname, to.Ident, "", branchLevel+1);
                            if (testbnlC.TargetFound)
                            {
                                var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                                bnl.ViablePaths.AddRange(testbnlC.ViablePaths);
                                bnl.TargetFound = true;
                            }
                            traversedBlocks.RemoveAt(branchLevel + 1);
                        }
                        bnl.Breadcrumb += nextItemIdent + ";";
                    }
                }
                //turnout still in same block, keep going


            }
            else if (LayoutItem.Substring(0, 1) == "T")
            {
                //track
                var ts = config.GetLayoutTracksegment(LayoutItem);
                if (ts.Blockname != currentBlock)
                {
                    traversedBlocks[branchLevel].Add(currentBlock);
                    currentBlock = ts.Blockname;
                }
                var blockLog = FlattenLog(traversedBlocks, branchLevel - 1);
                if (ts.Blockname == targetBlock)
                {
                    bnl.TargetFound = true;
                    traversedBlocks[branchLevel].Add(ts.Blockname);
                    bnl.ValidBlockPath.Add(ts.Blockname);
                    bnl.ViablePaths.Add(FlattenLog(traversedBlocks, branchLevel));
                }
                else if (blockLog.Contains(ts.Blockname))
                {
                    bnl.TargetFound = false;
                }
                else
                {
                    var nextItem = ts.Connect2name;
                    if (ts.Connect2name == previousLayoutItem) nextItem = ts.Connect1name;
                    bnl.Breadcrumb += nextItem + ";";
                    var newbnl = SearchForBlock(currentBlock, targetBlock, traversedBlocks, nextItem, ts.Ident, "", branchLevel);
                    bnl.Breadcrumb += newbnl.Breadcrumb;
                    bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                    bnl.ValidBlockPath.AddRange(newbnl.ValidBlockPath);
                    bnl.TargetFound = newbnl.TargetFound;
                    bnl.EdgeConnector = newbnl.EdgeConnector;
                    bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                    bnl.NextBlockEdgeConnector = newbnl.NextBlockEdgeConnector;
                    bnl.ViablePaths.AddRange(newbnl.ViablePaths);
                }
            }
            else if (LayoutItem.Substring(0, 1) == "A")
            {
                //anchor
                var a = config.GetTrackLayoutAnchorPoint(LayoutItem);
                var nextItem = a.Connect2name;
                if (a.Connect2name == previousLayoutItem) nextItem = a.Connect1name;
                bnl.Breadcrumb += nextItem + ";";
                var newbnl = SearchForBlock(currentBlock, targetBlock, traversedBlocks, nextItem, a.Ident, "", branchLevel);
                bnl.Breadcrumb += newbnl.Breadcrumb;
                bnl.NoMoreBlocksFound = newbnl.NoMoreBlocksFound;
                bnl.ValidBlockPath.AddRange(newbnl.ValidBlockPath);
                bnl.TargetFound = newbnl.TargetFound;
                bnl.EdgeConnector = newbnl.EdgeConnector;
                bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                bnl.NextBlockEdgeConnector = newbnl.NextBlockEdgeConnector;
                bnl.ViablePaths.AddRange(newbnl.ViablePaths);
            }
            else if (LayoutItem.Substring(0, 2) == "SL")
            {
                //slip

                var slip = config.GetSlip(LayoutItem);

                if (slip.Blockname != currentBlock)
                {  
                    traversedBlocks[branchLevel].Add(currentBlock);     
                    currentBlock = slip.Blockname;
                }
                var blockLog = FlattenLog(traversedBlocks, branchLevel - 1);
                if (slip.Blockname == targetBlock)
                {
                    bnl.TargetFound = true;
                    bnl.ValidBlockPath.Add(slip.Blockname);
                    traversedBlocks[branchLevel].Add(slip.Blockname);
                    bnl.ViablePaths.Add(FlattenLog(traversedBlocks, branchLevel));
                    traversedBlocks[branchLevel].Add(slip.Blockname);
                }
                else if (blockLog.Contains(slip.Blockname))
                {
                    bnl.TargetFound = false;
                }
                else
                {
                    if (slip.Connectaname == previousLayoutItem || slip.Connectbname == previousLayoutItem)
                    {
                        //Approaching from...
                        //Route is through C or D
                        var testbnlC = SearchForBlock(currentBlock, targetBlock, traversedBlocks, slip.Connectcname, slip.Ident, "", branchLevel+1);
                        if (testbnlC.TargetFound)
                        {
                            var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                            bnl.ViablePaths.AddRange(testbnlC.ViablePaths);
                            bnl.TargetFound = true;
                        }
                        traversedBlocks.RemoveAt(branchLevel + 1);
                        var testbnlD = SearchForBlock(currentBlock, targetBlock, traversedBlocks, slip.Connectdname, slip.Ident, "", branchLevel+1);
                        if (testbnlD.TargetFound)
                        {
                            var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                            bnl.ViablePaths.AddRange(testbnlD.ViablePaths);
                            bnl.TargetFound = true;
                        }
                        traversedBlocks.RemoveAt(branchLevel + 1);

                    }
                    else
                    {
                        var testbnlA = SearchForBlock(currentBlock, targetBlock, traversedBlocks, slip.Connectaname, slip.Ident, "", branchLevel+1);
                        if (testbnlA.TargetFound)
                        {
                            var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                            bnl.ViablePaths.AddRange(testbnlA.ViablePaths);
                            bnl.TargetFound = true;
                        }
                        traversedBlocks.RemoveAt(branchLevel + 1);
                        var testbnlB = SearchForBlock(currentBlock, targetBlock, traversedBlocks, slip.Connectbname, slip.Ident, "", branchLevel+1);
                        if (testbnlB.TargetFound)
                        {
                            var viablePath = FlattenLog(traversedBlocks, branchLevel + 1);
                            bnl.ViablePaths.AddRange(testbnlB.ViablePaths);
                            bnl.TargetFound = true;
                        }
                        traversedBlocks.RemoveAt(branchLevel + 1);
                    }
                }
            }
            else if (LayoutItem.Substring(0, 2) == "EB")
            {
                //end bumper - end of the line
                bnl.NoMoreBlocksFound = true;

                //can only go backwards from here
                bnl.EdgeConnector = previousLayoutItem;
                bnl.EdgeConnectorDirectionConnector = LayoutItem;
                bnl.BlockFound = "";
                bnl.LikelyIssue = "No more blocks";
            }
            return bnl;
        }
        public List<string> FlattenLog(List<List<string>> list, int toLevel)
        {
            List<string> result = new List<string>();
            for (int i = 0; i <= toLevel;i++)
            {
                result.AddRange(list[i]);
            }
            return result;
        }

        private void btnMoveTrain_Click(object sender, EventArgs e)
        {
            if (lbStartBlocks.SelectedIndex < 0 || lbDestinationBlocks.SelectedIndex < 0) return;

            ViableRoutes = new List<ViableRouteList>();

            dynamic sb = lbStartBlocks.SelectedItem;
            string db = lbDestinationBlocks.SelectedItem as string;

            if (sb == null || db == null) return;

            string sbv = sb.Value as string;

            var sourceBlock = config.GetBlockBySystemName(sbv);
            var destinationBlock = config.GetBlockByUserName(db);

            var connectingBlocks = GetAllConnectedBlocks(sourceBlock.userName);

            foreach (var cb in connectingBlocks)
            {
                var emptyBC = new List<List<string>>();
                var bnl = GetFirstBNL(sourceBlock.userName, cb);
                var result = SearchForBlock(sourceBlock.userName, destinationBlock.userName, emptyBC, bnl.EdgeConnector, bnl.EdgeConnectorDirectionConnector, "", 0);
                foreach (var route in result.ViablePaths)
                {
                    var vrbList = ProcessViableRoute(route);
                    var vrl = new ViableRouteList();
                    vrl.Blocks = vrbList;
                    var unAv = vrbList.Count(c => c.IsAvailable == false);
                    vrl.NumberOfUnavailableBlocks = unAv;
                    ViableRoutes.Add(vrl);
                }
            }

            ViableRoutes = ViableRoutes.OrderBy(o => o.Blocks.Count).ThenBy(t => t.NumberOfUnavailableBlocks).ToList();
            routeIndex = 0;
            PopulateRoute();

        }

        private List<ViableRouteBlock> ProcessViableRoute(List<string> Route)
        {
            var blockList = new List<ViableRouteBlock>();

            foreach (var block in Route)
            {
                var vrb = new ViableRouteBlock();
                vrb.Blockname = block;
                var blockName = block;

                var liveBlock = _allBlocks.FirstOrDefault(f => f.data.userName == block);
                if (liveBlock != null)
                {
                    if (liveBlock.data != null && liveBlock.data.state == 4)
                    {
                        //unoccupied
                        vrb.IsAvailable = true;
                    }
                    else
                    {
                        vrb.IsAvailable = false;
                        blockName = "(X) "+blockName;
                    }
                    vrb.IsAvailable = liveBlock.data.state == 4 ? true : false;
                }
                vrb.Displayname = blockName;
                blockList.Add(vrb);
            }
            return blockList;
        }

        private List<string> GetAllConnectedBlocks(string blockName)
        {
            List<string> result = new List<string>();

            string connector1 = "";
            string connector2 = "";
            string previousConnector = "";
            string breadcrumbStart = "";

            List<Positionablepoint> anchorPoints = new List<Positionablepoint>();
            List<Layoutturnout> turnouts = new List<Layoutturnout>();

            var trackSegments = config.GetTracksegmentsForBlock(blockName).OrderBy(o => o.Ident).ToList();

            foreach (var ts in trackSegments)
            {
                if (ts.Connect1name.StartsWith("A"))
                {
                    var ap = config.GetTrackLayoutAnchorPoint(ts.Connect1name);
                    anchorPoints.Add(ap);
                }
                if (ts.Connect2name.StartsWith("A"))
                {
                    var ap = config.GetTrackLayoutAnchorPoint(ts.Connect2name);
                    anchorPoints.Add(ap);
                }
                if (ts.Connect1name.StartsWith("TO"))
                {
                    var turnout = config.GetLayuoutTurnout(ts.Connect1name);
                    turnouts.Add(turnout);
                }
                if (ts.Connect2name.StartsWith("TO"))
                {
                    var turnout = config.GetLayuoutTurnout(ts.Connect2name);
                    turnouts.Add(turnout);
                }
            }

            List<string> blockNames = new List<string>();
            foreach (var ap in anchorPoints)
            {
                var ts1 = config.GetLayoutTracksegment(ap.Connect1name);
                var ts2 = config.GetLayoutTracksegment(ap.Connect2name);
                if (!blockNames.Contains(ts1.Blockname) && ts1.Blockname != blockName)
                    blockNames.Add(ts1.Blockname);
                if (!blockNames.Contains(ts2.Blockname) && ts2.Blockname != blockName)
                    blockNames.Add(ts2.Blockname);
            }

            if (blockNames.Count == 0)
            {
                foreach (var to in turnouts)
                {
                    if (!blockNames.Contains(to.Blockname) && to.Blockname != blockName)
                    {
                        blockNames.Add(to.Blockname);
                    }
                }
            }

            return blockNames;
        }

        private void btnRoutePrev_Click(object sender, EventArgs e)
        {
            routeIndex--;
            PopulateRoute();
        }

        private void btnRouteAccept_Click(object sender, EventArgs e)
        {
            List<SectionJourneyLog> sections = new List<SectionJourneyLog>();
            var log = new LiveJourneyLog();

            var route = ViableRoutes.ElementAtOrDefault(routeIndex);
            var transit = config.BuildTransitFromBlockList(route.Blocks);
            transit.Type = TransitType.Generated;
            ViableRoutes = new List<ViableRouteList>();
            lbRoute.Items.Clear();

            TrainDirection dir = TrainDirection.Forward;
            if (cbTransitTrainDirection.Text == "Reverse")
                dir = TrainDirection.Reverse;
            StartAutoTrain(transit, dir, DateTime.Now);
        }

        private void btnRouteNext_Click(object sender, EventArgs e)
        {
            if (routeIndex >=0)
            {

            }
            routeIndex++;
            PopulateRoute();
        }



        private void PopulateRoute()
        {
            lbRoute.Items.Clear();
            var route = ViableRoutes.ElementAtOrDefault(routeIndex);
            if (route == null)
            {
                btnRoutePrev.Enabled = false;
                btnRouteNext.Enabled = false;
                btnRouteAccept.Enabled = false;
                lblRoute.Text = "Route";
                return;
            }

            btnRouteAccept.Enabled = true;
            lblRoute.Text = "Route "+(routeIndex+1).ToString()+ " / "+ViableRoutes.Count.ToString() +" ("+route.NumberOfUnavailableBlocks.ToString()+")";
            foreach (var ap in route.Blocks)
            {
                lbRoute.Items.Add(ap.Displayname);
            }

            if (routeIndex > 0)
            {
                btnRoutePrev.Enabled = true;
            }
            else
                btnRoutePrev.Enabled = false;

            if (ViableRoutes.Count > routeIndex + 1)
                btnRouteNext.Enabled = true;
            else
                btnRouteNext.Enabled = false;
        }

        private void lbStartBlocks_SelectedIndexChanged(object sender, EventArgs e)
        {

            var sbIndex = lbStartBlocks.SelectedIndex;
            dynamic sb = lbStartBlocks.SelectedItem;
            if (sb == null) return;

            var sbName = sb.Value as string;
            var startBlock = _startBlocks.FirstOrDefault(f => f.data.name == sbName);

            var dccId = startBlock.data.value.data.userName;
            var re = c.Roster.FirstOrDefault(f => f.ID == dccId);
            if (re != null)
            {
                var robject = new
                {
                    Name = re.Name + " (" + re.ID + ")",
                    DCCID = re.ID
                };
                var rIndex = lbRoster.Items.IndexOf(robject);
                if (rIndex > -1)
                {
                    lbRoster.SelectedIndex = rIndex;
                }
            }
        }

        private async void ProcessBlocksToDecorate()
        {
            var queue = blocksToDecorate.OrderBy(o => o.Position).ToList();
            var issueEncountered = false;
            var first = queue.FirstOrDefault();
            try
            {
                if (first != null)
                {
                    if (first.SetToAlternate)
                    {
                        await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + first.BlockUserName, first.BlockUserName, false);
                    }
                    else
                    {
                        await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + first.BlockUserName, first.BlockUserName, false);
                    }
                }

            }
            catch (Exception ex)
            {
                WriteToLog("block decorate MQTT exception "+first.BlockUserName+" - " + ex.Message);
                issueEncountered = true;
            }
            if (!issueEncountered)
            {
                blocksToDecorate.Remove(first);
            }
        }

        private async void btnStopTransit_Click(object sender, EventArgs e)
        {
            if (lbRunningTransits.SelectedIndex >= 0)
            {
                dynamic rt = lbRunningTransits.SelectedItem as dynamic;
                var dccId = rt.Value;
                var log = _logs.FirstOrDefault(f => f.DCCiD == dccId && f.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete);
                if (log == null) return;

                log.TrainMotionCfg.IsActive = false;
                log.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Cancelled;
                log.StatusLastChanged = DateTime.Now;

                foreach (var b in log.AllocatedBlocks)
                {
                    blocksToDecorate.Add(new BlockToDecorate
                    {
                        SetToAlternate = false,
                        BlockUserName = b,
                        Position = 1
                    });
                    var bl = config.GetBlockByUserName(b);
                    await webClient.AllocateBlock(bl.systemName, "");
                }
                lblActiveTransitID.Text = "";
                lblActiveTransitName.Text = "";
                lblSignalAspect.Text = "";
                lblSignalReason.Text = "";
                lblSpeed.Text = "";
                lblSpeedReason.Text = "";
                lblSpeedStep.Text = "";

            }
        }

        private async void ManageStationAutomation(List<BlockRootObject> LiveBlocks)
        {
            if (!sam.StationManagementACRunning && !sam.StationManagementCWRunning) return;
            var acDiff = DateTime.Now - sam.LastACSensorCheck;
            var cwDiff = DateTime.Now - sam.LastCWSensorCheck;

            if (acDiff.TotalMilliseconds > 2000)
            {
                var acTriggerSensor = await webClient.GetSensor("AC SA TriggerNextTrain");
                if (sam.StationManagementACRunning && acTriggerSensor != null && acTriggerSensor.data.state == 2)
                {
                    var yardLines = config.GetYardSections();
                    var acYardLines = yardLines.Where(w => w.userName.Contains("AC")).ToList();
                    var searchResult = FindUsableLaunchBlock(acYardLines, sam.LastACLaunchAttemptSectionIndex, LiveBlocks);
                    sam.LastACLaunchAttemptSectionIndex = searchResult.index;
                    WriteToLog("AC line index now " + sam.LastACLaunchAttemptSectionIndex.ToString());

                    if (searchResult.found)
                    {
                        var fullInfo = rosterConfig.FullRoster.FirstOrDefault(f => f.DccAddress == searchResult.DCCID);

                        var isFreightProp = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "IsFreight");
                        if (isFreightProp != null && isFreightProp.Value.ToString().ToUpper() == "TRUE")
                        {
                            var acTransit = PrepareSATransit(ACSAFreightTransit, searchResult.name);
                            StartAutoTrain(acTransit, TrainDirection.Forward, DateTime.Now);
                        }
                        else
                        {
                            var acTransit = PrepareSATransit(ACSAYardTransit, searchResult.name);
                            StartAutoTrain(acTransit, TrainDirection.Forward, DateTime.Now);
                        }

                        await webClient.SetSensor("AC SA TriggerNextTrain", "4");
                    }
                }
                sam.LastACSensorCheck = DateTime.Now;
            }

            if (cwDiff.TotalMilliseconds > 2000)
            {
                var cwTriggerSensor = await webClient.GetSensor("CW SA TriggerNextTrain");

                if (sam.StationManagementCWRunning && cwTriggerSensor != null && cwTriggerSensor.data.state == 2)
                {
                    var yardLines = config.GetYardSections();
                    var cwLines = yardLines.Where(w => w.userName.Contains("SA") && w.userName.Contains("CW")).ToList();
                    var searchResult = FindUsableLaunchBlock(cwLines, sam.LastCWLaunchAttemptSectionIndex, LiveBlocks);
                    sam.LastCWLaunchAttemptSectionIndex = searchResult.index;
                    WriteToLog("CW line index now " + sam.LastCWLaunchAttemptSectionIndex.ToString());
                    if (searchResult.found)
                    {
                        if (searchResult.name == "Yard CW Line 5 Block 1")
                        {
                            if (searchResult.isFreight)
                            {
                                var cw5FreightTransit = config.GetTransit(CWSAYard5FreightTransit, DispatcherPath);
                                cw5FreightTransit.Type = TransitType.StationAutomation;
                                StartAutoTrain(cw5FreightTransit, TrainDirection.Forward, DateTime.Now);
                            }
                            else
                            {
                                var cw5Transit = config.GetTransit(CWSAYard5Transit, DispatcherPath);
                                cw5Transit.Type = TransitType.StationAutomation;
                                StartAutoTrain(cw5Transit, TrainDirection.Forward, DateTime.Now);
                            }
                        }
                        else
                        {
                            if (searchResult.isFreight)
                            {
                                var cwTransit = PrepareSATransit(CWSAYardFreightTransit, searchResult.name);
                                cwTransit.Type = TransitType.StationAutomation;
                                StartAutoTrain(cwTransit, TrainDirection.Forward, DateTime.Now);
                            }
                            else
                            {
                                var cwTransit = PrepareSATransit(CWSAYardTransit, searchResult.name);
                                cwTransit.Type = TransitType.StationAutomation;
                                StartAutoTrain(cwTransit, TrainDirection.Forward, DateTime.Now);
                            }

                        }

                        await webClient.SetSensor("CW SA TriggerNextTrain", "4");
                    }
                }
                sam.LastCWSensorCheck = DateTime.Now;
            }                    
        }

        private transit PrepareSATransit(string transitName, string startBlockName)
        {
            var newTransit = config.GetTransit(transitName,DispatcherPath);
            var replacementBlock = config.GetBlockByUserName(startBlockName);
            //big assumption here that a section exists with the same name, containing only this block
            var replacementSection = config.GetSectionByUserName(startBlockName);
            if (replacementSection == null)
            {
                WriteToLog("Auto launch failed - section " + startBlockName + " could not be found");
            }
            newTransit.transitsection[0] = new transitTransitsection()
            {
                alternate = "no",
                sectionname = replacementSection.systemName
            };
            var oldSec = newTransit.Sections[0];
            newTransit.Sections[0] = new SectionJourneyLog()
            {
                HasAlternate = false,
                PossibleAlternate = false,
                BlockBNLs = new List<BlockNavigationLog>(),
                Blocks = new List<block>(),
                IsStorage = false,
                SectionkUserName = replacementSection.userName,
                AllocationStatus = AllocationStatus.NotAllocated,
                SectionID = oldSec.SectionID,
                SectionSystemname = replacementSection.systemName,
                Sequence = oldSec.Sequence
            };
            newTransit.Sections[0].Blocks.Add(replacementBlock);

            var oldBlock = newTransit.BlocksInOrder[0];
            newTransit.BlocksInOrder[0] = new BlockJourneyLog()
            {
                HasAlternate = false,
                BlockLengthMM = replacementBlock.length,
                BlockSystemname = replacementBlock.systemName,
                BlockUserName = replacementBlock.userName,
                PossibleAlternate = false,
                OccupationSensorSystemName = replacementBlock.occupancysensor,
                SectionId = oldSec.SectionID,
                Sequence = oldBlock.Sequence,
                SectionSequenceId = oldBlock.SectionSequenceId,
                SpeedLog = new List<SpeedStepLog>()
            };

            newTransit.Type = TransitType.StationAutomation;
            newTransit.StartBlock = replacementBlock.userName;
            return newTransit;
        }

        private (bool found, string name, string DCCID, int index, bool isFreight) FindUsableLaunchBlock(List<section> YardLines, int index, List<BlockRootObject> LiveBlocks)
        {
            index++;
            var lineToAttempt = YardLines.ElementAtOrDefault(index);
            if (lineToAttempt == null)
            {
                index = 0;
                lineToAttempt = YardLines.ElementAtOrDefault(index);
            }

            if (lineToAttempt == null) return (false,"","",index,false);
            var endBlock = lineToAttempt.blockentry.LastOrDefault();
            if (endBlock == null) return (false, "","",index, false);
            var liveBlock = LiveBlocks.FirstOrDefault(f => f.data.name == endBlock.sName);
            if (liveBlock == null) return (false, "","",index, false);

            if (liveBlock.data.state == 2 && liveBlock.data.value != null && !string.IsNullOrEmpty(liveBlock.data.value.data.userName))
            {
                var dccIdFound = liveBlock.data.value.data.userName;
                var alreadyRunning = _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete).Any(a => a.DCCiD == dccIdFound);

                var fullInfo = rosterConfig.FullRoster.FirstOrDefault(f => f.DccAddress == dccIdFound);

                if (fullInfo != null && fullInfo.Attributepairs != null)
                {
                    var saUsable = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerSAUsable");
                    var saIsFreight = fullInfo.Attributepairs.Keyvaluepair.FirstOrDefault(f => f.Key == "ShuttlerIsFreight");

                    var isFreight = false;
                    if (saIsFreight != null && saIsFreight.Value != null && (string)saIsFreight.Value == "True")
                        isFreight = true;
                    if (saUsable != null)
                    {
                        if (!alreadyRunning)
                        {
                            return (true, liveBlock.data.userName, dccIdFound, index,isFreight);
                        }
                        else
                        {
                            WriteToLog("Loco " + dccIdFound + " usable but already running so skipping");
                        }
                    }
                    else
                    {
                        WriteToLog("Loco " + dccIdFound + " not usable so skipping");
                    }
                }         
            }

            return (false, "","",index, false);

        }

        private async void btnStartStationAutomation_Click(object sender, EventArgs e)
        {
            if (webClient == null) return;
            btnStartStationAutomation.BackColor = Color.Green;
            btnStartStationAutomation.Enabled = false;
            sam.StationManagementACRunning = true;
            btnStopStationAutomation.Enabled = true;
            sam.LastACLaunchAttemptSectionIndex = -1;

            await webClient.SetSensor("AC SA TriggerNextTrain", "2");
            sam.LastACSensorCheck = DateTime.Now;

        }

        private void btnStopStationAutomation_Click(object sender, EventArgs e)
        {
            btnStartStationAutomation.BackColor = Color.Gray;
            btnStartStationAutomation.Enabled = true;
            sam.StationManagementACRunning = false;
            btnStopStationAutomation.Enabled = false;
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            //await webClient.SetSensor("ISIS11107", "2");
            await webClient.SetTurnout("MT2001", 2);
        }

        private void btnCopyLogToClipboard_Click(object sender, EventArgs e)
        {
            StringBuilder output = new StringBuilder();
            foreach (var line in lbOutput.Items)
            {
                output.AppendLine(line.ToString());
            }
            if (output != null &&  output.Length > 0)
            {
                try
                {
                    Clipboard.SetText(output.ToString());
                }
                catch (Exception ex)
                {
                    WriteToLog("Error copying text to clipboard");
                }
            }
                
        }

        private async void btnStartClockwiseSA_Click(object sender, EventArgs e)
        {
            if (webClient == null) return;
            btnStartClockwiseSA.BackColor = Color.Green;
            btnStartClockwiseSA.Enabled = false;
            sam.StationManagementCWRunning = true;
            btnStopClockwiseSA.Enabled = true;
            sam.LastCWLaunchAttemptSectionIndex = -1;

            await webClient.SetSensor("CW SA TriggerNextTrain", "2");
            sam.LastCWSensorCheck = DateTime.Now;
        }

        private void btnStopClockwiseSA_Click(object sender, EventArgs e)
        {
            btnStartClockwiseSA.BackColor = Color.LightGray;
            btnStartClockwiseSA.Enabled = true;
            sam.StationManagementCWRunning = false;
            btnStopClockwiseSA.Enabled = false;
        }

        private async void PurgeLateBlockValues(List<BlockRootObject> blocks)
        {
            var assignedBlocks = blocks.Where(w => w.data.value != null && !string.IsNullOrEmpty(w.data.value.data.userName) && w.data.state == (int)BlockState.Unoccupied);
            var allocatedBlocks = new List<string>();
            foreach (var log in _logs.Where(w => w.AutomatedTrainRunningStatus != AutomatedTrainRunningStatus.ReadyToDelete))
            {
                allocatedBlocks.AddRange(log.AllocatedBlocks);
            }
            
            foreach (var ass in assignedBlocks)
            {
                if (!allocatedBlocks.Contains(ass.data.userName))
                {                    
                    var resp = await webClient.AllocateBlock(ass.data.name, "");
                    if (resp.data.value == null || string.IsNullOrEmpty(resp.data.value.data.userName))
                    {
                        WriteToLog("Possible rogue block value " + ass.data.value.data.userName + " in " + ass.data.userName + " - removed");
                    }
                    else
                    {
                        WriteToLog("Possible rogue block value " + ass.data.value.data.userName + " in " + ass.data.userName + " - removal failed, value now "+resp.data.value.data.userName);
                    }
                }
            }
        }
    }
}
