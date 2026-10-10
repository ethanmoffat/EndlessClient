using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class CollectionElementEvaluator : BaseEvaluator
    {
        public CollectionElementEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            var eval = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
            if (eval.Result != EvalResult.Ok || !input.Match(BotTokenType.Colon))
                return eval;

            return await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
        }
    }
}
