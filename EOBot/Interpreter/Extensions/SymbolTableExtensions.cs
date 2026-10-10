using System.Collections.Generic;
using System.Linq;
using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.Extensions
{
    public static class SymbolTableExtensions
    {

        public static (EvalResult Result, string Reason, IVariable Variable) GetVariable(this Dictionary<string, (bool, IIdentifiable Identifiable)> symbols, string identifier, IVariable indexer = null)
        {
            if (!symbols.ContainsKey(identifier))
                symbols[identifier] = (false, UndefinedVariable.Instance);

            if (symbols[identifier].Identifiable is not IVariable variableValue)
            {
                return (EvalResult.Failed, $"Identifier {identifier} is not a variable", null);
            }

            if (indexer != null)
            {
                if (variableValue is ArrayVariable arrayVariable)
                {
                    if (indexer is not IntVariable iv)
                    {
                        return (EvalResult.Failed, $"Expected array indexer {indexer} to be an integer", null);
                    }

                    if (arrayVariable.Value.Count <= iv.Value)
                    {
                        return (EvalResult.Failed, $"Index {iv.Value} is out of range of the array {identifier} (size {arrayVariable.Value.Count})", null);
                    }

                    variableValue = arrayVariable.Value[iv.Value];
                }
                else if (variableValue is DictVariable dictVariable)
                {
                    variableValue = dictVariable.Value.TryGetValue(indexer.StringValue, out var entry)
                        ? entry
                        : UndefinedVariable.Instance;
                }
            }

            return (EvalResult.Ok, string.Empty, variableValue);
        }

        public static (EvalResult Result, string Reason, T Variable) GetVariable<T>(this Dictionary<string, (bool, IIdentifiable)> symbols, string identifier, IVariable indexer = null)
            where T : class, IVariable
        {
            var getResult = GetVariable(symbols, identifier, indexer);

            if (getResult.Result != EvalResult.Ok)
                return (getResult.Result, getResult.Reason, null);

            var variable = getResult.Variable as T;
            if (variable == null)
                return (EvalResult.Failed, $"Identifier {identifier} is not a {typeof(T).Name}", null);

            return (EvalResult.Ok, string.Empty, variable);
        }

        /// <summary>
        /// Resolve a symbol identified by the identifier token out of the symbol table
        /// </summary>
        public static (EvalResult, string, BotToken) ResolveIdentifier(this Dictionary<string, (bool, IIdentifiable)> symbols, BotToken input)
        {
            if (input is not IdentifierBotToken identifier)
                return (EvalResult.Failed, $"Expected operand of type Variable or Identifier but got {input.TokenType}", input);

            if (identifier.Member == null)
            {
                var getVariableRes = symbols.GetVariable(identifier.TokenValue, identifier.Indexer);
                if (getVariableRes.Result != EvalResult.Ok)
                    return (getVariableRes.Result, getVariableRes.Reason, identifier);

                return (EvalResult.Ok, string.Empty, new VariableBotToken(BotTokenType.Literal, getVariableRes.Variable.ToString(), getVariableRes.Variable));
            }
            else
            {
                var getVariableRes = symbols.GetVariable(identifier.TokenValue, identifier.Indexer);
                if (getVariableRes.Result != EvalResult.Ok)
                {
                    return (getVariableRes.Result, getVariableRes.Reason, identifier);
                }

                var variable = getVariableRes.Variable switch
                {
                    ObjectVariable objectVariable => objectVariable,
                    RuntimeEvaluatedMemberObjectVariable runtimeVariable =>
                        new ObjectVariable(
                            runtimeVariable.SymbolTable
                                .Select(x => (x.Key, Identifier: (x.Value.ReadOnly, x.Value.Variable())))
                                .ToDictionary(x => x.Key, x => x.Identifier)
                        ),
                    EnumVariable enumVariable => enumVariable.ToObject(),
                    _ => null,
                };

                return variable == null
                    ? (EvalResult.Failed, $"Identifier '{identifier.TokenValue}' is not an object", identifier)
                    : variable.SymbolTable.ResolveIdentifier(identifier.Member);
            }
        }
    }
}
