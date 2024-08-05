using JMRIReader;
using JMRIReader.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiThrottleClient;
using WiThrottleClient.Classes;
using static System.Net.Mime.MediaTypeNames;

namespace Shuttler
{
    public partial class Shuttler : Form
    {
        private string _JMRIServerIP;
        private int _WiThrottlePort;
        private string _cfgFilePath;
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

                    foreach (var nab in newActiveThisTimeBlocks)
                    {
                        var nextBlockName = "";
                        var existingLog = _logs.FirstOrDefault(f => f.NextBlock == nab.data.userName);
                        if (existingLog == null)
                        {
                            continue;
                        }
                        var activeSection = existingLog.AutomatedSectionList.ElementAtOrDefault(existingLog.AutomatedCurrentSectionIndex);
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
                                
                        }

                        var newBlockBNL = NavigateThroughBlockItems(nab.data.userName, nextBlockName, existingLog.CurrentBlockBNL.EdgeConnector, existingLog.CurrentBlockBNL.EdgeConnectorDirectionConnector, "");
                        existingLog.CurrentBlock = nab.data.userName;
                        existingLog.NextBlock = newBlockBNL.BlockFound;
                        existingLog.CurrentBlockBNL = newBlockBNL;
                        WriteToLog("New block " + nab.data.userName + " for train " + existingLog.Name + " next block " + existingLog.NextBlock);
                    }
                    CheckRunningTrains(newBlockStates);
                    _allBlocks = newBlockStates;
                }

                await c.CheckForMessages();
                await Task.Delay(500);
            }

        }

        private async void CheckRunningTrains(List<BlockRootObject> LiveBlocks)
        {
            foreach (var log in _logs)
            {
                var lastBlock = "";
                var lastBlockNextBlock = "";
                var lastBlockEdgeConnector = "";
                var lastBlockEdgeDirectionConnector = "";
                int sectionCounter = 0;
                string sectionIssueLog = "";

                for (int i = log.AutomatedCurrentSectionIndex;i <  log.AutomatedCurrentSectionIndex+ _sectionsAhead;i++)
                {
                    sectionCounter++;
                    var section = log.AutomatedSectionList.ElementAtOrDefault(i);
                    if (section == null) continue;
                    if (section.BlockBNLs == null) section.BlockBNLs = new List<BlockNavigationLog>();
                    bool issueInAnySectionBlock = false;

                    foreach (var block in section.Blocks)
                    {
                        if (block.BNL != null)
                        {
                            var newBNL = NavigateThroughBlockItems(block.BNL.BlockChecked, block.BNL.BlockFound, block.BNL.UsedEdgeConnector, block.BNL.UsedEdgeConnectorDirectionConnector, "");

                            block.BNL = newBNL;
                            lastBlock = newBNL.BlockFound;
                            lastBlockNextBlock = newBNL.BlockFound;
                            lastBlockEdgeConnector = newBNL.EdgeConnector;
                            lastBlockEdgeDirectionConnector = newBNL.EdgeConnectorDirectionConnector; 
                        }
                        else
                        {
                            var nextBlockName = "";
                            var blockIndex = section.Blocks.IndexOf(block);
                            if (blockIndex > -1)
                            {
                                if (blockIndex+1 < section.Blocks.Count)
                                {
                                    var nextBlock = section.Blocks[blockIndex+1];
                                    if (nextBlock != null)
                                        nextBlockName = nextBlock.userName;
                                }
                                else
                                {
                                    //first block of next section for next block name
                                    if (i+1 < log.AutomatedSectionList.Count)
                                    {
                                        var nextSection = log.AutomatedSectionList.ElementAt(i + 1);
                                        if (nextSection != null)
                                        {
                                            var nextBlock = nextSection.Blocks.FirstOrDefault();
                                            if (nextBlock != null)
                                                nextBlockName = nextBlock.userName;
                                        }                                        
                                    }                                    
                                }
                            }
                            if (nextBlockName != "")
                            {
                                var bnl = NavigateThroughBlockItems(block.userName, nextBlockName, lastBlockEdgeConnector, lastBlockEdgeDirectionConnector, "");
                                if (bnl != null)
                                {
                                    block.BNL = bnl;
                                    lastBlock = bnl.BlockFound;
                                    lastBlockNextBlock = bnl.BlockFound;
                                    lastBlockEdgeConnector = bnl.EdgeConnector;
                                    lastBlockEdgeDirectionConnector = bnl.EdgeConnectorDirectionConnector;
                                }
                                
                            }
                            else
                            {
                                //End of transit
                                var bnl = NavigateThroughBlockItems(block.userName, "", lastBlockEdgeConnector, lastBlockEdgeDirectionConnector, "");
                                if (bnl != null)
                                {
                                    block.BNL = bnl;
                                    lastBlock = bnl.BlockFound;
                                    lastBlockNextBlock = bnl.BlockFound;
                                    lastBlockEdgeConnector = bnl.EdgeConnector;
                                    lastBlockEdgeDirectionConnector = bnl.EdgeConnectorDirectionConnector;
                                }
                            }
                        }

                        string issue = "";
                        var liveStateBlock = LiveBlocks.FirstOrDefault(f => f.data.name == block.systemName);

                        if (liveStateBlock != null)
                        {
                            var state = liveStateBlock.data.state;
                            var value = liveStateBlock.data.value != null ? liveStateBlock.data.value.data.userName : "";
                            if (state == 2 && sectionCounter > 1) //occupied
                            {                                
                                issue = "Occupied";
                                if (value.Length > 0) issue += " by " + value;
                                block.BNL.OccupiedBy = value;
                            }
                            else
                            {
                                block.BNL.OccupiedBy = "";
                                if (value.Length > 0 && value != log.DCCiD)
                                {
                                    //check for allocation                              

                                    issue = "Allocated to " + value;
                                    block.BNL.AllocatedTo = value;
                                }
                                else
                                {
                                    block.BNL.AllocatedTo = "";
                                }
                            }
                        }

                        if (issue.Length > 0)
                        {
                            block.BNL.LikelyIssue += "; " + issue;
                            issueInAnySectionBlock = true;
                            sectionIssueLog += "; " + block.BNL.LikelyIssue;
                        }

                        if ((string.IsNullOrEmpty(block.BNL.OccupiedBy) && string.IsNullOrEmpty(block.BNL.AllocatedTo)) || block.BNL.AllocatedTo == log.DCCiD)
                        {
                            if (sectionCounter > 1 && liveStateBlock.data.value == null)
                            {
                                await webClient.AllocateBlock(block.systemName, log.DCCiD);
                                await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + block.userName, block.userName, false);
                                WriteToLog("Allocated block " + block.userName + " to " + log.Name);
                            }
                            //not occupied and not allocated - set turnouts
                            foreach (var to in block.BNL.BNLTurnouts)
                            {
                                if (to.RequiredState != null && (to.CurrentState == null || to.CurrentState != to.RequiredState))
                                {
                                    c.SetTurnout(to.ID, int.Parse(to.RequiredState));
                                    WriteToLog("Set turnout " + to.Name + " to required state " + to.RequiredState);
                                }
                            }
                            section.IsAllocated = true;
                        }

                        //section.BlockBNLs.Add(block.BNL);
                    }

                    var position = log.AutomatedSectionList.IndexOf(section);
                    if (position == (log.AutomatedSectionList.Count-1))
                    {
                        issueInAnySectionBlock = true;
                        sectionIssueLog += "; End of journey";
                    }

                    if (issueInAnySectionBlock || sectionIssueLog.Length > 0) 
                    {
                        section.SignalAspectReason = sectionIssueLog;
                        switch (sectionCounter)
                        {
                            case 1:
                                //this section - stop train
                                section.SignalAspect = "Stop";
                                break;
                            case 2:
                                //next section - danger
                                section.SignalAspect = "Danger";
                                break;
                            case 3:
                                //two sections - caution
                                section.SignalAspect = "Caution";
                                break;

                        }
                        WriteToLog("Detected issue in section "+section.SectionkUserName+" - "+section.SignalAspect+" - "+section.SignalAspectReason);
                    }
                    else
                    {
                        section.SignalAspect = "Proceed";
                        section.SignalAspectReason = "";
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





            //var onBlocks = lbStartBlocks.Items.IndexOf(startBlock);
            //lbStartBlocks.SelectedIndex = onBlocks;

            //var dccId = lbStartBlocks.SelectedValue;

        }

        private async void btnStartTransit_Click(object sender, EventArgs e)
        {
            dynamic transitItem = cbAvailableTransits.SelectedItem;
            var tName = transitItem.Name;
            var tSysName = transitItem.Value;
            var transit = _transits.FirstOrDefault(f => f.systemName == tSysName);
            if (transit == null) return;

            LiveJourneyLog trainLog = new LiveJourneyLog();
            trainLog.AutomatedSectionList = transit.Sections;
            trainLog.AutomatedCurrentSectionIndex = 0;
            trainLog.AutomatedTrainDirection = "Forward";
            if (cbTransitTrainDirection.SelectedText == "Reverse")
                trainLog.AutomatedTrainDirection = "Reverse";


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

            var firstBlockBNL =  GetFirstBNL(trainLog.CurrentBlock, trainLog.NextBlock);

            if (firstBlockBNL == null) return;

            var secondBlockBNL = NavigateThroughBlockItems(firstBlockBNL.BlockFound, transit.BlocksInOrder[2].BlockUserName, firstBlockBNL.EdgeConnector, firstBlockBNL.EdgeConnectorDirectionConnector, firstBlockBNL.EdgeConnector);

            var firstSection = transit.Sections.FirstOrDefault();
            if (firstSection == null) return;

            firstSection.Blocks.First().BNL = firstBlockBNL;

            foreach (var block in firstSection.Blocks)
            {
                if (block.BNL != null)
                {
                    var newBNL = NavigateThroughBlockItems(block.BNL.BlockChecked,block.BNL.BlockFound,block.BNL.UsedEdgeConnector,block.BNL.UsedEdgeConnectorDirectionConnector, "");
                }
            }

            trainLog.AutomatedTrainActive = true;
            trainLog.LastUpdated = DateTime.Now;
            trainLog.AutomatedCurrentSectionIndex = 0;
            trainLog.CurrentBlockBNL = firstBlockBNL;
            _logs.Add(trainLog);

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

                if (derivedXoverBlockName != currentBlock && derivedXoverBlockName == nextBlock && to.Connectaname != breadcrumbStart && to.Connectbname != breadcrumbStart && to.Connectcname != breadcrumbStart && to.Connectdname != breadcrumbStart)
                {
                    bnl.EdgeConnector = to.Ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    bnl.BlockFound = to.Blockname;
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

                //Approaching from...



                /*
                if (slip.Connectaname == previousLayoutItem)
                {
                    //Approaching from A, need to make sure the first turnout on the slip is in our favour

                    //Approaching from A, the exit options are C and D
                    var reqioredState = slip.States.AC.Turnout;
                    bnltoA.RequiredState = slip.States.AC.Turnout;

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
                        bnltoA.RequiredState = "2";
                        if (liveTA.State == "2")
                        {
                            state = " CLOSED";
                            bnltoA.RequiredState = "4";
                        }
                        
                        if (slip.States.AC.Turnout == astate)
                        {
                            nextItem = slip.Connectcname;
                        }
                        else
                        {
                            nextItem = slip.Connectdname;
                        }
                        issueFound = liveTA.Name + state + " AGAINST";
                    }
                }
                else if (slip.Connectbname == previousLayoutItem)
                {
                    bnltoA.RequiredState = slip.States.BC.Turnout;
                    //Approaching from B, the exit options are C and D
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
                        bnltoA.RequiredState = "2";
                        if (liveTA.State == "2")
                        {
                            state = " CLOSED";
                            bnltoA.RequiredState = "4";
                        }
                        if (slip.States.BC.Turnout == astate)
                        {
                            nextItem = slip.Connectcname;
                        }
                        else
                        {
                            nextItem = slip.Connectdname;
                        }
                        issueFound = liveTA.Name + state + " AGAINST";
                    }
                }
                else if (slip.Connectcname == previousLayoutItem)
                {
                    bnltoB.RequiredState = slip.States.AC.TurnoutB;
                    //approaching from C or D, the exit options are A or B
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
                        bnltoB.RequiredState = "2";
                        if (liveTB.State == "2")
                        {
                            state = " CLOSED";
                            bnltoB.RequiredState = "4";
                        }
                        
                        if (slip.States.AC.TurnoutB == bstate)
                        {
                            nextItem = slip.Connectaname;
                        }
                        else
                        {
                            nextItem = slip.Connectbname;
                        }
                        issueFound = liveTB.Name + state + " AGAINST";
                    }
                }
                else if (slip.Connectdname == previousLayoutItem)
                {
                    bnltoB.RequiredState = slip.States.BD.TurnoutB;
                    //approaching from C or D, the exit options are A or B
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
                        bnltoB.RequiredState = "2";
                        if (liveTB.State == "2")
                        {
                            state = " CLOSED";
                            bnltoB.RequiredState = "4";
                        }
                        if (slip.States.AD.TurnoutB == bstate)
                        {
                            nextItem = slip.Connectaname;
                        }
                        else
                        {
                            nextItem = slip.Connectbname;
                        }
                        issueFound = liveTB.Name + state + " AGAINST";
                    }
                }
                */
                if (slip.Blockname != currentBlock)
                {
                    bnl.EdgeConnector = slip.Ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    bnl.BlockFound = slip.Blockname;
                    bnl.NextBlockEdgeConnector = nextItem;
                }
                else
                {
                    if (slip.Connectaname == previousLayoutItem || slip.Connectbname == previousLayoutItem)
                    {
                        //if (slip.Connectaname == previousLayoutItem)
                        //{
                        //    bnltoB.RequiredState = slip.States.AC.Turnout;
                        //}
                        //else
                        //{
                        //    bnltoB.RequiredState = slip.States.BC.Turnout;
                        //}

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
                            //bnltoB.RequiredState = slip.States.AC.TurnoutB;
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
                            //bnltoB.RequiredState = slip.States.AD.TurnoutB;
                        }
                        else
                        {
                            blockFoundOnTurnoutSearch = testbnlC.BlockFound;
                        }
                    }
                    else
                    {
                        //if (slip.Connectcname == previousLayoutItem)
                        //{
                        //    bnltoB.RequiredState = slip.States.BC.TurnoutB;
                        //}
                        //else
                        //{
                        //    bnltoB.RequiredState = slip.States.BD.TurnoutB;
                        //}
                        //Route is through A or B
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
                            //bnltoA.RequiredState = slip.States.AC.Turnout;
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
                            bnltoA.RequiredState = slip.States.BC.Turnout;
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
            return bnl;
        }
        private void WriteToLog(string message)
        {
            var fullMess = DateTime.Now + " - " + message;
            lbOutput.Items.Add(fullMess);
            lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
        }
    }
}
