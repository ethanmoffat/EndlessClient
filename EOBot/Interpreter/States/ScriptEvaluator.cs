using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class ScriptEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public ScriptEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public async Task<bool> EvaluateAsync(ProgramState input)
        {
            return await _evaluators
                .OfType<StatementListEvaluator>()
                .Single()
                .EvaluateAsync(input)
                && input.Expect(BotTokenType.EOF);
        }
    }
}
