using EOBot.Interpreter.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class IfEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public IfEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            // ensure we have the right keyword before advancing the program
            var current = input.Current();
            if (current.TokenType != BotTokenType.Keyword || current.TokenValue != "if")
                return false;

            if (!input.Expect(BotTokenType.Keyword) ||
                !input.Expect(BotTokenType.LParen) ||
                !_evaluators.OfType<ExpressionEvaluator>().Single().Evaluate(input) ||
                !input.Expect(BotTokenType.RParen))
                return false;

            if (input.OperationStack.Count == 0)
                return false;
            var condition = (VariableBotToken)input.OperationStack.Pop();

            if (bool.TryParse(condition.TokenValue, out var conditionValue) && conditionValue)
            {
                // either: multi-line statement / evaluate statement list (consumes RBrace as end condition)
                // or:     single statement
                if ((input.Expect(BotTokenType.LBrace) && !_evaluators.OfType<StatementListEvaluator>().Single().Evaluate(input)) ||
                    !_evaluators.OfType<StatementEvaluator>().Single().Evaluate(input))
                    return false;

                // hack: put the \n token back since StatementList/Statement will have consumed it
                if (input.Program[input.ExecutionIndex - 1].TokenType == BotTokenType.NewLine)
                    input.Goto(input.ExecutionIndex - 1);
            }
            else if (input.Expect(BotTokenType.LBrace))
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

                while(input.Current().TokenType != BotTokenType.NewLine && input.Current().TokenType != BotTokenType.EOF)
                    input.SkipToken();
            }

            return true;
        }
    }
}
