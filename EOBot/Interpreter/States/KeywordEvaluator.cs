using System.Collections.Generic;

namespace EOBot.Interpreter.States
{
    public class KeywordEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public KeywordEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public bool Evaluate(ProgramState input)
        {
            // evaluate if
            // or evaluate while
            // or evaluate goto
            throw new System.NotImplementedException();
        }
    }
}