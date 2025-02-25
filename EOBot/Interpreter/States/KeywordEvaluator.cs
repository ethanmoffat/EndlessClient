using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
                // ForEachEvaluator
                // FunctionEvaluator
                Evaluator<GotoEvaluator>(),
            ];

            (EvalResult Result, string, BotToken) res = default;
            foreach (var evaluator in evaluators)
            {
                res = await evaluator.EvaluateAsync(input, ct);
                if (res.Result != EvalResult.NotMatch)
                    return res;
            }

            return res;
        }
    }
}
