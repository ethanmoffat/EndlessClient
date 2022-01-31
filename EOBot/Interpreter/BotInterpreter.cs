using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;
using System;
using System.Collections.Generic;
using System.IO;

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

        public void Run(ArgumentsParser parsedArgs, IReadOnlyList<BotToken> tokens)
        {
            var setup = new BuiltInIdentifierConfigurator();

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

            var scriptEvaluator = new ScriptEvaluator(evaluators);

            ProgramState input = new ProgramState(tokens);
            setup.SetupBuiltInFunctions(input);
            setup.SetupBuiltInVariables(input, parsedArgs);

            if (!scriptEvaluator.Evaluate(input))
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, "Error during execution! //todo: better error handling", ConsoleColor.Red);
            }
        }
    }
}
