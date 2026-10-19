using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter
{
    /// <summary>
    /// An identifier optionally qualified by modules, e.g. 'Max' or 'Math::Max'
    /// </summary>
    public sealed class SymbolName
    {
        private readonly IReadOnlyList<BotToken> _parts;

        public bool IsEmpty => _parts.Count == 0;

        public bool IsQualified => _parts.Count > 1;

        public BotToken Identifier => _parts[^1];

        public SymbolName Qualifier => IsQualified ? new(_parts.Take(_parts.Count - 1).ToList()) : null;

        /// <summary>
        /// The token following a trailing '::' that is not an identifier, e.g. '1' in 'Outer::Inner::1'
        /// </summary>
        public BotToken DanglingToken { get; }

        public string QualifiedName => string.Join("::", _parts.Select(x => x.TokenValue));

        // identifiers are separated by '::' tokens
        public int TokenLength => IsEmpty ? 0 : _parts.Count * 2 - 1;

        public SymbolName(IReadOnlyList<BotToken> parts, BotToken danglingToken = null)
        {
            _parts = parts;
            DanglingToken = danglingToken;
        }

        public override string ToString() => QualifiedName;

        public static implicit operator string(SymbolName name) => name.QualifiedName;
    }
}
