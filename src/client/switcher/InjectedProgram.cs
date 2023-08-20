namespace Arise.Client.Switcher;

[SuppressMessage("", "CA1812")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/6218
internal sealed class InjectedProgram : IInjectedProgram
{
    public static Task<int> RunAsync(InjectedProgramContext context, ReadOnlyMemory<string> args)
    {
        TaskScheduler.UnobservedTaskException += static (_, e) =>
        {
            // TODO: https://github.com/dotnet/runtime/issues/80111
            if (!e.Exception.InnerExceptions.Any(static ex => ex is QuicException))
                ExceptionDispatchInfo.Throw(e.Exception);
        };

        return context.InjectorProcessId is { } ppid
            ? SymbioteProgram.RunAsync(args, ppid, context.WakeUp)
            : LauncherProgram.RunAsync(args);
    }
}
