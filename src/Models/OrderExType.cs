using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class OrderExType
{
    [DataMember(Name = "invoiceNumber", EmitDefaultValue = false)]
    public string InvoiceNumber { get; set; } = "";

    [DataMember(Name = "purchaseOrderNumber", EmitDefaultValue = false)]
    public string PurchaseOrderNumber { get; set; } = "";
}