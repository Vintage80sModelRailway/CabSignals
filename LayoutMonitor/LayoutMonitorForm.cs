using JMRIReader;
using JMRIReader.Classes;
using LayoutMonitor.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LayoutMonitor
{
    public partial class LayoutMonitorForm : Form
    {
        private JSONReader webClient;
        private ConfigReader config;
        private List<BlockRootObject> activeBlocks;
        private bool monitorRuning;
        private List<Alert> alerts;
        private bool ShowProceedMessages;
        private bool TrackAllocation;
        private int CautionNagFrequencySeconds;
        private int DangerNagFrequencySeconds;
        private List<DeOccupiedBlock> DeoccupiedBlocks;
        private string MQTTServer;
        private string BlockAllocateTopic;
        private string BlockReleaseTopic;
        public List<string> AllocatedBlocks;
        public List<LiveTrainLog> Log;
        public int TrainCounter;

        public LayoutMonitorForm()
        {
            InitializeComponent();
            lvUpdates.Columns.Add("Status");
            lvUpdates.HeaderStyle = ColumnHeaderStyle.None;

            var cfgFilePath = ConfigurationManager.AppSettings["ConfigFilePath"];
            if (cfgFilePath != null)
            {
                tbConfigLocation.Text = cfgFilePath.ToString();
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

            DeoccupiedBlocks = new List<DeOccupiedBlock>();
            Log = new List<LiveTrainLog>();
            TrainCounter = 1;
        }

        private async void btnStartMonitoring_Click(object sender, EventArgs e)
        {
            monitorRuning = true;
            config = new ConfigReader(tbConfigLocation.Text);
            webClient = new JSONReader("http://" + tbServerIP.Text + ":" + tbServerPort.Text);
            activeBlocks = await webClient.GetOccupiedBlocks();
            alerts = new List<Alert>();
            lbOutput.Items.Add("Monitoring started");
            ListViewItem item = new ListViewItem();
            item.Text = "Monitoring started";
            item.BackColor = Color.LimeGreen;
            lvUpdates.Items.Add(item);
            lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
            lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            while (monitorRuning)
            {
                await MonitorLayout();
                await ProcessAlerts();
                ProcessDeoccupiedBlocks();
            }
        }

        private async Task<bool> MonitorLayout()
        {
            var newActiveBlocks = await webClient.GetOccupiedBlocks();

            foreach (var nab in newActiveBlocks)
            {
                try {
                    var alreadyExists = activeBlocks.Any(a => a.data.name == nab.data.name);
                    var justDeactivated = DeoccupiedBlocks.Any(a => a.BlockName == nab.data.name);
                    if (!alreadyExists && !justDeactivated)
                    {
                        if (nab.data == null) continue;
                        var result = await ProcessNewActiveBlock(nab.data.userName, nab.data.name);
                    }
                }
                catch (Exception ex)
                {
                    lbOutput.Items.Add(ex.Message);
                }
            }

            var alertRemoved = false;
            var goneInactive = activeBlocks.Where(x => !newActiveBlocks.Select(i => i.data.name).Contains(x.data.name));
            foreach (var inactive in goneInactive)
            {
                var alreadyExists = DeoccupiedBlocks.Any(a => a.BlockName == inactive.data.name);
                if (!alreadyExists)
                {
                    DeoccupiedBlocks.Add(new DeOccupiedBlock
                    {
                        BlockName = inactive.data.name,
                        DeactivatedTime = DateTime.Now
                    });
                }
                var relatedAlert = alerts.FirstOrDefault(f => f.BlockUserName == inactive.data.userName);
                if (relatedAlert != null)
                {
                    alertRemoved = true;
                    ListViewItem item = new ListViewItem();
                    item.Text = "Proceed " + relatedAlert.BlockUserName + " cleared";
                    item.BackColor = Color.LimeGreen;
                    lvUpdates.Items.Add(item);
                    lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                    lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    relatedAlert.Deactivated = true;
                    relatedAlert.DeactivatedTime = DateTime.Now;
                    if (lblBlockWarning.Text == relatedAlert.BlockUserName)
                    {
                        lblBlockWarning.Text = "";
                        lblBlockContainingDanger.Text = "";
                        lblLikelyIssue.Text = "";
                    }
                }
                var activeLog = Log.Where(w => w.CurrentBlock == inactive.data.userName).ToList();
                foreach (var al in activeLog)
                {
                    Log.Remove(al);
                }
            }

            if (alertRemoved && alerts.Count == 0)
            {
                var latestAlert = lvUpdates.Items[lvUpdates.Items.Count - 1];
                lvUpdates.Items.Clear();
                lvUpdates.Items.Add(latestAlert);
                lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }

            activeBlocks = newActiveBlocks;
            return true;
        }

        private void btnStopMonitoring_Click(object sender, EventArgs e)
        {
            lbOutput.Items.Add("Monitoring stopped");
            monitorRuning = false;
            ListViewItem item = new ListViewItem();
            item.Text = "Monitoring stopped";
            item.BackColor = Color.Red;
            lvUpdates.Items.Add(item);
            lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
        }

        private async Task<bool> ProcessNewActiveBlock(string blockUserName, string blockSystemName, string previousBlockName = "")
        {
            //new block gone occupied
            bool isRecheck = false;
            if (previousBlockName != "") isRecheck = true;
            var likelyNextBlock = "";
            var likelyPreviousBlock = previousBlockName;
            lbOutput.Items.Add("New active block " + blockUserName);
            var thisBlock = config.GetBlockBySystemName(blockSystemName);
            bool oneConnectedBlockUnoccipied = false;
            bool oneConnectedBlockOccupied = false;
            bool issueFoundThisBlock = false;
            bool issueFoundNextBlock = false;
            bool issueFoundTwoBlocks = false;
            bool determinedPreviousBlockFromAlerts = false;
            string connectingAnchorPoint = "";
            string likelyIssueThisBlock = "";
            string likelyIssueNextBlock = "";
            string likelyIssueTwoBlocks = "";
            BlockNavigationLog BNLThisBlock = new BlockNavigationLog();
            BlockNavigationLog BNLNextBlock = new BlockNavigationLog();
            BlockNavigationLog BNLTwoBlocks = new BlockNavigationLog();
            List<BlockRootObject> nextBlocks = new List<BlockRootObject>();
            var blockLog = new LiveTrainLog();
            blockLog.History = new List<string>();

            var liveBlock = await webClient.GetBlock(blockUserName);
            var trainName = liveBlock.data.value;
            var existingLog = Log.FirstOrDefault(f => f.NextBlock == blockUserName);

            if (isRecheck)
                existingLog = Log.FirstOrDefault(f => f.CurrentBlock == blockUserName);

            if (existingLog == null)
            {
                blockLog.Name = "Manual train " + TrainCounter.ToString();
                TrainCounter++;
            }
            else
            {
                blockLog = existingLog;
                Log.Remove(existingLog);
            }

            var trackSegments = config.GetTracksegmentsForBlock(blockUserName).OrderBy(o => o.Ident).ToList();
            var ts = trackSegments.FirstOrDefault();
            if (ts == null) return false;
            var firstBoundaryFromMiddle = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, ts.Connect1name, ts.Ident, ts.Ident);
            if (firstBoundaryFromMiddle == null)
            {
                return false;
            }
            var secondBoundaryFromMiddle = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, ts.Connect2name, ts.Ident, firstBoundaryFromMiddle.EdgeConnector);
            if (secondBoundaryFromMiddle == null)
            {
                return false;
            }
            var secondBoundary = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, firstBoundaryFromMiddle.EdgeConnectorDirectionConnector, firstBoundaryFromMiddle.EdgeConnector, firstBoundaryFromMiddle.EdgeConnector);
            if (secondBoundary == null)
            {
                return false;
            }

            var firstBoundary = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, secondBoundary.EdgeConnectorDirectionConnector, secondBoundary.EdgeConnector, secondBoundary.EdgeConnector);
            if (firstBoundary == null)
            {
                return false;
            }

            int numberOfOccupiedBlocks = 0;
            List<string> LikelyPreviousBlocks = new List<string>();
            foreach (var connectedBlock in thisBlock.path)
            {
                var configBlock = config.GetBlockBySystemName(connectedBlock.block);
                var livePathBlock = await webClient.GetBlock(configBlock.userName);
                if (livePathBlock.data.state == 2) //occupied
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

            if ((numberOfOccupiedBlocks == thisBlock.path.Count() || !oneConnectedBlockUnoccipied) && !determinedPreviousBlockFromAlerts)
            {
                issueFoundNextBlock = true;
                likelyIssueNextBlock = "Collision all surrounding blocks occupied ";
                BNLNextBlock.PreviousBlock = blockUserName;
                BNLNextBlock.BlockChecked = blockUserName;
            }

            if (!oneConnectedBlockOccupied)
            {
                lbOutput.Items.Add("No connected active blocks, done nothing for " + blockUserName);
                return false;
            }


            if (!string.IsNullOrEmpty(blockLog.CurrentBlock) && thisBlock.path.Any(a => a.block == blockLog.NextNextBlock))
            {
                likelyPreviousBlock = blockLog.CurrentBlock;
            }
            else if (isRecheck)
            {
                likelyPreviousBlock = previousBlockName;
            }
            else
            {
                if (LikelyPreviousBlocks.Count == 1)
                {
                    likelyPreviousBlock = LikelyPreviousBlocks.First();
                }
                if (LikelyPreviousBlocks.Count > 1)
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
            }

            blockLog.PreviousBlock = likelyPreviousBlock;
            blockLog.History.Add(blockUserName);
            blockLog.CurrentBlock = blockUserName;

            if (ts != null && !issueFoundNextBlock)
            {
                var bnl = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, firstBoundary.EdgeConnectorDirectionConnector, firstBoundary.EdgeConnector, firstBoundary.EdgeConnector);
                if (bnl.BlockFound == likelyPreviousBlock)
                {
                    BNLThisBlock = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, bnl.EdgeConnectorDirectionConnector, bnl.EdgeConnector, bnl.EdgeConnector);
                    likelyNextBlock = BNLThisBlock.BlockFound;
                    blockLog.NextBlock = likelyNextBlock;
                    if (BNLThisBlock.EdgeConnectorDirectionConnector.StartsWith("A"))
                    {
                        connectingAnchorPoint = BNLThisBlock.EdgeConnectorDirectionConnector;
                    }
                    if (!string.IsNullOrEmpty(BNLThisBlock.LikelyIssue))
                    {
                        likelyIssueThisBlock = BNLThisBlock.LikelyIssue;
                        issueFoundThisBlock = true;
                    }

                    BNLNextBlock = await NavigateThroughBlockItems(likelyNextBlock, BNLThisBlock.BlockChecked, BNLThisBlock.EdgeConnector, BNLThisBlock.EdgeConnectorDirectionConnector, BNLThisBlock.EdgeConnector);
                    if (BNLNextBlock != null)
                    {
                        blockLog.NextNextBlock = BNLNextBlock.BlockFound;
                        if (!String.IsNullOrEmpty(BNLNextBlock.LikelyIssue))
                        {
                            issueFoundNextBlock = true;
                            likelyIssueNextBlock = BNLNextBlock.LikelyIssue + " ";
                        }
                        var liveNextBlock = await webClient.GetBlock(likelyNextBlock);
                        if (liveNextBlock.data.state == 2) //occupied
                        {
                            issueFoundNextBlock = true;
                            likelyIssueNextBlock += "Collision ";// in " + liveNextBlock.data.userName;
                        }
                        if (!string.IsNullOrEmpty(liveNextBlock.data.value) && TrackAllocation && liveNextBlock.data.value != blockLog.Name)
                        {
                            issueFoundNextBlock = true;
                            likelyIssueNextBlock += "Allocated to " + liveNextBlock.data.value + " ";
                        }
                        if (!issueFoundNextBlock)
                        {
                            webClient.AllocateBlock(liveNextBlock.data.name,blockLog.Name);
                            await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic+"/"+liveNextBlock.data.name, liveNextBlock.data.userName, false);
                        }
                        
                        BNLTwoBlocks = await NavigateThroughBlockItems(BNLNextBlock.BlockFound, BNLNextBlock.BlockChecked, BNLNextBlock.EdgeConnector, BNLNextBlock.EdgeConnectorDirectionConnector, BNLNextBlock.EdgeConnector);
                        if (BNLTwoBlocks != null)
                        {
                            if (!String.IsNullOrEmpty(BNLTwoBlocks.LikelyIssue))
                            {
                                issueFoundTwoBlocks = true;
                                likelyIssueTwoBlocks = BNLTwoBlocks.LikelyIssue + " ";
                            }
                            var twoBlocksLiveBlock = await (webClient.GetBlock(BNLNextBlock.BlockFound));
                            if (twoBlocksLiveBlock.data.state == 2)
                            {
                                issueFoundTwoBlocks = true;
                                likelyIssueTwoBlocks = "Collision ";// in "+twoBlocksLiveBlock.data.userName;
                            }
                            if (!string.IsNullOrEmpty(twoBlocksLiveBlock.data.value) && TrackAllocation && twoBlocksLiveBlock.data.value != blockLog.Name)
                            {
                                issueFoundTwoBlocks = true;
                                likelyIssueTwoBlocks += "Allocated to " + twoBlocksLiveBlock.data.value + " ";
                            }
                            if (!issueFoundTwoBlocks)
                            {
                                webClient.AllocateBlock(twoBlocksLiveBlock.data.name, blockLog.Name);
                                await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + twoBlocksLiveBlock.data.name, twoBlocksLiveBlock.data.userName, false);
                            }
                        }
                    }
                }
                else
                {
                    likelyNextBlock = bnl.BlockFound;
                    blockLog.NextBlock = likelyNextBlock;
                    BNLThisBlock = bnl;
                    if (BNLThisBlock.EdgeConnectorDirectionConnector.StartsWith("A"))
                    {
                        connectingAnchorPoint = BNLThisBlock.EdgeConnectorDirectionConnector;
                    }
                    if (!string.IsNullOrEmpty(BNLThisBlock.LikelyIssue))
                    {
                        likelyIssueThisBlock = BNLThisBlock.LikelyIssue;
                        issueFoundThisBlock = true;
                    }

                    BNLNextBlock = await NavigateThroughBlockItems(likelyNextBlock, BNLThisBlock.BlockChecked, BNLThisBlock.EdgeConnector, BNLThisBlock.EdgeConnectorDirectionConnector, BNLThisBlock.EdgeConnector);
                    if (BNLNextBlock != null)
                    {
                        if (!String.IsNullOrEmpty(BNLNextBlock.LikelyIssue))
                        {
                            issueFoundNextBlock = true;
                            likelyIssueNextBlock = BNLNextBlock.LikelyIssue + " ";
                        }
                        var liveNextBlock = await webClient.GetBlock(likelyNextBlock);
                        if (liveNextBlock.data.state == 2) //occupied
                        {
                            issueFoundNextBlock = true;
                            likelyIssueNextBlock += "Collision ";// in "+liveNextBlock.data.userName;
                        }
                        if (!string.IsNullOrEmpty(liveNextBlock.data.value) && TrackAllocation && liveNextBlock.data.value != blockLog.Name)
                        {
                            issueFoundNextBlock = true;
                            likelyIssueNextBlock += "Allocated to " + liveNextBlock.data.value + " ";
                        }
                        if (!issueFoundNextBlock)
                        {
                            webClient.AllocateBlock(liveNextBlock.data.name, blockLog.Name);
                            await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + liveNextBlock.data.name, liveNextBlock.data.userName, false);
                        }
                        blockLog.NextNextBlock = BNLNextBlock.BlockFound;
                        BNLTwoBlocks = await NavigateThroughBlockItems(BNLNextBlock.BlockFound, BNLNextBlock.BlockChecked, BNLNextBlock.EdgeConnector, BNLNextBlock.EdgeConnectorDirectionConnector, BNLNextBlock.EdgeConnector);
                        if (BNLTwoBlocks != null)
                        {
                            if (!String.IsNullOrEmpty(BNLTwoBlocks.LikelyIssue))
                            {
                                issueFoundTwoBlocks = true;
                                likelyIssueTwoBlocks = BNLTwoBlocks.LikelyIssue + " ";
                            }
                            var liveTwoBlocks = await webClient.GetBlock(BNLNextBlock.BlockFound);
                            if (liveTwoBlocks.data.state == 2) //occupied
                            {
                                issueFoundTwoBlocks = true;
                                likelyIssueTwoBlocks += "Collision  ";// in " + liveTwoBlocks.data.userName; ;
                            }
                            if (!string.IsNullOrEmpty(liveTwoBlocks.data.value) && TrackAllocation && liveTwoBlocks.data.value != blockLog.Name)
                            {
                                issueFoundTwoBlocks = true;
                                likelyIssueTwoBlocks += "Allocated to " + liveTwoBlocks.data.value + " ";
                            }
                            if (!issueFoundTwoBlocks)
                            {
                                webClient.AllocateBlock(liveTwoBlocks.data.name, blockLog.Name); ;
                                await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + liveTwoBlocks.data.name, liveTwoBlocks.data.userName, false);
                            }
                        }
                    }
                }
            }

            if (issueFoundThisBlock && !isRecheck)
            {
                var alertExists = alerts.Any(a => a.BlockSystemName == blockSystemName && a.Severity == AlertSeverity.Extreme);
                if (!alertExists)
                {
                    alerts.Add(new Alert()
                    {
                        id = Guid.NewGuid(),
                        BlockSystemName = blockSystemName,
                        BlockUserName = blockUserName,
                        SignalMastSystemName = "",
                        SignalMastUserName = "",
                        Severity = AlertSeverity.Extreme,
                        PreviousBlockUserName = likelyPreviousBlock,
                        NextBlockUserName = likelyNextBlock,
                        LikelyIssue = likelyIssueThisBlock,
                        BNL = BNLThisBlock,
                        Deactivated = false
                    });
                }
            }

            if (issueFoundNextBlock && !isRecheck)
            {
                //Danger alert
                var alertExists = alerts.Any(a => a.BlockSystemName == blockSystemName && a.Severity == AlertSeverity.Danger);
                if (!alertExists)
                {
                    alerts.Add(new Alert()
                    {
                        id = Guid.NewGuid(),
                        BlockSystemName = blockSystemName,
                        BlockUserName = blockUserName,
                        SignalMastSystemName = "",
                        SignalMastUserName = "",
                        Severity = AlertSeverity.Danger,
                        PreviousBlockUserName = likelyPreviousBlock,
                        NextBlockUserName = likelyNextBlock,
                        LikelyIssue = likelyIssueNextBlock,
                        BNL = BNLNextBlock,
                        Deactivated = false
                    });
                }
            }
            if (issueFoundTwoBlocks && !isRecheck)
            {
                //Caution alert
                var alertExists = alerts.Any(a => a.BlockSystemName == blockSystemName && a.Severity == AlertSeverity.Caution);
                var dangerAlertExistsForNextBlock = alerts.Any(a => a.BNL.BlockChecked == BNLNextBlock.BlockChecked && a.Severity == AlertSeverity.Danger);

                if (!alertExists && !dangerAlertExistsForNextBlock)
                {
                    alerts.Add(new Alert()
                    {
                        id = Guid.NewGuid(),
                        BlockSystemName = blockSystemName,
                        BlockUserName = blockUserName,
                        SignalMastSystemName = "",
                        SignalMastUserName = "",
                        Severity = AlertSeverity.Caution,
                        PreviousBlockUserName = likelyPreviousBlock,
                        NextBlockUserName = likelyNextBlock,
                        LikelyIssue = likelyIssueTwoBlocks,
                        BNL = BNLTwoBlocks,
                        Deactivated = false
                    });
                }
            }

            if (!issueFoundThisBlock && !issueFoundNextBlock && !issueFoundTwoBlocks)
            {
                if (!isRecheck)
                {
                    lbOutput.Items.Add(("Proceed " + blockUserName));
                    lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                    if (ShowProceedMessages)
                    {
                        ListViewItem item = new ListViewItem();
                        if (BNLTwoBlocks != null)
                        {
                            item.Text = "Proceed " + blockUserName + " to " + likelyNextBlock + " to " + BNLTwoBlocks.BlockChecked;
                        }
                        else
                        {
                            item.Text = "Proceed " + blockUserName + " to " + likelyNextBlock + " then possible end of blocks";
                        }
                        item.BackColor = Color.LimeGreen;
                        lvUpdates.Items.Add(item);
                        lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                        lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    }
                }

                var alertsForThisBlock = alerts.Where(w => w.BlockSystemName == blockSystemName && !w.Deactivated);
                foreach(var alert in alertsForThisBlock)
                {
                    ListViewItem item = new ListViewItem();
                    if (BNLTwoBlocks != null)
                    {
                        item.Text = "Cleared - proceed " + blockUserName + " to " + likelyNextBlock + " to " + BNLTwoBlocks.BlockChecked;
                    }
                    else
                    {
                        item.Text = "Cleared - proceed " + blockUserName + " to " + likelyNextBlock + " then possible end of blocks";
                    }
                    item.BackColor = Color.LimeGreen;
                    lvUpdates.Items.Add(item);
                    lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                    lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    alert.Deactivated = true;
                    alert.DeactivatedTime = DateTime.Now;
                    if (lblBlockWarning.Text == alert.BlockUserName)
                    {
                        lblBlockWarning.Text = "";
                        lblBlockContainingDanger.Text = "";
                        lblLikelyIssue.Text = "";
                    }

                }
            }
            blockLog.LastUpdated = DateTime.Now;
            //if (!isRecheck)
            //var existingLogRecheck = Log.FirstOrDefault(f => f.Name == trainName && f.NextBlock == blockUserName);
            //if (existingLogRecheck == null)
                Log.Add(blockLog);
            return true;
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

                //Approaching from...
                if (slip.Connectaname == previousLayoutItem)
                {
                    if (liveTB.data.state == 2)
                    {
                        //straight over
                        nextItem = slip.Connectcname;
                        var states = slip.States.AC;
                    }
                    else
                    {
                        nextItem = slip.Connectdname;
                        var states = slip.States.AD;
                    }
                    if (liveTA.data.state == 4)
                    {
                        //oncoming thrown - alert
                        issueFound = liveTA.data.userName + " THROWN AGAINST";
                    }
                }
                else if (slip.Connectbname == previousLayoutItem)
                {
                    if (liveTB.data.state == 4)
                    {
                        //c
                        nextItem = slip.Connectcname;
                        var states = slip.States.BC;
                    }
                    else
                    {
                        //d
                        nextItem = slip.Connectdname;
                        var states = slip.States.BD;
                    }
                    if (liveTA.data.state == 2)
                    {
                        //alert - TA against 
                        issueFound = liveTA.data.userName + " CLOSED AGAINST";
                    }
                }
                else if (slip.Connectcname == previousLayoutItem)
                {
                    if (liveTA.data.state == 2)
                    {
                        //straight over
                        nextItem = slip.Connectaname;
                        var states = slip.States.AC;
                    }
                    else
                    {
                        nextItem = slip.Connectbname;
                        var states = slip.States.BC;
                    }
                    if (liveTB.data.state == 4)
                    {
                        //alert approaching closed but thrown
                        issueFound = liveTA.data.userName + " THROWN AGAINST";
                    }
                }
                else if (slip.Connectdname == previousLayoutItem)
                {
                    if (liveTA.data.state == 2)
                    {
                        nextItem = slip.Connectaname;
                        var states = slip.States.AD;
                    }
                    else
                    {
                        nextItem = slip.Connectbname;
                        var states = slip.States.BD;
                    }
                    if (liveTB.data.state == 2)
                    {
                        //alert approaching thrown but closed
                        issueFound = liveTA.data.userName + " CLOSED AGAINST";
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
            var ackBlockName = lblBlockWarning.Text;
            var ackBlockChecked = lblBlockContainingDanger.Text.Substring(3);
            var ackAlert = alerts.FirstOrDefault(f => f.BlockUserName == ackBlockName && f.BNL.BlockChecked == ackBlockChecked);
            ListViewItem item = new ListViewItem();

            if (ackAlert != null)
            {
                item.Text = "Acknowledged " + ackAlert.BlockUserName;
                item.BackColor = Color.LimeGreen;
                lvUpdates.Items.Add(item);
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                ackAlert.Acknowledged = true;
                ackAlert.Deactivated = true;
                ackAlert.DeactivatedTime = DateTime.Now;
                if (lblBlockWarning.Text == ackAlert.BlockUserName)
                {
                    lblBlockWarning.Text = "";
                    lblBlockContainingDanger.Text = "";
                    lblLikelyIssue.Text = "";
                }
            }
        }

        private async Task<bool> ProcessAlerts()
        {
            if (alerts.Count == 0)
            {
                lblBlockWarning.Text = "";
                lblLikelyIssue.Text = "";
                lblBlockContainingDanger.Text = "";
            }

            var currentVisibleAlert = alerts.FirstOrDefault(f => f.Visible == true);
            List<Alert> alertsToRemove = new List<Alert>();

            try
            {
                foreach (var alert in alerts.OrderBy(o => o.Severity))
                {
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
                    var checkAlert = await NavigateThroughBlockItems(alert.BNL.BlockChecked,alert.BNL.PreviousBlock, alert.BNL.StartItem, alert.BNL.StartPreviousItem, alert.BNL.StartItem);
                    var recheck = await ProcessNewActiveBlock(alert.BlockUserName, alert.BlockSystemName,alert.PreviousBlockUserName);
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
                    alert.BNL = checkAlert;
                    
                    var checkAlertLiveBlock = await webClient.GetBlock(alert.BNL.BlockChecked);
                    var alertStillActive = false;
                    if (!string.IsNullOrEmpty(checkAlert.LikelyIssue)) alertStillActive = true;
                    if (checkAlertLiveBlock.data.state == 2) alertStillActive = true;
                    if (TrackAllocation && !string.IsNullOrEmpty(checkAlertLiveBlock.data.value)) alertStillActive = true;
                    //var alertStillActive = !string.IsNullOrEmpty(checkAlert.LikelyIssue) || checkAlertLiveBlock.data.state == 2;

                    if (!alertStillActive)
                    {
                        alert.Acknowledged = false;
                        alert.Deactivated = true;
                        alert.DeactivatedTime = DateTime.Now;
                        if (lblBlockWarning.Text == alert.BlockUserName)
                        {
                            lblBlockWarning.Text = "";
                            lblBlockContainingDanger.Text = "";
                            lblLikelyIssue.Text = "";
                        }
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
                lbOutput.Items.Add(ex.Message);
            }

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
            /*
            foreach (var alert in alertsToRemove)
            {
                if (alerts.Contains(alert))
                {
                    if (!alert.Superceded)
                    {
                        ListViewItem item = new ListViewItem();
                        if (!alert.Acknowledged)
                        {
                            item.Text = "Proceed " + alert.BlockUserName;
                        }
                        else
                        {
                            item.Text = "Acknowledged " + alert.BlockUserName;
                        }
                        item.BackColor = Color.LimeGreen;
                        lvUpdates.Items.Add(item);
                        lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                    }
                    alerts.Remove(alert);
                    alertsEmptied = true;
                }
            }
            */

            int numberOfActiveAlerts = alerts.Where(w => !w.Deactivated).Count();
            if(alertsEmptied && numberOfActiveAlerts == 0)
            {
                var latestAlert = lvUpdates.Items[lvUpdates.Items.Count - 1];
                lvUpdates.Items.Clear();
                lvUpdates.Items.Add(latestAlert);
                lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            return true;
        }

        private void DisplayAlert(Alert alert)
        {
            lblBlockWarning.Text = alert.BlockUserName;
            lblBlockWarning.ForeColor = Color.White;
            lblLikelyIssue.Text = alert.LikelyIssue;
            lblLikelyIssue.ForeColor = Color.White;
            lblBlockContainingDanger.ForeColor = Color.White;
            lblBlockContainingDanger.Text = "In "+ alert.BNL.BlockChecked;
            if (alert.Severity == AlertSeverity.Danger)
            {
                lblBlockWarning.BackColor = Color.Red;
                lblLikelyIssue.BackColor = Color.Red;
                lblBlockContainingDanger.BackColor = Color.Red;
                lbOutput.Items.Add(alert.BlockUserName+" DANGER "+ alerts.Count.ToString());
                lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                ListViewItem item = new ListViewItem();
                item.Name = alert.id.ToString();
                item.Text = alert.BlockUserName+" - "+alert.LikelyIssue+"- in "+alert.BNL.BlockChecked;
                item.BackColor = Color.Red;
                lvUpdates.Items.Add(item);
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
            }
            else if (alert.Severity == AlertSeverity.Caution)
            {
                lblBlockWarning.BackColor = Color.Orange;
                lblLikelyIssue.BackColor = Color.Orange;
                lblBlockContainingDanger.BackColor = Color.Orange;
                lbOutput.Items.Add(alert.BlockUserName + " CAUTION " + alerts.Count.ToString());
                lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                ListViewItem item = new ListViewItem();
                item.Name = alert.id.ToString();
                item.Text = alert.BlockUserName + " - " + alert.LikelyIssue + "- in " + alert.BNL.BlockChecked;
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

        private void btnPostTest_Click(object sender, EventArgs e)
        {
            webClient = new JSONReader("http://" + tbServerIP.Text + ":" + tbServerPort.Text);
            webClient.AllocateBlock("IB:AUTO:0001","");
        }

        private async void MQTT_Click(object sender, EventArgs e)
        {
            await MQTTClient.SendMQTTMessage("192.168.1.29", "layout/block/allocate/IB:AUTO:0001", "Yard AC Line 4 Block 1", false);
        }
    }
}
