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
        private int cautiomBlockPercentToBeginRampDown;
        private int dangerBlockPercentToBeginRampDown;

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
            cautiomBlockPercentToBeginRampDown = 75;
            dangerBlockPercentToBeginRampDown = 50;
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            c = new WiThrottle(_JMRIServerIP, _WiThrottlePort, "Shuttler");
            if (webClient == null && c != null && c.WebServerPort > -1)
            {
                var serverAddress = "http://" + _JMRIServerIP + ":" + c.WebServerPort.ToString();
                webClient = new JSONReader(serverAddress);
                LoadStartBlocks();
                _allBlocks = await webClient.GetBlocks();
            }
            LoadConfig();

            var emptyBC = new List<List<string>>();
            var bnl = GetFirstBNL("Yard AC Line 1 Block 1", "AC Yard Exit");
            var result = SearchForBlock("Yard AC Line 1 Block 1", "UD-AC Station Bay Platform", emptyBC, bnl.EdgeConnector, bnl.EdgeConnectorDirectionConnector,"",0);
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
                            var posInSequence = relatedLog.AutomatedBlockList.IndexOf(sequenceBlock);
                            if (posInSequence > -1)
                            {
                                relatedLog.AutomatedBlockList.ElementAt(posInSequence + 1).PreviousBlockExited = true;
                            }
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

                        logBlock.TimeTrainEnteredBlock = DateTime.Now;
                        logBlock.mmCovered = 0;
                        logBlock.SequenceState = JourneySequenceState.Active;
                        logBlock.SpeedLog = new List<SpeedStepLog>();

                        existingLog.AutomatedCurrentBlockIndex = existingLog.AutomatedBlockList.IndexOf(logBlock);

                        var allocatedBlockToRemove= existingLog.AllocatedBlocks.FirstOrDefault(w => w == logBlock.BlockUserName);
                        if (allocatedBlockToRemove != null)
                        {
                            existingLog.AllocatedBlocks.Remove(allocatedBlockToRemove);
                        }

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
                    ProcessBlocksToDecorate();

                    UpdateLogPanel();
                    _allBlocks = newBlockStates;
                }

                await c.CheckForMessages();
                await Task.Delay(500);
            }

        }

        private void CalculateSpeedForTrains()
        {
            foreach (var log in _logs.ToList())
            {
                if (log.TrainMotionCfg == null) return;

                if (!log.TrainMotionCfg.IsActive && log.TrainMotionCfg.CurrentSpeedStep > 0)
                {
                    log.TrainMotionCfg.TargetSpeedStep = 0;
                    log.TrainMotionCfg.RequiredSpeedStep = 0;
                    log.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                    log.AutomatedTrainSpeedReason = "Manually cancelled";
                    return;
                }

                var timeSinceStarted = DateTime.Now - log.TimeStarted;
                if (timeSinceStarted.TotalSeconds < 10)
                {
                    log.TrainMotionCfg.CurrentSpeedStep = 0;
                    log.TrainMotionCfg.TargetSpeedStep = 0;
                    return;
                }

                //get speed setting and set that first - this will be superceded by signal based speed
                //switch (log.SignalAspect)
                //{
                //    case SignalAspect.Danger:
                //        log.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Stop;
                //        break;
                //    case SignalAspect.Caution:
                //        log.AutomatedTrainRunningSpeed = AutomatedTrainRunningSpeed.Caution;
                //        break;
                //}

                bool emergencyStopRequiremd = false;
                int targetSpeedRequired = 0;
                decimal actualSpeedMMSRequired = 0.0M;
                switch (log.AutomatedTrainRunningSpeed)
                {
                    case AutomatedTrainRunningSpeed.Stop:
                        emergencyStopRequiremd = true;
                        targetSpeedRequired = 0;
                        break;
                    case AutomatedTrainRunningSpeed.Crawl:
                        targetSpeedRequired = log.TrainMotionCfg.TrainDirection == TrainDirection.Forward
                            ? log.TrainMotionCfg.ForwardCrawlSpeedStep : log.TrainMotionCfg.ReverseCrawlSpeedStep;
                        actualSpeedMMSRequired = log.TrainMotionCfg.TrainDirection == TrainDirection.Forward
                            ? log.TrainMotionCfg.ForwardCrawlMMS : log.TrainMotionCfg.ReverseCrawlMMS;
                        break;
                    case AutomatedTrainRunningSpeed.Caution:
                        targetSpeedRequired = log.TrainMotionCfg.TrainDirection == TrainDirection.Forward
                            ? log.TrainMotionCfg.ForwardCautionSpeedStep : log.TrainMotionCfg.ReverseCautionSpeedStep;
                        actualSpeedMMSRequired = log.TrainMotionCfg.TrainDirection == TrainDirection.Forward
                            ? log.TrainMotionCfg.ForwardCautionMMS : log.TrainMotionCfg.ReverseCautionMMS;
                        break;
                    default:
                        targetSpeedRequired = log.TrainMotionCfg.TrainDirection == TrainDirection.Forward
                            ? log.TrainMotionCfg.ForwardFullSpeedStep : log.TrainMotionCfg.ReverseFullSpeedStep;
                        actualSpeedMMSRequired = log.TrainMotionCfg.TrainDirection == TrainDirection.Forward
                            ? log.TrainMotionCfg.ForwardFullSpeedMMS : log.TrainMotionCfg.ReverseFullSpeedMMS;
                        break;                                          
                }

                if (targetSpeedRequired > log.TrainMotionCfg.CurrentSpeedStep)
                {
                    log.TrainMotionCfg.InRampDown = false;
                    log.TrainMotionCfg.InRampUp = true;
                }
                else if (targetSpeedRequired < log.TrainMotionCfg.CurrentSpeedStep)
                {
                    log.TrainMotionCfg.InRampDown = true;
                    log.TrainMotionCfg.InRampUp = false;
                }

                log.TrainMotionCfg.TargetSpeedStep = targetSpeedRequired;

                int actualSpeedRequired = targetSpeedRequired;

                if (targetSpeedRequired != log.TrainMotionCfg.CurrentSpeedStep)
                {
                    var timeSinceLastChange = DateTime.Now - log.TrainMotionCfg.RampSpeedLastSet;
                    if (log.TrainMotionCfg.InRampUp)
                    {
                        if (timeSinceLastChange.TotalMilliseconds > log.TrainMotionCfg.RampUpIntervalMS)
                        {
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
                            if (actualSpeedRequired <= targetSpeedRequired)
                            {
                                actualSpeedRequired = targetSpeedRequired;
                                log.TrainMotionCfg.InRampDown = false;
                            }
                            log.TrainMotionCfg.RampSpeedLastSet = DateTime.Now;
                        }
                        log.TrainMotionCfg.InRampUp = false;
                    }
                }

                log.TrainMotionCfg.TargetSpeedStep = targetSpeedRequired;
                log.TrainMotionCfg.RequiredSpeedStep = actualSpeedRequired;

                if (emergencyStopRequiremd)
                {
                    log.TrainMotionCfg.TargetSpeedStep = 0;
                    log.TrainMotionCfg.CurrentSpeedStep = 0;
                }

                log.TrainMotionCfg.CurrentSpeedMMS = actualSpeedMMSRequired;

                //then set signal based speed



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
                //pos = (scale / frac) * 100;
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
            foreach (var log in _logs.ToList())
            {
                if (log.TrainMotionCfg.RequiredSpeedStep != log.TrainMotionCfg.CurrentSpeedStep)
                {
                    var re = c.Roster.FirstOrDefault(f => f.ID == log.DCCiD);
                    var rosterIndex = c.Roster.IndexOf(re);
                    c.SetThrottleSpeedStep(rosterIndex, log.TrainMotionCfg.RequiredSpeedStep);
                    log.TrainMotionCfg.CurrentSpeedStep = log.TrainMotionCfg.RequiredSpeedStep;


                    //get relative position of new speed step
                    var rosterCfG = new RosterReader(RosterPath);
                    var roster = rosterCfG.GetRoster();
                    var fullInfo = rosterCfG.FullRoster.FirstOrDefault(f => f.DccAddress == log.DCCiD);
                    decimal interimMMS = 0.0M;

                    if (fullInfo != null && fullInfo.Speedprofile != null)
                    {
                        var firstSpeed = fullInfo.Speedprofile.Speeds.Speed.FirstOrDefault();
                        if (firstSpeed != null)
                        {
                            decimal prevForwardSpeed = 0.0M;
                            decimal prevReverseSpeed = 0.0M;
                            decimal prevStep = 0.0M;
                            var decFSuccess = decimal.TryParse(firstSpeed.Forward, out prevForwardSpeed);
                            var decRSuccess = decimal.TryParse(firstSpeed.Reverse, out prevReverseSpeed);
                            //bool fsSuccess = decimal.TryParse(firstSpeed.Step, out prevStep);
                            var mmPerSecond = 0.0M;
                            foreach (var step in fullInfo.Speedprofile.Speeds.Speed)
                            {
                                var tStep = step.Step;
                                var speed = step.Forward;
                                decimal dForward = 0.0M;
                                decimal dReverse = 0.0M;
                                decimal dStep = 0.0M;
                                decimal prevReverseSpeedMM = 0.0M;
                                decimal prevForwardSpeedMM = 0.0M;

                                bool fSuccess = decimal.TryParse(step.Forward, out dForward);
                                bool rSuccess = decimal.TryParse(step.Reverse, out dReverse);
                                bool dSuccess = decimal.TryParse(step.Step, out dStep);

                                if (fSuccess && rSuccess && dSuccess)
                                {

                                    if ((log.TrainMotionCfg.RequiredSpeedStep < dStep && log.TrainMotionCfg.RequiredSpeedStep > prevStep))
                                    {
                                        var percent = GetRelativeSpeedStepPosition(log.TrainMotionCfg.RequiredSpeedStep, prevStep, dStep);
                                        if (log.TrainMotionCfg.TrainDirection == TrainDirection.Forward)
                                        {
                                            mmPerSecond = GetRelativeSpeedMM(percent, dForward, prevForwardSpeedMM);
                                        }
                                        else
                                        {
                                            mmPerSecond = GetRelativeSpeedMM(percent, dReverse, prevReverseSpeedMM);
                                        }
                                    }
                                }
                                prevStep = dStep;
                                prevReverseSpeedMM = dReverse;
                                prevForwardSpeedMM = dForward;
                            }
                            if (mmPerSecond > 0.0M)
                            {
                                var activeBlock = log.AutomatedBlockList.ElementAtOrDefault(log.AutomatedCurrentBlockIndex);
                                if (activeBlock != null)
                                {
                                    activeBlock.SpeedLog.Add(new SpeedStepLog()
                                    {
                                        SpeedMMS = mmPerSecond,
                                        start = DateTime.Now,
                                        SpeedStep = log.TrainMotionCfg.RequiredSpeedStep
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void CheckRunningTrains(List<BlockRootObject> LiveBlocks)
        {
            foreach (var log in _logs.ToList())
            {
                if (!log.TrainMotionCfg.IsActive)
                {
                    return;
                }
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

                    //mm covered
                    decimal mmCoveredSoFar = 0.0M;
                    for (int b =  0; b < thisLogBlock.SpeedLog.Count; b++)
                    {
                        var dateTimeTo = DateTime.Now;
                        if (b+1 < thisLogBlock.SpeedLog.Count)
                        {
                            dateTimeTo = thisLogBlock.SpeedLog.ElementAt(b + 1).start;
                        }

                        var timeDiff = dateTimeTo - thisLogBlock.SpeedLog.ElementAt(b).start;
                        mmCoveredSoFar += thisLogBlock.SpeedLog.ElementAt(b).SpeedMMS * (decimal)timeDiff.TotalSeconds;
                        thisLogBlock.mmCovered = mmCoveredSoFar;
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
                        thisLogSectionBlock.AutomatedSpeedReason = "Default speed";
                    }

                    var lengthMM = thisLogBlock.BlockLengthMM;
                    var traversedSoFarMM = thisLogBlock.mmCovered;
                    decimal percentageOfBlockTraversed = 100.0M;

                    if (lengthMM != null && lengthMM > 0 && traversedSoFarMM > 0)
                    {
                        percentageOfBlockTraversed = (traversedSoFarMM / lengthMM) * 100;
                    }

                    if (log.AutomatedTrainRunningSpeed == AutomatedTrainRunningSpeed.Caution && !log.TrainMotionCfg.InRampDown && percentageOfBlockTraversed > cautiomBlockPercentToBeginRampDown 
                        && thisLogSectionBlock.SignalAspect == SignalAspect.Caution && blockCounter == 1)
                    {
                        thisLogSectionBlock.BlockSpeed = AutomatedTrainRunningSpeed.Crawl;
                        thisLogSectionBlock.AutomatedSpeedReason = "Towards end of caution block and approaching danger";
                    }

                    if (log.AutomatedTrainRunningSpeed == AutomatedTrainRunningSpeed.Crawl && percentageOfBlockTraversed > dangerBlockPercentToBeginRampDown
                            && thisLogSectionBlock.SignalAspect == SignalAspect.Danger && blockCounter == 1)
                    {
                        thisLogSectionBlock.BlockSpeed = AutomatedTrainRunningSpeed.Stop;
                        thisLogSectionBlock.AutomatedSpeedReason = "Danger block time to stop";
                    }

                    if (log.AutomatedTrainRunningSpeed != thisLogSectionBlock.BlockSpeed && blockCounter == 1)
                    {
                        WriteToLog("Speed change required for " + log.Name + " from " + log.AutomatedTrainRunningSpeed.ToString() + " to " + thisLogSectionBlock.BlockSpeed.ToString() + " - " + thisLogSectionBlock.AutomatedSpeedReason);
                        log.AutomatedTrainRunningSpeed = thisLogSectionBlock.BlockSpeed;
                        log.AutomatedTrainSpeedReason = thisLogSectionBlock.AutomatedSpeedReason;
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
                                        var correspondingLogBlock = log.AutomatedBlockList.FirstOrDefault(f => f.BlockSystemname == block.systemName && f.SectionSequenceId == section.Sequence);
                                        blocksToDecorate.Add(new BlockToDecorate
                                        {
                                            SetToAlternate = true,
                                            BlockUserName = block.userName,
                                            Position = log.AutomatedBlockList.IndexOf(correspondingLogBlock)
                                        });
                                        await webClient.AllocateBlock(block.systemName, log.DCCiD);
                                        //await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + block.userName, block.userName, false);
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteToLog("MQTT Error - " + ex.Message);
                                        allocateFailedAnywhere = true;
                                    }
                                    WriteToLog("Allocated block " + block.userName + " to " + log.Name);
                                    log.AllocatedBlocks.Add(block.userName);
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

        private void StartAutoTrain(List<string> Blocks)
        {

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
            trainLog.TrainMotionCfg = new TrainMotionConfig();
            trainLog.AllocatedBlocks = new List<string>();
            trainLog.AutomatedSectionList = transit.Sections;
            trainLog.AutomatedCurrentSectionIndex = 0;
            trainLog.TrainMotionCfg.TrainDirection = TrainDirection.Forward;
            if (cbTransitTrainDirection.SelectedText == "Reverse")
                trainLog.TrainMotionCfg.TrainDirection = TrainDirection.Reverse;


            var startBlock = transit.StartBlock;

            var thisLiveStartBlock = _allBlocks.FirstOrDefault(f => f.data.userName == startBlock);
            if (thisLiveStartBlock == null || thisLiveStartBlock.data.value == null) return;

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


            trainLog.TrainMotionCfg.Name = trainLog.Name;
            trainLog.TrainMotionCfg.DCCID = trainLog.DCCiD;

            if (fullInfo != null && fullInfo.Speedprofile != null)
            {
                var firstSpeed = fullInfo.Speedprofile.Speeds.Speed.FirstOrDefault();
                if (firstSpeed != null)
                {
                    decimal prevForwardSpeed = 0.0M;
                    decimal prevReverseSpeed = 0.0M;
                    decimal prevStep = 0.0M;
                    //var decFSuccess = decimal.TryParse(firstSpeed.Forward, out prevForwardSpeed);
                    //var decRSuccess = decimal.TryParse(firstSpeed.Reverse, out prevReverseSpeed);
                    //bool fsSuccess = decimal.TryParse(firstSpeed.Step, out prevStep);

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
                            if ((DefaultCrawlMMS < dForward && DefaultCrawlMMS > prevForwardSpeed))
                            {

                                var calc = GetRelativeSpeedPercentage(DefaultCrawlMMS, prevForwardSpeed, dForward);
                                var calculatedMMS = prevForwardSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                trainLog.TrainMotionCfg.ForwardCrawlSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ForwardCrawlMMS = calculatedMMS;
                            }
                            if ((DefaultCrawlMMS < dReverse && DefaultCrawlMMS > prevReverseSpeed))
                            {

                                var calc = GetRelativeSpeedPercentage(DefaultCrawlMMS, prevReverseSpeed, dReverse);
                                var calculatedMMS = prevReverseSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                trainLog.TrainMotionCfg.ReverseCrawlSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ReverseCrawlMMS = calculatedMMS;
                            }


                            if ((DefaultCautionMMS < dForward && DefaultCautionMMS > prevForwardSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(DefaultCautionMMS,prevForwardSpeed,dForward);
                                var calculatedMMS = prevForwardSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage,prevStep,dStep);

                                trainLog.TrainMotionCfg.ForwardCautionSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ForwardCautionMMS = calculatedMMS;
                            }
                            if ((DefaultCautionMMS < dReverse && DefaultCautionMMS > prevReverseSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(DefaultCautionMMS, prevReverseSpeed, dReverse);
                                var calculatedMMS = prevReverseSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                trainLog.TrainMotionCfg.ReverseCautionSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ReverseCautionMMS = calculatedMMS;
                            }

                            if ((DefaultFullSpeedMMS < dForward && DefaultFullSpeedMMS > prevForwardSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(DefaultFullSpeedMMS, prevForwardSpeed, dForward);
                                var calculatedMMS = prevForwardSpeed + calc.speed;
                                var calculatedSpeedStep = GetRelativeSpeedStep(calc.percentage, prevStep, dStep);

                                trainLog.TrainMotionCfg.ForwardFullSpeedStep = calculatedSpeedStep;
                                trainLog.TrainMotionCfg.ForwardFullSpeedMMS = calculatedMMS;
                            }
                            if ((DefaultFullSpeedMMS < dReverse && DefaultFullSpeedMMS > prevReverseSpeed))
                            {
                                var calc = GetRelativeSpeedPercentage(DefaultFullSpeedMMS, prevReverseSpeed, dReverse);
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


                //get speed val
                //divide by 1000 then x by 128

            }
            else
            {
                decimal speed = new decimal(DefaultCrawlMMS);
                var asPerc1 = speed / 1000;
                trainLog.TrainMotionCfg.ForwardCrawlSpeedStep = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
                trainLog.TrainMotionCfg.ReverseCrawlSpeedStep = trainLog.TrainMotionCfg.ForwardCrawlSpeedStep;
                trainLog.TrainMotionCfg.ForwardCrawlMMS = DefaultCrawlMMS;
                trainLog.TrainMotionCfg.ReverseCrawlMMS = DefaultCrawlMMS;

                speed = DefaultCautionMMS;
                asPerc1 = speed / 1000;
                trainLog.TrainMotionCfg.ForwardCautionSpeedStep = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
                trainLog.TrainMotionCfg.ReverseCautionSpeedStep = trainLog.TrainMotionCfg.ForwardCautionSpeedStep;
                trainLog.TrainMotionCfg.ForwardCautionMMS = DefaultCautionMMS;
                trainLog.TrainMotionCfg.ReverseCautionMMS = DefaultCautionMMS;

                speed = DefaultFullSpeedMMS;
                asPerc1 = speed / 1000;
                trainLog.TrainMotionCfg.ForwardFullSpeedStep = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
                trainLog.TrainMotionCfg.ReverseFullSpeedStep = trainLog.TrainMotionCfg.ForwardFullSpeedStep;
                trainLog.TrainMotionCfg.ForwardFullSpeedMMS = DefaultFullSpeedMMS;
                trainLog.TrainMotionCfg.ReverseFullSpeedMMS = DefaultFullSpeedMMS;
            }

            trainLog.TrainMotionCfg.RampUpSpeedStepIncrease = 1;
            trainLog.TrainMotionCfg.RampUpIntervalMS = 200;
            trainLog.TrainMotionCfg.RampDownIntervalMS = 200;
            trainLog.TrainMotionCfg.RampDownSpeedStepDecrease = 2;


            trainLog.TrainMotionCfg.CurrentSpeedStep = 0;
            trainLog.TrainMotionCfg.TargetSpeedStep = 0;
            trainLog.TrainMotionCfg.IsActive = true;

            trainLog.TrainMotionCfg = trainLog.TrainMotionCfg;
            trainLog.TimeStarted = DateTime.Now;

            string mtIndex = c.GetThrottle(rosterIndex);
            var newThrottle = new Throttle();
            newThrottle.mtIndex = mtIndex;
            newThrottle.RosterIndex = rosterIndex;
            newThrottle.ID = trainLog.DCCiD;

            trainLog.TransitName = transit.userName;

            _logs.Add(trainLog);

            lbRunningTransits.Items.Add(new
            {
                Name = transit.userName + " (" + trainLog.DCCiD + ")",
                Value = trainLog.DCCiD
            });

            WriteToLog("Started transit " + transit.userName + " for train " + trainLog.Name); 
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
                //pos = (scale / frac) * 100;
                pos = (percentage / 100) * scale;
            //then add the pos to the ... step?
            //int requiredSpeedStep = (int)decimal.Round( prevStep + pos,0,MidpointRounding.AwayFromZero);
            var requiredStep =  prevStep + pos;
            var asPerc1 = requiredStep / 1000;
            var beforeRound = asPerc1 * 128;
            int dSS = (int)decimal.Round((asPerc1 * 128), 0, MidpointRounding.AwayFromZero);
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
                lblSpeedStep.Text = log.TrainMotionCfg.CurrentSpeedStep.ToString()+" / "+log.TrainMotionCfg.TargetSpeedStep.ToString();


                var logBlock = log.AutomatedBlockList.ElementAtOrDefault(log.AutomatedCurrentBlockIndex);
                if (logBlock != null)
                {
                    lblBlockLength.Text = logBlock.BlockLengthMM.ToString();
                    lblMmCoveredThisBlock.Text = logBlock.mmCovered.ToString();
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
            //bnl.PreviousBlock = previousBlock;
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
                    //bnl.ValidBlockPath.AddRange(traversedBlocks);
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
                    //bnl.ValidBlockPath.AddRange(traversedBlocks);
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
                        if (slip.Ident.Contains("approach"))
                        {
                            var test = "stop here";
                        }
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
                        if (slip.Ident.Contains("approach"))
                        {
                            var test = "stop here";
                        }
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

                        //if (testbnlA.TargetFound || testbnlB.TargetFound)
                        //{
                        //    bnl.ViablePaths.Add(FlattenLog(traversedBlocks, branchLevel + 1));
                        //}

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
                vrb.Blockname = blockName;
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
                lbRoute.Items.Add(ap.Blockname);
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
                var log = _logs.FirstOrDefault(f => f.DCCiD == dccId);
                if (log == null) return;

                log.TrainMotionCfg.IsActive = false;

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

                _logs.Remove(log);
                lbRunningTransits.Items.Clear();
                lblActiveTransitID.Text = "";
                lblActiveTransitName.Text = "";
                lblSignalAspect.Text = "";
                lblSignalReason.Text = "";
                lblSpeed.Text = "";
                lblSpeedReason.Text = "";
                lblSpeedStep.Text = "";
                foreach (var remainingLog in _logs.ToList())
                {
                    lbRunningTransits.Items.Add(new
                    {
                        Name = remainingLog.TransitName + " (" + remainingLog.DCCiD + ")",
                        Value = remainingLog.DCCiD
                    });
                }
            }
        }
    }
}
