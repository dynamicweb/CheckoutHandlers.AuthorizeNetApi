using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "lineItem")]
internal sealed class LineItem
{
    [DataMember(Name = "itemId")]
    public string ItemId { get; set; } = "";

    [DataMember(Name = "name")]
    public string Name { get; set; } = "";

    [DataMember(Name = "description")]
    public string Description { get; set; } = "";

    [DataMember(Name = "quantity")]
    public double Quantity { get; set; }

    [DataMember(Name = "unitPrice")]
    public double UnitPrice { get; set; }
}


