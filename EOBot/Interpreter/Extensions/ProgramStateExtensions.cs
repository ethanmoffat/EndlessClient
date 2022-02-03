using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.Extensions
{
    public static class ProgramStateExtensions
    {
        public static bool MatchOneOf(this ProgramState input, params BotTokenType[] tokenTypes)
        {
            // todo: there's probably LINQ for this but I'm tired and my brain isn't quite working
            // needs to stop trying to match after first match, can't use FirstOrDefault unless default(BotTokenType) can be used as an "empty" value
            foreach (var type in tokenTypes)
            {
                if (input.Match(type))
                    return true;
            }

            return false;
        }

        public static BotToken Current(this ProgramState input)
        {
            if (input.ExecutionIndex >= input.Program.Count)
                return input.Program[input.Program.Count - 1];

            return input.Program[input.ExecutionIndex];
        }

        public static (EvalResult Result, string Reason, IVariable Variable) GetVariable(this ProgramState input, string identifier, int? arrayIndex = null)
        {
            if (!input.SymbolTable.ContainsKey(identifier))
                input.SymbolTable[identifier] = (false, UndefinedVariable.Instance);

            var variableValue = input.SymbolTable[identifier].Identifiable as IVariable;
            if (variableValue == null)
            {
                return (EvalResult.Failed, $"Identifier {identifier} is not a variable", null);
            }

            if (arrayIndex != null)
            {
                var arrayVariable = variableValue as ArrayVariable;
                if (arrayVariable == null)
                {
                    return (EvalResult.Failed, $"Identifier {identifier} is not an array", null);
                }

                if (arrayVariable.Value.Count <= arrayIndex.Value)
                {
                    return (EvalResult.Failed, $"Index {identifier} is out of range of the array {identifier} (size {arrayVariable.Value.Count})", null);
                }

                variableValue = arrayVariable.Value[arrayIndex.Value];
            }

            return (EvalResult.Ok, string.Empty, variableValue);
        }

        public static (EvalResult Result, string Reason, T Variable) GetVariable<T>(this ProgramState input, string identifier, int? arrayIndex = null)
            where T : class, IVariable
        {
            var getResult = GetVariable(input, identifier, arrayIndex);

            if (getResult.Result != EvalResult.Ok)
                return (getResult.Result, getResult.Reason, null);

            var variable = getResult.Variable as T;
            if (variable == null)
                return (EvalResult.Failed, $"Identifier {identifier} is not a {typeof(T).Name}", null);

            return (EvalResult.Ok, string.Empty, variable);
        }
    }
}
