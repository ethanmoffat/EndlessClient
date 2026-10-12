using System;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.Variables
{
    public sealed class ArrayVariable : CollectionVariable, IVariable<List<IVariable>>
    {
        public List<IVariable> Value { get; }

        public override string StringValue => "[" + string.Join(", ", Value.Select(x => x.StringValue)) + "]";

        public override string TypeName => Keywords.Array;

        protected override object Contents => Value;

        public ArrayVariable(List<IVariable> value)
        {
            Value = value;
        }

        protected override IReadOnlyDictionary<object, IIdentifiable> Members =>
            Value
                .Select((element, index) => (element, index))
                .ToDictionary(x => (object)x.index, x => (IIdentifiable)x.element);

        /// <summary>
        /// The index of the first element equal to value with ==, or -1. Elements that cannot be compared with value are not a match.
        /// </summary>
        public int IndexOf(IVariable value) => Value.FindIndex(x => x.LooseEquals(value) is { Ok: true, Equal: true });

        public IVariable<List<IVariable>> WithNewValue(List<IVariable> value)
        {
            return new ArrayVariable(value);
        }

        public override string ToString() => StringValue;
    }
}
