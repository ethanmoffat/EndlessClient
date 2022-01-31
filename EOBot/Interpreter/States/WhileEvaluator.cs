using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class WhileEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public WhileEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            // ensure we have the right keyword before advancing the program
            var current = input.Current();
            if (current.TokenType != BotTokenType.Keyword || current.TokenValue != "while")
                return false;

            var whileLoopStartIndex = input.ExecutionIndex;

            bool ok;
            VariableBotToken condition;
            for ((ok, condition) = EvaluateCondition(whileLoopStartIndex, input);
                 ok && bool.TryParse(condition.TokenValue, out var conditionValue) && conditionValue;
                 (ok, condition) = EvaluateCondition(whileLoopStartIndex, input))
            {
                // todo: this is the same evaluation code as if statement, see if there's a way to reuse it

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
            }

            // potential newline character - skip so we can advance execution beyond the while loop
            input.Expect(BotTokenType.NewLine);

            if (ok)
            {
                // skip the rest of the while block (same as if statement)
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

            return ok;
        }

        private (bool, VariableBotToken) EvaluateCondition(int whileLoopStartIndex, ProgramState input)
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
    }
}