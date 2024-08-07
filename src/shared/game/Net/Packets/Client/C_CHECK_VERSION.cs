// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Net.Packets.Client;

public sealed class C_CHECK_VERSION : TeraGamePacket
{
    public readonly struct VersionInfo
    {
        public enum VersionKind
        {
            NetworkProtocol = 0,
            SystemMessages = 1,
        }

        public required VersionKind Kind { get; init; }

        public required int Value { get; init; }
    }

    public override GamePacketCode Code => GamePacketCode.C_CHECK_VERSION;

    public required ImmutableArray<VersionInfo> Versions { get; init; }
}
