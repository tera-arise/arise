using Arise.Tools.Patcher;

using var parser = new Parser(settings =>
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
        async options =>
        {
            await ClientPatcher.PatchAsync(options);

            return 0;
        },
        _ => Task.FromResult(1));
