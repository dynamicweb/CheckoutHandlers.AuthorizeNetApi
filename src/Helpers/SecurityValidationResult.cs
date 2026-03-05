using System.Collections.Generic;
using System.Linq;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Result of security configuration validation
/// </summary>
internal class SecurityValidationResult
{
    public bool IsValid { get; init; }
    public List<string> SecurityIssues { get; init; } = [];
    public List<string> SecurityWarnings { get; init; } = [];

    public bool HasCriticalIssues => SecurityIssues.Any();
    public bool HasWarnings => SecurityWarnings.Any();
}