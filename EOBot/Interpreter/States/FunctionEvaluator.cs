using EOBot.Interpreter.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class FunctionEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public FunctionEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public async Task<bool> EvaluateAsync(ProgramState input)
        {
            if (!input.MatchPair(BotTokenType.Identifier, BotTokenType.LParen))
                return false;

            var firstParam = true;
            while (!input.Match(BotTokenType.RParen))
            {
                if (input.Expect(BotTokenType.NewLine) || input.Expect(BotTokenType.EOF))
                    return false;

                if (!firstParam && !input.Expect(BotTokenType.Comma))
                    return false;

                if (!await _evaluators.OfType<ExpressionEvaluator>().Single().EvaluateAsync(input))
                    return false;

                firstParam = false;
            }

            if (input.OperationStack.Count == 0)
                return false;
            var endParen = input.OperationStack.Pop();
            if (endParen.TokenType != BotTokenType.RParen)
                return false;

            var parameters = new List<VariableBotToken>();
            while (input.OperationStack.Count > 0 && input.OperationStack.Peek().TokenType != BotTokenType.LParen)
            {
                var parameter = (VariableBotToken)input.OperationStack.Pop();
                parameters.Insert(0, parameter);
            }

            input.OperationStack.Pop(); // LParen

            if (input.OperationStack.Count == 0)
                return false;
            var functionName = input.OperationStack.Pop();

            var function = input.SymbolTable[functionName.TokenValue].Identifiable;

            if (function is IAsyncFunction)
                return await CallAsync(input, (dynamic)function, parameters.Select(x => x.VariableValue).ToArray());

            return Call(input, (dynamic)function, parameters.Select(x => x.VariableValue).ToArray());
        }

        private bool Call(ProgramState input, ICallable function, params IVariable[] variables)
        {
            try
            {
                function.Call(variables);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private bool Call(ProgramState input, ICallable<int> function, params IVariable[] variables)
        {
            try
            {
                var result = function.Call(variables);
                var varResult = new IntVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private bool Call(ProgramState input, ICallable<string> function, params IVariable[] variables)
        {
            try
            {
                var result = function.Call(variables);
                var varResult = new StringVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private bool Call(ProgramState input, ICallable<List<IVariable>> function, params IVariable[] variables)
        {
            try
            {
                var result = function.Call(variables);
                var varResult = new ArrayVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private bool Call(ProgramState input, ICallable<bool> function, params IVariable[] variables)
        {
            try
            {
                var result = function.Call(variables);
                var varResult = new BoolVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> CallAsync(ProgramState input, IAsyncCallable function, params IVariable[] variables)
        {
            try
            {
                await function.CallAsync(variables);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> CallAsync(ProgramState input, IAsyncCallable<int> function, params IVariable[] variables)
        {
            try
            {
                var result = await function.CallAsync(variables);
                var varResult = new IntVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> CallAsync(ProgramState input, IAsyncCallable<string> function, params IVariable[] variables)
        {
            try
            {
                var result = await function.CallAsync(variables);
                var varResult = new StringVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> CallAsync(ProgramState input, IAsyncCallable<List<IVariable>> function, params IVariable[] variables)
        {
            try
            {
                var result = await function.CallAsync(variables);
                var varResult = new ArrayVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> CallAsync(ProgramState input, IAsyncCallable<bool> function, params IVariable[] variables)
        {
            try
            {
                var result = await function.CallAsync(variables);
                var varResult = new BoolVariable(result);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
                input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
    }
}