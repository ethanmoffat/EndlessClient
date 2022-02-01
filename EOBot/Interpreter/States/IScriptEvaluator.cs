using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public interface IScriptEvaluator
    {
        Task<bool> EvaluateAsync(ProgramState input);
    }
}
