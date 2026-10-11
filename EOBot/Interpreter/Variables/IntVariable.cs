namespace EOBot.Interpreter.Variables
{
    public class IntVariable : IVariable<int>
    {
        public int Value { get; }

        public IntVariable(int value) => Value = value;

        public virtual string StringValue => Value.ToString();

        public virtual string TypeName => Keywords.Int;

        public EqualityResult LooseEquals(IVariable other) =>
            other is ICollectionVariable ? EqualityResult.IncompatibleComparison(this, other) : EqualityResult.Create(Equals(other));

        public bool StrictEquals(IVariable other) => other.GetType() == GetType() && Equals(other);

        public override bool Equals(object obj) => CompareTo(obj) == 0;

        public override int GetHashCode() => Value.GetHashCode();

        public virtual int CompareTo(object obj) => obj is IntVariable ? Value.CompareTo(((IntVariable)obj).Value) : -1;

        public static explicit operator int(IntVariable input) => input.Value;

        public override string ToString() => StringValue;
    }
}
