using JMRIReader;
using JMRIReader.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiThrottleClient;
using WiThrottleClient.Classes;

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
                    var newActiveThisTimeBlocks = newActiveBlocks.Where(p => !oldActiveBlocks.Any(p2 => p2.data.name == p.data.name) && p.data.value != null).ToList();

                    foreach (var nab in newActiveThisTimeBlocks)
                    {
                        var nextBlockName = "";
                        var existingLog = _logs.FirstOrDefault(f => f.NextBlock == nab.data.userName);
                        var activeSection = existingLog.AutomatedSectionList.ElementAtOrDefault(existingLog.AutomatedCurrentSectionIndex);
                        var activeBlock = activeSection.Blocks.FirstOrDefault(f => f.userName == nab.data.userName);
                        if (activeBlock == null)
                        {
                            //entered new active section
                            activeSection = existingLog.AutomatedSectionList.ElementAtOrDefault(existingLog.AutomatedCurrentSectionIndex + 1);
                            existingLog.AutomatedCurrentSectionIndex++;
                            activeBlock = activeSection.Blocks.FirstOrDefault(f => f.userName == nab.data.userName);
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
                    CheckRunningTrains();
                    _allBlocks = newBlockStates;
                }

                await c.CheckForMessages();
                await Task.Delay(500);
            }

        }

        private void CheckRunningTrains()
        {
            foreach (var log in _logs)
            {
                var lastBlock = "";
                var lastBlockNextBlock = "";
                var lastBlockEdgeConnector = "";
                var lastBlockEdgeDirectionConnector = "";

                for (int i = log.AutomatedCurrentSectionIndex;i <  log.AutomatedCurrentSectionIndex+ _sectionsAhead;i++)
                {
                    var section = log.AutomatedSectionList.ElementAt(i);
                    if (section == null) continue;


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

        private async Task<LiveJourneyLog> ManageJourney(LiveJourneyLog log)
        {
            BlockNavigationLog BNLThisBlock = new BlockNavigationLog();
            BlockNavigationLog BNLNextBlock = new BlockNavigationLog();
            BlockNavigationLog BNLTwoBlocks = new BlockNavigationLog();

            bool issueFoundThisBlock = false;
            bool issueFoundNextBlock = false;
            bool issueFoundTwoBlocks = false;

            bool noMoreBlocks = false;

            string connectingAnchorPoint = "";
            string likelyIssueThisBlock = "";
            string likelyIssueNextBlock = "";
            string likelyIssueTwoBlocks = "";

            string connector1 = "";
            string connector2 = "";
            string previousConnector = "";
            string breadcrumbStart = "";
            var trackSegments = config.GetTracksegmentsForBlock(log.CurrentBlock).OrderBy(o => o.Ident).ToList();
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
                if (log.CurrentBlockBNL == null) return null;
                //could be a DS or turnout
                //need previous item
                var prev = log.CurrentBlockBNL.EdgeConnectorDirectionConnector;
                var turnouts = config.GetTurnoutsInBlock(log.CurrentBlock);
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
                    var slips = config.GetSlipsInBlock(log.CurrentBlock);
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
                return null;

            var firstBoundaryFromMiddle = NavigateThroughBlockItems(log.CurrentBlock, log.NextBlock, connector1, previousConnector, breadcrumbStart);
            if (firstBoundaryFromMiddle == null)
            {
                return null;
            }

            //if first boundary from middle has a warning - turnout closed against - we know we've gone the wrong way.
            //need to go the other way
            else if (firstBoundaryFromMiddle.LikelyIssue != null && firstBoundaryFromMiddle.LikelyIssue.Contains("AGAINST"))
            {
                firstBoundaryFromMiddle = NavigateThroughBlockItems(log.CurrentBlock, log.NextBlock, connector2, previousConnector, firstBoundaryFromMiddle.EdgeConnector);
            }


            var secondBoundaryFromMiddle = NavigateThroughBlockItems(log.CurrentBlock, log.NextBlock, connector2, previousConnector, firstBoundaryFromMiddle.EdgeConnector);
            if (secondBoundaryFromMiddle == null)
            {
                return null;
            }
            var secondBoundary = NavigateThroughBlockItems(log.CurrentBlock, log.NextBlock, firstBoundaryFromMiddle.EdgeConnectorDirectionConnector, firstBoundaryFromMiddle.EdgeConnector, firstBoundaryFromMiddle.EdgeConnector);
            if (secondBoundary == null)
            {
                return null;
            }

            var firstBoundary = NavigateThroughBlockItems(log.CurrentBlock, log.NextBlock, secondBoundary.EdgeConnectorDirectionConnector, secondBoundary.EdgeConnector, secondBoundary.EdgeConnector);
            if (firstBoundary == null)
            {
                return null;
            } 



            var bnl = NavigateThroughBlockItems(log.CurrentBlock, log.NextBlock, firstBoundary.EdgeConnectorDirectionConnector, firstBoundary.EdgeConnector, firstBoundary.EdgeConnector);
            if (bnl.BlockFound != log.NextBlock)
            {
                BNLThisBlock = NavigateThroughBlockItems(log.CurrentBlock, log.NextBlock, bnl.EdgeConnectorDirectionConnector, bnl.EdgeConnector, bnl.EdgeConnector);
            }
            else
            {
                BNLThisBlock = bnl;
            }

            //don't process if newly active block is surrounded by active blocks - most likely a detection issue
            var nextBlockLive = await webClient.GetBlock(log.NextBlock);


            log.CurrentBlockBNL = bnl;
            BNLThisBlock.BlockCheckedSystemName = log.CurrentBlock;

            if (BNLThisBlock.EdgeConnectorDirectionConnector.StartsWith("A"))
            {
                connectingAnchorPoint = BNLThisBlock.EdgeConnectorDirectionConnector;
            }
            if (!string.IsNullOrEmpty(BNLThisBlock.LikelyIssue))
            {
                likelyIssueThisBlock = BNLThisBlock.LikelyIssue;
                issueFoundThisBlock = true;
            }
            log.CurrentBlockBNL = BNLThisBlock;
            if (BNLThisBlock.NoMoreBlocksFound)
            {
                noMoreBlocks = true;
                BNLThisBlock.LikelyIssue = "End of line";
                issueFoundThisBlock = true;
            }

            BNLNextBlock = NavigateThroughBlockItems(BNLThisBlock.BlockChecked, BNLThisBlock.BlockFound, BNLThisBlock.EdgeConnector, BNLThisBlock.EdgeConnectorDirectionConnector, BNLThisBlock.EdgeConnector);
            if (BNLNextBlock != null && !noMoreBlocks)
            {
                log.NextNextBlock = BNLNextBlock.BlockFound;

                if (!String.IsNullOrEmpty(BNLNextBlock.LikelyIssue))
                {
                    issueFoundNextBlock = true;
                    likelyIssueNextBlock = BNLNextBlock.LikelyIssue + " ";
                }

                var liveNextBlock = await webClient.GetBlock(BNLNextBlock.BlockChecked);
                if (liveNextBlock != null && liveNextBlock.data != null)
                {
                    BNLNextBlock.BlockCheckedSystemName = liveNextBlock.data.name;
                    if ((liveNextBlock.data.value == null || liveNextBlock.data.value == null || liveNextBlock.data.value.data.userName != log.DCCiD) && liveNextBlock.data.state == 2)//occupied
                    {
                        issueFoundNextBlock = true;
                        likelyIssueNextBlock += "Collision ";// in " + liveNextBlock.data.userName;
                        BNLNextBlock.LikelyIssue = likelyIssueNextBlock;
                    }
                    if (liveNextBlock.data.value != null && liveNextBlock.data.value.data.userName != log.DCCiD && liveNextBlock.data.state == 4)
                    {
                        issueFoundNextBlock = true;
                        BNLNextBlock.BlockCheckedAllocatedTo = liveNextBlock.data.value.data.userName;
                        likelyIssueNextBlock += "Allocated to " + liveNextBlock.data.value + " ";
                        BNLNextBlock.LikelyIssue = likelyIssueNextBlock;
                    }
                }

                if (BNLNextBlock.NoMoreBlocksFound)
                {
                    noMoreBlocks = true;
                    BNLNextBlock.LikelyIssue = "End of line";
                    issueFoundNextBlock = true;
                }

                log.NextBlockBNL = BNLNextBlock;

                if (!log.AllocatedBlocks.Contains(liveNextBlock.data.name))
                    log.AllocatedBlocks.Add(liveNextBlock.data.userName);

                BNLTwoBlocks = NavigateThroughBlockItems(BNLNextBlock.BlockFound, BNLNextBlock.BlockChecked, BNLNextBlock.EdgeConnector, BNLNextBlock.EdgeConnectorDirectionConnector, BNLNextBlock.EdgeConnector);
                if (BNLTwoBlocks != null)
                {

                    if (!String.IsNullOrEmpty(BNLTwoBlocks.LikelyIssue))
                    {
                        issueFoundTwoBlocks = true;
                        likelyIssueTwoBlocks = BNLTwoBlocks.LikelyIssue + " ";
                    }
                    var twoBlocksLiveBlock = await(webClient.GetBlock(BNLNextBlock.BlockFound));
                    if (twoBlocksLiveBlock != null && twoBlocksLiveBlock.data != null)
                    {
                        BNLTwoBlocks.BlockCheckedSystemName = twoBlocksLiveBlock.data.name;
                        if ((twoBlocksLiveBlock.data.value == null || twoBlocksLiveBlock.data.value == null || twoBlocksLiveBlock.data.value.data.userName != log.DCCiD) && twoBlocksLiveBlock.data.state == 2)//occupied
                        {
                            issueFoundTwoBlocks = true;
                            likelyIssueTwoBlocks += "Collision ";// in "+twoBlocksLiveBlock.data.userName;
                            BNLTwoBlocks.LikelyIssue = likelyIssueTwoBlocks;
                        }
                        if (twoBlocksLiveBlock.data.value != null && twoBlocksLiveBlock.data.value.data.userName != log.DCCiD && twoBlocksLiveBlock.data.state == 4)
                        {
                            issueFoundTwoBlocks = true;
                            likelyIssueTwoBlocks += "Allocated to " + twoBlocksLiveBlock.data.value + " ";
                            BNLTwoBlocks.BlockCheckedAllocatedTo = twoBlocksLiveBlock.data.value.data.userName;
                            BNLTwoBlocks.LikelyIssue = likelyIssueTwoBlocks;
                        }
                    }

                    if (BNLTwoBlocks.NoMoreBlocksFound)
                    {
                        noMoreBlocks = true;
                        BNLTwoBlocks.LikelyIssue = "End of line";
                        issueFoundTwoBlocks = true;
                    }

                    log.TwoBlocksBNL = BNLTwoBlocks;

                    if (!log.AllocatedBlocks.Contains(twoBlocksLiveBlock.data.userName))
                        log.AllocatedBlocks.Add(twoBlocksLiveBlock.data.userName);
                }
            }
            return log;
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
                        if (to.Connectaname == previousLayoutItem)
                        {
                            var testbnlB = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectbname, to.Connectaname, "");
                            var testbnlC = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectcname, to.Connectaname, "");

                            if (testbnlB != null && testbnlB.BlockFound == nextBlock)
                            {
                                //a -> B requires closed
                                bnlto.RequiredState = "2";
                                nextItemIdent = to.Connectbname;
                            }
                            else if (testbnlC != null && testbnlC.BlockFound == nextBlock)
                            {
                                //a -> c requires thrown
                                bnlto.RequiredState = "4";
                                nextItemIdent = to.Connectcname;
                            }

                            /*
                            if (liveTurnout.State == "2")
                            {
                                //closed
                                nextItemIdent = to.Connectbname;
                                bnlto.RequiredState = "2";
                            }
                            else
                            {
                                if (to.Type.StartsWith("LH"))
                                {
                                    //approaching A on a thrown LH XOver - short imminent
                                    bnl.LikelyIssue = liveTurnout.Name + " CLOSED AGAINST";
                                    nextItemIdent = to.Connectbname;
                                    bnlto.RequiredState = "4";
                                }
                                else
                                {
                                    nextItemIdent = to.Connectcname;
                                    bnlto.RequiredState = "2";
                                }
                            }
                            */
                        }
                        else if (to.Connectbname == previousLayoutItem)
                        {                            
                            if (liveTurnout.State == "2")
                            {
                                nextItemIdent = to.Connectaname;
                                bnlto.RequiredState = "2";
                            }
                            else
                            {
                                if (to.Type.StartsWith("RH"))
                                {
                                    //approaching B on a RH Xover when it's open - short imminent - assign a SM that should be red
                                    bnl.LikelyIssue = liveTurnout.Name + " THROWN AGAINST";
                                    nextItemIdent = to.Connectaname;
                                    bnlto.RequiredState = "2";
                                }
                                else
                                {
                                    nextItemIdent = to.Connectdname;
                                    bnlto.RequiredState = "4";
                                }
                            }
                        }
                        else if (to.Connectcname == previousLayoutItem)
                        {
                            var testbnlA = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectaname, to.Ident, "");
                            var testbnlD = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectdname, to.Ident, "");

                            if (testbnlA != null && testbnlA.BlockFound == nextBlock)
                            {
                                //C to A == thrown required
                                nextItemIdent = to.Connectaname;
                                bnlto.RequiredState = "4";
                            }
                            else if (testbnlD != null && testbnlD.BlockFound == nextBlock)
                            {
                                //C to D == closed required
                                nextItemIdent = to.Connectdname;
                                bnlto.RequiredState = "2";
                            }

                            /*
                            if (liveTurnout.State == "2")
                            {
                                //closed                              
                                
                            }
                            else
                            {
                                if (to.Type.StartsWith("LH"))
                                {
                                    //approaching C on a LH Xover when it's thrown - short imminent
                                    bnl.LikelyIssue = liveTurnout.Name + " THROWN AGAINST";
                                    nextItemIdent = to.Connectdname;
                                    bnlto.RequiredState = "2";
                                }
                                else
                                {
                                    nextItemIdent = to.Connectaname;
                                    bnlto.RequiredState = "4";
                                }
                            }
                            */
                        }
                        else if (to.Connectdname == previousLayoutItem)
                        {                            
                            if (liveTurnout.State == "2")
                            {
                                nextItemIdent = to.Connectcname;
                                bnlto.RequiredState = "2";
                            }
                            else
                            {
                                if (to.Type.StartsWith("RH"))
                                {
                                    bnl.LikelyIssue = liveTurnout.Name + " THROWN AGAINST";
                                    nextItemIdent = to.Connectcname;
                                    bnlto.RequiredState = "2";
                                }
                                else
                                {
                                    nextItemIdent = to.Connectbname;
                                    bnlto.RequiredState = "4";
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
                        if (liveTurnout.State == "4")
                        {
                            //thrown                        
                            //need to determine direction of travel. If one of the C or B connectors matches the previousLayout Item, we're traversing head on.
                            if (to.Connectbname == previousLayoutItem || to.Connectcname == previousLayoutItem)
                            {
                                nextItemIdent = to.Connectaname;
                                if (to.Connectbname == previousLayoutItem)
                                {
                                    bnl.LikelyIssue = liveTurnout.Name + " THROWN AGAINST";
                                    bnlto.RequiredState = "2";
                                }
                            }
                            else
                            {
                                //so to.connectaname == previouslayoutitem
                                var thrownConnector = to.Connectcname;
                                if (thrownConnector != previousLayoutItem)
                                {
                                    nextItemIdent = thrownConnector;
                                }
                                else
                                {
                                    nextItemIdent = to.Connectaname;
                                    //so next item is the a connector of the TO
                                    var testbnlB = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectaname, to.Connectbname, "");
                                    var testbnlC = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectaname, to.Connectcname, "");
                                }
                                //var testbnl = NavigateThroughBlockItems(currentBlock, nextBlock, nextItemIdent, to.Ident, "");
                            }
                        }
                        else
                        {
                            if (to.Connectbname == previousLayoutItem || to.Connectcname == previousLayoutItem)
                            {
                                nextItemIdent = to.Connectaname;
                                if (to.Connectcname == previousLayoutItem)
                                {
                                    bnl.LikelyIssue = liveTurnout.Name + " CLOSED AGAINST";
                                    bnlto.RequiredState = "4";
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
                                    var testbnlB = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectaname, to.Connectbname, "");
                                    var testbnlC = NavigateThroughBlockItems(currentBlock, nextBlock, to.Connectaname, to.Connectcname, "");
                                }
                            }
                        }
                        bnl.Breadcrumb += nextItemIdent + ";";
                        bnl.BNLTurnouts.Add(bnlto);
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
