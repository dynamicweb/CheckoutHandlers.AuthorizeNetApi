using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "createCustomerProfileResponse")]
internal sealed class CreateCustomerProfileResponse
{
    [DataMember(Name = "messages")]
    public MessagesType Messages { get; set; } = new();

    [DataMember(Name = "customerProfileId")]
    public string CustomerProfileId { get; set; } = "";

    [DataMember(Name = "customerPaymentProfileIdList")]
    public IEnumerable<string> CustomerPaymentProfileIdList { get; set; } = [];
}


