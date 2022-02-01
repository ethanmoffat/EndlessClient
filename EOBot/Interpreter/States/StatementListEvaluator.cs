using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class StatementListEvaluator : IScriptEvaluator
    {
        private readonly IEnumerable<IScriptEvaluator> _evaluators;

        public StatementListEvaluator(IEnumerable<IScriptEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public async Task<bool> EvaluateAsync(ProgramState input)
        {
            // todo: iterative approach so call stack doesn't get huge
            return await _evaluators.OfType<StatementEvaluator>().Single().EvaluateAsync(input)
                && (input.Expect(BotTokenType.EOF) || input.Expect(BotTokenType.RBrace) || await this.EvaluateAsync(input));
        }
    }
}