using EOBot.Interpreter.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class IfEvaluator : BlockEvaluator
    {
        public IfEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            // ensure we have the right keyword before advancing the program
            var current = input.Current();
            if (current.TokenType != BotTokenType.Keyword || current.TokenValue != "if")
                return Error(input.Current(), BotTokenType.Keyword);

            var ifStartIndex = input.ExecutionIndex;

            var (result, reason, token) = await EvaluateConditionAsync(ifStartIndex, input);
            if (result == EvalResult.Ok)
            {
                if (bool.TryParse(token.TokenValue, out var conditionValue) && conditionValue)
                    return await EvaluateBlockAsync(input);

                SkipBlock(input);
            }

            return (result, reason, token);
        }
    }
}
