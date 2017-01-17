
/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class GetFreeLicenseResponse
{

    private GetFreeLicenseResponseData dataField;

    private string fingerprintField;

    /// <remarks/>
    public GetFreeLicenseResponseData Data
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
public partial class GetFreeLicenseResponseData
{

    private System.DateTime checkcodeField;

    private byte codeField;

    private string messageField;

    private string invoiceNumberField;

    private string orderNumberField;

    private decimal orderAmountField;

    private byte affiliateIDField;

    private uint userIdField;

    private string loginField;

    private string emailField;

    private string firstNameField;

    private string lastNameField;

    private string accountHashField;

    private string licenseKeyField;

    private object transactionsField;

    private string subscriptionIDField;

    private string machineCodeField;

    private string licenseStatusField;

    private bool isHaltedField;

    private bool automaticRenewalField;

    private string titleField;

    private string productIdentifierField;

    private string descriptionField;

    private string shortTitileField;

    private string shortTitleField;

    private GetFreeLicenseResponseDataAdditionalParams additionalParamsField;

    private GetFreeLicenseResponseDataPackageItems packageItemsField;

    private GetFreeLicenseResponseDataPackageAdditionalParams packageAdditionalParamsField;

    private GetFreeLicenseResponseDataExpirationDate expirationDateField;

    private string typeOfPurchaseField;

    private bool validField;

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
    public string InvoiceNumber
    {
        get
        {
            return this.invoiceNumberField;
        }
        set
        {
            this.invoiceNumberField = value;
        }
    }

    /// <remarks/>
    public string OrderNumber
    {
        get
        {
            return this.orderNumberField;
        }
        set
        {
            this.orderNumberField = value;
        }
    }

    /// <remarks/>
    public decimal OrderAmount
    {
        get
        {
            return this.orderAmountField;
        }
        set
        {
            this.orderAmountField = value;
        }
    }

    /// <remarks/>
    public byte AffiliateID
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
    public uint UserId
    {
        get
        {
            return this.userIdField;
        }
        set
        {
            this.userIdField = value;
        }
    }

    /// <remarks/>
    public string Login
    {
        get
        {
            return this.loginField;
        }
        set
        {
            this.loginField = value;
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
    public string FirstName
    {
        get
        {
            return this.firstNameField;
        }
        set
        {
            this.firstNameField = value;
        }
    }

    /// <remarks/>
    public string LastName
    {
        get
        {
            return this.lastNameField;
        }
        set
        {
            this.lastNameField = value;
        }
    }

    /// <remarks/>
    public string AccountHash
    {
        get
        {
            return this.accountHashField;
        }
        set
        {
            this.accountHashField = value;
        }
    }

    /// <remarks/>
    public string LicenseKey
    {
        get
        {
            return this.licenseKeyField;
        }
        set
        {
            this.licenseKeyField = value;
        }
    }

    /// <remarks/>
    public object Transactions
    {
        get
        {
            return this.transactionsField;
        }
        set
        {
            this.transactionsField = value;
        }
    }

    /// <remarks/>
    public string SubscriptionID
    {
        get
        {
            return this.subscriptionIDField;
        }
        set
        {
            this.subscriptionIDField = value;
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
    public string LicenseStatus
    {
        get
        {
            return this.licenseStatusField;
        }
        set
        {
            this.licenseStatusField = value;
        }
    }

    /// <remarks/>
    public bool IsHalted
    {
        get
        {
            return this.isHaltedField;
        }
        set
        {
            this.isHaltedField = value;
        }
    }

    /// <remarks/>
    public bool AutomaticRenewal
    {
        get
        {
            return this.automaticRenewalField;
        }
        set
        {
            this.automaticRenewalField = value;
        }
    }

    /// <remarks/>
    public string Title
    {
        get
        {
            return this.titleField;
        }
        set
        {
            this.titleField = value;
        }
    }

    /// <remarks/>
    public string ProductIdentifier
    {
        get
        {
            return this.productIdentifierField;
        }
        set
        {
            this.productIdentifierField = value;
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

    /// <remarks/>
    public string ShortTitile
    {
        get
        {
            return this.shortTitileField;
        }
        set
        {
            this.shortTitileField = value;
        }
    }

    /// <remarks/>
    public string ShortTitle
    {
        get
        {
            return this.shortTitleField;
        }
        set
        {
            this.shortTitleField = value;
        }
    }

    /// <remarks/>
    public GetFreeLicenseResponseDataAdditionalParams AdditionalParams
    {
        get
        {
            return this.additionalParamsField;
        }
        set
        {
            this.additionalParamsField = value;
        }
    }

    /// <remarks/>
    public GetFreeLicenseResponseDataPackageItems PackageItems
    {
        get
        {
            return this.packageItemsField;
        }
        set
        {
            this.packageItemsField = value;
        }
    }

    /// <remarks/>
    public GetFreeLicenseResponseDataPackageAdditionalParams PackageAdditionalParams
    {
        get
        {
            return this.packageAdditionalParamsField;
        }
        set
        {
            this.packageAdditionalParamsField = value;
        }
    }

    /// <remarks/>
    public GetFreeLicenseResponseDataExpirationDate ExpirationDate
    {
        get
        {
            return this.expirationDateField;
        }
        set
        {
            this.expirationDateField = value;
        }
    }

    /// <remarks/>
    public string TypeOfPurchase
    {
        get
        {
            return this.typeOfPurchaseField;
        }
        set
        {
            this.typeOfPurchaseField = value;
        }
    }

    /// <remarks/>
    public bool Valid
    {
        get
        {
            return this.validField;
        }
        set
        {
            this.validField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GetFreeLicenseResponseDataAdditionalParams
{

    private bool virusRemovalIncludedField;

    private byte licenseKeyActivationLimitCountField;

    private bool checkLicenseKeyActivationLimitField;

    /// <remarks/>
    public bool VirusRemovalIncluded
    {
        get
        {
            return this.virusRemovalIncludedField;
        }
        set
        {
            this.virusRemovalIncludedField = value;
        }
    }

    /// <remarks/>
    public byte LicenseKeyActivationLimitCount
    {
        get
        {
            return this.licenseKeyActivationLimitCountField;
        }
        set
        {
            this.licenseKeyActivationLimitCountField = value;
        }
    }

    /// <remarks/>
    public bool CheckLicenseKeyActivationLimit
    {
        get
        {
            return this.checkLicenseKeyActivationLimitField;
        }
        set
        {
            this.checkLicenseKeyActivationLimitField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GetFreeLicenseResponseDataPackageItems
{

    private string productIdentifierField;

    /// <remarks/>
    public string ProductIdentifier
    {
        get
        {
            return this.productIdentifierField;
        }
        set
        {
            this.productIdentifierField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GetFreeLicenseResponseDataPackageAdditionalParams
{

    private GetFreeLicenseResponseDataPackageAdditionalParamsPackageItem packageItemField;

    /// <remarks/>
    public GetFreeLicenseResponseDataPackageAdditionalParamsPackageItem PackageItem
    {
        get
        {
            return this.packageItemField;
        }
        set
        {
            this.packageItemField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GetFreeLicenseResponseDataPackageAdditionalParamsPackageItem
{

    private string productIdentifierField;

    private string productNameField;

    private string licenseKeyField;

    private GetFreeLicenseResponseDataPackageAdditionalParamsPackageItemExpirationDate expirationDateField;

    private GetFreeLicenseResponseDataPackageAdditionalParamsPackageItemAdditionalParams additionalParamsField;

    /// <remarks/>
    public string ProductIdentifier
    {
        get
        {
            return this.productIdentifierField;
        }
        set
        {
            this.productIdentifierField = value;
        }
    }

    /// <remarks/>
    public string ProductName
    {
        get
        {
            return this.productNameField;
        }
        set
        {
            this.productNameField = value;
        }
    }

    /// <remarks/>
    public string LicenseKey
    {
        get
        {
            return this.licenseKeyField;
        }
        set
        {
            this.licenseKeyField = value;
        }
    }

    /// <remarks/>
    public GetFreeLicenseResponseDataPackageAdditionalParamsPackageItemExpirationDate ExpirationDate
    {
        get
        {
            return this.expirationDateField;
        }
        set
        {
            this.expirationDateField = value;
        }
    }

    /// <remarks/>
    public GetFreeLicenseResponseDataPackageAdditionalParamsPackageItemAdditionalParams AdditionalParams
    {
        get
        {
            return this.additionalParamsField;
        }
        set
        {
            this.additionalParamsField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GetFreeLicenseResponseDataPackageAdditionalParamsPackageItemExpirationDate
{

    private System.DateTime dateFromField;

    private System.DateTime dateToField;

    /// <remarks/>
    public System.DateTime DateFrom
    {
        get
        {
            return this.dateFromField;
        }
        set
        {
            this.dateFromField = value;
        }
    }

    /// <remarks/>
    public System.DateTime DateTo
    {
        get
        {
            return this.dateToField;
        }
        set
        {
            this.dateToField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GetFreeLicenseResponseDataPackageAdditionalParamsPackageItemAdditionalParams
{

    private bool virusRemovalIncludedField;

    private bool checkLicenseKeyActivationLimitField;

    private byte licenseKeyActivationLimitCountField;

    private byte valkyrieEndpointsFromField;

    private byte valkyrieEndpointsToField;

    private bool checkMachineCodeLimitAsEndpointsField;

    /// <remarks/>
    public bool VirusRemovalIncluded
    {
        get
        {
            return this.virusRemovalIncludedField;
        }
        set
        {
            this.virusRemovalIncludedField = value;
        }
    }

    /// <remarks/>
    public bool CheckLicenseKeyActivationLimit
    {
        get
        {
            return this.checkLicenseKeyActivationLimitField;
        }
        set
        {
            this.checkLicenseKeyActivationLimitField = value;
        }
    }

    /// <remarks/>
    public byte LicenseKeyActivationLimitCount
    {
        get
        {
            return this.licenseKeyActivationLimitCountField;
        }
        set
        {
            this.licenseKeyActivationLimitCountField = value;
        }
    }

    /// <remarks/>
    public byte ValkyrieEndpointsFrom
    {
        get
        {
            return this.valkyrieEndpointsFromField;
        }
        set
        {
            this.valkyrieEndpointsFromField = value;
        }
    }

    /// <remarks/>
    public byte ValkyrieEndpointsTo
    {
        get
        {
            return this.valkyrieEndpointsToField;
        }
        set
        {
            this.valkyrieEndpointsToField = value;
        }
    }

    /// <remarks/>
    public bool CheckMachineCodeLimitAsEndpoints
    {
        get
        {
            return this.checkMachineCodeLimitAsEndpointsField;
        }
        set
        {
            this.checkMachineCodeLimitAsEndpointsField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GetFreeLicenseResponseDataExpirationDate
{

    private System.DateTime dateFromField;

    private System.DateTime dateToField;

    /// <remarks/>
    public System.DateTime DateFrom
    {
        get
        {
            return this.dateFromField;
        }
        set
        {
            this.dateFromField = value;
        }
    }

    /// <remarks/>
    public System.DateTime DateTo
    {
        get
        {
            return this.dateToField;
        }
        set
        {
            this.dateToField = value;
        }
    }
}

