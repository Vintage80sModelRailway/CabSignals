
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
using System.Collections.Generic;
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]

public partial class block
{

    private string systemNameField;

    private string userNameField;

    private string commentField;

    private string permissiveField;

    private string occupancysensorField;

    private blockPath[] pathField;

    private string systemName1Field;

    private decimal lengthField;

    private byte curveField;

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
    public string comment
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
    public string permissive
    {
        get
        {
            return this.permissiveField;
        }
        set
        {
            this.permissiveField = value;
        }
    }

    /// <remarks/>
    public string occupancysensor
    {
        get
        {
            return this.occupancysensorField;
        }
        set
        {
            this.occupancysensorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("path")]
    public blockPath[] path
    {
        get
        {
            return this.pathField;
        }
        set
        {
            this.pathField = value;
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
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal length
    {
        get
        {
            return this.lengthField;
        }
        set
        {
            this.lengthField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte curve
    {
        get
        {
            return this.curveField;
        }
        set
        {
            this.curveField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class blockPath
{

    private blockPathBeansetting beansettingField;

    private byte todirField;

    private byte fromdirField;

    private string blockField;

    /// <remarks/>
    public blockPathBeansetting beansetting
    {
        get
        {
            return this.beansettingField;
        }
        set
        {
            this.beansettingField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte todir
    {
        get
        {
            return this.todirField;
        }
        set
        {
            this.todirField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte fromdir
    {
        get
        {
            return this.fromdirField;
        }
        set
        {
            this.fromdirField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string block
    {
        get
        {
            return this.blockField;
        }
        set
        {
            this.blockField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class blockPathBeansetting
{

    private blockPathBeansettingTurnout turnoutField;

    private byte settingField;

    /// <remarks/>
    public blockPathBeansettingTurnout turnout
    {
        get
        {
            return this.turnoutField;
        }
        set
        {
            this.turnoutField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte setting
    {
        get
        {
            return this.settingField;
        }
        set
        {
            this.settingField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class blockPathBeansettingTurnout
{

    private string systemNameField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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
}

