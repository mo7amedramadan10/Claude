using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.Integrations;

/// <summary>
/// WCAG 2.x contrast-ratio math, used to enforce Part B's "validate resulting contrast ratios
/// server-side before accepting; if insufficient, adjust toward a compliant nearby value and
/// show the analyst what changed and why" rule for AI-suggested (and vision-extracted) visual
/// identities. Manual entry (path 1) and deterministic image-palette extraction (path 3a) are
/// NOT run through this — the analyst picked those values on purpose, so this only guards
/// values a model produced without seeing how they'll actually render.
/// </summary>
public static class ContrastValidator
{
    /// <summary>WCAG AA for normal-size text/UI foreground content — the bar an accent color
    /// used for button/link text on a white surface should clear.</summary>
    public const double MinContrastRatio = 4.5;

    private static readonly Regex HexPattern = new(@"^#?([0-9A-Fa-f]{6})$", RegexOptions.Compiled);

    public static bool IsValidHex(string? hex) => hex is not null && HexPattern.IsMatch(hex.Trim());

    /// <summary>Contrast ratio of <paramref name="hex"/> against a white (#FFFFFF) surface —
    /// the common case for an accent color used as button/link text or a border on this app's
    /// (and the generated viewer page's) light surfaces.</summary>
    public static double ContrastRatioAgainstWhite(string hex)
    {
        var (r, g, b) = ParseHex(hex);
        var luminance = RelativeLuminance(r, g, b);
        return ContrastRatio(1.0, luminance); // 1.0 = white's own relative luminance
    }

    /// <summary>
    /// Returns <paramref name="hex"/> unchanged if it already clears <paramref name="minRatio"/>
    /// against white; otherwise progressively darkens it (reducing HSL lightness in small steps)
    /// until it does, and reports that it changed. Never lightens — a color a model suggested is
    /// almost always too light against white, never too dark, and darkening preserves hue/
    /// character far better than an arbitrary hue shift would.
    /// </summary>
    public static (string Hex, bool Adjusted) EnsureContrastAgainstWhite(string hex, double minRatio = MinContrastRatio)
    {
        if (ContrastRatioAgainstWhite(hex) >= minRatio) return (Normalize(hex), false);

        var (h, s, l) = ToHsl(hex);
        for (var step = 1; step <= 25; step++)
        {
            var candidateL = Math.Max(0.0, l - step * 0.03);
            var candidate = FromHsl(h, s, candidateL);
            if (ContrastRatioAgainstWhite(candidate) >= minRatio) return (candidate, true);
            if (candidateL <= 0.0) break;
        }
        // Pathological input (e.g. already near-black hue with a weird saturation) — fall back
        // to pure black, which always clears any realistic minRatio against white.
        return ("#000000", true);
    }

    private static double ContrastRatio(double luminance1, double luminance2)
    {
        var lighter = Math.Max(luminance1, luminance2);
        var darker = Math.Min(luminance1, luminance2);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double RelativeLuminance(int r, int g, int b)
    {
        double Channel(int c)
        {
            var v = c / 255.0;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(r) + 0.7152 * Channel(g) + 0.0722 * Channel(b);
    }

    private static (int R, int G, int B) ParseHex(string hex)
    {
        var clean = hex.Trim().TrimStart('#');
        return (
            Convert.ToInt32(clean[..2], 16),
            Convert.ToInt32(clean.Substring(2, 2), 16),
            Convert.ToInt32(clean.Substring(4, 2), 16));
    }

    private static string Normalize(string hex) => "#" + hex.Trim().TrimStart('#').ToUpperInvariant();

    private static (double H, double S, double L) ToHsl(string hex)
    {
        var (r, g, b) = ParseHex(hex);
        double rf = r / 255.0, gf = g / 255.0, bf = b / 255.0;
        var max = Math.Max(rf, Math.Max(gf, bf));
        var min = Math.Min(rf, Math.Min(gf, bf));
        var l = (max + min) / 2.0;
        if (Math.Abs(max - min) < 1e-9) return (0, 0, l);

        var d = max - min;
        var s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
        double h;
        if (Math.Abs(max - rf) < 1e-9) h = (gf - bf) / d + (gf < bf ? 6 : 0);
        else if (Math.Abs(max - gf) < 1e-9) h = (bf - rf) / d + 2;
        else h = (rf - gf) / d + 4;
        h /= 6.0;
        return (h, s, l);
    }

    private static string FromHsl(double h, double s, double l)
    {
        double r, g, b;
        if (s <= 1e-9)
        {
            r = g = b = l;
        }
        else
        {
            double Hue2Rgb(double p, double q, double t)
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1.0 / 6) return p + (q - p) * 6 * t;
                if (t < 1.0 / 2) return q;
                if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
                return p;
            }
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            r = Hue2Rgb(p, q, h + 1.0 / 3);
            g = Hue2Rgb(p, q, h);
            b = Hue2Rgb(p, q, h - 1.0 / 3);
        }
        int R = (int)Math.Round(r * 255), G = (int)Math.Round(g * 255), B = (int)Math.Round(b * 255);
        return $"#{R:X2}{G:X2}{B:X2}";
    }
}
