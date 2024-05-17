// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class Door : Object
{
    internal Door(int id)
        : base(new(EntityType.Door, id))
    {
    }
}
