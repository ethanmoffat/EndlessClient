using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class StatementListEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public StatementListEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            return _evaluators.OfType<StatementEvaluator>().Single().Evaluate(input)
                && (input.Expect(BotTokenType.EOF) || this.Evaluate(input));
        }
    }
}