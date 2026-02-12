using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "messagesType")]
internal sealed class MessagesType
{
    [DataMember(Name = "resultCode")]
    public string ResultCode { get; set; } = "";

    [DataMember(Name = "message")]
    public IEnumerable<Message> Message { get; set; } = [];
}


