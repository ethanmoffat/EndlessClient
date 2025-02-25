using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class ForEvaluator : BlockEvaluator
    {
        public ForEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            if (!input.Current().Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_FOR))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            var assignmentEval = await EvaluateAssignmentAsync(input, ct);
            if (assignmentEval.Result != EvalResult.Ok)
                return assignmentEval;

            var forLoopConditionIndex = input.ExecutionIndex;
            var (result, reason, token) = await EvaluateConditionAsync(forLoopConditionIndex, input, ct);

            var iterateOperationIndex = input.ExecutionIndex;

            while (input.Current().TokenType != BotTokenType.RParen)
            {
                if (input.MatchOneOf(BotTokenType.NewLine, BotTokenType.EOF, BotTokenType.Semicolon))
                    return Error(input.Current(), BotTokenType.RParen);

                input.SkipToken();
            }
            input.Expect(BotTokenType.RParen);
            var blockIndex = input.ExecutionIndex;

            while (result == EvalResult.Ok && bool.TryParse(token.TokenValue, out var conditionValue) && conditionValue)
            {
                input.Goto(blockIndex);
                var blockEval = await EvaluateBlockAsync(input, ct);
                if (blockEval.Item1 != EvalResult.Ok)
                    return blockEval;

                (result, reason, token) = await EvaluateIteration(iterateOperationIndex, input, ct);
                if (result != EvalResult.Ok)
                    return (result, reason, token);

                (result, reason, token) = await EvaluateConditionAsync(forLoopConditionIndex, input, ct);
            }

            if (result == EvalResult.Ok)
            {
                input.Goto(blockIndex);
                SkipBlock(input);

                RestoreLastNewline(input);
            }

            return (result, reason, token);
        }

        private async Task<(EvalResult Result, string, BotToken)> EvaluateAssignmentAsync(ProgramState input, CancellationToken ct)
        {
            if (!input.ExpectPair(BotTokenType.Keyword, BotTokenType.LParen))
                return (EvalResult.Failed, "Missing keyword and identifier to start for loop", input.Current());

            var evalResult = await Evaluator<AssignmentEvaluator>().EvaluateAsync(input, ct);
            if (evalResult.Result != EvalResult.Ok)
                return evalResult;

            if (!input.Expect(BotTokenType.Semicolon))
                return Error(input.Current(), BotTokenType.Semicolon);

            return evalResult;
        }

        protected override async Task<(EvalResult, string, BotToken)> EvaluateConditionAsync(int blockStartIndex, ProgramState input, CancellationToken ct)
        {
            input.Goto(blockStartIndex);

            var evalResult = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
            if (evalResult.Result != EvalResult.Ok)
                return evalResult;

            if (!input.Expect(BotTokenType.Semicolon))
                return Error(input.Current(), BotTokenType.Semicolon);

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());

            return Success(input.OperationStack.Pop());
        }

        private async Task<(EvalResult, string, BotToken)> EvaluateIteration(int iterateOperationIndex, ProgramState input, CancellationToken ct)
        {
            input.Goto(iterateOperationIndex);

            var evalResult = await Evaluator<AssignmentEvaluator>().EvaluateAsync(input, ct);
            if (evalResult.Result != EvalResult.Ok)
                return evalResult;


            if (!input.Expect(BotTokenType.RParen))
                return Error(input.Current(), BotTokenType.RParen);

            return evalResult;
        }
    }
}
