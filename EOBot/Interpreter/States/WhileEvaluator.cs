using EOBot.Interpreter.Extensions;
using System.Collections.Generic;

namespace EOBot.Interpreter.States
{
    public class WhileEvaluator : BlockEvaluator
    {
        public WhileEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override bool Evaluate(ProgramState input)
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
                if (!EvaluateBlock(input))
                    return false;
            }

            if (ok)
            {
                SkipBlock(input);
            }

            return ok;
        }
    }
}