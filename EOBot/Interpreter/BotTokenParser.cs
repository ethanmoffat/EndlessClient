using System;
using System.Collections.Generic;
using System.IO;

namespace EOBot.Interpreter
{
    public sealed class BotTokenParser : IDisposable
    {
        public const string KEYWORD_IF = "if";
        public const string KEYWORD_WHILE = "while";
        public const string KEYWORD_GOTO = "goto";
        public const string KEYWORD_ELSE = "else";
        public const string KEYWORD_FOR = "for";

        public const string KEYWORD_TRUE = "true";
        public const string KEYWORD_FALSE = "false";

        private static readonly HashSet<string> Keywords =
        [
            KEYWORD_IF,
            KEYWORD_WHILE,
            KEYWORD_GOTO,
            KEYWORD_ELSE,
            KEYWORD_FOR,
        ];

        private static readonly HashSet<string> Literals = [KEYWORD_TRUE, KEYWORD_FALSE];

        private readonly StreamReader _inputStream;
        private readonly bool _streamNeedsDispose;

        public int LineNumber { get; private set; }

        public int Column { get; private set; }

        public BotTokenParser(string filePath)
            : this(File.OpenText(filePath))
        {
            _streamNeedsDispose = true;
        }

        public BotTokenParser(StreamReader inputStream)
        {
            _inputStream = inputStream;
            LineNumber = 1;
            Column = 1;
        }

        public void Reset()
        {
            _inputStream.BaseStream.Seek(0, SeekOrigin.Begin);
            LineNumber = 1;
            Column = 1;
        }

        public BotToken GetNextToken()
        {
            if (_inputStream.EndOfStream)
                return Token(BotTokenType.EOF, string.Empty);

            char inputChar;
            do
            {
                inputChar = Read();

                if (inputChar == '\n')
                {
                    LineNumber++;
                    Column = 1;
                    return Token(BotTokenType.NewLine, inputChar.ToString());
                }

                if (inputChar == '/' && !_inputStream.EndOfStream && Peek() == '*')
                {
                    // skip the comment: block format
                    do
                    {
                        inputChar = Read();
                    } while (!(inputChar == '*' && !_inputStream.EndOfStream && Peek() == '/'));

                    // skip the slash ending the comment and set input char to the character after the comment
                    Read();
                    inputChar = Read();
                }
                else if (inputChar == '/' && !_inputStream.EndOfStream && Peek() == '/')
                {
                    // skip the comment: line format
                    do
                    {
                        inputChar = Read();
                    } while (inputChar != '\n' && !_inputStream.EndOfStream);

                    LineNumber++;
                    Column = 1;
                    return Token(BotTokenType.NewLine, inputChar.ToString());
                }
            } while (!_inputStream.EndOfStream && char.IsWhiteSpace(inputChar));

            if (char.IsLetter(inputChar))
            {
                var identifier = inputChar.ToString();
                while ((char.IsLetterOrDigit(Peek()) || Peek() == '_') && !_inputStream.EndOfStream)
                    identifier += Read();

                var type = Keywords.Contains(identifier)
                        ? BotTokenType.Keyword
                        : Literals.Contains(identifier)
                            ? BotTokenType.Literal
                            : BotTokenType.Identifier;

                return Token(type, identifier);
            }
            else if (char.IsDigit(inputChar))
            {
                var number = inputChar.ToString();
                while (char.IsDigit(Peek()) && !_inputStream.EndOfStream)
                    number += Read();
                return Token(BotTokenType.Literal, number);
            }
            else
            {
                switch (inputChar)
                {
                    case '(': return Token(BotTokenType.LParen, inputChar.ToString());
                    case ')': return Token(BotTokenType.RParen, inputChar.ToString());
                    case '{': return Token(BotTokenType.LBrace, inputChar.ToString());
                    case '}': return Token(BotTokenType.RBrace, inputChar.ToString());
                    case '[': return Token(BotTokenType.LBracket, inputChar.ToString());
                    case ']': return Token(BotTokenType.RBracket, inputChar.ToString());
                    case ':': return Token(BotTokenType.Colon, inputChar.ToString());
                    case ',': return Token(BotTokenType.Comma, inputChar.ToString());
                    case '"':
                        {
                            var stringLiteral = string.Empty;
                            while (Peek() != '"' && !_inputStream.EndOfStream)
                                stringLiteral += Read();

                            if (_inputStream.EndOfStream)
                                return Token(BotTokenType.Error, string.Empty);

                            Read();
                            return Token(BotTokenType.Literal, stringLiteral);
                        }
                    case '=':
                        {
                            switch (Peek())
                            {
                                case '=':
                                    var nextChar = Read();
                                    return Token(BotTokenType.EqualOperator, inputChar.ToString() + nextChar);
                                default:
                                    return Token(BotTokenType.AssignOperator, inputChar.ToString());
                            }
                        }
                    case '!':
                        {
                            switch (Peek())
                            {
                                case '=':
                                    var nextChar = Read();
                                    return Token(BotTokenType.NotEqualOperator, inputChar.ToString() + nextChar);
                                default:
                                    return Token(BotTokenType.NotOperator, inputChar.ToString());
                            }
                        }
                    case '>':
                        {
                            switch (Peek())
                            {
                                case '=':
                                    var nextChar = Read();
                                    return Token(BotTokenType.GreaterThanEqOperator, inputChar.ToString() + nextChar);
                                default:
                                    return Token(BotTokenType.GreaterThanOperator, inputChar.ToString());
                            }
                        }
                    case '<':
                        {
                            switch (Peek())
                            {
                                case '=':
                                    var nextChar = Read();
                                    return Token(BotTokenType.LessThanEqOperator, inputChar.ToString() + nextChar);
                                default:
                                    return Token(BotTokenType.LessThanOperator, inputChar.ToString());
                            }
                        }
                    case '$':
                        {
                            if (_inputStream.EndOfStream)
                                return Token(BotTokenType.Error, inputChar.ToString());

                            // ensure variable starts with letter or underscore before getting variable name
                            inputChar = Peek();
                            if (!char.IsLetter(inputChar) && inputChar != '_')
                                return Token(BotTokenType.Error, inputChar.ToString());

                            var variable = string.Empty;
                            for (inputChar = Peek(); !_inputStream.EndOfStream && (char.IsLetterOrDigit(inputChar) || inputChar == '_'); inputChar = Peek())
                            {
                                variable += Read();
                            }

                            return Token(BotTokenType.Variable, variable);
                        }
                    case '|':
                        {
                            if (_inputStream.EndOfStream)
                                return Token(BotTokenType.Error, inputChar.ToString());

                            switch (Peek())
                            {
                                case '|':
                                    var nextChar = Read();
                                    return Token(BotTokenType.LogicalOrOperator, inputChar.ToString() + nextChar);
                                default:
                                    return Token(BotTokenType.Error, inputChar.ToString());
                            }
                        }
                    case '&':
                        {
                            if (_inputStream.EndOfStream)
                                return Token(BotTokenType.Error, inputChar.ToString());

                            switch (Peek())
                            {
                                case '&':
                                    var nextChar = Read();
                                    return Token(BotTokenType.LogicalAndOperator, inputChar.ToString() + nextChar);
                                default:
                                    return Token(BotTokenType.Error, inputChar.ToString());
                            }
                        }
                    case '+': return Token(BotTokenType.PlusOperator, inputChar.ToString());
                    case '-': return Token(BotTokenType.MinusOperator, inputChar.ToString());
                    case '*': return Token(BotTokenType.MultiplyOperator, inputChar.ToString());
                    case '/': return Token(BotTokenType.DivideOperator, inputChar.ToString());
                    case '%': return Token(BotTokenType.ModuloOperator, inputChar.ToString());
                    case '.': return Token(BotTokenType.Dot, inputChar.ToString());
                    case ';': return Token(BotTokenType.Semicolon, inputChar.ToString());
                    default: return Token(BotTokenType.Error, inputChar.ToString());
                }
            }
        }

        private BotToken Token(BotTokenType tokenType, string tokenValue)
        {
            return new BotToken(tokenType, tokenValue, LineNumber, Column);
        }

        public void Dispose()
        {
            if (_streamNeedsDispose)
                _inputStream.Dispose();

            GC.SuppressFinalize(this);
        }

        private char Peek()
        {
            return (char)_inputStream.Peek();
        }

        private char Read()
        {
            Column++;
            return (char)_inputStream.Read();
        }
    }
}
