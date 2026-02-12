using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "orderExType")]
internal sealed class OrderExType
{
    [DataMember(Name = "invoiceNumber")]
    public string InvoiceNumber { get; set; } = "";

    [DataMember(Name = "purchaseOrderNumber")]
    public string PurchaseOrderNumber { get; set; } = "";
}


