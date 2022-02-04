using EOBot.Interpreter.Extensions;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class GotoEvaluator : BaseEvaluator
    {
        public override Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            // ensure we have the right keyword before advancing the program
            var current = input.Current();
            if (current.TokenType != BotTokenType.Keyword || current.TokenValue != "goto")
                return Task.FromResult((EvalResult.NotMatch, string.Empty, input.Current()));

            input.Expect(BotTokenType.Keyword);

            if (!input.Match(BotTokenType.Identifier))
                return Task.FromResult(Error(input.Current(), BotTokenType.Identifier));

            var label = input.OperationStack.Pop();
            if (!input.Labels.ContainsKey(label.TokenValue))
                return Task.FromResult(IdentifierNotFoundError(new IdentifierBotToken(label)));
            
            var result = input.Goto(input.Labels[label.TokenValue]);
            return Task.FromResult(result ? Success() : GotoError(label));
        }
    }
}
