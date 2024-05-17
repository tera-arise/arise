// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class GuildTower : Fixture
{
    internal GuildTower(int id)
        : base(new EntityId(EntityType.GuildTower, id))
    {
    }
}
