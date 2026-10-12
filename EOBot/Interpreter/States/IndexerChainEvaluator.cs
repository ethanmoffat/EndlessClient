using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    /// <summary>
    /// Evaluates consecutive index expressions ([a][b]...) and pushes their values onto the stack as an <see cref="IndexersBotToken"/>.
    /// </summary>
    public class IndexerChainEvaluator : BaseEvaluator
    {
        public IndexerChainEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            var firstLBracket = input.Current();
            var indexers = new List<IVariable>();
            while (input.Match(BotTokenType.LBracket))
            {
                var expressionEval = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
                if (expressionEval.Result != EvalResult.Ok)
                    return expressionEval;

                if (!input.Expect(BotTokenType.RBracket))
                    return Error(input.Current(), BotTokenType.RBracket);

                if (input.OperationStack.Count == 0)
                    return StackEmptyError(input.Current());
                var expressionResult = (VariableBotToken)input.OperationStack.Pop();

                if (input.OperationStack.Count == 0)
                    return StackEmptyError(input.Current());
                var lBracket = input.OperationStack.Pop();
                if (!lBracket.Is(BotTokenType.LBracket))
                    return StackTokenError(BotTokenType.LBracket, lBracket);

                indexers.Add(expressionResult.VariableValue);
            }

            if (indexers.Count == 0)
                return (EvalResult.NotMatch, string.Empty, input.Current());

            input.OperationStack.Push(new IndexersBotToken(firstLBracket, indexers));
            return Success();
        }
    }
}
