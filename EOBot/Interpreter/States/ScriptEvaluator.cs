using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class ScriptEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public ScriptEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            return _evaluators
                .OfType<StatementListEvaluator>()
                .Single()
                .Evaluate(input)
                && input.Expect(BotTokenType.EOF);
        }
    }
}
