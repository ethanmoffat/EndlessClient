namespace EOBot.Interpreter
{
    public class IdentifierBotToken : BotToken
    {
        public int? ArrayIndex { get; }

        public IdentifierBotToken(BotTokenType tokenType, string tokenValue, int? arrayIndex = null)
            : base(tokenType, tokenValue)
        {
            ArrayIndex = arrayIndex;
        }
    }
}
