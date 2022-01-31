using EOBot.Interpreter.Extensions;
using System.Collections.Generic;

namespace EOBot.Interpreter.States
{
    public class IfEvaluator : BlockEvaluator
    {
        public IfEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override bool Evaluate(ProgramState input)
        {
            // ensure we have the right keyword before advancing the program
            var current = input.Current();
            if (current.TokenType != BotTokenType.Keyword || current.TokenValue != "if")
                return false;

            var ifStartIndex = input.ExecutionIndex;

            var (ok, condition) = EvaluateCondition(ifStartIndex, input);
            if (ok && bool.TryParse(condition.TokenValue, out var conditionValue) && conditionValue)
            {
                if (!EvaluateBlock(input))
                    return false;

                return true;
            }

            if (ok)
            {
                SkipBlock(input);
            }

            return true;
        }
    }
}
