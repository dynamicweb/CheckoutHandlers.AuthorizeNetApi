namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;

internal static class AuthorizeNetEndpoints
{
    private const string ProductionApi = "https://api.authorize.net/xml/v1/request.api";
    private const string SandboxApi = "https://apitest.authorize.net/xml/v1/request.api";

    private const string ProductionJs = "https://js.authorize.net/v1/Accept.js";
    private const string SandboxJs = "https://jstest.authorize.net/v1/Accept.js";

    private const string ProductionUi = "https://js.authorize.net/v3/AcceptUI.js";
    private const string SandboxUi = "https://jstest.authorize.net/v3/AcceptUI.js";

    private const string ProductionHostedForm = "https://accept.authorize.net/payment/payment";
    private const string SandboxHostedForm = "https://test.authorize.net/payment/payment";

    public static string SandboxApiUrl => SandboxApi;
    public static string ProductionApiUrl => ProductionApi;

    public static string GetApiEndpoint(bool isTestMode) => isTestMode
        ? SandboxApi
        : ProductionApi;

    public static string GetAcceptJsUrl(bool isTestMode) => isTestMode
        ? SandboxJs
        : ProductionJs;

    public static string GetAcceptUiUrl(bool isTestMode) => isTestMode
        ? SandboxUi
        : ProductionUi;

    public static string GetHostedFormUrl(bool isTestMode) => isTestMode
        ? SandboxHostedForm
        : ProductionHostedForm;
}
