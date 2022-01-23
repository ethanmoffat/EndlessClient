namespace EOBot.Interpreter.States
{
    public class LabelEvaluator : IScriptEvaluator
    {
        public bool Evaluate(ProgramState input)
        {
            return input.Expect(BotTokenType.Label)
                && input.Expect(BotTokenType.Colon);
        }
    }
}