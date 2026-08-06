namespace FilterByExample.Domain
{
    /// <summary>
    /// Defines when an empty drag is probably a recoverable near miss, isolated
    /// from RimWorld UI state so the interaction boundary stays testable.
    /// </summary>
    internal static class EmptyDragRetryPolicy
    {
        internal const int MaximumSpan = 5;

        internal static bool KeepActive(int width, int height)
        {
            return width <= MaximumSpan && height <= MaximumSpan;
        }
    }
}
