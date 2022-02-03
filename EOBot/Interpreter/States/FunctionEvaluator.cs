using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class FunctionEvaluator : BaseEvaluator
    {
        public FunctionEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            if (!input.MatchPair(BotTokenType.Identifier, BotTokenType.LParen))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            var firstParam = true;
            while (!input.Match(BotTokenType.RParen))
            {
                if (input.Expect(BotTokenType.NewLine) || input.Expect(BotTokenType.EOF))
                    return UnexpectedTokenError(input.Current(), BotTokenType.NewLine, BotTokenType.EOF);

                if (!firstParam && !input.Expect(BotTokenType.Comma))
                    return Error(input.Current(), BotTokenType.Comma);

                var parameterExpression = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input);
                if (parameterExpression.Result != EvalResult.Ok)
                    return parameterExpression;

                firstParam = false;
            }

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());
            var endParen = input.OperationStack.Pop();
            if (endParen.TokenType != BotTokenType.RParen)
                return StackTokenError(BotTokenType.RParen, endParen);

            var parameters = new List<VariableBotToken>();
            while (input.OperationStack.Count > 0 && input.OperationStack.Peek().TokenType != BotTokenType.LParen)
            {
                var parameter = (VariableBotToken)input.OperationStack.Pop();
                parameters.Insert(0, parameter);
            }

            // todo: check this result
            input.OperationStack.Pop(); // LParen

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());
            var functionToken = input.OperationStack.Pop();

            // todo: error when function not found
            var function = input.SymbolTable[functionToken.TokenValue].Identifiable;

            if (function is IAsyncFunction)
                return await CallAsync(input, functionToken, (dynamic)function, parameters.Select(x => x.VariableValue).ToArray());

            return Call(input, functionToken, (dynamic)function, parameters.Select(x => x.VariableValue).ToArray());
        }

        private (EvalResult, string, BotToken) Call(ProgramState input, BotToken functionToken, ICallable function, params IVariable[] variables)
        {
            try
            {
                function.Call(variables);
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }

            return Success();
        }

        private (EvalResult, string, BotToken) Call(ProgramState input, BotToken functionToken, ICallable<int> function, params IVariable[] variables)
        {
            try
            {
                var result = function.Call(variables);
                var varResult = new IntVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }

            return Success();
        }

        private (EvalResult, string, BotToken) Call(ProgramState input, BotToken functionToken, ICallable<string> function, params IVariable[] variables)
        {
            try
            {
                var result = function.Call(variables);
                var varResult = new StringVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }

            return Success();
        }

        private (EvalResult, string, BotToken) Call(ProgramState input, BotToken functionToken, ICallable<List<IVariable>> function, params IVariable[] variables)
        {
            try
            {
                var result = function.Call(variables);
                var varResult = new ArrayVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }

            return Success();
        }

        private (EvalResult, string, BotToken) Call(ProgramState input, BotToken functionToken, ICallable<bool> function, params IVariable[] variables)
        {
            try
            {
                var result = function.Call(variables);
                var varResult = new BoolVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }

            return Success();
        }

        private async Task<(EvalResult, string, BotToken)> CallAsync(ProgramState input, BotToken functionToken, IAsyncCallable function, params IVariable[] variables)
        {
            try
            {
                await function.CallAsync(variables);
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }

            return Success();
        }

        private async Task<(EvalResult, string, BotToken)> CallAsync(ProgramState input, BotToken functionToken, IAsyncCallable<int> function, params IVariable[] variables)
        {
            try
            {
                var result = await function.CallAsync(variables);
                var varResult = new IntVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }

            return Success();
        }

        private async Task<(EvalResult, string, BotToken)> CallAsync(ProgramState input, BotToken functionToken, IAsyncCallable<string> function, params IVariable[] variables)
        {
            try
            {
                var result = await function.CallAsync(variables);
                var varResult = new StringVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }

            return Success();
        }

        private async Task<(EvalResult, string, BotToken)> CallAsync(ProgramState input, BotToken functionToken, IAsyncCallable<List<IVariable>> function, params IVariable[] variables)
        {
            try
            {
                var result = await function.CallAsync(variables);
                var varResult = new ArrayVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }

            return Success();
        }

        private async Task<(EvalResult, string, BotToken)> CallAsync(ProgramState input, BotToken functionToken, IAsyncCallable<bool> function, params IVariable[] variables)
        {
            try
            {
                var result = await function.CallAsync(variables);
                var varResult = new BoolVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException ae)
            {
                return (EvalResult.Failed, ae.Message, functionToken);
            }

            return Success();
        }
    }
}