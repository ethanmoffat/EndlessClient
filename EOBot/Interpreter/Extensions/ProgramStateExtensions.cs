using EOBot.Interpreter.States;

namespace EOBot.Interpreter.Extensions
{
    public static class ProgramStateExtensions
    {
        public static bool MatchOneOf(this ProgramState input, params BotTokenType[] tokenTypes)
        {
            // todo: there's probably LINQ for this but I'm tired and my brain isn't quite working
            // needs to stop trying to match after first match, can't use FirstOrDefault unless default(BotTokenType) can be used as an "empty" value
            foreach (var type in tokenTypes)
            {
                if (input.Match(type))
                    return true;
            }

            return false;
        }
    }
}
