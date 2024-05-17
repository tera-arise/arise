// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public abstract class Entity
{
    public EntityId Id { get; }

    private protected Entity(EntityId id)
    {
        Id = id;
    }
}
