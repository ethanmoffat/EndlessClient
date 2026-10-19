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
        /// Parses: [from Namespace.Path] map [object] (Type[.Nested...][.MethodName | .*] | *) [to Alias]
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

            var objectToken = program[i].Is(BotTokenType.Identifier, Keywords.MapObject) && program[i + 1].Is(BotTokenType.Identifier)
                ? program[i++]
                : null;

            var path = new List<BotToken>();
            BotToken segment;
            do
            {
                var previous = path.Count == 0 ? objectToken ?? mapToken : program[i++];
                segment = NextToken(program, ref i, previous);
                if (!segment.IsOneOf(BotTokenType.Identifier, BotTokenType.MultiplyOperator))
                    throw new BotScriptErrorException(path.Count == 0 ? "Expected type name or '*' after 'map'" : "Expected member name or '*' after '.'", segment);

                path.Add(segment);
            }
            while (!segment.Is(BotTokenType.MultiplyOperator) && program[i].Is(BotTokenType.Dot));

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

            List<(string Name, IIdentifiable Symbol)> symbols =
                objectToken != null
                    ? [GetMappedObject(ns, path, aliasToken)]
                    : path.Count == 1 && !IsClass(ns, path[0])
                        ? GetMappedEnums(ns, path[0], aliasToken)
                        : GetMappedMembers(ns, path, aliasToken);

            foreach (var (name, symbol) in symbols)
            {
                if (retDict.ContainsKey(name))
                    throw new BotScriptErrorException($"Symbol {name} is already defined", aliasToken ?? path[^1]);

                retDict[name] = (true, symbol);
            }

            program.RemoveRange(mapStartIndex, i - mapStartIndex);
            i = mapStartIndex - 1;
        }

        private List<(string, IIdentifiable)> GetMappedEnums(string ns, BotToken typeToken, BotToken aliasToken)
        {
            if (typeToken.Is(BotTokenType.MultiplyOperator))
            {
                if (ns == null)
                    throw new BotScriptErrorException("Mapping all types with '*' requires a namespace ('from <namespace> map *')", typeToken);
                if (aliasToken != null)
                    throw new BotScriptErrorException("An alias cannot be specified when mapping all types with '*'", aliasToken);

                var types = _host.TypeCatalog.FindEnumsByNamespace(ns);
                if (types.Count == 0)
                    throw new BotScriptErrorException($"No enum types found in namespace {ns}", typeToken);

                return [.. types.Select(x => EnumDefinition.FromType(x)).Select(x => (x.Name, (IIdentifiable)x))];
            }

            var namedTypes = _host.TypeCatalog.FindEnumsByName(typeToken.TokenValue, ns);
            if (namedTypes.Count == 0)
                throw new BotScriptErrorException($"No enum type {typeToken.TokenValue} found{(ns == null ? string.Empty : $" in namespace {ns}")}", typeToken);
            if (namedTypes.Count > 1)
                throw new BotScriptErrorException($"Enum type {typeToken.TokenValue} is ambiguous between: {string.Join(", ", namedTypes.Select(x => x.FullName))}. Use 'from <namespace> map {typeToken.TokenValue}'", typeToken);

            var definition = EnumDefinition.FromType(namedTypes[0], aliasToken?.TokenValue);
            return [(definition.Name, definition)];
        }

        /// <summary>
        /// Maps a class or nested type (Class, Class.Nested.Enum), Class.Method, or Class.*
        /// </summary>
        private List<(string, IIdentifiable)> GetMappedMembers(string ns, List<BotToken> path, BotToken aliasToken)
        {
            if (ns == null)
                throw new BotScriptErrorException($"Mapping a class member requires a namespace ('from <namespace> map {string.Join(".", path.Select(x => x.TokenValue))}')", path[0]);

            var (type, typeLength) = FindNestedType(ns, path);
            if (typeLength == path.Count)
            {
                if (type.IsEnum)
                {
                    var definition = EnumDefinition.FromType(type, aliasToken?.TokenValue);
                    return [(definition.Name, definition)];
                }

                var module = GetModule(aliasToken?.TokenValue ?? type.Name, type);

                if (module.Members.Count == 0)
                {
                    throw new BotScriptErrorException($"Type {type.FullName} has no public static methods or nested types that can be used in scripts", path[^1]);
                }

                return [(module.Name, module)];
            }

            if (typeLength < path.Count - 1)
                throw new BotScriptErrorException($"No nested type {path[typeLength].TokenValue} found in type {type.FullName}", path[typeLength]);

            var memberToken = path[^1];
            if (memberToken.Is(BotTokenType.MultiplyOperator))
                return GetAllMappedFunctions(type, memberToken, aliasToken);

            var function = GetMappedFunction(type, memberToken, aliasToken);
            return [(function.Name, function)];
        }

        private (string, IIdentifiable) GetMappedObject(string ns, List<BotToken> path, BotToken aliasToken)
        {
            if (ns == null)
                throw new BotScriptErrorException($"Mapping an object requires a namespace ('from <namespace> map object {string.Join(".", path.Select(x => x.TokenValue))}')", path[0]);
            if (path[^1].Is(BotTokenType.MultiplyOperator))
                throw new BotScriptErrorException("Expected class name after 'object'", path[^1]);

            var (type, typeLength) = FindNestedType(ns, path);
            if (typeLength < path.Count)
                throw new BotScriptErrorException($"No nested type {path[typeLength].TokenValue} found in type {type.FullName}", path[typeLength]);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(_host.Converter.IsSupported)
                .ToList();
            if (properties.Count == 0)
                throw new BotScriptErrorException($"Type {type.FullName} has no public static properties with types that can be used in scripts", path[^1]);

            return (aliasToken?.TokenValue ?? type.Name, _host.Converter.ToObject(properties));
        }

        private (Type Type, int Length) FindNestedType(string ns, List<BotToken> path)
        {
            var type = _host.TypeCatalog.FindClass(ns, path[0].TokenValue)
                ?? throw new BotScriptErrorException($"No class {path[0].TokenValue} found in namespace {ns}", path[0]);

            var length = 1;
            while (length < path.Count && type.GetNestedType(path[length].TokenValue) is { } nested)
            {
                type = nested;
                length++;
            }

            return (type, length);
        }

        private bool IsClass(string ns, BotToken typeToken) => ns != null && _host.TypeCatalog.FindClass(ns, typeToken.TokenValue) != null;

        /// <summary>
        /// Creates a module of a class's public static methods, nested enums, and nested classes. The module has no members if the class has none that can be used in scripts.
        /// </summary>
        private ModuleDefinition GetModule(string name, Type type)
        {
            var members = new Dictionary<string, IIdentifiable>();
            foreach (var function in GetStaticFunctions(type))
                members[function.Name] = function;

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public).Where(x => !x.ContainsGenericParameters))
            {
                if (nested.IsEnum)
                    members[nested.Name] = EnumDefinition.FromType(nested);
                else if (nested.IsClass && GetModule(nested.Name, nested) is { Members.Count: > 0 } module)
                    members[nested.Name] = module;
            }

            return new ModuleDefinition(name, members);
        }

        private List<(string, IIdentifiable)> GetAllMappedFunctions(Type type, BotToken wildcardToken, BotToken aliasToken)
        {
            if (aliasToken != null)
                throw new BotScriptErrorException("An alias cannot be specified when mapping all methods with '*'", aliasToken);

            var functions = GetStaticFunctions(type);
            if (functions.Count == 0)
                throw new BotScriptErrorException($"Type {type.FullName} has no public static methods with parameter and return types that can be used in scripts", wildcardToken);

            return [.. functions.Select(x => (x.Name, (IIdentifiable)x))];
        }

        private List<DotNetFunction> GetStaticFunctions(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(x => !x.IsSpecialName)
                .GroupBy(x => x.Name)
                .Select(x => (x.Key, Overloads: x.Where(_host.Converter.IsSupported).ToList()))
                .Where(x => x.Overloads.Count > 0)
                .Select(x => new DotNetFunction(x.Key, x.Overloads, _host.Converter))
                .ToList();
        }

        private DotNetFunction GetMappedFunction(Type type, BotToken methodToken, BotToken aliasToken)
        {
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
