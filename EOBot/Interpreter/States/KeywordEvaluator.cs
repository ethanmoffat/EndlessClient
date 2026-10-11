using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class KeywordEvaluator : BaseEvaluator
    {
        public KeywordEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            List<IScriptEvaluator> evaluators =
            [
                Evaluator<IfEvaluator>(),
                Evaluator<WhileEvaluator>(),
                Evaluator<ForEvaluator>(),
                Evaluator<ForeachEvaluator>(),
                Evaluator<ReturnEvaluator>(),
                Evaluator<GotoEvaluator>(),
            ];

            (EvalResult Result, string, BotToken) res = default;
            foreach (var evaluator in evaluators)
            {
                res = await evaluator.EvaluateAsync(input, ct);
                if (res.Result != EvalResult.NotMatch)
                    return res;
            }

            if (input.Current().Is(BotTokenType.Keyword, Keywords.Continue) ||
                input.Current().Is(BotTokenType.Keyword, Keywords.Break))
            {
                input.OperationStack.Push(input.Current());
                res.Result = EvalResult.ControlFlow;
            }

            return res;
        }
    }
}
