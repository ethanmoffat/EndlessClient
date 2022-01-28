using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class OperandEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public OperandEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            return _evaluators.OfType<VariableEvaluator>().Single().Evaluate(input) ||
                input.Match(BotTokenType.Literal);
        }
    }
}
