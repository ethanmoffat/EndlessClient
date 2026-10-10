using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.States;

namespace EOBot.Interpreter.Variables
{
    public class UserDefinedFunction : IUserDefinedFunction
    {
        public const int MaxCallDepth = 1000;

        private readonly ProgramDefinition _definition;
        private readonly List<BotToken> _paramSpecs;

        public string StringValue { get; }

        public UserDefinedFunction(string functionName, ProgramDefinition definition, List<BotToken> paramSpecs)
        {
            StringValue = functionName;
            _definition = definition;
            _paramSpecs = paramSpecs;
        }

        public virtual async Task<(EvalResult, string, BotToken)> CallAsync(ProgramState programState, BotToken callSite, CancellationToken ct, params IIdentifiable[] parameters)
        {
            if (parameters.Length != _paramSpecs.Count)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            using var scope = new FunctionScope(_definition, programState, functionName: StringValue, callSite);

            var bindResult = scope.BindParameters(_paramSpecs, parameters);
            if (bindResult.Result != EvalResult.Ok)
                return bindResult;

            // Evaluator awaits mostly complete synchronously, so nested script calls accumulate real stack frames on one thread
            //   and deep recursion would overflow it (uncatchable). When the stack runs low, Task.Yield always suspends; every
            //   caller up the chain unwinds and execution resumes on a thread pool thread (or the host's synchronization context)
            //   with a fresh stack. The logical call chain continues as async state machines on the heap.
            if (!RuntimeHelpers.TryEnsureSufficientExecutionStack())
                await Task.Yield();

            var (evalResult, reason, token) = await ScriptEvaluator.Instance.EvaluateAsync(scope.State, ct);

            IVariable returnValue = UndefinedVariable.Instance;
            if (evalResult == EvalResult.ControlFlow)
            {
                if (!scope.State.OperationStack.TryPop(out var controlToken) || !controlToken.Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_RETURN))
                    return (EvalResult.Failed, $"'{controlToken?.TokenValue}' is not valid outside of a loop", controlToken ?? token);

                if (scope.State.OperationStack.TryPop(out var valueToken))
                {
                    if (valueToken is not VariableBotToken returnVar)
                        return (EvalResult.Failed, $"Expected return value to be a variable, but got {valueToken}", valueToken);
                    returnValue = returnVar.VariableValue;
                }

                evalResult = EvalResult.Ok;
            }

            if (evalResult != EvalResult.Failed)
            {
                programState.OperationStack.Push(new VariableBotToken(BotTokenType.Literal, returnValue.StringValue, returnValue));
            }

            return (evalResult, reason, token);
        }
    }
}
