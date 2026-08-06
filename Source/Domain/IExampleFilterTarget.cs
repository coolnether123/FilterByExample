namespace FilterByExample.Domain
{
    /// <summary>
    /// Separates filter policy from RimWorld storage objects so the mutation
    /// service can operate on stable identities and enforce parent constraints.
    /// </summary>
    public interface IExampleFilterTarget<TDefinition>
    {
        object Identity { get; }

        string Label { get; }

        bool Allows(TDefinition definition);

        bool CanAllow(TDefinition definition);

        void SetAllow(TDefinition definition, bool allow);
    }
}
