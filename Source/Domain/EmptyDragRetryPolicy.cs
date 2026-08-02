namespace FilterByExample.Domain
{
    internal static class EmptyDragRetryPolicy
    {
        internal const int MaximumSpan = 5;

        internal static bool KeepActive(int width, int height)
        {
            return width <= MaximumSpan && height <= MaximumSpan;
        }
    }
}
