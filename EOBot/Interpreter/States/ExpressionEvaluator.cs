using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Linq;
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
            input.Match(BotTokenType.NotOperator);

            if (input.Expect(BotTokenType.LParen))
            {
                var evalRes = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input);
                if (evalRes.Result != EvalResult.Ok)
                    return evalRes;

                // if we get an RParen, the nested expression has been evaluated
                if (input.Expect(BotTokenType.RParen))
                    return NegateIfNeeded(input);

                // expression_tail is optional
                evalRes = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input);
                if (evalRes.Result == EvalResult.NotMatch)
                    return EvaluateSingleOperand(input);
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
                    {
                        // function call as single operand expression - negate the result if needed
                        return NegateIfNeeded(input);
                    }
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
                        return EvaluateSingleOperand(input);
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

            var (result, reason, op2) = GetOperand(input.SymbolTable, input.OperationStack.Pop());
            if (result == EvalResult.Failed)
                return (result, reason, op2);
            var operand2 = op2 as VariableBotToken;

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());
            var @operator = input.OperationStack.Pop();

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());

            BotToken op1;
            (result, reason, op1) = GetOperand(input.SymbolTable, input.OperationStack.Pop());
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
            return NegateIfNeeded(input);
        }

        private (EvalResult, string, BotToken) EvaluateSingleOperand(ProgramState input)
        {
            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());

            // convert to variable token (resolve identifier) so consumer of expression result can use it
            var (localResult, localReason, singleOperand) = GetOperand(input.SymbolTable, input.OperationStack.Pop());
            if (localResult != EvalResult.Ok)
                return (localResult, localReason, singleOperand);

            input.OperationStack.Push(singleOperand);
            return NegateIfNeeded(input);
        }

        // todo: a lot of this code is the same as what's in AssignmentEvaluator::Assign, see if it can be split out/shared
        private (EvalResult, string, BotToken) GetOperand(Dictionary<string, (bool, IIdentifiable)> symbols, BotToken nextToken)
        {
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

                if (identifier.Member == null)
                {
                    var getVariableRes = symbols.GetVariable(identifier.TokenValue, identifier.ArrayIndex);
                    if (getVariableRes.Result != EvalResult.Ok)
                        return (getVariableRes.Result, getVariableRes.Reason, identifier);

                    operand = new VariableBotToken(BotTokenType.Literal, getVariableRes.Variable.ToString(), getVariableRes.Variable);
                }
                else
                {
                    var getVariableRes = symbols.GetVariable<ObjectVariable>(identifier.TokenValue, identifier.ArrayIndex);
                    if (getVariableRes.Result != EvalResult.Ok)
                    {
                        var getRuntimeEvaluatedVariableRes = symbols.GetVariable<RuntimeEvaluatedMemberObjectVariable>(identifier.TokenValue, identifier.ArrayIndex);
                        if (getRuntimeEvaluatedVariableRes.Result != EvalResult.Ok)
                            return (EvalResult.Failed, $"Identifier '{identifier.TokenValue}' is not an object", identifier);

                        getVariableRes.Result = getRuntimeEvaluatedVariableRes.Result;
                        getVariableRes.Reason = getRuntimeEvaluatedVariableRes.Reason;
                        getVariableRes.Variable = new ObjectVariable(
                            getRuntimeEvaluatedVariableRes.Variable.SymbolTable
                                .Select(x => (x.Key, (x.Value.ReadOnly, x.Value.Variable())))
                                .ToDictionary(x => x.Key, x => x.Item2));
                    }

                    return GetOperand(getVariableRes.Variable.SymbolTable, identifier.Member);
                }
            }

            return Success(operand);
        }

        // negate the VariableBotToken on top of the stack if there as a 'not' operator immediately below it
        private (EvalResult, string, BotToken) NegateIfNeeded(ProgramState input)
        {
            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());

            var operand = input.OperationStack.Pop();

            var varToken = operand as VariableBotToken;
            if (varToken == null)
                return StackTokenError(BotTokenType.Literal, operand);

            while (input.OperationStack.Count > 0 && input.OperationStack.Peek().TokenType == BotTokenType.NotOperator)
            {
                var notOperator = input.OperationStack.Pop();

                var boolOperand = varToken.VariableValue as BoolVariable;
                if (boolOperand == null)
                    return UnsupportedOperatorError(notOperator);

                varToken = new VariableBotToken(varToken.TokenType, (!boolOperand.Value).ToString(), new BoolVariable(!boolOperand.Value));
            }

            input.OperationStack.Push(varToken);
            return Success();
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