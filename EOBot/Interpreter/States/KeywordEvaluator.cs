using System.Collections.Generic;
using System.Linq;

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
            return Evaluate<IfEvaluator>(input)
                    || Evaluate<WhileEvaluator>(input)
                    || Evaluate<GotoEvaluator>(input);
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