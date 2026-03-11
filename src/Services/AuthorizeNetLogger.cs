using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Logging;
using System;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;

/// <summary>
/// Simplified logger for Authorize.Net operations
/// </summary>
internal sealed class AuthorizeNetLogger
{
    private readonly ILogger _logger;
    private readonly bool _debugLogging;
    private readonly Order? _order;

    /// <summary>
    /// Initializes a new instance of AuthorizeNetLogger
    /// </summary>
    /// <param name="debugLogging">Whether debug logging is enabled</param>
    public AuthorizeNetLogger(bool debugLogging)
    {
        _debugLogging = debugLogging;
        _logger = LogManager.Current.GetLogger($"/eCom/CheckoutHandler/{typeof(AuthorizeNetCheckoutHandler).FullName}");
    }

    public AuthorizeNetLogger(bool debugLogging, Order? order)
        : this(debugLogging)
    {
        _order = order;
    }

    /// <summary>
    /// Logs information message (only when debug logging is enabled)
    /// </summary>
    /// <param name="message">Message to log</param>
    public void LogInfo(string message)
    {
        if (!IsLogNeeded(message))
            return;

        _logger.Info(message);
        LogDebugOrderInfo(message);
    }

    /// <summary>
    /// Logs formatted information message (only when debug logging is enabled)
    /// </summary>
    /// <param name="messageFormat">Message format string</param>
    /// <param name="args">Format arguments</param>
    public void LogInfo(string messageFormat, params object[] args)
    {
        string message = string.Format(messageFormat, args);
        if (!IsLogNeeded(message))
            return;

        _logger.Info(message);
        LogDebugOrderInfo(message);
    }

    /// <summary>
    /// Logs error message (always logged, regardless of debug setting)
    /// </summary>
    /// <param name="message">Message to log</param>
    public void LogError(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        _logger.Error(message);
        LogDebugOrderInfo(message);
    }

    /// <summary>
    /// Logs formatted error message (always logged, regardless of debug setting)
    /// </summary>
    /// <param name="messageFormat">Message format string</param>
    /// <param name="args">Format arguments</param>
    public void LogError(string messageFormat, params object[] args)
    {
        string message = string.Format(messageFormat, args);
        if (string.IsNullOrWhiteSpace(message))
            return;

        _logger.Error(message);
        LogDebugOrderInfo(message);
    }

    /// <summary>
    /// Logs error with exception (always logged, regardless of debug setting)
    /// </summary>
    /// <param name="message">Message to log</param>
    /// <param name="exception">Exception to log</param>
    public void LogError(string message, Exception exception)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        _logger.Error(message, exception);
        LogDebugOrderInfo($"{message}: {exception.Message}");
    }

    /// <summary>
    /// Logs error with exception and formatting (always logged, regardless of debug setting)
    /// </summary>
    /// <param name="exception">Exception to log</param>
    /// <param name="messageFormat">Message format string</param>
    /// <param name="args">Format arguments</param>
    public void LogError(Exception exception, string messageFormat, params object[] args)
    {
        string message = string.Format(messageFormat, args);
        if (string.IsNullOrWhiteSpace(message))
            return;

        _logger.Error(message, exception);
        LogDebugOrderInfo($"{message}: {exception.Message}");
    }

    private bool IsLogNeeded(string message)
    {
        if (_debugLogging is false)
            return false;

        if (string.IsNullOrWhiteSpace(message))
            return false;

        return true;
    }

    private void LogDebugOrderInfo(string message)
    {
        if (_order is not null)
            Ecommerce.Services.OrderDebuggingInfos.Save(_order, message, "Authorize.Net");
    }
}