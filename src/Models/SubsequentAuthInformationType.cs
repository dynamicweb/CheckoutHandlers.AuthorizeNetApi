using System.Runtime.Serialization;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class SubsequentAuthInformationType
{
    [DataMember(Name = "originalNetworkTransId", EmitDefaultValue = false)]
    public string? OriginalNetworkTransId { get; set; }

    private double _originalAuthAmount;

    [DataMember(Name = "originalAuthAmount", EmitDefaultValue = false)]
    public double OriginalAuthAmount 
    { 
        get => _originalAuthAmount;
        set => _originalAuthAmount = AmountHelper.AdjustAmount(value);
    }

    [DataMember(Name = "reason", EmitDefaultValue = false)]
    public string? Reason { get; set; }
}
