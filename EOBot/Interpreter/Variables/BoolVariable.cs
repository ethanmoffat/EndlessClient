namespace EOBot.Interpreter.Variables
{
    public class BoolVariable : IVariable<bool>
    {
        public bool Value { get; }

        public BoolVariable(bool value) => Value = value;

        public string StringValue => Value.ToString();

        public IVariable<bool> WithNewValue(bool value) => new BoolVariable(value);

        public int CompareTo(object obj) => Value.CompareTo(obj);

        public static explicit operator bool(BoolVariable input) => input.Value;
    }
}
