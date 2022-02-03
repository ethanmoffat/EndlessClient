﻿using EOBot.Interpreter.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class ExpressionTailEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public ExpressionTailEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public async Task<bool> EvaluateAsync(ProgramState input)
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
                && await _evaluators.OfType<ExpressionEvaluator>().Single().EvaluateAsync(input);
        }
    }
}