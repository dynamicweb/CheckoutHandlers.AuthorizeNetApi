using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "returnedItem")]
internal sealed class ReturnedItem
{
    [DataMember(Name = "id")]
    public string Id { get; set; } = "";

    [DataMember(Name = "dateUTC")]
    public string DateUTC { get; set; } = "";

    [DataMember(Name = "dateLocal")]
    public string DateLocal { get; set; } = "";

    [DataMember(Name = "code")]
    public string Code { get; set; } = "";

    [DataMember(Name = "description")]
    public string Description { get; set; } = "";
}


