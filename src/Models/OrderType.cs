using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "orderType")]
internal sealed class OrderType
{
    [DataMember(Name = "invoiceNumber")]
    public string InvoiceNumber { get; set; } = "";
}


