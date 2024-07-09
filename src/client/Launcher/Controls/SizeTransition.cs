using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Cue = Avalonia.Animation.Cue;

namespace Arise.Client.Launcher.Controls;

public class SizeTransition : IPageTransition
{
    public double SizeFactor { get; set; }

    public Easing SlideInEasing { get; set; } = new LinearEasing();

    public Easing SlideOutEasing { get; set; } = new LinearEasing();

    public TimeSpan Duration { get; set; }

    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var tasks = new List<Task>();

        if (from != null)
        {
            var animation = new Animation
            {
                Easing = SlideOutEasing,
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter { Property = ScaleTransform.ScaleXProperty, Value = 1d },
                            new Setter { Property = ScaleTransform.ScaleYProperty, Value = 1d },
                            new Setter { Property = TranslateTransform.YProperty, Value = 0d },
                        },
                        Cue = new Cue(0d),
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = ScaleTransform.ScaleXProperty,
                                Value = SizeFactor,
                            },
                            new Setter
                            {
                                Property = ScaleTransform.ScaleYProperty,
                                Value = SizeFactor,
                            },
                            new Setter
                            {
                                Property = TranslateTransform.YProperty,
                                Value = -50d,
                            },
                        },
                        Cue = new Cue(1d),
                    },
                },
                Duration = Duration,
            };
            tasks.Add(animation.RunAsync(from, cancellationToken));
        }

        if (to != null)
        {
            to.IsVisible = true;
            var animation = new Animation
            {
                Easing = SlideInEasing,
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = ScaleTransform.ScaleXProperty,
                                Value = SizeFactor,
                            },
                            new Setter
                            {
                                Property = ScaleTransform.ScaleYProperty,
                                Value = SizeFactor,
                            },
                            new Setter
                            {
                                Property = TranslateTransform.YProperty,
                                Value = 50d,
                            },
                        },
                        Cue = new Cue(0d),
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter { Property = ScaleTransform.ScaleXProperty, Value = 1d },
                            new Setter { Property = ScaleTransform.ScaleYProperty, Value = 1d },
                            new Setter { Property = TranslateTransform.YProperty, Value = 0d },
                        },
                        Cue = new Cue(1d),
                    },
                },
                Duration = Duration,
            };
            tasks.Add(animation.RunAsync(to, cancellationToken));
        }

        await Task.WhenAll(tasks)
            .ConfigureAwait(true);

        if (from != null && !cancellationToken.IsCancellationRequested)
        {
            from.IsVisible = false;
        }
    }
}
