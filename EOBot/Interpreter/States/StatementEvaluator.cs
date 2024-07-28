using EOBot.Interpreter.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States;

public class StatementEvaluator : BaseEvaluator
{
    public StatementEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        : base(evaluators) { }

    public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
    {
        while (input.Current().TokenType == BotTokenType.NewLine)
            input.Expect(BotTokenType.NewLine);

        var (result, reason, token) = await Evaluator<AssignmentEvaluator>().EvaluateAsync(input);
        if (result == EvalResult.NotMatch)
        {
            (result, reason, token) = await Evaluator<KeywordEvaluator>().EvaluateAsync(input);
            if (result == EvalResult.NotMatch)
            {
                (result, reason, token) = await Evaluator<LabelEvaluator>().EvaluateAsync(input);
                if (result == EvalResult.NotMatch)
                {
                    (result, reason, token) = await Evaluator<FunctionEvaluator>().EvaluateAsync(input);
                }
            }
        }

        if (result != EvalResult.Ok)
            return (result, reason, token);

        if (!input.Expect(BotTokenType.NewLine) && !input.Expect(BotTokenType.EOF))
            return Error(input.Current(), BotTokenType.NewLine, BotTokenType.EOF);

        return (result, reason, token);
    }
}