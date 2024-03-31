using JMRIReader.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static JMRIReader.Classes.Enums;

namespace JMRIReader
{
    public class ConfigReader
    {
        private string filePath;
        XDocument config;
        public ConfigReader(string FilePath)
        {
            filePath = FilePath;
            config = XDocument.Load(filePath);          

        }

        public transit GetTransit(string name)
        {
            transit tr = new transit();
            XmlSerializer serial = new XmlSerializer(typeof(transit));            
            var transit = config.Descendants("transit").FirstOrDefault(x => x.Attribute("userName").Value.Equals(name));
            var serializer = new XmlSerializer(typeof(transit));
            tr = (transit)serializer.Deserialize(transit.CreateReader());
            int sectionCounter = -1;
            int blockCounter = -1;

            foreach (var transitsection in tr.transitsection)
            {
                var newSection = new SectionJourneyLog();
                sectionCounter++;
                var hasAlternate = false;
                var nextSection = tr.transitsection.ElementAtOrDefault(sectionCounter);
                if (nextSection != null && nextSection.alternate == "yes")
                {
                    hasAlternate = true;
                }
                section s = GetSectionBySystemName(transitsection.sectionname);
                newSection.Section = s;
                newSection.TransitSection = transitsection;
                newSection.Blocks = new List<block>();

                foreach (var blockEntry in s.blockentry.OrderBy(o => o.order))
                {
                    blockCounter++;
                    block b = GetBlockBySystemName(blockEntry.sName);
                    newSection.Blocks.Add(b);
                    var logEntry = new BlockJourneyLog();
                    logEntry.BlockSystemname = b.systemName;
                    logEntry.BlockUserName = b.userName;
                    logEntry.Traversed = false;
                    logEntry.Sequence = blockCounter;
                    logEntry.PossibleAlternate = transitsection.alternate == "yes" ? true : false;
                    logEntry.HasAlternate = hasAlternate;
                    logEntry.SectionSequenceId = sectionCounter;
                    tr.BlocksInOrder.Add(logEntry);
                }

                newSection.SectionkUserName = s.userName;
                newSection.SectionSystemname = s.systemName;
                newSection.HasAlternate = hasAlternate;
                newSection.PossibleAlternate = transitsection.alternate == "yes" ? true : false;
                newSection.Sequence = sectionCounter;
                newSection.Traversed = false;
                tr.Sections.Add(newSection);
            }
            return tr;
        }

        public block GetBlockBySystemName(string systemName)
        {            
            var configBlocks = config.Elements("layout-config").Elements("blocks").Elements("block").Where(f => f.Attribute("systemName").Value.Equals(systemName));
            if (configBlocks.Count() ==1)
            {
                var configBlock = configBlocks.First();
                var blockSerializer = new XmlSerializer(typeof(block));
                block b = (block)blockSerializer.Deserialize(configBlock.CreateReader());
                return b;
            }
            else if (configBlocks.Count() == 2)
            {
                var configBlock = configBlocks.Last();
                var blockSerializer = new XmlSerializer(typeof(block));
                block b = (block)blockSerializer.Deserialize(configBlock.CreateReader());
                return b;
            }
            return null;
        }

        public block GetBlockByUserName(string userName)
        {
            var configBlock = config.Elements("layout-config").Elements("blocks").Elements("block").FirstOrDefault(f => f.Attribute("userName").Value.Equals(userName));
            var blockSerializer = new XmlSerializer(typeof(block));
            block b = (block)blockSerializer.Deserialize(configBlock.CreateReader());
            return b;
        }

        public section GetSectionBySystemName(string SystemName)
        {
            var configSection = config.Elements("layout-config").Elements("sections").Elements("section").FirstOrDefault(f => f.Attribute("systemName").Value.Equals(SystemName));
            var sectionSerializer = new XmlSerializer(typeof(section));
            section s = (section)sectionSerializer.Deserialize(configSection.CreateReader());
            return s;
        }

        public signalmast GetSignalMastByUserName(string userName)
        {
            var smXML = config.Elements("layout-config").Elements("signalmasts").Elements("signalmast").FirstOrDefault(f => f.Element("userName").Value.Equals(userName));
            var smSerializer = new XmlSerializer(typeof(signalmast));
            if (smXML != null)
            {
                signalmast sm = (signalmast)smSerializer.Deserialize(smXML.CreateReader());
                return sm;
            }
            else return null;
        }

        public turnout GetTurnoutByUserName(string username)
        {
            var configTurnout = config.Elements("layout-config").Elements("turnouts").Elements("turnout").FirstOrDefault(f => f.Element("userName") != null && f.Element("userName").Value.Equals(username));
            var turnouterializer = new XmlSerializer(typeof(turnout));
            turnout t = (turnout)turnouterializer.Deserialize(configTurnout.CreateReader());
            return t;
        }

        public string GetNextBlockForLayoutItem(string currentBlock, string LayoutItem, string previousLayoutItem, LayoutEditor layout = null)
        {
            if (layout ==  null)
            {
                var layoutXML = config.Elements("layout-config").Elements("LayoutEditor").FirstOrDefault();
                var layoutSerializer = new XmlSerializer(typeof(LayoutEditor));
                layout = (LayoutEditor)layoutSerializer.Deserialize(layoutXML.CreateReader());
            }
            string newBlockName = "";
            if (LayoutItem.Substring(0,2) == "TO")
            {
                //turnout
                var to = layout.layoutturnout.FirstOrDefault(f => f.ident == LayoutItem);
            }
            else if (LayoutItem.Substring(0,1) == "T")
            {
                //track
                var tr = layout.tracksegment.FirstOrDefault(f => f.ident == LayoutItem);
                if (tr.blockname != currentBlock)
                {
                    newBlockName = tr.blockname;
                }
                else
                {
                    var nextItem = tr.connect2name;
                    if (tr.connect2name == previousLayoutItem) nextItem = tr.connect1name;
                    newBlockName = GetNextBlockForLayoutItem(currentBlock, nextItem, LayoutItem, layout);
                }
            }
            else if (LayoutItem.Substring(0,1) == "A")
            {
                //anchor
                var a = layout.positionablepoint.Where(w => w.type == "ANCHOR").FirstOrDefault(f => f.ident == LayoutItem);
                var nextItem = a.connect2name;
                if (a.connect2name == previousLayoutItem) nextItem = a.connect1name;
                newBlockName = GetNextBlockForLayoutItem(currentBlock, nextItem, LayoutItem, layout);
            }
            return newBlockName;
        }

        

        public signalmast GetSignalMastForBlock (List<BlockJourneyLog> journeyBlocksInOrder, List<BlockJourneyLog> configBlocksInOrder, int currentBlockIndex, string direction, List<string> ChainedSignalMasts)
        {
            try
            {
                string derivedDirection = direction;
                //southwest - Station 1 to Incline top - needs to be west
                //southeast - incline pi end to station 1 - needs to be east

                if (direction == "North") derivedDirection = "West";
                if (direction == "South") derivedDirection = "East";

                if (direction.Contains("west")) derivedDirection = "West";
                if (direction.Contains("east")) derivedDirection = "East";

                //if (direction.ToLower().Contains("north")) derivedDirection = "West";
                //if (direction.ToLower().Contains("south")) derivedDirection = "East";

                string signalMastName = string.Empty;
                var layoutXML = config.Elements("layout-config").Elements("LayoutEditor").FirstOrDefault();
                var layoutSerializer = new XmlSerializer(typeof(LayoutEditor));
                LayoutEditor layout = (LayoutEditor)layoutSerializer.Deserialize(layoutXML.CreateReader());

                var eastAnchorPoint = new LayoutBlockChainItem();
                var westAnchorPoint = new LayoutBlockChainItem();

                string eastboundSignalMast = string.Empty;
                string westboundSignalMast = string.Empty;

                int blockJumpCount = 0;

                signalMastName = SearchTrackSegentsForSignalMast(layout, journeyBlocksInOrder, configBlocksInOrder,  currentBlockIndex, derivedDirection, ChainedSignalMasts, ref blockJumpCount);

                var sm = config.Elements("layout-config").Elements("signalmasts").Elements("signalmast").FirstOrDefault(f => f.Element("userName").Value.Equals(signalMastName));
                if (sm != null)
                {
                    var smSerializer = new XmlSerializer(typeof(signalmast));
                    signalmast mast = (signalmast)smSerializer.Deserialize(sm.CreateReader());
                    if (blockJumpCount > 0)
                    {
                        //handle block jump
                        mast.BlockJumped = true;
                }
                    return mast;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<LayoutEditorLayoutturnout> GetTurnoutsInBlock(string blockName)
        {
            var layoutXML = config.Elements("layout-config").Elements("LayoutEditor").FirstOrDefault();
            var layoutSerializer = new XmlSerializer(typeof(LayoutEditor));
            LayoutEditor layout = (LayoutEditor)layoutSerializer.Deserialize(layoutXML.CreateReader());
            var turnoutsInThisBlock = layout.layoutturnout.Where(w => w.blockname == blockName || w.blockcname == blockName || w.blockdname == blockName).ToList();
            var thisTO = turnoutsInThisBlock.FirstOrDefault();
      
            

            return turnoutsInThisBlock;
        }

        public signalmast GetSignalMastForBlock(string thisBlock, string nextBlock, string direction, List<string> ChainedSignalMasts)
        {
            try
            {
                string derivedDirection = direction;
                //southwest - Station 1 to Incline top - needs to be west
                //southeast - incline pi end to station 1 - needs to be east

                if (direction == "North") derivedDirection = "West";
                if (direction == "South") derivedDirection = "East";

                if (direction.Contains("west")) derivedDirection = "West";
                if (direction.Contains("east")) derivedDirection = "East";

                //if (direction.ToLower().Contains("north")) derivedDirection = "West";
                //if (direction.ToLower().Contains("south")) derivedDirection = "East";

                string signalMastName = string.Empty;
                var layoutXML = config.Elements("layout-config").Elements("LayoutEditor").FirstOrDefault();
                var layoutSerializer = new XmlSerializer(typeof(LayoutEditor));
                LayoutEditor layout = (LayoutEditor)layoutSerializer.Deserialize(layoutXML.CreateReader());

                var eastAnchorPoint = new LayoutBlockChainItem();
                var westAnchorPoint = new LayoutBlockChainItem();

                string eastboundSignalMast = string.Empty;
                string westboundSignalMast = string.Empty;

                int blockJumpCount = 0;

                signalMastName = SearchTrackSegentsForSignalMast(layout,thisBlock, nextBlock, derivedDirection, ChainedSignalMasts, ref blockJumpCount);

                var sm = config.Elements("layout-config").Elements("signalmasts").Elements("signalmast").FirstOrDefault(f => f.Element("userName").Value.Equals(signalMastName));
                if (sm != null)
                {
                    var smSerializer = new XmlSerializer(typeof(signalmast));
                    signalmast mast = (signalmast)smSerializer.Deserialize(sm.CreateReader());
                    if (blockJumpCount > 0)
                    {
                        //handle block jump
                        mast.BlockJumped = true;
                    }
                    return mast;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public signalmastlogic GetLogicForSignalMast(string SignalMastName)
        {
            var sml = config.Elements("layout-config").Elements("signalmastlogics").Elements("signalmastlogic").FirstOrDefault(f => f.Element("sourceSignalMast").Value.Equals(SignalMastName));
            var smlSerializer = new XmlSerializer(typeof(signalmastlogic));
            signalmastlogic signalMastLogic = (signalmastlogic)smlSerializer.Deserialize(sml.CreateReader());
            return signalMastLogic;
        }

        public List<string> GetSignalDestinationMasts(string SignalMastName)
        {
            List<string> returnList = new List<string>();
            var sml = GetLogicForSignalMast(SignalMastName);
            if (sml != null && sml.destinationMast != null)
            {
                returnList = sml.destinationMast.Select(s => s.destinationSignalMast).ToList();
            }
            return returnList;
        }

        private string SearchTrackSegentsForSignalMast(LayoutEditor layout,string currentBlock, string nextBlock, string direction, List<string> DestinationMastNames, ref int blockJumpCount)
        {

            if (currentBlock == null || nextBlock == null)
            {
                return "";
            }

            var segments = layout.tracksegment.Where(w => w.blockname == currentBlock).ToList();
            var nextBlockSegments = layout.tracksegment.Where(w => w.blockname == nextBlock).ToList();
            var turnoutsInThisBlock = layout.layoutturnout.Where(w => w.blockname == currentBlock || w.blockcname == currentBlock || w.blockdname == currentBlock).ToList();
            var nextBlockTurnouts = layout.layoutturnout.Where(w => w.blockcname == nextBlock).ToList();
            var anchors = layout.positionablepoint.Where(w => w.type == "ANCHOR").ToList();

            string signalMastName = "";
            List<LayoutEditorPositionablepoint> anchorPointsWithNoSignalMasts = new List<LayoutEditorPositionablepoint>();

            foreach (var s in segments)
            {
                var ap = new LayoutEditorPositionablepoint();
                var isBoundary = false;
                var prefix = s.connect1name.Substring(0, 1);
                if (prefix == "A")
                {
                    ap = anchors.FirstOrDefault(f => f.ident == s.connect1name);
                    if (ap != null)
                    {
                        isBoundary = CheckForBoundary(nextBlockSegments, ap.ident);
                        if (!isBoundary) anchorPointsWithNoSignalMasts.Add(ap);
                    }
                }
                if (!isBoundary)
                {
                    prefix = s.connect2name.Substring(0, 1);
                    if (prefix == "A")
                    {
                        ap = anchors.FirstOrDefault(f => f.ident == s.connect2name);
                        if (ap != null)
                        {
                            isBoundary = CheckForBoundary(nextBlockSegments, ap.ident);
                            if (!isBoundary) anchorPointsWithNoSignalMasts.Add(ap);
                        }
                    }
                }

                if (isBoundary)
                {
                    if (DestinationMastNames != null && DestinationMastNames.Count > 0)
                    {
                        var eastMatches = DestinationMastNames.Where(w => w == ap.eastboundsignalmast);
                        var westMatches = DestinationMastNames.Where(w => w == ap.westboundsignalmast);


                        if (eastMatches != null && eastMatches.Count() == 1)
                        {
                            signalMastName = ap.eastboundsignalmast;
                        }
                        else if (westMatches != null && westMatches.Count() == 1)
                        {
                            signalMastName = ap.westboundsignalmast;
                        }
                        else
                        {
                            if (direction == "East")
                            {
                                signalMastName = ap.eastboundsignalmast;
                            }
                            else
                            {
                                signalMastName = ap.westboundsignalmast;
                            }
                        }
                    }
                    else
                    {
                        if (direction == "East")
                        {
                            signalMastName = ap.eastboundsignalmast;
                        }
                        else
                        {
                            signalMastName = ap.westboundsignalmast;
                        }
                    }
                    break;
                }
            }

            if (signalMastName == null || signalMastName.Length < 1)
            {
                //been through all track segments and APs with no luck - try turnouts
                //it may be that a turnout is a block boundary
                var turnoutsWithBoundaries = turnoutsInThisBlock.Where(w => w.blockname == nextBlock || w.blockcname == nextBlock || w.blockdname == nextBlock);
                if (turnoutsWithBoundaries != null)
                {
                    //the route takes the train out of the current block, into the next one, before the 'official' anchor poing signal mast
                    //therefore the current block is governed by the signal mast at the end of the block that the train will turn into
                    blockJumpCount++;
                    //signalMastName = SearchTrackSegentsForSignalMast(layout, journeyBlocksInOrder, configBlocksInOrder, currentBlockIndex + 1, direction, DestinationMastNames, ref blockJumpCount);
                }
            }
            return signalMastName;
        }

        private string SearchTrackSegentsForSignalMast(LayoutEditor layout, List<BlockJourneyLog> journeyBlocksInOrder, List<BlockJourneyLog> configBlocksInOrder, int currentBlockIndex, string direction, List<string> DestinationMastNames, ref int blockJumpCount)
        {
            var currentBlock = journeyBlocksInOrder.ElementAtOrDefault(currentBlockIndex);
            var nextBlock = journeyBlocksInOrder.ElementAtOrDefault(currentBlockIndex + 1);

            if (nextBlock == null)
            {
                nextBlock = configBlocksInOrder.ElementAtOrDefault(currentBlockIndex + 1);
            }

            if (currentBlock == null || nextBlock == null)
            {
                return "";
            }

            var segments = layout.tracksegment.Where(w => w.blockname == currentBlock.BlockUserName).ToList();
            var nextBlockSegments = layout.tracksegment.Where(w => w.blockname == nextBlock.BlockUserName).ToList();
            var turnoutsInThisBlock = layout.layoutturnout.Where(w => w.blockname == currentBlock.BlockUserName || w.blockcname == currentBlock.BlockUserName || w.blockdname == currentBlock.BlockUserName).ToList();
            var nextBlockTurnouts = layout.layoutturnout.Where(w => w.blockcname == nextBlock.BlockUserName).ToList();
            var anchors = layout.positionablepoint.Where(w => w.type == "ANCHOR").ToList();

            string signalMastName = "";
            List<LayoutEditorPositionablepoint> anchorPointsWithNoSignalMasts = new List<LayoutEditorPositionablepoint>();

            foreach (var s in segments)
            {
                var ap = new LayoutEditorPositionablepoint();
                var isBoundary = false;
                var prefix = s.connect1name.Substring(0, 1);
                if (prefix == "A")
                {
                    ap = anchors.FirstOrDefault(f => f.ident == s.connect1name);
                    if (ap != null)
                    {
                        isBoundary = CheckForBoundary(nextBlockSegments, ap.ident);
                        if (!isBoundary) anchorPointsWithNoSignalMasts.Add(ap);
                    }
                }
                if (!isBoundary)
                {
                    prefix = s.connect2name.Substring(0, 1);
                    if (prefix == "A")
                    {
                        ap = anchors.FirstOrDefault(f => f.ident == s.connect2name);
                        if (ap != null)
                        {
                            isBoundary = CheckForBoundary(nextBlockSegments, ap.ident);
                            if (!isBoundary) anchorPointsWithNoSignalMasts.Add(ap);
                        }
                    }
                }

                if (isBoundary)
                {
                    if (DestinationMastNames != null && DestinationMastNames.Count > 0)
                    {
                        var eastMatches = DestinationMastNames.Where(w => w == ap.eastboundsignalmast);
                        var westMatches = DestinationMastNames.Where(w => w == ap.westboundsignalmast);


                        if (eastMatches != null &&  eastMatches.Count() == 1)
                        {
                            signalMastName = ap.eastboundsignalmast;
                        }
                        else if (westMatches != null && westMatches.Count() == 1)
                        {
                            signalMastName = ap.westboundsignalmast;
                        }
                        else
                        {
                            if (direction == "East")
                            {
                                signalMastName = ap.eastboundsignalmast;
                            }
                            else
                            {
                                signalMastName = ap.westboundsignalmast;
                            }
                        }
                    }
                    else
                    {
                        if (direction == "East")
                        {
                            signalMastName = ap.eastboundsignalmast;
                        }
                        else
                        {
                            signalMastName = ap.westboundsignalmast;
                        }
                    }
                    break;
                }
            }

            if (signalMastName == null || signalMastName.Length < 1)
            {
                //been through all track segments and APs with no luck - try turnouts
                //it may be that a turnout is a block boundary
                var turnoutsWithBoundaries = turnoutsInThisBlock.Where(w => w.blockname == nextBlock.BlockUserName || w.blockcname == nextBlock.BlockUserName || w.blockdname == nextBlock.BlockUserName);
                if (turnoutsWithBoundaries != null)
                {
                    //the route takes the train out of the current block, into the next one, before the 'official' anchor poing signal mast
                    //therefore the current block is governed by the signal mast at the end of the block that the train will turn into
                    blockJumpCount++;
                    signalMastName = SearchTrackSegentsForSignalMast(layout, journeyBlocksInOrder, configBlocksInOrder, currentBlockIndex + 1, direction, DestinationMastNames, ref blockJumpCount);
                }
            }
            return signalMastName;
        }

        public object GetNextConnectedItem(object thisObject, string thisBlock)
        {
            var layoutXML = config.Elements("layout-config").Elements("LayoutEditor").FirstOrDefault();
            var layoutSerializer = new XmlSerializer(typeof(LayoutEditor));
            LayoutEditor layout = (LayoutEditor)layoutSerializer.Deserialize(layoutXML.CreateReader());

            if (thisObject.GetType() == typeof(LayoutEditorPositionablepoint))
            {
                LayoutEditorPositionablepoint ap = (LayoutEditorPositionablepoint)thisObject;
                

            }
            else if (thisObject.GetType() == typeof(LayoutEditorTracksegment))
            {

            }
            else if (thisObject.GetType() == typeof(LayoutEditorLayoutturnout))
            {

            }
            return "";
        }

        public List<LayoutEditorPositionablepoint> GetBoundariesForBlock(string blockName)
        {
            var layoutXML = config.Elements("layout-config").Elements("LayoutEditor").FirstOrDefault();
            var layoutSerializer = new XmlSerializer(typeof(LayoutEditor));
            LayoutEditor layout = (LayoutEditor)layoutSerializer.Deserialize(layoutXML.CreateReader());

            List<LayoutEditorPositionablepoint> boundaries = new List<LayoutEditorPositionablepoint>();
            var segments = layout.tracksegment.Where(w => w.blockname == blockName).ToList();
            var nextBlockSegments = layout.tracksegment.Where(w => w.blockname == blockName).ToList();
            var turnoutsInThisBlock = layout.layoutturnout.Where(w => w.blockname == blockName || w.blockcname == blockName || w.blockdname == blockName).ToList();
            var nextBlockTurnouts = layout.layoutturnout.Where(w => w.blockcname == blockName).ToList();
            var anchors = layout.positionablepoint.Where(w => w.type == "ANCHOR").ToList();

            List<LayoutEditorPositionablepoint> anchorPointsWithNoSignalMasts = new List<LayoutEditorPositionablepoint>();

            foreach (var s in segments)
            {
                var ap = new LayoutEditorPositionablepoint();
                var isBoundary = false;
                var prefix = s.connect1name.Substring(0, 1);
                if (prefix == "A")
                {
                    ap = anchors.FirstOrDefault(f => f.ident == s.connect1name);
                    if (ap != null)
                    {
                        isBoundary = CheckForBoundary(nextBlockSegments, ap.ident);
                        if (!isBoundary) anchorPointsWithNoSignalMasts.Add(ap);
                    }
                }
                if (!isBoundary)
                {
                    prefix = s.connect2name.Substring(0, 1);
                    if (prefix == "A")
                    {
                        ap = anchors.FirstOrDefault(f => f.ident == s.connect2name);
                        if (ap != null)
                        {
                            isBoundary = CheckForBoundary(nextBlockSegments, ap.ident);
                            if (!isBoundary) anchorPointsWithNoSignalMasts.Add(ap);
                        }
                    }
                }

                if (isBoundary)
                {
                    boundaries.Add(ap);
                }
            }
            return boundaries;
        }

        private bool CheckForBoundary(List<LayoutEditorTracksegment> NextBlockSegments, string AnchorPointID)
        {
            var nbSegment = NextBlockSegments.FirstOrDefault(f =>f.connect1name == AnchorPointID);
            if (nbSegment != null)
            {
                return true;
            }

            nbSegment = NextBlockSegments.FirstOrDefault(f => f.connect2name == AnchorPointID);
            if (nbSegment != null)
            {
                return true;
            }
            return false;
        }

        private bool CheckForBoundary(List<LayoutEditorTracksegment> NextBlockSegments, string AnchorPointID, string currentBlockName)
        {
            var nbSegment = NextBlockSegments.FirstOrDefault(f => f.connect1name == AnchorPointID);
            if (nbSegment != null)
            {
                return true;
            }

            nbSegment = NextBlockSegments.FirstOrDefault(f => f.connect2name == AnchorPointID);
            if (nbSegment != null)
            {
                return true;
            }
            return false;
        }


        public signalhead GetSignalHeadForMastName(string signalMastName)
        {
            //var signalMasts2 = config.Elements("layout-config").Elements("signalmasts").Elements("signalmast");
            var signalMasts = config.Elements("layout-config").Elements("signalmasts").Elements("signalmast").FirstOrDefault(w => w.Element("userName").Value == signalMastName);
            var smSerializer = new XmlSerializer(typeof(signalmast));
            signalmast sm = (signalmast)smSerializer.Deserialize(signalMasts.CreateReader());

            var firstBIndex = sm.systemName.LastIndexOf("(");
            var lastBIndex = sm.systemName.LastIndexOf(")");
            var signalHeadName = sm.systemName.Substring(firstBIndex + 1, lastBIndex - firstBIndex - 1);
            var shead = config.Elements("layout-config").Elements("signalheads").Elements("signalhead").FirstOrDefault(f => f.Element("userName").Value.Equals(signalHeadName));

            if (shead == null) return null;

            var shSerializer = new XmlSerializer(typeof(signalhead));
            signalhead sh = (signalhead)shSerializer.Deserialize(shead.CreateReader());

            return sh;

        }

        public signalmast GetSignalMastForBlock(List<BlockJourneyLog> manifest, int currentBlockIndex, ref int blockJumpCount)
        {
            var returnSM = new signalmast();
            returnSM.BlockJumped = false;

            var layoutXML = config.Elements("layout-config").Elements("LayoutEditor").FirstOrDefault();
            var layoutSerializer = new XmlSerializer(typeof(LayoutEditor));
            LayoutEditor layout = (LayoutEditor)layoutSerializer.Deserialize(layoutXML.CreateReader());

            var thisManifestBlock = manifest.ElementAtOrDefault(currentBlockIndex);
            var nextManifestBlock = manifest.ElementAtOrDefault(currentBlockIndex + 1);

            if (thisManifestBlock == null || nextManifestBlock == null) return null;

            var dir = GetDirectionForNextBlock(thisManifestBlock.BlockSystemname, nextManifestBlock.BlockSystemname);

            var thisBlock = GetBlockBySystemName(thisManifestBlock.BlockSystemname);
            var nextBlock = GetBlockBySystemName(nextManifestBlock.BlockSystemname);

            string signalMastNameFound = string.Empty;

            foreach (var path in thisBlock.path)
            {
                if (path.block == nextBlock.systemName)
                {
                    dir = (direction)path.todir;                    

                    //not determined by a turnout state
                    foreach (var ap in layout.positionablepoint.Where(w => w.type == "ANCHOR"))
                    {
                        var block1 = GetConnectingBlockNameForAnchorPointConnection(ap.connect1name, layout);
                        var block2 = GetConnectingBlockNameForAnchorPointConnection(ap.connect2name, layout);

                        if ((block1 == thisBlock.userName && block2 == nextBlock.userName) || (block2 == thisBlock.userName && block1 == nextBlock.userName))
                        {
                            if (block2 == thisBlock.userName && block1 == nextBlock.userName)
                            {
                                var stop = "debug";
                                //dir = (direction)path.fromdir;
                            }
                            string strDir = dir.ToString();
                            // if (strDir == "East" || strDir.Contains("east") || strDir == "South")
                            if (strDir == "East" || strDir.Contains("South") || strDir.Contains("east") || strDir == "South")
                            {
                                signalMastNameFound = ap.eastboundsignalmast;
                                //break;
                            }
                            else if (strDir == "West" || strDir.Contains("North") || strDir.Contains("west") || strDir == "North")
                            {
                                signalMastNameFound = ap.westboundsignalmast;
                                //break;
                            }
                            else
                            {
                                string shouldNeverComehere = "";
                            }


                        }
                        //else if (block2 == thisBlock.userName && block1 == nextBlock.userName)
                        //{
                        //    var reverse = "things here?";
                        //    string strDir = dir.ToString();
                        //    if (strDir == "East" || strDir.Contains("east") || strDir == "South")
                        //    //if (strDir == "East" || strDir.Contains("South") || strDir == "South")
                        //    {
                        //        signalMastNameFound = ap.eastboundsignalmast;
                        //        //break;
                        //    }
                        //    else if (strDir == "West" || strDir.Contains("west") || strDir == "North")
                        //    {
                        //        signalMastNameFound = ap.westboundsignalmast;
                        //        //break;
                        //    }
                        //}


                    }

                    if (signalMastNameFound == string.Empty && path.beansetting != null)
                    {
                        if (path.beansetting.turnout != null)
                        {
                            var turnout = path.beansetting.turnout;
                            var state = path.beansetting.setting;
                            var lTurnout = layout.layoutturnout.FirstOrDefault(f => f.turnoutname == turnout.systemName);
                            var blockInManifest = manifest.FirstOrDefault(f => f.BlockUserName == lTurnout.blockname && f.Sequence >= currentBlockIndex);
                            var manifestIndex = manifest.IndexOf(blockInManifest);

                            blockJumpCount++;
                            returnSM = GetSignalMastForBlock(manifest, manifestIndex, ref blockJumpCount);

                            if (state == 4) //thrown so get from next block
                            {
                                

                            }
                        }
                    }
                }
            }

            if (signalMastNameFound != string.Empty)
            {
                returnSM = GetSignalMastByUserName(signalMastNameFound);
            }

            if (blockJumpCount >0)
            {
                returnSM.BlockJumped = true;
            }

            return returnSM;
        }


        public string GetConnectingBlockNameForAnchorPointConnection (string connection, LayoutEditor layout)
        {
            string connectingBlock = "";

            if (connection.Substring(0,2) == "TO")
            {
                //turnouts as connections not implemented yet
            }
            else if (connection.Substring(0,1) == "T")
            {
                //track segment
                var ts = layout.tracksegment.FirstOrDefault(f => f.ident == connection);
                if (ts != null)
                {
                    connectingBlock = ts.blockname;
                }
            }
            return connectingBlock;
        }

        public direction GetDirectionForNextBlock(string thisBlockSystemName, string nextBlockSystemName)
        {
            var dir = direction.Notknown;
            var thisBlock = GetBlockBySystemName(thisBlockSystemName);
            var path = thisBlock.path.FirstOrDefault(w => w.block == nextBlockSystemName);
            if (path != null)
            {
                dir = (direction)path.todir;
            }
            return dir;
        }
    }
}
