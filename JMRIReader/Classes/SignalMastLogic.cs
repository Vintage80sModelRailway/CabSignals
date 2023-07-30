
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class signalmastlogic
{

    private string sourceSignalMastField;

    private signalmastlogicDestinationMast[] destinationMastField;

    private string sourceField;

    /// <remarks/>
    public string sourceSignalMast
    {
        get
        {
            return this.sourceSignalMastField;
        }
        set
        {
            this.sourceSignalMastField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("destinationMast")]
    public signalmastlogicDestinationMast[] destinationMast
    {
        get
        {
            return this.destinationMastField;
        }
        set
        {
            this.destinationMastField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string source
    {
        get
        {
            return this.sourceField;
        }
        set
        {
            this.sourceField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class signalmastlogicDestinationMast
{

    private string destinationSignalMastField;

    private object commentField;

    private string enabledField;

    private string allowAutoMaticSignalMastGenerationField;

    private string useLayoutEditorField;

    private string useLayoutEditorTurnoutsField;

    private string useLayoutEditorBlocksField;

    private string lockTurnoutsField;

    private string destinationField;

    /// <remarks/>
    public string destinationSignalMast
    {
        get
        {
            return this.destinationSignalMastField;
        }
        set
        {
            this.destinationSignalMastField = value;
        }
    }

    /// <remarks/>
    public object comment
    {
        get
        {
            return this.commentField;
        }
        set
        {
            this.commentField = value;
        }
    }

    /// <remarks/>
    public string enabled
    {
        get
        {
            return this.enabledField;
        }
        set
        {
            this.enabledField = value;
        }
    }

    /// <remarks/>
    public string allowAutoMaticSignalMastGeneration
    {
        get
        {
            return this.allowAutoMaticSignalMastGenerationField;
        }
        set
        {
            this.allowAutoMaticSignalMastGenerationField = value;
        }
    }

    /// <remarks/>
    public string useLayoutEditor
    {
        get
        {
            return this.useLayoutEditorField;
        }
        set
        {
            this.useLayoutEditorField = value;
        }
    }

    /// <remarks/>
    public string useLayoutEditorTurnouts
    {
        get
        {
            return this.useLayoutEditorTurnoutsField;
        }
        set
        {
            this.useLayoutEditorTurnoutsField = value;
        }
    }

    /// <remarks/>
    public string useLayoutEditorBlocks
    {
        get
        {
            return this.useLayoutEditorBlocksField;
        }
        set
        {
            this.useLayoutEditorBlocksField = value;
        }
    }

    /// <remarks/>
    public string lockTurnouts
    {
        get
        {
            return this.lockTurnoutsField;
        }
        set
        {
            this.lockTurnoutsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string destination
    {
        get
        {
            return this.destinationField;
        }
        set
        {
            this.destinationField = value;
        }
    }
}

