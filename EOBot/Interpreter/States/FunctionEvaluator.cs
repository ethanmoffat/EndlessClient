using System.Collections.Generic;

namespace EOBot.Interpreter.States
{
    public class FunctionEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public FunctionEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            throw new System.NotImplementedException();
        }
    }
}