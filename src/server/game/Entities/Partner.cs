// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class Partner : Companion
{
    internal Partner(int id)
        : base(new EntityId(EntityType.Partner, id))
    {
    }
}
