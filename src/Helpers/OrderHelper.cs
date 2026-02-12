using Dynamicweb.Ecommerce.Orders;

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
        if (!string.IsNullOrEmpty(transactionId) && !transactionId.Equals(order.TransactionNumber, System.StringComparison.OrdinalIgnoreCase))
            order.TransactionNumber = transactionId;
    }
}
