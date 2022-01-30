using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class GotoEvaluator : IScriptEvaluator
    {
        public bool Evaluate(ProgramState input)
        {
            // ensure we have the right keyword before advancing the program
            var current = input.Current();
            if (current.TokenType != BotTokenType.Keyword || current.TokenValue != "goto")
                return false;

            input.Expect(BotTokenType.Keyword);

            if (!input.Match(BotTokenType.Identifier))
                return false;

            var label = input.OperationStack.Pop().TokenValue;
            return input.Goto(input.Labels[label]);
        }
    }
}
