namespace Arise.Tools.Patcher;

internal static class Program
{
    [SuppressMessage("", "CA1031")]
    private static async Task<int> Main(string[] args)
    {
        try
        {
            TaskScheduler.UnobservedTaskException += static (_, e) => throw e.Exception;

            using var parser = new Parser(static settings =>
            {
                settings.GetoptMode = true;
                settings.PosixlyCorrect = true;
                settings.CaseSensitive = false;
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = Terminal.StandardError.TextWriter;
            });

            return await parser
                .ParseArguments<PatcherOptions>(args)
                .MapResult(
                    static async options =>
                    {
                        await ClientPatcher.PatchAsync(options);

                        return 0;
                    },
                    static _ => Task.FromResult(1));
        }
        catch (Exception ex)
        {
            await Terminal.ErrorLineAsync($"Error: {ex.Message}");
            await Terminal.ErrorLineAsync(ex.StackTrace);

            return 1;
        }
    }
}
