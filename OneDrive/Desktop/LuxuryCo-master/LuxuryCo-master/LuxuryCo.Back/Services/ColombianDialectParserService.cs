using System;
using System.Text.RegularExpressions;

namespace LuxuryCo.Back.Services;

public class ColombianDialectParserService
{
    private static readonly Regex NumberRegex = new Regex(@"(\d+(?:\.\d+)?)\s*(lucas?|palos?|barras?|gambas?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TextNumberRegex = new Regex(@"\b(un|uno|dos|tres|cuatro|cinco|seis|siete|ocho|nueve|diez|veinte|cincuenta|cien)\s*(lucas?|palos?|barras?|gambas?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public DialectParseResult ParseAmount(string input)
    {
        var result = new DialectParseResult();
        if (string.IsNullOrWhiteSpace(input)) return result;

        var match = NumberRegex.Match(input);
        if (!match.Success)
        {
            match = TextNumberRegex.Match(input);
        }

        if (match.Success)
        {
            var rawValueStr = match.Groups[1].Value;
            var unit = match.Groups[2].Value.ToLower();

            double multiplier = 1.0;
            double confidence = 0.95;
            bool requiresClarification = false;

            double value = ParseWordToNumber(rawValueStr);

            if (unit.Contains("luca"))
            {
                // luca = 1,000 COP
                multiplier = 1000.0;
                confidence = 0.98;
            }
            else if (unit.Contains("gamba"))
            {
                // gamba = 100 COP (or in some contexts 100,000 COP)
                // We mark it as requiring clarification due to high ambiguity!
                multiplier = 100.0;
                confidence = 0.50; 
                requiresClarification = true;
            }
            else if (unit.Contains("palo"))
            {
                // palo = 1,000,000 COP
                multiplier = 1000000.0;
                confidence = 0.95;
            }
            else if (unit.Contains("barra"))
            {
                // barra = 1,000,000 COP (in large transactions like retail/admin) OR 1,000 COP in low end chat
                // High ambiguity, mark for confirmation!
                multiplier = 1000000.0;
                confidence = 0.61;
                requiresClarification = true;
            }

            result.Amount = (decimal)(value * multiplier);
            result.Confidence = confidence;
            result.RequiresClarification = requiresClarification;
            result.Parsed = true;
        }

        return result;
    }

    private double ParseWordToNumber(string word)
    {
        if (double.TryParse(word, out double val)) return val;

        return word.ToLower().Trim() switch
        {
            "un" or "uno" => 1,
            "dos" => 2,
            "tres" => 3,
            "cuatro" => 4,
            "cinco" => 5,
            "seis" => 6,
            "siete" => 7,
            "ocho" => 8,
            "nueve" => 9,
            "diez" => 10,
            "veinte" => 20,
            "cincuenta" => 50,
            "cien" => 100,
            _ => 1
        };
    }
}

public class DialectParseResult
{
    public decimal Amount { get; set; }
    public double Confidence { get; set; } = 0.0;
    public bool RequiresClarification { get; set; } = false;
    public bool Parsed { get; set; } = false;
}
