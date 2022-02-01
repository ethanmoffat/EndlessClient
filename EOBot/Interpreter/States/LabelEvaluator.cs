using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class LabelEvaluator : IScriptEvaluator
    {
        public Task<bool> EvaluateAsync(ProgramState input)
        {
            return Task.FromResult(input.ExpectPair(BotTokenType.Identifier, BotTokenType.Colon));
        }
    }
}
