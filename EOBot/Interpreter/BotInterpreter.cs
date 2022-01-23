using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EOBot.Interpreter
{
    public class BotInterpreter
    {
        private readonly BotTokenParser _parser;

        public BotInterpreter(string filePath)
            : this(File.OpenText(filePath))
        {
        }

        public BotInterpreter(StreamReader inputStream)
        {
            _parser = new BotTokenParser(inputStream);
        }

        public IReadOnlyList<BotToken> Parse()
        {
            _parser.Reset();

            var retList = new List<BotToken>();

            BotToken nextToken;
            do
            {
                nextToken = _parser.GetNextToken();
                if (nextToken.TokenType == BotTokenType.Error)
                {
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, $"Error at line {_parser.LineNumber} column {_parser.Column}: token value {nextToken.TokenValue}", ConsoleColor.Red);
                    throw new InvalidOperationException("Unable to parse input");
                }

                retList.Add(nextToken);
            } while (nextToken.TokenType != BotTokenType.EOF);

            return retList;
        }

        public async Task Run(IReadOnlyList<BotToken> tokens)
        {
        }
    }
}
