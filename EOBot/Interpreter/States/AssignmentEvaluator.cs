using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class AssignmentEvaluator : BaseEvaluator
    {
        private static readonly BotTokenType[] AssignTokens = [
            BotTokenType.AssignOperator,
            BotTokenType.PlusEquals,
            BotTokenType.MinusEquals,
            BotTokenType.MultiplyEquals,
            BotTokenType.DivideEquals,
            BotTokenType.ModuloEquals,
            BotTokenType.Increment,
            BotTokenType.Decrement,
        ];

        public AssignmentEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            var prefixOperator = input.MatchOneOf(BotTokenType.Increment, BotTokenType.Decrement) ? input.OperationStack.Pop() : null;

            var eval = await Evaluator<VariableEvaluator>().EvaluateAsync(input, ct);
            if (eval.Result != EvalResult.Ok)
                return prefixOperator != null && eval.Result == EvalResult.NotMatch ? Error(input.Current(), BotTokenType.Variable) : eval;

            if (prefixOperator != null)
                input.OperationStack.Push(prefixOperator);
            else if (!input.MatchOneOf(AssignTokens))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            if (input.OperationStack.Peek().IsUnary())
            {
                // hack the unary assignment operators by 
                input.OperationStack.Push(input.OperationStack.Peek().TokenType switch
                {
                    BotTokenType.Increment => new VariableBotToken(BotTokenType.Literal, "1", new IntVariable(1)),
                    BotTokenType.Decrement => new VariableBotToken(BotTokenType.Literal, "-1", new IntVariable(-1)),
                    _ => throw new BotScriptErrorException("Execution should never reach here - was a unary assignment operator added?"),
                });
            }
            else
            {
                eval = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
                if (eval.Result != EvalResult.Ok)
                    return eval;
            }

            // object initializers should keep the operands on the stack so that they can be assigned into the new object later
            // an object initializer is determined to be when an LBrace is on the stack (usually these are not stored on the stack when evaluating blocks)
            // todo: see if it's worth creating a separate assignment evaluator type that does this logic instead of branching based on presence of stack token
            var isObjectInitializer = input.OperationStack.Any(x => x.TokenType == BotTokenType.LBrace);
            if (!isObjectInitializer)
            {
                if (input.OperationStack.Count == 0)
                    return StackEmptyError(input.Current());

                var expressionResult = (VariableBotToken)input.OperationStack.Pop();

                if (input.OperationStack.Count == 0)
                    return StackEmptyError(input.Current());
                var assignOp = input.OperationStack.Pop();
                if (!AssignTokens.Contains(assignOp.TokenType))
                    return StackTokenError(BotTokenType.AssignOperator, assignOp);

                if (input.OperationStack.Count == 0)
                    return StackEmptyError(input.Current());
                var assignmentTarget = (IdentifierBotToken)input.OperationStack.Pop();

                return Assign(input.SymbolTable, assignmentTarget, expressionResult, assignOp);
            }

            return Success();
        }

        protected static (EvalResult, string, BotToken) Assign(Dictionary<string, (bool ReadOnly, IIdentifiable Identifiable)> symbols,
            IdentifierBotToken assignmentTarget,
            VariableBotToken expressionResult,
            BotToken assignOp)
        {
            if (assignmentTarget.Member != null)
            {
                if (!symbols.ContainsKey(assignmentTarget.TokenValue))
                    return IdentifierNotFoundError(assignmentTarget);

                var getVariableRes = symbols.GetVariable<ObjectVariable>(assignmentTarget.TokenValue, assignmentTarget.Indexer);
                if (getVariableRes.Result != EvalResult.Ok)
                {
                    var getRuntimeEvaluatedVariableRes = symbols.GetVariable<RuntimeEvaluatedMemberObjectVariable>(assignmentTarget.TokenValue, assignmentTarget.Indexer);
                    if (getRuntimeEvaluatedVariableRes.Result != EvalResult.Ok)
                        return (EvalResult.Failed, $"Identifier '{assignmentTarget.TokenValue}' is not an object", assignmentTarget);

                    getVariableRes.Result = getRuntimeEvaluatedVariableRes.Result;
                    getVariableRes.Reason = getRuntimeEvaluatedVariableRes.Reason;
                    getVariableRes.Variable = new ObjectVariable(
                        getRuntimeEvaluatedVariableRes.Variable.SymbolTable
                            .Select(x => (x.Key, (x.Value.ReadOnly, x.Value.Variable())))
                            .ToDictionary(x => x.Key, x => x.Item2));
                }

                var targetObject = getVariableRes.Variable;
                return Assign(targetObject.SymbolTable, assignmentTarget.Member, expressionResult, assignOp);
            }

            if (assignmentTarget.Indexer != null)
            {
                if (!symbols.ContainsKey(assignmentTarget.TokenValue))
                    return IdentifierNotFoundError(assignmentTarget);

                var (result, reason, retVar) = symbols.GetVariable(assignmentTarget.TokenValue);
                if (result != EvalResult.Ok)
                    return (result, reason, assignmentTarget);

                if (retVar is DictVariable targetDict)
                {
                    IVariable lhs = null;
                    if (targetDict.Value.TryGetValue(assignmentTarget.Indexer.StringValue, out var v))
                        lhs = v;

                    var (newValue, applyReason) = ApplyOp(assignOp, lhs, expressionResult.VariableValue);
                    if (newValue == null)
                        return (EvalResult.Failed, applyReason, assignOp);

                    targetDict.Value[assignmentTarget.Indexer.StringValue] = newValue;
                }
                else if (retVar is ArrayVariable targetArray)
                {
                    if (assignmentTarget.Indexer is not IntVariable indexVar)
                        return (EvalResult.Failed, $"Expected integer for array index, but got: {assignmentTarget.Indexer} ({assignmentTarget.Indexer.GetType().Name})", assignmentTarget);

                    var (newValue, applyReason) = ApplyOp(assignOp, targetArray.Value[indexVar.Value], expressionResult.VariableValue);
                    if (newValue == null)
                        return (EvalResult.Failed, applyReason, assignOp);

                    targetArray.Value[indexVar.Value] = newValue;
                }
                else
                {
                    return (EvalResult.Failed, $"Indexer operation on non-indexable value: {retVar} ({retVar.GetType().Name})", assignmentTarget);
                }
            }
            else
            {
                if (symbols.ContainsKey(assignmentTarget.TokenValue) && symbols[assignmentTarget.TokenValue].ReadOnly)
                    return ReadOnlyVariableError(assignmentTarget);

                IVariable lhsVar = null;
                if (assignOp.TokenType != BotTokenType.AssignOperator)
                {
                    if (!symbols.ContainsKey(assignmentTarget.TokenValue))
                        return IdentifierNotFoundError(assignmentTarget);

                    if (symbols[assignmentTarget.TokenValue].Identifiable is not IVariable v)
                        return (EvalResult.Failed, $"Expected assignment target {assignmentTarget.TokenValue} to be a variable", assignmentTarget);

                    lhsVar = v;
                }

                var (newValue, reason) = ApplyOp(assignOp, lhsVar, expressionResult.VariableValue);
                if (newValue == null)
                    return (EvalResult.Failed, reason, assignOp);

                if (symbols.ContainsKey(assignmentTarget.TokenValue) &&
                    symbols[assignmentTarget.TokenValue].Identifiable.GetType() != newValue.GetType()
                    && symbols[assignmentTarget.TokenValue].Identifiable is not UndefinedVariable
                    && newValue is not UndefinedVariable)
                {
                    // todo: surface warnings to caller and let caller decide what to do with it instead of making the interpreter write to console directly
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, $"Changing type of variable {assignmentTarget.TokenValue} from {symbols[assignmentTarget.TokenValue].Identifiable.GetType()} to {newValue.GetType()} (at: {assignmentTarget.LineNumber}:{assignmentTarget.Column})", ConsoleColor.DarkYellow);
                }

                symbols[assignmentTarget.TokenValue] = (false, newValue);
            }

            return Success();
        }

        private static (IVariable Result, string Reason) ApplyOp(BotToken assignToken, IVariable lhs, IVariable rhs)
        {
            if (assignToken.IsOneOf(BotTokenType.DivideEquals, BotTokenType.ModuloEquals) && CoerceToInt(rhs) == 0)
                return (null, "Division by zero");

            return (assignToken.TokenType switch
            {
                BotTokenType.PlusEquals when lhs is StringVariable || rhs is StringVariable =>
                    new StringVariable(lhs?.StringValue + rhs.StringValue),

                BotTokenType.AssignOperator => rhs,
                BotTokenType.PlusEquals => new IntVariable(CoerceToInt(lhs) + CoerceToInt(rhs)),
                BotTokenType.MinusEquals => new IntVariable(CoerceToInt(lhs) - CoerceToInt(rhs)),
                BotTokenType.MultiplyEquals => new IntVariable(CoerceToInt(lhs) * CoerceToInt(rhs)),
                BotTokenType.DivideEquals => new IntVariable(CoerceToInt(lhs) / CoerceToInt(rhs)),
                BotTokenType.ModuloEquals => new IntVariable(CoerceToInt(lhs) % CoerceToInt(rhs)),
                BotTokenType.Increment => new IntVariable(CoerceToInt(lhs) + 1),
                BotTokenType.Decrement => new IntVariable(CoerceToInt(lhs) - 1),
                _ => throw new Exception("This code should be unreachable; was a new assign operator added?")
            }, string.Empty);
        }

        private static int CoerceToInt(IVariable variable)
        {
            return variable switch
            {
                IntVariable iv => iv.Value,
                BoolVariable bv => bv.Value ? 1 : 0,
                StringVariable sv => int.TryParse(sv.Value, out var iv) ? iv : 0,
                _ => 0,
            };
        }
    }
}
