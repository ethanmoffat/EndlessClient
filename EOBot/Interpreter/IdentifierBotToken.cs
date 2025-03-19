using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter
{
    public class IdentifierBotToken : BotToken
    {
        public IVariable Indexer { get; }

        public IdentifierBotToken Member { get; }

        public IdentifierBotToken(BotToken identifier, IVariable indexer = null, IdentifierBotToken member = null)
            : base(identifier.TokenType, identifier.TokenValue, identifier.LineNumber, identifier.Column)
        {
            Indexer = indexer;
            Member = member;
        }
    }
}
