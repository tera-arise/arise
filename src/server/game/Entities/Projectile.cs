// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class Projectile : Object
{
    internal Projectile(int id)
        : base(new(EntityType.Projectile, id))
    {
    }
}
