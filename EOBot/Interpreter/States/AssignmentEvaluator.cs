using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class AssignmentEvaluator : BaseEvaluator
    {
        public AssignmentEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            var eval = await Evaluator<VariableEvaluator>().EvaluateAsync(input);
            if (eval.Result != EvalResult.Ok)
                return eval;

            if (!input.Match(BotTokenType.AssignOperator))
                return Error(input.Current(), BotTokenType.AssignOperator);

            eval = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input);
            if (eval.Result != EvalResult.Ok)
                return eval;

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());
            var expressionResult = (VariableBotToken)input.OperationStack.Pop();

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());
            var assignOp = input.OperationStack.Pop();
            if (assignOp.TokenType != BotTokenType.AssignOperator)
                return StackTokenError(BotTokenType.AssignOperator, assignOp);

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());
            var variable = (IdentifierBotToken)input.OperationStack.Pop();

            if (input.SymbolTable.ContainsKey(variable.TokenValue) &&
                input.SymbolTable[variable.TokenValue].ReadOnly)
                return ReadOnlyVariableError(variable);

            if (variable.ArrayIndex != null)
            {
                if (!input.SymbolTable.ContainsKey(variable.TokenValue))
                    return IdentifierNotFoundError(variable);

                var getVariableResult = input.GetVariable<ArrayVariable>(variable.TokenValue, variable.ArrayIndex);
                if (getVariableResult.Result != EvalResult.Ok)
                    return (getVariableResult.Result, getVariableResult.Reason, variable);

                var targetArray = getVariableResult.Variable;
                targetArray.Value[variable.ArrayIndex.Value] = expressionResult.VariableValue;
            }
            else
            {
                if (input.SymbolTable.ContainsKey(variable.TokenValue) && input.SymbolTable[variable.TokenValue].Identifiable.GetType() != expressionResult.VariableValue.GetType())
                {
                    // todo: surface warnings to caller and let caller decide what to do with it instead of making the interpreter write to console directly
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, $"Changing type of variable {variable.TokenValue} from {input.SymbolTable[variable.TokenValue].Identifiable.GetType()} to {expressionResult.VariableValue.GetType()}", System.ConsoleColor.DarkYellow);
                }

                input.SymbolTable[variable.TokenValue] = (false, expressionResult.VariableValue);
            }

            return Success();
        }
    }
}
