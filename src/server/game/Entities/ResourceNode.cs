// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Entities;

public sealed class ResourceNode : Object
{
    internal ResourceNode(int id)
        : base(new(EntityType.ResourceNode, id))
    {
    }
}
