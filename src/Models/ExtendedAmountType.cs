using System.Runtime.Serialization;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class ExtendedAmountType
{
    private double _amount;

    [DataMember(Name = "amount", EmitDefaultValue = false)]
    public double Amount 
    { 
        get => _amount;
        set => _amount = AmountHelper.AdjustAmount(value);
    }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string? Name { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    public string? Description { get; set; }
}
