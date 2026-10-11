namespace EOBot.Interpreter.Variables
{
    public sealed class UndefinedVariable : IVariable
    {
        public static UndefinedVariable Instance { get; } = new UndefinedVariable();

        public string StringValue => "Undefined";

        public string TypeName => Keywords.Undefined;

        private UndefinedVariable() { }

        public int CompareTo(object obj) => obj is UndefinedVariable ? 0 : -1;

        public EqualityResult LooseEquals(IVariable other) => EqualityResult.Create(StrictEquals(other));

        public bool StrictEquals(IVariable other) => other is UndefinedVariable;

        public override string ToString() => StringValue;
    }
}
