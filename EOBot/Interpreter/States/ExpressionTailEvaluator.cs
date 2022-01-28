using EOBot.Interpreter.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class ExpressionTailEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public ExpressionTailEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            return input.MatchOneOf(
                BotTokenType.EqualOperator,
                BotTokenType.NotEqualOperator,
                BotTokenType.GreaterThanOperator,
                BotTokenType.LessThanOperator,
                BotTokenType.GreaterThanEqOperator,
                BotTokenType.LessThanEqOperator,
                BotTokenType.PlusOperator,
                BotTokenType.MinusOperator,
                BotTokenType.MultiplyOperator,
                BotTokenType.DivideOperator)
                && _evaluators.OfType<ExpressionEvaluator>().Single().Evaluate(input);
        }
    }
}
