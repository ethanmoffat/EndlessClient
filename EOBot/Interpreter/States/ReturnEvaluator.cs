using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class ReturnEvaluator : BaseEvaluator
    {
        public ReturnEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult Result, string Reason, BotToken Token)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (!input.Current().Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_RETURN))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            if (input.CallStack.Count == 0)
                return (EvalResult.Failed, "Call stack is empty. return can only be used within the context of a user-defined function.", input.Current());

            input.Expect(BotTokenType.Keyword);

            var result = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
            if (result.Result != EvalResult.Ok)
                return result;

            // check for value-returning function
            if (input.OperationStack.Count > 0)
            {
                var resultToken = input.OperationStack.Pop();
                if (resultToken is not VariableBotToken resultVar)
                    return StackTokenError(BotTokenType.Variable, resultToken);
                input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, resultVar.VariableValue);
            }

            return result;
        }
    }
}
