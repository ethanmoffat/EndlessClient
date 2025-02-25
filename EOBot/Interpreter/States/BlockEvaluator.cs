using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public abstract class BlockEvaluator : BaseEvaluator
    {
        protected BlockEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        protected virtual async Task<(EvalResult, string, BotToken)> EvaluateConditionAsync(int blockStartIndex, ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            input.Goto(blockStartIndex);

            if (!input.ExpectPair(BotTokenType.Keyword, BotTokenType.LParen))
                return (EvalResult.Failed, "Missing keyword and lparen to start condition evaluation", input.Current());

            var evalResult = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input, ct);
            if (evalResult.Result != EvalResult.Ok)
                return evalResult;

            if (!input.Expect(BotTokenType.RParen))
                return Error(input.Current(), BotTokenType.RParen);

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());

            return Success(input.OperationStack.Pop());
        }

        protected async Task<(EvalResult, string, BotToken)> EvaluateBlockAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            input.Expect(BotTokenType.NewLine);

            (EvalResult Result, string, BotToken) evalResult;

            // either: multi-line statement / evaluate statement list (consumes RBrace as end condition)
            // or:     single statement
            // evaluated in separate blocks because we want to check statement list OR statement, not both
            if (input.Expect(BotTokenType.LBrace))
            {
                evalResult = await Evaluator<StatementListEvaluator>().EvaluateAsync(input, ct);
                if (evalResult.Result != EvalResult.Ok)
                    return evalResult;
            }
            else
            {
                evalResult = await Evaluator<StatementEvaluator>().EvaluateAsync(input, ct);
                if (evalResult.Result != EvalResult.Ok)
                    return evalResult;
            }


            RestoreLastNewline(input);
            return evalResult;
        }

        protected void SkipBlock(ProgramState input)
        {
            // ensure that for 'else if' the if condition is skipped as well
            if (input.Current().Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_IF))
            {
                input.Expect(BotTokenType.Keyword);
                input.Expect(BotTokenType.LParen);
                while (!input.Expect(BotTokenType.RParen))
                    input.SkipToken();
            }

            // potential newline character - skip so we can advance execution beyond the block
            input.Expect(BotTokenType.NewLine);

            // skip the rest of the block
            if (input.Expect(BotTokenType.LBrace))
            {
                int rBraceCount = 1;
                while (rBraceCount > 0 && input.Current().TokenType != BotTokenType.EOF)
                {
                    if (input.Current().TokenType == BotTokenType.LBrace)
                        rBraceCount++;
                    else if (input.Current().TokenType == BotTokenType.RBrace)
                        rBraceCount--;

                    input.SkipToken();
                }
            }
            else
            {
                // optional newline before statement
                input.Expect(BotTokenType.NewLine);

                while (input.Current().TokenType != BotTokenType.NewLine && input.Current().TokenType != BotTokenType.EOF)
                    input.SkipToken();

                // optional newline after statement
                input.Expect(BotTokenType.NewLine);
            }
        }

        protected static void RestoreLastNewline(ProgramState input)
        {
            // hack: put the \n token back since StatementList/Statement will have consumed it
            if (input.Program[input.ExecutionIndex - 1].TokenType == BotTokenType.NewLine)
                input.Goto(input.ExecutionIndex - 1);
        }
    }
}
