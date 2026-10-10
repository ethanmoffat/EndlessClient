using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
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

            if (!input.SymbolTable.ContainsKey(functionToken.TokenValue))
                return IdentifierNotFoundError(new IdentifierBotToken(functionToken));

            var function = input.SymbolTable[functionToken.TokenValue].Identifiable;

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
                else if (function is IUserDefinedFunction udf)
                {
                    if (input.CallStack.Count >= UserDefinedFunction.MaxCallDepth)
                    {
                        return (EvalResult.Failed, $"Maximum call depth of {UserDefinedFunction.MaxCallDepth} exceeded calling function '{functionToken.TokenValue}'", functionToken);
                    }

                    return await udf.CallAsync(input, functionToken, ct, parameters.Select(x => x.VariableValue).ToArray());
                }
                else
                {
                    return (EvalResult.Failed, $"Expected identifier {functionToken.TokenValue} to be a function, but it was {function.GetType().Name}", functionToken);
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
                return (EvalResult.Failed, $"Invalid argument type calling function '{functionToken.TokenValue}'", functionToken);
            }
            catch (TaskCanceledException)
            {
                return (EvalResult.Cancelled, string.Empty, functionToken);
            }

            return Success();
        }

        private static void Call(ProgramState input, ICallable function, params IVariable[] variables) => function.Call(variables);

        private static void Call(ProgramState input, ICallable<int> function, params IVariable[] variables) => Push(input, new IntVariable(function.Call(variables)));

        private static void Call(ProgramState input, ICallable<string> function, params IVariable[] variables) => Push(input, new StringVariable(function.Call(variables)));

        private static void Call(ProgramState input, ICallable<List<IVariable>> function, params IVariable[] variables) => Push(input, new ArrayVariable(function.Call(variables)));

        private static void Call(ProgramState input, ICallable<Dictionary<string, IVariable>> function, params IVariable[] variables) => Push(input, new DictVariable(function.Call(variables)));

        private static void Call(ProgramState input, ICallable<bool> function, params IVariable[] variables) => Push(input, new BoolVariable(function.Call(variables)));

        private static void Call(ProgramState input, ICallable<ObjectVariable> function, params IVariable[] variables) => Push(input, function.Call(variables));

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable function, params IVariable[] variables) => await function.CallAsync(ct, variables).ConfigureAwait(false);

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<int> function, params IVariable[] variables) => Push(input, new IntVariable(await function.CallAsync(ct, variables).ConfigureAwait(false)));

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<string> function, params IVariable[] variables) => Push(input, new StringVariable(await function.CallAsync(ct, variables).ConfigureAwait(false)));

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<List<IVariable>> function, params IVariable[] variables) => Push(input, new ArrayVariable(await function.CallAsync(ct, variables).ConfigureAwait(false)));

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<bool> function, params IVariable[] variables) => Push(input, new BoolVariable(await function.CallAsync(ct, variables).ConfigureAwait(false)));

        private static async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<ObjectVariable> function, params IVariable[] variables) => Push(input, await function.CallAsync(ct, variables).ConfigureAwait(false));

        private static void Push(ProgramState input, IVariable result) => input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, result.StringValue, result));
    }
}
