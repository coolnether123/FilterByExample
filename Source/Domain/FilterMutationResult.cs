namespace FilterByExample.Domain
{
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
