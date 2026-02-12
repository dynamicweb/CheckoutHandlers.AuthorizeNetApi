using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "subscriptionPaymentType")]
internal sealed class SubscriptionPaymentType
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "payNum")]
    public int PayNum { get; set; }
}


