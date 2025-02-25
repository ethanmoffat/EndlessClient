using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class GotoEvaluator : BaseEvaluator
    {
        public override Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return Task.FromResult((EvalResult.Cancelled, string.Empty, (BotToken)null));

            if (!input.Current().Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_GOTO))
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
