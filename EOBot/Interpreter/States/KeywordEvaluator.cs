using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class KeywordEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public KeywordEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public async Task<bool> EvaluateAsync(ProgramState input)
        {
            return await Evaluate<IfEvaluator>(input)
                    || await Evaluate<WhileEvaluator>(input)
                    || await Evaluate<GotoEvaluator>(input);
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