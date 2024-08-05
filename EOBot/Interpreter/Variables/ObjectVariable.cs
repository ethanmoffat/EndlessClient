using System;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.Variables
{
    public class ObjectVariable : IVariable<object>
    {
        public object Value => SymbolTable;

        public Dictionary<string, (bool ReadOnly, IIdentifiable Variable)> SymbolTable { get; }

        public ObjectVariable() => SymbolTable = new Dictionary<string, (bool, IIdentifiable)>();

        public ObjectVariable(Dictionary<string, (bool, IIdentifiable)> symbolTable) => SymbolTable = symbolTable;

        public string StringValue => $"Object: [{string.Join(", ", SymbolTable.Select(x => $"({x.Key}, {x.Value.Variable})"))}]";

        public override bool Equals(object obj) => CompareTo(obj) == 0;

        public override int GetHashCode() => Value.GetHashCode();

        public int CompareTo(object obj) => obj is ObjectVariable ? SymbolTable.Equals(((ObjectVariable)obj).SymbolTable) ? 0 : -1 : -1;

        public override string ToString() => StringValue;
    }

    public class RuntimeEvaluatedMemberObjectVariable : IVariable<object>
    {
        public object Value => SymbolTable;

        public Dictionary<string, (bool ReadOnly, Func<IIdentifiable> Variable)> SymbolTable { get; }

        public RuntimeEvaluatedMemberObjectVariable() => SymbolTable = new Dictionary<string, (bool, Func<IIdentifiable>)>();

        public string StringValue => $"Object: [{string.Join(", ", SymbolTable.Select(x => $"({x.Key}, {x.Value.Variable()})"))}]";

        public override bool Equals(object obj) => CompareTo(obj) == 0;

        public override int GetHashCode() => Value.GetHashCode();

        public int CompareTo(object obj) => obj is ObjectVariable ? SymbolTable.Equals(((ObjectVariable)obj).SymbolTable) ? 0 : -1 : -1;

        public override string ToString() => StringValue;
    }
}