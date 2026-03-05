using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class ReturnedItem
{
    [DataMember(Name = "id")]
    public string Id { get; set; } = "";

    [DataMember(Name = "dateUTC", EmitDefaultValue = false)]
    public string DateUTC { get; set; } = "";

    [DataMember(Name = "dateLocal", EmitDefaultValue = false)]
    public string DateLocal { get; set; } = "";

    [DataMember(Name = "code", EmitDefaultValue = false)]
    public string Code { get; set; } = "";

    [DataMember(Name = "description", EmitDefaultValue = false)]
    public string Description { get; set; } = "";
}