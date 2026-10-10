using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class ObjectInitializerEvaluator : BaseEvaluator
    {
        public ObjectInitializerEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            var eval = await Evaluator<VariableEvaluator>().EvaluateAsync(input, ct);
            if (eval.Result != EvalResult.Ok)
                return eval;

            if (!input.Match(BotTokenType.AssignOperator))
                return Error(input.Current(), BotTokenType.AssignOperator);

            return await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
        }
    }
}
