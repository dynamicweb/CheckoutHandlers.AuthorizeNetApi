using System;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Helper class for adjusting amounts according to Authorize.Net API requirements
/// </summary>
internal static class AmountHelper
{
    /// <summary>
    /// Maximum decimal places allowed by Authorize.Net API
    /// </summary>
    private const int MaxDecimalPlaces = 15;

    /// <summary>
    /// Adjusts amount to comply with Authorize.Net API requirements:
    /// - Decimal, up to 15 digits with a decimal point
    /// </summary>
    /// <param name="amount">Amount to adjust</param>
    /// <returns>Adjusted amount that complies with API limits</returns>
    public static double AdjustAmount(double amount)
    {
        return Math.Round(amount, MaxDecimalPlaces, MidpointRounding.AwayFromZero);
    }
}