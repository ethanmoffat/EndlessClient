using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class IfEvaluator : BlockEvaluator
    {
        public IfEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            if (!input.Current().Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_IF))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            var ifStartIndex = input.ExecutionIndex;

            var (result, reason, token) = await EvaluateConditionAsync(ifStartIndex, input, ct);
            if (result == EvalResult.Ok)
            {
                if (bool.TryParse(token.TokenValue, out var conditionValue) && conditionValue)
                {
                    var ifRes = await EvaluateBlockAsync(input, ct);
                    if (ifRes.Item1 == EvalResult.Ok)
                        SkipElseBlocks(input);

                    return ifRes;
                }

                SkipBlock(input);

                while (input.Expect(BotTokenType.NewLine)) ;

                if (input.Current().Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_ELSE))
                {
                    input.Expect(BotTokenType.Keyword);

                    var elseIfRes = await Evaluator<IfEvaluator>().EvaluateAsync(input, ct);
                    if (elseIfRes.Result == EvalResult.Failed)
                    {
                        return elseIfRes;
                    }
                    else if (elseIfRes.Result == EvalResult.Ok)
                    {
                        SkipElseBlocks(input);
                        return elseIfRes;
                    }

                    // if not a match for else if, it is an else block
                    return await EvaluateBlockAsync(input, ct);
                }
            }

            RestoreLastNewline(input);
            return (result, reason, token);
        }

        private void SkipElseBlocks(ProgramState input)
        {
            while (input.Expect(BotTokenType.NewLine)) ;

            // skip the rest of the following blocks if evaluated
            while (input.Current().Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_ELSE))
            {
                input.Expect(BotTokenType.Keyword);
                SkipBlock(input);
                while (input.Expect(BotTokenType.NewLine)) ;
            }

            RestoreLastNewline(input);
        }
    }
}
