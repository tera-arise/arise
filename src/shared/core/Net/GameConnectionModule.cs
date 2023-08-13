using Arise.Bridge;

namespace Arise.Net;

internal sealed class GameConnectionModule : IDisposable
{
    private sealed class ModuleAssemblyLoadContext : AssemblyLoadContext
    {
        public ModuleAssemblyLoadContext()
            : base(isCollectible: true)
        {
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // Load dependencies in the default context.
            return null;
        }
    }

    public ReadOnlyMemory<byte> Raw { get; }

    public BridgeModule Instance { get; }

    private readonly ModuleAssemblyLoadContext _context = new();

    public GameConnectionModule(ReadOnlyMemory<byte> raw)
    {
        using var stream = new SlimMemoryStream
        {
            Buffer = MemoryMarshal.AsMemory(raw),
        };

        Raw = raw.ToArray();
        Instance = Unsafe.As<BridgeModule>(
            Activator.CreateInstance(
                _context
                    .LoadFromStream(stream)
                    .DefinedTypes
                    .Single(static t => t.BaseType == typeof(BridgeModule)))!);
    }

    public void Dispose()
    {
        _context.Unload();
    }
}
