# Sirv Image Transformation - .NET (C#)

A fluent builder for constructing [Sirv image transformation](https://sirv.com/help/articles/dynamic-imaging/) URLs in .NET.

## Requirements

- .NET 6.0 or later

## Installation

Add the `SirvImage.cs` source file to your project, or reference the project directly.

## Quick Start

```csharp
using Sirv;

var url = new SirvImage("https://demo.sirv.com/photo.jpg")
    .Resize(400, 300)
    .Format("webp")
    .Quality(80)
    .ToUrl();

// https://demo.sirv.com/photo.jpg?w=400&h=300&format=webp&q=80
```

## Constructors

### From a full URL

```csharp
var img = new SirvImage("https://demo.sirv.com/photo.jpg");
```

Any existing query parameters on the URL are preserved.

### From base URL and path

```csharp
var img = new SirvImage("https://demo.sirv.com", "/photos/landscape.jpg");
```

## API Reference

All methods return the `SirvImage` instance for chaining. Call `.ToUrl()` or `.ToString()` to get the final URL string.

### Resize

| Method | Parameters | Description |
|--------|-----------|-------------|
| `Resize` | `int? width, int? height, string? option` | Resize image. Option: `"ignore"`, `"fill"`, `"force"` |
| `Width` | `int w` | Set width in pixels |
| `Height` | `int h` | Set height in pixels |
| `ScaleByLongest` | `int s` | Scale by longest edge |
| `Thumbnail` | `int size` | Generate square thumbnail |

```csharp
new SirvImage(url).Resize(400, 300, "fill").ToUrl();
new SirvImage(url).Width(800).ToUrl();
new SirvImage(url).Thumbnail(150).ToUrl();
```

### Crop

| Method | Parameters | Description |
|--------|-----------|-------------|
| `Crop` | `int? width, int? height, int? x, int? y, string? type, int? padWidth, int? padHeight` | Crop image. Type: `"trim"`, `"face"`, `"poi"` |
| `ClipPath` | `string name` | Apply a clip path |

```csharp
new SirvImage(url).Crop(width: 300, height: 300, type: "face").ToUrl();
```

### Rotation

| Method | Parameters | Description |
|--------|-----------|-------------|
| `Rotate` | `double degrees` | Rotate by degrees |
| `Flip` | - | Flip vertically |
| `Flop` | - | Flop horizontally |

```csharp
new SirvImage(url).Rotate(90).Flip().ToUrl();
```

### Format

| Method | Parameters | Description |
|--------|-----------|-------------|
| `Format` | `string fmt` | Output format: `"jpg"`, `"png"`, `"webp"`, `"avif"`, `"gif"`, `"svg"` |
| `Quality` | `int q` | Quality 0-100 |
| `WebpFallback` | `string fmt` | Fallback format when WebP unsupported |
| `Subsampling` | `string v` | JPEG subsampling: `"420"`, `"422"`, `"444"` |
| `PngOptimize` | `bool enabled` | Enable PNG optimization |
| `GifLossy` | `int level` | GIF lossy compression level (30-200) |

```csharp
new SirvImage(url).Format("webp").Quality(85).ToUrl();
```

### Color Adjustments

| Method | Parameters | Description |
|--------|-----------|-------------|
| `Brightness` | `int v` | Adjust brightness (-100 to 100) |
| `Contrast` | `int v` | Adjust contrast (-100 to 100) |
| `Exposure` | `int v` | Adjust exposure (-100 to 100) |
| `Hue` | `int v` | Adjust hue (-360 to 360) |
| `Saturation` | `int v` | Adjust saturation (-100 to 100) |
| `Lightness` | `int v` | Adjust lightness (-100 to 100) |
| `Shadows` | `int v` | Adjust shadows (-100 to 100) |
| `Highlights` | `int v` | Adjust highlights (-100 to 100) |
| `Grayscale` | - | Convert to grayscale |
| `ColorLevel` | `int black, int white` | Set black/white points (0-255) |
| `Histogram` | `string channel` | Request histogram: `"r"`, `"g"`, `"b"`, `"rgb"`, `"rgba"` |

```csharp
new SirvImage(url).Brightness(10).Contrast(20).Grayscale().ToUrl();
```

### Color Effects

| Method | Parameters | Description |
|--------|-----------|-------------|
| `Colorize` | `string color, int opacity` | Apply color overlay |
| `Colortone` | `string preset` | Apply preset: `"sepia"`, `"warm"`, etc. |
| `Colortone` | `string color, int level, string mode` | Custom colortone. Mode: `"solid"`, `"multiply"`, `"screen"` |

```csharp
new SirvImage(url).Colortone("sepia").ToUrl();
new SirvImage(url).Colorize("ff0000", 30).ToUrl();
```

### Effects

| Method | Parameters | Description |
|--------|-----------|-------------|
| `Blur` | `int v` | Gaussian blur (1-100) |
| `Sharpen` | `int v` | Sharpen (1-100) |
| `Vignette` | `int value, string? color` | Vignette effect |
| `Opacity` | `int v` | Set opacity (0-100) |

```csharp
new SirvImage(url).Blur(5).Sharpen(10).Vignette(50, "333333").ToUrl();
```

### Text Overlays

Add text overlays with the `Text` method. Call multiple times for multiple layers.

```csharp
var url = new SirvImage("https://demo.sirv.com/photo.jpg")
    .Text("Title", new TextOptions
    {
        Size = 48,
        Color = "ffffff",
        FontFamily = "Arial",
        FontWeight = "bold",
        Position = "center",
        PositionGravity = "north",
        BackgroundColor = "000000",
        BackgroundOpacity = 60
    })
    .Text("Subtitle", new TextOptions
    {
        Size = 24,
        Color = "cccccc",
        PositionGravity = "south"
    })
    .ToUrl();
```

#### TextOptions Properties

| Property | Type | Description |
|----------|------|-------------|
| `Size` | `int?` | Font size in pixels |
| `FontFamily` | `string?` | Font family name |
| `FontStyle` | `string?` | `"normal"` or `"italic"` |
| `FontWeight` | `string?` | `"normal"`, `"bold"`, or numeric |
| `Color` | `string?` | Text color (hex) |
| `Opacity` | `int?` | Text opacity (0-100) |
| `OutlineWidth` | `int?` | Outline width in pixels |
| `OutlineColor` | `string?` | Outline color (hex) |
| `BackgroundColor` | `string?` | Background color (hex) |
| `BackgroundOpacity` | `int?` | Background opacity (0-100) |
| `Position` | `string?` | Horizontal position |
| `PositionGravity` | `string?` | Gravity position |
| `PositionX` | `int?` | X offset in pixels |
| `PositionY` | `int?` | Y offset in pixels |
| `Rotate` | `double?` | Rotation in degrees |

### Watermarks

Add watermark overlays with the `Watermark` method. Call multiple times for multiple watermarks.

```csharp
var url = new SirvImage("https://demo.sirv.com/photo.jpg")
    .Watermark("/logos/logo.png", new WatermarkOptions
    {
        Position = "southeast",
        Opacity = 70,
        Scale = "20"
    })
    .Watermark("/logos/badge.png", new WatermarkOptions
    {
        Position = "northeast",
        Width = 100
    })
    .ToUrl();
```

#### WatermarkOptions Properties

| Property | Type | Description |
|----------|------|-------------|
| `Position` | `string?` | Position: `"center"`, `"northwest"`, `"southeast"`, etc. |
| `PositionX` | `int?` | X offset in pixels |
| `PositionY` | `int?` | Y offset in pixels |
| `Width` | `int?` | Width in pixels |
| `Height` | `int?` | Height in pixels |
| `Opacity` | `int?` | Opacity (0-100) |
| `Scale` | `string?` | Scale: `"fit"`, `"fill"`, or percentage |

### Canvas

```csharp
new SirvImage(url).Canvas(new CanvasOptions
{
    Width = 800,
    Height = 600,
    Color = "f5f5f5",
    Position = "center",
    BorderWidth = 2,
    BorderColor = "000000"
}).ToUrl();
```

#### CanvasOptions Properties

| Property | Type | Description |
|----------|------|-------------|
| `Width` | `int?` | Canvas width |
| `Height` | `int?` | Canvas height |
| `Color` | `string?` | Background color (hex) |
| `Opacity` | `int?` | Background opacity (0-100) |
| `Position` | `string?` | Image position on canvas |
| `PositionX` | `int?` | X offset |
| `PositionY` | `int?` | Y offset |
| `BorderWidth` | `int?` | Border width |
| `BorderColor` | `string?` | Border color (hex) |

### Frame

```csharp
new SirvImage(url).Frame(new FrameOptions
{
    Style = "solid",
    Color = "333333",
    Width = 10,
    RimColor = "ffffff",
    RimWidth = 2
}).ToUrl();
```

#### FrameOptions Properties

| Property | Type | Description |
|----------|------|-------------|
| `Style` | `string?` | Frame style: `"solid"`, `"shadow"` |
| `Color` | `string?` | Frame color (hex) |
| `Width` | `int?` | Frame width in pixels |
| `RimColor` | `string?` | Rim color (hex) |
| `RimWidth` | `int?` | Rim width in pixels |

### Other

| Method | Parameters | Description |
|--------|-----------|-------------|
| `Page` | `int num` | Select page from multi-page document |
| `Profile` | `string name` | Apply a named processing profile |

```csharp
new SirvImage("https://demo.sirv.com/doc.pdf").Page(3).Width(800).Format("jpg").ToUrl();
```

## Method Chaining

All transformation methods return the `SirvImage` instance, enabling fluent chaining:

```csharp
var url = new SirvImage("https://demo.sirv.com/photo.jpg")
    .Resize(800, 600, "fill")
    .Crop(width: 800, height: 600, type: "face")
    .Format("webp")
    .Quality(85)
    .Sharpen(3)
    .Brightness(5)
    .Watermark("/logos/brand.png", new WatermarkOptions { Position = "southeast", Opacity = 60 })
    .Text("Copyright", new TextOptions { Size = 14, Color = "ffffff", Opacity = 50 })
    .ToUrl();
```

## URL Output

- `.ToUrl()` -- returns the complete URL as a `string`
- `.ToString()` -- override that calls `ToUrl()`, so you can use `SirvImage` directly in string interpolation

```csharp
var img = new SirvImage("https://demo.sirv.com/photo.jpg").Thumbnail(200);
Console.WriteLine($"URL: {img}");
```

## Running Tests

```bash
dotnet test
```

## License

See the repository root for license information.
