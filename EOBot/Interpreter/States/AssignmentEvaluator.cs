using System.Collections.Generic;

namespace EOBot.Interpreter.States
{
    public class AssignmentEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public AssignmentEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            // match variable
            // match =
            // match expr
            throw new System.NotImplementedException();
        }
    }
}