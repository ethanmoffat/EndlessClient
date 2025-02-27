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

            var whileLoopStartIndex = input.ExecutionIndex;

            EvalResult result;
            string reason;
            BotToken token;
            for ((result, reason, token) = await EvaluateConditionAsync(whileLoopStartIndex, input, ct);
                 result == EvalResult.Ok && bool.TryParse(token.TokenValue, out var conditionValue) && conditionValue;
                 (result, reason, token) = await EvaluateConditionAsync(whileLoopStartIndex, input, ct))
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
            }

            if (result == EvalResult.Ok)
            {
                SkipBlock(input);
            }

            return (result, reason, token);
        }
    }
}
