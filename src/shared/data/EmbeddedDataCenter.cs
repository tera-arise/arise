// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Data;

public static class EmbeddedDataCenter
{
    public static Stream OpenStream()
    {
        return typeof(ThisAssembly).Assembly.GetManifestResourceStream("DataCenter.dat")!;
    }
}
