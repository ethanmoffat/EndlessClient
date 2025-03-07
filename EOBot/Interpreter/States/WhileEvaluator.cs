using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class WhileEvaluator : BlockEvaluator
    {
        public WhileEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            if (!input.Current().Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_WHILE))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            var conditionIndex = input.ExecutionIndex; // the index of the token starting the while loop condition expression
            var (result, reason, token) = await EvaluateConditionAsync(conditionIndex, input, ct);
            var blockStartIndex = input.ExecutionIndex; // the index of the token starting the while loop's execution block
            while (result == EvalResult.Ok && bool.TryParse(token.TokenValue, out var conditionValue) && conditionValue)
            {
                var blockEval = await EvaluateBlockAsync(input, ct);
                if (blockEval.Item1 == EvalResult.ControlFlow)
                {
                    if (IsBreak(input)) break;
                }
                else if (blockEval.Item1 != EvalResult.Ok)
                {
                    return blockEval;
                }

                (result, reason, token) = await EvaluateConditionAsync(conditionIndex, input, ct);
            }

            if (result == EvalResult.Ok)
            {
                input.Goto(blockStartIndex);
                SkipBlock(input);
            }

            return (result, reason, token);
        }
    }
}
