using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CustomerPaymentProfileMaskedType
{
    [DataMember(Name = "customerType", EmitDefaultValue = false)]
    public CustomerTypeEnum? CustomerType { get; set; }

    [DataMember(Name = "billTo", EmitDefaultValue = false)]
    public CustomerAddressType BillTo { get; set; } = new();

    [DataMember(Name = "customerProfileId", EmitDefaultValue = false)]
    public string CustomerProfileId { get; set; } = "";

    [DataMember(Name = "customerPaymentProfileId", EmitDefaultValue = false)]
    public string CustomerPaymentProfileId { get; set; } = "";

    [DataMember(Name = "defaultPaymentProfile", EmitDefaultValue = false)]
    public bool DefaultPaymentProfile { get; set; }

    [DataMember(Name = "payment", EmitDefaultValue = false)]
    public PaymentMaskedType Payment { get; set; } = new();

    [DataMember(Name = "driversLicense", EmitDefaultValue = false)]
    public DriversLicenseMaskedType DriversLicense { get; set; } = new();

    [DataMember(Name = "taxId", EmitDefaultValue = false)]
    public string TaxId { get; set; } = "";

    [DataMember(Name = "subscriptionIds", EmitDefaultValue = false)]
    public IEnumerable<string> SubscriptionIds { get; set; } = [];

    [DataMember(Name = "originalNetworkTransId", EmitDefaultValue = false)]
    public string OriginalNetworkTransId { get; set; } = "";

    [DataMember(Name = "originalAuthAmount", EmitDefaultValue = false)]
    public double OriginalAuthAmount { get; set; }
}