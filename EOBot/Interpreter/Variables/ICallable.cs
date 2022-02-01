using System.Threading.Tasks;

namespace EOBot.Interpreter.Variables
{
    public interface ICallable : IIdentifiable
    {
        void Call(params IIdentifiable[] parameters);
    }

    public interface ICallable<T> : IIdentifiable
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
