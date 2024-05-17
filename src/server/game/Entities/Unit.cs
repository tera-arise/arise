// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public abstract class Unit : Entity
{
    private protected Unit(EntityId id)
        : base(id)
    {
    }
}
