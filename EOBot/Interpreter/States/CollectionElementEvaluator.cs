using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class CollectionElementEvaluator : BaseEvaluator
    {
        public CollectionElementEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            var eval = await EvaluateElementAsync(input, ct);
            if (eval.Result != EvalResult.Ok || !input.Match(BotTokenType.Colon))
                return eval;

            return await EvaluateElementAsync(input, ct);
        }

        private async Task<(EvalResult Result, string, BotToken)> EvaluateElementAsync(ProgramState input, CancellationToken ct)
        {
            var elementToken = input.Current();

            var eval = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
            if (eval.Result == EvalResult.Ok && input.OperationStack.Peek() is VariableBotToken value)
            {
                input.OperationStack.Pop();
                input.OperationStack.Push(value.WithPosition(elementToken));
            }

            return eval;
        }
    }
}
