// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class Bonfire : Object
{
    internal Bonfire(int id)
        : base(new(EntityType.Bonfire, id))
    {
    }
}
