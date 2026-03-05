using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CustomerProfileType
{
    [DataMember(Name = "merchantCustomerId")]
    public string MerchantCustomerId { get; set; } = "";
}