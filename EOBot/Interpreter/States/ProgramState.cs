using System.Collections.Generic;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class ProgramState
    {
        public Stack<BotToken> OperationStack { get; } = [];

        public Stack<(string, BotToken, int)> CallStack { get; }

        public IReadOnlyList<BotToken> Program => Definition.Program;

        public Dictionary<string, (bool ReadOnly, IIdentifiable Identifiable)> SymbolTable { get; }

        public IReadOnlyDictionary<string, int> Labels => Definition.Labels;

        public int ExecutionIndex { get; private set; }

        public ProgramDefinition Definition { get; }

        public ProgramState(ProgramDefinition definition)
            : this(definition, new(definition.DeclaredSymbols), []) { }

        public ProgramState(
            ProgramDefinition definition,
            Dictionary<string, (bool ReadOnly, IIdentifiable Identifiable)> symbolTable,
            Stack<(string, BotToken, int)> callStack
        )
        {
            Definition = definition;
            SymbolTable = symbolTable;
            CallStack = callStack;
        }

        public void SkipToken()
        {
            ExecutionIndex++;
        }

        public bool Goto(int executionIndex)
        {
            if (executionIndex >= Program.Count)
                return false;

            ExecutionIndex = executionIndex;
            return true;
        }

        /// <summary>
        /// Check for a token at the program's execution index. If it is the expected type, increment execution index.
        /// </summary>
        public bool Expect(BotTokenType tokenType)
        {
            if (ExecutionIndex >= Program.Count)
                return tokenType == BotTokenType.EOF;

            if (Program[ExecutionIndex].TokenType == tokenType)
            {
                ExecutionIndex++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check for a token at the program's execution index. If it is the expected type, push it onto the operation stack and increment execution index.
        /// </summary>
        public bool Match(BotTokenType tokenType)
        {
            if (ExecutionIndex >= Program.Count)
                return false;

            if (Program[ExecutionIndex].TokenType == tokenType)
            {
                OperationStack.Push(Program[ExecutionIndex]);
                ExecutionIndex++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the identifiers of a name qualified with '::' (e.g. 'Math::Max') at the program's execution index, without changing the execution index.
        /// </summary>
        public SymbolName PeekQualifiedName()
        {
            var name = new List<BotToken>();
            BotToken danglingToken = null;
            for (var i = ExecutionIndex; i < Program.Count && Program[i].TokenType == BotTokenType.Identifier; i += 2)
            {
                name.Add(Program[i]);

                if (i + 1 >= Program.Count || Program[i + 1].TokenType != BotTokenType.ScopeResolution)
                    break;

                if (i + 2 < Program.Count && Program[i + 2].TokenType != BotTokenType.Identifier)
                    danglingToken = Program[i + 2];
            }

            return new SymbolName(name, danglingToken);
        }

        /// <summary>
        /// Checks for a pair of tokens. Advances the program index by 2 if they match.
        /// </summary>
        public bool ExpectPair(BotTokenType first, BotTokenType second)
        {
            var pairMatches = PeekPair(first, second);
            if (pairMatches)
            {
                ExecutionIndex += 2;
            }
            return pairMatches;
        }

        /// <summary>
        /// Checks for a pair of tokens. Places them on the operation stack and advances the program index by 2 if they match.
        /// </summary>
        public bool MatchPair(BotTokenType first, BotTokenType second)
        {
            var pairMatches = PeekPair(first, second);
            if (pairMatches)
            {
                OperationStack.Push(Program[ExecutionIndex]);
                OperationStack.Push(Program[ExecutionIndex + 1]);
                ExecutionIndex += 2;
            }
            return pairMatches;
        }

        private bool PeekPair(BotTokenType first, BotTokenType second)
        {
            if (ExecutionIndex >= Program.Count - 1)
                return false;

            if (Program[ExecutionIndex].TokenType == first &&
                Program[ExecutionIndex + 1].TokenType == second)
            {
                return true;
            }

            return false;
        }
    }
}
