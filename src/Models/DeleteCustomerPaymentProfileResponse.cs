using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "deleteCustomerPaymentProfileResponse")]
internal sealed class DeleteCustomerPaymentProfileResponse
{
    [DataMember(Name = "messages")]
    public MessagesType Messages { get; set; } = new();
}


