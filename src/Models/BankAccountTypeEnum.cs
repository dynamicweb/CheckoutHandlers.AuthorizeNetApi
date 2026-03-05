using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<BankAccountTypeEnum>))]
[DataContract]
internal enum BankAccountTypeEnum
{
    [EnumMember(Value = "checking")]
    Checking,

    [EnumMember(Value = "savings")]
    Savings,

    [EnumMember(Value = "businessChecking")]
    BusinessChecking
}