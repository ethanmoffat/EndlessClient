using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class ExpressionTailEvaluator : BaseEvaluator
    {
        private static BotTokenType[] ExpectedTokens { get; } = new[]
        {
            BotTokenType.EqualOperator, BotTokenType.NotEqualOperator, BotTokenType.GreaterThanOperator,
            BotTokenType.LessThanOperator, BotTokenType.GreaterThanEqOperator, BotTokenType.LessThanEqOperator,
            BotTokenType.LogicalAndOperator, BotTokenType.LogicalOrOperator,
            BotTokenType.PlusOperator, BotTokenType.MinusOperator, BotTokenType.MultiplyOperator, BotTokenType.DivideOperator, BotTokenType.ModuloOperator,
            BotTokenType.StrictEqualOperator, BotTokenType.StrictNotEqualOperator, BotTokenType.IsOperator,
        };

        public ExpressionTailEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            if (!input.MatchOneOf(ExpectedTokens))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            var operatorToken = input.OperationStack.Peek();
            var result = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
            if (result.Result == EvalResult.NotMatch)
                return (EvalResult.Failed, $"Expected operand after {operatorToken.TokenValue} operator, got {input.Current().TokenType} {input.Current().TokenValue}", input.Current());

            return result;
        }
    }
}
