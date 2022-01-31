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

        public override Task InitializeAsync(string host, int port)
        {
            var tokens = _interpreter.Parse();
            _programState = _interpreter.Prepare(_index, _parsedArgs, tokens);

            var connectFunction = _programState.SymbolTable[PredefinedIdentifiers.CONNECT_FUNC].Identifiable as VoidFunctionRef<string, int>;
            if (connectFunction == null)
                throw new InvalidOperationException("Something went wrong getting the connect function out of the symbol table");

            // call connect function that uses user-defined $version variable instead of base logic that has it hard-coded
            connectFunction.Call(new StringVariable(_parsedArgs.Host), new IntVariable(_parsedArgs.Port));
            _initialized = true;
            return Task.CompletedTask;
        }

        protected override async Task DoWorkAsync(CancellationToken ct)
        {
            await Task.Run(() =>
            {
                if (_programState == null)
                    throw new InvalidOperationException("Scripted bot must be initialized before it is run");

                // todo - async.... :(
                _interpreter.Run(_programState);
            });
        }
    }
}
