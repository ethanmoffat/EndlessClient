using EOBot.Interpreter.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class ExpressionTailEvaluator : BaseEvaluator
    {
        public ExpressionTailEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            var expectedTokens = new[]
            {
                BotTokenType.EqualOperator, BotTokenType.NotEqualOperator, BotTokenType.GreaterThanOperator,
                BotTokenType.LessThanOperator, BotTokenType.GreaterThanEqOperator, BotTokenType.LessThanEqOperator,
                BotTokenType.PlusOperator, BotTokenType.MinusOperator, BotTokenType.MultiplyOperator, BotTokenType.DivideOperator
            };

            if (!input.MatchOneOf(expectedTokens))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            return await Evaluator<ExpressionEvaluator>().EvaluateAsync(input);
        }
    }
}
