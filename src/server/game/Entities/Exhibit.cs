// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class Exhibit : Object
{
    internal Exhibit(int id)
        : base(new(EntityType.Exhibit, id))
    {
    }
}
