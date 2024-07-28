using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States;

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
        var rParen = input.OperationStack.Pop();
        if (rParen.TokenType != BotTokenType.RParen)
            return StackTokenError(BotTokenType.RParen, rParen);

        var parameters = new List<VariableBotToken>();
        while (input.OperationStack.Count > 0 && input.OperationStack.Peek().TokenType != BotTokenType.LParen)
        {
            var parameter = (VariableBotToken)input.OperationStack.Pop();
            parameters.Insert(0, parameter);
        }

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
                await CallAsync(input, (dynamic)function, parameters.Select(x => x.VariableValue).ToArray());
            else if (function is IFunction)
                Call(input, (dynamic)function, parameters.Select(x => x.VariableValue).ToArray());
            else
                return (EvalResult.Failed, $"Expected identifier {functionToken.TokenValue} to be a function, but it was {function.GetType().Name}", functionToken);
        }
        catch (BotScriptErrorException bse)
        {
            // stack information isn't really important since this is only used to signal to the framework that a bot failed
            // recreate the exception so it prints line number/column info with the error
            throw new BotScriptErrorException(bse.Message, functionToken);
        }
        catch (ArgumentException ae)
        {
            return (EvalResult.Failed, ae.Message, functionToken);
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

    private async Task CallAsync(ProgramState input, IAsyncCallable function, params IVariable[] variables)
    {
        await function.CallAsync(variables);
    }

    private async Task CallAsync(ProgramState input, IAsyncCallable<int> function, params IVariable[] variables)
    {
        var result = await function.CallAsync(variables);
        var varResult = new IntVariable(result);
        input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
        input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
    }

    private async Task CallAsync(ProgramState input, IAsyncCallable<string> function, params IVariable[] variables)
    {
        var result = await function.CallAsync(variables);
        var varResult = new StringVariable(result);
        input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
        input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
    }

    private async Task CallAsync(ProgramState input, IAsyncCallable<List<IVariable>> function, params IVariable[] variables)
    {
        var result = await function.CallAsync(variables);
        var varResult = new ArrayVariable(result);
        input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
        input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
    }

    private async Task CallAsync(ProgramState input, IAsyncCallable<bool> function, params IVariable[] variables)
    {
        var result = await function.CallAsync(variables);
        var varResult = new BoolVariable(result);
        input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
        input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
    }

    private async Task CallAsync(ProgramState input, IAsyncCallable<ObjectVariable> function, params IVariable[] variables)
    {
        var varResult = await function.CallAsync(variables);
        input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, varResult);
        input.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, varResult.StringValue, varResult));
    }
}