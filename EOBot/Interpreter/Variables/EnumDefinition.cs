using System;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.Variables
{
    public class EnumDefinition : IIdentifiable
    {
        private readonly Dictionary<string, int> _members;

        public string Name { get; }

        public IReadOnlyList<(string Name, int Value)> Members { get; }

        public string StringValue => $"enum {Name} {{ {string.Join(", ", Members.Select(x => $"{x.Name} = {x.Value}"))} }}";

        public EnumDefinition(string name, IReadOnlyList<(string Name, int Value)> members)
        {
            Name = name;
            Members = members;
            _members = members.ToDictionary(x => x.Name, x => x.Value);
        }

        public static EnumDefinition FromType(Type enumType, string alias = null)
        {
            var members = Enum.GetNames(enumType)
                .Select(x => (x, (int)Enum.Parse(enumType, x)))
                .ToList();
            return new EnumDefinition(alias ?? enumType.Name, members);
        }

        public bool TryGetMember(string memberName, out EnumVariable value)
        {
            value = _members.TryGetValue(memberName, out var intValue)
                ? new EnumVariable(Name, memberName, intValue)
                : null;
            return value != null;
        }

        public override string ToString() => StringValue;
    }
}
