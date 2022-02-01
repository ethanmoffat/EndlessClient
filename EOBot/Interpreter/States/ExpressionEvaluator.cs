using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class ExpressionEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public ExpressionEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public async Task<bool> EvaluateAsync(ProgramState input)
        {
            if (input.Expect(BotTokenType.LParen))
            {
                if (!await _evaluators.OfType<ExpressionEvaluator>().Single().EvaluateAsync(input))
                    return false;

                // if we get an RParen just be done
                if (input.Expect(BotTokenType.RParen))
                    return true;

                // expression_tail is optional
                if (!await _evaluators.OfType<ExpressionTailEvaluator>().Single().EvaluateAsync(input))
                {
                    if (input.OperationStack.Count == 0)
                        return false;

                    // convert to variable token (resolve identifier) so consumer of expression result can use it
                    var singleOperand = GetOperand(input);
                    input.OperationStack.Push(singleOperand);

                    return true;
                }

                if (!input.Expect(BotTokenType.RParen))
                    return false;
            }
            else
            {
                // an expression can be a function call
                if (await _evaluators.OfType<FunctionEvaluator>().Single().EvaluateAsync(input))
                {
                    // there may or may not be an expression tail after a function call
                    if (!await _evaluators.OfType<ExpressionTailEvaluator>().Single().EvaluateAsync(input))
                        return true;
                }
                else
                {

                    if (!await _evaluators.OfType<OperandEvaluator>().Single().EvaluateAsync(input))
                        return false;

                    // expression_tail is optional
                    if (!await _evaluators.OfType<ExpressionTailEvaluator>().Single().EvaluateAsync(input))
                    {
                        if (input.OperationStack.Count == 0)
                            return false;

                        // convert to variable token (resolve identifier) so consumer of expression result can use it
                        var singleOperand = GetOperand(input);
                        input.OperationStack.Push(singleOperand);

                        return true;
                    }
                }
            }

            if (input.OperationStack.Count == 0)
                return false;
            var operand2 = GetOperand(input);

            if (input.OperationStack.Count == 0)
                return false;
            var @operator = input.OperationStack.Pop();

            if (input.OperationStack.Count == 0)
                return false;
            var operand1 = GetOperand(input);

            IVariable expressionResult;
            switch (@operator.TokenType)
            {
                case BotTokenType.EqualOperator: expressionResult = new BoolVariable(operand1.VariableValue.Equals(operand2.VariableValue)); break;
                case BotTokenType.NotEqualOperator: expressionResult = new BoolVariable(!operand1.VariableValue.Equals(operand2.VariableValue)); break;
                case BotTokenType.LessThanOperator: expressionResult = new BoolVariable(operand1.VariableValue.CompareTo(operand2.VariableValue) < 0); break;
                case BotTokenType.GreaterThanOperator: expressionResult = new BoolVariable(operand1.VariableValue.CompareTo(operand2.VariableValue) > 0); break;
                case BotTokenType.LessThanEqOperator: expressionResult = new BoolVariable(operand1.VariableValue.CompareTo(operand2.VariableValue) <= 0); break;
                case BotTokenType.GreaterThanEqOperator: expressionResult = new BoolVariable(operand1.VariableValue.CompareTo(operand2.VariableValue) >= 0); break;
                case BotTokenType.PlusOperator: expressionResult = Add((dynamic)operand1.VariableValue, (dynamic)operand2.VariableValue); break;
                case BotTokenType.MinusOperator: expressionResult = Subtract((dynamic)operand1.VariableValue, (dynamic)operand2.VariableValue); break;
                case BotTokenType.MultiplyOperator: expressionResult = Multiply((dynamic)operand1.VariableValue, (dynamic)operand2.VariableValue); break;
                case BotTokenType.DivideOperator: expressionResult = Divide((dynamic)operand1.VariableValue, (dynamic)operand2.VariableValue); break;
                default: return false;
            }

            // todo: indicate errors in the operation
            if (expressionResult == null)
                return false;

            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, expressionResult.StringValue, expressionResult));

            return true;
        }

        private static VariableBotToken GetOperand(ProgramState input)
        {
            var nextToken = input.OperationStack.Pop();
            if (nextToken.TokenType == BotTokenType.Literal)
            {
                if (int.TryParse(nextToken.TokenValue, out var intValue))
                    return new VariableBotToken(BotTokenType.Literal, nextToken.TokenValue, new IntVariable(intValue));
                else if (bool.TryParse(nextToken.TokenValue, out var boolValue))
                    return new VariableBotToken(BotTokenType.Literal, nextToken.TokenValue, new BoolVariable(boolValue));
                else
                    return new VariableBotToken(BotTokenType.Literal, nextToken.TokenValue, new StringVariable(nextToken.TokenValue));
            }

            var operand = nextToken as VariableBotToken;
            if (operand == null)
            {
                var identifier = (IdentifierBotToken)nextToken;
                if (!input.SymbolTable.ContainsKey(identifier.TokenValue))
                    input.SymbolTable[identifier.TokenValue] = (true, UndefinedVariable.Instance);

                var variableValue = (IVariable)input.SymbolTable[identifier.TokenValue].Identifiable;
                if (identifier.ArrayIndex != null)
                    variableValue = ((ArrayVariable)variableValue).Value[identifier.ArrayIndex.Value];
                operand = new VariableBotToken(BotTokenType.Literal, variableValue.ToString(), variableValue);
            }

            return operand;
        }

        private IIdentifiable Add(IntVariable a, IntVariable b) => new IntVariable(a.Value + b.Value);
        private IIdentifiable Add(IntVariable a, StringVariable b) => new StringVariable(a.Value + b.Value);
        private IIdentifiable Add(StringVariable a, IntVariable b) => new StringVariable(a.Value + b.Value);
        private IIdentifiable Add(StringVariable a, StringVariable b) => new StringVariable(a.Value + b.Value);
        private IIdentifiable Add(object a, object b) => null;

        private IIdentifiable Subtract(IntVariable a, IntVariable b) => new IntVariable(a.Value - b.Value);
        private IIdentifiable Subtract(object a, object b) => null;

        private IIdentifiable Multiply(IntVariable a, IntVariable b) => new IntVariable(a.Value * b.Value);
        private IIdentifiable Multiply(object a, object b) => null;

        private IIdentifiable Divide(IntVariable a, IntVariable b) => new IntVariable(a.Value / b.Value);
        private IIdentifiable Divide(object a, object b) => null;
    }
}
