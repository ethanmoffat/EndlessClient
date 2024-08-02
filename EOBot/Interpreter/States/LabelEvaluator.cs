using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class LabelEvaluator : BaseEvaluator
    {
        public override Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            var result = input.ExpectPair(BotTokenType.Identifier, BotTokenType.Colon)
                ? EvalResult.Ok
                : EvalResult.NotMatch;
            return Task.FromResult((result, string.Empty, input.Current()));
        }
    }
}