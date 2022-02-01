using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class AssignmentEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public AssignmentEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public async Task<bool> EvaluateAsync(ProgramState input)
        {
            if (!await _evaluators.OfType<VariableEvaluator>().Single().EvaluateAsync(input) ||
                !input.Match(BotTokenType.AssignOperator) ||
                !await _evaluators.OfType<ExpressionEvaluator>().Single().EvaluateAsync(input))
            {
                return false;
            }

            if (input.OperationStack.Count == 0)
                return false;
            var expressionResult = (VariableBotToken)input.OperationStack.Pop();

            // todo: check that assignOp is an assignment operator
            if (input.OperationStack.Count == 0)
                return false;
            var assignOp = input.OperationStack.Pop();

            if (input.OperationStack.Count == 0)
                return false;
            var variable = (IdentifierBotToken)input.OperationStack.Pop();

            if (input.SymbolTable.ContainsKey(variable.TokenValue) &&
                input.SymbolTable[variable.TokenValue].ReadOnly)
                return false;

            if (variable.ArrayIndex != null)
            {
                if (!input.SymbolTable.ContainsKey(variable.TokenValue))
                    return false;

                ((ArrayVariable)input.SymbolTable[variable.TokenValue].Identifiable).Value[variable.ArrayIndex.Value] = expressionResult.VariableValue;
            }
            else
            {
                // todo: dynamic typing with no warning, or warn if changing typing of variable on assignment?
                input.SymbolTable[variable.TokenValue] = (false, expressionResult.VariableValue);
            }

            return true;
        }
    }
}