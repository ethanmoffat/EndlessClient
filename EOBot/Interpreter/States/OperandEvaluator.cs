using EOBot.Interpreter.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States;

public class OperandEvaluator : BaseEvaluator
{
    public OperandEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        : base(evaluators) { }

    public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
    {
        if (input.Match(BotTokenType.NotOperator))
        {

        }

        var evalRes = await Evaluator<VariableEvaluator>().EvaluateAsync(input);
        if (evalRes.Result == EvalResult.Ok)
            return evalRes;

        var matchRes = input.Match(BotTokenType.Literal);
        return matchRes ? Success() : (EvalResult.NotMatch, string.Empty, input.Current());
    }
}