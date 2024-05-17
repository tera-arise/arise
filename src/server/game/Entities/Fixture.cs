// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public class Fixture : Unit
{
    internal Fixture(int id)
        : this(new EntityId(EntityType.Fixture, id))
    {
    }

    private protected Fixture(EntityId id)
        : base(id)
    {
    }
}
