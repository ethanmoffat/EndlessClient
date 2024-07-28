using EOBot.Interpreter;
using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        public override async Task InitializeAsync(string host, int port)
        {
            var tokens = _interpreter.Parse();
            _programState = _interpreter.Prepare(_index, _parsedArgs, tokens);

            if (_parsedArgs.AutoConnect)
            {
                var connectFunction = _programState.SymbolTable[PredefinedIdentifiers.CONNECT_FUNC].Identifiable as AsyncVoidFunction<string, int>;
                if (connectFunction == null)
                    throw new InvalidOperationException("Something went wrong getting the connect function out of the symbol table");

                // call connect function that uses user-defined $version variable instead of base logic that has it hard-coded
                await connectFunction.CallAsync(new StringVariable(_parsedArgs.Host), new IntVariable(_parsedArgs.Port));

                WorkCompleted += () =>
                {
                    Thread.Sleep(2000);

                    var disconnectionFunction = _programState.SymbolTable[PredefinedIdentifiers.DISCONNECT_FUNC].Identifiable as VoidFunction;
                    disconnectionFunction.Call();
                };
            }

            _initialized = true;
        }

        protected override async Task DoWorkAsync(CancellationToken ct)
        {
            if (_programState == null)
                throw new InvalidOperationException("Scripted bot must be initialized before it is run");

            await _interpreter.Run(_programState);
        }
    }
}