using System;
using System.Collections.Generic;
using System.Linq;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    /// <summary>
    /// Execution of a user-defined function body against its caller's symbol table and call stack.
    /// Disposing restores the caller's symbol table and pops the call stack frame.
    /// </summary>
    public sealed class FunctionScope : IDisposable
    {
        private readonly Dictionary<string, (bool ReadOnly, IIdentifiable Identifiable)> _originalSymbols;
        private readonly HashSet<string> _readOnlySymbols;
        private readonly HashSet<string> _localSymbols;
        private bool _disposed;

        public ProgramState State { get; }

        public FunctionScope(ProgramDefinition definition, ProgramState parentState, string functionName, BotToken callSite)
        {
            State = new ProgramState(definition, parentState.SymbolTable, parentState.CallStack);

            _originalSymbols = new(State.SymbolTable);
            _localSymbols = [.. definition.DeclaredSymbols.Keys];
            foreach (var declared in definition.DeclaredSymbols)
                State.SymbolTable[declared.Key] = declared.Value;

            _readOnlySymbols = [.. State.SymbolTable.Where(x => x.Value.ReadOnly).Select(x => x.Key)];

            State.CallStack.Push((functionName, callSite, parentState.ExecutionIndex));
        }

        /// <summary>
        /// Binds input parameters of a function call as symbols local to this scope.
        /// </summary>
        public (EvalResult Result, string Reason, BotToken Token) BindParameters(IReadOnlyList<BotToken> paramSpecs, IReadOnlyList<IIdentifiable> paramValues)
        {
            for (int i = 0; i < paramSpecs.Count; i++)
            {
                var name = paramSpecs[i].TokenValue;
                if (_readOnlySymbols.Contains(name))
                    return (EvalResult.Failed, $"Parameter {name} overrides built-in variable or function.", paramSpecs[i]);

                State.SymbolTable[name] = (false, paramValues[i]);
                _localSymbols.Add(name);
            }

            return (EvalResult.Ok, string.Empty, null);
        }

        /// <summary>
        /// Symbols created during execution are removed and local symbols that shadowed existing ones get their original values back.
        /// Changes to existing symbols are kept.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            foreach (var key in State.SymbolTable.Keys.Where(x => !_originalSymbols.ContainsKey(x)).ToList())
                State.SymbolTable.Remove(key);

            foreach (var name in _localSymbols.Where(_originalSymbols.ContainsKey))
                State.SymbolTable[name] = _originalSymbols[name];

            State.CallStack.Pop();
        }
    }
}
