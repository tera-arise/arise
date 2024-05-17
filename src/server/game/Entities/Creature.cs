// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public class Creature : Unit
{
    internal Creature(int id)
        : this(new EntityId(EntityType.Creature, id))
    {
    }

    private protected Creature(EntityId id)
        : base(id)
    {
    }
}
