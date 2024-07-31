namespace EOBot.Interpreter.Variables
{
    public class UndefinedVariable : IVariable
    {
        public static UndefinedVariable Instance { get; } = new UndefinedVariable();

        public string StringValue => "Undefined";

        private UndefinedVariable() { }

        public int CompareTo(object obj) => obj is UndefinedVariable ? 0 : -1;

        public override string ToString() => StringValue;
    }
}