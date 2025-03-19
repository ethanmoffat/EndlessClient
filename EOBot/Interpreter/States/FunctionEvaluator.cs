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
            if (parameters.OfType<VariableBotToken>().Any(x => x.VariableValue is UndefinedVariable))
                return (EvalResult.Failed, "Function parameter is undefined", input.Current());

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
                    await CallAsync(input, ct, (dynamic)function, parameters.Select(x => x.VariableValue).ToArray()).ConfigureAwait(false);
                else if (function is IFunction)
                    Call(input, (dynamic)function, parameters.Select(x => x.VariableValue).ToArray());
                else if (function is IUserDefinedFunction udf)
                    return await udf.CallAsync(input, ct, parameters.Select(x => x.VariableValue).ToArray());
                else
                    return (EvalResult.Failed, $"Expected identifier {functionToken.TokenValue} to be a function, but it was {function.GetType().Name}", functionToken);
            }
            catch (BotScriptErrorException bse)
            {
                // stack information isn't really important since this is only used to signal to the framework that a bot failed
                // recreate the exception so it prints line number/column info with the error
                throw new BotScriptErrorException(bse.Message, functionToken, input.CallStack);
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }
            catch (TaskCanceledException)
            {
                return (EvalResult.Cancelled, string.Empty, functionToken);
            }

            return Success();
        }

        private void Call(ProgramState input, ICallable function, params IVariable[] variables)
        {
            function.Call(variables);
        }

        private void Call(ProgramState input, ICallable<int> function, params IVariable[] variables)
        {
            var result = function.Call(variables);
            var varResult = new IntVariable(result);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }

        private void Call(ProgramState input, ICallable<string> function, params IVariable[] variables)
        {
            var result = function.Call(variables);
            var varResult = new StringVariable(result);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }

        private void Call(ProgramState input, ICallable<List<IVariable>> function, params IVariable[] variables)
        {
            var result = function.Call(variables);
            var varResult = new ArrayVariable(result);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }

        private void Call(ProgramState input, ICallable<Dictionary<string, IVariable>> function, params IVariable[] variables)
        {
            var result = function.Call(variables);
            var varResult = new DictVariable(result);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }

        private void Call(ProgramState input, ICallable<bool> function, params IVariable[] variables)
        {
            var result = function.Call(variables);
            var varResult = new BoolVariable(result);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }

        private void Call(ProgramState input, ICallable<ObjectVariable> function, params IVariable[] variables)
        {
            var varResult = function.Call(variables);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }

        private async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable function, params IVariable[] variables)
        {
            await function.CallAsync(ct, variables).ConfigureAwait(false);
        }

        private async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<int> function, params IVariable[] variables)
        {
            var result = await function.CallAsync(ct, variables).ConfigureAwait(false);
            var varResult = new IntVariable(result);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }

        private async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<string> function, params IVariable[] variables)
        {
            var result = await function.CallAsync(ct, variables).ConfigureAwait(false);
            var varResult = new StringVariable(result);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }

        private async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<List<IVariable>> function, params IVariable[] variables)
        {
            var result = await function.CallAsync(ct, variables).ConfigureAwait(false);
            var varResult = new ArrayVariable(result);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }

        private async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<bool> function, params IVariable[] variables)
        {
            var result = await function.CallAsync(ct, variables).ConfigureAwait(false);
            var varResult = new BoolVariable(result);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }

        private async Task CallAsync(ProgramState input, CancellationToken ct, IAsyncCallable<ObjectVariable> function, params IVariable[] variables)
        {
            var varResult = await function.CallAsync(ct, variables).ConfigureAwait(false);
            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
            input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
        }
    }
}
