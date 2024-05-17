// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Tools.Packer.Passes;

internal abstract class DataCenterPass
{
    public abstract void Run(DataCenterNode root, Action<DataCenterNode, string> error);
}
