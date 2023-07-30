using JMRIReader;
using JMRIReader.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace JMRIDebugOutput
{
    public partial class Form1 : Form
    {
        private JSONReader webClient;
        private ConfigReader config;        

        private bool journeyRunning;
        public Form1()
        {
            InitializeComponent();

            this.Size = new Size(142, 489);


            var cfgFilePath = ConfigurationManager.AppSettings["ConfigFilePath"];
            if (cfgFilePath != null)
            {
                tbConfigLocation.Text = cfgFilePath.ToString();
            }

            var cfgDispatchesFolder = ConfigurationManager.AppSettings["DispatchesFolder"];
            if (cfgDispatchesFolder != null)
            {
                tbDispatchPath.Text = cfgDispatchesFolder.ToString();
            }

            var cfgWebServerIP = ConfigurationManager.AppSettings["WebServerIP"];
            if (cfgWebServerIP != null)
            {
                tbJMRIWebServerIP.Text = cfgWebServerIP.ToString();
            }

            var cfgWebServerPort = ConfigurationManager.AppSettings["WebServerPort"];
            if (cfgWebServerPort != null)
            {
                tbWebServerPort.Text = cfgWebServerPort.ToString();
            }           


            journeyRunning = false;

            pbSignal.Load("./Assets/off.png");


        }

        private void btnConfigLocationBrowse_Click(object sender, EventArgs e)
        {
            ofConfigFile.ShowDialog();
            tbConfigLocation.Text = ofConfigFile.FileName;
        }

        private void btnDispatchPath_Click(object sender, EventArgs e)
        {
            fbDispatchesPath.ShowDialog();
            tbDispatchPath.Text = fbDispatchesPath.SelectedPath;
        }

        private void btnOpenFiles_Click(object sender, EventArgs e)
        {
            cbDispatches.Items.Clear();
            var dispatches = Directory.GetFiles(tbDispatchPath.Text);
            if (dispatches.Length > 0)
            {
                cbDispatches.Enabled = true;
                btnOpenDispatch.Enabled = true;
            }
            foreach (var file in dispatches)
            {
                var split = file.Split('\\');
                var filename = split[split.Length - 1];
                cbDispatches.Items.Add(filename);
            }

            config = new ConfigReader(tbConfigLocation.Text);
            webClient = new JSONReader("http://"+tbJMRIWebServerIP.Text+":"+tbWebServerPort.Text);
        }

        private void btnOpenDispatch_Click(object sender, EventArgs e)
        {
            var filename = tbDispatchPath.Text + "\\" + cbDispatches.SelectedItem;
            if (File.Exists(filename))
            {
                XmlDocument dispatch = new XmlDocument();
                dispatch.Load(filename);
                var stop = "";
                XmlNode dispatchData = dispatch.DocumentElement.SelectSingleNode("/traininfofile/traininfo");
                if (dispatchData == null)
                {
                    lbOutput.Items.Add("Sorry, no dispatch data found");
                }
                var transit = dispatchData.Attributes["transitid"];
                var startBlock = dispatchData.Attributes["startblockid"];
                var trainName = dispatchData.Attributes["trainname"];

                tbTransit.Text = transit.Value;
                tbStartBlock.Text = startBlock.Value;
                tbTrainName.Text = trainName.Value;
            }
        }

        private async void btnStartJourney_Click(object sender, EventArgs e)
        {
            try
            {
                await TrackJourney();
            }
            catch (Exception ex)
            {
                lbOutput.Items.Add("Exception - " + ex.Message);
            }
            
        }

        private async Task TrackJourney()
        {
            int currentSectionIndex = 0;
            int currentBlockIndex = 0;

            string direction = "";

            journeyRunning = true;

            BlockRootObject block = await webClient.GetBlock(tbStartBlock.Text);

            lbOutput.Items.Add("Current block " + block.data.userName);
            var journey = new Journey(tbConfigLocation.Text, tbTransit.Text, tbTrainName.Text, tbStartBlock.Text, "ed");
            var transit = journey.GetTransit();

            var currentBlock = transit.BlocksInOrder.FirstOrDefault(f => f.userName == tbStartBlock.Text);

            var configStartBlockIndex = transit.BlocksInOrder.IndexOf(currentBlock);
            var nextBlock = transit.BlocksInOrder.ElementAt(configStartBlockIndex + 1);

            var currentSection = transit.Sections.ElementAt(currentSectionIndex);

            tbNextBlock.Text = nextBlock.userName;
            tbCurrentBlock.Text = currentBlock.userName;

            var currentLveBlock = await webClient.GetBlock(tbStartBlock.Text);
            var nextLiveBlock = await webClient.GetBlock(tbNextBlock.Text);

            direction = GetDirectionFromSectionAndTransit(transit, currentSectionIndex);
            await UpdateSignalStatus(currentLveBlock.data.userName, nextLiveBlock.data.userName, direction,false);

            while (journeyRunning)
            {
                var newCurrentBlock = await webClient.GetBlock(currentLveBlock.data.userName);
                var newNextBlock = await webClient.GetBlock(nextLiveBlock.data.userName);

                int currentNextBlockstate = nextLiveBlock.data.state;
                int newNextBlockstate = newNextBlock.data.state;

                //if (newCurrentBlock.data.state == 4 || newNextBlock.data.state == 1)
                if (nextLiveBlock.data.state != newNextBlock.data.state && newNextBlock.data.value == tbTrainName.Text)
                {
                    var previousBlock = currentLveBlock;
                    lbOutput.Items.Add("State change on block?");
                    currentBlockIndex++;

                    if (currentBlockIndex >= transit.BlocksInOrder.Count - 1)
                    {
                        journeyRunning = false;
                        break;
                    }

                    currentLveBlock = newNextBlock;
                    var nextdbBlock = transit.BlocksInOrder.ElementAt(currentBlockIndex + 1);
                    nextLiveBlock = await webClient.GetBlock(nextdbBlock.userName);
                    lbOutput.Items.Add("Block change detected - new block " + currentLveBlock.data.userName + " and next block " + nextLiveBlock.data.userName+" block index "+currentBlockIndex.ToString());

                    var currentBlockInCurrentSection = transit.Sections.ElementAt(currentSectionIndex).Blocks.FirstOrDefault(f => f.userName == currentLveBlock.data.userName);
                    if (currentBlockInCurrentSection == null)
                    {
                        currentSectionIndex++;
                        direction = UpdateSectionStatusInfoAndGetDirection(transit, currentSectionIndex, previousBlock.data.name, currentLveBlock.data.name, nextLiveBlock.data.name);
                    }
                    tbCurrentBlock.Text = currentLveBlock.data.userName;
                    tbNextBlock.Text = nextLiveBlock.data.userName;
                    await UpdateSignalStatus(currentLveBlock.data.userName, nextLiveBlock.data.userName, direction, true);
                    lbOutput.SelectedIndex = lbOutput.Items.Count - 1;
                }
                else
                {
                    await UpdateSignalStatus(currentLveBlock.data.userName, nextLiveBlock.data.userName, direction, false);
                }
            }
            CompleteJourney();
        }

        protected async Task UpdateSignalStatus(string currentLiveBlockUsername, string nextLiveBlockUsername, string direction, bool blockHasChanged)
        {
            var sm = config.GetSignalMastForBlock(currentLiveBlockUsername, nextLiveBlockUsername, direction);
            if (sm == null)
            {
                return;
            }

            tbCurrentBlockSignalMast.Text = sm.userName;

            //always check signal state regardless of whether block state has changed
            if (sm == null) return;
            var sh = config.GetSignalHeadForMastName(sm.userName);
            var colour = "";
            foreach (var sigPointer in sh.turnoutname)
            {
                colour = sigPointer.defines;
                var sig = await webClient.GetTurnout(sigPointer.Value);
                if (sig.data.state == 4) //closed = 2, thrown = 4
                {
                    break;
                }
            }
            if (colour != tbCurrentBlockSignalMastState.Text)
            {
                pbSignal.Load("./Assets/" + colour + ".png");
                tbCurrentBlockSignalMastState.Text = colour;
                SoundPlayer signalBeep = new SoundPlayer("./Assets/"+colour+".wav");
                signalBeep.Play();
            }
            else if (blockHasChanged)
            {
                SoundPlayer signalBeep = new SoundPlayer("./Assets/" + colour + ".wav");
                signalBeep.Play();
            }
        }

        protected string UpdateSectionStatusInfoAndGetDirection(transit transit, int currentSectionIndex, string previousLiveBlockName, string currentLiveBlockName, string nextLiveBlockName)
        {
            string direction = "";
            var dir = transit.transitsection.ElementAt(currentSectionIndex).direction;
            lbOutput.Items.Add("Section change detected - new section " + transit.transitsection.ElementAt(currentSectionIndex).sectionname + " direction " + dir.ToString());
            var entryBlock = transit.Sections.ElementAt(currentSectionIndex + 1).entrypoint.Where(w => w.toblock == currentLiveBlockName && w.fromblock == previousLiveBlockName).ToList();
            entryBlock = transit.Sections.ElementAt(currentSectionIndex + 1).entrypoint.Where(w => w.toblock == nextLiveBlockName && w.fromblock == currentLiveBlockName).ToList();
            lbOutput.Items.Add("Found " + entryBlock.Count.ToString() + " matching entry point blocks");
            var eb = entryBlock.FirstOrDefault();
            if (eb != null)
            {
                direction = eb.fromblockdirection;
                lbOutput.Items.Add("Section direction " + direction);
            }
            return direction;
        }

        private string GetDirectionFromSectionAndTransit(transit tr, int sectionIndex)
        {
            var transitSec = tr.transitsection.OrderBy(o => o.sequence).ElementAt(sectionIndex+1);
            var sec = tr.Sections.ElementAt(sectionIndex+1);

            var dir = transitSec.direction;
            var ep = sec.entrypoint.FirstOrDefault(f => f.direction == dir);
            if (ep != null)
            {
                return ep.fromblockdirection;
            }
            return "";
        }

        private void btnStopJourney_Click(object sender, EventArgs e)
        {
            CompleteJourney();
        }

        private void CompleteJourney()
        {
            journeyRunning = false;
            pbSignal.Load("./Assets/red.png");
            tbCurrentBlock.Text = "";
            tbNextBlock.Text = "";
            tbCurrentBlockSignalMast.Text = "";
            tbCurrentBlockSignalMastState.Text = "";
            SoundPlayer signalBeep = new SoundPlayer("./Assets/red.wav");
            signalBeep.Play();
        }

        private async void btnTestSM_Click(object sender, EventArgs e)
        {
            var config = new ConfigReader(tbConfigLocation.Text);


            var journey = new Journey(tbConfigLocation.Text, tbTransit.Text, tbTrainName.Text, tbStartBlock.Text, "ed");
            var transit = journey.GetTransit();
            var sm = config.GetSignalMastForBlock("CW Lower Junction PC End", "CW Lower Junction Pi End", "East");
            if (sm == null) return;
            var sh = config.GetSignalHeadForMastName(sm.userName);
            var colour = "";
            foreach (var sigPointer in sh.turnoutname)
            {
                colour = sigPointer.defines;
                var sig = await webClient.GetTurnout(sigPointer.Value);
                if (sig.data.state == 4) //closed = 2, thrown = 4
                {
                    break;
                }
            }
            string stop = "";
        }

        private void btnMore_Click(object sender, EventArgs e)
        {
            if (btnMore.Text == ">>")
            {
                this.Size = new Size(939, 489);
                btnMore.Text = "<<";
            }
            else
            {
                this.Size = new Size(142, 489);
                btnMore.Text = ">>";
            }
            
        }
    }
}
