namespace EOBot.Interpreter.Variables;

public class IntVariable : IVariable<int>
{
    public int Value { get; }

    public IntVariable(int value) => Value = value;

    public string StringValue => Value.ToString();

    public override bool Equals(object obj) => CompareTo(obj) == 0;

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(object obj) => obj is IntVariable ? Value.CompareTo(((IntVariable)obj).Value) : -1;

    public static explicit operator int(IntVariable input) => input.Value;

    public override string ToString() => StringValue;
}