namespace FilterByExample.Domain
{
    /// <summary>
    /// Summarizes a batch without exposing mutation internals, allowing the UI
    /// to report blocked, unchanged, and successful operations accurately.
    /// </summary>
    public readonly struct FilterMutationResult
    {
        public FilterMutationResult(
            int selectedDefinitionCount,
            int targetCount,
            int changedTargetCount,
            int changedDefinitionCount,
            int blockedDefinitionCount)
        {
            SelectedDefinitionCount = selectedDefinitionCount;
            TargetCount = targetCount;
            ChangedTargetCount = changedTargetCount;
            ChangedDefinitionCount = changedDefinitionCount;
            BlockedDefinitionCount = blockedDefinitionCount;
        }

        public int SelectedDefinitionCount { get; }

        public int TargetCount { get; }

        public int ChangedTargetCount { get; }

        public int ChangedDefinitionCount { get; }

        public int BlockedDefinitionCount { get; }

        public bool Changed => ChangedDefinitionCount > 0;
    }
}
