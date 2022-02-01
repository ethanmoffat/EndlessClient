using EOBot.Interpreter.Extensions;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class GotoEvaluator : IScriptEvaluator
    {
        public Task<bool> EvaluateAsync(ProgramState input)
        {
            // ensure we have the right keyword before advancing the program
            var current = input.Current();
            if (current.TokenType != BotTokenType.Keyword || current.TokenValue != "goto")
                return Task.FromResult(false);

            input.Expect(BotTokenType.Keyword);

            if (!input.Match(BotTokenType.Identifier))
                return Task.FromResult(false);

            var label = input.OperationStack.Pop().TokenValue;
            return Task.FromResult(input.Goto(input.Labels[label]));
        }
    }
}
