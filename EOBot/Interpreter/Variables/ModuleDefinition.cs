using System.Collections.Generic;

namespace EOBot.Interpreter.Variables
{
    /// <summary>
    /// A named group of symbols whose members are accessed with '::', e.g. 'Math::Max(1, 2)'.
    /// </summary>
    public sealed class ModuleDefinition : IIdentifiable
    {
        public string Name { get; }

        public IReadOnlyDictionary<string, IIdentifiable> Members { get; }

        public string StringValue => Name;

        public ModuleDefinition(string name, IReadOnlyDictionary<string, IIdentifiable> members)
        {
            Name = name;
            Members = members;
        }
    }
}
