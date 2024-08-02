using System.Collections.Generic;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class WhileEvaluator : BlockEvaluator
    {
        public WhileEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            // ensure we have the right keyword before advancing the program
            var current = input.Current();
            if (current.TokenType != BotTokenType.Keyword || current.TokenValue != "while")
                return (EvalResult.NotMatch, string.Empty, input.Current());

            var whileLoopStartIndex = input.ExecutionIndex;

            EvalResult result;
            string reason;
            BotToken token;
            for ((result, reason, token) = await EvaluateConditionAsync(whileLoopStartIndex, input);
                 result == EvalResult.Ok && bool.TryParse(token.TokenValue, out var conditionValue) && conditionValue;
                 (result, reason, token) = await EvaluateConditionAsync(whileLoopStartIndex, input))
            {
                var blockEval = await EvaluateBlockAsync(input);
                if (blockEval.Item1 != EvalResult.Ok)
                    return blockEval;
            }

            if (result == EvalResult.Ok)
            {
                SkipBlock(input);
            }

            return (result, reason, token);
        }
    }
}