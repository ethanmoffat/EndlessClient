using System.Collections.Generic;

namespace EOBot.Interpreter.Variables
{
    public class EnumVariable : IntVariable
    {
        public const string NAME_MEMBER = "name";
        public const string VALUE_MEMBER = "value";
        public const string TYPE_MEMBER = "type";

        public string EnumName { get; }

        public string MemberName { get; }

        public override string StringValue => $"{EnumName}::{MemberName}";

        public EnumVariable(string enumName, string memberName, int value)
            : base(value)
        {
            EnumName = enumName;
            MemberName = memberName;
        }

        /// <summary>
        /// Creates a variable representing an enum type (used as the right-hand side of an 'is' expression). A null enum name matches any enum.
        /// </summary>
        public static EnumVariable TypeSpecifier(string enumName) => new(enumName, null, default);

        public bool IsInstanceOf(EnumVariable typeSpecifier) => typeSpecifier.EnumName == null || typeSpecifier.EnumName == EnumName;

        public override int CompareTo(object obj) => obj is EnumVariable other && other.EnumName != EnumName ? -1 : base.CompareTo(obj);

        public override bool Equals(object obj) => CompareTo(obj) == 0;

        public override int GetHashCode() => base.GetHashCode();

        public ObjectVariable ToObject() => new(new Dictionary<string, (bool, IIdentifiable)>
        {
            [NAME_MEMBER] = (true, new StringVariable(MemberName)),
            [VALUE_MEMBER] = (true, new IntVariable(Value)),
            [TYPE_MEMBER] = (true, new StringVariable(EnumName)),
        });
    }
}
