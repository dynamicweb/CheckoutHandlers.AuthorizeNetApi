using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// JSON converter for enums that supports EnumMember attributes for custom string values
/// </summary>
/// <typeparam name="T">The enum type to convert</typeparam>
internal sealed class DataContractEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();

            // First, try to find enum field by EnumMember value
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
                if (string.Equals(enumMember?.Value, stringValue, StringComparison.Ordinal))
                    return (T)field.GetValue(null)!;
            }

            // Fallback to standard enum parsing by name
            if (Enum.TryParse<T>(stringValue, true, out T result))
                return result;
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            // Handle numeric enum values
            if (reader.TryGetInt32(out int intValue) && Enum.IsDefined(typeof(T), intValue))
                return (T)Enum.ToObject(typeof(T), intValue);
        }

        // Return default value if parsing fails
        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Get the field info for the enum value
        FieldInfo? fieldInfo = typeof(T).GetField(value.ToString());
        EnumMemberAttribute? enumMember = fieldInfo?.GetCustomAttribute<EnumMemberAttribute>();

        // Use EnumMember.Value if available, otherwise use enum name
        if (!string.IsNullOrEmpty(enumMember?.Value))
            writer.WriteStringValue(enumMember.Value);
        else
            writer.WriteStringValue(value.ToString());
    }
}