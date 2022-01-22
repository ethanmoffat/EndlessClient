namespace EOBot.Interpreter.Variables
{
    public class IntVariable : IVariable<int>
    {
        public int Value { get; }

        public IntVariable(int value) => Value = value;

        public string StringValue => Value.ToString();

        public IVariable<int> WithNewValue(int value) => new IntVariable(value);

        public static explicit operator int(IntVariable input)
        {
            return input.Value;
        }
    }
}
