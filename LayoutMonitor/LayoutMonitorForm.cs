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
        private List<BlockRootObject> allBlocks;
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
        private List<string> AllocatedBlocks;
        private List<LiveTrainLog> Log;
        private int TrainCounter;
        private bool AllocateBlocks;


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
            allBlocks = await webClient.GetBlocks();
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
            List<string> blocksProcessed = new List<string>();
            var newBlockStates = await webClient.GetBlocks();
            var newActiveBlocks = newBlockStates.Where(w => w.data.state == 2).ToList();
            var oldActiveBlocks = allBlocks.Where(w => w.data.state == 2).ToList();

            foreach (var nab in newActiveBlocks)
            {
                try {
                    var previousBlockState = allBlocks.FirstOrDefault(f => f.data.name == nab.data.name);
                    
                    var alreadyExists = oldActiveBlocks.Any(a => a.data.name == nab.data.name);
                    var justDeactivated = DeoccupiedBlocks.Any(a => a.BlockName == nab.data.name);
                    if (!alreadyExists && !justDeactivated)
                    {
                        if (nab.data == null) continue;
                        var success = await ProcessNewActiveBlock(nab.data.userName, nab.data.name,"",previousBlockState.data.value);
                        if (success)
                        {
                            blocksProcessed.Add(nab.data.userName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lbOutput.Items.Add(ex.Message);
                }
            }

            var alertRemoved = false;
            var goneInactive = oldActiveBlocks.Where(x => !newActiveBlocks.Select(i => i.data.name).Contains(x.data.name));
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

            //Check current journeys for re-routing
            var logs = Log.ToList();
            foreach (var log in logs)
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
                    continue;
                }
                else if (log.IsAutomated) continue;

                var issueFoundThisBlock = false;
                var issueFoundNextBlock = false;
                var issueFoundTwoBlocks = false;
                var issueThisBlockDetails = "";
                var issueNextBlockDetails = "";
                var issueTwoBlockDetails = "";
                var allocateNextBlock = false;
                var allocateTwoBlocks = false;

                var newLog = log;
                //Go back to the start of the block in case we're joining it in the middle
                var currentBlockReverse = await NavigateThroughBlockItems(log.CurrentBlockBNL.BlockChecked, log.CurrentBlockBNL.PreviousBlock, log.CurrentBlockBNL.EdgeConnectorDirectionConnector, log.CurrentBlockBNL.EdgeConnector, log.CurrentBlockBNL.EdgeConnector);

                var currentBlockRoute = await NavigateThroughBlockItems(currentBlockReverse.BlockChecked, currentBlockReverse.PreviousBlock, currentBlockReverse.EdgeConnectorDirectionConnector, currentBlockReverse.EdgeConnector, log.CurrentBlockBNL.EdgeConnector);
                var nextBlock = await NavigateThroughBlockItems(currentBlockRoute.BlockFound, currentBlockRoute.BlockFound, currentBlockRoute.EdgeConnector, currentBlockRoute.EdgeConnectorDirectionConnector, currentBlockRoute.EdgeConnector);
                var twoBlock = await NavigateThroughBlockItems(nextBlock.BlockFound, nextBlock.BlockChecked, nextBlock.EdgeConnector, nextBlock.EdgeConnectorDirectionConnector, nextBlock.EdgeConnector);
                if (currentBlockRoute.BlockFound != log.CurrentBlockBNL.BlockFound)
                {
                    nextBlock = await NavigateThroughBlockItems(currentBlockRoute.BlockFound, currentBlockRoute.BlockFound, currentBlockRoute.EdgeConnector, currentBlockRoute.EdgeConnectorDirectionConnector, currentBlockRoute.EdgeConnector);
                    twoBlock = await NavigateThroughBlockItems(nextBlock.BlockFound, nextBlock.BlockChecked, nextBlock.EdgeConnector, nextBlock.EdgeConnectorDirectionConnector, nextBlock.EdgeConnector);

                    var sysName = config.GetBlockByUserName(log.CurrentBlockBNL.BlockFound);
                    await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                    webClient.AllocateBlock(sysName.userName, "");
                    sysName = config.GetBlockByUserName(log.NextBlockBNL.BlockFound);
                    await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                    webClient.AllocateBlock(sysName.userName, "");

                    newLog.AllocatedBlocks.Remove(log.CurrentBlockBNL.BlockFound);
                    newLog.AllocatedBlocks.Remove(log.NextBlockBNL.BlockFound);

                    allocateNextBlock = true;
                    allocateTwoBlocks = true;

                    var alertsForNextBlock = alerts.Where(w => w.BNL.BlockChecked == nextBlock.BlockChecked && w.TrainName == newLog.Name);
                    foreach (var alert in alertsForNextBlock)
                    {
                        alert.Deactivated = true;
                        alert.DeactivatedTime = DateTime.Now;
                    }

                    var alertsForTwoBlock = alerts.Where(w => w.BNL.BlockChecked == twoBlock.BlockChecked && w.TrainName == newLog.Name);
                    foreach (var alert in alertsForTwoBlock)
                    {
                        alert.Deactivated = true;
                        alert.DeactivatedTime = DateTime.Now;
                    }

                    newLog.AllocatedBlocks.Add(currentBlockRoute.BlockFound);
                    newLog.AllocatedBlocks.Add(nextBlock.BlockFound);
                }
                else if (nextBlock.BlockFound != log.NextBlockBNL.BlockFound)
                {
                    twoBlock = await NavigateThroughBlockItems(nextBlock.BlockFound, nextBlock.BlockChecked, nextBlock.EdgeConnector, nextBlock.EdgeConnectorDirectionConnector, nextBlock.EdgeConnector);
                    var sysName = config.GetBlockByUserName(log.NextBlockBNL.BlockFound);
                    await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                    webClient.AllocateBlock(sysName.userName, "");
                    log.AllocatedBlocks.Remove(log.NextBlockBNL.BlockFound);
                    newLog.AllocatedBlocks.Add(nextBlock.BlockFound);
                    allocateTwoBlocks = true;
                }

                issueFoundThisBlock = !string.IsNullOrEmpty(currentBlockRoute.LikelyIssue);
                issueFoundNextBlock = nextBlock != null && !string.IsNullOrEmpty(nextBlock.LikelyIssue);
                issueFoundTwoBlocks = twoBlock != null && !string.IsNullOrEmpty(twoBlock.LikelyIssue);
                issueThisBlockDetails = currentBlockRoute.LikelyIssue;
                if (nextBlock != null) issueNextBlockDetails = nextBlock.LikelyIssue;
                if (twoBlock != null) issueTwoBlockDetails = twoBlock.LikelyIssue;

                var currentBlockcfg = config.GetBlockByUserName(currentBlockRoute.BlockChecked);

                var liveNextBlock = newBlockStates.FirstOrDefault(f => f.data.userName == nextBlock.BlockChecked);
                if (liveNextBlock.data.state == 2)
                {
                    issueFoundNextBlock = true;
                    if (issueNextBlockDetails == "")
                        issueNextBlockDetails = "Collision ";
                    else
                        issueNextBlockDetails = issueNextBlockDetails + " - Collision ";
                }
                else if (!string.IsNullOrEmpty(liveNextBlock.data.value) && liveNextBlock.data.value != log.Name)
                {
                    issueFoundNextBlock = true;
                    if (issueNextBlockDetails == "")
                        issueNextBlockDetails = "Allocated to " + liveNextBlock.data.value;
                    else
                        issueNextBlockDetails = issueNextBlockDetails + " - Allocated to " + liveNextBlock.data.value;
                }

                if (twoBlock != null)
                {
                    var liveTwoBlocks = newBlockStates.FirstOrDefault(f => f.data.userName == twoBlock.BlockChecked);
                    if (liveTwoBlocks.data.state == 2)
                    {
                        issueFoundTwoBlocks = true;
                        if (issueTwoBlockDetails == "")
                            issueTwoBlockDetails = "Collision ";
                        else
                            issueTwoBlockDetails = issueTwoBlockDetails + " - Collision ";
                    }
                    else if (!string.IsNullOrEmpty(liveTwoBlocks.data.value) && liveTwoBlocks.data.value != log.Name)
                    {
                        issueFoundTwoBlocks = true;
                        if (issueTwoBlockDetails == "")
                            issueTwoBlockDetails = "Allocated to " + liveTwoBlocks.data.value;
                        else
                            issueTwoBlockDetails = issueTwoBlockDetails + " - Allocated to " + liveTwoBlocks.data.value;
                    }
                }

                newLog.CurrentBlock = currentBlockRoute.BlockChecked;
                if (nextBlock != null) newLog.NextBlock = nextBlock.BlockChecked;
                if (twoBlock != null) newLog.NextNextBlock = twoBlock.BlockChecked;
                newLog.CurrentBlockBNL = currentBlockRoute;
                newLog.NextBlockBNL = nextBlock;
                newLog.TwoBlocksBNL = twoBlock;

                if (issueFoundThisBlock)
                {
                    var alertExists = alerts.Any(a => a.AffectedBlockUserName == currentBlockRoute.BlockChecked && a.Severity == AlertSeverity.Extreme && a.TrainName == log.Name);
                    var existingAlert = alerts.FirstOrDefault(a => a.AffectedBlockUserName == currentBlockRoute.BlockChecked && a.Severity == AlertSeverity.Extreme && a.TrainName == log.Name);
                    if (existingAlert == null)
                    {
                        alerts.Add(new Alert()
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
                            TrainName = log.Name
                        });
                    }
                    else
                    {
                        existingAlert.Deactivated = false;
                    }
                }

                if (issueFoundNextBlock)
                {
                    //Danger alert
                    var alertExists = alerts.Any(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == nextBlock.BlockChecked && a.Severity == AlertSeverity.Danger && a.TrainName == log.Name);
                    var existingAlert = alerts.FirstOrDefault(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == nextBlock.BlockChecked && a.Severity == AlertSeverity.Danger && a.TrainName == log.Name);
                    if (existingAlert == null)
                    {
                        alerts.Add(new Alert()
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
                            TrainName = log.Name
                        });
                    }
                    else if (existingAlert == null)
                    {
                        existingAlert.Deactivated = false;
                    }
                }
                else
                {
                    if (allocateNextBlock)
                    {
                        var nbConfig = config.GetBlockByUserName(currentBlockRoute.BlockFound);
                        await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + nbConfig.userName, nbConfig.userName, false);
                        webClient.AllocateBlock(nbConfig.userName, "");
                        if (!log.AllocatedBlocks.Contains(currentBlockRoute.BlockFound))
                            log.AllocatedBlocks.Add(currentBlockRoute.BlockFound);

                    }
                }
                if (issueFoundTwoBlocks)
                {
                    //Caution alert
                    var alertExists = alerts.Any(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == twoBlock.BlockChecked && a.Severity == AlertSeverity.Caution && a.TrainName == log.Name);
                    var existingAlert = alerts.FirstOrDefault(a => a.BlockSystemName == currentBlockcfg.systemName && a.BNL.BlockChecked == twoBlock.BlockChecked && a.Severity == AlertSeverity.Caution && a.TrainName == log.Name);
                    var dangerAlertExistsForNextBlock = alerts.Any(a => a.BNL.BlockChecked == nextBlock.BlockChecked && a.Severity == AlertSeverity.Danger && a.TrainName == log.Name);

                    if (existingAlert == null)
                    {
                        if (!dangerAlertExistsForNextBlock)
                        {
                            alerts.Add(new Alert()
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
                                TrainName = log.Name
                            });
                        }
                    }
                    else
                    {
                        existingAlert.Deactivated = false;
                    }
                }
                else
                {
                    if (allocateTwoBlocks)
                    {
                        var tbConfig = config.GetBlockByUserName(nextBlock.BlockFound);
                        await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + tbConfig.userName, tbConfig.userName, false);
                        webClient.AllocateBlock(tbConfig.systemName, newLog.Name);
                        if (!log.AllocatedBlocks.Contains(nextBlock.BlockFound))
                            log.AllocatedBlocks.Add(nextBlock.BlockFound);
                    }
                }

                //check allocation in case a collision alert has been cleared
                if (blocksProcessed.Count <= 0)
                {
                    foreach (var allocation in newLog.AllocatedBlocks)
                    {
                        var liveBlock = newBlockStates.FirstOrDefault(f => f.data.userName == allocation);
                        if (liveBlock != null && liveBlock.data.state == 4)
                        {
                            if (string.IsNullOrEmpty(liveBlock.data.value))
                            {
                                await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + liveBlock.data.userName, liveBlock.data.userName, false);
                                webClient.AllocateBlock(liveBlock.data.name, log.Name);
                            }
                        }
                    }
                }

                int logIndex = Log.IndexOf(log);
                if (logIndex >= 0)
                    Log[logIndex] = newLog;
            }            

            allBlocks = newBlockStates;
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
            foreach (var log in Log.ToList())
            {
                TerminateTrain(log.Name);
            }
            Log.Clear();
            alerts.Clear();
        }

        private async Task<bool> ProcessNewActiveBlock(string blockUserName, string blockSystemName, string previousBlockName = "", string trainName = "")
        {
            //new block gone occupied
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
            bool noMoreBlocks = false;
            bool handlingNewTrain = false;
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
            blockLog.AllocatedBlocks = new List<string>();

            var liveBlock = await webClient.GetBlock(blockUserName);
            var existingLog = Log.FirstOrDefault(f => f.Name == trainName);

            if (existingLog == null)
                 existingLog = Log.FirstOrDefault(f => f.NextBlock == blockUserName);

            if (existingLog == null)
            {
                var splitBlock = blockUserName.Split(' ');
                handlingNewTrain = true;
                blockLog.IsAutomated = false;
                blockLog.Name = trainName = "Manual train " + TrainCounter.ToString() + " " + splitBlock[0];
            }
            else
            {
                blockLog = existingLog;
            }

            if (blockLog.AllocatedBlocks.Contains(blockUserName))
                blockLog.AllocatedBlocks.Remove(blockUserName);

            string connector1 = "";
            string connector2 = "";
            string previousConnector = "";
            string breadcrumbStart = "";
            var trackSegments = config.GetTracksegmentsForBlock(blockUserName).OrderBy(o => o.Ident).ToList();
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
                if (blockLog.CurrentBlockBNL == null) return false;
                //could be a DS or turnout
                //need previous item
                var prev = blockLog.CurrentBlockBNL.EdgeConnectorDirectionConnector;
                var turnouts = config.GetTurnoutsInBlock(blockUserName);
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
                    var slips = config.GetSlipsInBlock(blockUserName);
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
                return false;

            var firstBoundaryFromMiddle = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, connector1, previousConnector, breadcrumbStart);
            if (firstBoundaryFromMiddle == null)
            {
                return false;
            }

            //if first boundary from middle has a warning - turnout closed against - we know we've gone the wrong way.
            //need to go the other way
            else if (firstBoundaryFromMiddle.LikelyIssue != null && firstBoundaryFromMiddle.LikelyIssue.Contains("AGAINST"))
            {
                firstBoundaryFromMiddle = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, connector2, previousConnector, firstBoundaryFromMiddle.EdgeConnector);
            }


            var secondBoundaryFromMiddle = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, connector2, previousConnector, firstBoundaryFromMiddle.EdgeConnector);
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

            if ((numberOfOccupiedBlocks == thisBlock.path.Count() || !oneConnectedBlockUnoccipied) && !determinedPreviousBlockFromAlerts && handlingNewTrain)
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


            if (!string.IsNullOrEmpty(blockLog.CurrentBlock)) //&& thisBlock.path.Any(a => a.block == blockLog.NextNextBlock))
            {
                likelyPreviousBlock = blockLog.CurrentBlock;
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
            

            if (!issueFoundNextBlock)
            {
                var bnl = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, firstBoundary.EdgeConnectorDirectionConnector, firstBoundary.EdgeConnector, firstBoundary.EdgeConnector);
                if (bnl.BlockFound == likelyPreviousBlock)
                {
                    BNLThisBlock = await NavigateThroughBlockItems(blockUserName, likelyPreviousBlock, bnl.EdgeConnectorDirectionConnector, bnl.EdgeConnector, bnl.EdgeConnector);
                }
                else
                {
                    BNLThisBlock = bnl;
                }
                    
                likelyNextBlock = BNLThisBlock.BlockFound;
                blockLog.NextBlock = likelyNextBlock;
                blockLog.CurrentBlockBNL = bnl;
                BNLThisBlock.BlockCheckedSystemName = blockSystemName;
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
                    BNLNextBlock.BlockCheckedSystemName = liveNextBlock.data.name;
                    if (liveNextBlock.data.state == 2) //occupied
                    {
                        issueFoundNextBlock = true;
                        likelyIssueNextBlock += "Collision ";// in " + liveNextBlock.data.userName;
                    }
                    if (!string.IsNullOrEmpty(liveNextBlock.data.value) && TrackAllocation && liveNextBlock.data.value != blockLog.Name)
                    {
                        issueFoundNextBlock = true;
                        BNLNextBlock.BlockCheckedAllocatedTo = liveNextBlock.data.value;
                        likelyIssueNextBlock += "Allocated to " + liveNextBlock.data.value + " ";
                    }

                    if (!blockLog.AllocatedBlocks.Contains(liveNextBlock.data.userName))
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
                        BNLTwoBlocks.BlockCheckedSystemName = twoBlocksLiveBlock.data.name;
                        if (twoBlocksLiveBlock.data.state == 2)
                        {
                            issueFoundTwoBlocks = true;
                            likelyIssueTwoBlocks = "Collision ";// in "+twoBlocksLiveBlock.data.userName;
                        }
                        if (!string.IsNullOrEmpty(twoBlocksLiveBlock.data.value) && TrackAllocation && twoBlocksLiveBlock.data.value != blockLog.Name)
                        {
                            issueFoundTwoBlocks = true;
                            likelyIssueTwoBlocks += "Allocated to " + twoBlocksLiveBlock.data.value + " ";
                            BNLTwoBlocks.BlockCheckedAllocatedTo = twoBlocksLiveBlock.data.value;
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
                        Deactivated = false,
                        TrainName = trainName
                    });
                }
            }

            if (issueFoundNextBlock)
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
                        Deactivated = false,
                        TrainName = trainName
                    });
                }
            }
            if (issueFoundTwoBlocks)
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
                        Deactivated = false,
                        TrainName = trainName
                    }) ;
                }
            }

            if (noMoreBlocks)
            {
                blockLog.Terminated = true;
                blockLog.TerminatedReason = "No more blocks";
                blockLog.LastUpdated = DateTime.Now;
                //TerminateTrain(blockLog.Name);
                return false;
            }

            if (!issueFoundThisBlock && !issueFoundNextBlock && !issueFoundTwoBlocks)
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
            
            if (!issueFoundNextBlock && !blockLog.IsAutomated)
            {
                webClient.AllocateBlock(BNLNextBlock.BlockCheckedSystemName, blockLog.Name);
                await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + BNLNextBlock.BlockCheckedSystemName, BNLNextBlock.BlockChecked, false);
            }
            if (!issueFoundTwoBlocks && !blockLog.IsAutomated)
            {
                webClient.AllocateBlock(BNLTwoBlocks.BlockCheckedSystemName, blockLog.Name);
                await MQTTClient.SendMQTTMessage(MQTTServer, BlockAllocateTopic + "/" + BNLTwoBlocks.BlockCheckedSystemName, BNLTwoBlocks.BlockChecked, false);
            }
            blockLog.LastUpdated = DateTime.Now;

            Log.Remove(existingLog);
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
                foreach (var alert in alerts.OrderBy(o => o.Severity).ToList())
                {
                    if (alert.Deactivated) continue;

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
                    if (log != null)
                    {
                        if (log.Terminated)
                        {
                            alert.Deactivated = true;
                            alert.DeactivatedTime = DateTime.Now;
                        }
                        else
                        {
                            if (log.CurrentBlockBNL.BlockChecked == alert.BNL.BlockChecked)
                                checkAlert = log.CurrentBlockBNL;
                            else if (log.NextBlockBNL.BlockChecked == alert.BNL.BlockChecked)
                                checkAlert = log.NextBlockBNL;
                            else if (log.TwoBlocksBNL.BlockChecked == alert.BNL.BlockChecked)
                                checkAlert = log.TwoBlocksBNL;
                        }
                    }
                    if (string.IsNullOrEmpty(checkAlert.BlockChecked)) continue;

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
                    if (TrackAllocation && !string.IsNullOrEmpty(checkAlertLiveBlock.data.value) && log != null && checkAlertLiveBlock.data.value != log.Name) alertStillActive = true;

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
                        ListViewItem item = new ListViewItem();
                        item.Name = alert.id.ToString();
                        if (log.Terminated)
                        {
                            item.Text = "Terminated - " + log.TerminatedReason;
                        }
                        else
                            item.Text = "Cleared "+ alert.BNL.BlockChecked + " - proceed";

                        item.BackColor = Color.LimeGreen;
                        lvUpdates.Items.Add(item);
                        lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
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

        private void btnTerminateTrain_Click(object sender, EventArgs e)
        {
            var trainName = (string)ddlTrainSelector.SelectedItem;
            TerminateTrain(trainName);
            var log = Log.FirstOrDefault(f => f.Name == trainName);
            if (log != null) Log.Remove(log);
            ddlTrainSelector.Items.Remove(trainName);
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
            if (Log.Count > 0)
            {
                foreach (var blockToUnallocate in log.AllocatedBlocks)
                {
                    webClient.AllocateBlock(blockToUnallocate, "");
                    var sysName = config.GetBlockByUserName(blockToUnallocate);
                    await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                }
            }
            Log.Remove(log);
            ddlTrainSelector.Items.Clear();
            foreach (var remainingLog in Log)
            {
                ddlTrainSelector.Items.Add(remainingLog.Name);
            }
        }

        private async void btnCancelAllocations_Click(object sender, EventArgs e)
        {
            if (webClient == null || config == null)
            {
                config = new ConfigReader(tbConfigLocation.Text);
                webClient = new JSONReader("http://" + tbServerIP.Text + ":" + tbServerPort.Text);
            }
            var blocks = await webClient.GetBlocks();
            var allocatedBlocks = blocks.Where(w => w.data.value != null);
            foreach (var alloc in allocatedBlocks)
            {
                if (!string.IsNullOrEmpty(alloc.data.value))
                {
                    webClient.AllocateBlock(alloc.data.userName, "");
                    var sysName = config.GetBlockByUserName(alloc.data.userName);
                    await MQTTClient.SendMQTTMessage(MQTTServer, BlockReleaseTopic + "/" + sysName.userName, sysName.userName, false);
                }
            }
        }
    }
}
