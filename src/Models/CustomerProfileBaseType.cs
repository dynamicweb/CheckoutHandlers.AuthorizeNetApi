using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "customerProfileBaseType")]
internal sealed class CustomerProfileBaseType
{
    [DataMember(Name = "merchantCustomerId")]
    public string MerchantCustomerId { get; set; } = "";
}


