using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class PaymentMaskedType
{
    [DataMember(Name = "creditCard", EmitDefaultValue = false)]
    public CreditCardMaskedType? CreditCard { get; set; }

    [DataMember(Name = "bankAccount", EmitDefaultValue = false)]
    public BankAccountMaskedType? BankAccount { get; set; }

    [DataMember(Name = "tokenInformation", EmitDefaultValue = false)]
    public TokenMaskedType? TokenInformation { get; set; }
}
