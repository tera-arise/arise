namespace Arise.Module;

[SuppressMessage("", "CA1812")]
internal sealed class GameBridgeModule : BridgeModule
{
    public override GameDataComponent Data { get; } = new GameDataComponent();

    public override GameProtocolComponent Protocol { get; } = new GameProtocolComponent();

    public override GameWatchdogComponent Watchdog { get; } = new GameWatchdogComponent();
}
