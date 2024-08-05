using System.Threading.Tasks;

namespace EOBot.Interpreter.Variables
{
    public interface IFunction : IIdentifiable { }
    public interface ICallable : IFunction
    {
        void Call(params IIdentifiable[] parameters);
    }

    public interface ICallable<T> : IFunction
    {
        T Call(params IIdentifiable[] parameters);
    }

    public interface IAsyncFunction : IIdentifiable { }

    public interface IAsyncCallable : IAsyncFunction
    {
        Task CallAsync(params IIdentifiable[] parameters);
    }

    public interface IAsyncCallable<T> : IAsyncFunction
    {
        Task<T> CallAsync(params IIdentifiable[] parameters);
    }
}