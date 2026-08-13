using UnityEngine;
using Verse;

namespace FilterByExample.Presentation
{
    /// <summary>
    /// Draws the settings-free full-row highlight used by the storage filter
    /// preview, following Better Work Tab's translucent row-fill pattern.
    /// </summary>
    internal static class HighlightDrawer
    {
        private static readonly Color TargetThingDefColor =
            // Matches Better Work Tab Dev's float-menu highlight vocabulary.
            new Color(0.114f, 0.737f, 0.737f, 0.5f);

        internal static void DrawHighlight(Rect rowRect)
        {
            Widgets.DrawBoxSolid(rowRect, TargetThingDefColor);
        }
    }
}
