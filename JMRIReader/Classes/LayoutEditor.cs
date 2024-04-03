/* 
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMRIReader.Classes
{

    [XmlRoot(ElementName = "layoutTrackDrawingOptions")]
    public class LayoutTrackDrawingOptions
    {
        [XmlElement(ElementName = "mainBallastColor")]
        public string MainBallastColor { get; set; }
        [XmlElement(ElementName = "mainBallastWidth")]
        public string MainBallastWidth { get; set; }
        [XmlElement(ElementName = "mainBlockLineDashPercentageX10")]
        public string MainBlockLineDashPercentageX10 { get; set; }
        [XmlElement(ElementName = "mainBlockLineWidth")]
        public string MainBlockLineWidth { get; set; }
        [XmlElement(ElementName = "mainRailColor")]
        public string MainRailColor { get; set; }
        [XmlElement(ElementName = "mainRailCount")]
        public string MainRailCount { get; set; }
        [XmlElement(ElementName = "mainRailGap")]
        public string MainRailGap { get; set; }
        [XmlElement(ElementName = "mainRailWidth")]
        public string MainRailWidth { get; set; }
        [XmlElement(ElementName = "mainTieColor")]
        public string MainTieColor { get; set; }
        [XmlElement(ElementName = "mainTieGap")]
        public string MainTieGap { get; set; }
        [XmlElement(ElementName = "mainTieLength")]
        public string MainTieLength { get; set; }
        [XmlElement(ElementName = "mainTieWidth")]
        public string MainTieWidth { get; set; }
        [XmlElement(ElementName = "sideBallastColor")]
        public string SideBallastColor { get; set; }
        [XmlElement(ElementName = "sideBallastWidth")]
        public string SideBallastWidth { get; set; }
        [XmlElement(ElementName = "sideBlockLineDashPercentageX10")]
        public string SideBlockLineDashPercentageX10 { get; set; }
        [XmlElement(ElementName = "sideBlockLineWidth")]
        public string SideBlockLineWidth { get; set; }
        [XmlElement(ElementName = "sideRailColor")]
        public string SideRailColor { get; set; }
        [XmlElement(ElementName = "sideRailCount")]
        public string SideRailCount { get; set; }
        [XmlElement(ElementName = "sideRailGap")]
        public string SideRailGap { get; set; }
        [XmlElement(ElementName = "sideRailWidth")]
        public string SideRailWidth { get; set; }
        [XmlElement(ElementName = "sideTieColor")]
        public string SideTieColor { get; set; }
        [XmlElement(ElementName = "sideTieGap")]
        public string SideTieGap { get; set; }
        [XmlElement(ElementName = "sideTieLength")]
        public string SideTieLength { get; set; }
        [XmlElement(ElementName = "sideTieWidth")]
        public string SideTieWidth { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
    }

    [XmlRoot(ElementName = "BlockContentsIcon")]
    public class BlockContentsIcon
    {
        [XmlAttribute(AttributeName = "blockcontents")]
        public string Blockcontents { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "level")]
        public string Level { get; set; }
        [XmlAttribute(AttributeName = "forcecontroloff")]
        public string Forcecontroloff { get; set; }
        [XmlAttribute(AttributeName = "hidden")]
        public string Hidden { get; set; }
        [XmlAttribute(AttributeName = "positionable")]
        public string Positionable { get; set; }
        [XmlAttribute(AttributeName = "showtooltip")]
        public string Showtooltip { get; set; }
        [XmlAttribute(AttributeName = "editable")]
        public string Editable { get; set; }
        [XmlAttribute(AttributeName = "fontFamily")]
        public string FontFamily { get; set; }
        [XmlAttribute(AttributeName = "fontname")]
        public string Fontname { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
        [XmlAttribute(AttributeName = "style")]
        public string Style { get; set; }
        [XmlAttribute(AttributeName = "red")]
        public string Red { get; set; }
        [XmlAttribute(AttributeName = "green")]
        public string Green { get; set; }
        [XmlAttribute(AttributeName = "blue")]
        public string Blue { get; set; }
        [XmlAttribute(AttributeName = "hasBackground")]
        public string HasBackground { get; set; }
        [XmlAttribute(AttributeName = "justification")]
        public string Justification { get; set; }
        [XmlAttribute(AttributeName = "selectable")]
        public string Selectable { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
    }

    [XmlRoot(ElementName = "signalmasticon")]
    public class Signalmasticon
    {
        [XmlAttribute(AttributeName = "signalmast")]
        public string Signalmast { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "level")]
        public string Level { get; set; }
        [XmlAttribute(AttributeName = "forcecontroloff")]
        public string Forcecontroloff { get; set; }
        [XmlAttribute(AttributeName = "hidden")]
        public string Hidden { get; set; }
        [XmlAttribute(AttributeName = "positionable")]
        public string Positionable { get; set; }
        [XmlAttribute(AttributeName = "showtooltip")]
        public string Showtooltip { get; set; }
        [XmlAttribute(AttributeName = "editable")]
        public string Editable { get; set; }
        [XmlAttribute(AttributeName = "clickmode")]
        public string Clickmode { get; set; }
        [XmlAttribute(AttributeName = "litmode")]
        public string Litmode { get; set; }
        [XmlAttribute(AttributeName = "degrees")]
        public string Degrees { get; set; }
        [XmlAttribute(AttributeName = "scale")]
        public string Scale { get; set; }
        [XmlAttribute(AttributeName = "imageset")]
        public string Imageset { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
    }

    [XmlRoot(ElementName = "active")]
    public class Active
    {
        [XmlElement(ElementName = "rotation")]
        public string Rotation { get; set; }
        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }
        [XmlAttribute(AttributeName = "degrees")]
        public string Degrees { get; set; }
        [XmlAttribute(AttributeName = "scale")]
        public string Scale { get; set; }
    }

    [XmlRoot(ElementName = "inactive")]
    public class Inactive
    {
        [XmlElement(ElementName = "rotation")]
        public string Rotation { get; set; }
        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }
        [XmlAttribute(AttributeName = "degrees")]
        public string Degrees { get; set; }
        [XmlAttribute(AttributeName = "scale")]
        public string Scale { get; set; }
    }

    [XmlRoot(ElementName = "unknown")]
    public class Unknown
    {
        [XmlElement(ElementName = "rotation")]
        public string Rotation { get; set; }
        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }
        [XmlAttribute(AttributeName = "degrees")]
        public string Degrees { get; set; }
        [XmlAttribute(AttributeName = "scale")]
        public string Scale { get; set; }
    }

    [XmlRoot(ElementName = "inconsistent")]
    public class Inconsistent
    {
        [XmlElement(ElementName = "rotation")]
        public string Rotation { get; set; }
        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }
        [XmlAttribute(AttributeName = "degrees")]
        public string Degrees { get; set; }
        [XmlAttribute(AttributeName = "scale")]
        public string Scale { get; set; }
    }

    [XmlRoot(ElementName = "sensoricon")]
    public class Sensoricon
    {
        [XmlElement(ElementName = "active")]
        public Active Active { get; set; }
        [XmlElement(ElementName = "inactive")]
        public Inactive Inactive { get; set; }
        [XmlElement(ElementName = "unknown")]
        public Unknown Unknown { get; set; }
        [XmlElement(ElementName = "inconsistent")]
        public Inconsistent Inconsistent { get; set; }
        [XmlElement(ElementName = "iconmaps")]
        public string Iconmaps { get; set; }
        [XmlAttribute(AttributeName = "sensor")]
        public string Sensor { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "level")]
        public string Level { get; set; }
        [XmlAttribute(AttributeName = "forcecontroloff")]
        public string Forcecontroloff { get; set; }
        [XmlAttribute(AttributeName = "hidden")]
        public string Hidden { get; set; }
        [XmlAttribute(AttributeName = "positionable")]
        public string Positionable { get; set; }
        [XmlAttribute(AttributeName = "showtooltip")]
        public string Showtooltip { get; set; }
        [XmlAttribute(AttributeName = "editable")]
        public string Editable { get; set; }
        [XmlAttribute(AttributeName = "momentary")]
        public string Momentary { get; set; }
        [XmlAttribute(AttributeName = "icon")]
        public string Icon { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
    }

    [XmlRoot(ElementName = "layoutturnout")]
    public class Layoutturnout
    {
        [XmlAttribute(AttributeName = "ident")]
        public string Ident { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "hidden")]
        public string Hidden { get; set; }
        [XmlAttribute(AttributeName = "disabled")]
        public string Disabled { get; set; }
        [XmlAttribute(AttributeName = "disableWhenOccupied")]
        public string DisableWhenOccupied { get; set; }
        [XmlAttribute(AttributeName = "continuing")]
        public string Continuing { get; set; }
        [XmlAttribute(AttributeName = "xcen")]
        public string Xcen { get; set; }
        [XmlAttribute(AttributeName = "ycen")]
        public string Ycen { get; set; }
        [XmlAttribute(AttributeName = "xa")]
        public string Xa { get; set; }
        [XmlAttribute(AttributeName = "ya")]
        public string Ya { get; set; }
        [XmlAttribute(AttributeName = "xb")]
        public string Xb { get; set; }
        [XmlAttribute(AttributeName = "yb")]
        public string Yb { get; set; }
        [XmlAttribute(AttributeName = "xc")]
        public string Xc { get; set; }
        [XmlAttribute(AttributeName = "yc")]
        public string Yc { get; set; }
        [XmlAttribute(AttributeName = "xd")]
        public string Xd { get; set; }
        [XmlAttribute(AttributeName = "yd")]
        public string Yd { get; set; }
        [XmlAttribute(AttributeName = "ver")]
        public string Ver { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
        [XmlAttribute(AttributeName = "turnoutname")]
        public string Turnoutname { get; set; }
        [XmlAttribute(AttributeName = "secondturnoutname")]
        public string Secondturnoutname { get; set; }
        [XmlAttribute(AttributeName = "blockname")]
        public string Blockname { get; set; }
        [XmlAttribute(AttributeName = "blockcname")]
        public string Blockcname { get; set; }
        [XmlAttribute(AttributeName = "blockdname")]
        public string Blockdname { get; set; }
        [XmlAttribute(AttributeName = "connectaname")]
        public string Connectaname { get; set; }
        [XmlAttribute(AttributeName = "connectbname")]
        public string Connectbname { get; set; }
        [XmlAttribute(AttributeName = "connectcname")]
        public string Connectcname { get; set; }
        [XmlAttribute(AttributeName = "connectdname")]
        public string Connectdname { get; set; }
    }

    [XmlRoot(ElementName = "tracksegment")]
    public class Tracksegment
    {
        [XmlAttribute(AttributeName = "ident")]
        public string Ident { get; set; }
        [XmlAttribute(AttributeName = "blockname")]
        public string Blockname { get; set; }
        [XmlAttribute(AttributeName = "connect1name")]
        public string Connect1name { get; set; }
        [XmlAttribute(AttributeName = "type1")]
        public string Type1 { get; set; }
        [XmlAttribute(AttributeName = "connect2name")]
        public string Connect2name { get; set; }
        [XmlAttribute(AttributeName = "type2")]
        public string Type2 { get; set; }
        [XmlAttribute(AttributeName = "dashed")]
        public string Dashed { get; set; }
        [XmlAttribute(AttributeName = "mainline")]
        public string Mainline { get; set; }
        [XmlAttribute(AttributeName = "hidden")]
        public string Hidden { get; set; }
        [XmlAttribute(AttributeName = "arc")]
        public string Arc { get; set; }
        [XmlAttribute(AttributeName = "flip")]
        public string Flip { get; set; }
        [XmlAttribute(AttributeName = "circle")]
        public string Circle { get; set; }
        [XmlAttribute(AttributeName = "angle")]
        public string Angle { get; set; }
        [XmlAttribute(AttributeName = "hideConLines")]
        public string HideConLines { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
        [XmlElement(ElementName = "decorations")]
        public Decorations Decorations { get; set; }
    }

    [XmlRoot(ElementName = "bridge")]
    public class Bridge
    {
        [XmlAttribute(AttributeName = "side")]
        public string Side { get; set; }
        [XmlAttribute(AttributeName = "color")]
        public string Color { get; set; }
        [XmlAttribute(AttributeName = "linewidth")]
        public string Linewidth { get; set; }
        [XmlAttribute(AttributeName = "approachwidth")]
        public string Approachwidth { get; set; }
        [XmlAttribute(AttributeName = "deckwidth")]
        public string Deckwidth { get; set; }
    }

    [XmlRoot(ElementName = "decorations")]
    public class Decorations
    {
        [XmlElement(ElementName = "bridge")]
        public Bridge Bridge { get; set; }
    }

    [XmlRoot(ElementName = "positionablepoint")]
    public class Positionablepoint
    {
        [XmlAttribute(AttributeName = "ident")]
        public string Ident { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "connect1name")]
        public string Connect1name { get; set; }
        [XmlAttribute(AttributeName = "connect2name")]
        public string Connect2name { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
        [XmlAttribute(AttributeName = "westboundsignalmast")]
        public string Westboundsignalmast { get; set; }
        [XmlAttribute(AttributeName = "eastboundsignalmast")]
        public string Eastboundsignalmast { get; set; }
    }

    [XmlRoot(ElementName = "A-C")]
    public class AC
    {
        [XmlElement(ElementName = "turnout")]
        public string Turnout { get; set; }
        [XmlElement(ElementName = "turnoutB")]
        public string TurnoutB { get; set; }
    }

    [XmlRoot(ElementName = "A-D")]
    public class AD
    {
        [XmlElement(ElementName = "turnout")]
        public string Turnout { get; set; }
        [XmlElement(ElementName = "turnoutB")]
        public string TurnoutB { get; set; }
    }

    [XmlRoot(ElementName = "B-D")]
    public class BD
    {
        [XmlElement(ElementName = "turnout")]
        public string Turnout { get; set; }
        [XmlElement(ElementName = "turnoutB")]
        public string TurnoutB { get; set; }
    }

    [XmlRoot(ElementName = "B-C")]
    public class BC
    {
        [XmlElement(ElementName = "turnout")]
        public string Turnout { get; set; }
        [XmlElement(ElementName = "turnoutB")]
        public string TurnoutB { get; set; }
    }

    [XmlRoot(ElementName = "states")]
    public class States
    {
        [XmlElement(ElementName = "A-C")]
        public AC AC { get; set; }
        [XmlElement(ElementName = "A-D")]
        public AD AD { get; set; }
        [XmlElement(ElementName = "B-D")]
        public BD BD { get; set; }
        [XmlElement(ElementName = "B-C")]
        public BC BC { get; set; }
    }

    [XmlRoot(ElementName = "layoutSlip")]
    public class LayoutSlip
    {
        [XmlElement(ElementName = "turnout")]
        public string Turnout { get; set; }
        [XmlElement(ElementName = "turnoutB")]
        public string TurnoutB { get; set; }
        [XmlElement(ElementName = "states")]
        public States States { get; set; }
        [XmlAttribute(AttributeName = "ident")]
        public string Ident { get; set; }
        [XmlAttribute(AttributeName = "slipType")]
        public string SlipType { get; set; }
        [XmlAttribute(AttributeName = "hidden")]
        public string Hidden { get; set; }
        [XmlAttribute(AttributeName = "disabled")]
        public string Disabled { get; set; }
        [XmlAttribute(AttributeName = "disableWhenOccupied")]
        public string DisableWhenOccupied { get; set; }
        [XmlAttribute(AttributeName = "xcen")]
        public string Xcen { get; set; }
        [XmlAttribute(AttributeName = "ycen")]
        public string Ycen { get; set; }
        [XmlAttribute(AttributeName = "xa")]
        public string Xa { get; set; }
        [XmlAttribute(AttributeName = "ya")]
        public string Ya { get; set; }
        [XmlAttribute(AttributeName = "xb")]
        public string Xb { get; set; }
        [XmlAttribute(AttributeName = "yb")]
        public string Yb { get; set; }
        [XmlAttribute(AttributeName = "blockname")]
        public string Blockname { get; set; }
        [XmlAttribute(AttributeName = "connectaname")]
        public string Connectaname { get; set; }
        [XmlAttribute(AttributeName = "connectbname")]
        public string Connectbname { get; set; }
        [XmlAttribute(AttributeName = "connectcname")]
        public string Connectcname { get; set; }
        [XmlAttribute(AttributeName = "connectdname")]
        public string Connectdname { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
    }

    [XmlRoot(ElementName = "LayoutEditor")]
    public class LayoutEditor
    {
        [XmlElement(ElementName = "layoutTrackDrawingOptions")]
        public LayoutTrackDrawingOptions LayoutTrackDrawingOptions { get; set; }
        [XmlElement(ElementName = "BlockContentsIcon")]
        public List<BlockContentsIcon> BlockContentsIcon { get; set; }
        [XmlElement(ElementName = "signalmasticon")]
        public List<Signalmasticon> Signalmasticon { get; set; }
        [XmlElement(ElementName = "sensoricon")]
        public List<Sensoricon> Sensoricon { get; set; }
        [XmlElement(ElementName = "layoutturnout")]
        public List<Layoutturnout> Layoutturnout { get; set; }
        [XmlElement(ElementName = "tracksegment")]
        public List<Tracksegment> Tracksegment { get; set; }
        [XmlElement(ElementName = "positionablepoint")]
        public List<Positionablepoint> Positionablepoint { get; set; }
        [XmlElement(ElementName = "layoutSlip")]
        public List<LayoutSlip> LayoutSlip { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "windowheight")]
        public string Windowheight { get; set; }
        [XmlAttribute(AttributeName = "windowwidth")]
        public string Windowwidth { get; set; }
        [XmlAttribute(AttributeName = "panelheight")]
        public string Panelheight { get; set; }
        [XmlAttribute(AttributeName = "panelwidth")]
        public string Panelwidth { get; set; }
        [XmlAttribute(AttributeName = "sliders")]
        public string Sliders { get; set; }
        [XmlAttribute(AttributeName = "scrollable")]
        public string Scrollable { get; set; }
        [XmlAttribute(AttributeName = "editable")]
        public string Editable { get; set; }
        [XmlAttribute(AttributeName = "positionable")]
        public string Positionable { get; set; }
        [XmlAttribute(AttributeName = "controlling")]
        public string Controlling { get; set; }
        [XmlAttribute(AttributeName = "animating")]
        public string Animating { get; set; }
        [XmlAttribute(AttributeName = "showhelpbar")]
        public string Showhelpbar { get; set; }
        [XmlAttribute(AttributeName = "drawgrid")]
        public string Drawgrid { get; set; }
        [XmlAttribute(AttributeName = "snaponadd")]
        public string Snaponadd { get; set; }
        [XmlAttribute(AttributeName = "snaponmove")]
        public string Snaponmove { get; set; }
        [XmlAttribute(AttributeName = "antialiasing")]
        public string Antialiasing { get; set; }
        [XmlAttribute(AttributeName = "turnoutcircles")]
        public string Turnoutcircles { get; set; }
        [XmlAttribute(AttributeName = "tooltipsnotedit")]
        public string Tooltipsnotedit { get; set; }
        [XmlAttribute(AttributeName = "tooltipsinedit")]
        public string Tooltipsinedit { get; set; }
        [XmlAttribute(AttributeName = "mainlinetrackwidth")]
        public string Mainlinetrackwidth { get; set; }
        [XmlAttribute(AttributeName = "xscale")]
        public string Xscale { get; set; }
        [XmlAttribute(AttributeName = "yscale")]
        public string Yscale { get; set; }
        [XmlAttribute(AttributeName = "sidetrackwidth")]
        public string Sidetrackwidth { get; set; }
        [XmlAttribute(AttributeName = "defaulttrackcolor")]
        public string Defaulttrackcolor { get; set; }
        [XmlAttribute(AttributeName = "defaultoccupiedtrackcolor")]
        public string Defaultoccupiedtrackcolor { get; set; }
        [XmlAttribute(AttributeName = "defaultalternativetrackcolor")]
        public string Defaultalternativetrackcolor { get; set; }
        [XmlAttribute(AttributeName = "defaulttextcolor")]
        public string Defaulttextcolor { get; set; }
        [XmlAttribute(AttributeName = "turnoutcirclecolor")]
        public string Turnoutcirclecolor { get; set; }
        [XmlAttribute(AttributeName = "turnoutcirclethrowncolor")]
        public string Turnoutcirclethrowncolor { get; set; }
        [XmlAttribute(AttributeName = "turnoutfillcontrolcircles")]
        public string Turnoutfillcontrolcircles { get; set; }
        [XmlAttribute(AttributeName = "turnoutcirclesize")]
        public string Turnoutcirclesize { get; set; }
        [XmlAttribute(AttributeName = "turnoutdrawunselectedleg")]
        public string Turnoutdrawunselectedleg { get; set; }
        [XmlAttribute(AttributeName = "turnoutbx")]
        public string Turnoutbx { get; set; }
        [XmlAttribute(AttributeName = "turnoutcx")]
        public string Turnoutcx { get; set; }
        [XmlAttribute(AttributeName = "turnoutwid")]
        public string Turnoutwid { get; set; }
        [XmlAttribute(AttributeName = "xoverlong")]
        public string Xoverlong { get; set; }
        [XmlAttribute(AttributeName = "xoverhwid")]
        public string Xoverhwid { get; set; }
        [XmlAttribute(AttributeName = "xovershort")]
        public string Xovershort { get; set; }
        [XmlAttribute(AttributeName = "autoblkgenerate")]
        public string Autoblkgenerate { get; set; }
        [XmlAttribute(AttributeName = "redBackground")]
        public string RedBackground { get; set; }
        [XmlAttribute(AttributeName = "greenBackground")]
        public string GreenBackground { get; set; }
        [XmlAttribute(AttributeName = "blueBackground")]
        public string BlueBackground { get; set; }
        [XmlAttribute(AttributeName = "gridSize")]
        public string GridSize { get; set; }
        [XmlAttribute(AttributeName = "gridSize2nd")]
        public string GridSize2nd { get; set; }
        [XmlAttribute(AttributeName = "openDispatcher")]
        public string OpenDispatcher { get; set; }
        [XmlAttribute(AttributeName = "useDirectTurnoutControl")]
        public string UseDirectTurnoutControl { get; set; }
    }
}

