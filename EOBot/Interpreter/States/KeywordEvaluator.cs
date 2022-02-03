using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class KeywordEvaluator : BaseEvaluator
    {
        public KeywordEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            var res = await Evaluator<IfEvaluator>().EvaluateAsync(input);
            if (res.Result == EvalResult.Ok)
                return res;

            res = await Evaluator<WhileEvaluator>().EvaluateAsync(input);
            if (res.Result == EvalResult.Ok)
                return res;

            return await Evaluator<GotoEvaluator>().EvaluateAsync(input);
        }
    }
}