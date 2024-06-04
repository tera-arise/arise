// SPDX-License-Identifier: AGPL-3.0-or-later
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Cue = Avalonia.Animation.Cue;

namespace Arise.Client.Launcher.Controls;

[TemplatePart("PART_StepContent", typeof(ContentPresenter))]
public class StepControl : UserControl
{
    private readonly Animation _expandingAnim;
    private readonly Animation _collapsingAnim;
    private readonly Setter _originalHeightSetter;

    private double _originalHeight;

    private ContentPresenter? _stepContent;

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<StepControl, string>("Header", defaultValue: "First step");

    public string StepId
    {
        get => GetValue(StepIdProperty);
        set => SetValue(StepIdProperty, value);
    }

    public static readonly StyledProperty<string> StepIdProperty =
        AvaloniaProperty.Register<StepControl, string>("StepId", defaultValue: "1");

    public bool Collapsed
    {
        get => GetValue(CollapsedProperty);
        set => SetValue(CollapsedProperty, value);
    }

    public static readonly StyledProperty<bool> CollapsedProperty =
        AvaloniaProperty.Register<StepControl, bool>("Collapsed", defaultValue: false);

    public StepControl()
    {
        var time = TimeSpan.FromMilliseconds(200);
        Debug.WriteLine(_originalHeight);
        _originalHeightSetter = new Setter(HeightProperty, _originalHeight);
        _expandingAnim = new Animation()
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
                        _originalHeightSetter,
                        new Setter(OpacityProperty, 1d),
                    },
                    Cue = new Cue(1),
                },
            },
        };

        _collapsingAnim = new Animation()
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
                        _originalHeightSetter,
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
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _stepContent = e.NameScope.Find<ContentPresenter>("PART_StepContent");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_stepContent == null)
            return;

        _originalHeight = _stepContent.Bounds.Size.Height;

        _originalHeightSetter.Value = _originalHeight;

        if (Collapsed)
        {
            _stepContent.Height = 0;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.Name == nameof(Collapsed) && _stepContent != null)
        {
            var anim = Collapsed ? _collapsingAnim : _expandingAnim;
            {
                _ = anim.RunAsync(_stepContent)
                   .ConfigureAwait(true);
            }
        }
    }
}
