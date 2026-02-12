using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<MessageTypeEnum>))]
[DataContract(Name = "messageTypeEnum")]
internal enum MessageTypeEnum
{
    [EnumMember(Value = "Ok")]
    Ok,

    [EnumMember(Value = "Error")]
    Error
}


