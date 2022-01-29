namespace EOBot.Interpreter.Variables
{
    public class StringVariable : IVariable<string>
    {
        public string Value { get; }

        public StringVariable(string value) => Value = value;

        public string StringValue => Value;

        public IVariable<string> WithNewValue(string value) => new StringVariable(value);

        public override bool Equals(object obj) => CompareTo(obj) == 0;

        public int CompareTo(object obj) => obj is StringVariable ? Value.CompareTo(((StringVariable)obj).Value) : -1;

        public static explicit operator string(StringVariable variable) => variable.Value;

        public override string ToString() => StringValue;
    }
}
