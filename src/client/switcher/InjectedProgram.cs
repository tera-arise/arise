namespace Arise.Client.Switcher;

[SuppressMessage("", "CA1812")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/6218
internal sealed class InjectedProgram : IInjectedProgram
{
    public static Task<int> RunAsync(InjectedProgramContext context, ReadOnlyMemory<string> args)
    {
        return context.InjectorProcessId is int ppid
            ? GameProgram.RunAsync(args, ppid, context.WakeUp)
            : LauncherProgram.RunAsync(args);
    }
}
