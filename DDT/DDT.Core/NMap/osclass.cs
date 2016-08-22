
/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class osmatchOsclass
{

    private string[] cpeField;

    private string typeField;

    private string vendorField;

    private string osfamilyField;

    private string osgenField;

    private byte accuracyField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("cpe")]
    public string[] cpe
    {
        get
        {
            return this.cpeField;
        }
        set
        {
            this.cpeField = value;
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
    public string vendor
    {
        get
        {
            return this.vendorField;
        }
        set
        {
            this.vendorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string osfamily
    {
        get
        {
            return this.osfamilyField;
        }
        set
        {
            this.osfamilyField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string osgen
    {
        get
        {
            return this.osgenField;
        }
        set
        {
            this.osgenField = value;
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
}

