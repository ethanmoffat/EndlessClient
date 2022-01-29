using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class VariableEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public VariableEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            if (!input.Match(BotTokenType.Variable))
                return false;

            int? arrayIndex = null;

            if (input.Expect(BotTokenType.LBracket))
            {
                if (!_evaluators.OfType<ExpressionEvaluator>().Single().Evaluate(input))
                    return false;
                input.Expect(BotTokenType.RBracket);

                if (input.OperationStack.Count == 0)
                    return false;
                var expressionResult = (VariableBotToken)input.OperationStack.Pop();

                // todo: error checking if expression doesn't evaluate down to int
                arrayIndex = ((IntVariable)expressionResult.VariableValue).Value;
            }

            if (input.OperationStack.Count == 0)
                return false;
            var identifier = input.OperationStack.Pop();

            input.OperationStack.Push(new IdentifierBotToken(BotTokenType.Variable, identifier.TokenValue, arrayIndex));

            return true;
        }
    }
}
