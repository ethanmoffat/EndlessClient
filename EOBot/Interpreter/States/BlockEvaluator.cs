using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public abstract class BlockEvaluator : IScriptEvaluator
    {
        protected readonly IEnumerable<IScriptEvaluator> _evaluators;

        protected BlockEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public abstract bool Evaluate(ProgramState input);

        protected (bool, VariableBotToken) EvaluateCondition(int whileLoopStartIndex, ProgramState input)
        {
            input.Goto(whileLoopStartIndex);

            if (!input.Expect(BotTokenType.Keyword) ||
                !input.Expect(BotTokenType.LParen) ||
                !_evaluators.OfType<ExpressionEvaluator>().Single().Evaluate(input) ||
                !input.Expect(BotTokenType.RParen))
                return (false, new VariableBotToken(BotTokenType.Error, string.Empty, UndefinedVariable.Instance));

            if (input.OperationStack.Count == 0)
                return (false, new VariableBotToken(BotTokenType.Error, string.Empty, UndefinedVariable.Instance));

            return (true, (VariableBotToken)input.OperationStack.Pop());
        }

        protected bool EvaluateBlock(ProgramState input)
        {
            input.Expect(BotTokenType.NewLine);

            // either: multi-line statement / evaluate statement list (consumes RBrace as end condition)
            // or:     single statement
            // evaluated in separate blocks because we want to check statement list OR statement, not both
            if (input.Expect(BotTokenType.LBrace))
            {
                if (!_evaluators.OfType<StatementListEvaluator>().Single().Evaluate(input))
                    return false;
            }
            else if (!_evaluators.OfType<StatementEvaluator>().Single().Evaluate(input))
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

            // skip the rest of the block (same as if statement)
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
                // optional newline after if
                input.Expect(BotTokenType.NewLine);

                while (input.Current().TokenType != BotTokenType.NewLine && input.Current().TokenType != BotTokenType.EOF)
                    input.SkipToken();
            }
        }
    }
}
