using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class LineItem
{
    [DataMember(Name = "itemId", EmitDefaultValue = false)]
    public string? ItemId { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string? Name { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    public string? Description { get; set; }

    [DataMember(Name = "quantity", EmitDefaultValue = false)]
    public double Quantity { get; set; }

    [DataMember(Name = "unitPrice", EmitDefaultValue = false)]
    public double UnitPrice { get; set; }
}
