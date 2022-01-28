using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class AssignmentEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public AssignmentEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            if (!_evaluators.OfType<VariableEvaluator>().Single().Evaluate(input) ||
                !input.Match(BotTokenType.AssignOperator) ||
                !_evaluators.OfType<ExpressionEvaluator>().Single().Evaluate(input))
            {
                return false;
            }

            var expressionResult = (VariableBotToken)input.OperationStack.Pop();
            if (input.OperationStack.Count == 0)
                return false;

            // todo: check that assignOp is an assignment operator
            var assignOp = input.OperationStack.Pop();
            if (input.OperationStack.Count == 0)
                return false;

            var variable = (IdentifierBotToken)input.OperationStack.Pop();
            if (input.OperationStack.Count == 0)
                return false;

            if (variable.ArrayIndex != null)
            {
                if (!input.SymbolTable.ContainsKey(variable.TokenValue))
                    return false;

                ((ArrayVariable)input.SymbolTable[variable.TokenValue]).Value[variable.ArrayIndex.Value] = expressionResult.VariableValue;
            }
            else
            {
                // todo: dynamic typing with no warning, or warn if changing typing of variable on assignment?
                input.SymbolTable[variable.TokenValue] = expressionResult.VariableValue;
            }

            return true;
        }
    }
}