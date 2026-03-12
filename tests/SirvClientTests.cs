using Xunit;

namespace Sirv.Tests;

public class SirvClientTests
{
    private const string Domain = "demo.sirv.com";

    // ── Constructor ─────────────────────────────────────────

    [Fact]
    public void Constructor_RequiresDomain()
    {
        Assert.Throws<ArgumentException>(() => new SirvClient(""));
        Assert.Throws<ArgumentException>(() => new SirvClient("  "));
    }

    [Fact]
    public void Constructor_WithDomainOnly()
    {
        var sirv = new SirvClient(Domain);
        Assert.Equal("https://demo.sirv.com/image.jpg", sirv.Url("/image.jpg"));
    }

    [Fact]
    public void Constructor_StripsTrailingSlash()
    {
        var sirv = new SirvClient("demo.sirv.com/");
        Assert.Equal("https://demo.sirv.com/image.jpg", sirv.Url("/image.jpg"));
    }

    [Fact]
    public void Constructor_WithDefaults()
    {
        var sirv = new SirvClient(Domain, new Dictionary<string, object> { { "q", 80 } });
        Assert.Equal("https://demo.sirv.com/image.jpg?q=80", sirv.Url("/image.jpg"));
    }

    // ── Url() ──────────────────────────────────────────────

    [Fact]
    public void Url_WithSimpleParams()
    {
        var sirv = new SirvClient(Domain);
        var url = sirv.Url("/image.jpg", new Dictionary<string, object> { { "w", 300 }, { "h", 200 }, { "format", "webp" } });
        Assert.Equal("https://demo.sirv.com/image.jpg?w=300&h=200&format=webp", url);
    }

    [Fact]
    public void Url_MergesDefaultsWithParams()
    {
        var sirv = new SirvClient(Domain, new Dictionary<string, object> { { "q", 80 } });
        var url = sirv.Url("/image.jpg", new Dictionary<string, object> { { "w", 300 }, { "h", 200 }, { "format", "webp" } });
        Assert.Equal("https://demo.sirv.com/image.jpg?q=80&w=300&h=200&format=webp", url);
    }

    [Fact]
    public void Url_ParamsOverrideDefaults()
    {
        var sirv = new SirvClient(Domain, new Dictionary<string, object> { { "q", 80 } });
        var url = sirv.Url("/image.jpg", new Dictionary<string, object> { { "q", 90 } });
        Assert.Equal("https://demo.sirv.com/image.jpg?q=90", url);
    }

    [Fact]
    public void Url_NestedParamsFlattenToDotNotation()
    {
        var sirv = new SirvClient(Domain);
        var url = sirv.Url("/image.jpg", new Dictionary<string, object>
        {
            { "crop", new Dictionary<string, object>
                {
                    { "type", "face" },
                    { "pad", new Dictionary<string, object> { { "width", 10 }, { "height", 10 } } }
                }
            }
        });
        Assert.Contains("crop.type=face", url);
        Assert.Contains("crop.pad.width=10", url);
        Assert.Contains("crop.pad.height=10", url);
    }

    [Fact]
    public void Url_DeeplyNestedParams()
    {
        var sirv = new SirvClient(Domain);
        var url = sirv.Url("/image.jpg", new Dictionary<string, object>
        {
            { "text", new Dictionary<string, object>
                {
                    { "font", new Dictionary<string, object> { { "family", "Arial" }, { "size", 24 } } },
                    { "color", "white" }
                }
            }
        });
        Assert.Contains("text.font.family=Arial", url);
        Assert.Contains("text.font.size=24", url);
        Assert.Contains("text.color=white", url);
    }

    [Fact]
    public void Url_AddsLeadingSlashIfMissing()
    {
        var sirv = new SirvClient(Domain);
        Assert.Equal("https://demo.sirv.com/image.jpg", sirv.Url("image.jpg"));
    }

    [Fact]
    public void Url_WithNoParamsReturnsCleanUrl()
    {
        var sirv = new SirvClient(Domain);
        Assert.Equal("https://demo.sirv.com/image.jpg", sirv.Url("/image.jpg"));
    }

    [Fact]
    public void Url_EncodesSpecialCharacters()
    {
        var sirv = new SirvClient(Domain);
        var url = sirv.Url("/image.jpg", new Dictionary<string, object> { { "subsampling", "4:2:0" } });
        Assert.Contains("subsampling=4%3A2%3A0", url);
    }

    // ── SrcSet() ───────────────────────────────────────────

    [Fact]
    public void SrcSet_WithExplicitWidths()
    {
        var sirv = new SirvClient(Domain);
        var srcset = sirv.SrcSet("/image.jpg",
            new Dictionary<string, object> { { "format", "webp" } },
            new SrcSetOptions { Widths = new[] { 320, 640, 960 } });
        Assert.Contains("w=320 320w", srcset);
        Assert.Contains("w=640 640w", srcset);
        Assert.Contains("w=960 960w", srcset);
        Assert.Contains("format=webp", srcset);
    }

    [Fact]
    public void SrcSet_WithMinMaxWidthTolerance()
    {
        var sirv = new SirvClient(Domain);
        var srcset = sirv.SrcSet("/image.jpg",
            new Dictionary<string, object> { { "format", "webp" } },
            new SrcSetOptions { MinWidth = 200, MaxWidth = 2000, Tolerance = 0.15 });
        var entries = srcset.Split(", ");
        Assert.True(entries.Length > 2);
        Assert.Contains("w=200", entries[0]);
        Assert.Contains("w=2000", entries[^1]);
    }

    [Fact]
    public void SrcSet_WithDevicePixelRatios()
    {
        var sirv = new SirvClient(Domain, new Dictionary<string, object> { { "q", 80 } });
        var srcset = sirv.SrcSet("/hero.jpg",
            new Dictionary<string, object> { { "w", 600 }, { "h", 400 } },
            new SrcSetOptions { DevicePixelRatios = new[] { 1.0, 2.0, 3.0 } });
        Assert.Contains("1x", srcset);
        Assert.Contains("2x", srcset);
        Assert.Contains("3x", srcset);
        Assert.Contains("q=80", srcset);
        Assert.Contains("w=1200", srcset);
        Assert.Contains("w=1800", srcset);
    }

    [Fact]
    public void SrcSet_DprUsesVariableQuality()
    {
        var sirv = new SirvClient(Domain, new Dictionary<string, object> { { "q", 80 } });
        var srcset = sirv.SrcSet("/hero.jpg",
            new Dictionary<string, object> { { "w", 600 } },
            new SrcSetOptions { DevicePixelRatios = new[] { 1.0, 2.0, 3.0 } });
        var entries = srcset.Split(", ");
        Assert.Contains("q=80", entries[0]);
        Assert.Contains("q=60", entries[1]);
    }

    [Fact]
    public void SrcSet_ReturnsEmptyStringWithNoOptions()
    {
        var sirv = new SirvClient(Domain);
        Assert.Equal("", sirv.SrcSet("/image.jpg"));
    }

    // ── Image() ────────────────────────────────────────────

    [Fact]
    public void Image_GeneratesImgTag()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Image("/tomatoes.jpg", new ImageOptions { Alt = "Fresh tomatoes" });
        Assert.Equal("<img class=\"Sirv\" data-src=\"https://demo.sirv.com/tomatoes.jpg\" alt=\"Fresh tomatoes\">", html);
    }

    [Fact]
    public void Image_WithTransformParams()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Image("/photo.jpg", new ImageOptions
        {
            Transform = new Dictionary<string, object> { { "w", 300 }, { "format", "webp" } }
        });
        Assert.Contains("data-src=\"https://demo.sirv.com/photo.jpg?w=300&amp;format=webp\"", html);
    }

    [Fact]
    public void Image_WithViewerOptions()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Image("/photo.jpg", new ImageOptions
        {
            Viewer = new Dictionary<string, object> { { "autostart", "visible" }, { "threshold", 200 } }
        });
        Assert.Contains("data-options=\"autostart:visible;threshold:200\"", html);
    }

    [Fact]
    public void Image_WithCustomClassName()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Image("/photo.jpg", new ImageOptions { ClassName = "hero-image" });
        Assert.Contains("class=\"Sirv hero-image\"", html);
    }

    [Fact]
    public void Image_WithEmptyAlt()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Image("/photo.jpg", new ImageOptions { Alt = "" });
        Assert.Contains("alt=\"\"", html);
    }

    // ── Zoom() ─────────────────────────────────────────────

    [Fact]
    public void Zoom_GeneratesDivWithDataTypeZoom()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Zoom("/product.jpg");
        Assert.Equal("<div class=\"Sirv\" data-src=\"https://demo.sirv.com/product.jpg\" data-type=\"zoom\"></div>", html);
    }

    [Fact]
    public void Zoom_WithViewerOptions()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Zoom("/product.jpg", new ViewerDivOptions
        {
            Viewer = new Dictionary<string, object> { { "mode", "deep" }, { "wheel", false } }
        });
        Assert.Contains("data-type=\"zoom\"", html);
        Assert.Contains("data-options=\"mode:deep;wheel:false\"", html);
    }

    // ── Spin() ─────────────────────────────────────────────

    [Fact]
    public void Spin_GeneratesDivWithoutDataType()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Spin("/product.spin");
        Assert.Equal("<div class=\"Sirv\" data-src=\"https://demo.sirv.com/product.spin\"></div>", html);
        Assert.DoesNotContain("data-type", html);
    }

    [Fact]
    public void Spin_WithViewerOptions()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Spin("/product.spin", new ViewerDivOptions
        {
            Viewer = new Dictionary<string, object> { { "autostart", "visible" }, { "autospin", "lazy" } }
        });
        Assert.Contains("data-options=\"autostart:visible;autospin:lazy\"", html);
    }

    // ── Video() ────────────────────────────────────────────

    [Fact]
    public void Video_GeneratesDivWithoutDataType()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Video("/clip.mp4");
        Assert.Equal("<div class=\"Sirv\" data-src=\"https://demo.sirv.com/clip.mp4\"></div>", html);
    }

    // ── Model() ────────────────────────────────────────────

    [Fact]
    public void Model_GeneratesDivWithoutDataType()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Model("/shoe.glb");
        Assert.Equal("<div class=\"Sirv\" data-src=\"https://demo.sirv.com/shoe.glb\"></div>", html);
    }

    // ── Gallery() ──────────────────────────────────────────

    [Fact]
    public void Gallery_GeneratesNestedDivs()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Gallery(new[]
        {
            new GalleryItem { Src = "/product.spin" },
            new GalleryItem { Src = "/front.jpg", Type = "zoom" }
        });
        Assert.Contains("<div class=\"Sirv\">", html);
        Assert.Contains("data-src=\"https://demo.sirv.com/product.spin\"", html);
        Assert.Contains("data-src=\"https://demo.sirv.com/front.jpg\" data-type=\"zoom\"", html);
        Assert.EndsWith("</div></div>", html);
    }

    [Fact]
    public void Gallery_WithGalleryLevelViewerOptions()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Gallery(
            new[] { new GalleryItem { Src = "/image1.jpg" } },
            new GalleryOptions
            {
                Viewer = new Dictionary<string, object> { { "arrows", true }, { "thumbnails", "bottom" } }
            });
        Assert.Contains("data-options=\"arrows:true;thumbnails:bottom\"", html);
    }

    [Fact]
    public void Gallery_WithPerItemViewerOptions()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Gallery(new[]
        {
            new GalleryItem
            {
                Src = "/product.jpg",
                Type = "zoom",
                Viewer = new Dictionary<string, object> { { "mode", "deep" } }
            }
        });
        Assert.Contains("data-type=\"zoom\"", html);
        Assert.Contains("data-options=\"mode:deep\"", html);
    }

    [Fact]
    public void Gallery_WithPerItemTransforms()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Gallery(new[]
        {
            new GalleryItem
            {
                Src = "/photo.jpg",
                Transform = new Dictionary<string, object> { { "w", 800 }, { "format", "webp" } }
            }
        });
        Assert.Contains("w=800", html);
        Assert.Contains("format=webp", html);
    }

    [Fact]
    public void Gallery_WithCustomClassName()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.Gallery(
            new[] { new GalleryItem { Src = "/img.jpg" } },
            new GalleryOptions { ClassName = "product-gallery" });
        Assert.Contains("class=\"Sirv product-gallery\"", html);
    }

    // ── ScriptTag() ────────────────────────────────────────

    [Fact]
    public void ScriptTag_WithNoModules()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.ScriptTag();
        Assert.Equal("<script src=\"https://scripts.sirv.com/sirvjs/v3/sirv.js\" async></script>", html);
    }

    [Fact]
    public void ScriptTag_WithModules()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.ScriptTag(new ScriptTagOptions { Modules = new[] { "spin", "zoom" } });
        Assert.Equal("<script src=\"https://scripts.sirv.com/sirvjs/v3/sirv.spin.zoom.js\" async></script>", html);
    }

    [Fact]
    public void ScriptTag_WithoutAsync()
    {
        var sirv = new SirvClient(Domain);
        var html = sirv.ScriptTag(new ScriptTagOptions { Async = false });
        Assert.Equal("<script src=\"https://scripts.sirv.com/sirvjs/v3/sirv.js\"></script>", html);
    }
}
