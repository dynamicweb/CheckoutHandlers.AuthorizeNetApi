using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "extendedAmountType")]
internal sealed class ExtendedAmountType
{
    [DataMember(Name = "amount")]
    public double Amount { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; } = "";

    [DataMember(Name = "description")]
    public string Description { get; set; } = "";
}


