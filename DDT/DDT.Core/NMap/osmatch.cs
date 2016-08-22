/// <remarks/>
/// <remarks/>

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class osmatch
{

    private osmatchOsclass[] osclassField;

    private string nameField;

    private byte accuracyField;

    private uint lineField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("osclass")]
    public osmatchOsclass[] osclass
    {
        get
        {
            return this.osclassField;
        }
        set
        {
            this.osclassField = value;
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
    public byte accuracy
    {
        get
        {
            return this.accuracyField;
        }
        set
        {
            this.accuracyField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public uint line
    {
        get
        {
            return this.lineField;
        }
        set
        {
            this.lineField = value;
        }
    }
}
