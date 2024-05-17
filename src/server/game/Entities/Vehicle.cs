// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class Vehicle : Unit
{
    internal Vehicle(int id)
        : base(new(EntityType.Vehicle, id))
    {
    }
}
