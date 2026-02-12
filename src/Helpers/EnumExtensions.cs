using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

internal static class EnumExtensions
{
    /// <summary>
    /// Gets EnumMember.Value or the enum name if the attribute is not found
    /// </summary>
    /// <param name="enumValue">The enum value</param>
    /// <returns>String representation for the API</returns>
    public static string ToEnumMemberValue(this Enum enumValue)
    {
        Type type = enumValue.GetType();
        FieldInfo? fieldInfo = type.GetField(enumValue.ToString());

        EnumMemberAttribute? enumMember = fieldInfo?.GetCustomAttribute<EnumMemberAttribute>();

        return enumMember?.Value ?? enumValue.ToString();
    }
}