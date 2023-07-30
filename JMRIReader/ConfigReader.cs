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

            foreach (var transitsection in tr.transitsection)
            {
                section s = GetSectionBySystemName(transitsection.sectionname);
                foreach (var blockEntry in s.blockentry.OrderBy(o => o.order))
                {
                    block b = GetBlockBySystemName(blockEntry.sName);
                    s.Blocks.Add(b);
                    tr.BlocksInOrder.Add(b);
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

        public signalmast GetSignalMastForBlock(string BlockUserName, string NextBlockUserName, string direction)
        {
            if (direction == "North") direction = "West";
            if (direction == "South") direction = "East";

            if (direction.Contains("west")) direction = "West";
            if (direction.Contains("east")) direction = "East";

            string signalMastName = string.Empty;
            var layoutXML = config.Elements("layout-config").Elements("LayoutEditor").FirstOrDefault();
            var layoutSerializer = new XmlSerializer(typeof(LayoutEditor));
            LayoutEditor layout = (LayoutEditor)layoutSerializer.Deserialize(layoutXML.CreateReader());

            var segments = layout.tracksegment.Where(w => w.blockname == BlockUserName);
            var nextBlockSegments = layout.tracksegment.Where(w => w.blockname == NextBlockUserName).ToList();
            var turnouts = layout.layoutturnout.Where(w => w.blockname == BlockUserName);
            var anchors = layout.positionablepoint.Where(w => w.type == "ANCHOR");

            var eastAnchorPoint = new LayoutBlockChainItem();
            var westAnchorPoint = new LayoutBlockChainItem();

            string eastboundSignalMast = string.Empty;
            string westboundSignalMast = string.Empty;

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
                }
            }

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
