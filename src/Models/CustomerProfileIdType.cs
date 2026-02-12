using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "customerProfileIdType")]
internal sealed class CustomerProfileIdType
{
    [DataMember(Name = "customerProfileId")]
    public string CustomerProfileId { get; set; } = "";

    [DataMember(Name = "customerPaymentProfileId")]
    public string CustomerPaymentProfileId { get; set; } = "";

    [DataMember(Name = "customerAddressId")]
    public string CustomerAddressId { get; set; } = "";
}


