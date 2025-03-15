using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.States;

namespace EOBot.Interpreter.Variables
{
    public class UserDefinedFunction : IUserDefinedFunction
    {
        private readonly ProgramState _funcState;
        private readonly List<BotToken> _paramSpecs;

        public string StringValue { get; }

        public UserDefinedFunction(string functionName, List<BotToken> functionTokens, List<BotToken> paramSpecs)
        {
            StringValue = functionName;

            // constructed here so any nested functions will be created/evaluated at program state initialization time
            _funcState = new ProgramState(functionTokens);

            _paramSpecs = paramSpecs;
        }

        public virtual async Task<(EvalResult, string, BotToken)> CallAsync(ProgramState programState, CancellationToken ct, params IIdentifiable[] parameters)
        {
            if (parameters.Length != _paramSpecs.Count)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            var originalSymbols = new Dictionary<string, (bool, IIdentifiable)>(programState.SymbolTable);
            _funcState.InheritFrom(programState);
            var readOnlyItems = _funcState.SymbolTable.Where(x => x.Value.ReadOnly);

            for (int i = 0; i < parameters.Length; i++)
            {
                if (readOnlyItems.Any(x => x.Key == _paramSpecs[i].TokenValue))
                    return ParameterOverwritesBuiltinError(_paramSpecs[i]);

                _funcState.SymbolTable[_paramSpecs[i].TokenValue] = (false, parameters[i]);
            }

            _funcState.CallStack.Push((StringValue, _funcState.Program[0], programState.ExecutionIndex));
            var (evalResult, reason, token) = await ScriptEvaluator.Instance.EvaluateAsync(_funcState, ct);

            if (evalResult != EvalResult.Failed)
            {
                if (_funcState.SymbolTable.TryGetValue(PredefinedIdentifiers.RESULT, out var resultVar))
                {
                    programState.SymbolTable[PredefinedIdentifiers.RESULT] = resultVar;
                    if (resultVar.Identifiable is not IVariable iv)
                        return (EvalResult.Failed, $"Expected result to be a variable, but got {resultVar.Identifiable.GetType()}", programState.Current());
                    programState.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, iv.StringValue, iv));
                }

                // restore symbol table to state it was previously in prior to the function invocation
                //   - this method of restoration allows for variables in the parent scope to be overwritten
                //   - also want to restore the values of any parameters so that original values in outer
                //     scope are preserved
                var removeKeys = programState.SymbolTable.Keys.Where(x => !originalSymbols.ContainsKey(x));
                foreach (var key in removeKeys)
                    programState.SymbolTable.Remove(key);

                foreach (var param in _paramSpecs)
                    if (programState.SymbolTable.ContainsKey(param.TokenValue))
                        programState.SymbolTable[param.TokenValue] = originalSymbols[param.TokenValue];

                _funcState.CallStack.Pop();
            }

            return (evalResult, reason, token);
        }

        private static (EvalResult, string, BotToken) ParameterOverwritesBuiltinError(BotToken paramSpec)
        {
            return (EvalResult.Failed, $"Parameter {paramSpec.TokenValue} overrides built-in variable or function.", paramSpec);
        }
    }
}
