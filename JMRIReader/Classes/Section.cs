
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
using JMRIReader.Classes;
using System.Collections.Generic;
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class section
{

    private string systemNameField;

    private string userNameField;

    private sectionBlockentry[] blockentryField;

    private sectionEntrypoint[] entrypointField;

    private string systemName1Field;

    private string userName1Field;

    private string creationtypeField;

    private string fStoppingSensor;
    public List<block> Blocks { get; set; }

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
    [System.Xml.Serialization.XmlElementAttribute("blockentry")]
    public sectionBlockentry[] blockentry
    {
        get
        {
            return this.blockentryField;
        }
        set
        {
            this.blockentryField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("entrypoint")]
    public sectionEntrypoint[] entrypoint
    {
        get
        {
            return this.entrypointField;
        }
        set
        {
            this.entrypointField = value;
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string creationtype
    {
        get
        {
            return this.creationtypeField;
        }
        set
        {
            this.creationtypeField = value;
        }
    }

    [System.Xml.Serialization.XmlAttributeAttribute("fstopsensorname")]
    public string forwardStoppingSensor
    {
        get
        {
            return this.fStoppingSensor;
        }
        set
        {
            this.fStoppingSensor = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class sectionBlockentry
{

    private string sNameField;

    private byte orderField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string sName
    {
        get
        {
            return this.sNameField;
        }
        set
        {
            this.sNameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte order
    {
        get
        {
            return this.orderField;
        }
        set
        {
            this.orderField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class sectionEntrypoint
{

    private string fromblockField;

    private string toblockField;

    private byte directionField;

    private string fixedField;

    private string fromblockdirectionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string fromblock
    {
        get
        {
            return this.fromblockField;
        }
        set
        {
            this.fromblockField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string toblock
    {
        get
        {
            return this.toblockField;
        }
        set
        {
            this.toblockField = value;
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
    public string @fixed
    {
        get
        {
            return this.fixedField;
        }
        set
        {
            this.fixedField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string fromblockdirection
    {
        get
        {
            return this.fromblockdirectionField;
        }
        set
        {
            this.fromblockdirectionField = value;
        }
    }
}

