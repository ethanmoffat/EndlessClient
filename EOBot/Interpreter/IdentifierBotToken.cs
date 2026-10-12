using System.Collections.Generic;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter
{
    public class IdentifierBotToken : BotToken
    {
        public IReadOnlyList<IVariable> Indexers { get; }

        public IdentifierBotToken Member { get; }

        public IdentifierBotToken(BotToken identifier, IReadOnlyList<IVariable> indexers = null, IdentifierBotToken member = null)
            : base(identifier.TokenType, identifier.TokenValue, identifier.LineNumber, identifier.Column)
        {
            Indexers = indexers ?? [];
            Member = member;
        }
    }
}
