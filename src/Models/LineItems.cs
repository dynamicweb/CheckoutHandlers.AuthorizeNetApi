using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class LineItems
{
    [DataMember(Name = "lineItem", EmitDefaultValue = false)]
    public IEnumerable<LineItem>? LineItem { get; set; }
}