namespace EOBot.Interpreter
{
    public class BotToken
    {
        public BotTokenType TokenType { get; }
        public string TokenValue { get; }

        public BotToken(BotTokenType tokenType, string tokenValue)
        {
            TokenType = tokenType;
            TokenValue = tokenValue;
        }

        public override string ToString() => $"{TokenType}: {TokenValue}";
    }
}
