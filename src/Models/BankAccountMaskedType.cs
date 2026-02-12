using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "bankAccountMaskedType")]
internal sealed class BankAccountMaskedType
{
    [DataMember(Name = "accountType")]
    public BankAccountTypeEnum AccountType { get; set; }

    [DataMember(Name = "routingNumber")]
    public string RoutingNumber { get; set; } = "";

    [DataMember(Name = "accountNumber")]
    public string AccountNumber { get; set; } = "";

    [DataMember(Name = "nameOnAccount")]
    public string NameOnAccount { get; set; } = "";

    [DataMember(Name = "echeckType")]
    public EcheckTypeEnum EcheckType { get; set; }

    [DataMember(Name = "bankName")]
    public string BankName { get; set; } = "";
}


