using EOBot.Interpreter.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.States
{
    public class ProgramState
    {
        public Stack<BotToken> OperationStack { get; }

        public IReadOnlyList<BotToken> Program { get; }

        public Dictionary<string, (bool ReadOnly, IIdentifiable Identifiable)> SymbolTable { get; }

        public Dictionary<string, int> Labels { get; }

        public int ExecutionIndex { get; private set; }

        public ProgramState(IReadOnlyList<BotToken> program)
        {
            OperationStack = new Stack<BotToken>();
            Program = program;
            SymbolTable = new Dictionary<string, (bool ReadOnly, IIdentifiable Identifiable)>();
            Labels = Program
                .Select((token, ndx) => (token, ndx))
                .Where(x => x.token.TokenType == BotTokenType.Identifier && Program[x.ndx + 1].TokenType == BotTokenType.Colon)
                .ToDictionary(x => x.token.TokenValue, y => y.ndx + 2);
            ExecutionIndex = 0;
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