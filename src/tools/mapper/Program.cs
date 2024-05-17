// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Tools.Mapper;

internal static class Program
{
    [SuppressMessage("", "CA1031")]
    private static async Task<int> Main(string[] args)
    {
        try
        {
            TaskScheduler.UnobservedTaskException += static (_, e) => ExceptionDispatchInfo.Throw(e.Exception);

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
                    static async options =>
                    {
                        options.GeometryDirectory.Create();

                        await GeometryDataConverter.ConvertAsync(options);
                        await PathDataConverter.ConvertAsync(options);

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
