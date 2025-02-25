namespace EOBot.Interpreter.Extensions
{
    public static class BotTokenExtensions
    {
        public static bool Is(this BotToken token, BotTokenType expectedType, string expectedValue = null)
        {
            return expectedValue == null
                ? token.TokenType == expectedType
                : token.TokenType == expectedType && token.TokenValue == expectedValue;
        }
    }
}
