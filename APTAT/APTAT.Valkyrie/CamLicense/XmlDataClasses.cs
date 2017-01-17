
/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class ErrorResponse
{

    private ErrorResponseData dataField;

    private string fingerprintField;

    /// <remarks/>
    public ErrorResponseData Data
    {
        get
        {
            return this.dataField;
        }
        set
        {
            this.dataField = value;
        }
    }

    /// <remarks/>
    public string Fingerprint
    {
        get
        {
            return this.fingerprintField;
        }
        set
        {
            this.fingerprintField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ErrorResponseData
{

    private System.DateTime checkcodeField;

    private byte codeField;

    private string codeIdField;

    private string messageField;

    private string descriptionField;

    /// <remarks/>
    public System.DateTime Checkcode
    {
        get
        {
            return this.checkcodeField;
        }
        set
        {
            this.checkcodeField = value;
        }
    }

    /// <remarks/>
    public byte Code
    {
        get
        {
            return this.codeField;
        }
        set
        {
            this.codeField = value;
        }
    }

    /// <remarks/>
    public string CodeId
    {
        get
        {
            return this.codeIdField;
        }
        set
        {
            this.codeIdField = value;
        }
    }

    /// <remarks/>
    public string Message
    {
        get
        {
            return this.messageField;
        }
        set
        {
            this.messageField = value;
        }
    }

    /// <remarks/>
    public string Description
    {
        get
        {
            return this.descriptionField;
        }
        set
        {
            this.descriptionField = value;
        }
    }
}


/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public class GetFreeLicenseRequest
{

    private GetFreeLicenseRequestData dataField;

    private string fingerprintField;

    /// <remarks/>
    public GetFreeLicenseRequestData Data
    {
        get
        {
            return this.dataField;
        }
        set
        {
            this.dataField = value;
        }
    }

    /// <remarks/>
    public string Fingerprint
    {
        get
        {
            return this.fingerprintField;
        }
        set
        {
            this.fingerprintField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public class GetFreeLicenseRequestData
{

    private string webApiLoginField;

    private uint affiliateIDField;

    private string machineCodeField;

    private string emailField;

    private string pricingTermIdentifierField;

    private GetFreeLicenseRequestDataLicenseKeyInfo licenseKeyInfoField;

    /// <remarks/>
    public string WebApiLogin
    {
        get
        {
            return this.webApiLoginField;
        }
        set
        {
            this.webApiLoginField = value;
        }
    }

    /// <remarks/>
    public uint AffiliateID
    {
        get
        {
            return this.affiliateIDField;
        }
        set
        {
            this.affiliateIDField = value;
        }
    }

    /// <remarks/>
    public string MachineCode
    {
        get
        {
            return this.machineCodeField;
        }
        set
        {
            this.machineCodeField = value;
        }
    }

    /// <remarks/>
    public string Email
    {
        get
        {
            return this.emailField;
        }
        set
        {
            this.emailField = value;
        }
    }

    /// <remarks/>
    public string PricingTermIdentifier
    {
        get
        {
            return this.pricingTermIdentifierField;
        }
        set
        {
            this.pricingTermIdentifierField = value;
        }
    }

    /// <remarks/>
    public GetFreeLicenseRequestDataLicenseKeyInfo LicenseKeyInfo
    {
        get
        {
            return this.licenseKeyInfoField;
        }
        set
        {
            this.licenseKeyInfoField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GetFreeLicenseRequestDataLicenseKeyInfo
{

    private string versionField;

    private ushort osLangField;

    private ushort appLangField;

    private byte osIdField;

    private byte osPlatField;

    /// <remarks/>
    public string Version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }

    /// <remarks/>
    public ushort OsLang
    {
        get
        {
            return this.osLangField;
        }
        set
        {
            this.osLangField = value;
        }
    }

    /// <remarks/>
    public ushort AppLang
    {
        get
        {
            return this.appLangField;
        }
        set
        {
            this.appLangField = value;
        }
    }

    /// <remarks/>
    public byte OsId
    {
        get
        {
            return this.osIdField;
        }
        set
        {
            this.osIdField = value;
        }
    }

    /// <remarks/>
    public byte OsPlat
    {
        get
        {
            return this.osPlatField;
        }
        set
        {
            this.osPlatField = value;
        }
    }
}