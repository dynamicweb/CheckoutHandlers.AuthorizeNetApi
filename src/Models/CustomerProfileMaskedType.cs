using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "customerProfileMaskedType")]
internal sealed class CustomerProfileMaskedType
{
    [DataMember(Name = "customerProfileId")]
    public string CustomerProfileId { get; set; } = "";
}


