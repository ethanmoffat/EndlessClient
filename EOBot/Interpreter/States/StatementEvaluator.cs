using EOBot.Interpreter.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class StatementEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public StatementEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public async Task<bool> EvaluateAsync(ProgramState input)
        {
            while (input.Current().TokenType == BotTokenType.NewLine)
                input.Expect(BotTokenType.NewLine);

            return (await Evaluate<AssignmentEvaluator>(input)
                    || await Evaluate<KeywordEvaluator>(input)
                    || await Evaluate<LabelEvaluator>(input)
                    || await Evaluate<FunctionEvaluator>(input))
                    && (input.Expect(BotTokenType.NewLine) || input.Expect(BotTokenType.EOF));
        }

        private Task<bool> Evaluate<T>(ProgramState input)
            where T : IScriptEvaluator
        {
            return _evaluators
                .OfType<T>()
                .Single()
                .EvaluateAsync(input);
        }
    }
}