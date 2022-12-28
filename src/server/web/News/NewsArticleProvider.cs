namespace Arise.Server.Web.News;

public sealed class NewsArticleProvider : IHostedService
{
    public IEnumerable<NewsArticle> Articles { get; private set; } = null!;

    private readonly IHostEnvironment _environment;

    public NewsArticleProvider(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var provider = _environment.ContentRootFileProvider;

        IEnumerable<IFileInfo> GetContents(params string[] names)
        {
            return provider.GetDirectoryContents(Path.Combine(names.Prepend("articles").ToArray()));
        }

        var articles = new List<NewsArticle>();
        var pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .DisableHtml()
            .UseBootstrap()
            .UseAutoIdentifiers(
                AutoIdentifierOptions.AutoLink |
                AutoIdentifierOptions.AllowOnlyAscii |
                AutoIdentifierOptions.GitHub)
            .UseEmojiAndSmiley()
            .UseEmphasisExtras()
            .UseListExtras()
            .UseGridTables()
            .UsePipeTables()
            .UseMediaLinks()
            .Build();

        foreach (var (year, month, day, file) in from yearDir in GetContents()
                                                 where yearDir.IsDirectory
                                                 let year = yearDir.Name
                                                 from monthDir in GetContents(year)
                                                 where monthDir.IsDirectory
                                                 let month = monthDir.Name
                                                 from dayDir in GetContents(year, month)
                                                 where dayDir.IsDirectory
                                                 let day = dayDir.Name
                                                 from articleFile in GetContents(year, month, day)
                                                 where !articleFile.IsDirectory
                                                 select (year, month, day, articleFile))
        {
            MarkdownDocument ast;
            string markdown;

            await using (var markdownStream = file.CreateReadStream())
            {
                using var markdownReader = new StreamReader(markdownStream, leaveOpen: true);

                markdown = await markdownReader.ReadToEndAsync(cancellationToken);
                ast = Markdown.Parse(markdown, pipeline);
            }

            var yamlStream = new YamlStream();
            var yamlBlock = ast.OfType<YamlFrontMatterBlock>().Single().Span;

            using (var yamlReader = new StringReader(markdown.Substring(yamlBlock.Start, yamlBlock.Length)))
                yamlStream.Load(yamlReader);

            var root = (YamlMappingNode)yamlStream.Documents[0].RootNode;

            articles.Add(new()
            {
                Date = new(int.Parse(year), int.Parse(month), int.Parse(day)),
                Slug = Path.GetFileNameWithoutExtension(file.Name),
                Title = (string)root["title"]!,
                Summary = (string)root["summary"]!,
                Content = new(ast.ToHtml(pipeline)),
            });
        }

        Articles = articles
            .OrderByDescending(article => article.Date)
            .ThenBy(article => article.Slug)
            .ToArray()
            .AsReadOnly();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
