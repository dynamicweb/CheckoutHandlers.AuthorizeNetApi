using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class DeleteCustomerPaymentProfileResponse
{
    [DataMember(Name = "messages", EmitDefaultValue = false)]
    public MessagesType? Messages { get; set; }
}
