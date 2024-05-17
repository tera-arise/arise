// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class Device : Object
{
    internal Device(int id)
        : base(new(EntityType.Device, id))
    {
    }
}
