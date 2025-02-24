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

            var res = await Evaluator<IfEvaluator>().EvaluateAsync(input, ct);
            if (res.Result == EvalResult.NotMatch)
            {
                res = await Evaluator<WhileEvaluator>().EvaluateAsync(input, ct);

                if (res.Result == EvalResult.NotMatch)
                {
                    res = await Evaluator<GotoEvaluator>().EvaluateAsync(input, ct);
                }
            }

            return res;
        }
    }
}
