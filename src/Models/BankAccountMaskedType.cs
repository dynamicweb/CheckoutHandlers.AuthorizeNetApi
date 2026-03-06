using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class BankAccountMaskedType
{
    [DataMember(Name = "accountType", EmitDefaultValue = false)]
    public BankAccountTypeEnum? AccountType { get; set; }

    [DataMember(Name = "echeckType", EmitDefaultValue = false)]
    public EcheckTypeEnum? EcheckType { get; set; }

    [DataMember(Name = "routingNumber", EmitDefaultValue = false)]
    public string? RoutingNumber { get; set; }

    [DataMember(Name = "accountNumber", EmitDefaultValue = false)]
    public string? AccountNumber { get; set; }

    [DataMember(Name = "nameOnAccount", EmitDefaultValue = false)]
    public string? NameOnAccount { get; set; }

    [DataMember(Name = "bankName", EmitDefaultValue = false)]
    public string? BankName { get; set; }
}
