// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public abstract class Object : Entity
{
    private protected Object(EntityId id)
        : base(id)
    {
    }
}
