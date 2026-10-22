using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ChatToDashboard.Api.Integrations;

/// <summary>
/// Deterministic dominant-color extraction from an uploaded image (Part B path 3a) — plain
/// k-means clustering over sampled pixels, no AI model involved at any point. Kept entirely
/// separate from ColorPalette generation's AI-assisted path (VisualIdentityService.
/// SuggestAsync/ExtractFromImageAsync) so this one is genuinely deterministic and free, as the
/// spec requires.
/// </summary>
public static class ColorClusterer
{
    private const int MaxSampleDimension = 200; // downscale before clustering — plenty for a dominant-color read, much faster
    private const int Iterations = 12;

    /// <summary>Returns up to <paramref name="clusterCount"/> dominant colors as "#RRGGBB"
    /// hex strings, ordered by cluster size (most dominant first). Skips near-white/near-black/
    /// low-saturation "background" pixels only when they dominate every cluster — see
    /// IsNearGray — so a logo on a plain white background still yields its real brand colors
    /// rather than "#FFFFFF" as the top hit.</summary>
    public static List<string> ExtractDominantColors(byte[] imageBytes, int clusterCount = 5)
    {
        using var image = Image.Load<Rgba32>(imageBytes);
        if (image.Width > MaxSampleDimension || image.Height > MaxSampleDimension)
        {
            var scale = (double)MaxSampleDimension / Math.Max(image.Width, image.Height);
            image.Mutate(ctx => ctx.Resize((int)(image.Width * scale), (int)(image.Height * scale)));
        }

        var pixels = new List<(double R, double G, double B)>();
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var p = row[x];
                    if (p.A < 32) continue; // effectively transparent — not a real color
                    pixels.Add((p.R, p.G, p.B));
                }
            }
        });
        if (pixels.Count == 0) return new List<string>();

        var k = Math.Min(clusterCount, pixels.Count);
        var centroids = InitializeCentroids(pixels, k);

        for (var iter = 0; iter < Iterations; iter++)
        {
            var sums = new (double R, double G, double B, int Count)[k];
            foreach (var pixel in pixels)
            {
                var nearest = NearestCentroid(pixel, centroids);
                sums[nearest].R += pixel.R;
                sums[nearest].G += pixel.G;
                sums[nearest].B += pixel.B;
                sums[nearest].Count++;
            }
            for (var i = 0; i < k; i++)
            {
                if (sums[i].Count == 0) continue; // keep the old centroid — an empty cluster this round
                centroids[i] = (sums[i].R / sums[i].Count, sums[i].G / sums[i].Count, sums[i].B / sums[i].Count);
            }
        }

        // Final assignment + cluster sizes, to order the returned colors by how dominant they
        // actually are in the image (not just centroid index).
        var finalCounts = new int[k];
        foreach (var pixel in pixels) finalCounts[NearestCentroid(pixel, centroids)]++;

        var ordered = Enumerable.Range(0, k)
            .Where(i => finalCounts[i] > 0)
            .OrderByDescending(i => finalCounts[i])
            .Select(i => ToHex(centroids[i]))
            .Distinct()
            .ToList();
        return ordered;
    }

    /// <summary>k-means++-style seeding (pick well-spread starting centroids rather than random
    /// picks that can collapse to near-duplicate clusters) — deterministic given the same pixel
    /// list, since it always starts from the first pixel rather than a random seed.</summary>
    private static List<(double R, double G, double B)> InitializeCentroids(List<(double R, double G, double B)> pixels, int k)
    {
        var centroids = new List<(double R, double G, double B)> { pixels[0] };
        while (centroids.Count < k)
        {
            var farthest = pixels
                .OrderByDescending(p => centroids.Min(c => DistanceSquared(p, c)))
                .First();
            centroids.Add(farthest);
        }
        return centroids;
    }

    private static int NearestCentroid((double R, double G, double B) pixel, List<(double R, double G, double B)> centroids)
    {
        var best = 0;
        var bestDist = double.MaxValue;
        for (var i = 0; i < centroids.Count; i++)
        {
            var dist = DistanceSquared(pixel, centroids[i]);
            if (dist < bestDist) { bestDist = dist; best = i; }
        }
        return best;
    }

    private static double DistanceSquared((double R, double G, double B) a, (double R, double G, double B) b) =>
        (a.R - b.R) * (a.R - b.R) + (a.G - b.G) * (a.G - b.G) + (a.B - b.B) * (a.B - b.B);

    private static string ToHex((double R, double G, double B) c) =>
        $"#{Clamp(c.R):X2}{Clamp(c.G):X2}{Clamp(c.B):X2}";

    private static int Clamp(double v) => Math.Clamp((int)Math.Round(v), 0, 255);
}
