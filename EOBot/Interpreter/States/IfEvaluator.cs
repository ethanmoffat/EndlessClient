using EOBot.Interpreter.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States;

public class IfEvaluator : BlockEvaluator
{
    public IfEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        : base(evaluators) { }

    public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
    {
        // ensure we have the right keyword before advancing the program
        var current = input.Current();
        if (current.TokenType != BotTokenType.Keyword || current.TokenValue != "if")
            return (EvalResult.NotMatch, string.Empty, input.Current());

        var ifStartIndex = input.ExecutionIndex;

        var (result, reason, token) = await EvaluateConditionAsync(ifStartIndex, input);
        if (result == EvalResult.Ok)
        {
            if (bool.TryParse(token.TokenValue, out var conditionValue) && conditionValue)
            {
                var ifRes = await EvaluateBlockAsync(input);
                if (ifRes.Item1 == EvalResult.Ok)
                    SkipElseBlocks(input);

                return ifRes;
            }

            SkipBlock(input);

            while (input.Expect(BotTokenType.NewLine)) ;

            if (IsElse(input))
            {
                input.Expect(BotTokenType.Keyword);

                var elseIfRes = await Evaluator<IfEvaluator>().EvaluateAsync(input);
                if (elseIfRes.Result == EvalResult.Failed)
                    return elseIfRes;
                else if (elseIfRes.Result == EvalResult.Ok)
                {
                    SkipElseBlocks(input);
                    return elseIfRes;
                }

                // if not a match for else if, it is an else block
                return await EvaluateBlockAsync(input);
            }
        }

        RestoreLastNewline(input);
        return (result, reason, token);
    }

    private bool IsElse(ProgramState input)
    {
        var current = input.Current();
        return current.TokenType == BotTokenType.Keyword && current.TokenValue == "else";
    }

    private void SkipElseBlocks(ProgramState input)
    {
        while (input.Expect(BotTokenType.NewLine)) ;

        // skip the rest of the following blocks if evaluated
        while (IsElse(input))
        {
            input.Expect(BotTokenType.Keyword);
            SkipBlock(input);
            while (input.Expect(BotTokenType.NewLine)) ;
        }

        RestoreLastNewline(input);
    }
}