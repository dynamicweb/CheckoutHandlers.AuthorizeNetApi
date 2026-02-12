using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "customerProfileExType")]
internal sealed class CustomerProfileExType
{
    [DataMember(Name = "merchantCustomerId")]
    public string MerchantCustomerId { get; set; } = "";

    [DataMember(Name = "customerProfileId")]
    public string CustomerProfileId { get; set; } = "";
}


