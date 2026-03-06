using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class OrderType
{
    [DataMember(Name = "invoiceNumber", EmitDefaultValue = false)]
    public string? InvoiceNumber { get; set; }
}
