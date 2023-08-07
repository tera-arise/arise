namespace Arise.Tools.Mapper;

internal static class Program
{
    [SuppressMessage("", "CA1031")]
    private static async Task<int> Main(string[] args)
    {
        try
        {
            using var parser = new Parser(static settings =>
            {
                settings.GetoptMode = true;
                settings.PosixlyCorrect = true;
                settings.CaseSensitive = false;
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = Terminal.StandardError.TextWriter;
            });

            return await parser
                .ParseArguments<MapperOptions>(args)
                .MapResult(
                    static options =>
                    {
                        // TODO: Implement map/geometry extraction.

                        return Task.FromResult(0);
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
