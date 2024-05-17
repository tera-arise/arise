// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class Shuttle : Object
{
    internal Shuttle(int id)
        : base(new(EntityType.Shuttle, id))
    {
    }
}
