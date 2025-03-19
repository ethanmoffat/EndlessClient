using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.States;

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
        Task CallAsync(CancellationToken ct, params IIdentifiable[] parameters);
    }

    public interface IAsyncCallable<T> : IAsyncFunction
    {
        Task<T> CallAsync(CancellationToken ct, params IIdentifiable[] parameters);
    }

    public interface IUserDefinedFunction : IIdentifiable
    {
        Task<(EvalResult Result, string Reason, BotToken Token)> CallAsync(ProgramState programState, CancellationToken ct, params IIdentifiable[] parameters);
    }
}
