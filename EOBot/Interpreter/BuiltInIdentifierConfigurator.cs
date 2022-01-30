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
            input.SymbolTable[PredefinedIdentifiers.PRINT_FUNC] = printFunc;

            var lenFunc = new FunctionRef<ArrayVariable, int>(PredefinedIdentifiers.LEN_FUNC, param1 => param1.Value.Count);
            input.SymbolTable[PredefinedIdentifiers.LEN_FUNC] = lenFunc;

            var arrayFunc = new FunctionRef<int, List<IVariable>>(PredefinedIdentifiers.ARRAY_FUNC,
                param1 => Enumerable.Repeat(UndefinedVariable.Instance, param1).Cast<IVariable>().ToList());
            input.SymbolTable.Add(PredefinedIdentifiers.ARRAY_FUNC, arrayFunc);
        }

        public void SetupBuiltInVariables(ProgramState input, ArgumentsParser parsedArgs)
        {
            input.SymbolTable[PredefinedIdentifiers.HOST] = new StringVariable(parsedArgs.Host);
            input.SymbolTable[PredefinedIdentifiers.PORT] = new IntVariable(parsedArgs.Port);
            input.SymbolTable[PredefinedIdentifiers.ARGS] = new ArrayVariable(
                parsedArgs.UserArgs.Select(x => new StringVariable(x)).Cast<IVariable>().ToList());

            input.SymbolTable[PredefinedIdentifiers.RESULT] = UndefinedVariable.Instance;
            input.SymbolTable[PredefinedIdentifiers.ACCOUNT] = UndefinedVariable.Instance;
            input.SymbolTable[PredefinedIdentifiers.CHARACTER] = UndefinedVariable.Instance;
            input.SymbolTable[PredefinedIdentifiers.MAPSTATE] = UndefinedVariable.Instance;
        }
    }
}
