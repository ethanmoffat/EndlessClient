namespace EOBot.Interpreter.Variables
{
    public sealed class StringVariable : IVariable<string>
    {
        public string Value { get; }

        public StringVariable(string value) => Value = value;

        public string StringValue => Value;

        public string TypeName => Keywords.String;

        public EqualityResult LooseEquals(IVariable other) =>
            other is ICollectionVariable ? EqualityResult.IncompatibleComparison(this, other) : EqualityResult.Create(Equals(other));

        public bool StrictEquals(IVariable other) => other is StringVariable && Equals(other);

        public override bool Equals(object obj) => CompareTo(obj) == 0;

        public override int GetHashCode() => Value.GetHashCode();

        public int CompareTo(object obj) => obj is StringVariable ? string.CompareOrdinal(Value, ((StringVariable)obj).Value) : -1;

        public static explicit operator string(StringVariable variable) => variable.Value;

        public override string ToString() => StringValue;
    }
}
