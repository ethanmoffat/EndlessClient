using System;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.Variables
{
    public class EnumDefinition : IIdentifiable
    {
        private readonly Dictionary<string, int> _members;

        public string Name { get; }

        /// <summary>
        /// The .NET enum type this definition was mapped from, or null for script-defined enums.
        /// </summary>
        public Type DotNetType { get; }

        public IReadOnlyList<(string Name, int Value)> Members { get; }

        public string StringValue => $"enum {Name} {{ {string.Join(", ", Members.Select(x => $"{x.Name} = {x.Value}"))} }}";

        private EnumDefinition(string name, IReadOnlyList<(string Name, int Value)> members)
            : this(name, members, dotNetType: null) { }

        private EnumDefinition(string name, IReadOnlyList<(string Name, int Value)> members, Type dotNetType)
        {
            Name = name;
            DotNetType = dotNetType;
            Members = members;
            _members = members.ToDictionary(x => x.Name, x => x.Value);
        }

        public static EnumDefinition FromScriptDefinition(string name, IReadOnlyList<(string Name, int Value)> members)
        {
            return new EnumDefinition(name, members);
        }

        public static EnumDefinition FromType(Type enumType, string alias = null)
        {
            var members = Enum.GetNames(enumType)
                .Select(x => (x, (int)Enum.Parse(enumType, x)))
                .ToList();
            return new EnumDefinition(alias ?? enumType.Name, members, enumType);
        }

        public bool TryGetMember(string memberName, out EnumVariable value)
        {
            value = _members.TryGetValue(memberName, out var intValue)
                ? new EnumVariable(this, memberName, intValue)
                : null;
            return value != null;
        }

        /// <summary>
        /// Mapped .NET enums are the same type if they map the same .NET type (regardless of alias); script-defined enums are the same type if they have the same name.
        /// </summary>
        public bool IsSameType(EnumDefinition other) =>
            DotNetType != null && other.DotNetType != null
            ? DotNetType == other.DotNetType
            : Name == other.Name;

        public override string ToString() => StringValue;
    }
}
