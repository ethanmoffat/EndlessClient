using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.States;

namespace EOBot.Interpreter
{
    public class BotInterpreter
    {
        private readonly BotTokenParser _parser;
        private readonly IScriptEvaluator _scriptEvaluator;

        public BotInterpreter(string filePath)
            : this(File.OpenText(filePath))
        {
        }

        public BotInterpreter(StreamReader inputStream)
            : this()
        {
            _parser = new BotTokenParser(inputStream);
        }

        private BotInterpreter()
        {
            var evaluators = new List<IScriptEvaluator>();
            evaluators.Add(new StatementListEvaluator(evaluators));
            evaluators.Add(new StatementEvaluator(evaluators));
            evaluators.Add(new AssignmentEvaluator(evaluators));
            evaluators.Add(new KeywordEvaluator(evaluators));
            evaluators.Add(new LabelEvaluator());
            evaluators.Add(new FunctionEvaluator(evaluators));
            evaluators.Add(new VariableEvaluator(evaluators));
            evaluators.Add(new ExpressionEvaluator(evaluators));
            evaluators.Add(new ExpressionTailEvaluator(evaluators));
            evaluators.Add(new OperandEvaluator(evaluators));
            evaluators.Add(new IfEvaluator(evaluators));
            evaluators.Add(new WhileEvaluator(evaluators));
            evaluators.Add(new GotoEvaluator());
            _scriptEvaluator = new ScriptEvaluator(evaluators);
        }

        public ProgramState Parse()
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

            return new ProgramState(retList);
        }

        public async Task Run(ProgramState programState, CancellationToken ct)
        {
            var (result, reason, token) = await _scriptEvaluator.EvaluateAsync(programState, ct).ConfigureAwait(false);

            if (result == EvalResult.Failed)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, $"Error during execution at line {token.LineNumber} column {token.Column}", ConsoleColor.DarkRed);
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, reason, ConsoleColor.DarkRed);
            }
            else if (result == EvalResult.NotMatch)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, $"Error at line {token.LineNumber} column {token.Column}: {token.TokenType} {token.TokenValue} was unexpected", ConsoleColor.DarkRed);
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, reason, ConsoleColor.DarkRed);
                }
            }
            else if (result == EvalResult.Cancelled)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, "Execution was cancelled");
            }
        }
    }
}
