namespace EOBot.Interpreter.States
{
    public class LabelEvaluator : IScriptEvaluator
    {
        public bool Evaluate(ProgramState input)
        {
            return input.ExpectPair(BotTokenType.Identifier, BotTokenType.Colon);
        }
    }
}
