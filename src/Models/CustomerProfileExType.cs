using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CustomerProfileExType
{
    [DataMember(Name = "merchantCustomerId", EmitDefaultValue = false)]
    public string MerchantCustomerId { get; set; } = "";

    [DataMember(Name = "customerProfileId", EmitDefaultValue = false)]
    public string CustomerProfileId { get; set; } = "";
}