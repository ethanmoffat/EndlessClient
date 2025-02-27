using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.Variables
{
    public class DictVariable : IVariable<Dictionary<string, IVariable>>
    {
        public Dictionary<string, IVariable> Value { get; }

        public string StringValue => "[" + string.Join(", ", Value.Select(x => $"{x.Key}, {x.Value.StringValue}")) + "]";

        public DictVariable(Dictionary<string, IVariable> value)
        {
            Value = value;
        }

        // todo: should dicts be comparable?
        public int CompareTo(object obj) => -1;

        public static IVariable<Dictionary<string, IVariable>> WithNewValue(Dictionary<string, IVariable> value)
        {
            return new DictVariable(value);
        }

        public override string ToString() => StringValue;
    }
}
