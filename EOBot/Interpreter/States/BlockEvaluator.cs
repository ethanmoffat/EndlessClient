using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public abstract class BlockEvaluator : IScriptEvaluator
    {
        protected readonly IEnumerable<IScriptEvaluator> _evaluators;

        protected BlockEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public abstract Task<bool> EvaluateAsync(ProgramState input);

        protected async Task<(bool, VariableBotToken)> EvaluateConditionAsync(int blockStartIndex, ProgramState input)
        {
            input.Goto(blockStartIndex);

            if (!input.Expect(BotTokenType.Keyword) ||
                !input.Expect(BotTokenType.LParen) ||
                !await _evaluators.OfType<ExpressionEvaluator>().Single().EvaluateAsync(input) ||
                !input.Expect(BotTokenType.RParen))
                return (false, new VariableBotToken(BotTokenType.Error, string.Empty, UndefinedVariable.Instance));

            if (input.OperationStack.Count == 0)
                return (false, new VariableBotToken(BotTokenType.Error, string.Empty, UndefinedVariable.Instance));

            return (true, (VariableBotToken)input.OperationStack.Pop());
        }

        protected async Task<bool> EvaluateBlockAsync(ProgramState input)
        {
            input.Expect(BotTokenType.NewLine);

            // either: multi-line statement / evaluate statement list (consumes RBrace as end condition)
            // or:     single statement
            // evaluated in separate blocks because we want to check statement list OR statement, not both
            if (input.Expect(BotTokenType.LBrace))
            {
                if (!await _evaluators.OfType<StatementListEvaluator>().Single().EvaluateAsync(input))
                    return false;
            }
            else if (!await _evaluators.OfType<StatementEvaluator>().Single().EvaluateAsync(input))
            {
                return false;
            }

            // hack: put the \n token back since StatementList/Statement will have consumed it
            if (input.Program[input.ExecutionIndex - 1].TokenType == BotTokenType.NewLine)
                input.Goto(input.ExecutionIndex - 1);

            return true;
        }

        protected void SkipBlock(ProgramState input)
        {
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
                // optional newline after block
                input.Expect(BotTokenType.NewLine);

                while (input.Current().TokenType != BotTokenType.NewLine && input.Current().TokenType != BotTokenType.EOF)
                    input.SkipToken();
            }
        }
    }
}
