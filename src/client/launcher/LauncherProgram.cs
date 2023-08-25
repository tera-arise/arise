using static Windows.Win32.WindowsPInvoke;

namespace Arise.Client.Launcher;

public static class LauncherProgram
{
    public static Task<int> RunAsync(ReadOnlyMemory<string> args)
    {
        // We build for the Windows GUI subsystem, so no console output will appear. This is not very helpful. Try
        // attaching to the parent process's console if it exists.
        _ = AttachConsole(ATTACH_PARENT_PROCESS);

        // Note that we switch to ShutdownMode.OnExplicitShutdown while the game is running.
        return Task.FromResult(
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args.ToArray(), ShutdownMode.OnMainWindowClose));
    }

    // Used by the Avalonia designer.
    internal static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<LauncherApplication>()
            .UseWin32()
            .UseSkia()
            .UseReactiveUI()
            .WithInterFont();
    }
}
