using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Hosting;
using EOBot.Interpreter.States;

namespace EOBot.Interpreter
{
    public class BotInterpreter
    {
        private readonly BotTokenizer _tokenizer;
        private readonly ProgramParser _parser;

        public BotInterpreter(string filePath, ScriptHost host = null)
            : this(File.OpenText(filePath), host)
        {
        }

        public BotInterpreter(StreamReader inputStream, ScriptHost host = null)
        {
            _tokenizer = new BotTokenizer(inputStream);
            _parser = new ProgramParser(host);
        }

        public ProgramState Parse()
        {
            _tokenizer.Reset();

            var retList = new List<BotToken>();

            BotToken nextToken;
            do
            {
                nextToken = _tokenizer.GetNextToken();
                if (nextToken.TokenType == BotTokenType.Error)
                {
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, $"Error at line {_tokenizer.LineNumber} column {_tokenizer.Column}: token value {nextToken.TokenValue}", ConsoleColor.Red);
                    throw new InvalidOperationException("Unable to parse input");
                }

                retList.Add(nextToken);
            } while (nextToken.TokenType != BotTokenType.EOF);

            return new ProgramState(_parser.Parse(retList));
        }

        public async Task<int> Run(ProgramState programState, CancellationToken ct)
        {
            var (result, reason, token) = await ScriptEvaluator.Instance.EvaluateAsync(programState, ct).ConfigureAwait(false);

            if (result == EvalResult.Failed)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, $"Error during execution at line {token.LineNumber} column {token.Column}", ConsoleColor.DarkRed);
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, reason, ConsoleColor.DarkRed);

                ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, string.Empty);
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, "Operation stack:");
                foreach (var item in programState.OperationStack)
                {
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, $"{item} ({item.LineNumber}:{item.Column})");
                }

                return 3;
            }
            else if (result == EvalResult.NotMatch)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, $"Error at line {token.LineNumber} column {token.Column}: {token.TokenType} {token.TokenValue} was unexpected", ConsoleColor.DarkRed);
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, reason, ConsoleColor.DarkRed);
                }

                return 2;
            }
            else if (result == EvalResult.Cancelled)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, "Execution was cancelled");
                return 130;
            }

            return 0;
        }
    }
}
