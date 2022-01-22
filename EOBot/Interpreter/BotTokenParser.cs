using System;
using System.Collections.Generic;
using System.IO;

namespace EOBot.Interpreter
{
    public sealed class BotTokenParser : IDisposable
    {
        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "if",
            "while",
            "goto"
        };

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
                return new BotToken(BotTokenType.EOF, string.Empty);

            char inputChar;
            do
            {
                inputChar = Read();

                if (inputChar == '\n')
                {
                    LineNumber++;
                    Column = 1;
                    return new BotToken(BotTokenType.NewLine, inputChar.ToString());
                }

                if (inputChar == '/' && !_inputStream.EndOfStream && (char)_inputStream.Peek() == '*')
                {
                    // skip the comment: block format
                    do
                    {
                        inputChar = Read();
                    } while (!(inputChar == '*' && !_inputStream.EndOfStream && _inputStream.Peek() == '/'));

                    // skip the slash ending the comment and set input char to the character after the comment
                    Read();
                    inputChar = Read();
                }
                else if (inputChar == '/' && !_inputStream.EndOfStream && (char)_inputStream.Peek() == '/')
                {
                    // skip the comment: line format
                    do
                    {
                        inputChar = Read();
                    } while (inputChar != '\n');

                    LineNumber++;
                    Column = 1;
                    return new BotToken(BotTokenType.NewLine, inputChar.ToString());
                }
            } while (!_inputStream.EndOfStream && char.IsWhiteSpace(inputChar));

            if (_inputStream.EndOfStream)
                return new BotToken(BotTokenType.EOF, string.Empty);

            if (char.IsLetter(inputChar))
            {
                var identifier = inputChar.ToString();
                while (char.IsLetterOrDigit((char)_inputStream.Peek()))
                    identifier += Read();

                var type = Keywords.Contains(identifier)
                        ? BotTokenType.Keyword
                        : BotTokenType.Label;

                if (type == BotTokenType.Label && (char)_inputStream.Peek() == ':')
                {
                    Read();
                }

                return new BotToken(type, identifier);
            }
            else if (char.IsDigit(inputChar))
            {
                var number = inputChar.ToString();
                while (char.IsDigit((char)_inputStream.Peek()))
                    number += Read();
                return new BotToken(BotTokenType.Literal, number);
            }
            else
            {
                switch(inputChar)
                {
                    case '(': return new BotToken(BotTokenType.LParen, inputChar.ToString());
                    case ')': return new BotToken(BotTokenType.RParen, inputChar.ToString());
                    case '{': return new BotToken(BotTokenType.LBrace, inputChar.ToString());
                    case '}': return new BotToken(BotTokenType.RBrace, inputChar.ToString());
                    case '[': return new BotToken(BotTokenType.LBracket, inputChar.ToString());
                    case ']': return new BotToken(BotTokenType.RBracket, inputChar.ToString());
                    case '"':
                        {
                            var stringLiteral = string.Empty;
                            while ((char)_inputStream.Peek() != '"')
                                stringLiteral += Read();
                            Read();
                            return new BotToken(BotTokenType.Literal, stringLiteral);
                        }
                    case '=':
                        {
                            switch((char)_inputStream.Peek())
                            {
                                case '=':
                                    var nextChar = Read();
                                    return new BotToken(BotTokenType.EqualOperator, inputChar.ToString() + nextChar);
                                default:
                                    return new BotToken(BotTokenType.AssignOperator, inputChar.ToString());
                            }
                        }
                    case '!':
                        {
                            var nextChar = Read();
                            if (nextChar != '=')
                                return new BotToken(BotTokenType.Error, inputChar.ToString() + nextChar);
                            return new BotToken(BotTokenType.NotEqualOperator, inputChar.ToString() + nextChar);
                        }
                    case '>':
                        {
                            switch ((char)_inputStream.Peek())
                            {
                                case '=':
                                    var nextChar = Read();
                                    return new BotToken(BotTokenType.GreaterThanEqOperator, inputChar.ToString() + nextChar);
                                default:
                                    return new BotToken(BotTokenType.GreaterThanOperator, inputChar.ToString());
                            }
                        }
                    case '<':
                        {
                            switch ((char)_inputStream.Peek())
                            {
                                case '=':
                                    var nextChar = Read();
                                    return new BotToken(BotTokenType.LessThanEqOperator, inputChar.ToString() + nextChar);
                                default:
                                    return new BotToken(BotTokenType.LessThanOperator, inputChar.ToString());
                            }
                        }
                    case '$':
                        {
                            if (_inputStream.EndOfStream)
                                return new BotToken(BotTokenType.Error, inputChar.ToString());

                            // ensure identifier starts with letter or underscore before getting identifier name
                            inputChar = (char)_inputStream.Peek();
                            if (!char.IsLetter(inputChar) && inputChar != '_')
                                return new BotToken(BotTokenType.Error, inputChar.ToString());

                            var identifier = string.Empty;
                            do
                            {
                                inputChar = (char)Read();
                                identifier += inputChar;
                            } while (!_inputStream.EndOfStream && (char.IsLetterOrDigit(inputChar) || inputChar == '_'));

                            return new BotToken(BotTokenType.Identifier, identifier);
                        }
                    default: return new BotToken(BotTokenType.Error, inputChar.ToString());
                }
            }
        }

        public void Dispose()
        {
            if (_streamNeedsDispose)
                _inputStream.Dispose();

            GC.SuppressFinalize(this);
        }

        private char Read()
        {
            Column++;
            return (char)_inputStream.Read();
        }
    }
}
