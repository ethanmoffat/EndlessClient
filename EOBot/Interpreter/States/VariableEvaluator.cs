using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class VariableEvaluator : BaseEvaluator
    {
        public VariableEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            if (!input.Match(BotTokenType.Variable))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            IVariable indexer = null;
            if (input.Match(BotTokenType.LBracket))
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

                indexer = expressionResult.VariableValue;
            }

            IdentifierBotToken nestedIdentifier = null;
            if (input.Match(BotTokenType.Dot))
            {
                var evalRes = await Evaluator<VariableEvaluator>().EvaluateAsync(input, ct);
                if (evalRes.Result != EvalResult.Ok)
                    return evalRes;

                if (input.OperationStack.Count == 0)
                    return StackEmptyError(input.Current());
                nestedIdentifier = (IdentifierBotToken)input.OperationStack.Pop();
                if (nestedIdentifier.TokenType != BotTokenType.Variable)
                    return StackTokenError(BotTokenType.Variable, nestedIdentifier);

                if (input.OperationStack.Count == 0)
                    return StackEmptyError(input.Current());
                var expectedDot = input.OperationStack.Pop();
                if (expectedDot.TokenType != BotTokenType.Dot)
                    return StackTokenError(BotTokenType.Dot, expectedDot);
            }

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());
            var identifier = input.OperationStack.Pop();

            input.OperationStack.Push(new IdentifierBotToken(identifier, indexer, nestedIdentifier));

            return Success();
        }
    }
}
