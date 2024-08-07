using JMRIReader;
using JMRIReader.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiThrottleClient;
using WiThrottleClient.Classes;
using static JMRIReader.Classes.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace Shuttler
{
    public partial class Shuttler : Form
    {
        private string _JMRIServerIP;
        private int _WiThrottlePort;
        private string _cfgFilePath;
        private string RosterPath;
        private ConfigReader config;
        private JSONReader webClient;
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
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            
        }

        private async void RunShuttles()
        {
            c = new WiThrottle(_JMRIServerIP, _WiThrottlePort, "Shuttler");
            lbRoster.Items.Clear();
            lbRoster.DisplayMember = "Name";
            lbRoster.ValueMember = "DCCID";

            while (_isRunning)
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
                if (webClient == null && c!= null && c.WebServerPort > -1)
                {
                    var serverAddress = "http://" + _JMRIServerIP + ":" + c.WebServerPort.ToString();
                    webClient = new JSONReader(serverAddress);
                    LoadStartBlocks();
                    _allBlocks = await webClient.GetBlocks();
                }

                if (webClient != null && c != null && _allBlocks != null)
                {
                    var newBlockStates = await webClient.GetBlocks();
                    var newActiveBlocks = newBlockStates.Where(w => w.data.state == 2).ToList();
                    var oldActiveBlocks = _allBlocks.Where(w => w.data.state == 2).ToList();                    

                    var activeBlocks = oldActiveBlocks.Union(newActiveBlocks).ToList();
                    var newActiveThisTimeBlocks = newActiveBlocks.Where(p => !oldActiveBlocks.Any(p2 => p2.data.name == p.data.name)).ToList();

                    var goneInactiveBlocks = oldActiveBlocks.Where(x => !newActiveBlocks.Select(i => i.data.name).Contains(x.data.name));

                    foreach (var gib in goneInactiveBlocks)
                    {
                        if (gib.data.value == null) continue;
                        var relatedLog = _logs.FirstOrDefault(f => f.DCCiD == gib.data.value.data.userName);
                        if (relatedLog == null) continue;
                        var sequenceBlock = relatedLog.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == gib.data.name && f.SequenceState == JourneySequenceState.Active);
                        if (sequenceBlock != null)
                        {
                            sequenceBlock.SequenceState = JourneySequenceState.Traversed;
                            WriteToLog("Block " + sequenceBlock.BlockUserName + " exited");
                        }
                    }

                    foreach (var nab in newActiveThisTimeBlocks)
                    {
                        var nextBlockName = "";
                        var existingLog = _logs.FirstOrDefault(f => f.NextBlock == nab.data.userName);
                        if (existingLog == null)
                        {
                            continue;
                        }

                        var activeSection = existingLog.AutomatedSectionList.ElementAtOrDefault(existingLog.AutomatedCurrentSectionIndex);


                        //if new block is in current section


                        var activeBlock = activeSection.Blocks.FirstOrDefault(f => f.userName == nab.data.userName);
                        if (activeBlock == null)
                        {
                            //entered new active section
                            activeSection = existingLog.AutomatedSectionList.ElementAtOrDefault(existingLog.AutomatedCurrentSectionIndex + 1);
                            existingLog.AutomatedCurrentSectionIndex++;
                            activeSection.IsTraversed = true;
                            activeBlock = activeSection.Blocks.FirstOrDefault(f => f.userName == nab.data.userName);
                            WriteToLog("New active section " + activeSection.SectionkUserName + " - block " + activeBlock.userName + " section index now at " + existingLog.AutomatedCurrentSectionIndex.ToString());
                        }
                        if (activeBlock != null)
                        {
                            //should be 0 but hey ho
                            var abIndex = activeSection.Blocks.IndexOf(activeBlock);
                            if (abIndex > -1)
                            {
                                if (abIndex + 1 < activeSection.Blocks.Count)
                                {
                                    var nextBlock = activeSection.Blocks.ElementAtOrDefault(abIndex+1);
                                    if (nextBlock != null)
                                    {
                                        nextBlockName = nextBlock.userName;
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
                                            nextBlockName = nextBlock.userName;
                                        }
                                    }
                                }
                            }

                            existingLog.AutomatedTrainRunningSpeed = activeBlock.BlockSpeed;
                            existingLog.AutomatedTrainSpeedReason = activeBlock.AutomatedSpeedReason;
                        }

                        var logBlock = existingLog.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == nab.data.name && f.SectionSequenceId == activeSection.Sequence);

                        if (logBlock == null)
                        {
                            logBlock = existingLog.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == nab.data.name && f.SectionSequenceId == existingLog.AutomatedCurrentSectionIndex + 1);
                            if (logBlock.SequenceState != JourneySequenceState.Queued)
                            {
                                //flickering? This block has already been processed as a new block.
                                WriteToLog(nab.data.userName + " detected as new block but already procedded - ignored as it's probably flickering");
                                continue;
                            }
                        }

                        if (logBlock == null)
                        {
                            //last section? shouldn't get here but just in case
                            logBlock = existingLog.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == nab.data.name && f.SequenceState == JourneySequenceState.Queued);
                        }

                        logBlock.SequenceState = JourneySequenceState.Active;

                        existingLog.AutomatedCurrentBlockIndex = existingLog.AutomatedBlockList.IndexOf(logBlock);

                        var newBlockBNL = NavigateThroughBlockItems(nab.data.userName, nextBlockName, existingLog.CurrentBlockBNL.EdgeConnector, existingLog.CurrentBlockBNL.EdgeConnectorDirectionConnector, "");
                        existingLog.CurrentBlock = nab.data.userName;
                        existingLog.NextBlock = newBlockBNL.BlockFound;
                        existingLog.PreviousBlockBNL = existingLog.CurrentBlockBNL;
                        existingLog.CurrentBlockBNL = newBlockBNL;

                        WriteToLog("New block " + nab.data.userName + " block index "+existingLog.AutomatedCurrentBlockIndex.ToString()+ " for train " + existingLog.Name + " next block " + existingLog.NextBlock 
                            + " speed "+existingLog.AutomatedTrainRunningSpeed.ToString()+" - reason "+existingLog.AutomatedTrainSpeedReason);
                    }
                    CheckRunningTrains(newBlockStates);
                    CalculateSpeedForTrains();
                    SetTrainSpeeds();

                    UpdateLogPanel();
                    _allBlocks = newBlockStates;
                }

                await c.CheckForMessages();
                await Task.Delay(500);
            }

        }

        private void CalculateSpeedForTrains()
        {
            foreach (var log in _logs)
            {
                //Get current block

                //Check speed matches speed set on 

                var signalAspect = log.SignalAspect;
                var expectedSpeed = log.AutomatedTrainRunningSpeed;

                if (log.TargetTrainSpeedStep != log.TrainSpeedStep)
                {
                    //is it ramping? Check ramp
                }
            }
        }

        private void SetTrainSpeeds()
        {

        }

        private async void CheckRunningTrains(List<BlockRootObject> LiveBlocks)
        {
            foreach (var log in _logs)
            {
                int sectionCounter = 0;
                int blockCounter = 0;
                var previousBlockBNL = log.PreviousBlockBNL;

                //check blocks for issues
                List<block> CheckedBlocks = new List<block>();

                if (!string.IsNullOrEmpty(log.PreviousBlock))
                {
                    var livePreviousBlock = LiveBlocks.FirstOrDefault(f => f.data.userName == log.PreviousBlock);
                }

                int blocksRemaining = log.AutomatedBlockList.Count - log.AutomatedCurrentBlockIndex;
                if (blocksRemaining > 3) blocksRemaining = 3;

                for (int i = log.AutomatedCurrentBlockIndex; i < log.AutomatedCurrentBlockIndex + blocksRemaining; i++)
                {
                    bool requiresCustomSpeedValue = false;
                    blockCounter++;

                    var thisLogBlock = log.AutomatedBlockList[i];
                    var thisLogSection = log.AutomatedSectionList.ElementAtOrDefault(thisLogBlock.SectionSequenceId);
                    var thisLogSectionBlock = thisLogSection.Blocks.FirstOrDefault(f => f.systemName == thisLogBlock.BlockSystemname);

                    var nextBlock = log.AutomatedBlockList.ElementAtOrDefault(i + 1);

                    if (thisLogSectionBlock.BNL == null)
                    {
                        if (nextBlock != null)
                        {
                            thisLogSectionBlock.BNL = NavigateThroughBlockItems(thisLogBlock.BlockUserName, nextBlock.BlockUserName, previousBlockBNL.EdgeConnector, previousBlockBNL.EdgeConnectorDirectionConnector, "");
                        }
                    }

                    thisLogSectionBlock.ClearToAllocate = true;

                    if (log.AutomatedTrainRunningSpeed != thisLogSectionBlock.BlockSpeed && blockCounter == 1)
                    {
                        WriteToLog("Speed change required for " + log.Name + " from " + log.AutomatedTrainRunningSpeed.ToString() + " to " + thisLogSectionBlock.BlockSpeed.ToString() + " - " + thisLogSectionBlock.AutomatedSpeedReason);
                        log.AutomatedTrainRunningSpeed = thisLogSectionBlock.BlockSpeed;
                        log.AutomatedTrainSpeedReason = thisLogSectionBlock.AutomatedSpeedReason;
                    }


                    string issue = "";

                    //previous speed restrictions first as they should be superceded by current block speed restrictions
                    var previousBlocksStillActive = log.AutomatedBlockList.Where(w => w.SequenceState == JourneySequenceState.Active && w.Sequence < thisLogBlock.Sequence);
                    var previousActiveBlockHasSpeedRestriction = false;
                    var previousActiveSpeedRestrictionReason = "";
                    var previousSpeedRestriction = AutomatedTrainRunningSpeed.Caution;
                    foreach (var pb in previousBlocksStillActive)
                    {
                        var bSection = log.AutomatedSectionList.FirstOrDefault(f => f.Sequence == pb.SectionSequenceId);
                        if (bSection == null) continue;
                        var cBlock = bSection.Blocks.FirstOrDefault(f => f.systemName == pb.BlockSystemname);
                        if (cBlock == null) continue;
                        if (cBlock.BlockSpeed != AutomatedTrainRunningSpeed.Full)
                        {
                            previousActiveBlockHasSpeedRestriction = true;
                            previousActiveSpeedRestrictionReason = "Previous block " + cBlock.userName + " still active and has speed " + cBlock.AutomatedSpeedReason + " for " + cBlock.AutomatedSpeedReason;
                            previousSpeedRestriction = cBlock.BlockSpeed;
                        }
                    }

                    if (previousActiveBlockHasSpeedRestriction)
                    {
                        requiresCustomSpeedValue = true;
                        thisLogSectionBlock.BlockSpeed = previousSpeedRestriction;
                        thisLogSectionBlock.AutomatedSpeedReason = previousActiveSpeedRestrictionReason;
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
                            thisLogSectionBlock.BlockSpeed = AutomatedTrainRunningSpeed.Caution;
                            thisLogSectionBlock.AutomatedSpeedReason = "Thrown turnout in path ("+to.Name+")";
                            requiresCustomSpeedValue = true;
                        }
                    }

                    if (nextBlock != null)
                    {
                        bool nextBlockHasThrownTurnout = false;
                        var nextBlockSection = log.AutomatedSectionList.ElementAtOrDefault(nextBlock.SectionSequenceId);
                        var nextBlockSectionBlock = nextBlockSection.Blocks.FirstOrDefault(f => f.systemName == nextBlock.BlockSystemname);
                        if (nextBlockSectionBlock != null && nextBlockSectionBlock.BNL != null && nextBlockSectionBlock.BNL.BNLTurnouts != null)
                        {
                            foreach (var to in nextBlockSectionBlock.BNL.BNLTurnouts)
                            {
                                if (to.RequiredState == "4")
                                {
                                    nextBlockHasThrownTurnout = true;
                                }
                            }
                        }
                        if (nextBlockHasThrownTurnout)
                        {
                            thisLogSectionBlock.BlockSpeed = AutomatedTrainRunningSpeed.Caution;
                            thisLogSectionBlock.AutomatedSpeedReason = "Thrown turnout in next block";
                            requiresCustomSpeedValue = true;
                        }
                    }

                    if (i == log.AutomatedBlockList.Count-1)
                    {
                        issue += "End of journey";
                    }

                    if (i == log.AutomatedBlockList.Count-2)
                    {
                        thisLogSectionBlock.BlockSpeed = AutomatedTrainRunningSpeed.Caution;
                        thisLogSectionBlock.AutomatedSpeedReason = "Penultimate block";
                        requiresCustomSpeedValue = true;
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

                    thisLogSectionBlock.CheckSequence = blockCounter;
                    CheckedBlocks.Add(thisLogSectionBlock);
                    previousBlockBNL = thisLogSectionBlock.BNL;



                    if (!requiresCustomSpeedValue && thisLogSectionBlock.AutomatedSpeedReason != "Approaching end of journey")
                    {
                        thisLogSectionBlock.BlockSpeed = AutomatedTrainRunningSpeed.Full;
                        thisLogSectionBlock.AutomatedSpeedReason = "Default apeed";
                    }
                }

                //calculate aspect here?
                //if next next block contains danger then caution
                if (CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(2) != null && CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(2).BlockContainsDanger)
                {
                    //caution
                    log.SignalAspect = SignalAspect.Caution;
                    log.SignalAspectReason = CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(2).DangerReason;
                }

                if (CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(1) != null && CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(1).BlockContainsDanger)
                {
                    //danger
                    log.SignalAspect = SignalAspect.Danger;
                    log.SignalAspectReason = CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(1).DangerReason;
                }

                if (CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(0) != null && CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(0).BlockContainsDanger)
                {
                    //stop
                    log.SignalAspect = SignalAspect.Stop;
                    log.SignalAspectReason = CheckedBlocks.OrderBy(o => o.CheckSequence).ElementAtOrDefault(0).DangerReason;
                }

                var anyDanger = CheckedBlocks.Any(a => a.BlockContainsDanger);
                if (!anyDanger)
                {
                    log.SignalAspect = SignalAspect.Proceed;
                    log.SignalAspectReason = "";
                }


                //check sections for allocation and turnout setting

                bool previousSectionAllocated = true;
                for (int i = log.AutomatedCurrentSectionIndex;i <  log.AutomatedCurrentSectionIndex+ _sectionsAhead;i++)
                {
                    sectionCounter++;
                    var section = log.AutomatedSectionList.ElementAtOrDefault(i);
                    if (section == null) continue;

                    foreach (var block in section.Blocks)
                    {
                        var liveStateBlock = LiveBlocks.FirstOrDefault(f => f.data.name == block.systemName);

                        if (block.BNL != null)
                        {
                            block.BNL = NavigateThroughBlockItems(block.userName, block.BNL.BlockFound, block.BNL.UsedEdgeConnector, block.BNL.UsedEdgeConnectorDirectionConnector, "");
                        }
                        else continue;

                        if (liveStateBlock != null)
                        {
                            string issue = "";
                            bool allocationIssueFound = false;

                            var state = liveStateBlock.data.state;
                            var value = liveStateBlock.data.value != null ? liveStateBlock.data.value.data.userName : "";
                            if (state == 2 && sectionCounter > 1) //occupied
                            {
                                issue = "Occupied";
                                if (value.Length > 0) issue += " by " + value;
                                allocationIssueFound = true;
                            }
                            else
                            {
                                if (value.Length > 0 && value != log.DCCiD)
                                {
                                    //check for allocation                              
                                    issue = "Allocated to " + value;
                                    allocationIssueFound = true;
                                }
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
                        }
                    }

                    bool sectionContainsUnallocatableBlock = section.Blocks.Any(a => !a.ClearToAllocate);

                    if (!sectionContainsUnallocatableBlock && previousSectionAllocated)
                    {
                        bool allocateFailedAnywhere = false;

                        foreach (var block in section.Blocks)
                        {
                            var liveStateBlock = LiveBlocks.FirstOrDefault(f => f.data.name == block.systemName);

                            if (sectionCounter > 1 && (liveStateBlock.data.value == null || liveStateBlock.data.value.data.userName != log.DCCiD))
                            {
                                if (!section.IsAllocated)
                                {
                                    try
                                    {
                                        await webClient.AllocateBlock(block.systemName, log.DCCiD);
                                        await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + block.userName, block.userName, false);
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteToLog("MQTT Error - " + ex.Message);
                                        allocateFailedAnywhere = true;
                                    }
                                    WriteToLog("Allocated block " + block.userName + " to " + log.Name);
                                }
                            }

                            if (liveStateBlock.data.value != null && liveStateBlock.data.value.data.userName == log.DCCiD)
                            {
                                //not occupied and not allocated - set turnouts
                                //Check all turnouts even if allocated - one may have been set by human error
                                if (block.BNL != null && block.BNL.BNLTurnouts != null)
                                {
                                    foreach (var to in block.BNL.BNLTurnouts)
                                    {
                                        var liveTurnout = c.Turnouts.FirstOrDefault(f => f.ID == to.ID);
                                        if (liveTurnout != null)
                                        {
                                            to.CurrentState = liveTurnout.State;
                                        }

                                        if (to.RequiredState != to.CurrentState && to.NumberOfRetries > 5)
                                        {
                                            try
                                            {
                                                var liveTO = await webClient.GetTurnout(to.ID);
                                                to.CurrentState = liveTO.data.state.ToString();
                                                c.SetTurnout(to.ID, liveTO.data.state); 
                                            }
                                            catch (Exception ex)
                                            {
                                                WriteToLog("Live turnout update failed - " + to.Name);
                                            }
                                        }
                                        if (to.RequiredState != null && (to.CurrentState == null || to.CurrentState != to.RequiredState))
                                        {
                                            to.NumberOfRetries++;
                                            c.SetTurnout(to.ID, int.Parse(to.RequiredState));
                                            WriteToLog("Set turnout " + to.Name + " to required state " + to.RequiredState);
                                        }
                                    }
                                }

                            }
                        }
                        if (!allocateFailedAnywhere)
                            section.IsAllocated = true;
                        else
                            previousSectionAllocated = false;
                    }
                    else
                        previousSectionAllocated = false;

                    //Go down to caution in penultimate section
                    var position = log.AutomatedSectionList.IndexOf(section);
                    var positionRelative = log.AutomatedSectionList.Count - position;


                    if (positionRelative <= 2)
                    {
                        foreach (var block in section.Blocks)
                        {
                            block.BlockSpeed = AutomatedTrainRunningSpeed.Caution;
                            block.AutomatedSpeedReason = "Approaching end of journey";
                        }
                    }
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


        }

        private void LoadAvailableTransits()
        {
            if (_startBlocks == null) return;
            var transits = config.GetTransits();
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

            var index = lbRoster.SelectedIndex;
            c.GetThrottle(index);
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

        private void btnStartTransit_Click(object sender, EventArgs e)
        {
            dynamic transitItem = cbAvailableTransits.SelectedItem;
            if (transitItem == null) return;
            var tName = transitItem.Name;
            var tSysName = transitItem.Value;
            var transit = _transits.FirstOrDefault(f => f.systemName == tSysName);
            if (transit == null) return;

            LiveJourneyLog trainLog = new LiveJourneyLog();
            trainLog.AutomatedSectionList = transit.Sections;
            trainLog.AutomatedCurrentSectionIndex = 0;
            trainLog.AutomatedTrainDirection = TrainDirection.Forward;
            if (cbTransitTrainDirection.SelectedText == "Reverse")
                trainLog.AutomatedTrainDirection = TrainDirection.Reverse;


            var startBlock = transit.StartBlock;

            var thisLiveStartBlock = _allBlocks.FirstOrDefault(f => f.data.userName == startBlock);
            if (thisLiveStartBlock == null || thisLiveStartBlock.data.value == null) return;

            trainLog.DCCiD = thisLiveStartBlock.data.value.data.userName;
            var rosterEntry = c.Roster.FirstOrDefault(f => f.ID == trainLog.DCCiD);
            if (rosterEntry == null) return;
            trainLog.Name = rosterEntry.Name;

            //determine panel direction
            var nextBlock = transit.BlocksInOrder[1].BlockUserName;
            if (nextBlock == null) return;

            trainLog.CurrentBlock = transit.StartBlock;
            trainLog.NextBlock = nextBlock;

            trainLog.AutomatedBlockList = transit.BlocksInOrder;

            var firstBlockBNL =  GetFirstBNL(trainLog.CurrentBlock, trainLog.NextBlock);
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
                    var nextBlockInSequence = transit.BlocksInOrder.ElementAtOrDefault(seqIndex + 1);
                    if (nextBlockInSequence != null)
                        block.BNL = NavigateThroughBlockItems(block.userName, nextBlockInSequence.BlockUserName, prevBNL.EdgeConnector, prevBNL.EdgeConnectorDirectionConnector, "");
                    else
                        block.BNL = NavigateThroughBlockItems(block.userName, "", prevBNL.EdgeConnector, prevBNL.EdgeConnectorDirectionConnector, "");
                    prevBNL = block.BNL;
                }
            }

            trainLog.AutomatedTrainActive = true;
            trainLog.LastUpdated = DateTime.Now;
            trainLog.AutomatedCurrentSectionIndex = 0;
            trainLog.CurrentBlockBNL = firstBlockBNL;
            trainLog.StatusLastChanged = DateTime.Now;
            trainLog.AutomatedTrainRunningStatus = AutomatedTrainRunningStatus.Starting;

            //get train roster entry
            var rosterCfG = new RosterReader(RosterPath);
            var roster = rosterCfG.GetRoster();
            var fullInfo = rosterCfG.FullRoster.FirstOrDefault(f => f.DccAddress == trainLog.DCCiD);

            int crawlSpeedStep;
            int cautionSpeedStep;
            int FullSpeedStep;

            var tmc = new TrainMotionConfig();
            tmc.Name = trainLog.Name;
            tmc.DCCID = trainLog.DCCiD;

            if (fullInfo != null && fullInfo.Speedprofile != null)
            {
                var firstSpeed = fullInfo.Speedprofile.Speeds.Speed.FirstOrDefault();
                if (firstSpeed != null)
                {
                    decimal prevForwardSpeed = 0.0M;
                    decimal prevReverseSpeed = 0.0M;
                    var decFSuccess = decimal.TryParse(firstSpeed.Forward, out prevForwardSpeed);
                    var decRSuccess = decimal.TryParse(firstSpeed.Reverse, out prevReverseSpeed);
                    if (decFSuccess && decRSuccess)
                    {
                        foreach (var step in fullInfo.Speedprofile.Speeds.Speed)
                        {
                            var tStep = step.Step;
                            var speed = step.Forward;
                            decimal dForward = 0.0M;
                            decimal dReverse = 0.0M;
                            bool fSuccess = decimal.TryParse(step.Forward, out dForward);
                            bool rSuccess = decimal.TryParse(step.Reverse, out dReverse);
                            if (fSuccess && rSuccess)
                            {
                                if ((DefaultCrawlMMS < dForward && DefaultCrawlMMS > prevForwardSpeed) || (DefaultCrawlMMS < prevForwardSpeed && tmc.ForwardCrawlSpeedStep <= 0))
                                {
                                    var sSperc = step.Step;
                                    decimal dss = 0.0M;
                                    var dssSuccess = decimal.TryParse(step.Step, out dss);
                                    if (dssSuccess)
                                    {
                                        var asPerc1 = dss / 1000;
                                        
                                        int dSS = (int) decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
                                        tmc.ForwardCrawlSpeedStep = dSS;
                                    }

                                }
                                if ((DefaultCrawlMMS < dReverse && DefaultCrawlMMS > prevReverseSpeed) || (DefaultCrawlMMS < prevReverseSpeed && tmc.ReverseCrawlSpeedStep <= 0))
                                {
                                    var sSperc = step.Step;
                                    decimal dss = 0.0M;
                                    var dssSuccess = decimal.TryParse(step.Step, out dss);
                                    if (dssSuccess)
                                    {
                                        var asPerc1 = dss / 1000;

                                        int dSS = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
                                        tmc.ReverseCrawlSpeedStep = dSS;
                                    }

                                }


                                if ((DefaultCautionMMS < dForward && DefaultCautionMMS > prevForwardSpeed) || (DefaultCrawlMMS < prevForwardSpeed && tmc.ForwardCrawlSpeedStep <= 0))
                                {
                                    var sSperc = step.Step;
                                    decimal dss = 0.0M;
                                    var dssSuccess = decimal.TryParse(step.Step, out dss);
                                    if (dssSuccess)
                                    {
                                        var asPerc1 = dss / 1000;

                                        int dSS = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
                                        tmc.ForwardCautionSpeedStep = dSS;
                                    }

                                }
                                if ((DefaultCautionMMS < dReverse && DefaultCautionMMS > prevReverseSpeed) || (DefaultCrawlMMS < prevReverseSpeed && tmc.ReverseCautionSpeedStep <= 0))
                                {
                                    decimal dss = 0.0M;
                                    var dssSuccess = decimal.TryParse(step.Step, out dss);
                                    if (dssSuccess)
                                    {
                                        var asPerc1 = dss / 1000;

                                        int dSS = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
                                        tmc.ReverseCautionSpeedStep = dSS;
                                    }

                                }

                                if ((DefaultFullSpeedMMS < dForward && DefaultFullSpeedMMS > prevForwardSpeed))
                                {
                                    var sSperc = step.Step;
                                    decimal dss = 0.0M;
                                    var dssSuccess = decimal.TryParse(step.Step, out dss);
                                    if (dssSuccess)
                                    {
                                        var asPerc1 = dss / 1000;

                                        int dSS = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
                                        tmc.ForwardFullSpeedStep = dSS;
                                    }

                                }
                                if ((DefaultFullSpeedMMS < dReverse && DefaultFullSpeedMMS > prevReverseSpeed))
                                {
                                    decimal dss = 0.0M;
                                    var dssSuccess = decimal.TryParse(step.Step, out dss);
                                    if (dssSuccess)
                                    {
                                        var asPerc1 = dss / 1000;

                                        int dSS = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
                                        tmc.ReverseFullSpeedStep = dSS;
                                    }

                                }

                            }
                            prevForwardSpeed = dForward;
                            prevReverseSpeed = dReverse;
                        }
                    }
                }


                //get speed val
                //divide by 1000 then x by 128

            }

            trainLog.tmc = tmc;

            _logs.Add(trainLog);

            lbRunningTransits.Items.Add(new
            {
                Name = transit.userName + "(" + trainLog.DCCiD + ")",
                Value = trainLog.DCCiD
            });

            WriteToLog("Started transit " + transit.userName + " for train " + trainLog.Name); 
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
            //bnl.PreviousBlock = previousBlock;
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
                                blockFoundOnTurnoutSearch = testbnlB.BlockFound;
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
                var liveTA = c.Turnouts.FirstOrDefault(f => f.ID == cfgTurnoutA.systemName);// await webClient.GetTurnout(cfgTurnoutA.systemName);
                var liveTB = c.Turnouts.FirstOrDefault(f => f.ID == cfgTurnoutB.systemName);// await webClient.GetTurnout(cfgTurnoutB.systemName);
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
                            nextItem = slip.Connectdname;
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
                bnl.EdgeConnector = previousLayoutItem;
                bnl.EdgeConnectorDirectionConnector = LayoutItem;
                bnl.BlockFound = "";
                bnl.LikelyIssue = "No more blocks";
            }
            return bnl;
        }
        private void WriteToLog(string message)
        {
            var fullMess = DateTime.Now + " - " + message;
            lbOutput.Items.Add(fullMess);
            lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
        }

        private void UpdateLogPanel()
        {
            if (lbRunningTransits.SelectedIndex >=0)
            {
                dynamic rt = lbRunningTransits.SelectedItem as dynamic;
                var dccId = rt.Value;
                var log = _logs.FirstOrDefault(f => f.DCCiD == dccId);
                if (log == null) return;

                lblActiveTransitID.Text = log.DCCiD;
                lblActiveTransitName.Text = log.Name;
                lblSignalAspect.Text = log.SignalAspect.ToString();
                lblSignalReason.Text = log.SignalAspectReason;
                lblSpeed.Text = log.AutomatedTrainRunningSpeed.ToString();
                lblSpeedReason.Text = log.AutomatedTrainSpeedReason;

            }
        }

        private void lbRunningTransits_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
