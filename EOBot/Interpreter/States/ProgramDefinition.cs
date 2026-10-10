using System.Collections.Generic;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public record class ProgramDefinition(
        IReadOnlyList<BotToken> Program,
        IReadOnlyDictionary<string, (bool ReadOnly, IIdentifiable Identifiable)> DeclaredSymbols,
        IReadOnlyDictionary<string, int> Labels
    );
}
