using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class OperandEvaluator : BaseEvaluator
    {
        public OperandEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            input.Match(BotTokenType.NotOperator);

            var evalRes = await Evaluator<VariableEvaluator>().EvaluateAsync(input, ct);
            if (evalRes.Result == EvalResult.Ok)
                return evalRes;

            var matchRes = input.MatchOneOf(BotTokenType.Literal, BotTokenType.TypeSpecifier);
            return matchRes ? Success() : (EvalResult.NotMatch, string.Empty, input.Current());
        }
    }
}
