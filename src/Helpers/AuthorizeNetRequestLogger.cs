using Dynamicweb.Logging;
using System;
using System.Net.Http;
using System.Text;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

internal sealed class AuthorizeNetRequestLogger
{
    private readonly StringBuilder _logBuilder = new StringBuilder();
    private readonly bool _debugEnabled;
    private readonly ILogger _logger;

    public AuthorizeNetRequestLogger(bool debugEnabled)
    {
        _debugEnabled = debugEnabled;

        _logger = LogManager.Current.GetLogger(typeof(AuthorizeNetRequestLogger).FullName ?? "");
        _logBuilder.AppendLine("Authorize.Net API Interaction Log:");
        _logBuilder.AppendLine();
    }

    public void LogRequest(HttpRequestMessage request, string requestData)
    {
        _logBuilder.AppendLine("--- REQUEST ---");
        _logBuilder.AppendLine($"Method: {request.Method}");
        _logBuilder.AppendLine($"URL: {request.RequestUri}");
        _logBuilder.AppendLine("Headers:");
        _logBuilder.AppendLine(request.Headers.ToString());
        _logBuilder.AppendLine("Request Data:");
        _logBuilder.AppendLine(requestData);
    }

    public void LogResponse(HttpResponseMessage response, string responseText)
    {
        _logBuilder.AppendLine("--- RESPONSE ---");
        _logBuilder.AppendLine($"Status: {response.StatusCode} ({response.ReasonPhrase})");
        _logBuilder.AppendLine("Headers:");
        _logBuilder.AppendLine(response.Headers.ToString());
        _logBuilder.AppendLine("Response Data:");
        _logBuilder.AppendLine(responseText);
    }

    public void LogException(Exception ex)
    {
        _logBuilder.AppendLine("--- EXCEPTION ---");
        _logBuilder.AppendLine($"Type: {ex.GetType().Name}");
        _logBuilder.AppendLine($"Message: {ex.Message}");
        _logBuilder.AppendLine($"Stack Trace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            _logBuilder.AppendLine("Inner Exception:");
            _logBuilder.AppendLine($"Type: {ex.InnerException.GetType().Name}");
            _logBuilder.AppendLine($"Message: {ex.InnerException.Message}");
            _logBuilder.AppendLine($"Stack Trace: {ex.InnerException.StackTrace}");
        }
    }

    public void LogError(string message)
    {
        _logBuilder.AppendLine("--- ERROR ---");
        _logBuilder.AppendLine(message);
    }

    public void FinalizeLog()
    {
        _logBuilder.AppendLine("--- END OF INTERACTION ---");
        var message = _logBuilder.ToString();

        if (_debugEnabled)
            _logger.Log(LogLevel.Debug, message);
        else
        {
            // Log non-debug messages at a lower level or only if there was an error
            _logger.Log(LogLevel.Information, message);
        }
    }
}
