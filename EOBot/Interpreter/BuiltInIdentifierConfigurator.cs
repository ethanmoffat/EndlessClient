using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter
{
    public class BuiltInIdentifierConfigurator
    {
        public void SetupBuiltInFunctions(ProgramState input)
        {
            var printFunc = new VoidFunctionRef<object>(PredefinedIdentifiers.PRINT_FUNC, param1 => ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, param1.ToString()));
            input.SymbolTable[PredefinedIdentifiers.PRINT_FUNC] = (false, printFunc);

            var lenFunc = new FunctionRef<ArrayVariable, int>(PredefinedIdentifiers.LEN_FUNC, param1 => param1.Value.Count);
            input.SymbolTable[PredefinedIdentifiers.LEN_FUNC] = (false, lenFunc);

            var arrayFunc = new FunctionRef<int, List<IVariable>>(PredefinedIdentifiers.ARRAY_FUNC,
                param1 => Enumerable.Repeat(UndefinedVariable.Instance, param1).Cast<IVariable>().ToList());
            input.SymbolTable[PredefinedIdentifiers.ARRAY_FUNC] = (false, arrayFunc);
        }

        public void SetupBuiltInVariables(ProgramState input, ArgumentsParser parsedArgs)
        {
            input.SymbolTable[PredefinedIdentifiers.HOST] = (false, new StringVariable(parsedArgs.Host));
            input.SymbolTable[PredefinedIdentifiers.PORT] = (false, new IntVariable(parsedArgs.Port));
            input.SymbolTable[PredefinedIdentifiers.ARGS] = (false, new ArrayVariable(
                parsedArgs.UserArgs.Select(x => new StringVariable(x)).Cast<IVariable>().ToList()));

            input.SymbolTable[PredefinedIdentifiers.RESULT] = (true, UndefinedVariable.Instance);
            input.SymbolTable[PredefinedIdentifiers.ACCOUNT] = (false, UndefinedVariable.Instance);
            input.SymbolTable[PredefinedIdentifiers.CHARACTER] = (false, UndefinedVariable.Instance);
            input.SymbolTable[PredefinedIdentifiers.MAPSTATE] = (false, UndefinedVariable.Instance);
        }
    }
}
