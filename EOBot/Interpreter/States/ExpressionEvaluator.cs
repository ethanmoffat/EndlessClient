using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Syntax;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class ExpressionEvaluator : CommaDelimitedListEvaluator
    {
        public ExpressionEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            // Check for logical short-circuit condition, terminate expression evaluation early if short-circuited
            if (input.OperationStack.TryPeek(out var previous) && previous.IsBinaryLogicalOperator())
            {
                var res = EvaluateLogicalLeftOperand(input, out var shortCircuited);
                if (res.Result != EvalResult.Ok)
                    return res;

                if (shortCircuited)
                {
                    res = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input, ct);
                    if (res.Result != EvalResult.Ok && res.Result != EvalResult.NotMatch)
                        return res;

                    return await EvaluateStackOperandsAndTernaryAsync(input, ct);
                }
            }

            if (!input.MatchUnaryOperators(out var unaryMinusCount))
            {
                if (input.Match(BotTokenType.LBracket))
                {
                    var isEmptyDict = input.ExpectPair(BotTokenType.Colon, BotTokenType.RBracket);
                    (EvalResult Result, string Reason, BotToken Token) res = isEmptyDict
                        ? Success()
                        : await EvalCommaDelimitedList<CollectionElementEvaluator>(input, BotTokenType.RBracket, ct);
                    if (res.Result == EvalResult.Ok)
                    {
                        // Array or dictionary initializer: create collection from stack params
                        var elements = GetCollectionElementsFromStack(input, BotTokenType.LBracket);
                        var lbracket = input.OperationStack.Pop();
                        if (lbracket.TokenType != BotTokenType.LBracket)
                            return StackTokenError(BotTokenType.LBracket, lbracket);

                        res = CreateCollection(elements, isEmptyDict);
                        if (res.Result != EvalResult.Ok)
                            return res;

                        input.OperationStack.Push(res.Token);

                        // check for an expression tail after collection literal
                        res = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input, ct);
                        if (res.Result != EvalResult.Ok && res.Result != EvalResult.NotMatch)
                            return res;

                        return Success();
                    }
                    else if (res.Result != EvalResult.NotMatch)
                    {
                        return res;
                    }
                }
                else if (input.Match(BotTokenType.LBrace))
                {
                    var res = await EvalCommaDelimitedList<ObjectInitializerEvaluator>(input, BotTokenType.RBrace, ct);
                    if (res.Result == EvalResult.Ok)
                    {
                        // Object initializer: create object from stack params
                        var assignmentPairs = GetAssignmentPairsFromStack(input, BotTokenType.LBrace);
                        var lBrace = input.OperationStack.Pop();
                        if (lBrace.TokenType != BotTokenType.LBrace)
                            return StackTokenError(BotTokenType.LBrace, lBrace);

                        var duplicateMember = assignmentPairs
                            .GroupBy(p => p.Item1.TokenValue)
                            .FirstOrDefault(g => g.Count() > 1);
                        if (duplicateMember != null)
                            return (EvalResult.Failed, $"Duplicate member {duplicateMember.Key} in object initializer", duplicateMember.Last().Item1);

                        var objectVariable = new ObjectVariable(
                            assignmentPairs.ToDictionary(
                                p => p.Item1.TokenValue,
                                v => (false, (IIdentifiable)v.Item2.VariableValue)
                            )
                        );
                        input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, objectVariable.StringValue, objectVariable));

                        // check for an expression tail after object literal
                        res = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input, ct);
                        if (res.Result != EvalResult.Ok && res.Result != EvalResult.NotMatch)
                            return res;

                        return Success();
                    }
                    else if (res.Result != EvalResult.NotMatch)
                    {
                        return res;
                    }
                }
            }

            if (input.Match(BotTokenType.LParen))
            {
                var evalRes = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
                if (evalRes.Result != EvalResult.Ok)
                    return evalRes;

                // if we get an RParen, the nested expression has been evaluated
                var closedParen = false;
                if (!input.Expect(BotTokenType.RParen))
                {
                    // expression_tail is optional
                    evalRes = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input, ct);
                    if (evalRes.Result != EvalResult.Ok && evalRes.Result != EvalResult.NotMatch)
                        return evalRes;

                    if (!input.Expect(BotTokenType.RParen))
                        return Error(input.Current(), BotTokenType.RParen);
                }
                else
                {
                    closedParen = true;
                }

                // take care of the LParen that we matched on
                // LParen is used as a demarcation for when to stop evaluating a stream of tokens as a single expression
                var tmp = input.OperationStack.Pop();

                if (input.OperationStack.Peek().TokenType != BotTokenType.LParen)
                    return StackTokenError(BotTokenType.LParen, input.OperationStack.Peek());

                input.OperationStack.Pop();
                input.OperationStack.Push(tmp);

                evalRes = ApplyUnaryMinus(input, unaryMinusCount);
                if (evalRes.Result != EvalResult.Ok)
                    return evalRes;

                // the tail after the close paren is evaluated once the parenthesized value is an operand, so precedence applies across it
                if (closedParen)
                {
                    evalRes = await Evaluator<ExpressionTailEvaluator>().EvaluateAsync(input, ct);
                    if (evalRes.Result != EvalResult.Ok && evalRes.Result != EvalResult.NotMatch)
                        return evalRes;
                }
            }
            else
            {
                // an expression can be a function call
                var stackDepth = input.OperationStack.Count;
                var functionToken = input.Current();
                var evalRes = await Evaluator<FunctionEvaluator>().EvaluateAsync(input, ct);
                if (evalRes.Result == EvalResult.Ok)
                {
                    if (input.OperationStack.Count == stackDepth)
                        return (EvalResult.Failed, $"Function '{functionToken.TokenValue}' must return a value when used in an expression", functionToken);

                    evalRes = ApplyUnaryMinus(input, unaryMinusCount);
                    if (evalRes.Result != EvalResult.Ok)
                        return evalRes;

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

                    evalRes = ApplyUnaryMinus(input, unaryMinusCount);
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

            return await EvaluateStackOperandsAndTernaryAsync(input, ct);
        }

        private async Task<(EvalResult, string, BotToken)> EvaluateStackOperandsAndTernaryAsync(ProgramState input, CancellationToken ct)
        {
            var res = EvaluateStackOperands(input);
            if (res.Result != EvalResult.Ok || !input.Expect(BotTokenType.QuestionMark))
                return res;

            // We have a ? token so the top of the stack is the evaluated condition.
            var condition = (VariableBotToken)input.OperationStack.Pop();
            var boolValue = CoerceToBool(condition.VariableValue);
            if (boolValue == null)
                return (EvalResult.Failed, $"Error evaluating expression: ternary condition {condition} could not be coerced to bool", condition);

            if (boolValue.Value) // condition true: evaluate expression following '?' and skip ':' expression
            {
                res = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
                if (res.Result != EvalResult.Ok)
                    return res;

                if (!input.Expect(BotTokenType.Colon))
                    return Error(input.Current(), BotTokenType.Colon);

                SkipOperand(input);
                return Success();
            }
            else // condition false: skip expression following '?' and evaluate ':' expression
            {
                SkipOperand(input);

                if (!input.Expect(BotTokenType.Colon))
                    return Error(input.Current(), BotTokenType.Colon);

                return await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
            }
        }

        private static (EvalResult Result, string Reason, BotToken Token) EvaluateLogicalLeftOperand(ProgramState input, out bool shortCircuited)
        {
            shortCircuited = false;

            var logicalOperator = input.OperationStack.Pop();
            var evalRes = EvaluateStackOperands(input, logicalOperator);
            if (evalRes.Result != EvalResult.Ok)
                return evalRes;

            var leftOperand = (VariableBotToken)input.OperationStack.Pop();
            var boolValue = CoerceToBool(leftOperand.VariableValue);
            if (boolValue == null)
                return (EvalResult.Failed, $"Error evaluating expression: operand {leftOperand} of {logicalOperator.TokenType} could not be coerced to bool", leftOperand);

            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, boolValue.StringValue, boolValue));

            if (!(shortCircuited = boolValue.Value == logicalOperator.Is(BotTokenType.LogicalOrOperator)))
            {
                input.OperationStack.Push(logicalOperator);
            }
            else
            {
                // '||' is the only binary operator with lower precedence than '&&', so it ends the right operand of '&&'
                var isAnd = logicalOperator.Is(BotTokenType.LogicalAndOperator);
                SkipOperand(input, current => current.Is(BotTokenType.QuestionMark) || (isAnd && current.Is(BotTokenType.LogicalOrOperator)));
            }

            return Success();
        }

        private static (EvalResult Result, string Reason, BotToken Token) CreateCollection(List<(VariableBotToken Key, VariableBotToken Value)> elements, bool isEmptyDict)
        {
            var isDict = isEmptyDict || elements.Any(x => x.Key != null);
            foreach (var (key, value) in elements)
            {
                if ((key != null) != isDict)
                    return (EvalResult.Failed, "Array elements and dictionary entries cannot be mixed in a collection initializer", key ?? value);
            }

            if (!isDict)
            {
                var arrayVariable = new ArrayVariable(elements.Select(x => x.Value.VariableValue).ToList());
                return (EvalResult.Ok, string.Empty, new VariableBotToken(BotTokenType.Literal, arrayVariable.StringValue, arrayVariable));
            }

            var dict = new Dictionary<string, IVariable>();
            foreach (var (key, value) in elements)
            {
                if (!dict.TryAdd(key.VariableValue.StringValue, value.VariableValue))
                    return (EvalResult.Failed, $"Duplicate key {key.VariableValue.StringValue} in dictionary initializer", key);
            }

            var dictVariable = new DictVariable(dict);
            return (EvalResult.Ok, string.Empty, new VariableBotToken(BotTokenType.Literal, dictVariable.StringValue, dictVariable));
        }

        private static void SkipOperand(ProgramState input, Func<BotToken, bool> endsOperand = null)
        {
            endsOperand ??= _ => false;

            var groupingDepth = 0;
            var ternaryDepth = 0;
            while (input.ExecutionIndex < input.Program.Count)
            {
                var current = input.Current();
                if (current.IsOneOf(BotTokenType.LParen, BotTokenType.LBracket, BotTokenType.LBrace))
                {
                    groupingDepth++;
                }
                else if (current.IsOneOf(BotTokenType.RParen, BotTokenType.RBracket, BotTokenType.RBrace))
                {
                    if (groupingDepth == 0)
                        break;
                    groupingDepth--;
                }
                else if (groupingDepth == 0)
                {
                    var expressionEnd = current.IsOneOf(BotTokenType.Comma, BotTokenType.Semicolon, BotTokenType.NewLine, BotTokenType.EOF);
                    if (expressionEnd || endsOperand(current))
                        break;

                    if (current.Is(BotTokenType.QuestionMark))
                    {
                        ternaryDepth++;
                    }
                    else if (current.Is(BotTokenType.Colon))
                    {
                        if (ternaryDepth == 0)
                            break;
                        ternaryDepth--;
                    }
                }

                input.SkipToken();
            }
        }

        private static (EvalResult Result, string Reason, BotToken Token) EvaluateStackOperands(ProgramState input, BotToken leftOperandOf = null)
        {
            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());

            var syntaxTree = new SyntaxTree(input.OperationStack, leftOperandOf)
            {
                VisitOrder = SyntaxTree.Order.PostOrder
            };

            var (result, reason, token) = EvaluateTree(input, syntaxTree.Root);
            if (result != EvalResult.Ok)
                return (result, reason, token);

            input.OperationStack.Push(token);

            return Success();
        }

        private static (EvalResult, string, BotToken) EvaluateTree(ProgramState input, SyntaxTree.Node node)
        {
            if (node.Token.IsUnary())
            {
                if (node.Left != null)
                    RestoreToStack(input.OperationStack, node.Right);

                var (res, reason, operand) = node.Left != null
                    ? EvaluateTree(input, node.Left)
                    : node.Right != null
                        ? EvaluateTree(input, node.Right)
                        : (EvalResult.Failed, "Error evaluating expression: no operands for operator", node.Token);
                if (res == EvalResult.Failed)
                    return (res, reason, operand);
                if (operand is not VariableBotToken variable || operand.TokenType == BotTokenType.TypeSpecifier)
                    return (EvalResult.Failed, $"Error evaluating expression: expected operand but got {operand.TokenType}", operand);

                return HandleUnaryOperator(input, node.Token, variable);
            }
            else if (node.Token.IsBinary())
            {
                var (result, reason, resolved) = EvaluateTree(input, node.Right);
                if (result == EvalResult.Failed)
                    return (result, reason, node.Right.Token);
                if (resolved is not VariableBotToken lhs || resolved.TokenType == BotTokenType.TypeSpecifier)
                    return (EvalResult.Failed, $"Error evaluating expression: expected operand but got {resolved.TokenType}", resolved);

                (result, reason, resolved) = EvaluateTree(input, node.Left);
                if (result == EvalResult.Failed)
                    return (result, reason, node.Left.Token);
                if (resolved is not VariableBotToken rhs)
                    return (EvalResult.Failed, $"Error evaluating expression: expected operand but got {resolved.TokenType}", resolved);

                // special case: check that 'is' operator is comparing against type specifier tokens (and that anything else has no type specifiers)
                if (node.Token.TokenType == BotTokenType.IsOperator && resolved.TokenType != BotTokenType.TypeSpecifier
                 || node.Token.TokenType != BotTokenType.IsOperator && resolved.TokenType == BotTokenType.TypeSpecifier)
                {
                    return (EvalResult.Failed, $"Error evaluating expression: expected valid operand for {node.Token.TokenType} operator, but got {resolved.TokenType}", resolved);
                }

                return HandleBinaryOperator(input, node.Token, lhs, rhs);
            }
            else
            {
                // Multiple parameters to a function will be popped off the stack and added to the expression tree since there are no delimiters
                //   between them. Any additional parameters will be "orphaned" if not restored to the stack recursively.
                RestoreToStack(input.OperationStack, node.Left);
                RestoreToStack(input.OperationStack, node.Right);

                return GetOperand(input.SymbolTable, node.Token);
            }

            static void RestoreToStack(Stack<BotToken> opStack, SyntaxTree.Node node)
            {
                if (node == null)
                    return;

                RestoreToStack(opStack, node.Left);
                RestoreToStack(opStack, node.Right);

                opStack.Push(node.Token);
            }
        }

        // UnaryMinus is not a detected token type. The minus operations are applied prior to evaluation so the values
        //   are processed by the tree with the correctly stacked negation.
        private static (EvalResult, string, BotToken) ApplyUnaryMinus(ProgramState input, int unaryMinusCount)
        {
            if (unaryMinusCount == 0)
                return Success();

            var (result, reason, operand) = GetOperand(input.SymbolTable, input.OperationStack.Pop());
            if (result != EvalResult.Ok)
                return (result, reason, operand);

            var value = ((VariableBotToken)operand).VariableValue;
            for (; unaryMinusCount > 0; unaryMinusCount--)
            {
                input.OperationStack.Pop();

                (IVariable negateResult, reason) = Negate(value);
                if (negateResult == null)
                    return (EvalResult.Failed, $"Error evaluating expression: {reason}", input.Current());

                value = negateResult;
            }

            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, value.StringValue, value));
            return Success();
        }

        private static (EvalResult, string, BotToken) HandleUnaryOperator(ProgramState input, BotToken operatorToken, VariableBotToken operand)
        {
            (IVariable Result, string Reason) res;
            res.Reason = string.Empty;
            switch (operatorToken.TokenType)
            {
                case BotTokenType.NotOperator: res = LogicalNegate(operand.VariableValue); break;
                default: return UnsupportedOperatorError(operatorToken);
            }

            if (res.Result == null)
                return (EvalResult.Failed, $"Error evaluating expression: {res.Reason}", input.Current());

            return Success(new VariableBotToken(BotTokenType.Literal, res.Result.StringValue, res.Result));
        }

        private static (EvalResult, string, BotToken) HandleBinaryOperator(ProgramState input, BotToken operatorToken, VariableBotToken lhs, VariableBotToken rhs)
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
                case BotTokenType.IsOperator: res.Result = new BoolVariable(IsType(lhs.VariableValue, rhs.VariableValue)); break;
                case BotTokenType.StrictEqualOperator:
                case BotTokenType.StrictNotEqualOperator: res = StrictCompare(lhs.VariableValue, rhs.VariableValue, operatorToken.TokenType == BotTokenType.StrictEqualOperator); break;
                default: return UnsupportedOperatorError(operatorToken);
            }

            if (res.Result == null)
                return (EvalResult.Failed, $"Error evaluating expression: {res.Reason}", input.Current());

            return Success(new VariableBotToken(BotTokenType.Literal, res.Result.StringValue, res.Result));
        }

        private static (EvalResult, string, BotToken) GetOperand(Dictionary<string, (bool, IIdentifiable)> symbols, BotToken nextToken)
        {
            if (nextToken is not VariableBotToken operand)
            {
                if (nextToken is LiteralBotToken lbt)
                {
                    if (nextToken.TokenValue == "undefined")
                        return Success(new VariableBotToken(BotTokenType.Literal, nextToken.TokenValue, UndefinedVariable.Instance));
                    else if (lbt.LiteralValue is int iv)
                        return Success(new VariableBotToken(BotTokenType.Literal, nextToken.TokenValue, new IntVariable(iv)));
                    else if (lbt.LiteralValue is bool bv)
                        return Success(new VariableBotToken(BotTokenType.Literal, nextToken.TokenValue, new BoolVariable(bv)));
                    else
                        return Success(new VariableBotToken(BotTokenType.Literal, nextToken.TokenValue, new StringVariable(nextToken.TokenValue)));
                }
                else if (nextToken.TokenType == BotTokenType.TypeSpecifier)
                {
                    // convert type specifier to a variable of that type with the default value
                    // the variable value doesn't matter because type specifiers should only be used in `is` comparisons
                    IVariable variableValue = nextToken.TokenValue switch
                    {
                        BotTokenParser.KEYWORD_INT => new IntVariable(default),
                        BotTokenParser.KEYWORD_STRING => new StringVariable(default),
                        BotTokenParser.KEYWORD_BOOL => new BoolVariable(default),
                        BotTokenParser.KEYWORD_OBJECT => new ObjectVariable(default),
                        BotTokenParser.KEYWORD_ARRAY => new ArrayVariable(default),
                        BotTokenParser.KEYWORD_DICT => new DictVariable(default),
                        _ => throw new InvalidOperationException($"Type specifier token {nextToken} had unexpected value: {nextToken.TokenValue} (not a recognized type specifier)."),
                    };
                    return Success(new VariableBotToken(nextToken.TokenType, nextToken.TokenValue, variableValue));
                }

                return symbols.ResolveIdentifier(nextToken);
            }

            return Success(operand);
        }

        private static (IVariable Result, string Reason) LogicalNegate(IVariable variable)
        {
            var boolOperand = CoerceToBool(variable);
            if (boolOperand == null)
                return (null, "Unable to convert variable to bool");
            return (new BoolVariable(!boolOperand.Value), string.Empty);
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

        private static (IVariable, string) Negate(IVariable variable)
        {
            return variable switch
            {
                IntVariable iv => (new IntVariable(-iv.Value), string.Empty),
                _ => (null, $"Variable of type {variable.GetType().Name} could not be negated.")
            };
        }

        private static (IVariable, string) Add(IntVariable a, IntVariable b) => (new IntVariable(a.Value + b.Value), string.Empty);
        private static (IVariable, string) Add(StringVariable a, StringVariable b) => (new StringVariable(a.Value + b.Value), string.Empty);
        private static (IVariable, string) Add(IVariable a, StringVariable b) => (new StringVariable(a.StringValue + b.Value), string.Empty);
        private static (IVariable, string) Add(StringVariable a, IVariable b) => (new StringVariable(a.Value + b.StringValue), string.Empty);
        private static (IVariable, string) Add(object a, object b) => (null, $"Objects {a} and {b} could not be added (currently the operands must be int or convertable to variable)");

        private static (IVariable, string) Subtract(IntVariable a, IntVariable b) => (new IntVariable(a.Value - b.Value), string.Empty);
        private static (IVariable, string) Subtract(object a, object b) => (null, $"Objects {a} and {b} could not be subtracted (currently the operands must be int)");

        private static (IVariable, string) Multiply(IntVariable a, IntVariable b) => (new IntVariable(a.Value * b.Value), string.Empty);
        private static (IVariable, string) Multiply(object a, object b) => (null, $"Objects {a} and {b} could not be multiplied (currently the operands must be int)");

        private static (IVariable, string) Divide(IntVariable a, IntVariable b) => b.Value == 0 ? (null, "Division by zero") : (new IntVariable(a.Value / b.Value), string.Empty);
        private static (IVariable, string) Divide(object a, object b) => (null, $"Objects {a} and {b} could not be divided (currently the operands must be int)");

        private static (IVariable, string) Modulo(IntVariable a, IntVariable b) => b.Value == 0 ? (null, "Division by zero") : (new IntVariable(a.Value % b.Value), string.Empty);
        private static (IVariable, string) Modulo(object a, object b) => (null, $"Objects {a} and {b} could not be modulo'd (currently the operands must be int)");

        private static bool IsType(IVariable variable, IVariable typeSpecifier)
        {
            if (typeSpecifier is EnumVariable enumTypeSpecifier)
                return variable is EnumVariable enumVariable && enumVariable.IsInstanceOf(enumTypeSpecifier);

            return variable.GetType().Equals(typeSpecifier.GetType());
        }

        private static (IVariable, string) StrictCompare(IVariable lhs, IVariable rhs, bool expectEqual)
        {
            if (!lhs.GetType().Equals(rhs.GetType()))
            {
                return (new BoolVariable(!expectEqual), string.Empty);
            }

            return (new BoolVariable(expectEqual == lhs.Equals(rhs)), string.Empty);
        }
    }
}
