using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter
{
    public class BuiltInSetup
    {
        public void SetupBuiltInFunctions(ProgramState input)
        {
            var function = new VoidFunctionRef<string>(PredefinedIdentifiers.PRINT_FUNC, param1 => ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, param1));
            input.SymbolTable[PredefinedIdentifiers.PRINT_FUNC] = function;
        }
    }
}
