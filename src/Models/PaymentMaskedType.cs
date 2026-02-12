using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "paymentMaskedType")]
internal sealed class PaymentMaskedType
{
    [DataMember(Name = "creditCard")]
    public CreditCardMaskedType CreditCard { get; set; } = new();

    [DataMember(Name = "bankAccount")]
    public BankAccountMaskedType BankAccount { get; set; } = new();

    [DataMember(Name = "tokenInformation")]
    public TokenMaskedType TokenInformation { get; set; } = new();
}


