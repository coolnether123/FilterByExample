using System;
using System.Collections.Generic;
using System.Linq;
using FilterByExample.Definitions;
using FilterByExample.Domain;
using FilterByExample.Runtime;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterByExample.Presentation
{
    internal sealed class Designator_FilterByExample : Designator
    {
        private static readonly FilterByExampleService<ThingDef> Service =
            new FilterByExampleService<ThingDef>();

        private readonly IReadOnlyList<ThingDef> definitions;
        private readonly ExampleFilterOperation operation;
        private readonly List<IntVec3> previewCells = new List<IntVec3>();
        private readonly HashSet<object> seenTargetIdentities =
            new HashSet<object>(ReferenceIdentityComparer.Instance);

        private IntVec3 cachedMinimum = IntVec3.Invalid;
        private IntVec3 cachedMaximum = IntVec3.Invalid;
        private int cachedCellCount = -1;

        protected override bool DoTooltip => true;

        public override DrawStyleCategoryDef DrawStyleCategory =>
            FilterByExampleDefOf.FilterByExample_AffectedStorageArea;

        public Designator_FilterByExample(
            IReadOnlyList<ThingDef> definitions,
            ExampleFilterOperation operation,
            string label,
            string description,
            Texture iconTexture,
            KeyBindingDef keyBinding)
        {
            this.definitions = definitions ?? Array.Empty<ThingDef>();
            this.operation = operation;
            defaultLabel = label;
            defaultDesc = description;
            icon = iconTexture;
            hotKey = keyBinding;
            Order = float.MaxValue;
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_ZoneAdd_Stockpile;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 cell)
        {
            return cell.InBounds(Map);
        }

        public override void DesignateSingleCell(IntVec3 cell)
        {
            ApplyCells(new[] { cell });
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            List<IntVec3> selectedCells = cells?.ToList() ??
                new List<IntVec3>();
            ApplyCells(selectedCells);
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            RefreshPreview(dragCells);
            for (int index = 0; index < previewCells.Count; index++)
            {
                Graphics.DrawMesh(
                    MeshPool.plane10,
                    previewCells[index].ToVector3ShiftedWithAltitude(
                        AltitudeLayer.MetaOverlays.AltitudeFor()),
                    Quaternion.identity,
                    DesignatorUtility.DragHighlightCellMat,
                    0);
            }
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }

        private void ApplyCells(IReadOnlyList<IntVec3> cells)
        {
            List<StorageFilterTarget> targets = ResolveActionableTargets(
                cells,
                highlightCells: null);
            if (targets.Count == 0)
            {
                Messages.Message(
                    "FBE_NoValidTarget".Translate(),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                if (!IsSmallMiss(cells))
                {
                    Find.DesignatorManager.Deselect();
                }

                return;
            }

            if (cells.Count == 1 && targets.Count > 1)
            {
                OpenTargetChooser(targets);
                return;
            }

            ApplyTargets(targets);
        }

        private void OpenTargetChooser(
            IReadOnlyList<StorageFilterTarget> targets)
        {
            var options = new List<FloatMenuOption>(targets.Count);
            for (int index = 0; index < targets.Count; index++)
            {
                StorageFilterTarget target = targets[index];
                options.Add(new FloatMenuOption(
                    target.Label,
                    () => ApplyTargets(new[] { target })));
            }

            Find.DesignatorManager.Deselect();
            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void ApplyTargets(
            IReadOnlyList<StorageFilterTarget> targets)
        {
            FilterMutationResult result = Service.Apply(
                definitions,
                targets,
                operation);
            if (!result.Changed)
            {
                Messages.Message(
                    "FBE_NoChange".Translate(),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            string key;
            NamedArgument destination;
            if (result.ChangedTargetCount == 1)
            {
                key = operation == ExampleFilterOperation.Allow
                    ? "FBE_AllowedResult"
                    : "FBE_DisallowedResult";
                destination = targets[0].Label.Named("TARGET");
            }
            else
            {
                key = operation == ExampleFilterOperation.Allow
                    ? "FBE_AllowedBatchResult"
                    : "FBE_DisallowedBatchResult";
                destination = result.ChangedTargetCount.Named("TARGET");
            }

            Messages.Message(
                key.Translate(
                    result.ChangedDefinitionCount.Named("COUNT"),
                    destination),
                MessageTypeDefOf.TaskCompletion,
                historical: false);
            Finalize(somethingSucceeded: true);
            Find.DesignatorManager.Deselect();
        }

        private void RefreshPreview(IReadOnlyList<IntVec3> cells)
        {
            GetBounds(cells, out IntVec3 minimum, out IntVec3 maximum);
            if (cells.Count == cachedCellCount &&
                minimum == cachedMinimum &&
                maximum == cachedMaximum)
            {
                return;
            }

            cachedCellCount = cells.Count;
            cachedMinimum = minimum;
            cachedMaximum = maximum;
            previewCells.Clear();
            ResolveActionableTargets(cells, previewCells);
        }

        private List<StorageFilterTarget> ResolveActionableTargets(
            IReadOnlyList<IntVec3> cells,
            ICollection<IntVec3> highlightCells)
        {
            var targets = new List<StorageFilterTarget>();
            seenTargetIdentities.Clear();
            for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                bool cellHasTarget = false;
                List<StorageFilterTarget> cellTargets =
                    StorageTargetResolver.Resolve(cells[cellIndex], Map);
                for (int targetIndex = 0;
                    targetIndex < cellTargets.Count;
                    targetIndex++)
                {
                    StorageFilterTarget target = cellTargets[targetIndex];
                    bool actionable = false;
                    for (int definitionIndex = 0;
                        definitionIndex < definitions.Count;
                        definitionIndex++)
                    {
                        if (Service.CanApply(
                            definitions[definitionIndex],
                            target,
                            operation))
                        {
                            actionable = true;
                            break;
                        }
                    }

                    if (!actionable)
                    {
                        continue;
                    }

                    cellHasTarget = true;
                    if (seenTargetIdentities.Add(target.Identity))
                    {
                        targets.Add(target);
                    }
                }

                if (cellHasTarget)
                {
                    highlightCells?.Add(cells[cellIndex]);
                }
            }

            return targets;
        }

        private static bool IsSmallMiss(IReadOnlyList<IntVec3> cells)
        {
            GetBounds(cells, out IntVec3 minimum, out IntVec3 maximum);
            int width = maximum.IsValid ? maximum.x - minimum.x + 1 : 0;
            int height = maximum.IsValid ? maximum.z - minimum.z + 1 : 0;
            return EmptyDragRetryPolicy.KeepActive(width, height);
        }

        private static void GetBounds(
            IReadOnlyList<IntVec3> cells,
            out IntVec3 minimum,
            out IntVec3 maximum)
        {
            minimum = IntVec3.Invalid;
            maximum = IntVec3.Invalid;
            for (int index = 0; index < cells.Count; index++)
            {
                IntVec3 cell = cells[index];
                if (!minimum.IsValid)
                {
                    minimum = cell;
                    maximum = cell;
                    continue;
                }

                minimum.x = Math.Min(minimum.x, cell.x);
                minimum.z = Math.Min(minimum.z, cell.z);
                maximum.x = Math.Max(maximum.x, cell.x);
                maximum.z = Math.Max(maximum.z, cell.z);
            }
        }
    }
}
