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
            var assignmentTarget = (IdentifierBotToken)input.OperationStack.Pop();

            return Assign2(input.SymbolTable, assignmentTarget, expressionResult);
        }

        private (EvalResult, string, BotToken) Assign2(Dictionary<string, (bool ReadOnly, IIdentifiable Identifiable)> symbols, IdentifierBotToken assignmentTarget, VariableBotToken expressionResult)
        {
            if (assignmentTarget.Member != null)
            {
                if (!symbols.ContainsKey(assignmentTarget.TokenValue))
                    return IdentifierNotFoundError(assignmentTarget);

                var getVariableResult = symbols.GetVariable<ObjectVariable>(assignmentTarget.TokenValue, assignmentTarget.ArrayIndex);
                if (getVariableResult.Result != EvalResult.Ok)
                    return (getVariableResult.Result, getVariableResult.Reason, assignmentTarget);

                var targetObject = getVariableResult.Variable;
                return Assign2(targetObject.SymbolTable, assignmentTarget.Member, expressionResult);
            }

            if (assignmentTarget.ArrayIndex != null)
            {
                if (!symbols.ContainsKey(assignmentTarget.TokenValue))
                    return IdentifierNotFoundError(assignmentTarget);

                var getVariableResult = symbols.GetVariable<ArrayVariable>(assignmentTarget.TokenValue);
                if (getVariableResult.Result != EvalResult.Ok)
                    return (getVariableResult.Result, getVariableResult.Reason, assignmentTarget);

                var targetArray = getVariableResult.Variable;
                targetArray.Value[assignmentTarget.ArrayIndex.Value] = expressionResult.VariableValue;
            }
            else
            {
                if (symbols.ContainsKey(assignmentTarget.TokenValue) && symbols[assignmentTarget.TokenValue].ReadOnly)
                    return ReadOnlyVariableError(assignmentTarget);

                if (symbols.ContainsKey(assignmentTarget.TokenValue) && symbols[assignmentTarget.TokenValue].Identifiable.GetType() != expressionResult.VariableValue.GetType())
                {
                    // todo: surface warnings to caller and let caller decide what to do with it instead of making the interpreter write to console directly
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, $"Changing type of variable {assignmentTarget.TokenValue} from {symbols[assignmentTarget.TokenValue].Identifiable.GetType()} to {expressionResult.VariableValue.GetType()}", System.ConsoleColor.DarkYellow);
                }

                symbols[assignmentTarget.TokenValue] = (false, expressionResult.VariableValue);
            }

            return Success();
        }
    }
}
