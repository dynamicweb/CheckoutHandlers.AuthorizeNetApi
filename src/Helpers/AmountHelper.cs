using Dynamicweb.Ecommerce.Prices;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Helper class for adjusting amounts according to Authorize.Net API requirements
/// </summary>
internal static class AmountHelper
{
    /// <summary>
    /// Adjusts amount to comply with Authorize.Net API requirements and avoid potential issues with floating point precision.
    /// </summary>
    /// <param name="amount">Amount to adjust</param>
    public static double AdjustAmount(double amount) => PriceHelper.Round(4, amount);
}