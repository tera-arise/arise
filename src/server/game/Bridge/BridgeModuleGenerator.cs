// SPDX-License-Identifier: AGPL-3.0-or-later

using dnlib.DotNet;

namespace Arise.Server.Bridge;

[RegisterSingleton<BridgeModuleGenerator>]
[SuppressMessage("", "CA1001")]
internal sealed partial class BridgeModuleGenerator : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Generated {Count} bridge modules in {ElapsedMs:0.0000} ms")]
        public static partial void GeneratedBridgeModules(
            ILogger<BridgeModuleGenerator> logger, int count, double elapsedMs);
    }

    private static readonly ReadOnlyMemory<BridgeModulePass> _passes = new BridgeModulePass[]
    {
        new BridgeModulePatchingPass(),
        new BridgeModuleOptimizationPass(),
        new BridgeModuleObfuscationPass(),
    };

    private readonly CancellationTokenSource _cts = new();

    private readonly TaskCompletionSource _generateDone = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly List<(BridgeModule Server, ReadOnlyMemory<byte> Client)> _modules = [];

    private readonly IHostEnvironment _environment;

    private readonly IOptions<GameOptions> _options;

    private readonly ILogger<BridgeModuleGenerator> _logger;

    private readonly TimeProvider _timeProvider;

    public BridgeModuleGenerator(
        IHostEnvironment environment,
        IOptions<GameOptions> options,
        ILogger<BridgeModuleGenerator> logger,
        TimeProvider timeProvider)
    {
        _environment = environment;
        _options = options;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var ready = new TaskCompletionSource();
        var ct = _cts.Token;

        _ = Task.Run(() => GenerateModulesAsync(ready, ct), ct);

        await ready.Task;
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        // Signal the generator task to shut down.
        await _cts.CancelAsync();

        // Note that the generator task is not expected to encounter any exceptions.
        await _generateDone.Task;

        // The task is done; safe to dispose this now.
        _cts.Dispose();
    }

    private async Task GenerateModulesAsync(TaskCompletionSource ready, CancellationToken cancellationToken)
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

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var stopwatch = SlimStopwatch.Create();

                lock (_modules)
                {
                    _modules.Clear();

                    var clientKind = _environment.IsDevelopment() ? BridgeModuleKind.Normal : BridgeModuleKind.Hardened;

                    for (var i = 0; i < _options.Value.ConcurrentModules; i++)
                    {
                        var seed = Environment.TickCount;

                        _modules.Add(
                            (BridgeModuleActivator.Create(CreateModule(BridgeModuleKind.Normal, seed)),
                             CreateModule(clientKind, seed)));
                    }
                }

                Log.GeneratedBridgeModules(
                    _logger, _options.Value.ConcurrentModules, stopwatch.Elapsed.TotalMilliseconds);

                // Signal that we have an initial set of modules so startup can continue.
                _ = ready.TrySetResult();

                await Task.Delay(_options.Value.ModuleRotationTime, _timeProvider, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // StopAsync() was called.
        }
        finally
        {
            _generateDone.SetResult();
        }
    }

    [SuppressMessage("", "CA5394")]
    public (BridgeModule Server, ReadOnlyMemory<byte> Client) GetRandomModulePair()
    {
        lock (_modules)
            return _modules[Random.Shared.Next(_modules.Count)];
    }
}
