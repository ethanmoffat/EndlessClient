using System;
using System.Collections.Generic;
using System.Linq;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class ProgramState
    {
        public Stack<BotToken> OperationStack { get; } = [];

        public Stack<(string, BotToken, int)> CallStack { get; private set; } = [];

        public IReadOnlyList<BotToken> Program { get; }

        public Dictionary<string, (bool ReadOnly, IIdentifiable Identifiable)> SymbolTable { get; private set; }

        public Dictionary<string, int> Labels { get; }

        public int ExecutionIndex { get; private set; }

        public ProgramState(List<BotToken> program, bool isFunctionBody = false)
        {
            Program = program;

            SymbolTable = GetDeclaredSymbols(program, isFunctionBody);
            Labels = GetLabels(Program);

            OperationStack = [];
            CallStack = [];
        }

        public void SkipToken()
        {
            ExecutionIndex++;
        }

        public bool Goto(int executionIndex)
        {
            if (executionIndex >= Program.Count)
                return false;

            ExecutionIndex = executionIndex;
            return true;
        }

        /// <summary>
        /// Check for a token at the program's execution index. If it is the expected type, increment execution index.
        /// </summary>
        public bool Expect(BotTokenType tokenType)
        {
            if (ExecutionIndex >= Program.Count)
                return tokenType == BotTokenType.EOF;

            if (Program[ExecutionIndex].TokenType == tokenType)
            {
                ExecutionIndex++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check for a token at the program's execution index. If it is the expected type, push it onto the operation stack and increment execution index.
        /// </summary>
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

        public bool ExpectPair(BotTokenType first, BotTokenType second)
        {
            if (ExecutionIndex >= Program.Count - 1)
                return false;

            if (Program[ExecutionIndex].TokenType == first &&
                Program[ExecutionIndex + 1].TokenType == second)
            {
                ExecutionIndex += 2;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Matches a pair of tokens in order at the program's execution index.
        /// </summary>
        public bool MatchPair(BotTokenType first, BotTokenType second)
        {
            if (ExecutionIndex >= Program.Count - 1)
                return false;

            if (Program[ExecutionIndex].TokenType == first &&
                Program[ExecutionIndex + 1].TokenType == second)
            {
                OperationStack.Push(Program[ExecutionIndex]);
                OperationStack.Push(Program[ExecutionIndex + 1]);
                ExecutionIndex += 2;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Inherits the state from the parent programState. This includes any read-only variables and call stack.
        ///
        /// This is a destructive operation that clears the symbol table and execution stacks.
        /// </summary>
        /// <param name="parentState">The parent program.</param>
        public void InheritFrom(ProgramState parentState)
        {
            ExecutionIndex = 0;
            OperationStack.Clear();
            SymbolTable = parentState.SymbolTable;
            CallStack = parentState.CallStack;
        }

        private static Dictionary<string, int> GetLabels(IReadOnlyList<BotToken> program)
        {
            return program
                .Select((token, ndx) => (token, ndx))
                .Where(x => x.token.TokenType == BotTokenType.Identifier && program[x.ndx + 1].TokenType == BotTokenType.Colon)
                .ToDictionary(x => x.token.TokenValue, y => y.ndx + 2);
        }

        // m o m ' s   s p a g h e t t i
        private static Dictionary<string, (bool, IIdentifiable)> GetDeclaredSymbols(List<BotToken> program, bool isFunctionBody)
        {
            var retDict = new Dictionary<string, (bool, IIdentifiable)>();

            for (int i = 0; i < program.Count; i++)
            {
                if (program[i].Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_FUNC))
                {
                    GetFunction(retDict, program, ref i);
                }
                // 'enum' following 'is' is a type specifier, not a declaration
                else if ((program[i].Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_ENUM) && (i == 0 || !program[i - 1].Is(BotTokenType.IsOperator)))
                    || program[i].Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_MAP)
                    || program[i].Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_FROM))
                {
                    if (isFunctionBody)
                        throw new BotScriptErrorException($"'{program[i].TokenValue}' declarations are only allowed at the top level of a script", program[i]);

                    if (program[i].Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_ENUM))
                        GetEnum(retDict, program, ref i);
                    else
                        GetEnumMapping(retDict, program, ref i);
                }
            }

            return retDict;
        }

        private static void GetFunction(Dictionary<string, (bool, IIdentifiable)> retDict, List<BotToken> program, ref int i)
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
            if (!(errorToken = funcToken).Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_FUNC) ||
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

            retDict[funcName.TokenValue] = (true, new UserDefinedFunction(funcName.TokenValue, funcRange, paramSpecs));

            program.RemoveRange(funcStartIndex, i - funcStartIndex);
            i = funcStartIndex - 1;
        }

        private static void GetEnum(Dictionary<string, (bool, IIdentifiable)> retDict, List<BotToken> program, ref int i)
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

            retDict[enumName.TokenValue] = (true, new EnumDefinition(enumName.TokenValue, members));

            program.RemoveRange(enumStartIndex, i - enumStartIndex);
            i = enumStartIndex - 1;
        }

        /// <summary>
        /// Parses: [from Namespace.Path] map (TypeName | *) [to Alias]
        /// </summary>
        private static void GetEnumMapping(Dictionary<string, (bool, IIdentifiable)> retDict, List<BotToken> program, ref int i)
        {
            int mapStartIndex = i;

            string ns = null;
            if (program[i].Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_FROM))
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
            if (!mapToken.Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_MAP))
                throw new BotScriptErrorException("Expected 'map' keyword", mapToken);

            var typeToken = NextToken(program, ref i, mapToken);
            var isWildcard = typeToken.Is(BotTokenType.MultiplyOperator);
            if (!isWildcard && !typeToken.Is(BotTokenType.Identifier))
                throw new BotScriptErrorException("Expected type name or '*' after 'map'", typeToken);

            BotToken aliasToken = null;
            if (program[i].Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_TO))
            {
                var toToken = program[i++];
                aliasToken = NextToken(program, ref i, toToken);
                if (!aliasToken.Is(BotTokenType.Identifier))
                    throw new BotScriptErrorException("Expected identifier for alias after 'to'", aliasToken);
            }

            if (!program[i].IsOneOf(BotTokenType.NewLine, BotTokenType.EOF))
                throw new BotScriptErrorException("Unexpected token after map statement", program[i]);

            List<EnumDefinition> definitions;
            if (isWildcard)
            {
                if (ns == null)
                    throw new BotScriptErrorException("Mapping all types with '*' requires a namespace ('from <namespace> map *')", typeToken);
                if (aliasToken != null)
                    throw new BotScriptErrorException("An alias cannot be specified when mapping all types with '*'", aliasToken);

                var types = EnumTypeCatalog.FindByNamespace(ns);
                if (types.Count == 0)
                    throw new BotScriptErrorException($"No enum types found in namespace {ns}", typeToken);

                definitions = types.Select(x => EnumDefinition.FromType(x)).ToList();
            }
            else
            {
                var types = EnumTypeCatalog.FindByName(typeToken.TokenValue, ns);
                if (types.Count == 0)
                    throw new BotScriptErrorException($"No enum type {typeToken.TokenValue} found{(ns == null ? string.Empty : $" in namespace {ns}")}", typeToken);
                if (types.Count > 1)
                    throw new BotScriptErrorException($"Enum type {typeToken.TokenValue} is ambiguous between: {string.Join(", ", types.Select(x => x.FullName))}. Use 'from <namespace> map {typeToken.TokenValue}'", typeToken);

                definitions = [EnumDefinition.FromType(types[0], aliasToken?.TokenValue)];
            }

            foreach (var definition in definitions)
            {
                if (retDict.ContainsKey(definition.Name))
                    throw new BotScriptErrorException($"Symbol {definition.Name} is already defined", aliasToken ?? typeToken);

                retDict[definition.Name] = (true, definition);
            }

            program.RemoveRange(mapStartIndex, i - mapStartIndex);
            i = mapStartIndex - 1;
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
