using Sirv;

// ──────────────────────────────────────────────
// Create a SirvClient
// ──────────────────────────────────────────────

var sirv = new SirvClient("demo.sirv.com", new Dictionary<string, object> { { "q", 80 } });

// ──────────────────────────────────────────────
// Build URLs with transformations
// ──────────────────────────────────────────────

var url1 = sirv.Url("/photo.jpg", new Dictionary<string, object>
{
    { "w", 400 },
    { "h", 300 },
    { "format", "webp" }
});
Console.WriteLine(url1);
// https://demo.sirv.com/photo.jpg?q=80&w=400&h=300&format=webp

// ──────────────────────────────────────────────
// Nested params flatten to dot-notation
// ──────────────────────────────────────────────

var url2 = sirv.Url("/photo.jpg", new Dictionary<string, object>
{
    { "crop", new Dictionary<string, object>
        {
            { "type", "face" },
            { "pad", new Dictionary<string, object> { { "width", 10 } } }
        }
    }
});
Console.WriteLine(url2);
// https://demo.sirv.com/photo.jpg?q=80&crop.type=face&crop.pad.width=10

// ──────────────────────────────────────────────
// Responsive srcset with explicit widths
// ──────────────────────────────────────────────

var srcset1 = sirv.SrcSet("/photo.jpg",
    new Dictionary<string, object> { { "format", "webp" } },
    new SrcSetOptions { Widths = new[] { 320, 640, 960, 1280 } });
Console.WriteLine(srcset1);

// ──────────────────────────────────────────────
// Responsive srcset with auto-range
// ──────────────────────────────────────────────

var srcset2 = sirv.SrcSet("/photo.jpg",
    new Dictionary<string, object> { { "format", "webp" } },
    new SrcSetOptions { MinWidth = 200, MaxWidth = 2000, Tolerance = 0.15 });
Console.WriteLine(srcset2);

// ──────────────────────────────────────────────
// Responsive srcset with device pixel ratios
// ──────────────────────────────────────────────

var srcset3 = sirv.SrcSet("/hero.jpg",
    new Dictionary<string, object> { { "w", 600 }, { "h", 400 } },
    new SrcSetOptions { DevicePixelRatios = new[] { 1.0, 2.0, 3.0 } });
Console.WriteLine(srcset3);

// ──────────────────────────────────────────────
// Image tag
// ──────────────────────────────────────────────

var img = sirv.Image("/tomatoes.jpg", new ImageOptions
{
    Alt = "Fresh tomatoes",
    Transform = new Dictionary<string, object> { { "w", 300 } },
    Viewer = new Dictionary<string, object> { { "autostart", "visible" } }
});
Console.WriteLine(img);

// ──────────────────────────────────────────────
// Zoom viewer
// ──────────────────────────────────────────────

var zoom = sirv.Zoom("/product.jpg", new ViewerDivOptions
{
    Viewer = new Dictionary<string, object> { { "mode", "deep" }, { "wheel", false } }
});
Console.WriteLine(zoom);

// ──────────────────────────────────────────────
// Spin viewer
// ──────────────────────────────────────────────

var spin = sirv.Spin("/product.spin", new ViewerDivOptions
{
    Viewer = new Dictionary<string, object> { { "autostart", "visible" }, { "autospin", "lazy" } }
});
Console.WriteLine(spin);

// ──────────────────────────────────────────────
// Video
// ──────────────────────────────────────────────

var video = sirv.Video("/clip.mp4");
Console.WriteLine(video);

// ──────────────────────────────────────────────
// 3D Model
// ──────────────────────────────────────────────

var model = sirv.Model("/shoe.glb");
Console.WriteLine(model);

// ──────────────────────────────────────────────
// Gallery
// ──────────────────────────────────────────────

var gallery = sirv.Gallery(new[]
{
    new GalleryItem { Src = "/product.spin" },
    new GalleryItem { Src = "/front.jpg", Type = "zoom" },
    new GalleryItem
    {
        Src = "/side.jpg",
        Type = "zoom",
        Viewer = new Dictionary<string, object> { { "mode", "deep" } }
    }
}, new GalleryOptions
{
    Viewer = new Dictionary<string, object> { { "arrows", true }, { "thumbnails", "bottom" } },
    ClassName = "product-gallery"
});
Console.WriteLine(gallery);

// ──────────────────────────────────────────────
// Script tag
// ──────────────────────────────────────────────

var script1 = sirv.ScriptTag();
Console.WriteLine(script1);
// <script src="https://scripts.sirv.com/sirvjs/v3/sirv.js" async></script>

var script2 = sirv.ScriptTag(new ScriptTagOptions { Modules = new[] { "spin", "zoom" } });
Console.WriteLine(script2);
// <script src="https://scripts.sirv.com/sirvjs/v3/sirv.spin.zoom.js" async></script>

var script3 = sirv.ScriptTag(new ScriptTagOptions { Async = false });
Console.WriteLine(script3);
// <script src="https://scripts.sirv.com/sirvjs/v3/sirv.js"></script>
