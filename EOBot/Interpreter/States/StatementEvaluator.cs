using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class StatementEvaluator : BaseEvaluator
    {
        public StatementEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            var hasNewLines = false;
            while (input.Current().TokenType == BotTokenType.NewLine)
            {
                hasNewLines = true;
                input.Expect(BotTokenType.NewLine);
            }

            if (hasNewLines && input.Expect(BotTokenType.EOF))
                return Success();

            var (result, reason, token) = await Evaluator<AssignmentEvaluator>().EvaluateAsync(input, ct);
            if (result == EvalResult.NotMatch)
            {
                (result, reason, token) = await Evaluator<KeywordEvaluator>().EvaluateAsync(input, ct);
                if (result == EvalResult.NotMatch)
                {
                    (result, reason, token) = await Evaluator<LabelEvaluator>().EvaluateAsync(input, ct);
                    if (result == EvalResult.NotMatch)
                    {
                        (result, reason, token) = await Evaluator<FunctionEvaluator>().EvaluateAsync(input, ct);
                    }
                }
            }

            if (result != EvalResult.Ok)
                return (result, reason, token);

            // a closing brace ends a one-line block and is consumed by the enclosing statement list
            if (!input.Expect(BotTokenType.NewLine) && !input.Expect(BotTokenType.EOF) && !input.Current().Is(BotTokenType.RBrace))
                return Error(input.Current(), BotTokenType.NewLine, BotTokenType.EOF, BotTokenType.RBrace);

            return (result, reason, token);
        }
    }
}
