using System.Collections.ObjectModel;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Interactivity;
using Avalonia.Media;
using Path = Avalonia.Controls.Shapes.Path;

namespace Arise.Client.Launcher.Controls;

public class SparkleBorder : Panel
{
    private readonly DispatcherTimer _timer;

    private readonly Panel _container = new()
    {
        Effect = new DropShadowEffect
        {
            Color = Colors.White,
            OffsetX = 0,
            OffsetY = 0,
            BlurRadius = 10,
            Opacity = 1,
        },
    };

    private bool _sparkling;

    public bool SparklesEnabled
    {
        get => GetValue(SparklesEnabledProperty);
        set => SetValue(SparklesEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> SparklesEnabledProperty =
        AvaloniaProperty.Register<SparkleBorder, bool>(nameof(SparklesEnabled), defaultValue: true);

    public float Probability
    {
        get => GetValue(ProbabilityProperty);
        set => SetValue(ProbabilityProperty, value);
    }

    public static readonly StyledProperty<float> ProbabilityProperty =
        AvaloniaProperty.Register<SparkleBorder, float>(nameof(Probability), defaultValue: .6f);

    public float SpreadMultiplier
    {
        get => GetValue(SpreadMultiplierProperty);
        set => SetValue(SpreadMultiplierProperty, value);
    }

    public static readonly StyledProperty<float> SpreadMultiplierProperty =
        AvaloniaProperty.Register<SparkleBorder, float>(nameof(SpreadMultiplier), defaultValue: 2f);

    public float RadiusMultiplier
    {
        get => GetValue(RadiusMultiplierProperty);
        set => SetValue(RadiusMultiplierProperty, value);
    }

    public static readonly StyledProperty<float> RadiusMultiplierProperty =
        AvaloniaProperty.Register<SparkleBorder, float>(nameof(RadiusMultiplier), defaultValue: 1f);

    public int Limit
    {
        get => GetValue(LimitProperty);
        set => SetValue(LimitProperty, value);
    }

    public static readonly StyledProperty<int> LimitProperty =
        AvaloniaProperty.Register<SparkleBorder, int>(nameof(Limit), defaultValue: 40);

    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<SparkleBorder, TimeSpan>(nameof(AnimationDuration), defaultValue: TimeSpan.FromSeconds(3));

    public float FillProbabilty
    {
        get => GetValue(FillProbabiltyProperty);
        set => SetValue(FillProbabiltyProperty, value);
    }

    public static readonly StyledProperty<float> FillProbabiltyProperty =
        AvaloniaProperty.Register<SparkleBorder, float>(nameof(FillProbabilty), defaultValue: .5f);

#pragma warning disable CA2227
    public ObservableCollection<Geometry> Geometries
#pragma warning restore CA2227
    {
        get => GetValue(GeometriesProperty);
        set => SetValue(GeometriesProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<Geometry>> GeometriesProperty =
        AvaloniaProperty.Register<SparkleBorder, ObservableCollection<Geometry>>(nameof(Geometries), defaultValue: [
            Geometry.Parse("M 8,1.333 5.778,5.778 1.333,8 5.778,10.222 8,14.667 10.222,10.222 14.667,8 10.222,5.778 Z")]);

    public double MinGeometrySize
    {
        get => GetValue(MinGeometrySizeProperty);
        set => SetValue(MinGeometrySizeProperty, value);
    }

    public static readonly StyledProperty<double> MinGeometrySizeProperty =
        AvaloniaProperty.Register<SparkleBorder, double>(nameof(MinGeometrySize), defaultValue: 3d);

    public double MaxGeometrySize
    {
        get => GetValue(MaxGeometrySizeProperty);
        set => SetValue(MaxGeometrySizeProperty, value);
    }

    public static readonly StyledProperty<double> MaxGeometrySizeProperty =
        AvaloniaProperty.Register<SparkleBorder, double>(nameof(MaxGeometrySize), defaultValue: 6d);

    public bool RotationEnabled
    {
        get => GetValue(RotationEnabledProperty);
        set => SetValue(RotationEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> RotationEnabledProperty =
        AvaloniaProperty.Register<SparkleBorder, bool>(nameof(RotationEnabled), defaultValue: true);

    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<SparkleBorder, Color>(nameof(Color), defaultValue: Colors.White);

    private SolidColorBrush? _brush;

    public SparkleBorder()
    {
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(20), DispatcherPriority.Background, OnTick) { IsEnabled = false };
    }

#pragma warning disable CA5394

    private void OnTick(object? sender, EventArgs e)
    {
        if (Geometries.Count == 0)
            return;
        if (Children.Count >= Limit && Limit > 0)
            return;
        if (Random.Shared.NextDouble() > Probability)
            return;

        var rngX = (-0.5 + Random.Shared.NextDouble()) * Bounds.Width;
        var rngY = (-0.5 + Random.Shared.NextDouble()) * Bounds.Height;

        var tx = new TranslateTransform(rngX * RadiusMultiplier, rngY * RadiusMultiplier)
        {
            Transitions = [
                new DoubleTransition
                {
                    Duration = AnimationDuration,
                    Property = TranslateTransform.XProperty,
                    Easing = new QuadraticEaseOut(),
                },
                new DoubleTransition
                {
                    Duration = AnimationDuration,
                    Property = TranslateTransform.YProperty,
                    Easing = new QuadraticEaseOut(),
                }
            ],
        };

        var size = Math.Clamp(MinGeometrySize + Random.Shared.NextDouble() * (MaxGeometrySize - MinGeometrySize), MinGeometrySize, MaxGeometrySize);

        var fill = Random.Shared.NextSingle() < FillProbabilty;

        var geomRng = Random.Shared.Next(0, Geometries.Count);
        var txgrp = new TransformGroup { Children = [tx] };
        var p = new Path
        {
            Stroke = _brush,
            StrokeThickness = fill ? 0 : 1,
            Fill = fill ? _brush : Brushes.Transparent,
            Stretch = Stretch.Uniform,
            Width = size,
            Height = size,
            RenderTransform = txgrp,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Transitions = [
                new DoubleTransition
                {
                    Duration = AnimationDuration,
                    Property = OpacityProperty,
                }
            ],
            Data = Geometries[geomRng],
            ZIndex = ZIndex - 1,
        };

        if (RotationEnabled)
        {
            var rx = new RotateTransform
            {
                Transitions = [
                    new DoubleTransition
                    {
                        Duration = AnimationDuration,
                        Property = RotateTransform.AngleProperty,
                    }
                ],
            };
            txgrp.Children.Insert(0, rx);
        }

        _container.Children.Add(p);

        tx.X = rngX * SpreadMultiplier;
        tx.Y = rngY * SpreadMultiplier;
        p.Opacity = 0;

        if (RotationEnabled)
        {
            ((RotateTransform)txgrp.Children[0]).Angle = Math.Clamp(360 * 6 * Random.Shared.NextDouble(), 0, 360 * AnimationDuration.TotalMilliseconds / 1000f);
        }

        var t = new DispatcherTimer(AnimationDuration, DispatcherPriority.Background, (sender, ev) =>
        {
            _ = _container.Children.Remove(p);
            OnSparkleRemoved();
        });
    }

#pragma warning restore CA5394

    private void OnSparkleRemoved()
    {
        if (!_sparkling && _container.Children.Count == 0)
        {
            _ = Children.Remove(_container);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.Name == nameof(SparklesEnabled))
        {
            if (SparklesEnabled)
                StartSparkles();
            else
                StopSparkles();
        }
        else if (change.Property.Name == nameof(Color))
        {
            _brush = new SolidColorBrush(Color);
            ((DropShadowEffect)_container.Effect!).Color = Color;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (SparklesEnabled)
        {
            StartSparkles();
        }
    }

    private void StartSparkles()
    {
        if (_sparkling)
            return;

        _sparkling = true;

        if (!Children.Contains(_container))
            Children.Insert(0, _container);

        _timer.Start();
    }

    private void StopSparkles()
    {
        if (!_sparkling)
            return;

        _sparkling = false;
        _timer.Stop();
        OnSparkleRemoved();
    }
}
