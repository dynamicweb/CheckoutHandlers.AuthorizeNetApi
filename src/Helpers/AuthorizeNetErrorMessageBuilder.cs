using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

internal static class AuthorizeNetErrorMessageBuilder
{
    public static string Create(string prefix, CreateTransactionResponse? response)
    {
        if (response?.TransactionResponse is null)
            return prefix;

        string codeText = GetResponseTextByCode(response.TransactionResponse.ResponseCode);
        var allErrors = new List<string>();

        if (response.TransactionResponse.Errors?.Any() is true)
        {
            foreach (Error error in response.TransactionResponse.Errors)
            {
                string formattedError = FormatErrorMessage(error.ErrorCode, error.ErrorText);
                if (!string.IsNullOrEmpty(formattedError))
                    allErrors.Add(formattedError);
            }
        }

        if (!allErrors.Any() && response.Messages?.Message?.Any() is true)
        {
            foreach (Message message in response.Messages.Message)
            {
                string formattedMessage = FormatErrorMessage(message.Code, message.Text);
                if (!string.IsNullOrEmpty(formattedMessage))
                    allErrors.Add(formattedMessage);
            }
        }

        if (allErrors.Any())
        {
            string combinedErrors = string.Join("; ", allErrors);
            return $"{prefix} {codeText}: {combinedErrors}".Trim();
        }

        return $"{prefix} {codeText}".Trim();
    }

    public static string GetResponseTextByCode(string? responseCode)
    {
        if (!int.TryParse(responseCode, out int code))
            return "Unknown";

        return GetResponseTextByCode(code);
    }

    public static string GetResponseTextByCode(int responseCode) => responseCode switch
    {
        ResponseCodes.Approved => "Approved",
        ResponseCodes.Declined => "Declined",
        ResponseCodes.Error => "Error",
        ResponseCodes.HeldForReview => "Held for Review",
        _ => "Unknown"
    };

    private static string FormatErrorMessage(string? code, string? text)
    {
        string errorCode = code ?? "";
        string errorText = text ?? "";

        if (string.IsNullOrEmpty(errorCode) && string.IsNullOrEmpty(errorText))
            return "";

        return $"{errorCode} - {errorText}".Trim(' ', '-');
    }
}
