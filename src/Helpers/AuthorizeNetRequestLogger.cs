using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Handles logging for Authorize.Net requests and responses.
/// Automatically masks sensitive data including credit card numbers, CVV codes, 
/// API keys, and other personal information in logged content.
/// </summary>
internal sealed class AuthorizeNetRequestLogger
{
    private readonly StringBuilder _logBuilder = new StringBuilder();
    private readonly AuthorizeNetLogger _logger;

    public AuthorizeNetRequestLogger(bool debugEnabled)
    {
        _logger = new AuthorizeNetLogger(debugEnabled);
    }

    public AuthorizeNetRequestLogger(AuthorizeNetLogger logger)
    {
        _logger = logger;
    }

    public void InitializeLog(string apiUrl)
    {
        _logBuilder.AppendLine("Authorize.Net API Interaction Log:");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- BASE SERVICE URL ---");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine($"URL: {apiUrl}");
    }

    public void LogRequestInfo(string url, string method)
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- REQUEST ---");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine($"Method: {method}");
        _logBuilder.AppendLine($"URL: {url}");
    }

    public void LogRequestHeaders(string headers)
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- REQUEST HEADERS ---");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine(headers);
    }

    public void LogRequestData(string data)
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- REQUEST DATA ---");
        _logBuilder.AppendLine();

        // Mask sensitive data for security compliance
        string maskedData = SecurityHelper.MaskSensitiveDataInContent(data);
        _logBuilder.AppendLine(maskedData);
    }

    public void LogRequest(HttpRequestMessage request, string requestData)
    {
        LogRequestInfo(request.RequestUri?.ToString() ?? "", request.Method.ToString());
        LogRequestHeaders(request.Headers.ToString());
        LogRequestData(requestData);
    }

    public void LogResponse(HttpResponseMessage response, string responseText)
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- RESPONSE ---");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine($"HttpStatusCode: {response.StatusCode} ({response.ReasonPhrase})");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("Response Headers:");
        _logBuilder.AppendLine(response.Headers.ToString());
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("Response Text:");

        // Mask sensitive data in response for security compliance
        string maskedResponse = SecurityHelper.MaskSensitiveDataInContent(responseText);
        _logBuilder.AppendLine(maskedResponse);
    }

    public void LogError(string errorMessage)
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- HTTP ERROR ---");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine(errorMessage);
    }

    public void LogTaskCanceledException(TaskCanceledException taskException)
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- EXCEPTION CAUGHT (TaskCanceledException) ---");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine($"Message: {taskException.Message}");
        _logBuilder.AppendLine($"Stack Trace: {taskException.StackTrace}");
    }

    public void LogHttpRequestException(HttpRequestException requestException)
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- EXCEPTION CAUGHT (HttpRequestException) ---");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine($"Message: {requestException.Message}");
        _logBuilder.AppendLine($"Stack Trace: {requestException.StackTrace}");
    }

    public void LogUnhandledException(Exception exception)
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine($"--- UNEXPECTED EXCEPTION CAUGHT ({exception.GetType().Name}) ---");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine($"Message: {exception.Message}");
        _logBuilder.AppendLine($"Stack Trace: {exception.StackTrace}");
    }

    public void LogException(Exception ex)
    {
        if (ex is TaskCanceledException taskCanceled)
            LogTaskCanceledException(taskCanceled);
        else if (ex is HttpRequestException httpRequest)
            LogHttpRequestException(httpRequest);
        else
            LogUnhandledException(ex);
    }

    public void FinalizeLog()
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- END OF INTERACTION ---");

        string message = _logBuilder.ToString();
        _logger.LogInfo(message);
    }
}
