using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class ExpressionEvaluator : BaseEvaluator
    {
        public ExpressionEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        // todo: this code is a mess and could use cleaning up (lots of copy/paste...)
        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            if (input.Expect(BotTokenType.LParen))
            {
                var evalRes = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input);
                if (evalRes.Result != EvalResult.Ok)
                    return evalRes;

                // if we get an RParen, the nested expression has been evaluated
                if (input.Expect(BotTokenType.RParen))
                    return evalRes;

                // expression_tail is optional
                evalRes = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input);
                if (evalRes.Result == EvalResult.NotMatch)
                {
                    if (input.OperationStack.Count == 0)
                        return StackEmptyError(input.Current());

                    // convert to variable token (resolve identifier) so consumer of expression result can use it
                    var (localResult, localReason, singleOperand) = GetOperand(input);
                    if (localResult != EvalResult.Ok)
                        return (localResult, localReason, singleOperand);

                    input.OperationStack.Push(singleOperand);

                    return Success();
                }
                else if (evalRes.Result == EvalResult.Failed)
                    return evalRes;

                if (!input.Expect(BotTokenType.RParen))
                    return Error(input.Current(), BotTokenType.RParen);
            }
            else
            {
                // an expression can be a function call
                var evalRes = await Evaluator<FunctionEvaluator>().EvaluateAsync(input);
                if (evalRes.Result == EvalResult.Ok)
                {
                    // there may or may not be an expression tail after a function call
                    // if there is an expression tail, evaluate the operands below, otherwise return early
                    evalRes = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input);
                    if (evalRes.Result == EvalResult.NotMatch)
                        return Success();
                    else if (evalRes.Result == EvalResult.Failed)
                        return evalRes;
                }
                else if (evalRes.Result == EvalResult.NotMatch)
                {
                    // if not a function, evaluate operand and expression tail (basic expression)
                    evalRes = await Evaluator<OperandEvaluator>().EvaluateAsync(input);
                    if (evalRes.Result != EvalResult.Ok)
                        return evalRes;

                    // expression_tail is optional, if not set no need to evaluate operation stack below / return early
                    evalRes = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input);
                    if (evalRes.Result == EvalResult.NotMatch)
                    {
                        if (input.OperationStack.Count == 0)
                            return StackEmptyError(input.Current());

                        // convert to variable token (resolve identifier) so consumer of expression result can use it
                        var (localResult, localReason, singleOperand) = GetOperand(input);
                        if (localResult != EvalResult.Ok)
                            return (localResult, localReason, singleOperand);

                        input.OperationStack.Push(singleOperand);

                        return Success();
                    }
                    else if (evalRes.Result == EvalResult.Failed)
                        return evalRes;
                }
                else
                {
                    return evalRes;
                }
            }

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());

            var (result, reason, op2) = GetOperand(input);
            if (result == EvalResult.Failed)
                return (result, reason, op2);
            var operand2 = op2 as VariableBotToken;

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());
            var @operator = input.OperationStack.Pop();

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());

            BotToken op1;
            (result, reason, op1) = GetOperand(input);
            if (result == EvalResult.Failed)
                return (result, reason, op1);
            var operand1 = op1 as VariableBotToken;

            (IVariable Result, string Reason) res;
            res.Reason = string.Empty;
            switch (@operator.TokenType)
            {
                case BotTokenType.EqualOperator: res.Result = new BoolVariable(operand1.VariableValue.Equals(operand2.VariableValue)); break;
                case BotTokenType.NotEqualOperator: res.Result = new BoolVariable(!operand1.VariableValue.Equals(operand2.VariableValue)); break;
                case BotTokenType.LessThanOperator: res.Result = new BoolVariable(operand1.VariableValue.CompareTo(operand2.VariableValue) < 0); break;
                case BotTokenType.GreaterThanOperator: res.Result = new BoolVariable(operand1.VariableValue.CompareTo(operand2.VariableValue) > 0); break;
                case BotTokenType.LessThanEqOperator: res.Result = new BoolVariable(operand1.VariableValue.CompareTo(operand2.VariableValue) <= 0); break;
                case BotTokenType.GreaterThanEqOperator: res.Result = new BoolVariable(operand1.VariableValue.CompareTo(operand2.VariableValue) >= 0); break;
                case BotTokenType.PlusOperator: res = Add((dynamic)operand1.VariableValue, (dynamic)operand2.VariableValue); break;
                case BotTokenType.MinusOperator: res = Subtract((dynamic)operand1.VariableValue, (dynamic)operand2.VariableValue); break;
                case BotTokenType.MultiplyOperator: res = Multiply((dynamic)operand1.VariableValue, (dynamic)operand2.VariableValue); break;
                case BotTokenType.DivideOperator: res = Divide((dynamic)operand1.VariableValue, (dynamic)operand2.VariableValue); break;
                default: return UnsupportedOperatorError(@operator);
            }

            if (res.Result == null)
                return (EvalResult.Failed, $"Error evaluating expression: {res.Reason}", input.Current());

            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, res.Result.StringValue, res.Result));

            return Success();
        }

        private (EvalResult, string, BotToken) GetOperand(ProgramState input)
        {
            var nextToken = input.OperationStack.Pop();
            if (nextToken.TokenType == BotTokenType.Literal)
            {
                if (int.TryParse(nextToken.TokenValue, out var intValue))
                    return Success(new VariableBotToken(BotTokenType.Literal, nextToken.TokenValue, new IntVariable(intValue)));
                else if (bool.TryParse(nextToken.TokenValue, out var boolValue))
                    return Success(new VariableBotToken(BotTokenType.Literal, nextToken.TokenValue, new BoolVariable(boolValue)));
                else
                    return Success(new VariableBotToken(BotTokenType.Literal, nextToken.TokenValue, new StringVariable(nextToken.TokenValue)));
            }

            var operand = nextToken as VariableBotToken;
            if (operand == null)
            {
                var identifier = nextToken as IdentifierBotToken;
                if (identifier == null)
                    return (EvalResult.Failed, $"Expected operand of type Variable or Identifier but got {nextToken.TokenType}", nextToken);

                if (!input.SymbolTable.ContainsKey(identifier.TokenValue))
                    input.SymbolTable[identifier.TokenValue] = (true, UndefinedVariable.Instance);

                var variableValue = (IVariable)input.SymbolTable[identifier.TokenValue].Identifiable;
                if (identifier.ArrayIndex != null)
                {
                    var arrayVariable = variableValue as ArrayVariable;
                    if (arrayVariable == null)
                    {
                        return (EvalResult.Failed, $"Identifier {identifier.TokenValue} is not an array", identifier);
                    }

                    if (arrayVariable.Value.Count <= identifier.ArrayIndex.Value)
                    {
                        return (EvalResult.Failed, $"Index {identifier.ArrayIndex} is out of range of the array {identifier.TokenValue} (size {arrayVariable.Value.Count})", identifier);
                    }

                    variableValue = arrayVariable.Value[identifier.ArrayIndex.Value];
                }

                operand = new VariableBotToken(BotTokenType.Literal, variableValue.ToString(), variableValue);
            }

            return Success(operand);
        }

        private (IVariable, string) Add(IntVariable a, IntVariable b) => (new IntVariable(a.Value + b.Value), string.Empty);
        private (IVariable, string) Add(IntVariable a, StringVariable b) => (new StringVariable(a.Value + b.Value), string.Empty);
        private (IVariable, string) Add(StringVariable a, IntVariable b) => (new StringVariable(a.Value + b.Value), string.Empty);
        private (IVariable, string) Add(StringVariable a, StringVariable b) => (new StringVariable(a.Value + b.Value), string.Empty);
        private (IVariable, string) Add(object a, object b) => (null, $"Objects {a} and {b} could not be added (currently the operands must be int or string)");

        private (IVariable, string) Subtract(IntVariable a, IntVariable b) => (new IntVariable(a.Value - b.Value), string.Empty);
        private (IVariable, string) Subtract(object a, object b) => (null, $"Objects {a} and {b} could not be subtracted (currently the operands must be int)");

        private (IVariable, string) Multiply(IntVariable a, IntVariable b) => (new IntVariable(a.Value * b.Value), string.Empty);
        private (IVariable, string) Multiply(object a, object b) => (null, $"Objects {a} and {b} could not be multiplied (currently the operands must be int)");

        private (IVariable, string) Divide(IntVariable a, IntVariable b) => (new IntVariable(a.Value / b.Value), string.Empty);
        private (IVariable, string) Divide(object a, object b) => (null, $"Objects {a} and {b} could not be divided (currently the operands must be int)");
    }
}
