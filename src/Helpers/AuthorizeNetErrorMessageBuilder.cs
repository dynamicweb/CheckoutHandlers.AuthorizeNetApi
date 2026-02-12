using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using System.Linq;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

internal static class AuthorizeNetErrorMessageBuilder
{
    public static string Create(string prefix, CreateTransactionResponse? response)
    {
        if (response?.TransactionResponse is null)
            return prefix;

        string codeText = GetResponseTextByCode(response.TransactionResponse.ResponseCode);
        string errorCode = "";
        string errorText = "";

        if (response.TransactionResponse.Errors?.Any() is true)
        {
            errorCode = response.TransactionResponse.Errors.First().ErrorCode ?? "";
            errorText = response.TransactionResponse.Errors.First().ErrorText ?? "";
        }
        else if (response.Messages?.Message?.Any() is true)
        {
            errorCode = response.Messages.Message.First().Code ?? "";
            errorText = response.Messages.Message.First().Text ?? "";
        }

        return $"{prefix} {errorCode}/{codeText} - {errorText}".Trim();
    }

    public static string GetResponseTextByCode(string? responseCode)
    {
        if (!int.TryParse(responseCode, out int code))
            return "Unknown";

        return GetResponseTextByCode(code);
    }

    public static string GetResponseTextByCode(int responseCode) => responseCode switch
    {
        1 => "Approved",
        2 => "Declined",
        3 => "Error",
        4 => "Held for Review",
        _ => "Unknown"
    };
}
