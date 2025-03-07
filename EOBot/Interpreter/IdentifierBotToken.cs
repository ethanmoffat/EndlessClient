namespace EOBot.Interpreter
{
    public class IdentifierBotToken : BotToken
    {
        public int? ArrayIndex { get; }

        public string DictKey { get; }

        public IdentifierBotToken Member { get; }

        public IdentifierBotToken(BotToken identifier, int? arrayIndex = null, string dictKey = null, IdentifierBotToken member = null)
            : base(identifier.TokenType, identifier.TokenValue, identifier.LineNumber, identifier.Column)
        {
            ArrayIndex = arrayIndex;
            DictKey = dictKey;
            Member = member;
        }
    }
}
