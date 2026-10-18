using System.Collections.Generic;

namespace EOBot.Interpreter.Variables
{
    public sealed class EnumVariable : IntVariable
    {
        public const string NAME_MEMBER = "name";
        public const string VALUE_MEMBER = "value";
        public const string TYPE_MEMBER = "type";

        public EnumDefinition Definition { get; }

        public string EnumName => Definition?.Name;

        public string MemberName { get; }

        public override string StringValue => $"{EnumName}::{MemberName}";

        public override string TypeName => EnumName;

        public EnumVariable(EnumDefinition definition, string memberName, int value)
            : base(value)
        {
            Definition = definition;
            MemberName = memberName;
        }

        /// <summary>
        /// Creates a variable representing an enum type (used as the right-hand side of an 'is' expression). A null definition matches any enum.
        /// </summary>
        public static EnumVariable TypeSpecifier(EnumDefinition definition) => new(definition, null, default);

        public bool IsInstanceOf(EnumVariable typeSpecifier) => typeSpecifier.Definition == null || Definition.IsSameType(typeSpecifier.Definition);

        public override bool StrictEquals(IVariable other) => other is EnumVariable otherEnum && Definition.IsSameType(otherEnum.Definition) && Equals(other);

        public ObjectVariable ToObject() => new(new Dictionary<string, (bool, IIdentifiable)>
        {
            [NAME_MEMBER] = (true, new StringVariable(MemberName)),
            [VALUE_MEMBER] = (true, new IntVariable(Value)),
            [TYPE_MEMBER] = (true, new StringVariable(EnumName)),
        });
    }
}
