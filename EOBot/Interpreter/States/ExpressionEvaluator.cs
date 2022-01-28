using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class ExpressionEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public ExpressionEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            var rParenExpected = input.Match(BotTokenType.LParen);

            if (!_evaluators.OfType<OperandEvaluator>().Single().Evaluate(input))
                return false;

            // expression may just be a single result (since expressions are recursive - doesn't have to be 'A op B')
            if (!_evaluators.OfType<ExpressionTailEvaluator>().Single().Evaluate(input))
            {
                if (rParenExpected && !input.Expect(BotTokenType.RParen))
                    return false;

                if (input.OperationStack.Count == 0)
                    return false;

                // convert to variable token (resolve identifier) so consumer of expression result can use it
                var singleOperand = GetOperand(input);
                input.OperationStack.Push(singleOperand);

                return true;
            }

            if (rParenExpected && !input.Expect(BotTokenType.RParen))
                return false;

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

            if (rParenExpected)
            {
                if (input.OperationStack.Count == 0)
                    return false;

                // todo: check that this is the LPAREN
                input.OperationStack.Pop();
            }

            // todo: indicate errors in the operation
            if (expressionResult == null)
                return false;

            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, expressionResult.StringValue, expressionResult));

            return true;
        }

        private static VariableBotToken GetOperand(ProgramState input)
        {
            var operand = input.OperationStack.Peek() as VariableBotToken;

            if (operand == null)
            {
                var identifier = (IdentifierBotToken)input.OperationStack.Pop();
                var variableValue = (IVariable)input.SymbolTable[identifier.TokenValue];
                if (identifier.ArrayIndex != null)
                    variableValue = ((ArrayVariable)variableValue).Value[identifier.ArrayIndex.Value];
                operand = new VariableBotToken(BotTokenType.Literal, variableValue.ToString(), variableValue);
            }
            else
            {
                input.OperationStack.Pop();
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
