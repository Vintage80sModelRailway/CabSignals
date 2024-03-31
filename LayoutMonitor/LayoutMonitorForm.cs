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
        private List<KeyValuePair<string, string>> activeSignalMasts;
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
            activeSignalMasts = new List<KeyValuePair<string, string>>();
            activeBlocks = await webClient.GetOccupiedBlocks();
            alerts = new List<Alert>();
            while (monitorRuning)
            {
                await MonitorLayout();
                await ProcessAlerts();
                //await Task.Delay(50);
            }
        }

        private async Task<bool> MonitorLayout()
        {
            var newActiveBlocks = await webClient.GetOccupiedBlocks();

            foreach (var nab in newActiveBlocks)
            {                
                var alreadyExists = activeBlocks.Any(a => a.data.name == nab.data.name);
                if (!alreadyExists)
                {
                    //new block gone occupied
                    //var boundaries = config.GetBoundariesForBlock(nab.data.userName);
                    var likelyNextBlock = "";
                    var likelyPreviousBlock = "";
                    var unlikelyNextBlock = "";
                    lbOutput.Items.Add("New active block " + nab.data.userName);
                    var thisBlock = config.GetBlockBySystemName(nab.data.name);
                    int direction = -1;
                    bool oneConnectedBlockUnoccipied = false;
                    bool oneConnectedBlockOccupied = false;
                    List<BlockRootObject> nextBlocks = new List<BlockRootObject>();

                    foreach (var connectedBlock in thisBlock.path)
                    {
                        var configBlock = config.GetBlockBySystemName(connectedBlock.block);
                        var liveBlock = await webClient.GetBlock(configBlock.userName);
                        if (liveBlock.data.state == 2) //occupied
                        {
                            likelyPreviousBlock = liveBlock.data.userName;
                            break;
                        }
                    }

                    var turnouts = config.GetTurnoutsInBlock(nab.data.userName);
                    var to = turnouts.FirstOrDefault();
                    if (to != null)
                    {
                        var configTurnout = config.GetTurnoutByUserName(to.turnoutname);
                        var liveTurnout = await webClient.GetTurnout(configTurnout.systemName);
                        var state = liveTurnout.data.state;
                        string navigatedForwardPath = to.ident+";";
                        string navigatedBackwardPath = to.ident + ";";
                        if (to.type.Contains("XOVER"))
                        {
                            if (to.blockname == nab.data.userName)
                            {
                                if (state == 4)
                                {
                                    likelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectcname, to.ident, ref navigatedForwardPath);
                                    unlikelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectbname, to.ident, ref navigatedBackwardPath);
                                }
                                else
                                {
                                    unlikelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectcname, to.ident, ref navigatedBackwardPath);
                                    likelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectbname, to.ident, ref navigatedForwardPath);
                                }
                            }
                            else if(to.blockcname == nab.data.userName || to.blockdname == nab.data.userName)
                            {
                                if (state == 4)
                                {
                                    likelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectaname, to.ident, ref navigatedForwardPath);
                                    unlikelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectdname, to.ident, ref navigatedBackwardPath);
                                }
                                else
                                {
                                    unlikelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectaname, to.ident, ref navigatedBackwardPath);
                                    likelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectdname, to.ident, ref navigatedForwardPath);
                                }
                                
                            }
                        }
                        else
                        {
                            if (state == 4)
                            {
                                var connection = to.connectcname;
                                likelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, connection, to.ident, ref navigatedForwardPath);
                                unlikelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectbname, to.ident, ref navigatedBackwardPath);
                                if (likelyNextBlock == likelyPreviousBlock)
                                {
                                    //turnout facing train
                                    likelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectaname, to.ident, ref navigatedForwardPath);
                                    unlikelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectbname, to.ident, ref navigatedBackwardPath);
                                }
                            }
                            else
                            {
                                //closed
                                likelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectbname, to.ident, ref navigatedForwardPath);
                                unlikelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectaname, to.ident, ref navigatedBackwardPath);
                                if (likelyNextBlock == likelyPreviousBlock)
                                {
                                    likelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectaname, to.ident, ref navigatedForwardPath);
                                    unlikelyNextBlock = config.GetNextBlockForLayoutItem(nab.data.userName, to.connectcname, to.ident, ref navigatedBackwardPath);
                                }
                            }
                        }
                     

                        //4 = thrown
                    }
                    
                    //turnout
                    //connectbname = closed
                    //connectcname = thrown
                    
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
                        else if (liveBlock.data.state == 4 && likelyNextBlock == "" && liveBlock.data.userName != unlikelyNextBlock) //unoccupied
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

                    string smDirection = "West";
                    if (direction == 64 || direction == 80) //northeast?
                    {
                        smDirection = "East";
                    }

                    Enums.direction dir = (Enums.direction)direction;
                    smDirection = dir.ToString();


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
                    foreach(var block in nextBlocks)
                    {
                        lbOutput.Items.Add("Next block " + block.data.userName);
                        List<string> nextSignals = new List<string>();
                        //var storedSM = activeSignalMasts.FirstOrDefault(f => f.Key == nab.data.name);
                        //var oneExists = activeSignalMasts.Any(a => a.Key == nab.data.name);
                        //if (oneExists)
                        //{
                        //    nextSignals = config.GetSignalDestinationMasts(storedSM.Value);
                        //    lbOutput.Items.Add("Found next signals");
                        //}
                        var sm = config.GetSignalMastForBlock(nab.data.userName, block.data.userName, smDirection, null);
                        
                        if (sm != null)
                        {
                            smFound = true;
                            lbOutput.Items.Add("Signal mast " + sm.userName);
                            //var nextOneExists = activeSignalMasts.Any(a => a.Key == block.data.name);
                            //if (!nextOneExists)
                            //{
                            //    activeSignalMasts.Add(new KeyValuePair<string, string>(block.data.name, sm.userName));
                            //}
                            
                            var liveSM = await webClient.GetSignalMast(sm.systemName);
                            if (liveSM != null)
                            {
                                if (liveSM.data.state == "Danger")
                                {
                                    alerts.Add(new Alert()
                                    {
                                        BlockSystemName = nab.data.name,
                                        BlockUserName = nab.data.userName,
                                        SignalMastSystemName = sm.systemName,
                                        SignalMastUserName = sm.userName,
                                        Severity = AlertSeverity.Danger
                                    }) ;
                                }
                                else if (liveSM.data.state == "Caution")
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
                                else if (liveSM.data.state == "Proceed")
                                {
                                    lbOutput.Items.Add(("Proceed " + nab.data.userName));
                                }
                                    
                            }
                        }
                    }
                }
            }

            //var goneInactive = activeBlocks.Except(newActiveBlocks);
            var goneInactive = activeBlocks.Where(x => !newActiveBlocks.Select(i => i.data.name).Contains(x.data.name));
            foreach (var inactive in goneInactive)
            {
                var relatedAlert = alerts.FirstOrDefault(f => f.BlockUserName == inactive.data.userName);
                if (relatedAlert != null)
                {
                    alerts.Remove(relatedAlert);
                }
            }

            activeBlocks = newActiveBlocks;
            return true;
        }

        private void btnStopMonitoring_Click(object sender, EventArgs e)
        {
            monitorRuning = false;
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
                foreach (var alert in alerts)
                {
                    var liveSM = await webClient.GetSignalMast(alert.SignalMastSystemName);
                    if (liveSM != null && liveSM.data.state == "Proceed")
                    {
                        alertsToRemove.Add(alert);
                    }
                    if (currentVisibleAlert == null)
                    {
                        if (liveSM.data.state != "Proceed")
                        {
                            DisplayAlert(alert);
                            alerts[i].Visible = true;
                            alerts[i].AlertStart = DateTime.Now;
                            var colour = alert.Severity.ToString();
                            SoundPlayer signalBeep = new SoundPlayer("./Assets/" + colour + ".wav");
                            signalBeep.Play();
                        }
                    }
                    else if (alert.Visible == true)
                    {
                        currentAlertIndex = i;
                    }
                    else
                    {
                        if (i > currentAlertIndex && liveSM.data.state != "Proceed")
                        {
                            var activeAlert = alerts.ElementAtOrDefault(currentAlertIndex);
                            if (activeAlert != null)
                            {
                                TimeSpan timeDiff = DateTime.Now - activeAlert.AlertStart;
                                if (timeDiff.TotalSeconds > 3)
                                {
                                    alerts[currentAlertIndex].Visible = false;
                                    alerts[i].Visible = true;
                                    alerts[i].AlertStart = DateTime.Now;
                                    DisplayAlert(alert);
                                    var colour = alert.Severity.ToString();
                                    SoundPlayer signalBeep = new SoundPlayer("./Assets/" + colour + ".wav");
                                    signalBeep.Play();
                                }
                            }
                        }
                    }
                    i++;
                }
            }
            catch (Exception ex)
            {

            }
            foreach (var alert in alertsToRemove)
            {
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
            }
            else if (alert.Severity == AlertSeverity.Caution)
            {
                lblBlockWarning.BackColor = Color.OrangeRed;
            }
        }
    }
}
