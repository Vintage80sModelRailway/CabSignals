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
            btnDispatchPath.Enabled = true;
        }

        private void btnOpenDispatch_Click(object sender, EventArgs e)
        {
            var filename = tbDispatchPath.Text + "\\" + cbDispatches.SelectedItem;
            if (File.Exists(filename))
            {
                XmlDocument dispatch = new XmlDocument();
                dispatch.Load(filename);
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
                btnStartJourney.Enabled = true;
            }
        }

        private async void btnStartJourney_Click(object sender, EventArgs e)
        {
            btnStopJourney.Enabled = true;
            btnStartJourney.Enabled = false;
            try
            {
                await TrackJourney();
            }
            catch (Exception ex)
            {
                lbOutput.Items.Add("Exception - " + ex.Message);
                btnStopJourney.Enabled = false;
                btnStartJourney.Enabled = true;
            }
        }

        private async Task TrackJourney()
        {
            bool useSMLogic = cbUseSMLogic.Checked;
            int currentSectionIndex = 0;
            int currentBlockIndex = 0;
            int journeyAlternateOffset = 0;

            string direction = "";
            string signalMastName = "";
            string previousSignalMastName = "";

            var ActiveBlocks = await webClient.GetOccupiedBlocks();
            
            List<BlockJourneyLog> log = new List<BlockJourneyLog>();
            List<SectionJourneyLog> sectionLog = new List<SectionJourneyLog>();
            signalmast sm = new signalmast();

            journeyRunning = true;

            BlockRootObject block = await webClient.GetBlock(tbStartBlock.Text);     


            var journey = new Journey(tbConfigLocation.Text, tbTransit.Text, tbTrainName.Text, tbStartBlock.Text, "ed");
            var transit = journey.GetTransit();
            var nextBlock = transit.BlocksInOrder.ElementAtOrDefault(1);
            if (nextBlock != null)
            {
                tbNextBlock.Text = nextBlock.BlockUserName;
            }

            lbOutput.Items.Add("Journey started - number of blocks: " + transit.BlocksInOrder.Count.ToString() + "; number of sections: " + transit.Sections.Count.ToString());
            lbOutput.Items.Add("Current block " + block.data.userName + " - waiting for train progress");

            var startLveBlock = await webClient.GetBlock(tbStartBlock.Text);
            BlockRootObject previousBlock = startLveBlock;

            var start = transit.BlocksInOrder.FirstOrDefault(f => f.BlockUserName == tbStartBlock.Text);
            var configStartBlockIndex = transit.BlocksInOrder.IndexOf(start);

            tbCurrentBlock.Text = startLveBlock.data.userName;

            if (useSMLogic)
            {
                direction = GetDirectionFromSectionAndTransit(transit.Sections, currentSectionIndex);
                sm = GetSignalMastForBlock(transit.BlocksInOrder, transit.BlocksInOrder, tbCurrentBlockSignalMast.Text, currentBlockIndex, direction);
            }
            else
            {
                int blocksJumped = 0;
                sm = config.GetSignalMastForBlock(transit.BlocksInOrder, 0, ref blocksJumped);
            }

            if (sm.BlockJumped) lbOutput.Items.Add("Block jumped");

            //await UpdateSignalStatus(sm, false);
            await UpdateSignalMastStatus(sm.systemName, false);

            signalMastName = sm.userName;
            previousSignalMastName = sm.userName;

            tbCurrentBlockSignalMast.Text = signalMastName;

            var assignedAPIBlocks = await webClient.GetAssignedBlocks(tbTrainName.Text,true);
            var assignedBlocksLastTime = new List<BlockRootObject>();

            while (assignedAPIBlocks == null || assignedAPIBlocks.Count < 1)
            {
                await Task.Delay(1000);
                assignedAPIBlocks = await webClient.GetAssignedBlocks(tbTrainName.Text,true);
            }

            while (journeyRunning)
            {                
                //2 == occupied, 4 == unoccupied
                var blockJustGoneLive = new BlockRootObject();

                try
                {
                    
                    var newActiveBlocks = await webClient.GetOccupiedBlocks();

                    List<BlockRootObject> newBlocksThisTime = new List<BlockRootObject>();


                    if (cbUpdateAssignedBlocks.Checked)
                    {
                        //lbAssignedBlocks.Items.Clear();
                    }
                    foreach (var ab in assignedAPIBlocks)
                    {
                        if (ab.data.userName == "Yard AC Line 1 Block 3")
                        {
                            var s = "sto";
                        }
                        var alreadyExists = assignedBlocksLastTime.Any(a => a.data.name == ab.data.name);
                        if (!alreadyExists)
                        {
                            var possibleSequence = transit.BlocksInOrder.FirstOrDefault(w => w.Sequence >= currentBlockIndex && w.BlockUserName == ab.data.userName);

                            //totally lazy, just need an int to order by
                            if (possibleSequence != null)
                            {
                                ab.data.curvature = possibleSequence.Sequence;
                                newBlocksThisTime.Add(ab);
                            }
                        }
                    }                   

                    int previousSectionSequence = -1;
                    if (newBlocksThisTime != null)
                    {
                        newBlocksThisTime = newBlocksThisTime.OrderBy(o => o.data.curvature).ToList();
                        foreach (var nb in newBlocksThisTime)
                        {
                            int searchIndexLimit = -1;
                            bool foundMatch = false;

                            for (int i = log.Count - 1; i <= log.Count - 1 + newBlocksThisTime.Count + journeyAlternateOffset; i++)
                            {
                                var possibleMatch = transit.BlocksInOrder.ElementAtOrDefault(i);
                                if (possibleMatch != null && possibleMatch.BlockSystemname == nb.data.name)
                                {
                                    possibleMatch.Sequence = i;
                                    possibleMatch.Assigned = true;
                                    log.Add(possibleMatch);
                                    var sectionSequence = possibleMatch.SectionSequenceId;

                                    var sectionToAdd = transit.Sections.FirstOrDefault(f => f.Sequence == possibleMatch.SectionSequenceId);
                                    var existingIndex = sectionLog.FirstOrDefault(f => f.Sequence == sectionToAdd.Sequence);
                                    if (existingIndex == null)
                                    {
                                        sectionLog.Add(sectionToAdd);
                                    }
                                    tbExceptionTrace.Text = tbExceptionTrace.Text + sectionToAdd.SectionkUserName + "("+possibleMatch.SectionSequenceId.ToString()+")"+ "; ";

                                    previousSectionSequence = sectionSequence;

                                    //lbOutput.Items.Add("Added to log: " + possibleMatch.BlockUserName + " at " + i.ToString() + " section sequence ID " + possibleMatch.SectionSequenceId.ToString());
                                    foundMatch = true;
                                    break;
                                }
                                searchIndexLimit = i;
                            }

                            if (!foundMatch)
                            {
                                bool handlingAlternate = false;
                                var checkForAlternate = transit.BlocksInOrder.ElementAtOrDefault(searchIndexLimit);
                                //lbOutput.Items.Add("Alternate checking searchindexlimit " + searchIndexLimit.ToString());
                                while (checkForAlternate != null && checkForAlternate.PossibleAlternate)
                                {
                                    handlingAlternate = true;
                                    searchIndexLimit++;
                                    checkForAlternate = transit.BlocksInOrder.ElementAtOrDefault(searchIndexLimit);
                                    //lbOutput.Items.Add("Possible alt " + checkForAlternate.BlockUserName + " indexlimit " + searchIndexLimit.ToString());
                                    journeyAlternateOffset++;
                                }
                                while (checkForAlternate != null && checkForAlternate.HasAlternate)
                                {
                                    handlingAlternate = true;
                                    searchIndexLimit++;
                                    checkForAlternate = transit.BlocksInOrder.ElementAtOrDefault(searchIndexLimit);
                                    //lbOutput.Items.Add("Has alt " + checkForAlternate.BlockUserName + " indexlimit " + searchIndexLimit.ToString());
                                    journeyAlternateOffset++;
                                }

                                if (handlingAlternate)
                                {
                                    checkForAlternate.Sequence = searchIndexLimit;
                                    checkForAlternate.Assigned = true;
                                    log.Add(checkForAlternate);
                                    var sectionToAdd = transit.Sections.FirstOrDefault(f => f.Sequence == checkForAlternate.SectionSequenceId);
                                    //sectionToAdd.Sequence = searchIndexLimit;
                                    sectionLog.Add(sectionToAdd);

                                    //lbOutput.Items.Add("Added to log through alternate route handling: " + checkForAlternate.BlockUserName + " at " + searchIndexLimit.ToString() + " section sequence ID " + checkForAlternate.SectionSequenceId.ToString());

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
                                    lbJourneyLog.Items.Add(l.BlockUserName + tr);
                                }
                            }
                        }
                    }

                    //if one of this train's allocated blocks has just gone active
                    //compare last allocated blocks to this assigned blocks
                    newActiveBlocks = await webClient.GetOccupiedBlocks();

                    foreach (var nab in newActiveBlocks.Where(w => w.data.value != null && w.data.value.data != null))
                    {

                        var wasAlreadyOccupied = ActiveBlocks.Any(a => a.data.userName == nab.data.userName);
                        if (!wasAlreadyOccupied && blockJustGoneLive != null && blockJustGoneLive.data == null)
                        {
                            if (nab.data.userName == "Yard AC Line 1 Block 3")
                            {
                                var stopp = "";
                            }
                            var wasAssignedToThisTrain = log.FirstOrDefault(f => f.BlockUserName == nab.data.userName && f.Assigned == true && f.Traversed == false);
                            if (wasAssignedToThisTrain != null && tbNextBlock.Text == wasAssignedToThisTrain.BlockUserName)
                            {
                                //likely block was allocated to the wrong train
                                blockJustGoneLive = nab;
                            }
                        }
                    }


                    ActiveBlocks = newActiveBlocks;                    

                    assignedBlocksLastTime = assignedAPIBlocks;
                    assignedAPIBlocks = await webClient.GetAssignedBlocks(tbTrainName.Text);

                    if (blockJustGoneLive != null && blockJustGoneLive.data != null)
                    {
                        lbOutput.Items.Add("New live block - " + blockJustGoneLive.data.userName);
                    }

                    //sm = GetSignalMastForBlock(log, signalMastName, currentBlockIndex, direction);
                    //await UpdateSignalStatus(sm, false);
                    await UpdateSignalMastStatus(sm.systemName, false);
                }               

                catch (Exception ex)
                {
                    tbExceptionTrace.Text = ex.StackTrace;
                    lbOutput.Items.Add("Exception during new allocated block detection phase - " + ex.Message);
                }

                var scheduledBlockAtCurrentIndex = log.ElementAtOrDefault(currentBlockIndex+1);

                try
                {

                    if (blockJustGoneLive != null && blockJustGoneLive.data != null && scheduledBlockAtCurrentIndex != null 
                        && scheduledBlockAtCurrentIndex.BlockSystemname == blockJustGoneLive.data.name && scheduledBlockAtCurrentIndex.Traversed == false)
                    {
                        if (currentBlockIndex >= transit.BlocksInOrder.Count - 1)
                        {
                            journeyRunning = false;
                            break;
                        }

                        if (currentSectionIndex >= transit.Sections.Count - 1)
                        {
                            journeyRunning = false;
                            break;
                        }

                        currentBlockIndex++;
                        lbOutput.Items.Add("Current block index now " + currentBlockIndex.ToString());

                        log.ElementAtOrDefault(currentBlockIndex).Traversed = true;

                        log = log.OrderBy(o => o.Sequence).ToList();
                        sectionLog = sectionLog.OrderBy(o => o.Sequence).ToList();
                        lbJourneyLog.Items.Clear();
                        foreach (var l in log)
                        {
                            var tr = l.Traversed == true ? ", traversed" : "";
                            lbJourneyLog.Items.Add(l.BlockUserName + tr);
                        }

                        var nextdbBlock = log.ElementAtOrDefault(currentBlockIndex + 1);
                        if (nextdbBlock == null)
                        {
                            //Might be waiting for another train with no future allocations
                            nextdbBlock = transit.BlocksInOrder.ElementAtOrDefault(currentBlockIndex + 1 + journeyAlternateOffset);
                        }

                        if (nextdbBlock == null)
                        {
                            var debug = "styop";
                            journeyRunning = false;
                            break;
                        }
                        var nextLiveBlock = await webClient.GetBlock(nextdbBlock.BlockUserName);                        

                        //var currentBlockInCurrentSection = sectionLog.ElementAtOrDefault(currentSectionIndex).Blocks.FirstOrDefault(f => f.userName == blockJustGoneLive.data.userName);
                        var currentSection = transit.Sections.FirstOrDefault(f => f.Sequence == scheduledBlockAtCurrentIndex.SectionSequenceId);
                        currentSectionIndex = sectionLog.IndexOf(currentSection);
                        lbOutput.Items.Add("Section now " + currentSection.SectionkUserName);
                        tbCurrentSection.Text = currentSection.SectionkUserName;
                        tbSectionIndex.Text = currentSectionIndex.ToString();

                        direction = UpdateSectionStatusInfoAndGetDirection(sectionLog, nextdbBlock.SectionSequenceId, blockJustGoneLive.data.name, transit);
                        
                        tbCurrentBlock.Text = blockJustGoneLive.data.userName;
                        tbNextBlock.Text = nextLiveBlock.data.userName;

                        int blocksJumped = 0;
                        if (useSMLogic)
                        {
                            sm = GetSignalMastForBlock(log, transit.BlocksInOrder, signalMastName, currentBlockIndex, direction);
                        }
                        else
                        {                            
                            sm = config.GetSignalMastForBlock(log, currentBlockIndex, ref blocksJumped);
                        }

                        if (sm != null && sm.systemName != "")
                        {
                            bool smHasChanged = sm.userName != tbCurrentBlockSignalMast.Text;
                            if (blocksJumped < 1 && !sm.BlockJumped)
                            {
                                signalMastName = sm.userName;
                                lbOutput.Items.Add("Block change - " + blockJustGoneLive.data.userName + " mast " + signalMastName + " next block " + nextLiveBlock.data.userName + " block index " + currentBlockIndex.ToString());
                                tbCurrentBlockSignalMast.Text = signalMastName;

                            }
                            else
                            {
                                tbCurrentBlockSignalMast.Text = sm.userName;
                                lbOutput.Items.Add("Block change - " + blockJustGoneLive.data.userName + " mast stayed at " + signalMastName + " next block " + nextLiveBlock.data.userName + " block index " + currentBlockIndex.ToString());
                            }

                            if (smHasChanged)
                            {
                                await UpdateSignalMastStatus(sm.systemName, true);
                            }
                        }
                        else
                        {
                            //lbOutput.Items.Add("No signal mast found for block " + blockJustGoneLive.data.userName);
                        }

                        lbOutput.SelectedIndex = lbOutput.Items.Count - 1;

                        previousBlock = blockJustGoneLive;
                    }
                    else
                    {
                        blockJustGoneLive = null;
                    }
                }
                catch (Exception ex)
                {
                    lbOutput.Items.Add("Exception during block change handling - " + ex.Message);
                    tbExceptionTrace.Text = tbExceptionTrace.Text + ex.StackTrace;
                }
            }
            CompleteJourney();
        }

        private signalmast GetSignalMastForBlock(List<BlockJourneyLog> journeyBlocksInOrder, List<BlockJourneyLog> configBlocksInOrder, string currentSignalMastName, int blockIndex, string direction)
        {
            signalmast sm = new signalmast();
            sm.userName = "NULL";

            if (currentSignalMastName == "")
            {
                sm = config.GetSignalMastForBlock(journeyBlocksInOrder, configBlocksInOrder, blockIndex, direction, null);
            }
            else
            {
                var nextSignals = config.GetSignalDestinationMasts(currentSignalMastName);
                sm = config.GetSignalMastForBlock(journeyBlocksInOrder, configBlocksInOrder,  blockIndex, direction, nextSignals);
            }
            return sm;
        }

        protected async Task UpdateSignalStatus(signalmast sm, bool blockHasChanged)
        {
            if (sm == null) return;
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

        protected async Task UpdateSignalMastStatus(string SignalMastName, bool blockHasChanged)
        {
            var sm = new SMRootobject();

            try
            {
                 sm = await webClient.GetSignalMast(SignalMastName);
            }
            catch (Exception ex)
            {
                lbOutput.Items.Add("SM retrieval exception " + ex.Message);
                tbExceptionTrace.Text = ex.StackTrace;
                return;
            }

            if (sm == null || sm.data == null)
            {
                lbOutput.Items.Add("Signal mast query returned null");
                return;
            }

            if (sm.data.state != tbCurrentBlockSignalMastState.Text)
            {
                pbSignal.Load("./Assets/" + sm.data.state + ".png");
                tbCurrentBlockSignalMastState.Text = sm.data.state;
                SoundPlayer signalBeep = new SoundPlayer("./Assets/" + sm.data.state + ".wav");
                signalBeep.Play();
            }
            else if (blockHasChanged)
            {
                SoundPlayer signalBeep = new SoundPlayer("./Assets/" + sm.data.state + ".wav");
                signalBeep.Play();
            }
        }

        protected string UpdateSectionStatusInfoAndGetDirection(List<SectionJourneyLog> sectionLog, int nextDectionSequenceId, string currentLiveBlockName, transit tr)
        {
            try
            {
                string direction = "";
                var nextSection = sectionLog.FirstOrDefault(f => f.Sequence == nextDectionSequenceId);
                if (nextSection == null)
                {
                    //if train is queuing there will be no more sections
                    nextSection = tr.Sections.FirstOrDefault(f => f.Sequence == nextDectionSequenceId);
                }

                var entryBlock = nextSection.Section.entrypoint.Where(w => w.fromblock == currentLiveBlockName).ToList();

                if (entryBlock == null && nextSection.PossibleAlternate == true)
                {
                    //may well be null if the next section is an alternate section, therefore doesn't have a matching entry block
                    nextSection = sectionLog.FirstOrDefault(f => f.Sequence == nextDectionSequenceId+1);
                    if (nextSection == null)
                    {
                        //if train is queuing there will be no more sections
                        nextSection = tr.Sections.FirstOrDefault(f => f.Sequence == nextDectionSequenceId+1);
                    }
                    entryBlock = nextSection.Section.entrypoint.Where(w => w.fromblock == currentLiveBlockName).ToList();
                }

                lbOutput.Items.Add("Found " + entryBlock.Count.ToString() + " matching entry point blocks for section "+nextSection.SectionkUserName);
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
                lbOutput.Items.Add("Exception in getdir - " + ex.Message+" currentliveblockname "+currentLiveBlockName+" seq "+nextDectionSequenceId.ToString());
                tbExceptionTrace.Text = ex.StackTrace;
                var test = "stop";
                return "";
            }
        }

        private string GetDirectionFromSectionAndTransit(List<SectionJourneyLog> sectionLog, int sectionIndex)
        {
            var transitSec = sectionLog.OrderBy(o => o.Sequence).ElementAtOrDefault(sectionIndex+1);
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
            pbSignal.Load("./Assets/Danger.png");
            tbCurrentBlock.Text = "";
            tbNextBlock.Text = "";
            tbCurrentBlockSignalMast.Text = "";
            tbCurrentBlockSignalMastState.Text = "";
            SoundPlayer signalBeep = new SoundPlayer("./Assets/Danger.wav");
            signalBeep.Play();
            btnStopJourney.Enabled = false;
            btnStartJourney.Enabled = true;
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

        private void btnTest_Click(object sender, EventArgs e)
        {
            var journey = new Journey(tbConfigLocation.Text, tbTransit.Text, tbTrainName.Text, tbStartBlock.Text, "ed");
            var transit = journey.GetTransit();
            int blocksJumped = 0;
            var sm = config.GetSignalMastForBlock(transit.BlocksInOrder, int.Parse(tbTestVal1.Text), ref blocksJumped);
            var stop = "debug";

        }
    }
}
