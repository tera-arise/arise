// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Diagnostics;

[StackTraceHidden]
public static class Assert
{
    public static void Always(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))] string expression = "")
    {
        if (!condition)
            throw new AssertionException(expression);
    }

    [Conditional("DEBUG")]
    public static void Debug(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))] string expression = "")
    {
        Always(condition, expression);
    }
}
