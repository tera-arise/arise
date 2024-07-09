// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Client.Launcher.Controllers;
using Arise.Client.Launcher.Windows;

namespace Arise.Client.Launcher.Templates;

internal sealed class WindowLocatorDataTemplate : IDataTemplate
{
    private static readonly Assembly _assembly = typeof(ThisAssembly).Assembly;

    private static readonly string _namespace = typeof(MainWindow).Namespace!;

    private readonly IServiceProvider _services;

    public WindowLocatorDataTemplate(IServiceProvider services)
    {
        _services = services;
    }

    public bool Match(object? data)
    {
        return data is LauncherController 
            && data!.GetType().FullName!.Contains("Window", StringComparison.Ordinal);
    }

    public Control Build(object? param)
    {
        return Unsafe.As<Control>(
            _services.GetRequiredService(
                _assembly.GetType(
                    $"{_namespace}.{param!.GetType().Name[..^"Controller".Length]}Window", throwOnError: true)!));
    }
}
