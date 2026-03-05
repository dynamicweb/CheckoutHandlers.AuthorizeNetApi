using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class DeleteCustomerPaymentProfileRequestWrapper
{
    [DataMember(Name = "deleteCustomerPaymentProfileRequest", EmitDefaultValue = false)]
    public DeleteCustomerPaymentProfileRequest DeleteCustomerPaymentProfileRequest { get; set; } = new();
}