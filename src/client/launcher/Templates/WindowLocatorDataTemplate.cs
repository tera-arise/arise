using Arise.Client.Launcher.Controllers;
using Arise.Client.Launcher.Windows;

namespace Arise.Client.Launcher.Templates;

[SuppressMessage("", "CA1812")]
internal sealed class WindowLocatorDataTemplate : IDataTemplate
{
    private static readonly Assembly _assembly = typeof(ThisAssembly).Assembly;

    private static readonly string _namespace = typeof(MainWindow).Namespace!;

    public bool Match(object? data)
    {
        return data is LauncherController;
    }

    public Control Build(object? param)
    {
        return Unsafe.As<Control>(
            Activator.CreateInstance(
                _assembly.GetType(
                    $"{_namespace}.{param!.GetType().Name[..^"Controller".Length]}Window", throwOnError: true)!)!);
    }
}
