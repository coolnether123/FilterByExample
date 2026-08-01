namespace FilterByExample.Domain
{
    public interface IExampleFilterTarget<TDefinition>
    {
        object Identity { get; }

        string Label { get; }

        bool Allows(TDefinition definition);

        bool CanAllow(TDefinition definition);

        void SetAllow(TDefinition definition, bool allow);
    }
}
