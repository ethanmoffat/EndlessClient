namespace EOBot.Interpreter.Variables
{
    public interface ICallable :IIdentifiable
    {
        void Call(params IIdentifiable[] parameters);
    }

    public interface ICallable<T> : IIdentifiable
    {
        T Call(params IIdentifiable[] parameters);
    }
}
