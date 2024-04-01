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
                        string connectingAnchorPoint = "";
                        List<BlockRootObject> nextBlocks = new List<BlockRootObject>();
                        List<string> BoundaryBlocks = new List<string>();

                        var trackSegments = config.GetTrackSegmentsForBlock(nab.data.userName).OrderBy(o => o.ident).ToList();
                        var ts = trackSegments.FirstOrDefault();
                        if (ts == null) return false;
                        var firstBoundaryFromMiddle = await NavigateThroughBlockItems(nab.data.userName, ts.connect1name, ts.ident);
                        var secondBoundary = await NavigateThroughBlockItems(nab.data.userName, firstBoundaryFromMiddle.EdgeConnectorDirectionConnector, firstBoundaryFromMiddle.EdgeConnector);
                        var firstBoundary = await NavigateThroughBlockItems(nab.data.userName, secondBoundary.EdgeConnectorDirectionConnector, secondBoundary.EdgeConnector);


                        //if (nab.data.name == "AC Yard Entry")
                        //{
                        //    likelyPreviousBlock = "AC Yard bypass PC End";
                        //    var configBlock = config.GetBlockBySystemName(likelyPreviousBlock);
                        //    var liveBlock = await webClient.GetBlock(configBlock.userName);
                        //    if (liveBlock.data.state == 2)
                        //    {
                        //        oneConnectedBlockOccupied = true;
                        //    }
                        //    else
                        //    {
                        //        oneConnectedBlockUnoccipied = true;
                        //    }
                        //}
                        //else if (nab.data.name == "CW Yard Lower Entrance")
                        //{
                        //    likelyPreviousBlock = "CW Yard bypass Pi end";
                        //    var configBlock = config.GetBlockBySystemName(likelyPreviousBlock);
                        //    var liveBlock = await webClient.GetBlock(configBlock.userName);
                        //    if (liveBlock.data.state == 2)
                        //    {
                        //        oneConnectedBlockOccupied = true;
                        //    }
                        //    else
                        //    {
                        //        oneConnectedBlockUnoccipied = true;
                        //    }
                        //}
                        //else
                        //{
                        foreach (var connectedBlock in thisBlock.path)
                            {
                                var configBlock = config.GetBlockBySystemName(connectedBlock.block);
                                var liveBlock = await webClient.GetBlock(configBlock.userName);
                                if (liveBlock.data.state == 2 && (liveBlock.data.userName == firstBoundary.BlockFound || liveBlock.data.userName == secondBoundary.BlockFound)) //occupied
                                {
                                    likelyPreviousBlock = liveBlock.data.userName;
                                    oneConnectedBlockOccupied = true;
                                }
                                else
                                {
                                    oneConnectedBlockUnoccipied = true;
                                }
                            }
                        //}

                        if (!oneConnectedBlockOccupied)
                        {
                            lbOutput.Items.Add("No connected active blocks, done nothing for " + nab.data.userName);
                            activeBlocks = newActiveBlocks;
                            return false;
                        }


                        //if (nab.data.userName == "AC Yard Exit") likelyNextBlock = "AC Yard bypass Pi end";
                        //if (nab.data.userName == "CW Yard Lower Exit") likelyNextBlock = "CW Yard bypass PC End";


                        if (ts != null)
                        {
                            //this track segment might be in the middle of a block, after a set turnout, so may miss some of the path through and come up with the wrong answer
                            //so use it to get to an edge connector for the block
                            var bnl = await NavigateThroughBlockItems(nab.data.userName, firstBoundary.EdgeConnectorDirectionConnector, firstBoundary.EdgeConnector);
                            if (bnl.BlockFound == likelyPreviousBlock)
                            {
                                //we went in the wrong direction but got to the start of the block we need and now know the direction to go in
                                var result = await NavigateThroughBlockItems(nab.data.userName, bnl.EdgeConnectorDirectionConnector, bnl.EdgeConnector);
                                //A full navigation through the block should come up with the right answer
                                likelyNextBlock = result.BlockFound;
                                if (result.EdgeConnectorDirectionConnector.StartsWith("A"))
                                {
                                    connectingAnchorPoint = result.EdgeConnectorDirectionConnector;
                                }

                            }
                            else
                            {
                                //we went in the right direction but not necessarily from the start of the block, so go back
                                //var result = await NavigateThroughBlockItems(nab.data.userName, bnl.EdgeConnectorDirectionConnector, bnl.EdgeConnector);
                                //result gets us to the start of the block so now go back again through the whole block in the correct direction
                                //var fullNav = await NavigateThroughBlockItems(nab.data.userName, result.EdgeConnectorDirectionConnector, result.EdgeConnector);
                                likelyNextBlock = bnl.BlockFound;
                                if (bnl.EdgeConnectorDirectionConnector.StartsWith("A"))
                                {
                                    connectingAnchorPoint = bnl.EdgeConnectorDirectionConnector;
                                }
                            }
                        }

                        foreach (var connectedBlock in thisBlock.path)
                        {
                            var configBlock = config.GetBlockBySystemName(connectedBlock.block);
                            var liveBlock = await webClient.GetBlock(configBlock.userName);
                            if (liveBlock.data.userName == likelyNextBlock)
                            {
                                direction = connectedBlock.todir;
                                oneConnectedBlockUnoccipied = true;
                                nextBlocks.Add(liveBlock);
                            }
                            else if (liveBlock.data.state == 4 && likelyNextBlock == "") //unoccupied
                            {
                                direction = connectedBlock.todir;
                                oneConnectedBlockUnoccipied = true;
                                nextBlocks.Add(liveBlock);
                            }
                            else if (liveBlock.data.state == 2)
                            {
                                oneConnectedBlockOccupied = true;
                                likelyPreviousBlock = liveBlock.data.userName;
                            }
                        }


                        Enums.direction dir = (Enums.direction)direction;
                        var smDirection = dir.ToString();


                        if (!oneConnectedBlockUnoccipied)
                        {
                            //danger
                            alerts.Add(new Alert()
                            {
                                BlockSystemName = nab.data.name,
                                BlockUserName = nab.data.userName,
                                SignalMastSystemName = "",
                                SignalMastUserName = "",
                                Severity = AlertSeverity.Danger
                            });
                        }
                        else if (!oneConnectedBlockOccupied && likelyNextBlock == "")
                        {
                            //single isolated block occupied - do nothing
                            activeBlocks = newActiveBlocks;
                            return true;
                        }
                        bool smFound = false;
                        foreach (var block in nextBlocks)
                        {
                            lbOutput.Items.Add("Next block " + block.data.userName);
                            signalmast sm = new signalmast();
                            var signalMastName = "";
                            if (connectingAnchorPoint != "")
                            {
                                var ap = config.GetTrackLayoutAnchorPoint(connectingAnchorPoint);
                                string derivedDirection = config.GetDerivedDirection(smDirection);
                                if (derivedDirection == "East")
                                {
                                    signalMastName = ap.eastboundsignalmast;
                                }
                                else
                                {
                                    signalMastName = ap.westboundsignalmast;
                                }
                                sm = config.GetSignalMastByUserName(signalMastName);
                            }
                            else
                            {
                                sm = config.GetSignalMastForBlock(nab.data.userName, block.data.userName, smDirection, null);
                            }

                            if (sm != null && !String.IsNullOrEmpty(sm.userName))
                            {
                                smFound = true;
                                lbOutput.Items.Add("Signal mast " + signalMastName);
                                var liveSM = await webClient.GetSignalMast(sm.systemName);
                                if (liveSM != null)
                                {
                                    if (liveSM.data.state == "Danger")
                                    {
                                        var alertExists = alerts.Any(a => a.BlockSystemName == nab.data.name && a.Severity == AlertSeverity.Danger);
                                        if (!alertExists)
                                        {
                                            alerts.Add(new Alert()
                                            {
                                                BlockSystemName = nab.data.name,
                                                BlockUserName = nab.data.userName,
                                                SignalMastSystemName = sm.systemName,
                                                SignalMastUserName = sm.userName,
                                                Severity = AlertSeverity.Danger
                                            });
                                        }
                                    }
                                    else if (liveSM.data.state == "Caution")
                                    {
                                        var alertExists = alerts.Any(a => a.BlockSystemName == nab.data.name && a.Severity == AlertSeverity.Caution);
                                        if (!alertExists)
                                        {
                                            alerts.Add(new Alert()
                                            {
                                                BlockSystemName = nab.data.name,
                                                BlockUserName = nab.data.userName,
                                                SignalMastSystemName = sm.systemName,
                                                SignalMastUserName = sm.userName,
                                                Severity = AlertSeverity.Caution
                                            });
                                        }
                                    }
                                    else if (liveSM.data.state == "Proceed")
                                    {
                                        lbOutput.Items.Add(("Proceed " + nab.data.userName));
                                        lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                                    }
                                }
                            }
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
            monitorRuning = false;
        }

        private string FindEdgeOfBlock(string currentBlock, string layoutItem, string previousLayoutItem)
        {
            return "";
        }

        private async Task<BlockNavigationLog> NavigateThroughBlockItems(string currentBlock, string LayoutItem, string previousLayoutItem)
        {
            var bnl = new BlockNavigationLog();
            bnl.Breadcrumb = LayoutItem + ";";
            string newBlockName = "";
            //navigatedPath += LayoutItem + ":";
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

                if (derivedXoverBlockName != currentBlock)
                {
                    bnl.EdgeConnector = to.ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    bnl.BlockFound = to.blockname;
                }
                else
                {

                    if (liveTurnout.data.state == 4)
                    {
                        //thrown

                        string nextItemIdent = "";
                        //need to determine direction of travel. If one of the C or B connectors matches the previousLayout Item, we're traversing head on.
                        if (to.connectbname == previousLayoutItem || to.connectcname == previousLayoutItem)
                        {
                            nextItemIdent = to.connectaname;
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

                        bnl.Breadcrumb += nextItemIdent + ";";
                        var newbnl = await NavigateThroughBlockItems(currentBlock, nextItemIdent, to.ident);
                        bnl.Breadcrumb += newbnl.Breadcrumb;
                        bnl.BlockFound = newbnl.BlockFound;
                        bnl.EdgeConnector = newbnl.EdgeConnector;
                        bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                    }
                    else
                    {
                        //closed
                        string nextItemIdent = "";
                        //need to determine direction of travel. If one of the C or B connectors matches the previousLayout Item, we're traversing head on.
                        if (to.type.Contains("XOVER"))
                        {
                            if (liveTurnout.data.state == 2)
                            {
                                //closed
                                
                            }
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
                                    }
                                    nextItemIdent = to.connectbname;
                                }
                            }
                        }
                        else
                        {
                            if (to.connectbname == previousLayoutItem || to.connectcname == previousLayoutItem)
                            {
                                nextItemIdent = to.connectaname;
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
                        var newbnl = await NavigateThroughBlockItems(currentBlock, nextItemIdent, to.ident);
                        bnl.Breadcrumb += newbnl.Breadcrumb;
                        bnl.BlockFound = newbnl.BlockFound;
                        bnl.EdgeConnector = newbnl.EdgeConnector;
                        bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                    }
                }
            }
            else if (LayoutItem.Substring(0, 1) == "T")
            {
                //track
                var ts = config.GetLayoutTrackSegment(LayoutItem);
                if (ts.blockname != currentBlock)
                {
                    bnl.EdgeConnector = ts.ident;
                    bnl.EdgeConnectorDirectionConnector = previousLayoutItem;
                    bnl.BlockFound = ts.blockname;
                }
                else
                {
                    var nextItem = ts.connect2name;
                    if (ts.connect2name == previousLayoutItem) nextItem = ts.connect1name;
                    bnl.Breadcrumb += nextItem + ";";
                    var newbnl = await NavigateThroughBlockItems(currentBlock, nextItem, ts.ident);
                    bnl.Breadcrumb += newbnl.Breadcrumb;
                    bnl.BlockFound = newbnl.BlockFound;
                    bnl.EdgeConnector = newbnl.EdgeConnector;
                    bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
                }
            }
            else if (LayoutItem.Substring(0, 1) == "A")
            {
                //anchor
                var a = config.GetTrackLayoutAnchorPoint(LayoutItem);
                var nextItem = a.connect2name;
                if (a.connect2name == previousLayoutItem) nextItem = a.connect1name;
                bnl.Breadcrumb += nextItem + ";";
                var newbnl = await NavigateThroughBlockItems(currentBlock, nextItem, a.ident);
                bnl.Breadcrumb += newbnl.Breadcrumb;
                bnl.BlockFound = newbnl.BlockFound;
                bnl.EdgeConnector = newbnl.EdgeConnector;
                bnl.EdgeConnectorDirectionConnector = newbnl.EdgeConnectorDirectionConnector;
            }
            return bnl;
        }

        private void btnAcknowledgeAlert_Click(object sender, EventArgs e)
        {
            var ackBlockName = lblBlockWarning.Text;
            var ackAlert = alerts.FirstOrDefault(f => f.BlockUserName == ackBlockName);
            if (ackAlert != null)
            {
                alerts.Remove(ackAlert);
            }
        }

        private async Task<bool> ProcessAlerts()
        {
            if (alerts.Count == 0)
            {
                lblBlockWarning.Text = "";
            }

            var currentVisibleAlert = alerts.FirstOrDefault(f => f.Visible == true);
            int i = 0;
            int currentAlertIndex = -1;
            List<Alert> alertsToRemove = new List<Alert>();

            try
            {
                foreach (var alert in alerts.OrderBy(o => o.Severity))
                {
                    var liveBlock = await webClient.GetBlock(alert.BlockUserName);
                    if (liveBlock != null && liveBlock.data != null && liveBlock.data.state == 4)
                    {
                        //if block now unoccupied clear alert
                        alertsToRemove.Add(alert);
                        continue;
                    }
                    var liveSM = await webClient.GetSignalMast(alert.SignalMastSystemName);
                    if (liveSM != null && liveSM.data != null && liveSM.data.state == "Proceed")
                    {
                        alertsToRemove.Add(alert);
                        continue;
                    }
                    if (alert.Severity == AlertSeverity.Danger)
                    {
                        var existingCuationAlert = alerts.FirstOrDefault(a => a.Severity == AlertSeverity.Caution && a.BlockSystemName == alert.BlockSystemName);
                        if (existingCuationAlert != null)
                        {
                            alertsToRemove.Add(alert);
                        }
                    }

                    TimeSpan timeDiff = DateTime.Now - alert.AlertStart;
                    if (liveSM.data.state != "Proceed" && !alertsToRemove.Contains(alert) && timeDiff.TotalSeconds > 5)
                    {
                        DisplayAlert(alert);
                        alert.Visible = true;
                        alert.AlertStart = DateTime.Now;
                        var colour = alert.Severity.ToString();
                        SoundPlayer signalBeep = new SoundPlayer("./Assets/" + colour + ".wav");
                        signalBeep.Play();                        
                    }

                    //else
                    //{
                    //    if (i > currentAlertIndex && liveSM.data.state != "Proceed")
                    //    {
                    //        var activeAlert = alerts.ElementAtOrDefault(currentAlertIndex);
                    //        if (activeAlert != null)
                    //        {
                    //            TimeSpan timeDiff = DateTime.Now - activeAlert.AlertStart;
                    //            if (timeDiff.TotalSeconds > 3)
                    //            {
                    //                alerts[currentAlertIndex].Visible = false;
                    //                alerts[i].Visible = true;
                    //                alerts[i].AlertStart = DateTime.Now;
                    //                DisplayAlert(alert);
                    //                var colour = alert.Severity.ToString();
                    //                SoundPlayer signalBeep = new SoundPlayer("./Assets/" + colour + ".wav");
                    //                signalBeep.Play();
                    //            }
                    //        }
                    //    }
                    //}
                    i++;
                }
            }
            catch (Exception ex)
            {

            }
            foreach (var alert in alertsToRemove)
            {
                if (alerts.Contains(alert))
                    alerts.Remove(alert);
            }
            return true;
        }

        private void DisplayAlert(Alert alert)
        {
            lblBlockWarning.Text = alert.BlockUserName;
            lblBlockWarning.ForeColor = Color.White;
            if (alert.Severity == AlertSeverity.Danger)
            {
                lblBlockWarning.BackColor = Color.Red;
                lbOutput.Items.Add(alert.BlockUserName+" DANGER "+ alerts.Count.ToString());
                lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
            }
            else if (alert.Severity == AlertSeverity.Caution)
            {
                lblBlockWarning.BackColor = Color.OrangeRed;
                lbOutput.Items.Add(alert.BlockUserName + " CAUTION " + alerts.Count.ToString());
                lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
            }
        }

        private async void btnTestNavigation_Click(object sender, EventArgs e)
        {
            config = new ConfigReader(tbConfigLocation.Text);
            webClient = new JSONReader("http://" + tbServerIP.Text + ":" + tbServerPort.Text);
            var navPath = "";
            var block = await NavigateThroughBlockItems("AC Yard Entry", "TO19", "T60");
        }
    }
}
