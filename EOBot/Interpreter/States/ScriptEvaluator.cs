using System.Collections.Generic;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class ScriptEvaluator : BaseEvaluator
    {
        public ScriptEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            var evalResult = await Evaluator<StatementListEvaluator>().EvaluateAsync(input);
            if (evalResult.Result != EvalResult.Ok)
                return evalResult;

            if (!input.Expect(BotTokenType.EOF))
                return Error(input.Current(), BotTokenType.EOF);

            return Success();
        }
    }
}