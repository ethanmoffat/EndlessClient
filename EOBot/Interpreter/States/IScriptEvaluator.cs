using System.Threading;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public interface IScriptEvaluator
    {
        Task<(EvalResult Result, string Reason, BotToken Token)> EvaluateAsync(ProgramState input, CancellationToken ct);
    }
}
