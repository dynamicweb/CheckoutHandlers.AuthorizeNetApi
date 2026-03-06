using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CreateCustomerProfileResponse
{
    [DataMember(Name = "messages", EmitDefaultValue = false)]
    public MessagesType? Messages { get; set; }

    [DataMember(Name = "customerProfileId")]
    public string CustomerProfileId { get; set; } = "";

    [DataMember(Name = "customerPaymentProfileIdList", EmitDefaultValue = false)]
    public IEnumerable<string>? CustomerPaymentProfileIdList { get; set; }
}
