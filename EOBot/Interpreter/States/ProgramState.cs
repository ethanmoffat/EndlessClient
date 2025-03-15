using System.Collections.Generic;
using System.Linq;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class ProgramState
    {
        public Stack<BotToken> OperationStack { get; } = [];

        public Stack<(string, BotToken, int)> CallStack { get; private set; } = [];

        public IReadOnlyList<BotToken> Program { get; }

        public Dictionary<string, (bool ReadOnly, IIdentifiable Identifiable)> SymbolTable { get; private set; }

        public Dictionary<string, int> Labels { get; }

        public int ExecutionIndex { get; private set; }

        public ProgramState(List<BotToken> program)
        {
            Program = program;

            SymbolTable = GetFunctions(program);
            Labels = GetLabels(Program);

            OperationStack = [];
            CallStack = [];
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

        /// <summary>
        /// Inherits the state from the parent programState. This includes any read-only variables and call stack.
        ///
        /// This is a destructive operation that clears the symbol table and execution stacks.
        /// </summary>
        /// <param name="parentState">The parent program.</param>
        public void InheritFrom(ProgramState parentState)
        {
            OperationStack.Clear();
            SymbolTable = parentState.SymbolTable;
            CallStack = parentState.CallStack;
        }

        private static Dictionary<string, int> GetLabels(IReadOnlyList<BotToken> program)
        {
            return program
                .Select((token, ndx) => (token, ndx))
                .Where(x => x.token.TokenType == BotTokenType.Identifier && program[x.ndx + 1].TokenType == BotTokenType.Colon)
                .ToDictionary(x => x.token.TokenValue, y => y.ndx + 2);
        }

        // m o m ' s   s p a g h e t t i
        private static Dictionary<string, (bool, IIdentifiable)> GetFunctions(List<BotToken> program)
        {
            var retDict = new Dictionary<string, (bool, IIdentifiable)>();

            for (int i = 0; i < program.Count; i++)
            {
                if (!program[i].Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_FUNC))
                    continue;

                int funcStartIndex = i;

                var funcToken = program[i++];
                var funcName = program[i++];
                var lparen = program[i++];

                var paramSpecs = new List<BotToken>();
                var commas = new List<BotToken>();
                var firstParam = true;
                while (program[i].TokenType != BotTokenType.RParen)
                {
                    if (!firstParam)
                    {
                        commas.Add(program[i++]);
                    }

                    while (i < program.Count && program[i].TokenType == BotTokenType.NewLine) i++;

                    var nextParam = program[i++];
                    paramSpecs.Add(nextParam);
                    firstParam = false;

                    while (i < program.Count && program[i].TokenType == BotTokenType.NewLine) i++;
                }

                var rparen = program[i++];

                while (i < program.Count && program[i].TokenType == BotTokenType.NewLine) i++;

                var lbrace = program[i++];

                var tokenStartIndex = i; // incremented past first LBrace

                var braceCount = 1;
                while (braceCount > 0 && i < program.Count)
                {
                    if (program[i].TokenType == BotTokenType.LBrace)
                        braceCount++;
                    else if (program[i].TokenType == BotTokenType.RBrace)
                        braceCount--;

                    i++;
                }

                BotToken errorToken;
                if (!(errorToken = funcToken).Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_FUNC) ||
                    !(errorToken = funcName).Is(BotTokenType.Identifier) || !(errorToken = lparen).Is(BotTokenType.LParen) ||
                    !paramSpecs.All(x => (errorToken = x).Is(BotTokenType.Variable)) ||
                    !commas.All(x => (errorToken = x).Is(BotTokenType.Comma)) ||
                    !(errorToken = rparen).Is(BotTokenType.RParen) || !(errorToken = lbrace).Is(BotTokenType.LBrace))
                {
                    throw new BotScriptErrorException("Unexpected token in function definition", errorToken);
                }

                var funcRange = program.GetRange(tokenStartIndex, i - tokenStartIndex - 1);
                funcRange.Add(new BotToken(BotTokenType.EOF, string.Empty, 0, 0));

                retDict[funcName.TokenValue] = (true, new UserDefinedFunction(funcName.TokenValue, funcRange, paramSpecs));

                program.RemoveRange(funcStartIndex, i - funcStartIndex);
                i = funcStartIndex;
            }

            return retDict;
        }
    }
}
