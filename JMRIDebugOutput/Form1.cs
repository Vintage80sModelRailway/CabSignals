using JMRIReader;
using JMRIReader.Classes;
using JMRIReader.Classes.DTO;
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
                
            }
            catch (Exception ex)
            {
                lbOutput.Items.Add("Exception - " + ex.Message);
            }

            await TrackJourney();

        }

        private async Task TrackJourney()
        {
            int currentSectionIndex = 0;
            int currentBlockIndex = 0;
            int journeyAlternateOffset = 0;

            string direction = "";
            string signalMastName = "";
            List<BlockJourneyLog> log = new List<BlockJourneyLog>();
            List<SectionJourneyLog> sectionLog = new List<SectionJourneyLog>();

            journeyRunning = true;

            BlockRootObject block = await webClient.GetBlock(tbStartBlock.Text);
            

            var journey = new Journey(tbConfigLocation.Text, tbTransit.Text, tbTrainName.Text, tbStartBlock.Text, "ed");
            var transit = journey.GetTransit();


            lbOutput.Items.Add("Journey started - number of blocks: " + transit.BlocksInOrder.Count.ToString() + "; number of sections: " + transit.Sections.Count.ToString());
            lbOutput.Items.Add("Current block " + block.data.userName + " - waiting for train progress");

            var startLveBlock = await webClient.GetBlock(tbStartBlock.Text);
            BlockRootObject previousBlock = startLveBlock;

            var start = transit.BlocksInOrder.FirstOrDefault(f => f.BlockUserName == tbStartBlock.Text);
            var configStartBlockIndex = transit.BlocksInOrder.IndexOf(start);

            tbCurrentBlock.Text = startLveBlock.data.userName;

            direction = GetDirectionFromSectionAndTransit(transit.Sections, currentSectionIndex);
            var sm = GetSignalMastForBlock(transit.BlocksInOrder, tbCurrentBlockSignalMast.Text, currentBlockIndex, direction);
            if (sm.BlockJumped) lbOutput.Items.Add("Block jumped");

            signalMastName = sm.userName;

            var assignedAPIBlocks = await webClient.GetAssignedBlocks(tbTrainName.Text);
            var assignedBlocksLastTime = new List<BlockRootObject>();

            while (assignedAPIBlocks == null || assignedAPIBlocks.Count < 1)
            {
                await Task.Delay(1000);
                assignedAPIBlocks = await webClient.GetAssignedBlocks(tbTrainName.Text);
            }

            while (journeyRunning)
            {
                
                //2 == occupied, 4 == unoccupied
                var blockJustGoneLive = new BlockRootObject();

                assignedAPIBlocks = await webClient.GetAssignedBlocks(tbTrainName.Text);

                List<BlockRootObject> newBlocksThisTime = new List<BlockRootObject>();

                foreach (var ab in assignedAPIBlocks)
                {
                    var alreadyExists = assignedBlocksLastTime.Any(a => a.data.name == ab.data.name);
                    if (!alreadyExists) newBlocksThisTime.Add(ab);
                }

                foreach (var nb in newBlocksThisTime)
                {
                    int searchIndexLimit = -1;

                    //lbOutput.Items.Add(newBlocksThisTime.Count.ToString() + " new assigned blocks found");
                    bool foundMatch = false;

                    //for (int i = currentBlockIndex;i <= currentBlockIndex+assignedAPIBlocks.Count;i++)
                    for (int i = log.Count-1; i <= log.Count-1 + newBlocksThisTime.Count+journeyAlternateOffset; i++)
                    {
                        var possibleMatch = transit.BlocksInOrder.ElementAtOrDefault(i);
                        if (possibleMatch != null && possibleMatch.BlockSystemname == nb.data.name)
                        {
                            possibleMatch.Sequence = i;
                            log.Add(possibleMatch);
                            var sectionToAdd = transit.Sections.FirstOrDefault(f => f.Sequence == possibleMatch.SectionSequenceId);
                            sectionLog.Add(sectionToAdd);

                            lbOutput.Items.Add("Added to log: " + possibleMatch.BlockUserName+" at "+i.ToString()+" section sequence ID "+possibleMatch.SectionSequenceId.ToString());
                            foundMatch = true;
                            break;
                        }
                        searchIndexLimit = i;
                    }


                    if (!foundMatch)
                    {
                        if (nb.data.userName.Contains("Station 1"))
                        {
                            var stop = "test";
                        }
                        bool handlingAlternate = false;
                        var checkForAlternate = transit.BlocksInOrder.ElementAtOrDefault(searchIndexLimit);
                        lbOutput.Items.Add("Alternate checking searchindexlimit "+searchIndexLimit.ToString());
                        while (checkForAlternate != null && checkForAlternate.PossibleAlternate)
                        {
                            handlingAlternate = true;
                            searchIndexLimit++;
                            checkForAlternate = transit.BlocksInOrder.ElementAtOrDefault(searchIndexLimit);
                            lbOutput.Items.Add("Possible alt " + checkForAlternate.BlockUserName + " indexlimit " + searchIndexLimit.ToString());
                            journeyAlternateOffset++;
                        }
                        while (checkForAlternate != null && checkForAlternate.HasAlternate)
                        {
                            handlingAlternate = true;
                            searchIndexLimit++;
                            checkForAlternate = transit.BlocksInOrder.ElementAtOrDefault(searchIndexLimit);
                            lbOutput.Items.Add("Has alt " + checkForAlternate.BlockUserName + " indexlimit " + searchIndexLimit.ToString());
                            journeyAlternateOffset++;
                        }

                        if (handlingAlternate)
                        {
                            checkForAlternate.Sequence = searchIndexLimit;
                            log.Add(checkForAlternate);
                            var sectionToAdd = transit.Sections.FirstOrDefault(f => f.Sequence == checkForAlternate.SectionSequenceId);
                            sectionToAdd.Sequence = searchIndexLimit;
                            sectionLog.Add(sectionToAdd);

                            lbOutput.Items.Add("Added to log through alternate route handling: " + checkForAlternate.BlockUserName + " at " + searchIndexLimit.ToString() + " section sequence ID " + checkForAlternate.SectionSequenceId.ToString());

                            log = log.OrderBy(o => o.Sequence).ToList();
                            sectionLog = sectionLog.OrderBy(o => o.Sequence).ToList();
                            lbJourneyLog.Items.Clear();
                            foreach (var l in log)
                            {
                                var tr = l.Traversed == true ? ", traversed" : "";
                                lbJourneyLog.Items.Add(l.BlockUserName + tr);
                            }
                        }

                    }
                    else
                    {
                        log = log.OrderBy(o => o.Sequence).ToList();
                        sectionLog = sectionLog.OrderBy(o => o.Sequence).ToList();
                        lbJourneyLog.Items.Clear();
                        foreach (var l in log)
                        {
                            var tr = l.Traversed == true ? ", traversed" : "";
                            lbJourneyLog.Items.Add(l.BlockUserName+tr);
                        }
                    }
                }



                foreach (var assignedBlock in assignedAPIBlocks.Where(w => w.data.state == 2))
                {                    
                    var unassignedBlock = assignedBlocksLastTime.FirstOrDefault(f => f.data.name == assignedBlock.data.name && f.data.state == 4);
                    if (unassignedBlock != null)
                    {
                        blockJustGoneLive = unassignedBlock;
                        break;
                    }
                    else
                    {
                        blockJustGoneLive = null;
                    }
                }                

                assignedBlocksLastTime = assignedAPIBlocks;

                if (blockJustGoneLive != null && blockJustGoneLive.data != null)
                {
                    lbOutput.Items.Add("New live block - " + blockJustGoneLive.data.userName);
                }
                else
                {
                    await UpdateSignalStatus(sm, false);
                }                

                if (currentBlockIndex >= transit.BlocksInOrder.Count - 1)
                {
                    journeyRunning = false;
                    break;
                }

                if (currentSectionIndex >= transit.Sections.Count -1)
                {
                    journeyRunning = false;
                    break;
                }

                var currentIndex = currentBlockIndex;
                if (currentIndex > log.Count-1) currentIndex = transit.BlocksInOrder.Count - 1;
                //var scheduledBlockAtCurrentIndex = transit.BlocksInOrder.ElementAtOrDefault(currentIndex);
                var scheduledBlockAtCurrentIndex = log.ElementAtOrDefault(currentBlockIndex+1);

                try
                {
                    //if (newCurrentBlock.data.state == 4 || newNextBlock.data.state == 1)
                    if (blockJustGoneLive != null && blockJustGoneLive.data != null && scheduledBlockAtCurrentIndex != null && scheduledBlockAtCurrentIndex.BlockSystemname == blockJustGoneLive.data.name)
                    {
                        currentBlockIndex++;
                        lbOutput.Items.Add("Current block index now " + currentBlockIndex.ToString());

                        if (blockJustGoneLive.data.userName == "Incline Pi end")
                        {
                            var debug = "stop";
                        }

                        log.ElementAtOrDefault(currentBlockIndex).Traversed = true;

                        var nextdbBlock = log.ElementAtOrDefault(currentBlockIndex + 1);
                        if (nextdbBlock == null)
                        {
                            journeyRunning = false;
                            break;
                        }
                        var nextLiveBlock = await webClient.GetBlock(nextdbBlock.BlockUserName);
                        

                        var currentBlockInCurrentSection = sectionLog.ElementAtOrDefault(currentSectionIndex).Blocks.FirstOrDefault(f => f.userName == blockJustGoneLive.data.userName);
                        string altDirection = "";
                        if (currentBlockInCurrentSection == null)
                        {
                            currentSectionIndex++;
                            direction = UpdateSectionStatusInfoAndGetDirection(sectionLog, currentSectionIndex, previousBlock.data.name, blockJustGoneLive.data.name, nextLiveBlock.data.name);
                            altDirection = GetDirectionFromSectionAndTransit(sectionLog, currentSectionIndex);
                        }
                        
                        tbCurrentBlock.Text = blockJustGoneLive.data.userName;
                        tbNextBlock.Text = nextLiveBlock.data.userName;
                        sm = GetSignalMastForBlock(log, signalMastName, currentBlockIndex, direction);

                        if (!sm.BlockJumped)
                        {
                            signalMastName = sm.userName;
                            lbOutput.Items.Add("Block change - " + blockJustGoneLive.data.userName + "mast "+signalMastName+ " next block " + nextLiveBlock.data.userName + " block index " + currentBlockIndex.ToString());
                            tbCurrentBlockSignalMast.Text = signalMastName;
                        }
                        else
                        {
                            tbCurrentBlockSignalMast.Text = sm.userName;
                            lbOutput.Items.Add("Block change - " + blockJustGoneLive.data.userName + "mast stayed at " + signalMastName + " next block " + nextLiveBlock.data.userName + " block index " + currentBlockIndex.ToString());
                        }

                        

                        await UpdateSignalStatus(sm, true);                        

                        lbOutput.SelectedIndex = lbOutput.Items.Count - 1;

                        {
                            var scheduledBlock = "stop";
                        }

                        previousBlock = blockJustGoneLive;
                    }
                }
                catch (Exception ex)
                {
                    var scheduledBlock = "stop";
                }


            }
            CompleteJourney();
        }

        private signalmast GetSignalMastForBlock(List<BlockJourneyLog> journeyBlocksInOrder, string currentSignalMastName, int blockIndex, string direction)
        {
            signalmast sm = new signalmast();
            sm.userName = "NULL";

            if (currentSignalMastName == "")
            {
                sm = config.GetSignalMastForBlock(journeyBlocksInOrder, blockIndex, direction, null);
            }
            else
            {
                var nextSignals = config.GetSignalDestinationMasts(currentSignalMastName);
                sm = config.GetSignalMastForBlock(journeyBlocksInOrder, blockIndex, direction, nextSignals);
            }
            return sm;
        }



        protected async Task UpdateSignalStatus(signalmast sm, bool blockHasChanged)
        {
            //always check signal state regardless of whether block state has changed
            var sh = config.GetSignalHeadForMastName(sm.userName);
            var colour = "";
            if (sh == null || sh.turnoutname == null) return;

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

        protected string UpdateSectionStatusInfoAndGetDirection(List<SectionJourneyLog> sectionLog, int currentSectionIndex, string previousLiveBlockName, string currentLiveBlockName, string nextLiveBlockName)
        {
            try
            {
                string direction = "";
                var dir = sectionLog.ElementAt(currentSectionIndex).TransitSection.direction;
                lbOutput.Items.Add("Section change detected - new section " + sectionLog.ElementAt(currentSectionIndex).SectionkUserName + " direction " + dir.ToString());

                var entryBlock = sectionLog.ElementAt(currentSectionIndex + 1).Section.entrypoint.Where(w => w.toblock == nextLiveBlockName && w.fromblock == currentLiveBlockName).ToList();
                lbOutput.Items.Add("Found " + entryBlock.Count.ToString() + " matching entry point blocks");
                var eb = entryBlock.FirstOrDefault();
                if (eb != null)
                {
                    direction = eb.fromblockdirection;
                    lbOutput.Items.Add("Section direction " + direction);
                }
                return direction;
            }
            catch (Exception ex)
            {
                var test = "stop";
                return "";
            }
        }

        private string GetDirectionFromSectionAndTransit(List<SectionJourneyLog> sectionLog, int sectionIndex)
        {
            var transitSec = sectionLog.OrderBy(o => o.Sequence).ElementAtOrDefault(sectionIndex+1);
            //var transitSec = sectionLog.OrderBy(o => o.Sequence).ElementAtOrDefault(sectionIndex);
            if (transitSec == null) return string.Empty;

            var dir = transitSec.TransitSection.direction;
            var ep = transitSec.Section.entrypoint.FirstOrDefault(f => f.direction == dir);
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
            //var config = new ConfigReader(tbConfigLocation.Text);


            //var journey = new Journey(tbConfigLocation.Text, tbTransit.Text, tbTrainName.Text, tbStartBlock.Text, "ed");
            //var transit = journey.GetTransit();
            ////var sm = config.GetSignalMastForBlock("CW Lower Junction PC End", "Incline Bottom", "East");
            //var sm = config.GetSignalMastForBlock(transit.BlocksInOrder, 27, "East");
            //if (sm == null) return;
            //var sh = config.GetSignalHeadForMastName(sm.userName);
            //var colour = "";
            //foreach (var sigPointer in sh.turnoutname)
            //{
            //    colour = sigPointer.defines;
            //    var sig = await webClient.GetTurnout(sigPointer.Value);
            //    if (sig.data.state == 4) //closed = 2, thrown = 4
            //    {
            //        break;
            //    }
            //}
            //string stop = "";
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
