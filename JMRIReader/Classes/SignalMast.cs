
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class signalmast
{

    private string systemNameField;

    private string userNameField;

    private signalmastUnlit unlitField;

    private string classField;

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
    public signalmastUnlit unlit
    {
        get
        {
            return this.unlitField;
        }
        set
        {
            this.unlitField = value;
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

    public bool BlockJumped { get; set; }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class signalmastUnlit
{

    private string allowedField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string allowed
    {
        get
        {
            return this.allowedField;
        }
        set
        {
            this.allowedField = value;
        }
    }
}

