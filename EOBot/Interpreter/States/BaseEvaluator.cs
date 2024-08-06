using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public abstract class BaseEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        protected BaseEvaluator() { }

        protected BaseEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public abstract Task<(EvalResult Result, string Reason, BotToken Token)> EvaluateAsync(ProgramState input);

        protected IScriptEvaluator Evaluator<T>() where T : IScriptEvaluator
        {
            return _evaluators.OfType<T>().Single();
        }

        protected (EvalResult, string, BotToken) Success(BotToken token = null)
        {
            return (EvalResult.Ok, string.Empty, token);
        }

        protected (EvalResult, string, BotToken) Error(BotToken current, params BotTokenType[] expectedTypes)
        {
            var extra = string.IsNullOrWhiteSpace(current.TokenValue)
                ? string.Empty
                : $" ({current.TokenValue})";

            if (expectedTypes.Length == 0)
                return (EvalResult.Failed, string.Empty, current);
            else if (expectedTypes.Length == 1)
                return (EvalResult.Failed, $"Expected {expectedTypes[0]}, got {current.TokenType}{extra}", current);
            else
                return (EvalResult.Failed, $"Expected one of [{string.Join(", ", expectedTypes)}], got {current.TokenType}{extra}", current);
        }

        protected (EvalResult, string, BotToken) UnexpectedTokenError(BotToken current, params BotTokenType[] unexpectedTypes)
        {
            if (unexpectedTypes.Length == 0)
                return (EvalResult.Failed, string.Empty, current);
            else if (unexpectedTypes.Length == 1)
                return (EvalResult.Failed, $"{unexpectedTypes[0]} was unexpected", current);
            else
                return (EvalResult.Failed, $"One of {string.Join(",", unexpectedTypes)} was unexpected", current);
        }

        protected (EvalResult, string, BotToken) StackEmptyError(BotToken current)
        {
            return (EvalResult.Failed, $"Expected operation stack to have operands, but it was empty", current);
        }

        protected (EvalResult, string, BotToken) UnsupportedOperatorError(BotToken actualToken)
        {
            return (EvalResult.Failed, $"Unsupported operator evaluating expression: {actualToken.TokenType}", actualToken);
        }

        protected (EvalResult, string, BotToken) StackTokenError(BotTokenType expectedTokenType, BotToken actualToken)
        {
            return (EvalResult.Failed, $"Expected operation stack to have {expectedTokenType} token, got {actualToken.TokenType}", actualToken);
        }

        protected (EvalResult, string, BotToken) ReadOnlyVariableError(IdentifierBotToken token)
        {
            return (EvalResult.Failed, $"Attempted to assign value to read-only variable {token.TokenValue}", token);
        }

        protected (EvalResult, string, BotToken) IdentifierNotFoundError(IdentifierBotToken token)
        {
            return (EvalResult.Failed, $"Identifier {token.TokenValue} is not defined", token);
        }

        protected (EvalResult, string, BotToken) GotoError(BotToken token)
        {
            return (EvalResult.Failed, $"Failed to goto label {token.TokenValue}", token);
        }
    }
}
