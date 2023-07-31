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
            int counter = 0;

            foreach (var transitsection in tr.transitsection)
            {
                counter++;
                var hasAlternate = false;
                var nextSection = tr.transitsection.ElementAtOrDefault(counter);
                if (nextSection != null && nextSection.alternate == "yes")
                {
                    hasAlternate = true;
                }
                section s = GetSectionBySystemName(transitsection.sectionname);
                foreach (var blockEntry in s.blockentry.OrderBy(o => o.order))
                {
                    block b = GetBlockBySystemName(blockEntry.sName);
                    s.Blocks.Add(b);
                    var logEntry = new BlockJourneyLog();
                    logEntry.BlockSystemname = b.systemName;
                    logEntry.BlockUserName = b.userName;
                    logEntry.Traversed = false;
                    logEntry.Sequence = counter;
                    logEntry.PossibleAlternate = transitsection.alternate == "yes" ? true : false;
                    logEntry.HasAlternate = hasAlternate;
                    tr.BlocksInOrder.Add(logEntry);
                }
                tr.Sections.Add(s);
            }
            return tr;
        }

        public block GetBlockBySystemName(string systemName)
        {
            var configBlock = config.Elements("layout-config").Elements("blocks").Elements("block").FirstOrDefault(f => f.Attribute("systemName").Value.Equals(systemName));
            var blockSerializer = new XmlSerializer(typeof(block));
            block b = (block)blockSerializer.Deserialize(configBlock.CreateReader());
            return b;
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

        public signalmast GetSignalMastForBlock (List<BlockJourneyLog> journeyBlocksInOrder, int currentBlockIndex, string direction)
        {
            try
            {
                if (direction == "North") direction = "West";
                if (direction == "South") direction = "East";

                if (direction.Contains("west")) direction = "West";
                if (direction.Contains("east")) direction = "East";

                string signalMastName = string.Empty;
                var layoutXML = config.Elements("layout-config").Elements("LayoutEditor").FirstOrDefault();
                var layoutSerializer = new XmlSerializer(typeof(LayoutEditor));
                LayoutEditor layout = (LayoutEditor)layoutSerializer.Deserialize(layoutXML.CreateReader());



                var eastAnchorPoint = new LayoutBlockChainItem();
                var westAnchorPoint = new LayoutBlockChainItem();

                string eastboundSignalMast = string.Empty;
                string westboundSignalMast = string.Empty;

                signalMastName = SearchTrackSegentsForSignalMast(layout, journeyBlocksInOrder, currentBlockIndex, direction);

                //trying to get signal mast from a dividing anchor connected to a track segment from the current block
                //get both then work out direction?
                //get east signal mast
                //get west signal mast
                //know if it's a divider if next connected element in a different block

                var sm = config.Elements("layout-config").Elements("signalmasts").Elements("signalmast").FirstOrDefault(f => f.Element("userName").Value.Equals(signalMastName));
                if (sm != null)
                {
                    var smSerializer = new XmlSerializer(typeof(signalmast));
                    signalmast mast = (signalmast)smSerializer.Deserialize(sm.CreateReader());
                    return mast;
                }

                return null;
            }
            catch (Exception ex)
            {
                var test = "ttop";
                return null;
            }

        }

        private string SearchTrackSegentsForSignalMast(LayoutEditor layout, List<BlockJourneyLog> journeyBlocksInOrder, int currentBlockIndex, string direction)
        {
            var currentBlock = journeyBlocksInOrder.ElementAt(currentBlockIndex);
            var nextBlock = journeyBlocksInOrder.ElementAt(currentBlockIndex + 1);

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
                    if (direction == "East")
                    {
                        signalMastName = ap.eastboundsignalmast;
                    }
                    else
                    {
                        signalMastName = ap.westboundsignalmast;
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
                    signalMastName = SearchTrackSegentsForSignalMast(layout, journeyBlocksInOrder, currentBlockIndex + 1, direction);
                }

            }

            return signalMastName;
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

        public string GetSignalMastByCurrentAndNextBlock(string currentBlock, string nextBlock)
        {
            var signalMasts = config.Elements("layout-config").Elements("signalmasts").Elements("signalmast");
            var smSerializer = new XmlSerializer(typeof(signalmast));
            //signalmast sm = (signalmast)smSerializer.Deserialize(signalMasts.CreateReader());

            return "";
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

            var shSerializer = new XmlSerializer(typeof(signalhead));
            signalhead sh = (signalhead)shSerializer.Deserialize(shead.CreateReader());

            return sh;

        }

        public async Task<string> GetStateForSignalHead(signalhead SignalHead)
        {
            var state = string.Empty;
            List<turnout> turnouts = new List<turnout>();
            foreach (var sig in SignalHead.turnoutname)
            {
                var to = config.Elements("layout-config").Elements("turnouts").Elements("turnout").FirstOrDefault(f => f.Element("userName").Value.Equals(SignalHead.userName));
                var colur = sig.defines;
            }
            return state;
        }

        private XmlReader GetReader(XDocument doc)
        {
            return doc.Root.CreateReader();
        }
    }
}
