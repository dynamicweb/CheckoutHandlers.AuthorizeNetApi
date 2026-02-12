using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

internal static class HmacValidator
{
    public static bool IsValid(string? signatureKey, string? notificationBody, string? incomingHmac)
    {
        if (string.IsNullOrEmpty(signatureKey) || string.IsNullOrEmpty(notificationBody) || string.IsNullOrEmpty(incomingHmac))
            return false;

        string token = GetSHAToken(signatureKey, notificationBody);

        return string.Equals(token, incomingHmac, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetSHAToken(string signatureKey, string notificationBody)
    {
        try
        {
            byte[] key = Encoding.UTF8.GetBytes(signatureKey);
            using (var hmac = new HMACSHA512(key))
            {
                byte[] hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(notificationBody));

                return string.Concat(hashArray.Select(b => b.ToString("X2")));
            }
        }
        catch
        {
            return string.Empty;
        }
    }
}
