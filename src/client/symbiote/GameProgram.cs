namespace Arise.Client.Symbiote;

public static class GameProgram
{
    public static Task<int> RunAsync(ReadOnlyMemory<string> args, int ppid, Action waker)
    {
        // TODO
        _ = (args, ppid);

        waker();

        return Task.FromResult(0);
    }
}
