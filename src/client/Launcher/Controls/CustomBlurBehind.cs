using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace Arise.Client.Launcher.Controls;

// Taken from: https://gist.github.com/kekekeks/ac06098a74fe87d49a9ff9ea37fa67bc
// - modified to support rounded corners with different radius per corner
// - added partial designer support (from the first comment in the gist)
// - exposed blur radius as a StyledProperty
public sealed class CustomBlurBehind : Control
{
    private static readonly ImmutableExperimentalAcrylicMaterial DefaultAcrylicMaterial =
        (ImmutableExperimentalAcrylicMaterial)new ExperimentalAcrylicMaterial()
        {
            MaterialOpacity = 0.1,
            TintColor = Colors.White,
            TintOpacity = 0,
            PlatformTransparencyCompensationLevel = 0,
        }.ToImmutable();

    public static readonly StyledProperty<ExperimentalAcrylicMaterial> MaterialProperty =
        AvaloniaProperty.Register<CustomBlurBehind, ExperimentalAcrylicMaterial>(nameof(Material));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<CustomBlurBehind, CornerRadius>(nameof(CornerRadius));

    public static readonly StyledProperty<float> BlurRadiusProperty =
        AvaloniaProperty.Register<CustomBlurBehind, float>(nameof(BlurRadius), 7);

    private static SKShader? _acrylicNoiseShader;

    public ExperimentalAcrylicMaterial Material
    {
        get => GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public float BlurRadius
    {
        get => GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    static CustomBlurBehind()
    {
        AffectsRender<CustomBlurBehind>(MaterialProperty);
        AffectsRender<CustomBlurBehind>(CornerRadiusProperty);
    }

    public override void Render(DrawingContext context)
    {
        var mat = Material != null
            ? (ImmutableExperimentalAcrylicMaterial)Material.ToImmutable()
            : DefaultAcrylicMaterial;

        var op = new BlurBehindRenderOperation(
                    mat,
                    new RoundedRect(
                        new Rect(default, Bounds.Size),
                        CornerRadius.TopLeft,
                        CornerRadius.TopRight,
                        CornerRadius.BottomRight,
                        CornerRadius.BottomLeft),
                    BlurRadius);

        context.Custom(op);

        op.Dispose();
    }

    // Render operation implementation

    private sealed class BlurBehindRenderOperation : ICustomDrawOperation
    {
        private readonly ImmutableExperimentalAcrylicMaterial _material;
        private readonly RoundedRect _bounds;
        private readonly float _blurRadius;

        public Rect Bounds => _bounds.Rect.Inflate(4);

        public BlurBehindRenderOperation(ImmutableExperimentalAcrylicMaterial material, RoundedRect bounds, float blurRadius)
        {
            _material = material;
            _bounds = bounds;
            _blurRadius = blurRadius;
        }

        public void Render(ImmediateDrawingContext context)
        {
            var feature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (feature == null)
                return;

            using var skia = feature.Lease();

            if (skia == null)
                return;

            if (!skia.SkCanvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                return;

            using var backgroundSnapshot = skia.SkSurface!.Snapshot(); // todo: handle null SkSurface
            using var backdropShader = SKShader.CreateImage(
                backgroundSnapshot,
                SKShaderTileMode.Clamp,
                SKShaderTileMode.Clamp,
                currentInvertedTransform);

            using var skrrect = CreateSKRoundRect(_bounds);

            // todo: fix this (it's for the designer only)
            if (skia.GrContext == null)
            {
                using var designerFilter = SKImageFilter.CreateBlur(_blurRadius, _blurRadius, SKShaderTileMode.Clamp);
                using var tmp = new SKPaint()
                {
                    Shader = backdropShader,
                    ImageFilter = designerFilter,
                };
                skia.SkCanvas.DrawRoundRect(skrrect, tmp);

                return;
            }

            using var blurred = SKSurface.Create(skia.GrContext, false, new SKImageInfo(
                (int)Math.Ceiling(_bounds.Rect.Width),
                (int)Math.Ceiling(_bounds.Rect.Height),
                SKImageInfo.PlatformColorType,
                SKAlphaType.Premul));
            using var filter = SKImageFilter.CreateBlur(_blurRadius, _blurRadius, SKShaderTileMode.Clamp);
            using var blurPaint = new SKPaint
            {
                Shader = backdropShader,
                ImageFilter = filter,
            };

            blurred.Canvas.DrawRoundRect(skrrect, blurPaint);

            using var blurSnap = blurred.Snapshot();
            using var blurSnapShader = SKShader.CreateImage(blurSnap);
            using var blurSnapPaint = new SKPaint
            {
                Shader = blurSnapShader,
                IsAntialias = true,
            };

            skia.SkCanvas.DrawRoundRect(skrrect, blurSnapPaint);

            using var acrylliPaint = new SKPaint();
            acrylliPaint.IsAntialias = true;

            var tintColor = _material.TintColor;
            var tint = new SKColor(tintColor.R, tintColor.G, tintColor.B, tintColor.A);

            EnsureAcrylicNoiseShader();

            using var backdrop = SKShader.CreateColor(new SKColor(_material.MaterialColor.R, _material.MaterialColor.G, _material.MaterialColor.B, _material.MaterialColor.A));
            using var tintShader = SKShader.CreateColor(tint);
            using var effectiveTint = SKShader.CreateCompose(backdrop, tintShader);
            using var compose = SKShader.CreateCompose(effectiveTint, _acrylicNoiseShader);
            acrylliPaint.Shader = compose;
            acrylliPaint.IsAntialias = true;
            skia.SkCanvas.DrawRoundRect(skrrect, acrylliPaint);
        }

        [SuppressMessage("", "CA2000")]
        private static void EnsureAcrylicNoiseShader()
        {
            if (_acrylicNoiseShader != null)
                return;

            const double noiseOpacity = 0.0225;

            using var stream = typeof(SkiaPlatform).Assembly.GetManifestResourceStream("Avalonia.Skia.Assets.NoiseAsset_256X256_PNG.png");
            using var bitmap = SKBitmap.Decode(stream);

            _acrylicNoiseShader = SKShader.CreateBitmap(
                bitmap,
                SKShaderTileMode.Repeat,
                SKShaderTileMode.Repeat)
                .WithColorFilter(CreateAlphaColorFilter(noiseOpacity));
        }

        private static SKRoundRect CreateSKRoundRect(RoundedRect bounds)
        {
            var skrrect = new SKRoundRect(new SKRect(0, 0, (float)bounds.Rect.Width, (float)bounds.Rect.Height));
            skrrect.SetRectRadii(skrrect.Rect, [
                new SKPoint((float)bounds.RadiiTopLeft.X, (float)bounds.RadiiTopLeft.Y),
                new SKPoint((float)bounds.RadiiTopRight.X, (float)bounds.RadiiTopRight.Y),
                new SKPoint((float)bounds.RadiiBottomRight.X, (float)bounds.RadiiBottomRight.Y),
                new SKPoint((float)bounds.RadiiBottomLeft.X, (float)bounds.RadiiBottomLeft.Y)]);

            return skrrect;
        }

        private static SKColorFilter CreateAlphaColorFilter(double opacity)
        {
            if (opacity > 1)
                opacity = 1;
            var c = new byte[256];
            var a = new byte[256];
            for (var i = 0; i < 256; i++)
            {
                c[i] = (byte)i;
                a[i] = (byte)(i * opacity);
            }

            return SKColorFilter.CreateTable(a, c, c, c);
        }

        public bool HitTest(Point p)
        {
            return _bounds.ContainsExclusive(p);
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is BlurBehindRenderOperation op && op._bounds == _bounds && op._material.Equals(_material);
        }

        public void Dispose()
        {
        }
    }
}
