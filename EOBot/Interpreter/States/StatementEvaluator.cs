using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class StatementEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public StatementEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            return Evaluate<AssignmentEvaluator>(input)
                || Evaluate<KeywordEvaluator>(input)
                || Evaluate<LabelEvaluator>(input)
                || Evaluate<FunctionEvaluator>(input);
        }

        private bool Evaluate<T>(ProgramState input)
            where T : IScriptEvaluator
        {
            return _evaluators
                .OfType<T>()
                .Single()
                .Evaluate(input);
        }
    }
}