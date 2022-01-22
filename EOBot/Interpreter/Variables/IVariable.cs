namespace EOBot.Interpreter.Variables
{
    public interface IVariable<T> : IIdentifiable
    {
        T Value { get; }

        IVariable<T> WithNewValue(T value);
    }
}
