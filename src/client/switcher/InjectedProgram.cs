namespace Arise.Client.Switcher;

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

        return context.InjectorProcessId is { }
            ? SymbioteProgram.RunAsync(args, context.WakeUp)
            : LauncherProgram.RunAsync(args);
    }
}
