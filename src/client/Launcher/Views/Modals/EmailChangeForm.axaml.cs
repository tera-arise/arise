using System.ComponentModel;
using Arise.Client.Launcher.Controllers.Modals;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Cue = Avalonia.Animation.Cue;

namespace Arise.Client.Launcher.Views.Modals;

public partial class EmailChangeForm : UserControl
{
    private double _step1Size;
    private double _step2Size;
    private EmailChangeModalController? _controller;

    public EmailChangeForm()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _step1Size = Step1Content.Bounds.Size.Height;
        _step2Size = Step2Content.Bounds.Size.Height;

        _controller = (EmailChangeModalController)DataContext!;

        if (_controller.IsChangeInProgress)
        {
            Step1Content.Height = 0;
        }
        else
        {
            Step2Content.Height = 0;
        }

        _controller.PropertyChanged += OnControllerPropertyChanged;
    }

    private void OnControllerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(EmailChangeModalController.IsChangeInProgress))
        {
            return;
        }

        var expanding = _controller!.IsChangeInProgress ? Step2Content : Step1Content;

        var collapsing = _controller!.IsChangeInProgress ? Step1Content : Step2Content;

        var time = TimeSpan.FromMilliseconds(200);

        var expandingAnim = new Animation()
        {
            Easing = new QuadraticEaseOut(),
            Duration = time,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(HeightProperty, 0d),
                        new Setter(OpacityProperty, .0d),
                    },
                    Cue = new Cue(0),
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(OpacityProperty, .0d),
                    },
                    Cue = new Cue(.3),
                },

                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(HeightProperty, _controller.IsChangeInProgress ? _step2Size : _step1Size),
                        new Setter(OpacityProperty, 1d),
                    },
                    Cue = new Cue(1),
                },
            },
        };

        var collapsingAnim = new Animation()
        {
            Duration = time,
            FillMode = FillMode.Forward,
            Easing = new QuadraticEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                         new Setter(HeightProperty, _controller.IsChangeInProgress ? _step1Size : _step2Size),
                         new Setter(OpacityProperty, 1d),
                    },
                    Cue = new Cue(0),
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(OpacityProperty, .0d),
                    },
                    Cue = new Cue(.7),
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(HeightProperty, 0d),
                        new Setter(OpacityProperty, .0d),
                    },
                    Cue = new Cue(1),
                },
            },
        };

        _ = expandingAnim.RunAsync(expanding)
           .ConfigureAwait(true);

        _ = collapsingAnim.RunAsync(collapsing)
            .ConfigureAwait(true);
    }

    private void EmailTextBox_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is InputElement s)
        {
            _ = s.Focus();
        }
    }

    private void Step1Content_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
    }

    private void Step2Content_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
    }
}
