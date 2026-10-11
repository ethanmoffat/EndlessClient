using System;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.Variables
{
    public sealed class ObjectVariable : CollectionVariable, IVariable<object>
    {
        public object Value => SymbolTable;

        public Dictionary<string, (bool ReadOnly, IIdentifiable Variable)> SymbolTable { get; }

        public ObjectVariable() => SymbolTable = new Dictionary<string, (bool, IIdentifiable)>();

        public ObjectVariable(Dictionary<string, (bool, IIdentifiable)> symbolTable) => SymbolTable = symbolTable;

        public override string StringValue => $"Object: [{string.Join(", ", SymbolTable.Select(x => $"({x.Key}, {x.Value.Variable})"))}]";

        public override string TypeName => Keywords.Object;

        protected override object Contents => SymbolTable;

        public override bool Equals(object obj) => CompareTo(obj) == 0;

        public override int GetHashCode() => Value.GetHashCode();

        public override int CompareTo(object obj) => obj is ObjectVariable ? SymbolTable.Equals(((ObjectVariable)obj).SymbolTable) ? 0 : -1 : -1;

        protected override bool IsAlwaysUnequal(IVariable other) => other is UndefinedVariable or RuntimeEvaluatedMemberObjectVariable;

        protected override IReadOnlyDictionary<object, IIdentifiable> Members =>
            SymbolTable.ToDictionary(x => (object)x.Key, x => x.Value.Variable);

        public override string ToString() => StringValue;
    }

    /// <summary>
    /// An object whose members are evaluated on access (e.g. game state). Its members may change between comparisons, so equality is reference identity.
    /// </summary>
    public sealed class RuntimeEvaluatedMemberObjectVariable : IVariable<object>, ICollectionVariable
    {
        public object Value => SymbolTable;

        public Dictionary<string, (bool ReadOnly, Func<IIdentifiable> Variable)> SymbolTable { get; }

        public RuntimeEvaluatedMemberObjectVariable() => SymbolTable = new Dictionary<string, (bool, Func<IIdentifiable>)>();

        public string StringValue => $"Object: [{string.Join(", ", SymbolTable.Select(x => $"({x.Key}, {x.Value.Variable()})"))}]";

        public string TypeName => Keywords.Object;

        public override bool Equals(object obj) => CompareTo(obj) == 0;

        public override int GetHashCode() => Value.GetHashCode();

        public int CompareTo(object obj) => obj is ObjectVariable ? SymbolTable.Equals(((ObjectVariable)obj).SymbolTable) ? 0 : -1 : -1;

        public EqualityResult LooseEquals(IVariable other)
        {
            if (other is UndefinedVariable or ObjectVariable or RuntimeEvaluatedMemberObjectVariable)
                return EqualityResult.Create(StrictEquals(other));

            return EqualityResult.IncompatibleComparison(this, other);
        }

        public bool StrictEquals(IVariable other) => other is RuntimeEvaluatedMemberObjectVariable otherObject && ReferenceEquals(SymbolTable, otherObject.SymbolTable);

        public override string ToString() => StringValue;
    }
}
