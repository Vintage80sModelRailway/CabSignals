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

            string direction = "";
            List<BlockJourneyLog> log = new List<BlockJourneyLog>();

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

            direction = GetDirectionFromSectionAndTransit(transit, currentSectionIndex);
            await UpdateSignalStatus(transit.BlocksInOrder, currentBlockIndex, direction, false);

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
                    lbOutput.Items.Add(newBlocksThisTime.Count.ToString() + " new assigned blocks found");
                    bool foundMatch = false;
                    for (int i = currentBlockIndex;i <= currentBlockIndex+assignedAPIBlocks.Count;i++)
                    {
                        var possibleMatch = transit.BlocksInOrder.ElementAtOrDefault(i);
                        if (possibleMatch != null && possibleMatch.BlockSystemname == nb.data.name)
                        {
                            possibleMatch.Sequence = i;
                            log.Add(possibleMatch);
                            lbOutput.Items.Add("Added to log: " + possibleMatch.BlockUserName+" at "+i.ToString());
                            foundMatch = true;
                        }
                    }


                    if (!foundMatch)
                    {
                        var stop = "test";
                    }
                    else
                    {
                        log = log.OrderBy(o => o.Sequence).ToList();
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
                        blockJustGoneLive = unassignedBlock;
                }                

                assignedBlocksLastTime = assignedAPIBlocks;

                if (blockJustGoneLive != null && blockJustGoneLive.data != null)
                {
                    lbOutput.Items.Add("New live block - " + blockJustGoneLive.data.userName);
                }
                else
                {
                    await UpdateSignalStatus(transit.BlocksInOrder, currentBlockIndex, direction, false);
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

                        log.ElementAtOrDefault(currentBlockIndex).Traversed = true;

                        var nextdbBlock = log.ElementAtOrDefault(currentBlockIndex + 1);
                        if (nextdbBlock == null)
                        {
                            journeyRunning = false;
                            break;
                        }
                        var nextLiveBlock = await webClient.GetBlock(nextdbBlock.BlockUserName);
                        lbOutput.Items.Add("Block change detected - new block " + blockJustGoneLive.data.userName + " and next block " + nextLiveBlock.data.userName + " block index " + currentBlockIndex.ToString());

                        var currentBlockInCurrentSection = transit.Sections.ElementAtOrDefault(currentSectionIndex).Blocks.FirstOrDefault(f => f.userName == blockJustGoneLive.data.userName);
                        if (currentBlockInCurrentSection == null)
                        {
                            currentSectionIndex++;
                            direction = UpdateSectionStatusInfoAndGetDirection(transit, currentSectionIndex, previousBlock.data.name, blockJustGoneLive.data.name, nextLiveBlock.data.name);
                        }
                        tbCurrentBlock.Text = blockJustGoneLive.data.userName;
                        tbNextBlock.Text = nextLiveBlock.data.userName;
                        await UpdateSignalStatus(log, currentBlockIndex, direction, true);
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

        private async Task TrackJourneyBloated()
        {
            int currentSectionIndex = 0;
            int currentBlockIndex = 0;
            int logSequence = 1;
            int alternateOffset = 0;

            string direction = "";
            List<BlockJourneyLog> log = new List<BlockJourneyLog>();

            journeyRunning = true;

            BlockRootObject block = await webClient.GetBlock(tbStartBlock.Text);

            var journey = new Journey(tbConfigLocation.Text, tbTransit.Text, tbTrainName.Text, tbStartBlock.Text, "ed");
            var transit = journey.GetTransit();


            lbOutput.Items.Add("Journey started - number of blocks: " + transit.BlocksInOrder.Count.ToString() + "; number of sections: " + transit.Sections.Count.ToString());
            lbOutput.Items.Add("Current block " + block.data.userName+" - waiting for train progress");

            var currentBlock = transit.BlocksInOrder.FirstOrDefault(f => f.BlockUserName == tbStartBlock.Text);

            var configStartBlockIndex = transit.BlocksInOrder.IndexOf(currentBlock);
            var nextBlock = transit.BlocksInOrder.ElementAt(configStartBlockIndex + 1);

            var currentSection = transit.Sections.ElementAt(currentSectionIndex);

            tbNextBlock.Text = nextBlock.BlockUserName;
            tbCurrentBlock.Text = currentBlock.BlockUserName;

            var currentLveBlock = await webClient.GetBlock(tbStartBlock.Text);
            var nextLiveBlock = await webClient.GetBlock(tbNextBlock.Text);

            direction = GetDirectionFromSectionAndTransit(transit, currentSectionIndex);
            await UpdateSignalStatus(transit.BlocksInOrder, currentBlockIndex, direction,false);

            var assignedAPIBlocks = await webClient.GetAssignedBlocks(tbTrainName.Text);
            var assignedBlocksLastTime = assignedAPIBlocks;

            while (assignedAPIBlocks == null || assignedAPIBlocks.Count < 1)
            {
                await Task.Delay(1000);
                assignedAPIBlocks = await webClient.GetAssignedBlocks(tbTrainName.Text);
            }

            foreach (var apib in assignedAPIBlocks)
            {
                //Starting with an empty log
                var matchingBlock = transit.BlocksInOrder.FirstOrDefault(f => f.BlockSystemname == apib.data.name);
                if (matchingBlock != null)
                {
                    var seq = transit.BlocksInOrder.IndexOf(matchingBlock);
                    //It should be in the next 3 sections
                    if (seq - currentSectionIndex < 5)
                    {
                        var logEntry = new BlockJourneyLog();
                        logEntry.BlockSystemname = apib.data.name;
                        logEntry.BlockUserName = apib.data.userName;
                        logEntry.Traversed = false;
                        logEntry.Sequence = seq;
                        log.Add(logEntry);
                    }
                }
            }

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
                    currentBlockIndex++;

                    if (currentBlockIndex >= transit.BlocksInOrder.Count - 1)
                    {
                        journeyRunning = false;
                        break;
                    }

                    currentLveBlock = newNextBlock;
                    var nextdbBlock = transit.BlocksInOrder.ElementAt(currentBlockIndex + 1);
                    nextLiveBlock = await webClient.GetBlock(nextdbBlock.BlockUserName);
                    lbOutput.Items.Add("Block change detected - new block " + currentLveBlock.data.userName + " and next block " + nextLiveBlock.data.userName+" block index "+currentBlockIndex.ToString());

                    var currentBlockInCurrentSection = transit.Sections.ElementAt(currentSectionIndex).Blocks.FirstOrDefault(f => f.userName == currentLveBlock.data.userName);
                    if (currentBlockInCurrentSection == null)
                    {
                        currentSectionIndex++;
                        direction = UpdateSectionStatusInfoAndGetDirection(transit, currentSectionIndex, previousBlock.data.name, currentLveBlock.data.name, nextLiveBlock.data.name);
                    }
                    tbCurrentBlock.Text = currentLveBlock.data.userName;
                    tbNextBlock.Text = nextLiveBlock.data.userName;
                    await UpdateSignalStatus(transit.BlocksInOrder, currentBlockIndex, direction, true);
                    lbOutput.SelectedIndex = lbOutput.Items.Count - 1;

                    assignedAPIBlocks = await webClient.GetAssignedBlocks(tbTrainName.Text);
                    int numberOfBlocksAssigned = 0;

                    try
                    {
                        int blockNotFoundOffset = 0;
                        for (int i = 0; i < assignedAPIBlocks.Count; i++)
                        {
                            bool blockAssigned = false;
                            int numberOfBlocksInPlay = log.Count + assignedAPIBlocks.Count;
                            int probableIndexOfNextBlock = (log.Count) + alternateOffset;
                            if (probableIndexOfNextBlock >= transit.BlocksInOrder.Count) continue;

                            var probableNextBlockToBeAssigned = transit.BlocksInOrder.ElementAtOrDefault(probableIndexOfNextBlock);
                            if (probableNextBlockToBeAssigned == null) continue;
                            var possibleMatch = assignedAPIBlocks.FirstOrDefault(f => f.data.name == probableNextBlockToBeAssigned.BlockSystemname);
                            if (possibleMatch != null)
                            {
                                log.Add(probableNextBlockToBeAssigned);
                                assignedAPIBlocks.Remove(possibleMatch);
                                blockAssigned = true;
                            }
                            else if (probableNextBlockToBeAssigned.HasAlternate)
                            {
                                int numberOfBlocksInAlternate = 0;
                                while (probableNextBlockToBeAssigned != null && probableNextBlockToBeAssigned.HasAlternate && !assignedAPIBlocks.Any(a => a.data.name == probableNextBlockToBeAssigned.BlockSystemname) && !probableNextBlockToBeAssigned.PossibleAlternate)
                                {
                                    numberOfBlocksInAlternate++;
                                    probableNextBlockToBeAssigned = transit.BlocksInOrder.ElementAtOrDefault(probableIndexOfNextBlock + numberOfBlocksInAlternate);
                                }
                                if (probableNextBlockToBeAssigned.PossibleAlternate)
                                {
                                    log.Add(probableNextBlockToBeAssigned);
                                    alternateOffset = alternateOffset + numberOfBlocksInAlternate;
                                    assignedAPIBlocks.Remove(possibleMatch);
                                    blockAssigned = true;
                                }
                            }
                            if (!blockAssigned)
                            {
                                blockNotFoundOffset++;
                            }
                        }
                        foreach (var bl in assignedAPIBlocks)
                        {
                            lbOutput.Items.Add("assigned block not added to journey: " + bl.data.userName);
                        }
                    }
                    catch (Exception ex)
                    {
                        var test = "";
                    }
                }
                else
                {
                    await UpdateSignalStatus(transit.BlocksInOrder, currentBlockIndex, direction, false);
                }
            }
            CompleteJourney();
        }

        protected async Task UpdateSignalStatus(List<BlockJourneyLog> journeyBlocksInOrder, int blockIndex, string direction, bool blockHasChanged)
        {
            var sm = config.GetSignalMastForBlock(journeyBlocksInOrder, blockIndex, direction);
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
            try
            {
                string direction = "";
                var dir = transit.transitsection.ElementAt(currentSectionIndex).direction;
                lbOutput.Items.Add("Section change detected - new section " + transit.Sections.ElementAt(currentSectionIndex).userName + " direction " + dir.ToString());
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
            catch (Exception ex)
            {
                var test = "stop";
                return "";
            }
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
            //var sm = config.GetSignalMastForBlock("CW Lower Junction PC End", "Incline Bottom", "East");
            var sm = config.GetSignalMastForBlock(transit.BlocksInOrder, 27, "East");
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
