// SPDX-License-Identifier: AGPL-3.0-or-later
using Arise.Client.Launcher.Controllers;

namespace Arise.Client.Launcher.Templates;

internal sealed class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data == null)
            return null;

        var name = data.GetType().FullName!.Replace("Controller", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        return type != null
            ? (Control)Activator.CreateInstance(type)!
            : new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewController;
    }
}
