using System.Collections.Generic;
using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.Extensions
{
    public static class SymbolTableExtensions
    {

        public static (EvalResult Result, string Reason, IVariable Variable) GetVariable(this Dictionary<string, (bool, IIdentifiable Identifiable)> symbols, string identifier, int? arrayIndex = null)
        {
            if (!symbols.ContainsKey(identifier))
                symbols[identifier] = (false, UndefinedVariable.Instance);

            var variableValue = symbols[identifier].Identifiable as IVariable;
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
                    return (EvalResult.Failed, $"Index {arrayIndex.Value} is out of range of the array {identifier} (size {arrayVariable.Value.Count})", null);
                }

                variableValue = arrayVariable.Value[arrayIndex.Value];
            }

            return (EvalResult.Ok, string.Empty, variableValue);
        }

        public static (EvalResult Result, string Reason, T Variable) GetVariable<T>(this Dictionary<string, (bool, IIdentifiable)> symbols, string identifier, int? arrayIndex = null)
            where T : class, IVariable
        {
            var getResult = GetVariable(symbols, identifier, arrayIndex);

            if (getResult.Result != EvalResult.Ok)
                return (getResult.Result, getResult.Reason, null);

            var variable = getResult.Variable as T;
            if (variable == null)
                return (EvalResult.Failed, $"Identifier {identifier} is not a {typeof(T).Name}", null);

            return (EvalResult.Ok, string.Empty, variable);
        }
    }
}
