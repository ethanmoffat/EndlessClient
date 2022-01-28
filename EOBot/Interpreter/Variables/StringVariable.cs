namespace EOBot.Interpreter.Variables
{
    public class StringVariable : IVariable<string>
    {
        public string Value { get; }

        public StringVariable(string value) => Value = value;

        public string StringValue => Value;

        public IVariable<string> WithNewValue(string value) => new StringVariable(value);

        public int CompareTo(object obj) => Value.CompareTo(obj);

        public static explicit operator string(StringVariable variable) => variable.Value;
    }
}
