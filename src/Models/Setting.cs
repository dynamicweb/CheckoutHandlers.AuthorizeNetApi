using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "setting")]
internal sealed class Setting
{
    [DataMember(Name = "settingName")]
    public string SettingName { get; set; } = "";

    [DataMember(Name = "settingValue")]
    public string SettingValue { get; set; } = "";
}


