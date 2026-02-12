using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "getCustomerProfileResponse")]
internal sealed class GetCustomerProfileResponse
{
    [DataMember(Name = "messages")]
    public MessagesType Messages { get; set; } = new();

    [DataMember(Name = "profile")]
    public CustomerProfileMaskedType Profile { get; set; } = new();
}


