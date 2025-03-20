using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class ExpressionTailEvaluator : BaseEvaluator
    {
        public ExpressionTailEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            var expectedTokens = new[]
            {
                BotTokenType.EqualOperator, BotTokenType.NotEqualOperator, BotTokenType.GreaterThanOperator,
                BotTokenType.LessThanOperator, BotTokenType.GreaterThanEqOperator, BotTokenType.LessThanEqOperator,
                BotTokenType.LogicalAndOperator, BotTokenType.LogicalOrOperator,
                BotTokenType.PlusOperator, BotTokenType.MinusOperator, BotTokenType.MultiplyOperator, BotTokenType.DivideOperator, BotTokenType.ModuloOperator,
                BotTokenType.StrictEqualOperator, BotTokenType.StrictNotEqualOperator, BotTokenType.IsOperator,
            };

            if (!input.MatchOneOf(expectedTokens))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            return await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
        }
    }
}
