using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class StatementEvaluator : BaseEvaluator
    {
        public StatementEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            while (input.Current().TokenType == BotTokenType.NewLine)
                input.Expect(BotTokenType.NewLine);

            var (result, reason, token) = await Evaluator<AssignmentEvaluator>().EvaluateAsync(input, ct);
            if (result == EvalResult.NotMatch)
            {
                (result, reason, token) = await Evaluator<KeywordEvaluator>().EvaluateAsync(input, ct);
                if (result == EvalResult.NotMatch)
                {
                    (result, reason, token) = await Evaluator<LabelEvaluator>().EvaluateAsync(input, ct);
                    if (result == EvalResult.NotMatch)
                    {
                        (result, reason, token) = await Evaluator<FunctionEvaluator>().EvaluateAsync(input, ct);
                    }
                }
            }

            if (result != EvalResult.Ok)
                return (result, reason, token);

            if (!input.Expect(BotTokenType.NewLine) && !input.Expect(BotTokenType.EOF))
                return Error(input.Current(), BotTokenType.NewLine, BotTokenType.EOF);

            return (result, reason, token);
        }
    }
}
