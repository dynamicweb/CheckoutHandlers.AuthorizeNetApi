using Dynamicweb.Ecommerce.Orders;
using System;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

internal static class OrderHelper
{
    public static string GetOrderError(Order order)
    {
        if (order is null)
            return "Order is not set";

        if (string.IsNullOrEmpty(order.Id))
            return "Order id is not set";

        if (string.IsNullOrEmpty(order.TransactionNumber))
            return "Transaction number is not set";

        return string.Empty;
    }

    public static void UpdateTransactionNumber(Order order, string? transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId) || transactionId.Equals("0", StringComparison.OrdinalIgnoreCase)
            || transactionId.Equals(order.TransactionNumber, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        order.TransactionNumber = transactionId;
    }

    public static double GetOrderAmount(Order order) => Ecommerce.Services.Currencies.Round(order.Currency, order.Price.Price + order.ExternalPaymentFee);
}
