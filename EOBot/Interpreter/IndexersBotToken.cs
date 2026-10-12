using System.Collections.Generic;
using System.Linq;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter
{
    public class IndexersBotToken : BotToken
    {
        public IReadOnlyList<IVariable> Indexers { get; }

        public IndexersBotToken(BotToken firstLBracket, IReadOnlyList<IVariable> indexers)
            : base(BotTokenType.Indexers, string.Concat(indexers.Select(x => $"[{x.StringValue}]")), firstLBracket.LineNumber, firstLBracket.Column)
        {
            Indexers = indexers;
        }
    }
}
