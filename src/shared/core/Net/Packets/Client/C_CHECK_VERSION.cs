namespace Arise.Net.Packets.Client;

public sealed class C_CHECK_VERSION : TeraGamePacket
{
    public sealed class VersionInfo
    {
        public enum VersionKind
        {
            NetworkProtocol = 0,
            SystemMessages = 1,
        }

        public required VersionKind Kind { get; init; }

        public required int Value { get; init; }
    }

    public override TeraGamePacketCode Code => TeraGamePacketCode.C_CHECK_VERSION;

    public required List<VersionInfo> Versions { get; init; }
}
