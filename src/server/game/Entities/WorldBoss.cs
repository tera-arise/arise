// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class WorldBoss : Boss
{
    internal WorldBoss(int id)
        : base(new EntityId(EntityType.WorldBoss, id))
    {
    }
}
