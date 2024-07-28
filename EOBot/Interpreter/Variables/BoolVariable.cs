namespace EOBot.Interpreter.Variables;

public class BoolVariable : IVariable<bool>
{
    public bool Value { get; }

    public BoolVariable(bool value) => Value = value;

    public string StringValue => Value.ToString();

    public override bool Equals(object obj) => CompareTo(obj) == 0;

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(object obj) => obj is BoolVariable ? Value.CompareTo(((BoolVariable)obj).Value) : -1;

    public static explicit operator bool(BoolVariable input) => input.Value;

    public override string ToString() => StringValue;
}