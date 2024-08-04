
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
using JMRIReader.Classes;
using System.Collections.Generic;
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class transit
{
    private string systemNameField;

    private string userNameField;

    private transitTransitsection[] transitsectionField;

    private string systemName1Field;

    private string userName1Field;
    public List<SectionJourneyLog> Sections { get; set; }
    public List<BlockJourneyLog> BlocksInOrder { get; set; }
    public string StartBlock { get; set; }
    public string EndBlock { get; set; }

    /// <remarks/>
    public string systemName
    {
        get
        {
            return this.systemNameField;
        }
        set
        {
            this.systemNameField = value;
        }
    }

    /// <remarks/>
    public string userName
    {
        get
        {
            return this.userNameField;
        }
        set
        {
            this.userNameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("transitsection")]
    public transitTransitsection[] transitsection
    {
        get
        {
            return this.transitsectionField;
        }
        set
        {
            this.transitsectionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("systemName")]
    public string systemName1
    {
        get
        {
            return this.systemName1Field;
        }
        set
        {
            this.systemName1Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("userName")]
    public string userName1
    {
        get
        {
            return this.userName1Field;
        }
        set
        {
            this.userName1Field = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class transitTransitsection
{

    private string sectionnameField;

    private byte sequenceField;

    private byte directionField;

    private string alternateField;

    private string safeField;

    private string stopallocatingsensorField;
    public List<APIBlock> Blocks { get; set; }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string sectionname
    {
        get
        {
            return this.sectionnameField;
        }
        set
        {
            this.sectionnameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte sequence
    {
        get
        {
            return this.sequenceField;
        }
        set
        {
            this.sequenceField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte direction
    {
        get
        {
            return this.directionField;
        }
        set
        {
            this.directionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string alternate
    {
        get
        {
            return this.alternateField;
        }
        set
        {
            this.alternateField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string safe
    {
        get
        {
            return this.safeField;
        }
        set
        {
            this.safeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string stopallocatingsensor
    {
        get
        {
            return this.stopallocatingsensorField;
        }
        set
        {
            this.stopallocatingsensorField = value;
        }
    }


}

