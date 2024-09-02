
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
using JMRIReader.Classes;
using static JMRIReader.Classes.Enums;
using System.Collections.Generic;
using System;
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class transit
{

    private string systemNameField;

    private string userNameField;
    public List<SectionJourneyLog> Sections { get; set; }
    public List<BlockJourneyLog> BlocksInOrder { get; set; }
    public string StartBlock { get; set; }
    public string EndBlock { get; set; }
    public TransitType Type { get; set; }

    private transitTransitsection[] transitsectionField;

    private string systemName1Field;

    private string userName1Field;
    public string NextTransit { get; set; }
    public TrainDirection NextTransitDirection { get; set; }
    public int NextTransitDelayMS { get; set; }

    public transit GetCopy() { return (transit)this.MemberwiseClone(); }

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

    private transitTransitsectionTransitsectionaction[] transitsectionactionField;

    private string sectionnameField;

    private byte sequenceField;

    private byte directionField;

    private string alternateField;

    private string safeField;

    private string stopallocatingsensorField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("transitsectionaction")]
    public transitTransitsectionTransitsectionaction[] transitsectionaction
    {
        get
        {
            return this.transitsectionactionField;
        }
        set
        {
            this.transitsectionactionField = value;
        }
    }

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

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class transitTransitsectionTransitsectionaction
{

    private int whencodeField;

    private int whatcodeField;

    private string whendataField;

    private string whenstringField;

    private string whatdata1Field;

    private string whatdata2Field;

    private string whatstringField;

    private string whatstring2Field;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public int whencode
    {
        get
        {
            return this.whencodeField;
        }
        set
        {
            this.whencodeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public int whatcode
    {
        get
        {
            return this.whatcodeField;
        }
        set
        {
            this.whatcodeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string whendata
    {
        get
        {
            return this.whendataField;
        }
        set
        {
            this.whendataField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string whenstring
    {
        get
        {
            return this.whenstringField;
        }
        set
        {
            this.whenstringField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string whatdata1
    {
        get
        {
            return this.whatdata1Field;
        }
        set
        {
            this.whatdata1Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string whatdata2
    {
        get
        {
            return this.whatdata2Field;
        }
        set
        {
            this.whatdata2Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string whatstring
    {
        get
        {
            return this.whatstringField;
        }
        set
        {
            this.whatstringField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string whatstring2
    {
        get
        {
            return this.whatstring2Field;
        }
        set
        {
            this.whatstring2Field = value;
        }
    }
}

