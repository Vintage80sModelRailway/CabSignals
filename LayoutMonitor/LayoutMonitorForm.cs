using JMRIReader;
using JMRIReader.Classes;
using LayoutMonitor.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
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
            }
        }

        private async Task<bool> MonitorLayout()
        {
            var newActiveBlocks = await webClient.GetOccupiedBlocks();

            foreach (var nab in newActiveBlocks)
            {
                try {

                    var alreadyExists = activeBlocks.Any(a => a.data.name == nab.data.name);
                    if (!alreadyExists)
                    {
                        if (nab.data == null) continue;
                        //new block gone occupied
                        var likelyNextBlock = "";
                        var likelyPreviousBlock = "";
                        lbOutput.Items.Add("New active block " + nab.data.userName);
                        var thisBlock = config.GetBlockBySystemName(nab.data.name);
                        int direction = -1;
                        bool oneConnectedBlockUnoccipied = false;
                        bool oneConnectedBlockOccupied = false;
                        bool issueFoundThisBlock = false;
                        bool issueFoundNextBlock = false;
                        bool issueFoundTwoBlocks = false;
                        string connectingAnchorPoint = "";
                        string likelyIssueThisBlock = "";
                        string likelyIssueNextBlock = "";
                        string likelyIssueTwoBlocks = "";
                        BlockNavigationLog  BNLThisBlock = new BlockNavigationLog();
                        BlockNavigationLog BNLNextBlock = new BlockNavigationLog();
                        BlockNavigationLog BNLTwoBlocks = new BlockNavigationLog();
                        List<BlockRootObject> nextBlocks = new List<BlockRootObject>();

                        var trackSegments = config.GetTrackSegmentsForBlock(nab.data.userName).OrderBy(o => o.ident).ToList();
                        var ts = trackSegments.FirstOrDefault();
                        if (ts == null) return false;
                        var firstBoundaryFromMiddle = await NavigateThroughBlockItems(nab.data.userName, likelyPreviousBlock,ts.connect1name, ts.ident, ts.ident);
                        if (firstBoundaryFromMiddle == null)
                        {
                            activeBlocks = newActiveBlocks;
                            return false;
                        }
                        var secondBoundary = await NavigateThroughBlockItems(nab.data.userName, likelyPreviousBlock, firstBoundaryFromMiddle.EdgeConnectorDirectionConnector, firstBoundaryFromMiddle.EdgeConnector, firstBoundaryFromMiddle.EdgeConnector);
                        if (secondBoundary == null)
                        {
                            activeBlocks = newActiveBlocks;
                            return false;
                        }

                        var firstBoundary = await NavigateThroughBlockItems(nab.data.userName, likelyPreviousBlock, secondBoundary.EdgeConnectorDirectionConnector, secondBoundary.EdgeConnector, secondBoundary.EdgeConnector);
                        if (firstBoundary == null)
                        {
                            activeBlocks = newActiveBlocks;
                            return false;
                        }

                        int numberOfOccupiedBlocks = 0;
                        foreach (var connectedBlock in thisBlock.path)
                        {
                            var configBlock = config.GetBlockBySystemName(connectedBlock.block);
                            var liveBlock = await webClient.GetBlock(configBlock.userName);
                            if (liveBlock.data.state == 2) //occupied
                            {
                                numberOfOccupiedBlocks++;
                                if (liveBlock.data.userName == firstBoundary.BlockFound || liveBlock.data.userName == secondBoundary.BlockFound)
                                {
                                    likelyPreviousBlock = liveBlock.data.userName;
                                    oneConnectedBlockOccupied = true;
                                }
                            }
                            else
                            {
                                oneConnectedBlockUnoccipied = true;
                            }
                        }

                        if (numberOfOccupiedBlocks == thisBlock.path.Count() || !oneConnectedBlockUnoccipied)
                        {
                            issueFoundNextBlock = true;
                            likelyIssueNextBlock = "Collision";
                        }

                        if (!oneConnectedBlockOccupied)
                        {
                            lbOutput.Items.Add("No connected active blocks, done nothing for " + nab.data.userName);
                            activeBlocks = newActiveBlocks;
                            return false;
                        }

                        if (ts != null && !issueFoundNextBlock)
                        {
                            var bnl = await NavigateThroughBlockItems(nab.data.userName, likelyPreviousBlock, firstBoundary.EdgeConnectorDirectionConnector, firstBoundary.EdgeConnector, firstBoundary.EdgeConnector);
                            if (bnl.BlockFound == likelyPreviousBlock)
                            {
                                BNLThisBlock = await NavigateThroughBlockItems(nab.data.userName,likelyPreviousBlock, bnl.EdgeConnectorDirectionConnector, bnl.EdgeConnector, bnl.EdgeConnector);
                                BNLThisBlock.BlockChecked=nab.data.userName;

                                likelyNextBlock = BNLThisBlock.BlockFound;
                                if (BNLThisBlock.EdgeConnectorDirectionConnector.StartsWith("A"))
                                {
                                    connectingAnchorPoint = BNLThisBlock.EdgeConnectorDirectionConnector;
                                }
                                if (!string.IsNullOrEmpty(BNLThisBlock.LikelyIssue))
                                {
                                    likelyIssueThisBlock = BNLThisBlock.LikelyIssue;
                                    issueFoundThisBlock = true;
                                }

                                BNLNextBlock = await NavigateThroughBlockItems(likelyNextBlock,BNLThisBlock.BlockChecked, BNLThisBlock.EdgeConnector, BNLThisBlock.EdgeConnectorDirectionConnector, BNLThisBlock.EdgeConnector);
                                BNLNextBlock.BlockChecked = likelyNextBlock;

                                if (!String.IsNullOrEmpty(BNLNextBlock.LikelyIssue))
                                {
                                    issueFoundNextBlock = true;
                                    likelyIssueNextBlock = BNLNextBlock.LikelyIssue+" ";
                                }
                                var liveNextBlock = await webClient.GetBlock(likelyNextBlock);
                                if (liveNextBlock.data.state == 2) //occupied
                                {
                                    issueFoundNextBlock = true;
                                    likelyIssueNextBlock += "Collision in " + liveNextBlock.data.userName;
                                }
                                BNLTwoBlocks = await NavigateThroughBlockItems(BNLNextBlock.BlockFound, BNLNextBlock.BlockChecked, BNLNextBlock.EdgeConnector, BNLNextBlock.EdgeConnectorDirectionConnector, BNLNextBlock.EdgeConnector);
                                BNLTwoBlocks.BlockChecked = BNLNextBlock.BlockFound;

                                if (!String.IsNullOrEmpty(BNLTwoBlocks.LikelyIssue))
                                {
                                    issueFoundTwoBlocks = true;
                                    likelyIssueTwoBlocks = BNLTwoBlocks.LikelyIssue+" ";
                                }
                                var twoBlocksLiveBlock = await (webClient.GetBlock(BNLNextBlock.BlockFound));
                                if (twoBlocksLiveBlock.data.state == 2)
                                {
                                    issueFoundTwoBlocks = true;
                                    likelyIssueTwoBlocks = "Collision in "+twoBlocksLiveBlock.data.userName;
                                }
                            }
                            else
                            {
                                likelyNextBlock = bnl.BlockFound;
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
                                if (!String.IsNullOrEmpty(BNLNextBlock.LikelyIssue))
                                {
                                    issueFoundNextBlock = true;
                                    likelyIssueNextBlock = BNLNextBlock.LikelyIssue+" ";
                                }
                                var liveNextBlock = await webClient.GetBlock(likelyNextBlock);
                                if (liveNextBlock.data.state == 2) //occupied
                                {
                                    issueFoundNextBlock = true;
                                    likelyIssueNextBlock += "Collision in "+liveNextBlock.data.userName;
                                }
                                BNLTwoBlocks = await NavigateThroughBlockItems(BNLNextBlock.BlockFound,BNLNextBlock.BlockChecked, BNLNextBlock.EdgeConnector, BNLNextBlock.EdgeConnectorDirectionConnector, BNLNextBlock.EdgeConnector);
                                if (!String.IsNullOrEmpty(BNLTwoBlocks.LikelyIssue))
                                {
                                    issueFoundTwoBlocks = true;
                                    likelyIssueTwoBlocks = BNLTwoBlocks.LikelyIssue+" ";
                                }
                                var liveTwoBlocks = await webClient.GetBlock(BNLNextBlock.BlockFound);
                                if (liveTwoBlocks.data.state == 2) //occupied
                                {
                                    issueFoundTwoBlocks = true;
                                    likelyIssueTwoBlocks += "Collision in " + liveTwoBlocks.data.userName; ;
                                }
                            }
                        }

                        if (issueFoundThisBlock)
                        {
                            var alertExists = alerts.Any(a => a.BlockSystemName == nab.data.name && a.Severity == AlertSeverity.Extreme);
                            if (!alertExists)
                            {
                                alerts.Add(new Alert()
                                {
                                    BlockSystemName = nab.data.name,
                                    BlockUserName = nab.data.userName,
                                    SignalMastSystemName = "",
                                    SignalMastUserName = "",
                                    Severity = AlertSeverity.Extreme,
                                    PreviousBlockUserName = likelyPreviousBlock,
                                    NextBlockUserName = likelyNextBlock,
                                    LikelyIssue = likelyIssueThisBlock,
                                    BNL = BNLThisBlock
                                });
                            }
                        }
                        if (issueFoundNextBlock)
                        {
                            //Danger alert
                            var alertExists = alerts.Any(a => a.BlockSystemName == nab.data.name && a.Severity == AlertSeverity.Danger);
                            if (!alertExists)
                            {
                                alerts.Add(new Alert()
                                {
                                    BlockSystemName = nab.data.name,
                                    BlockUserName = nab.data.userName,
                                    SignalMastSystemName = "",
                                    SignalMastUserName = "",
                                    Severity = AlertSeverity.Danger,
                                    PreviousBlockUserName = likelyPreviousBlock,
                                    NextBlockUserName = likelyNextBlock,
                                    LikelyIssue = likelyIssueNextBlock,
                                    BNL = BNLNextBlock
                                });
                            }
                        }
                        if (issueFoundTwoBlocks)
                        {
                            //Caution alert
                            var alertExists = alerts.Any(a => a.BlockSystemName == nab.data.name && a.Severity == AlertSeverity.Caution);
                            if (!alertExists)
                            {
                                alerts.Add(new Alert()
                                {
                                    BlockSystemName = nab.data.name,
                                    BlockUserName = nab.data.userName,
                                    SignalMastSystemName = "",
                                    SignalMastUserName = "",
                                    Severity = AlertSeverity.Caution,
                                    PreviousBlockUserName = likelyPreviousBlock,
                                    NextBlockUserName = likelyNextBlock,
                                    LikelyIssue = likelyIssueTwoBlocks,
                                    BNL = BNLTwoBlocks
                                });
                            }
                        }

                        if (!issueFoundThisBlock && !issueFoundNextBlock && !issueFoundTwoBlocks)
                        {
                            lbOutput.Items.Add(("Proceed " + nab.data.userName));
                            lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                            ListViewItem item = new ListViewItem();
                            item.Text = "Proceed " + nab.data.userName;
                            item.BackColor = Color.LimeGreen;
                            lvUpdates.Items.Add(item);
                            lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                            lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                        }
                    }                    

                    var goneInactive = activeBlocks.Where(x => !newActiveBlocks.Select(i => i.data.name).Contains(x.data.name));
                    foreach (var inactive in goneInactive)
                    {
                        var relatedAlert = alerts.FirstOrDefault(f => f.BlockUserName == inactive.data.userName);
                        if (relatedAlert != null)
                        {
                            alerts.Remove(relatedAlert);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lbOutput.Items.Add(ex.Message);
                    activeBlocks = newActiveBlocks;
                    return false;
                }
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

        private string FindEdgeOfBlock(string currentBlock, string layoutItem, string previousLayoutItem)
        {
            return "";
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
                var configTurnout = config.GetTurnoutByUserName(to.turnoutname);
                var liveTurnout = await webClient.GetTurnout(configTurnout.systemName);
                var derivedXoverBlockName = "";
                if (to.type.Contains("XOVER"))
                {
                    if (to.connectaname == previousLayoutItem)
                    {
                        derivedXoverBlockName = to.blockname;
                    }
                    else if (to.connectbname == previousLayoutItem)
                    {
                        derivedXoverBlockName = to.blockname;
                    }
                    else if (to.connectcname == previousLayoutItem)
                    {
                        derivedXoverBlockName = to.blockcname;
                    }
                    else if (to.connectdname == previousLayoutItem)
                    {
                        derivedXoverBlockName = to.blockdname;
                    }

                }
                else derivedXoverBlockName = to.blockname;

                if (derivedXoverBlockName != currentBlock && derivedXoverBlockName != previousBlock)
                {
                    bnl.EdgeConnector = to.ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    bnl.BlockFound = to.blockname;
                    if (to.connectbname == previousLayoutItem || to.connectcname == previousLayoutItem)
                    {
                        bnl.NextBlockEdgeConnector = to.connectaname;
                    }
                    else if (liveTurnout.data.state == 2)
                    {
                        bnl.NextBlockEdgeConnector = to.connectbname;
                    }
                    else
                    {
                        bnl.NextBlockEdgeConnector = to.connectcname;
                    }
                }
                else
                {
                    //turnout still in same block, keep going
                    string nextItemIdent = "";
                    if (to.type.Contains("XOVER"))
                    {
                        if (to.connectaname == previousLayoutItem)
                        {
                            if (liveTurnout.data.state == 2)
                            {
                                //closed
                                nextItemIdent = to.connectbname;
                            }
                            else
                            {
                                if (to.type.StartsWith("LH"))
                                {
                                    //approaching A on a thrown LH XOver - short imminent
                                    bnl.LikelyIssue = liveTurnout.data.userName + " AGAINST";
                                }
                                nextItemIdent = to.connectcname;
                            }

                        }
                        else if (to.connectbname == previousLayoutItem)
                        {
                            if (liveTurnout.data.state == 2)
                            {
                                nextItemIdent = to.connectaname;
                            }
                            else
                            {
                                if (to.type.StartsWith("RH"))
                                {
                                    //approaching B on a RH Xover when it's open - short imminent - assign a SM that should be red
                                    bnl.LikelyIssue = liveTurnout.data.userName + " AGAINST";
                                }
                                nextItemIdent = to.connectdname;
                            }
                        }
                        else if (to.connectcname == previousLayoutItem)
                        {
                            if (liveTurnout.data.state == 2)
                            {
                                //closed
                                nextItemIdent = to.connectdname;
                            }
                            else
                            {
                                if (to.type.StartsWith("LH"))
                                {
                                    //approaching C on a LH Xover when it's thrown - short imminent
                                    bnl.LikelyIssue = liveTurnout.data.userName + " AGAINST";
                                }
                                nextItemIdent = to.connectaname;
                            }
                        }
                        else if (to.connectdname == previousLayoutItem)
                        {
                            if (liveTurnout.data.state == 2)
                            {
                                //closed
                                nextItemIdent = to.connectcname;
                            }
                            else
                            {
                                if (to.type.StartsWith("RH"))
                                {
                                    //approaching D on a RH Xover when it's thrown - short imminent
                                    bnl.LikelyIssue = liveTurnout.data.userName + " AGAINST";
                                }
                                nextItemIdent = to.connectbname;
                            }
                        }
                        bnl.Breadcrumb += nextItemIdent + ";";
                        var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItemIdent, to.ident, "");
                        bnl.Breadcrumb += newbnl.Breadcrumb;
                        bnl.BlockFound = newbnl.BlockFound;
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
                            if (to.connectbname == previousLayoutItem || to.connectcname == previousLayoutItem)
                            {
                                nextItemIdent = to.connectaname;
                                if (to.connectbname == previousLayoutItem)
                                {
                                    bnl.LikelyIssue = liveTurnout.data.userName + " AGAINST";
                                }
                            }
                            else
                            {
                                var thrownConnector = to.connectcname;
                                if (thrownConnector != previousLayoutItem)
                                {
                                    nextItemIdent = thrownConnector;
                                }
                                else
                                {
                                    nextItemIdent = to.connectaname;
                                }
                            }                         
                        }
                        else
                        {
                            if (to.connectbname == previousLayoutItem || to.connectcname == previousLayoutItem)
                            {
                                nextItemIdent = to.connectaname;
                                if (to.connectcname == previousLayoutItem)
                                {
                                    bnl.LikelyIssue = liveTurnout.data.userName + " AGAINST";
                                }
                            }
                            else
                            {
                                var closedConnector = to.connectbname;
                                if (closedConnector != previousLayoutItem)
                                {
                                    nextItemIdent = closedConnector;
                                }
                                else
                                {
                                    nextItemIdent = to.connectaname;
                                }
                            }
                        }
                        bnl.Breadcrumb += nextItemIdent + ";";
                        var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItemIdent, to.ident, "");
                        bnl.Breadcrumb += newbnl.Breadcrumb;
                        bnl.BlockFound = newbnl.BlockFound;
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
                var ts = config.GetLayoutTrackSegment(LayoutItem);
                if (ts.blockname != currentBlock && ts.blockname != previousBlock)
                {
                    bnl.EdgeConnector = ts.ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    bnl.BlockFound = ts.blockname;
                    if (ts.connect1name != previousLayoutItem)
                    {
                        bnl.NextBlockEdgeConnector = ts.connect1name;
                    }
                    else
                    {
                        bnl.NextBlockEdgeConnector = ts.connect2name;
                    }
                }
                else
                {
                    var nextItem = ts.connect2name;
                    if (ts.connect2name == previousLayoutItem) nextItem = ts.connect1name;
                    bnl.Breadcrumb += nextItem + ";";
                    var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItem, ts.ident,"");
                    bnl.Breadcrumb += newbnl.Breadcrumb;
                    bnl.BlockFound = newbnl.BlockFound;
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
                var nextItem = a.connect2name;
                if (a.connect2name == previousLayoutItem) nextItem = a.connect1name;
                bnl.Breadcrumb += nextItem + ";";
                var newbnl = await NavigateThroughBlockItems(currentBlock, previousBlock, nextItem, a.ident,"");
                bnl.Breadcrumb += newbnl.Breadcrumb;
                bnl.BlockFound = newbnl.BlockFound;
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
            return bnl;
        }

        private void btnAcknowledgeAlert_Click(object sender, EventArgs e)
        {
            var ackBlockName = lblBlockWarning.Text;
            var ackAlert = alerts.FirstOrDefault(f => f.BlockUserName == ackBlockName);
            ListViewItem item = new ListViewItem();

            if (ackAlert != null)
            {
                item.Text = "Acknowledged " + ackAlert.BlockUserName;
                item.BackColor = Color.LimeGreen;
                lvUpdates.Items.Add(item);
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
                alerts.Remove(ackAlert);
                ackAlert.Acknowledged = true;
                alerts.Remove(ackAlert);
            }
        }

        private async Task<bool> ProcessAlerts()
        {
            if (alerts.Count == 0)
            {
                lblBlockWarning.Text = "";
                lblLikelyIssue.Text = "";
            }

            var currentVisibleAlert = alerts.FirstOrDefault(f => f.Visible == true);
            List<Alert> alertsToRemove = new List<Alert>();

            try
            {
                foreach (var alert in alerts.OrderBy(o => o.Severity))
                {
                    var checkAlert = await NavigateThroughBlockItems(alert.BNL.BlockChecked,alert.BNL.PreviousBlock, alert.BNL.StartItem, alert.BNL.StartPreviousItem, alert.BNL.StartItem);
                    var checkAlertLiveBlock = await webClient.GetBlock(alert.BNL.BlockChecked);
                    var alertStillActive = !string.IsNullOrEmpty(checkAlert.LikelyIssue) || checkAlertLiveBlock.data.state == 2;
                    var liveBlock = await webClient.GetBlock(alert.BlockUserName);
                    if ((liveBlock != null && liveBlock.data != null && liveBlock.data.state == 4) || !alertStillActive)
                    {
                        //if block now unoccupied clear alert
                        alert.Acknowledged = false;
                        alertsToRemove.Add(alert);
                        continue;
                    }

                    if (alert.Severity == AlertSeverity.Danger)
                    {
                        var existingCuationAlert = alerts.FirstOrDefault(a => a.Severity == AlertSeverity.Caution && a.BlockUserName == alert.PreviousBlockUserName);
                        if (existingCuationAlert != null)
                        {
                            existingCuationAlert.Acknowledged = false;
                            existingCuationAlert.Superceded = true;
                            alertsToRemove.Add(existingCuationAlert);
                        }
                    }

                    TimeSpan timeDiff = DateTime.Now - alert.AlertStart;
                    bool showAlert = false;

                    if (alertStillActive && !alertsToRemove.Contains(alert))
                    {
                        if (alert.Severity == AlertSeverity.Caution)
                        {
                            if (timeDiff.TotalSeconds >= 10) showAlert = true;
                        }
                        else if (alert.Severity == AlertSeverity.Danger)
                        {
                            if (timeDiff.TotalSeconds > 5) showAlert = true;
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
                }
            }
            return true;
        }

        private void DisplayAlert(Alert alert)
        {
            lblBlockWarning.Text = alert.BlockUserName;
            lblBlockWarning.ForeColor = Color.White;
            lblLikelyIssue.Text = alert.LikelyIssue;
            lblLikelyIssue.ForeColor = Color.White;
            if (alert.Severity == AlertSeverity.Danger)
            {
                lblBlockWarning.BackColor = Color.Red;
                lblLikelyIssue.BackColor = Color.Red;
                lbOutput.Items.Add(alert.BlockUserName+" DANGER "+ alerts.Count.ToString());
                lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                ListViewItem item = new ListViewItem();
                item.Text = alert.BlockUserName+" "+alert.LikelyIssue+" in "+alert.BNL.BlockChecked;
                item.BackColor = Color.Red;
                lvUpdates.Items.Add(item);
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
            }
            else if (alert.Severity == AlertSeverity.Caution)
            {
                lblBlockWarning.BackColor = Color.OrangeRed;
                lblLikelyIssue.BackColor = Color.OrangeRed;
                lbOutput.Items.Add(alert.BlockUserName + " CAUTION " + alerts.Count.ToString());
                lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                ListViewItem item = new ListViewItem();
                item.Text = alert.BlockUserName + " " + alert.LikelyIssue + " in " + alert.BNL.BlockChecked;
                item.BackColor = Color.OrangeRed;
                lvUpdates.Items.Add(item);
                lvUpdates.Items[lvUpdates.Items.Count - 1].EnsureVisible();
            }

            lvUpdates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }
    }
}
