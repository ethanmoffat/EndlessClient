using System.Collections.Generic;
using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.Extensions
{
    public static class SymbolTableExtensions
    {

        public static (EvalResult Result, string Reason, IVariable Variable) GetVariable(this Dictionary<string, (bool, IIdentifiable Identifiable)> symbols, string identifier, IEnumerable<IVariable> indexers = null)
        {
            if (!symbols.ContainsKey(identifier))
                symbols[identifier] = (false, UndefinedVariable.Instance);

            if (symbols[identifier].Identifiable is not IVariable variableValue)
            {
                return (EvalResult.Failed, $"Identifier {identifier} is not a variable", null);
            }

            foreach (var indexer in indexers ?? [])
            {
                var indexRes = variableValue.Index(indexer, identifier);
                if (indexRes.Result != EvalResult.Ok)
                    return indexRes;

                variableValue = indexRes.Variable;
            }

            return (EvalResult.Ok, string.Empty, variableValue);
        }

        public static (EvalResult Result, string Reason, T Variable) GetVariable<T>(
            this Dictionary<string, (bool, IIdentifiable)> symbols,
            string identifier,
            IEnumerable<IVariable> indexers = null)
            where T : class, IVariable
        {
            var getResult = GetVariable(symbols, identifier, indexers);

            if (getResult.Result != EvalResult.Ok)
                return (getResult.Result, getResult.Reason, null);

            var variable = getResult.Variable as T;
            if (variable == null)
                return (EvalResult.Failed, $"Identifier {identifier} is not a {typeof(T).Name}", null);

            return (EvalResult.Ok, string.Empty, variable);
        }

        /// <summary>
        /// Resolve a name qualified by modules (e.g. [Math, Max] for 'Math::Max') out of the symbol table
        /// </summary>
        public static (EvalResult Result, string Reason, BotToken Token) TryResolveQualifiedName(
            this Dictionary<string, (bool, IIdentifiable Identifiable)> symbols,
            SymbolName name,
            out IIdentifiable symbol)
        {
            symbol = null;

            if (!name.IsQualified)
            {
                if (!symbols.TryGetValue(name.Identifier.TokenValue, out var root))
                    return (EvalResult.Failed, $"Identifier {name} is not defined", name.Identifier);

                symbol = root.Identifiable;
                return (EvalResult.Ok, string.Empty, name.Identifier);
            }

            var res = symbols.TryResolveQualifiedName(name.Qualifier, out var qualifier);
            if (res.Result != EvalResult.Ok)
                return res;

            if (qualifier is not ModuleDefinition module)
                return (EvalResult.Failed, $"Identifier {name.Qualifier} is not a module", name.Qualifier.Identifier);

            if (!module.Members.TryGetValue(name.Identifier.TokenValue, out symbol))
                return (EvalResult.Failed, $"Identifier {name} is not defined", name.Identifier);

            return (EvalResult.Ok, string.Empty, name.Identifier);
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
                var getVariableRes = symbols.GetVariable(identifier.TokenValue, identifier.Indexers);
                if (getVariableRes.Result != EvalResult.Ok)
                    return (getVariableRes.Result, getVariableRes.Reason, identifier);

                return (EvalResult.Ok, string.Empty, VariableBotToken.FromComputedValue(BotTokenType.Literal, getVariableRes.Variable));
            }
            else
            {
                var getVariableRes = symbols.GetVariable(identifier.TokenValue, identifier.Indexers);
                if (getVariableRes.Result != EvalResult.Ok)
                {
                    return (getVariableRes.Result, getVariableRes.Reason, identifier);
                }

                var variable = getVariableRes.Variable switch
                {
                    ObjectVariable objectVariable => objectVariable,
                    RuntimeEvaluatedMemberObjectVariable runtimeVariable => runtimeVariable.EvaluateMember(identifier.Member.TokenValue),
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
