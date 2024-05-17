// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class Statue : Object
{
    internal Statue(int id)
        : base(new(EntityType.Shuttle, id))
    {
    }
}
