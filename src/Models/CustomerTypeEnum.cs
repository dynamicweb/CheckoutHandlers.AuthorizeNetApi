using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<CustomerTypeEnum>))]
[DataContract(Name = "customerTypeEnum")]
internal enum CustomerTypeEnum
{
    [EnumMember(Value = "individual")]
    Individual,

    [EnumMember(Value = "business")]
    Business
}


