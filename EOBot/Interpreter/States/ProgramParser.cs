using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Hosting;
using EOBot.Interpreter.Mapping;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class ProgramParser
    {
        private readonly ScriptHost _host;

        /// <param name="host">The session that provides .NET types for map statements, or null if mapping is not available.</param>
        public ProgramParser(ScriptHost host)
        {
            _host = host;
        }

        /// <summary>
        /// Hoists function, enum, and map declarations out of the program tokens and indexes its labels.
        /// </summary>
        public ProgramDefinition Parse(List<BotToken> program, bool isFunctionBody = false)
        {
            var declaredSymbols = GetDeclaredSymbols(program, isFunctionBody);
            return new ProgramDefinition(program, declaredSymbols, GetLabels(program));
        }

        private Dictionary<string, int> GetLabels(IReadOnlyList<BotToken> program)
        {
            return program
                .Select((token, ndx) => (token, ndx))
                .Where(x => x.token.TokenType == BotTokenType.Identifier && program[x.ndx + 1].TokenType == BotTokenType.Colon)
                .Where(x => x.ndx == 0 || program[x.ndx - 1].TokenType != BotTokenType.ScopeResolution)
                .ToDictionary(x => x.token.TokenValue, y => y.ndx + 2);
        }

        // m o m ' s   s p a g h e t t i
        private Dictionary<string, (bool, IIdentifiable)> GetDeclaredSymbols(List<BotToken> program, bool isFunctionBody)
        {
            var retDict = new Dictionary<string, (bool, IIdentifiable)>();

            for (int i = 0; i < program.Count; i++)
            {
                if (program[i].Is(BotTokenType.Keyword, Keywords.Func))
                {
                    GetFunction(retDict, program, ref i);
                }
                // 'enum' following 'is' is a type specifier, not a declaration
                else if ((program[i].Is(BotTokenType.Keyword, Keywords.Enum) && (i == 0 || !program[i - 1].Is(BotTokenType.IsOperator)))
                    || program[i].Is(BotTokenType.Keyword, Keywords.Map)
                    || program[i].Is(BotTokenType.Keyword, Keywords.From))
                {
                    if (isFunctionBody)
                        throw new BotScriptErrorException($"'{program[i].TokenValue}' declarations are only allowed at the top level of a script", program[i]);

                    if (program[i].Is(BotTokenType.Keyword, Keywords.Enum))
                        GetEnum(retDict, program, ref i);
                    else
                        GetMapping(retDict, program, ref i);
                }
            }

            return retDict;
        }

        private void GetFunction(Dictionary<string, (bool, IIdentifiable)> retDict, List<BotToken> program, ref int i)
        {
            int funcStartIndex = i;

            var funcToken = program[i++];
            var funcName = NextToken(program, ref i, funcToken);

            SkipNewLines(program, ref i);

            var lparen = NextToken(program, ref i, funcName);

            var paramSpecs = new List<BotToken>();
            var commas = new List<BotToken>();
            var firstParam = true;
            while (program[i].TokenType != BotTokenType.RParen)
            {
                if (!firstParam)
                {
                    commas.Add(NextToken(program, ref i, lparen));
                }

                SkipNewLines(program, ref i);

                var nextParam = NextToken(program, ref i, lparen);
                paramSpecs.Add(nextParam);
                firstParam = false;

                SkipNewLines(program, ref i);
            }

            var rparen = NextToken(program, ref i, lparen);

            SkipNewLines(program, ref i);

            var lbrace = NextToken(program, ref i, rparen);

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
            if (!(errorToken = funcToken).Is(BotTokenType.Keyword, Keywords.Func) ||
                !(errorToken = funcName).Is(BotTokenType.Identifier) || !(errorToken = lparen).Is(BotTokenType.LParen) ||
                !paramSpecs.All(x => (errorToken = x).Is(BotTokenType.Variable)) ||
                !commas.All(x => (errorToken = x).Is(BotTokenType.Comma)) ||
                !(errorToken = rparen).Is(BotTokenType.RParen) || !(errorToken = lbrace).Is(BotTokenType.LBrace))
            {
                throw new BotScriptErrorException("Unexpected token in function definition", errorToken);
            }

            var funcRange = program.GetRange(tokenStartIndex, i - tokenStartIndex - 1);
            funcRange.Add(new BotToken(BotTokenType.EOF, string.Empty, 0, 0));

            if (retDict.ContainsKey(funcName.TokenValue))
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, $"Redefining function {funcName.TokenValue} at line {funcName.LineNumber}", ConsoleColor.Yellow);

            retDict[funcName.TokenValue] = (true, new UserDefinedFunction(funcName.TokenValue, Parse(funcRange, isFunctionBody: true), paramSpecs));

            program.RemoveRange(funcStartIndex, i - funcStartIndex);
            i = funcStartIndex - 1;
        }

        private void GetEnum(Dictionary<string, (bool, IIdentifiable)> retDict, List<BotToken> program, ref int i)
        {
            int enumStartIndex = i;

            var enumToken = program[i++];
            var enumName = NextToken(program, ref i, enumToken);
            if (!enumName.Is(BotTokenType.Identifier))
                throw new BotScriptErrorException("Expected identifier for enum name", enumName);

            SkipNewLines(program, ref i);

            var lBrace = NextToken(program, ref i, enumName);
            if (!lBrace.Is(BotTokenType.LBrace))
                throw new BotScriptErrorException("Expected '{' in enum definition", lBrace);

            var members = new List<(string Name, int Value)>();
            var nextValue = 0;

            SkipNewLines(program, ref i);
            while (!program[i].Is(BotTokenType.RBrace))
            {
                var memberName = NextToken(program, ref i, enumName);
                if (!memberName.Is(BotTokenType.Identifier))
                    throw new BotScriptErrorException("Expected identifier for enum member", memberName);

                if (members.Any(x => x.Name == memberName.TokenValue))
                    throw new BotScriptErrorException($"Duplicate member {memberName.TokenValue} in enum {enumName.TokenValue}", memberName);

                SkipNewLines(program, ref i);

                if (program[i].Is(BotTokenType.AssignOperator))
                {
                    i++;
                    SkipNewLines(program, ref i);

                    var valueToken = NextToken(program, ref i, memberName);
                    var negate = valueToken.Is(BotTokenType.MinusOperator);
                    if (negate)
                        valueToken = NextToken(program, ref i, valueToken);

                    if (valueToken is LiteralBotToken { LiteralValue: int literalValue })
                        nextValue = negate ? -literalValue : literalValue;
                    else
                        throw new BotScriptErrorException("Expected integer literal for enum member value", valueToken);

                    SkipNewLines(program, ref i);
                }

                members.Add((memberName.TokenValue, nextValue++));

                if (!program[i].Is(BotTokenType.RBrace))
                {
                    var delimiter = NextToken(program, ref i, memberName);
                    if (!delimiter.Is(BotTokenType.Comma))
                        throw new BotScriptErrorException("Expected ',' or '}' after enum member", delimiter);

                    SkipNewLines(program, ref i);
                }
            }

            i++;

            if (retDict.ContainsKey(enumName.TokenValue))
                throw new BotScriptErrorException($"Symbol {enumName.TokenValue} is already defined", enumName);

            retDict[enumName.TokenValue] = (true, EnumDefinition.FromScriptDefinition(enumName.TokenValue, members));

            program.RemoveRange(enumStartIndex, i - enumStartIndex);
            i = enumStartIndex - 1;
        }

        /// <summary>
        /// Parses: [from Namespace.Path] map (TypeName | TypeName.MethodName | *) [to Alias]
        /// </summary>
        private void GetMapping(Dictionary<string, (bool, IIdentifiable)> retDict, List<BotToken> program, ref int i)
        {
            int mapStartIndex = i;

            string ns = null;
            if (program[i].Is(BotTokenType.Keyword, Keywords.From))
            {
                var previous = program[i++];
                var parts = new List<string>();
                while (true)
                {
                    var part = NextToken(program, ref i, previous);
                    if (!part.IsOneOf(BotTokenType.Identifier, BotTokenType.Keyword, BotTokenType.TypeSpecifier))
                        throw new BotScriptErrorException("Expected namespace", part);

                    parts.Add(part.TokenValue);

                    if (!program[i].Is(BotTokenType.Dot))
                        break;

                    previous = program[i++];
                }

                ns = string.Join(".", parts);
            }

            var mapToken = NextToken(program, ref i, program[mapStartIndex]);
            if (!mapToken.Is(BotTokenType.Keyword, Keywords.Map))
                throw new BotScriptErrorException("Expected 'map' keyword", mapToken);

            var typeToken = NextToken(program, ref i, mapToken);
            var isWildcard = typeToken.Is(BotTokenType.MultiplyOperator);
            if (!isWildcard && !typeToken.Is(BotTokenType.Identifier))
                throw new BotScriptErrorException("Expected type name or '*' after 'map'", typeToken);

            BotToken methodToken = null;
            if (!isWildcard && program[i].Is(BotTokenType.Dot))
            {
                var dotToken = program[i++];
                methodToken = NextToken(program, ref i, dotToken);
                if (!methodToken.Is(BotTokenType.Identifier))
                    throw new BotScriptErrorException("Expected method name after '.'", methodToken);
            }

            BotToken aliasToken = null;
            if (program[i].Is(BotTokenType.Keyword, Keywords.To))
            {
                var toToken = program[i++];
                aliasToken = NextToken(program, ref i, toToken);
                if (!aliasToken.Is(BotTokenType.Identifier))
                    throw new BotScriptErrorException("Expected identifier for alias after 'to'", aliasToken);
            }

            if (!program[i].IsOneOf(BotTokenType.NewLine, BotTokenType.EOF))
                throw new BotScriptErrorException("Unexpected token after map statement", program[i]);

            if (_host == null)
                throw new BotScriptErrorException("Mapping .NET types is not supported without a script host", mapToken);

            if (methodToken != null)
            {
                var function = GetMappedFunction(ns, typeToken, methodToken, aliasToken);
                if (retDict.ContainsKey(function.Name))
                    throw new BotScriptErrorException($"Symbol {function.Name} is already defined", aliasToken ?? methodToken);

                retDict[function.Name] = (true, function);
            }
            else
            {
                foreach (var definition in GetMappedEnums(ns, typeToken, aliasToken, isWildcard))
                {
                    if (retDict.ContainsKey(definition.Name))
                        throw new BotScriptErrorException($"Symbol {definition.Name} is already defined", aliasToken ?? typeToken);

                    retDict[definition.Name] = (true, definition);
                }
            }

            program.RemoveRange(mapStartIndex, i - mapStartIndex);
            i = mapStartIndex - 1;
        }

        private List<EnumDefinition> GetMappedEnums(string ns, BotToken typeToken, BotToken aliasToken, bool isWildcard)
        {
            if (isWildcard)
            {
                if (ns == null)
                    throw new BotScriptErrorException("Mapping all types with '*' requires a namespace ('from <namespace> map *')", typeToken);
                if (aliasToken != null)
                    throw new BotScriptErrorException("An alias cannot be specified when mapping all types with '*'", aliasToken);

                var types = _host.TypeCatalog.FindEnumsByNamespace(ns);
                if (types.Count == 0)
                    throw new BotScriptErrorException($"No enum types found in namespace {ns}", typeToken);

                return [.. types.Select(x => EnumDefinition.FromType(x))];
            }

            var namedTypes = _host.TypeCatalog.FindEnumsByName(typeToken.TokenValue, ns);
            if (namedTypes.Count == 0)
                throw new BotScriptErrorException($"No enum type {typeToken.TokenValue} found{(ns == null ? string.Empty : $" in namespace {ns}")}", typeToken);
            if (namedTypes.Count > 1)
                throw new BotScriptErrorException($"Enum type {typeToken.TokenValue} is ambiguous between: {string.Join(", ", namedTypes.Select(x => x.FullName))}. Use 'from <namespace> map {typeToken.TokenValue}'", typeToken);

            return [EnumDefinition.FromType(namedTypes[0], aliasToken?.TokenValue)];
        }

        private DotNetFunction GetMappedFunction(string ns, BotToken typeToken, BotToken methodToken, BotToken aliasToken)
        {
            if (ns == null)
                throw new BotScriptErrorException($"Mapping a method requires a namespace ('from <namespace> map {typeToken.TokenValue}.{methodToken.TokenValue}')", typeToken);

            var type = _host.TypeCatalog.FindClass(ns, typeToken.TokenValue)
                ?? throw new BotScriptErrorException($"No class {typeToken.TokenValue} found in namespace {ns}", typeToken);

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.Name == methodToken.TokenValue)
                .ToList();
            if (methods.Count == 0)
                throw new BotScriptErrorException($"No public static method {methodToken.TokenValue} found on type {type.FullName}", methodToken);

            var overloads = methods.Where(_host.Converter.IsSupported).ToList();
            if (overloads.Count == 0)
                throw new BotScriptErrorException($"Method {type.FullName}.{methodToken.TokenValue} has no overloads with parameter and return types that can be used in scripts", methodToken);

            return new DotNetFunction(aliasToken?.TokenValue ?? methodToken.TokenValue, overloads, _host.Converter);
        }

        private static BotToken NextToken(List<BotToken> program, ref int i, BotToken previous)
        {
            if (i >= program.Count || program[i].Is(BotTokenType.EOF))
                throw new BotScriptErrorException("Unexpected end of file", previous);

            return program[i++];
        }

        private static void SkipNewLines(List<BotToken> program, ref int i)
        {
            while (i < program.Count && program[i].TokenType == BotTokenType.NewLine) i++;
        }
    }
}
