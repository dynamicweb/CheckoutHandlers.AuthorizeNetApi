using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "lineItems")]
internal sealed class LineItems
{
    [DataMember(Name = "lineItem")]
    public IEnumerable<LineItem> LineItem { get; set; } = [];
}


