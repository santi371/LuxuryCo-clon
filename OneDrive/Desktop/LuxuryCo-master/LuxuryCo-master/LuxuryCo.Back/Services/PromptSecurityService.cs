using System;
using System.Text.RegularExpressions;

namespace LuxuryCo.Back.Services;

public class PromptSecurityService
{
    private static readonly Regex InjectionPattern = new Regex(
        @"\b(ignore|bypass|override|delete|reset|reveal|system prompt|developer mode|dan mode|jailbreak|sudo|bypass security|drop table|truncate|delete database)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool IsPromptSafe(string input, out string reason)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            reason = "Empty input";
            return true;
        }

        // Check for common prompt injection/jailbreak keyphrases
        if (InjectionPattern.IsMatch(input))
        {
            reason = "Potential prompt injection or security override attempt detected.";
            return false;
        }

        // Look for typical scripting/HTML/SQL tags
        if (input.Contains("<script") || input.Contains("javascript:") || input.Contains("UNION SELECT"))
        {
            reason = "Malicious payload patterns (Script/SQL injection) detected.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public string SanitizedInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        
        // Remove HTML/Script tags to prevent XSS
        var temp = Regex.Replace(input, @"<[^>]*>", string.Empty);
        
        // Strip out carriage returns and potential command separators
        return temp.Replace("\r", " ").Replace("\n", " ").Trim();
    }

    public string SanitizeOutput(string output)
    {
        if (string.IsNullOrEmpty(output)) return string.Empty;
        
        // Ensure no malicious HTML/JS is executed if output contains it
        return Regex.Replace(output, @"<script[^>]*>([\s\S]*?)<\/script>", "[Script Blocked]", RegexOptions.IgnoreCase);
    }
}
