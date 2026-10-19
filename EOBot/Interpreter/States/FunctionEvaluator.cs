using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Mapping;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class FunctionEvaluator : CommaDelimitedListEvaluator
    {
        public FunctionEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            var name = input.PeekQualifiedName();
            var lParenIndex = input.ExecutionIndex + name.TokenLength;
            if (name.IsEmpty || lParenIndex >= input.Program.Count || !input.Program[lParenIndex].Is(BotTokenType.LParen))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            // modules are resolved from the qualified name; only the function name and paren are pushed
            input.Goto(lParenIndex - 1);
            if (!input.MatchPair(BotTokenType.Identifier, BotTokenType.LParen))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            var res = await EvalCommaDelimitedList<ExpressionEvaluator>(input, BotTokenType.RParen, ct);
            if (res.Result != EvalResult.Ok)
                return res;

            var parameters = GetParametersFromStack(input, BotTokenType.LParen);

            var lParen = input.OperationStack.Pop();
            if (lParen.TokenType != BotTokenType.LParen)
                return StackTokenError(BotTokenType.LParen, lParen);

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());
            var functionToken = input.OperationStack.Pop();

            res = input.SymbolTable.TryResolveQualifiedName(name, out var function);
            if (res.Result != EvalResult.Ok)
                return res;

            string functionName = name;

            try
            {
                if (function is IAsyncFunction)
                {
                    await CallAsync(input, ct, (dynamic)function, parameters.Select(x => x.VariableValue).ToArray()).ConfigureAwait(false);
                }
                else if (function is IFunction)
                {
                    Call(input, (dynamic)function, parameters.Select(x => x.VariableValue).ToArray());
                }
                else if (function is DotNetFunction dotNetFunction)
                {
                    var result = await dotNetFunction.CallAsync(parameters.Select(x => x.VariableValue).ToArray(), ct).ConfigureAwait(false);
                    if (result is IntVariable { Truncated: true })
                        ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, $"Return value of {functionName} does not fit in a script integer and was truncated to {result.StringValue} (at: {functionToken.LineNumber}:{functionToken.Column})", ConsoleColor.DarkYellow);
                    if (result != null)
                        PushResultToOperationStack(input, result);
                }
                else if (function is IUserDefinedFunction udf)
                {
                    if (input.CallStack.Count >= UserDefinedFunction.MaxCallDepth)
                    {
                        return (EvalResult.Failed, $"Maximum call depth of {UserDefinedFunction.MaxCallDepth} exceeded calling function '{functionName}'", functionToken);
                    }

                    return await udf.CallAsync(input, functionToken, ct, parameters.Select(x => x.VariableValue).ToArray());
                }
                else
                {
                    return (EvalResult.Failed, $"Expected identifier {functionName} to be a function, but it was {function.GetType().Name}", functionToken);
                }
            }
            catch (BotScriptErrorException bse) when (!bse.HasLocation)
            {
                // recreate the exception at the innermost call so it prints line number/column info and the call stack with the error
                throw new BotScriptErrorException(bse.Message, functionToken, input.CallStack);
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                return (EvalResult.Failed, $"Invalid argument type calling function '{functionName}'", functionToken);
            }
            catch (TaskCanceledException)
            {
                return (EvalResult.Cancelled, string.Empty, functionToken);
            }

            return Success();
        }

        private static void Call(ProgramState input, ICallable function, params IVariable[] variables) => function.Call(variables);

        private static void Call(ProgramState input, ICallable<int> function, params IVariable[] variables) => PushResultToOperationStack(input, new IntVariable(function.Call(variables)));

        private static void Call(ProgramState input, ICallable<string> function, params IVariable[] variables) => PushResultToOperationStack(input, new StringVariable(function.Call(variables)));

        private static void Call(ProgramState input, ICallable<List<IVariable>> function, params IVariable[] variables) => PushResultToOperationStack(input, new ArrayVariable(function.Call(variables)));

        private static void Call(ProgramState input, ICallable<Dictionary<string, IVariable>> function, params IVariable[] variables) => PushResultToOperationStack(input, new DictVariable(function.Call(variables)));

        private static void Call(ProgramState input, ICallable<bool> function, params IVariable[] variables) => PushResultToOperationStack(input, new BoolVariable(function.Call(variables)));

        private static void Call(ProgramState input, ICallable<ObjectVariable> function, params IVariable[] variables) => PushResultToOperationStack(input, function.Call(variables));

        private static void Call(ProgramState input, ICallable<IVariable> function, params IVariable[] variables) => PushResultToOperationStack(input, function.Call(variables));

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable function, params IVariable[] variables) => await function.CallAsync(ct, variables).ConfigureAwait(false);

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<int> function, params IVariable[] variables) => PushResultToOperationStack(input, new IntVariable(await function.CallAsync(ct, variables).ConfigureAwait(false)));

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<string> function, params IVariable[] variables) => PushResultToOperationStack(input, new StringVariable(await function.CallAsync(ct, variables).ConfigureAwait(false)));

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<List<IVariable>> function, params IVariable[] variables) => PushResultToOperationStack(input, new ArrayVariable(await function.CallAsync(ct, variables).ConfigureAwait(false)));

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<bool> function, params IVariable[] variables) => PushResultToOperationStack(input, new BoolVariable(await function.CallAsync(ct, variables).ConfigureAwait(false)));

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<ObjectVariable> function, params IVariable[] variables) => PushResultToOperationStack(input, await function.CallAsync(ct, variables).ConfigureAwait(false));

        private static void PushResultToOperationStack(ProgramState input, IVariable result) => input.OperationStack.Push(VariableBotToken.FromComputedValue(BotTokenType.Literal, result));
    }
}
