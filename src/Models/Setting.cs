using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class Setting
{
    [DataMember(Name = "settingName", EmitDefaultValue = false)]
    public string? SettingName { get; set; }

    [DataMember(Name = "settingValue", EmitDefaultValue = false)]
    public string? SettingValue { get; set; }
}
