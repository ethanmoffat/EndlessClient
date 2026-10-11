using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class StatementListEvaluator : BaseEvaluator
    {
        public StatementListEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ExpectEndOfList(input))
                return Success();

            (EvalResult Result, string, BotToken) result;
            do
            {
                result = await Evaluator<StatementEvaluator>().EvaluateAsync(input, ct);
            }
            while (result.Result == EvalResult.Ok && !ExpectEndOfList(input));

            return result;
        }

        private static bool ExpectEndOfList(ProgramState input)
        {
            while (input.Expect(BotTokenType.NewLine)) ;
            return input.Expect(BotTokenType.EOF) || input.Expect(BotTokenType.RBrace);
        }
    }
}
