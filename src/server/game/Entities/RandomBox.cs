// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class RandomBox : Object
{
    internal RandomBox(int id)
        : base(new(EntityType.RandomBox, id))
    {
    }
}
