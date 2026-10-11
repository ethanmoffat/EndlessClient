using System;
using System.IO;

namespace EOBot.Interpreter
{
    public sealed class BotTokenizer : IDisposable
    {
        private readonly StreamReader _inputStream;
        private readonly bool _streamNeedsDispose;

        public int LineNumber { get; private set; }

        public int Column { get; private set; }

        public BotTokenizer(string filePath)
            : this(File.OpenText(filePath))
        {
            _streamNeedsDispose = true;
        }

        public BotTokenizer(StreamReader inputStream)
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
            char inputChar;
            bool isComment;
            do
            {
                if (_inputStream.EndOfStream)
                    return Token(BotTokenType.EOF, string.Empty);

                inputChar = Read();

                if (inputChar == '\n')
                {
                    LineNumber++;
                    Column = 1;
                    return Token(BotTokenType.NewLine, inputChar.ToString());
                }

                isComment = inputChar == '/' && (Peek() == '*' || Peek() == '/');
                if (isComment && !SkipComment())
                    return Token(BotTokenType.Error, "/*");
            } while (isComment || char.IsWhiteSpace(inputChar));

            if (char.IsLetter(inputChar))
            {
                var identifier = inputChar.ToString();
                while ((char.IsLetterOrDigit(Peek()) || Peek() == '_') && !_inputStream.EndOfStream)
                    identifier += Read();

                var type = Keywords.GetTokenType(identifier);

                if (type == BotTokenType.Literal)
                {
                    return Literal(identifier, identifier == Keywords.Undefined ? null : bool.Parse(identifier));
                }
                else
                {
                    return Token(type, identifier);
                }
            }
            else if (char.IsDigit(inputChar))
            {
                var number = inputChar.ToString();
                while (char.IsDigit(Peek()) && !_inputStream.EndOfStream)
                    number += Read();
                return Literal(number, int.Parse(number));
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
                    case ':':
                        {
                            if (!_inputStream.EndOfStream && Peek() == ':')
                                return Token(BotTokenType.ScopeResolution, $"{inputChar}{Read()}");

                            return Token(BotTokenType.Colon, inputChar.ToString());
                        }
                    case '?': return Token(BotTokenType.QuestionMark, inputChar.ToString());
                    case ',': return Token(BotTokenType.Comma, inputChar.ToString());
                    case '"':
                        {
                            var stringLiteral = string.Empty;
                            while (Peek() != '"' && !_inputStream.EndOfStream)
                                stringLiteral += Read();

                            if (_inputStream.EndOfStream)
                                return Token(BotTokenType.Error, string.Empty);

                            Read();
                            return Literal(stringLiteral, stringLiteral);
                        }
                    case '=':
                        {
                            switch (Peek())
                            {
                                case '=':
                                    var nextChar = Read();
                                    switch (Peek())
                                    {
                                        case '=':
                                            var nextNextChar = Read();
                                            return Token(BotTokenType.StrictEqualOperator, inputChar.ToString() + nextChar + nextNextChar);
                                        default:
                                            return Token(BotTokenType.EqualOperator, inputChar.ToString() + nextChar);
                                    }
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
                                    switch (Peek())
                                    {
                                        case '=':
                                            var nextNextChar = Read();
                                            return Token(BotTokenType.StrictNotEqualOperator, inputChar.ToString() + nextChar + nextNextChar);
                                        default:
                                            return Token(BotTokenType.NotEqualOperator, inputChar.ToString() + nextChar);
                                    }
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
                    case '+':
                        if (_inputStream.EndOfStream)
                            return Token(BotTokenType.PlusOperator, inputChar.ToString());

                        return Peek() switch
                        {
                            '+' => Token(BotTokenType.Increment, $"{inputChar}{Read()}"),
                            '=' => Token(BotTokenType.PlusEquals, $"{inputChar}{Read()}"),
                            _ => Token(BotTokenType.PlusOperator, inputChar.ToString()),
                        };
                    case '-':
                        if (_inputStream.EndOfStream)
                            return Token(BotTokenType.MinusOperator, inputChar.ToString());

                        return Peek() switch
                        {
                            '-' => Token(BotTokenType.Decrement, $"{inputChar}{Read()}"),
                            '=' => Token(BotTokenType.MinusEquals, $"{inputChar}{Read()}"),
                            _ => Token(BotTokenType.MinusOperator, inputChar.ToString()),
                        };
                    case '*':
                        if (_inputStream.EndOfStream)
                            return Token(BotTokenType.MultiplyOperator, inputChar.ToString());

                        return Peek() switch
                        {
                            '=' => Token(BotTokenType.MultiplyEquals, $"{inputChar}{Read()}"),
                            _ => Token(BotTokenType.MultiplyOperator, inputChar.ToString()),
                        };
                    case '/':
                        if (_inputStream.EndOfStream)
                            return Token(BotTokenType.DivideOperator, inputChar.ToString());

                        return Peek() switch
                        {
                            '=' => Token(BotTokenType.DivideEquals, $"{inputChar}{Read()}"),
                            _ => Token(BotTokenType.DivideOperator, inputChar.ToString()),
                        };
                    case '%':
                        if (_inputStream.EndOfStream)
                            return Token(BotTokenType.ModuloOperator, inputChar.ToString());

                        return Peek() switch
                        {
                            '=' => Token(BotTokenType.ModuloEquals, $"{inputChar}{Read()}"),
                            _ => Token(BotTokenType.ModuloOperator, inputChar.ToString()),
                        };
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

        private LiteralBotToken Literal(string tokenValue, object literalValue)
        {
            return new LiteralBotToken(BotTokenType.Literal, tokenValue, literalValue, LineNumber, Column);
        }

        public void Dispose()
        {
            if (_streamNeedsDispose)
                _inputStream.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Skips a comment whose leading '/' has already been read. Returns false if a block comment is unterminated.
        /// </summary>
        private bool SkipComment()
        {
            if (Read() == '/')
            {
                // leave the newline in the stream so it is returned as a NewLine token
                while (!_inputStream.EndOfStream && Peek() != '\n')
                    Read();
                return true;
            }

            char inputChar;
            do
            {
                if (_inputStream.EndOfStream)
                    return false;

                inputChar = Read();
                if (inputChar == '\n')
                {
                    LineNumber++;
                    Column = 1;
                }
            } while (!(inputChar == '*' && Peek() == '/'));

            Read();
            return true;
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
