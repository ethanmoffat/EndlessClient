using System.Collections.Generic;

namespace EOBot.Interpreter
{
    public static class Keywords
    {
        public const string If = "if";
        public const string While = "while";
        public const string Goto = "goto";
        public const string Else = "else";
        public const string For = "for";
        public const string Foreach = "foreach";
        public const string In = "in";
        public const string Continue = "continue";
        public const string Break = "break";

        public const string Func = "func";
        public const string Return = "return";

        public const string Enum = "enum";
        public const string From = "from";
        public const string Map = "map";
        public const string To = "to";

        public const string Is = "is";

        public const string Undefined = "undefined";
        public const string True = "true";
        public const string False = "false";

        public const string Bool = "bool";
        public const string Int = "int";
        public const string String = "string";
        public const string Object = "Object";
        public const string Array = "Array";
        public const string Dict = "Dict";

        // contextual: only a keyword between 'map' and a class name, otherwise the object() function
        public const string MapObject = "object";

        private static readonly Dictionary<string, BotTokenType> TokenTypes = new()
        {
            [If] = BotTokenType.Keyword,
            [While] = BotTokenType.Keyword,
            [Goto] = BotTokenType.Keyword,
            [Else] = BotTokenType.Keyword,
            [For] = BotTokenType.Keyword,
            [Foreach] = BotTokenType.Keyword,
            [In] = BotTokenType.Keyword,
            [Continue] = BotTokenType.Keyword,
            [Break] = BotTokenType.Keyword,
            [Func] = BotTokenType.Keyword,
            [Return] = BotTokenType.Keyword,
            [Enum] = BotTokenType.Keyword,
            [From] = BotTokenType.Keyword,
            [Map] = BotTokenType.Keyword,
            [To] = BotTokenType.Keyword,

            [Is] = BotTokenType.IsOperator,

            [Undefined] = BotTokenType.Literal,
            [True] = BotTokenType.Literal,
            [False] = BotTokenType.Literal,

            [Bool] = BotTokenType.TypeSpecifier,
            [Int] = BotTokenType.TypeSpecifier,
            [String] = BotTokenType.TypeSpecifier,
            [Object] = BotTokenType.TypeSpecifier,
            [Array] = BotTokenType.TypeSpecifier,
            [Dict] = BotTokenType.TypeSpecifier,
        };

        public static BotTokenType GetTokenType(string identifier) => TokenTypes.GetValueOrDefault(identifier, BotTokenType.Identifier);
    }
}
