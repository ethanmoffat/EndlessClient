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

        public bool ExpectPair(BotTokenType first, BotTokenType second)
        {
            if (ExecutionIndex >= Program.Count - 1)
                return false;

            if (Program[ExecutionIndex].TokenType == first &&
                Program[ExecutionIndex + 1].TokenType == second)
            {
                ExecutionIndex += 2;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Matches a pair of tokens in order at the program's execution index.
        /// </summary>
        public bool MatchPair(BotTokenType first, BotTokenType second)
        {
            if (ExecutionIndex >= Program.Count - 1)
                return false;

            if (Program[ExecutionIndex].TokenType == first &&
                Program[ExecutionIndex + 1].TokenType == second)
            {
                OperationStack.Push(Program[ExecutionIndex]);
                OperationStack.Push(Program[ExecutionIndex + 1]);
                ExecutionIndex += 2;

                return true;
            }

            return false;
        }
    }
}
