namespace EOBot.Interpreter
{
    public class IdentifierBotToken : BotToken
    {
        public int? ArrayIndex { get; }

        public IdentifierBotToken(BotTokenType tokenType, string tokenValue, int lineNumber, int column, int? arrayIndex = null)
            : base(tokenType, tokenValue, lineNumber, column)
        {
            ArrayIndex = arrayIndex;
        }
    }
}
