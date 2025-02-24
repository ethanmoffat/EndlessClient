using System;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter;
using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;

namespace EOBot
{
    public class ScriptedBot : BotBase
    {
        private readonly BotInterpreter _interpreter;
        private readonly ArgumentsParser _parsedArgs;
        private ProgramState _programState;

        public ScriptedBot(int index, ArgumentsParser parsedArgs)
            : base(index)
        {
            _interpreter = new BotInterpreter(parsedArgs.ScriptFile);
            _parsedArgs = parsedArgs;
        }

        public override async Task InitializeAsync(string host, int port, CancellationToken cancellationToken)
        {
            _programState = _interpreter.Parse();

            var configurator = new BuiltInIdentifierConfigurator(_index, _parsedArgs);
            configurator.SetupBuiltInFunctions(_programState);
            configurator.SetupBuiltInVariables(_programState);

            if (_parsedArgs.AutoConnect)
            {
                if (_programState.SymbolTable[PredefinedIdentifiers.CONNECT_FUNC].Identifiable is not AsyncVoidFunction<string, int> connectFunction)
                    throw new InvalidOperationException("Something went wrong getting the connect function out of the symbol table");

                // call connect function that uses user-defined $version variable instead of base logic that has it hard-coded
                await connectFunction.CallAsync(cancellationToken, new StringVariable(_parsedArgs.Host), new IntVariable(_parsedArgs.Port)).ConfigureAwait(false);

                WorkCompleted += () =>
                {
                    Thread.Sleep(2000);

                    if (_programState.SymbolTable[PredefinedIdentifiers.DISCONNECT_FUNC].Identifiable is not VoidFunction disconnectionFunction)
                        throw new InvalidOperationException("Something went wrong getting the disconnect function out of the symbol table");

                    disconnectionFunction.Call();
                };
            }

            _initialized = true;
        }

        protected override async Task DoWorkAsync(CancellationToken ct)
        {
            if (_programState == null)
                throw new InvalidOperationException("Scripted bot must be initialized before it is run");

            await _interpreter.Run(_programState, ct).ConfigureAwait(false);
        }
    }
}
