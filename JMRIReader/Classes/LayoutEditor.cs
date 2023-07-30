
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class LayoutEditor
{

    private LayoutEditorLayoutTrackDrawingOptions layoutTrackDrawingOptionsField;

    private LayoutEditorBlockContentsIcon[] blockContentsIconField;

    private LayoutEditorSignalmasticon[] signalmasticonField;

    private LayoutEditorSensoricon[] sensoriconField;

    private LayoutEditorLayoutturnout[] layoutturnoutField;

    private LayoutEditorTracksegment[] tracksegmentField;

    private LayoutEditorPositionablepoint[] positionablepointField;

    private string classField;

    private string nameField;

    private sbyte xField;

    private sbyte yField;

    private ushort windowheightField;

    private ushort windowwidthField;

    private ushort panelheightField;

    private ushort panelwidthField;

    private string slidersField;

    private string scrollableField;

    private string editableField;

    private string positionableField;

    private string controllingField;

    private string animatingField;

    private string showhelpbarField;

    private string drawgridField;

    private string snaponaddField;

    private string snaponmoveField;

    private string antialiasingField;

    private string turnoutcirclesField;

    private string tooltipsnoteditField;

    private string tooltipsineditField;

    private byte mainlinetrackwidthField;

    private decimal xscaleField;

    private decimal yscaleField;

    private byte sidetrackwidthField;

    private string defaulttrackcolorField;

    private string defaultoccupiedtrackcolorField;

    private string defaultalternativetrackcolorField;

    private string defaulttextcolorField;

    private string turnoutcirclecolorField;

    private string turnoutcirclethrowncolorField;

    private string turnoutfillcontrolcirclesField;

    private byte turnoutcirclesizeField;

    private string turnoutdrawunselectedlegField;

    private decimal turnoutbxField;

    private decimal turnoutcxField;

    private decimal turnoutwidField;

    private decimal xoverlongField;

    private decimal xoverhwidField;

    private decimal xovershortField;

    private string autoblkgenerateField;

    private byte redBackgroundField;

    private byte greenBackgroundField;

    private byte blueBackgroundField;

    private byte gridSizeField;

    private byte gridSize2ndField;

    private string openDispatcherField;

    private string useDirectTurnoutControlField;

    /// <remarks/>
    public LayoutEditorLayoutTrackDrawingOptions layoutTrackDrawingOptions
    {
        get
        {
            return this.layoutTrackDrawingOptionsField;
        }
        set
        {
            this.layoutTrackDrawingOptionsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("BlockContentsIcon")]
    public LayoutEditorBlockContentsIcon[] BlockContentsIcon
    {
        get
        {
            return this.blockContentsIconField;
        }
        set
        {
            this.blockContentsIconField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("signalmasticon")]
    public LayoutEditorSignalmasticon[] signalmasticon
    {
        get
        {
            return this.signalmasticonField;
        }
        set
        {
            this.signalmasticonField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("sensoricon")]
    public LayoutEditorSensoricon[] sensoricon
    {
        get
        {
            return this.sensoriconField;
        }
        set
        {
            this.sensoriconField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("layoutturnout")]
    public LayoutEditorLayoutturnout[] layoutturnout
    {
        get
        {
            return this.layoutturnoutField;
        }
        set
        {
            this.layoutturnoutField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("tracksegment")]
    public LayoutEditorTracksegment[] tracksegment
    {
        get
        {
            return this.tracksegmentField;
        }
        set
        {
            this.tracksegmentField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("positionablepoint")]
    public LayoutEditorPositionablepoint[] positionablepoint
    {
        get
        {
            return this.positionablepointField;
        }
        set
        {
            this.positionablepointField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @class
    {
        get
        {
            return this.classField;
        }
        set
        {
            this.classField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public sbyte x
    {
        get
        {
            return this.xField;
        }
        set
        {
            this.xField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public sbyte y
    {
        get
        {
            return this.yField;
        }
        set
        {
            this.yField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort windowheight
    {
        get
        {
            return this.windowheightField;
        }
        set
        {
            this.windowheightField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort windowwidth
    {
        get
        {
            return this.windowwidthField;
        }
        set
        {
            this.windowwidthField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort panelheight
    {
        get
        {
            return this.panelheightField;
        }
        set
        {
            this.panelheightField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort panelwidth
    {
        get
        {
            return this.panelwidthField;
        }
        set
        {
            this.panelwidthField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string sliders
    {
        get
        {
            return this.slidersField;
        }
        set
        {
            this.slidersField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string scrollable
    {
        get
        {
            return this.scrollableField;
        }
        set
        {
            this.scrollableField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string editable
    {
        get
        {
            return this.editableField;
        }
        set
        {
            this.editableField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string positionable
    {
        get
        {
            return this.positionableField;
        }
        set
        {
            this.positionableField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string controlling
    {
        get
        {
            return this.controllingField;
        }
        set
        {
            this.controllingField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string animating
    {
        get
        {
            return this.animatingField;
        }
        set
        {
            this.animatingField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string showhelpbar
    {
        get
        {
            return this.showhelpbarField;
        }
        set
        {
            this.showhelpbarField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string drawgrid
    {
        get
        {
            return this.drawgridField;
        }
        set
        {
            this.drawgridField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string snaponadd
    {
        get
        {
            return this.snaponaddField;
        }
        set
        {
            this.snaponaddField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string snaponmove
    {
        get
        {
            return this.snaponmoveField;
        }
        set
        {
            this.snaponmoveField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string antialiasing
    {
        get
        {
            return this.antialiasingField;
        }
        set
        {
            this.antialiasingField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string turnoutcircles
    {
        get
        {
            return this.turnoutcirclesField;
        }
        set
        {
            this.turnoutcirclesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string tooltipsnotedit
    {
        get
        {
            return this.tooltipsnoteditField;
        }
        set
        {
            this.tooltipsnoteditField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string tooltipsinedit
    {
        get
        {
            return this.tooltipsineditField;
        }
        set
        {
            this.tooltipsineditField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte mainlinetrackwidth
    {
        get
        {
            return this.mainlinetrackwidthField;
        }
        set
        {
            this.mainlinetrackwidthField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal xscale
    {
        get
        {
            return this.xscaleField;
        }
        set
        {
            this.xscaleField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal yscale
    {
        get
        {
            return this.yscaleField;
        }
        set
        {
            this.yscaleField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte sidetrackwidth
    {
        get
        {
            return this.sidetrackwidthField;
        }
        set
        {
            this.sidetrackwidthField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string defaulttrackcolor
    {
        get
        {
            return this.defaulttrackcolorField;
        }
        set
        {
            this.defaulttrackcolorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string defaultoccupiedtrackcolor
    {
        get
        {
            return this.defaultoccupiedtrackcolorField;
        }
        set
        {
            this.defaultoccupiedtrackcolorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string defaultalternativetrackcolor
    {
        get
        {
            return this.defaultalternativetrackcolorField;
        }
        set
        {
            this.defaultalternativetrackcolorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string defaulttextcolor
    {
        get
        {
            return this.defaulttextcolorField;
        }
        set
        {
            this.defaulttextcolorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string turnoutcirclecolor
    {
        get
        {
            return this.turnoutcirclecolorField;
        }
        set
        {
            this.turnoutcirclecolorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string turnoutcirclethrowncolor
    {
        get
        {
            return this.turnoutcirclethrowncolorField;
        }
        set
        {
            this.turnoutcirclethrowncolorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string turnoutfillcontrolcircles
    {
        get
        {
            return this.turnoutfillcontrolcirclesField;
        }
        set
        {
            this.turnoutfillcontrolcirclesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte turnoutcirclesize
    {
        get
        {
            return this.turnoutcirclesizeField;
        }
        set
        {
            this.turnoutcirclesizeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string turnoutdrawunselectedleg
    {
        get
        {
            return this.turnoutdrawunselectedlegField;
        }
        set
        {
            this.turnoutdrawunselectedlegField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal turnoutbx
    {
        get
        {
            return this.turnoutbxField;
        }
        set
        {
            this.turnoutbxField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal turnoutcx
    {
        get
        {
            return this.turnoutcxField;
        }
        set
        {
            this.turnoutcxField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal turnoutwid
    {
        get
        {
            return this.turnoutwidField;
        }
        set
        {
            this.turnoutwidField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal xoverlong
    {
        get
        {
            return this.xoverlongField;
        }
        set
        {
            this.xoverlongField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal xoverhwid
    {
        get
        {
            return this.xoverhwidField;
        }
        set
        {
            this.xoverhwidField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal xovershort
    {
        get
        {
            return this.xovershortField;
        }
        set
        {
            this.xovershortField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string autoblkgenerate
    {
        get
        {
            return this.autoblkgenerateField;
        }
        set
        {
            this.autoblkgenerateField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte redBackground
    {
        get
        {
            return this.redBackgroundField;
        }
        set
        {
            this.redBackgroundField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte greenBackground
    {
        get
        {
            return this.greenBackgroundField;
        }
        set
        {
            this.greenBackgroundField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte blueBackground
    {
        get
        {
            return this.blueBackgroundField;
        }
        set
        {
            this.blueBackgroundField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte gridSize
    {
        get
        {
            return this.gridSizeField;
        }
        set
        {
            this.gridSizeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte gridSize2nd
    {
        get
        {
            return this.gridSize2ndField;
        }
        set
        {
            this.gridSize2ndField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string openDispatcher
    {
        get
        {
            return this.openDispatcherField;
        }
        set
        {
            this.openDispatcherField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string useDirectTurnoutControl
    {
        get
        {
            return this.useDirectTurnoutControlField;
        }
        set
        {
            this.useDirectTurnoutControlField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorLayoutTrackDrawingOptions
{

    private string mainBallastColorField;

    private byte mainBallastWidthField;

    private byte mainBlockLineDashPercentageX10Field;

    private byte mainBlockLineWidthField;

    private string mainRailColorField;

    private byte mainRailCountField;

    private byte mainRailGapField;

    private byte mainRailWidthField;

    private string mainTieColorField;

    private byte mainTieGapField;

    private byte mainTieLengthField;

    private byte mainTieWidthField;

    private string sideBallastColorField;

    private byte sideBallastWidthField;

    private byte sideBlockLineDashPercentageX10Field;

    private byte sideBlockLineWidthField;

    private string sideRailColorField;

    private byte sideRailCountField;

    private byte sideRailGapField;

    private byte sideRailWidthField;

    private string sideTieColorField;

    private byte sideTieGapField;

    private byte sideTieLengthField;

    private byte sideTieWidthField;

    private string nameField;

    private string classField;

    /// <remarks/>
    public string mainBallastColor
    {
        get
        {
            return this.mainBallastColorField;
        }
        set
        {
            this.mainBallastColorField = value;
        }
    }

    /// <remarks/>
    public byte mainBallastWidth
    {
        get
        {
            return this.mainBallastWidthField;
        }
        set
        {
            this.mainBallastWidthField = value;
        }
    }

    /// <remarks/>
    public byte mainBlockLineDashPercentageX10
    {
        get
        {
            return this.mainBlockLineDashPercentageX10Field;
        }
        set
        {
            this.mainBlockLineDashPercentageX10Field = value;
        }
    }

    /// <remarks/>
    public byte mainBlockLineWidth
    {
        get
        {
            return this.mainBlockLineWidthField;
        }
        set
        {
            this.mainBlockLineWidthField = value;
        }
    }

    /// <remarks/>
    public string mainRailColor
    {
        get
        {
            return this.mainRailColorField;
        }
        set
        {
            this.mainRailColorField = value;
        }
    }

    /// <remarks/>
    public byte mainRailCount
    {
        get
        {
            return this.mainRailCountField;
        }
        set
        {
            this.mainRailCountField = value;
        }
    }

    /// <remarks/>
    public byte mainRailGap
    {
        get
        {
            return this.mainRailGapField;
        }
        set
        {
            this.mainRailGapField = value;
        }
    }

    /// <remarks/>
    public byte mainRailWidth
    {
        get
        {
            return this.mainRailWidthField;
        }
        set
        {
            this.mainRailWidthField = value;
        }
    }

    /// <remarks/>
    public string mainTieColor
    {
        get
        {
            return this.mainTieColorField;
        }
        set
        {
            this.mainTieColorField = value;
        }
    }

    /// <remarks/>
    public byte mainTieGap
    {
        get
        {
            return this.mainTieGapField;
        }
        set
        {
            this.mainTieGapField = value;
        }
    }

    /// <remarks/>
    public byte mainTieLength
    {
        get
        {
            return this.mainTieLengthField;
        }
        set
        {
            this.mainTieLengthField = value;
        }
    }

    /// <remarks/>
    public byte mainTieWidth
    {
        get
        {
            return this.mainTieWidthField;
        }
        set
        {
            this.mainTieWidthField = value;
        }
    }

    /// <remarks/>
    public string sideBallastColor
    {
        get
        {
            return this.sideBallastColorField;
        }
        set
        {
            this.sideBallastColorField = value;
        }
    }

    /// <remarks/>
    public byte sideBallastWidth
    {
        get
        {
            return this.sideBallastWidthField;
        }
        set
        {
            this.sideBallastWidthField = value;
        }
    }

    /// <remarks/>
    public byte sideBlockLineDashPercentageX10
    {
        get
        {
            return this.sideBlockLineDashPercentageX10Field;
        }
        set
        {
            this.sideBlockLineDashPercentageX10Field = value;
        }
    }

    /// <remarks/>
    public byte sideBlockLineWidth
    {
        get
        {
            return this.sideBlockLineWidthField;
        }
        set
        {
            this.sideBlockLineWidthField = value;
        }
    }

    /// <remarks/>
    public string sideRailColor
    {
        get
        {
            return this.sideRailColorField;
        }
        set
        {
            this.sideRailColorField = value;
        }
    }

    /// <remarks/>
    public byte sideRailCount
    {
        get
        {
            return this.sideRailCountField;
        }
        set
        {
            this.sideRailCountField = value;
        }
    }

    /// <remarks/>
    public byte sideRailGap
    {
        get
        {
            return this.sideRailGapField;
        }
        set
        {
            this.sideRailGapField = value;
        }
    }

    /// <remarks/>
    public byte sideRailWidth
    {
        get
        {
            return this.sideRailWidthField;
        }
        set
        {
            this.sideRailWidthField = value;
        }
    }

    /// <remarks/>
    public string sideTieColor
    {
        get
        {
            return this.sideTieColorField;
        }
        set
        {
            this.sideTieColorField = value;
        }
    }

    /// <remarks/>
    public byte sideTieGap
    {
        get
        {
            return this.sideTieGapField;
        }
        set
        {
            this.sideTieGapField = value;
        }
    }

    /// <remarks/>
    public byte sideTieLength
    {
        get
        {
            return this.sideTieLengthField;
        }
        set
        {
            this.sideTieLengthField = value;
        }
    }

    /// <remarks/>
    public byte sideTieWidth
    {
        get
        {
            return this.sideTieWidthField;
        }
        set
        {
            this.sideTieWidthField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @class
    {
        get
        {
            return this.classField;
        }
        set
        {
            this.classField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorBlockContentsIcon
{

    private string blockcontentsField;

    private ushort xField;

    private ushort yField;

    private byte levelField;

    private bool forcecontroloffField;

    private string hiddenField;

    private bool positionableField;

    private bool showtooltipField;

    private bool editableField;

    private string fontnameField;

    private byte sizeField;

    private byte styleField;

    private byte redField;

    private byte greenField;

    private byte blueField;

    private string hasBackgroundField;

    private string justificationField;

    private string selectableField;

    private string classField;

    private ushort degreesField;

    private bool degreesFieldSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string blockcontents
    {
        get
        {
            return this.blockcontentsField;
        }
        set
        {
            this.blockcontentsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort x
    {
        get
        {
            return this.xField;
        }
        set
        {
            this.xField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort y
    {
        get
        {
            return this.yField;
        }
        set
        {
            this.yField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte level
    {
        get
        {
            return this.levelField;
        }
        set
        {
            this.levelField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool forcecontroloff
    {
        get
        {
            return this.forcecontroloffField;
        }
        set
        {
            this.forcecontroloffField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string hidden
    {
        get
        {
            return this.hiddenField;
        }
        set
        {
            this.hiddenField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool positionable
    {
        get
        {
            return this.positionableField;
        }
        set
        {
            this.positionableField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool showtooltip
    {
        get
        {
            return this.showtooltipField;
        }
        set
        {
            this.showtooltipField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool editable
    {
        get
        {
            return this.editableField;
        }
        set
        {
            this.editableField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string fontname
    {
        get
        {
            return this.fontnameField;
        }
        set
        {
            this.fontnameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte size
    {
        get
        {
            return this.sizeField;
        }
        set
        {
            this.sizeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte style
    {
        get
        {
            return this.styleField;
        }
        set
        {
            this.styleField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte red
    {
        get
        {
            return this.redField;
        }
        set
        {
            this.redField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte green
    {
        get
        {
            return this.greenField;
        }
        set
        {
            this.greenField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte blue
    {
        get
        {
            return this.blueField;
        }
        set
        {
            this.blueField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string hasBackground
    {
        get
        {
            return this.hasBackgroundField;
        }
        set
        {
            this.hasBackgroundField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string justification
    {
        get
        {
            return this.justificationField;
        }
        set
        {
            this.justificationField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string selectable
    {
        get
        {
            return this.selectableField;
        }
        set
        {
            this.selectableField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @class
    {
        get
        {
            return this.classField;
        }
        set
        {
            this.classField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort degrees
    {
        get
        {
            return this.degreesField;
        }
        set
        {
            this.degreesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool degreesSpecified
    {
        get
        {
            return this.degreesFieldSpecified;
        }
        set
        {
            this.degreesFieldSpecified = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorSignalmasticon
{

    private string signalmastField;

    private ushort xField;

    private ushort yField;

    private byte levelField;

    private bool forcecontroloffField;

    private string hiddenField;

    private bool positionableField;

    private bool showtooltipField;

    private bool editableField;

    private byte clickmodeField;

    private bool litmodeField;

    private ushort degreesField;

    private decimal scaleField;

    private string imagesetField;

    private string classField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string signalmast
    {
        get
        {
            return this.signalmastField;
        }
        set
        {
            this.signalmastField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort x
    {
        get
        {
            return this.xField;
        }
        set
        {
            this.xField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort y
    {
        get
        {
            return this.yField;
        }
        set
        {
            this.yField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte level
    {
        get
        {
            return this.levelField;
        }
        set
        {
            this.levelField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool forcecontroloff
    {
        get
        {
            return this.forcecontroloffField;
        }
        set
        {
            this.forcecontroloffField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string hidden
    {
        get
        {
            return this.hiddenField;
        }
        set
        {
            this.hiddenField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool positionable
    {
        get
        {
            return this.positionableField;
        }
        set
        {
            this.positionableField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool showtooltip
    {
        get
        {
            return this.showtooltipField;
        }
        set
        {
            this.showtooltipField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool editable
    {
        get
        {
            return this.editableField;
        }
        set
        {
            this.editableField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte clickmode
    {
        get
        {
            return this.clickmodeField;
        }
        set
        {
            this.clickmodeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool litmode
    {
        get
        {
            return this.litmodeField;
        }
        set
        {
            this.litmodeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort degrees
    {
        get
        {
            return this.degreesField;
        }
        set
        {
            this.degreesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal scale
    {
        get
        {
            return this.scaleField;
        }
        set
        {
            this.scaleField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string imageset
    {
        get
        {
            return this.imagesetField;
        }
        set
        {
            this.imagesetField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @class
    {
        get
        {
            return this.classField;
        }
        set
        {
            this.classField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorSensoricon
{

    private LayoutEditorSensoriconActive activeField;

    private LayoutEditorSensoriconInactive inactiveField;

    private LayoutEditorSensoriconUnknown unknownField;

    private LayoutEditorSensoriconInconsistent inconsistentField;

    private object iconmapsField;

    private string sensorField;

    private ushort xField;

    private ushort yField;

    private byte levelField;

    private bool forcecontroloffField;

    private string hiddenField;

    private bool positionableField;

    private bool showtooltipField;

    private bool editableField;

    private bool momentaryField;

    private string iconField;

    private string classField;

    /// <remarks/>
    public LayoutEditorSensoriconActive active
    {
        get
        {
            return this.activeField;
        }
        set
        {
            this.activeField = value;
        }
    }

    /// <remarks/>
    public LayoutEditorSensoriconInactive inactive
    {
        get
        {
            return this.inactiveField;
        }
        set
        {
            this.inactiveField = value;
        }
    }

    /// <remarks/>
    public LayoutEditorSensoriconUnknown unknown
    {
        get
        {
            return this.unknownField;
        }
        set
        {
            this.unknownField = value;
        }
    }

    /// <remarks/>
    public LayoutEditorSensoriconInconsistent inconsistent
    {
        get
        {
            return this.inconsistentField;
        }
        set
        {
            this.inconsistentField = value;
        }
    }

    /// <remarks/>
    public object iconmaps
    {
        get
        {
            return this.iconmapsField;
        }
        set
        {
            this.iconmapsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string sensor
    {
        get
        {
            return this.sensorField;
        }
        set
        {
            this.sensorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort x
    {
        get
        {
            return this.xField;
        }
        set
        {
            this.xField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort y
    {
        get
        {
            return this.yField;
        }
        set
        {
            this.yField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte level
    {
        get
        {
            return this.levelField;
        }
        set
        {
            this.levelField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool forcecontroloff
    {
        get
        {
            return this.forcecontroloffField;
        }
        set
        {
            this.forcecontroloffField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string hidden
    {
        get
        {
            return this.hiddenField;
        }
        set
        {
            this.hiddenField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool positionable
    {
        get
        {
            return this.positionableField;
        }
        set
        {
            this.positionableField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool showtooltip
    {
        get
        {
            return this.showtooltipField;
        }
        set
        {
            this.showtooltipField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool editable
    {
        get
        {
            return this.editableField;
        }
        set
        {
            this.editableField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool momentary
    {
        get
        {
            return this.momentaryField;
        }
        set
        {
            this.momentaryField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string icon
    {
        get
        {
            return this.iconField;
        }
        set
        {
            this.iconField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @class
    {
        get
        {
            return this.classField;
        }
        set
        {
            this.classField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorSensoriconActive
{

    private byte rotationField;

    private string urlField;

    private byte degreesField;

    private decimal scaleField;

    /// <remarks/>
    public byte rotation
    {
        get
        {
            return this.rotationField;
        }
        set
        {
            this.rotationField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string url
    {
        get
        {
            return this.urlField;
        }
        set
        {
            this.urlField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte degrees
    {
        get
        {
            return this.degreesField;
        }
        set
        {
            this.degreesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal scale
    {
        get
        {
            return this.scaleField;
        }
        set
        {
            this.scaleField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorSensoriconInactive
{

    private byte rotationField;

    private string urlField;

    private byte degreesField;

    private decimal scaleField;

    /// <remarks/>
    public byte rotation
    {
        get
        {
            return this.rotationField;
        }
        set
        {
            this.rotationField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string url
    {
        get
        {
            return this.urlField;
        }
        set
        {
            this.urlField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte degrees
    {
        get
        {
            return this.degreesField;
        }
        set
        {
            this.degreesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal scale
    {
        get
        {
            return this.scaleField;
        }
        set
        {
            this.scaleField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorSensoriconUnknown
{

    private byte rotationField;

    private string urlField;

    private byte degreesField;

    private decimal scaleField;

    /// <remarks/>
    public byte rotation
    {
        get
        {
            return this.rotationField;
        }
        set
        {
            this.rotationField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string url
    {
        get
        {
            return this.urlField;
        }
        set
        {
            this.urlField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte degrees
    {
        get
        {
            return this.degreesField;
        }
        set
        {
            this.degreesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal scale
    {
        get
        {
            return this.scaleField;
        }
        set
        {
            this.scaleField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorSensoriconInconsistent
{

    private byte rotationField;

    private string urlField;

    private byte degreesField;

    private decimal scaleField;

    /// <remarks/>
    public byte rotation
    {
        get
        {
            return this.rotationField;
        }
        set
        {
            this.rotationField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string url
    {
        get
        {
            return this.urlField;
        }
        set
        {
            this.urlField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte degrees
    {
        get
        {
            return this.degreesField;
        }
        set
        {
            this.degreesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal scale
    {
        get
        {
            return this.scaleField;
        }
        set
        {
            this.scaleField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorLayoutturnout
{

    private string identField;

    private string typeField;

    private string hiddenField;

    private string disabledField;

    private string disableWhenOccupiedField;

    private byte continuingField;

    private decimal xcenField;

    private decimal ycenField;

    private decimal xaField;

    private decimal yaField;

    private decimal xbField;

    private decimal ybField;

    private decimal xcField;

    private decimal ycField;

    private decimal xdField;

    private decimal ydField;

    private byte verField;

    private string classField;

    private string turnoutnameField;

    private string secondturnoutnameField;

    private string blocknameField;

    private string connectcnameField;

    private string connectdnameField;

    private string blockcnameField;

    private string blockdnameField;

    private string connectanameField;

    private string connectbnameField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ident
    {
        get
        {
            return this.identField;
        }
        set
        {
            this.identField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string hidden
    {
        get
        {
            return this.hiddenField;
        }
        set
        {
            this.hiddenField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string disabled
    {
        get
        {
            return this.disabledField;
        }
        set
        {
            this.disabledField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string disableWhenOccupied
    {
        get
        {
            return this.disableWhenOccupiedField;
        }
        set
        {
            this.disableWhenOccupiedField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte continuing
    {
        get
        {
            return this.continuingField;
        }
        set
        {
            this.continuingField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal xcen
    {
        get
        {
            return this.xcenField;
        }
        set
        {
            this.xcenField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal ycen
    {
        get
        {
            return this.ycenField;
        }
        set
        {
            this.ycenField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal xa
    {
        get
        {
            return this.xaField;
        }
        set
        {
            this.xaField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal ya
    {
        get
        {
            return this.yaField;
        }
        set
        {
            this.yaField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal xb
    {
        get
        {
            return this.xbField;
        }
        set
        {
            this.xbField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal yb
    {
        get
        {
            return this.ybField;
        }
        set
        {
            this.ybField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal xc
    {
        get
        {
            return this.xcField;
        }
        set
        {
            this.xcField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal yc
    {
        get
        {
            return this.ycField;
        }
        set
        {
            this.ycField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal xd
    {
        get
        {
            return this.xdField;
        }
        set
        {
            this.xdField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal yd
    {
        get
        {
            return this.ydField;
        }
        set
        {
            this.ydField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte ver
    {
        get
        {
            return this.verField;
        }
        set
        {
            this.verField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @class
    {
        get
        {
            return this.classField;
        }
        set
        {
            this.classField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string turnoutname
    {
        get
        {
            return this.turnoutnameField;
        }
        set
        {
            this.turnoutnameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string secondturnoutname
    {
        get
        {
            return this.secondturnoutnameField;
        }
        set
        {
            this.secondturnoutnameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string blockname
    {
        get
        {
            return this.blocknameField;
        }
        set
        {
            this.blocknameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connectcname
    {
        get
        {
            return this.connectcnameField;
        }
        set
        {
            this.connectcnameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connectdname
    {
        get
        {
            return this.connectdnameField;
        }
        set
        {
            this.connectdnameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string blockcname
    {
        get
        {
            return this.blockcnameField;
        }
        set
        {
            this.blockcnameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string blockdname
    {
        get
        {
            return this.blockdnameField;
        }
        set
        {
            this.blockdnameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connectaname
    {
        get
        {
            return this.connectanameField;
        }
        set
        {
            this.connectanameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connectbname
    {
        get
        {
            return this.connectbnameField;
        }
        set
        {
            this.connectbnameField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorTracksegment
{

    private string identField;

    private string blocknameField;

    private string connect1nameField;

    private string type1Field;

    private string connect2nameField;

    private string type2Field;

    private string dashedField;

    private string mainlineField;

    private string hiddenField;

    private string arcField;

    private string flipField;

    private string circleField;

    private decimal angleField;

    private bool angleFieldSpecified;

    private string hideConLinesField;

    private string classField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ident
    {
        get
        {
            return this.identField;
        }
        set
        {
            this.identField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string blockname
    {
        get
        {
            return this.blocknameField;
        }
        set
        {
            this.blocknameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connect1name
    {
        get
        {
            return this.connect1nameField;
        }
        set
        {
            this.connect1nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type1
    {
        get
        {
            return this.type1Field;
        }
        set
        {
            this.type1Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connect2name
    {
        get
        {
            return this.connect2nameField;
        }
        set
        {
            this.connect2nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type2
    {
        get
        {
            return this.type2Field;
        }
        set
        {
            this.type2Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string dashed
    {
        get
        {
            return this.dashedField;
        }
        set
        {
            this.dashedField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string mainline
    {
        get
        {
            return this.mainlineField;
        }
        set
        {
            this.mainlineField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string hidden
    {
        get
        {
            return this.hiddenField;
        }
        set
        {
            this.hiddenField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string arc
    {
        get
        {
            return this.arcField;
        }
        set
        {
            this.arcField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string flip
    {
        get
        {
            return this.flipField;
        }
        set
        {
            this.flipField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string circle
    {
        get
        {
            return this.circleField;
        }
        set
        {
            this.circleField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal angle
    {
        get
        {
            return this.angleField;
        }
        set
        {
            this.angleField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool angleSpecified
    {
        get
        {
            return this.angleFieldSpecified;
        }
        set
        {
            this.angleFieldSpecified = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string hideConLines
    {
        get
        {
            return this.hideConLinesField;
        }
        set
        {
            this.hideConLinesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @class
    {
        get
        {
            return this.classField;
        }
        set
        {
            this.classField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class LayoutEditorPositionablepoint
{

    private string identField;

    private string typeField;

    private decimal xField;

    private decimal yField;

    private string connect1nameField;

    private string connect2nameField;

    private string classField;

    private string westboundsignalmastField;

    private string eastboundsignalmastField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ident
    {
        get
        {
            return this.identField;
        }
        set
        {
            this.identField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal x
    {
        get
        {
            return this.xField;
        }
        set
        {
            this.xField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal y
    {
        get
        {
            return this.yField;
        }
        set
        {
            this.yField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connect1name
    {
        get
        {
            return this.connect1nameField;
        }
        set
        {
            this.connect1nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connect2name
    {
        get
        {
            return this.connect2nameField;
        }
        set
        {
            this.connect2nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @class
    {
        get
        {
            return this.classField;
        }
        set
        {
            this.classField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string westboundsignalmast
    {
        get
        {
            return this.westboundsignalmastField;
        }
        set
        {
            this.westboundsignalmastField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string eastboundsignalmast
    {
        get
        {
            return this.eastboundsignalmastField;
        }
        set
        {
            this.eastboundsignalmastField = value;
        }
    }
}

