using Arise.Tools.Distributor;

using var parser = new Parser(settings =>
{
    settings.GetoptMode = true;
    settings.PosixlyCorrect = true;
    settings.CaseSensitive = false;
    settings.CaseInsensitiveEnumValues = true;
    settings.HelpWriter = Terminal.StandardError.TextWriter;
});

return await parser
    .ParseArguments<DistributorOptions>(args)
    .MapResult(
        async options =>
        {
            await ClientDistributor.DistributeAsync(options);

            return 0;
        },
        _ => Task.FromResult(1));
