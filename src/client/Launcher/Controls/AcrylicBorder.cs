// SPDX-License-Identifier: AGPL-3.0-or-later
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Arise.Client.Launcher.Controls;

public class AcrylicBorder : TemplatedControl
{
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<AcrylicBorder, BoxShadows>(nameof(BoxShadow));

    public static readonly StyledProperty<ExperimentalAcrylicMaterial> MaterialProperty =
        AvaloniaProperty.Register<AcrylicBorder, ExperimentalAcrylicMaterial>(nameof(Material));

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    public ExperimentalAcrylicMaterial Material
    {
        get => GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }
}
