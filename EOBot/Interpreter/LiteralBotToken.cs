namespace EOBot.Interpreter
{
    public class LiteralBotToken : BotToken
    {
        public object LiteralValue { get; }

        public LiteralBotToken(BotTokenType tokenType, string tokenValue, object literalValue, int line, int col)
            : base(tokenType, tokenValue, line, col)
        {
            LiteralValue = literalValue;
        }

        public override string ToString() => $"{base.ToString()} ({LiteralValue} : {LiteralValue.GetType().Name})";
    }
}
