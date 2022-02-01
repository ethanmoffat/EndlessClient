using EOBot.Interpreter.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class IfEvaluator : BlockEvaluator
    {
        public IfEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<bool> EvaluateAsync(ProgramState input)
        {
            // ensure we have the right keyword before advancing the program
            var current = input.Current();
            if (current.TokenType != BotTokenType.Keyword || current.TokenValue != "if")
                return false;

            var ifStartIndex = input.ExecutionIndex;

            var (ok, condition) = await EvaluateConditionAsync(ifStartIndex, input);
            if (ok)
            {
                if (bool.TryParse(condition.TokenValue, out var conditionValue) && conditionValue)
                    return await EvaluateBlockAsync(input);

                SkipBlock(input);
            }

            return ok;
        }
    }
}
