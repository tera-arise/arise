using dnlib.DotNet;

namespace Arise.Server.Bridge;

[SuppressMessage("", "CA1812")]
internal sealed partial class BridgeModuleProvider : BackgroundService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Generated {Count} bridge modules in {ElapsedMs:0.0000} ms")]
        public static partial void GeneratedModules(
            Microsoft.Extensions.Logging.ILogger logger, int count, double elapsedMs);
    }

    private static readonly ReadOnlyMemory<BridgeModulePass> _passes = new BridgeModulePass[]
    {
        new BridgeModulePatchingPass(),
        new BridgeModuleOptimizationPass(),
        new BridgeModuleObfuscationPass(),
    };

    private readonly IOptions<WorldOptions> _options;

    private readonly ILogger<BridgeModuleProvider> _logger;

    private readonly List<(BridgeModule Server, ReadOnlyMemory<byte> Client)> _modules = new();

    public BridgeModuleProvider(IOptions<WorldOptions> options, ILogger<BridgeModuleProvider> logger)
    {
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ReadOnlyMemory<byte> CreateModule(BridgeModuleKind kind, int seed)
        {
            var module = ModuleDefMD.Load(typeof(PatchableBridgeModule).Module);
            var rng = new Random(seed);

            foreach (var pass in _passes.Span)
                pass.Run(module, kind, rng, _options.Value);

            using var stream = new MemoryStream();

            module.Write(stream);

            return stream.ToArray();
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var stamp = Stopwatch.GetTimestamp();

            lock (_modules)
            {
                _modules.Clear();

                for (var i = 0; i < _options.Value.ConcurrentModules; i++)
                {
                    var seed = Environment.TickCount;

                    _modules.Add(
                        (BridgeModuleActivator.Activate(CreateModule(BridgeModuleKind.Server, seed)),
                         CreateModule(BridgeModuleKind.Client, seed)));
                }
            }

            Log.GeneratedModules(
                _logger, _options.Value.ConcurrentModules, Stopwatch.GetElapsedTime(stamp).TotalMilliseconds);

            await Task.Delay(_options.Value.ModuleRotationTime.ToTimeSpan(), stoppingToken);
        }
    }

    [SuppressMessage("", "CA5394")]
    public (BridgeModule Server, ReadOnlyMemory<byte> Client) GetRandomModulePair()
    {
        lock (_modules)
            return _modules[Random.Shared.Next(_modules.Count)];
    }
}