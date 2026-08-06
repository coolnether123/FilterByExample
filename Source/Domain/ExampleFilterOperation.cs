namespace FilterByExample.Domain
{
    /// <summary>
    /// Carries the user's filtering intent through shared preview and mutation
    /// paths, preventing allow and disallow from becoming separate workflows.
    /// </summary>
    public enum ExampleFilterOperation
    {
        Allow = 0,
        Disallow = 1
    }
}
