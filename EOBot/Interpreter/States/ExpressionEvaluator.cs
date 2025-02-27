using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Syntax;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class ExpressionEvaluator : BaseEvaluator
    {
        public ExpressionEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        // todo: this code is a mess and could use cleaning up (lots of copy/paste...)
        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            input.Match(BotTokenType.NotOperator);

            if (input.Match(BotTokenType.LParen))
            {
                var evalRes = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
                if (evalRes.Result != EvalResult.Ok)
                    return evalRes;

                // if we get an RParen, the nested expression has been evaluated
                if (input.Expect(BotTokenType.RParen))
                {
                    // check for an expression tail after the close paren
                    evalRes = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input, ct);
                    if (evalRes.Result != EvalResult.Ok && evalRes.Result != EvalResult.NotMatch)
                        return evalRes;
                }
                else
                {
                    // expression_tail is optional
                    evalRes = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input, ct);
                    if (evalRes.Result != EvalResult.Ok && evalRes.Result != EvalResult.NotMatch)
                        return evalRes;

                    if (!input.Expect(BotTokenType.RParen))
                        return Error(input.Current(), BotTokenType.RParen);
                }

                // take care of the LParen that we matched on
                // LParen is used as a demarcation for when to stop evaluating a stream of tokens as a single expression
                var tmp = input.OperationStack.Pop();

                if (input.OperationStack.Peek().TokenType != BotTokenType.LParen)
                    return StackTokenError(BotTokenType.LParen, input.OperationStack.Peek());

                input.OperationStack.Pop();
                input.OperationStack.Push(tmp);
            }
            else
            {
                // an expression can be a function call
                var evalRes = await Evaluator<FunctionEvaluator>().EvaluateAsync(input, ct);
                if (evalRes.Result == EvalResult.Ok)
                {
                    // expression_tail is optional
                    evalRes = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input, ct);
                    if (evalRes.Result != EvalResult.Ok && evalRes.Result != EvalResult.NotMatch)
                    {
                        return evalRes;
                    }
                }
                else if (evalRes.Result == EvalResult.NotMatch)
                {
                    // if not a function, evaluate operand and expression tail (basic expression)
                    evalRes = await Evaluator<OperandEvaluator>().EvaluateAsync(input, ct);
                    if (evalRes.Result != EvalResult.Ok)
                        return evalRes;

                    // expression_tail is optional
                    evalRes = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input, ct);
                    if (evalRes.Result != EvalResult.Ok && evalRes.Result != EvalResult.NotMatch)
                    {
                        return evalRes;
                    }
                }
                else
                {
                    return evalRes;
                }
            }

            return EvaluateStackOperands(input);
        }

        private (EvalResult, string, BotToken) EvaluateStackOperands(ProgramState input)
        {
            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());

            var syntaxTree = new SyntaxTree(input.OperationStack)
            {
                VisitOrder = SyntaxTree.Order.PostOrder
            };

            var (result, reason, token) = EvaluateTree(input, syntaxTree.Root);
            if (result != EvalResult.Ok)
                return (result, reason, token);

            input.OperationStack.Push(token);

            return Success();
        }

        private (EvalResult, string, BotToken) EvaluateTree(ProgramState input, SyntaxTree.Node node)
        {
            if (node.Token.IsUnary())
            {
                var (res, reason, operand) = node.Left != null
                    ? EvaluateTree(input, node.Left)
                    : node.Right != null
                        ? EvaluateTree(input, node.Right)
                        : (EvalResult.Failed, "Error evaluating expression: no operands for operator", node.Token);
                if (res == EvalResult.Failed)
                    return (res, reason, operand);
                if (operand is not VariableBotToken variable)
                    return (EvalResult.Failed, $"Error evaluating expression: expected operand but got {operand.TokenType}", operand);

                return HandleUnaryOperator(input, node.Token, variable);
            }
            else if (node.Token.IsBinary())
            {
                var (result, reason, resolved) = EvaluateTree(input, node.Right);
                if (result == EvalResult.Failed)
                    return (result, reason, node.Right.Token);
                if (resolved is not VariableBotToken lhs)
                    return (EvalResult.Failed, $"Error evaluating expression: expected operand but got {resolved.TokenType}", resolved);

                (result, reason, resolved) = EvaluateTree(input, node.Left);
                if (result == EvalResult.Failed)
                    return (result, reason, node.Left.Token);
                if (resolved is not VariableBotToken rhs)
                    return (EvalResult.Failed, $"Error evaluating expression: expected operand but got {resolved.TokenType}", resolved);

                return HandleBinaryOperator(input, node.Token, lhs, rhs);
            }
            else
            {
                return GetOperand(input.SymbolTable, node.Token);
            }
        }

        private (EvalResult, string, BotToken) HandleUnaryOperator(ProgramState input, BotToken operatorToken, VariableBotToken operand)
        {
            (IVariable Result, string Reason) res;
            res.Reason = string.Empty;
            switch (operatorToken.TokenType)
            {
                case BotTokenType.NotOperator: res = Negate(operand.VariableValue); break;
                default: return UnsupportedOperatorError(operatorToken);
            }

            if (res.Result == null)
                return (EvalResult.Failed, $"Error evaluating expression: {res.Reason}", input.Current());

            return Success(new VariableBotToken(BotTokenType.Literal, res.Result.StringValue, res.Result));
        }

        private (EvalResult, string, BotToken) HandleBinaryOperator(ProgramState input, BotToken operatorToken, VariableBotToken lhs, VariableBotToken rhs)
        {
            (IVariable Result, string Reason) res;
            res.Reason = string.Empty;
            switch (operatorToken.TokenType)
            {
                case BotTokenType.EqualOperator: res.Result = new BoolVariable(lhs.VariableValue.Equals(rhs.VariableValue)); break;
                case BotTokenType.NotEqualOperator: res.Result = new BoolVariable(!lhs.VariableValue.Equals(rhs.VariableValue)); break;
                case BotTokenType.LessThanOperator: res.Result = new BoolVariable(lhs.VariableValue.CompareTo(rhs.VariableValue) < 0); break;
                case BotTokenType.GreaterThanOperator: res.Result = new BoolVariable(lhs.VariableValue.CompareTo(rhs.VariableValue) > 0); break;
                case BotTokenType.LessThanEqOperator: res.Result = new BoolVariable(lhs.VariableValue.CompareTo(rhs.VariableValue) <= 0); break;
                case BotTokenType.GreaterThanEqOperator: res.Result = new BoolVariable(lhs.VariableValue.CompareTo(rhs.VariableValue) >= 0); break;
                case BotTokenType.LogicalAndOperator: res = LogicalAnd(lhs.VariableValue, rhs.VariableValue); break;
                case BotTokenType.LogicalOrOperator: res = LogicalOr(lhs.VariableValue, rhs.VariableValue); break;
                case BotTokenType.PlusOperator: res = Add((dynamic)lhs.VariableValue, (dynamic)rhs.VariableValue); break;
                case BotTokenType.MinusOperator: res = Subtract((dynamic)lhs.VariableValue, (dynamic)rhs.VariableValue); break;
                case BotTokenType.MultiplyOperator: res = Multiply((dynamic)lhs.VariableValue, (dynamic)rhs.VariableValue); break;
                case BotTokenType.DivideOperator: res = Divide((dynamic)lhs.VariableValue, (dynamic)rhs.VariableValue); break;
                case BotTokenType.ModuloOperator: res = Modulo((dynamic)lhs.VariableValue, (dynamic)rhs.VariableValue); break;
                default: return UnsupportedOperatorError(operatorToken);
            }

            if (res.Result == null)
                return (EvalResult.Failed, $"Error evaluating expression: {res.Reason}", input.Current());

            return Success(new VariableBotToken(BotTokenType.Literal, res.Result.StringValue, res.Result));
        }

        // todo: a lot of this code is the same as what's in AssignmentEvaluator::Assign, see if it can be split out/shared
        private (EvalResult, string, BotToken) GetOperand(Dictionary<string, (bool, IIdentifiable)> symbols, BotToken nextToken)
        {
            var operand = nextToken as VariableBotToken;
            if (operand == null)
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

        private (IVariable Result, string Reason) Negate(IVariable variable)
        {
            var boolOperand = CoerceToBool(variable);
            if (boolOperand == null)
                return (null, "Unable to convert variable to bool");
            return (new BoolVariable(!boolOperand.Value), string.Empty);
        }

        private static BoolVariable CoerceToBool(IVariable variable)
        {
            if (variable is BoolVariable boolVar)
                return boolVar;
            else if (variable is IntVariable intVar)
                return new BoolVariable(intVar.Value != 0);
            else if (variable is StringVariable stringVar)
                return new BoolVariable(!string.IsNullOrEmpty(stringVar.Value));
            else
                return null;
        }

        private static (IVariable, string) LogicalAnd(IVariable a, IVariable b)
        {
            var aVal = CoerceToBool(a);
            var bVal = CoerceToBool(b);

            if (aVal == null || bVal == null)
                return (null, $"Error evaluating logical AND expression: operands {a} and {b} could not be coerced to bool");

            return (new BoolVariable(aVal.Value && bVal.Value), string.Empty);
        }

        private static (IVariable, string) LogicalOr(IVariable a, IVariable b)
        {
            var aVal = CoerceToBool(a);
            var bVal = CoerceToBool(b);

            if (aVal == null || bVal == null)
                return (null, $"Error evaluating logical OR expression: operands {a} and {b} could not be coerced to bool");

            return (new BoolVariable(aVal.Value || bVal.Value), string.Empty);
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

        private (IVariable, string) Modulo(IntVariable a, IntVariable b) => (new IntVariable(a.Value % b.Value), string.Empty);
        private (IVariable, string) Modulo(object a, object b) => (null, $"Objects {a} and {b} could not be modulo'd (currently the operands must be int)");
    }
}
