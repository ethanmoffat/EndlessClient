using EOBot.Interpreter.Variables;
using System.Collections.Generic;

namespace EOBot.Interpreter.States
{
    public class ProgramState
    {
        public Stack<BotToken> OperationStack { get; }

        public IReadOnlyList<BotToken> Program { get; }

        public Dictionary<string, IIdentifiable> SymbolTable { get; }

        public Dictionary<LabelIdentifier, int> Labels { get; }

        public int ExecutionIndex { get; private set; }

        public ProgramState(IReadOnlyList<BotToken> program)
        {
            OperationStack = new Stack<BotToken>();
            Program = program;
            SymbolTable = new Dictionary<string, IIdentifiable>();
            Labels = new Dictionary<LabelIdentifier, int>();
            ExecutionIndex = 0;
        }

        public bool Expect(BotTokenType tokenType)
        {
            if (ExecutionIndex >= Program.Count)
                return false;

            return Program[ExecutionIndex].TokenType == tokenType;
        }

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
    }
}
