using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class GetCustomerProfileResponse
{
    [DataMember(Name = "messages", EmitDefaultValue = false)]
    public MessagesType? Messages { get; set; }

    [DataMember(Name = "profile", EmitDefaultValue = false)]
    public CustomerProfileMaskedType? Profile { get; set; }
}
