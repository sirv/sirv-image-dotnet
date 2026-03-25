using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sirv;

/// <summary>
/// Options for generating srcset strings.
/// </summary>
public class SrcSetOptions
{
    /// <summary>Explicit list of widths to generate.</summary>
    public int[]? Widths { get; set; }

    /// <summary>Minimum width for auto-generation.</summary>
    public int? MinWidth { get; set; }

    /// <summary>Maximum width for auto-generation.</summary>
    public int? MaxWidth { get; set; }

    /// <summary>Tolerance for auto-generating widths (0-1). Default 0.15.</summary>
    public double? Tolerance { get; set; }

    /// <summary>DPR values (e.g. [1, 2, 3]).</summary>
    public double[]? DevicePixelRatios { get; set; }
}

/// <summary>
/// Options for generating an &lt;img&gt; tag.
/// </summary>
public class ImageOptions
{
    /// <summary>Transformation parameters for the URL.</summary>
    public Dictionary<string, object>? Transform { get; set; }

    /// <summary>Viewer options for the data-options attribute.</summary>
    public Dictionary<string, object>? Viewer { get; set; }

    /// <summary>Alt text for the image.</summary>
    public string? Alt { get; set; }

    /// <summary>Additional CSS class(es).</summary>
    public string? ClassName { get; set; }
}

/// <summary>
/// Options for generating viewer div tags (zoom, spin, video, model).
/// </summary>
public class ViewerDivOptions
{
    /// <summary>Transformation parameters for the URL.</summary>
    public Dictionary<string, object>? Transform { get; set; }

    /// <summary>Viewer options for the data-options attribute.</summary>
    public Dictionary<string, object>? Viewer { get; set; }

    /// <summary>Additional CSS class(es).</summary>
    public string? ClassName { get; set; }
}

/// <summary>
/// A single item in a gallery.
/// </summary>
public class GalleryItem
{
    /// <summary>Asset path (e.g. "/product.spin").</summary>
    public string Src { get; set; } = "";

    /// <summary>Asset type override (e.g. "zoom", "spin").</summary>
    public string? Type { get; set; }

    /// <summary>Per-item transformation parameters.</summary>
    public Dictionary<string, object>? Transform { get; set; }

    /// <summary>Per-item viewer options.</summary>
    public Dictionary<string, object>? Viewer { get; set; }
}

/// <summary>
/// Options for generating a gallery container.
/// </summary>
public class GalleryOptions
{
    /// <summary>Gallery-level viewer options.</summary>
    public Dictionary<string, object>? Viewer { get; set; }

    /// <summary>Additional CSS class(es) for the gallery container.</summary>
    public string? ClassName { get; set; }
}

/// <summary>
/// Options for generating a Sirv JS script tag.
/// </summary>
public class ScriptTagOptions
{
    /// <summary>Specific modules to load (e.g. ["spin", "zoom"]).</summary>
    public string[]? Modules { get; set; }

    /// <summary>Whether to add async attribute. Default true.</summary>
    public bool Async { get; set; } = true;
}

/// <summary>
/// SDK for building Sirv URLs and HTML tags for images, spins, videos, 3D models, and galleries.
/// </summary>
/// <example>
/// <code>
/// var sirv = new SirvClient("demo.sirv.com");
/// var url = sirv.Url("/image.jpg", new Dictionary&lt;string, object&gt; { { "w", 300 }, { "format", "webp" } });
/// var html = sirv.Image("/photo.jpg", new ImageOptions { Alt = "A photo" });
/// </code>
/// </example>
public class SirvClient
{
    private readonly string _domain;
    private readonly Dictionary<string, object> _defaults;

    /// <summary>
    /// Create a SirvClient instance.
    /// </summary>
    /// <param name="domain">Sirv domain (e.g. "demo.sirv.com").</param>
    /// <param name="defaults">Default query parameters merged into every URL.</param>
    public SirvClient(string domain, Dictionary<string, object>? defaults = null)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("domain is required", nameof(domain));

        _domain = domain.TrimEnd('/');
        _defaults = defaults ?? new Dictionary<string, object>();
    }

    // ──────────────────────────────────────────────
    //  Internal helpers
    // ──────────────────────────────────────────────

    /// <summary>
    /// Flatten a nested dictionary into dot-notation key-value pairs.
    /// </summary>
    private List<KeyValuePair<string, string>> Flatten(Dictionary<string, object> obj, string prefix = "")
    {
        var entries = new List<KeyValuePair<string, string>>();

        foreach (var kvp in obj)
        {
            var fullKey = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

            if (kvp.Value is Dictionary<string, object> nested)
            {
                entries.AddRange(Flatten(nested, fullKey));
            }
            else if (kvp.Value != null)
            {
                var strValue = kvp.Value is bool boolVal
                    ? (boolVal ? "true" : "false")
                    : kvp.Value.ToString()!;
                entries.Add(new KeyValuePair<string, string>(fullKey, strValue));
            }
        }

        return entries;
    }

    /// <summary>
    /// Build a query string from merged defaults + params.
    /// </summary>
    private string BuildQuery(Dictionary<string, object>? parameters = null)
    {
        var merged = new Dictionary<string, object>(_defaults);
        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                merged[kvp.Key] = kvp.Value;
            }
        }

        var entries = Flatten(merged);
        if (entries.Count == 0) return "";

        var sb = new StringBuilder("?");
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0) sb.Append('&');
            sb.Append(Uri.EscapeDataString(entries[i].Key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(entries[i].Value));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Serialize viewer options to semicolon-separated format for data-options.
    /// </summary>
    private static string SerializeViewerOptions(Dictionary<string, object> opts)
    {
        var sb = new StringBuilder();
        bool first = true;
        foreach (var kvp in opts)
        {
            if (!first) sb.Append(';');
            sb.Append(kvp.Key);
            sb.Append(':');
            if (kvp.Value is bool boolVal)
                sb.Append(boolVal ? "true" : "false");
            else
                sb.Append(kvp.Value.ToString()!);
            first = false;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Escape HTML attribute values.
    /// </summary>
    private static string EscapeAttr(string str)
    {
        return str
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    /// <summary>
    /// Calculate quality for a given DPR. Higher DPR uses lower quality since pixels are smaller.
    /// </summary>
    private static int DprQuality(double baseQ, double dpr)
    {
        if (dpr <= 1) return (int)baseQ;
        return (int)Math.Round(baseQ * Math.Pow(0.75, dpr - 1));
    }

    /// <summary>
    /// Generate widths between min and max using a tolerance step.
    /// </summary>
    private static List<int> GenerateWidths(int min, int max, double tolerance)
    {
        var widths = new List<int>();
        double current = min;
        while (current < max)
        {
            widths.Add((int)Math.Round(current));
            current *= 1 + tolerance * 2;
        }
        widths.Add((int)Math.Round((double)max));
        return widths;
    }

    // ──────────────────────────────────────────────
    //  Public API
    // ──────────────────────────────────────────────

    /// <summary>
    /// Build a full Sirv URL.
    /// </summary>
    /// <param name="path">Asset path (e.g. "/image.jpg").</param>
    /// <param name="parameters">Transformation parameters (nested dictionaries are flattened to dot-notation).</param>
    /// <returns>Full URL string.</returns>
    public string Url(string path, Dictionary<string, object>? parameters = null)
    {
        var normalizedPath = path.StartsWith("/") ? path : "/" + path;
        return $"https://{_domain}{normalizedPath}{BuildQuery(parameters)}";
    }

    /// <summary>
    /// Generate a srcset string for responsive images.
    /// </summary>
    /// <param name="path">Image path.</param>
    /// <param name="parameters">Transformation parameters.</param>
    /// <param name="options">Srcset generation options.</param>
    /// <returns>srcset string.</returns>
    public string SrcSet(string path, Dictionary<string, object>? parameters = null, SrcSetOptions? options = null)
    {
        parameters ??= new Dictionary<string, object>();
        options ??= new SrcSetOptions();

        if (options.Widths != null)
        {
            return string.Join(", ", options.Widths.Select(w =>
            {
                var p = new Dictionary<string, object>(parameters) { ["w"] = w };
                return $"{Url(path, p)} {w}w";
            }));
        }

        if (options.MinWidth.HasValue && options.MaxWidth.HasValue)
        {
            var tolerance = options.Tolerance ?? 0.15;
            var widths = GenerateWidths(options.MinWidth.Value, options.MaxWidth.Value, tolerance);
            return string.Join(", ", widths.Select(w =>
            {
                var p = new Dictionary<string, object>(parameters) { ["w"] = w };
                return $"{Url(path, p)} {w}w";
            }));
        }

        if (options.DevicePixelRatios != null)
        {
            double baseQ = 80;
            if (parameters.ContainsKey("q"))
                baseQ = Convert.ToDouble(parameters["q"]);
            else if (_defaults.ContainsKey("q"))
                baseQ = Convert.ToDouble(_defaults["q"]);

            return string.Join(", ", options.DevicePixelRatios.Select(dpr =>
            {
                var q = DprQuality(baseQ, dpr);
                var dprParams = new Dictionary<string, object>(parameters) { ["q"] = q };
                if (parameters.ContainsKey("w"))
                    dprParams["w"] = (int)Math.Round(Convert.ToDouble(parameters["w"]) * dpr);
                if (parameters.ContainsKey("h"))
                    dprParams["h"] = (int)Math.Round(Convert.ToDouble(parameters["h"]) * dpr);
                return $"{Url(path, dprParams)} {dpr}x";
            }));
        }

        return "";
    }

    /// <summary>
    /// Generate an &lt;img&gt; tag for a Sirv image.
    /// </summary>
    /// <param name="path">Image path.</param>
    /// <param name="options">Image tag options.</param>
    /// <returns>HTML string.</returns>
    public string Image(string path, ImageOptions? options = null)
    {
        options ??= new ImageOptions();
        var src = Url(path, options.Transform);
        var cls = options.ClassName != null ? $"Sirv {options.ClassName}" : "Sirv";
        var html = $"<img class=\"{cls}\" data-src=\"{EscapeAttr(src)}\"";
        if (options.Alt != null) html += $" alt=\"{EscapeAttr(options.Alt)}\"";
        if (options.Viewer != null) html += $" data-options=\"{EscapeAttr(SerializeViewerOptions(options.Viewer))}\"";
        html += ">";
        return html;
    }

    /// <summary>
    /// Generate a &lt;div&gt; tag for a Sirv zoom viewer.
    /// </summary>
    /// <param name="path">Image path.</param>
    /// <param name="options">Viewer div options.</param>
    /// <returns>HTML string.</returns>
    public string Zoom(string path, ViewerDivOptions? options = null)
    {
        return ViewerDiv(path, "zoom", options);
    }

    /// <summary>
    /// Generate a &lt;div&gt; tag for a Sirv spin viewer.
    /// </summary>
    /// <param name="path">Path to .spin file.</param>
    /// <param name="options">Viewer div options.</param>
    /// <returns>HTML string.</returns>
    public string Spin(string path, ViewerDivOptions? options = null)
    {
        return ViewerDiv(path, null, options);
    }

    /// <summary>
    /// Generate a &lt;div&gt; tag for a Sirv video.
    /// </summary>
    /// <param name="path">Video path.</param>
    /// <param name="options">Viewer div options.</param>
    /// <returns>HTML string.</returns>
    public string Video(string path, ViewerDivOptions? options = null)
    {
        return ViewerDiv(path, null, options);
    }

    /// <summary>
    /// Generate a &lt;div&gt; tag for a Sirv 3D model viewer.
    /// </summary>
    /// <param name="path">Path to .glb file.</param>
    /// <param name="options">Viewer div options.</param>
    /// <returns>HTML string.</returns>
    public string Model(string path, ViewerDivOptions? options = null)
    {
        return ViewerDiv(path, null, options);
    }

    /// <summary>
    /// Internal helper to generate viewer div tags.
    /// </summary>
    private string ViewerDiv(string path, string? type, ViewerDivOptions? options = null)
    {
        options ??= new ViewerDivOptions();
        var src = Url(path, options.Transform);
        var cls = options.ClassName != null ? $"Sirv {options.ClassName}" : "Sirv";
        var html = $"<div class=\"{cls}\" data-src=\"{EscapeAttr(src)}\"";
        if (type != null) html += $" data-type=\"{type}\"";
        if (options.Viewer != null) html += $" data-options=\"{EscapeAttr(SerializeViewerOptions(options.Viewer))}\"";
        html += "></div>";
        return html;
    }

    /// <summary>
    /// Generate a gallery container with multiple assets.
    /// </summary>
    /// <param name="items">Gallery items.</param>
    /// <param name="options">Gallery-level options.</param>
    /// <returns>HTML string.</returns>
    public string Gallery(GalleryItem[] items, GalleryOptions? options = null)
    {
        options ??= new GalleryOptions();
        var cls = options.ClassName != null ? $"Sirv {options.ClassName}" : "Sirv";
        var html = $"<div class=\"{cls}\"";
        if (options.Viewer != null) html += $" data-options=\"{EscapeAttr(SerializeViewerOptions(options.Viewer))}\"";
        html += ">";

        foreach (var item in items)
        {
            var src = Url(item.Src, item.Transform);
            var child = $"<div data-src=\"{EscapeAttr(src)}\"";
            if (item.Type != null) child += $" data-type=\"{item.Type}\"";
            if (item.Viewer != null) child += $" data-options=\"{EscapeAttr(SerializeViewerOptions(item.Viewer))}\"";
            child += "></div>";
            html += child;
        }

        html += "</div>";
        return html;
    }

    /// <summary>
    /// Generate a &lt;script&gt; tag to load Sirv JS.
    /// </summary>
    /// <param name="options">Script tag options.</param>
    /// <returns>HTML string.</returns>
    public string ScriptTag(ScriptTagOptions? options = null)
    {
        options ??= new ScriptTagOptions();
        var filename = "sirv";
        if (options.Modules != null && options.Modules.Length > 0)
        {
            filename = "sirv." + string.Join(".", options.Modules);
        }
        var html = $"<script src=\"https://scripts.sirv.com/sirvjs/v3/{filename}.js\"";
        if (options.Async) html += " async";
        html += "></script>";
        return html;
    }
}
