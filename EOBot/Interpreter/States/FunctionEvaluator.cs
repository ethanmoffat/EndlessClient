using EOBot.Interpreter.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class FunctionEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public FunctionEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
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

                if (!_evaluators.OfType<ExpressionEvaluator>().Single().Evaluate(input))
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

            var function = input.SymbolTable[functionName.TokenValue];
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
                input.SymbolTable[PredefinedIdentifiers.RESULT] = varResult;
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
                input.SymbolTable[PredefinedIdentifiers.RESULT] = new StringVariable(result);
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
                input.SymbolTable[PredefinedIdentifiers.RESULT] = new ArrayVariable(result);
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
                input.SymbolTable[PredefinedIdentifiers.RESULT] = new BoolVariable(result);
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