using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "userField")]
internal sealed class UserField
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = "";

    [DataMember(Name = "value")]
    public string Value { get; set; } = "";
}


