using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "hostedPaymentSettings")]
internal sealed class HostedPaymentSettings
{
    [DataMember(Name = "setting")]
    public IEnumerable<Setting> Setting { get; set; } = [];
}


