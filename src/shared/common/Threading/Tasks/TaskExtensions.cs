// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Threading.Tasks;

public static class TaskExtensions
{
    [SuppressMessage("", "VSTHRD200")]
    public static async Task PreserveAggregateException(this Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch
        {
            if (task.Exception is { } ex)
                ExceptionDispatchInfo.Capture(ex).Throw();

            throw;
        }
    }
}
