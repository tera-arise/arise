// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Net;

public sealed class GameConnectionException : Exception
{
    public GameConnectionException(string message)
        : base(message)
    {
    }

    public GameConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
