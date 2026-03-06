using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class DeleteCustomerPaymentProfileRequest
{
    [DataMember(Name = "merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [DataMember(Name = "refId", EmitDefaultValue = false)]
    public string? RefId { get; set; }

    [DataMember(Name = "customerProfileId", EmitDefaultValue = false)]
    public string? CustomerProfileId { get; set; }

    [DataMember(Name = "customerPaymentProfileId", EmitDefaultValue = false)]
    public string? CustomerPaymentProfileId { get; set; }
}
