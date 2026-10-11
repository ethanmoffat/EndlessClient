using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.Variables
{
    public sealed class DictVariable : CollectionVariable, IVariable<Dictionary<string, IVariable>>
    {
        public Dictionary<string, IVariable> Value { get; }

        public override string StringValue => "[" + string.Join(", ", Value.Select(x => $"{x.Key}, {x.Value.StringValue}")) + "]";

        public override string TypeName => BotTokenParser.KEYWORD_DICT;

        protected override object Contents => Value;

        public DictVariable(Dictionary<string, IVariable> value)
        {
            Value = value;
        }

        // todo: should dicts be comparable?
        public override int CompareTo(object obj) => -1;

        protected override IReadOnlyDictionary<object, IIdentifiable> Members =>
            Value.ToDictionary(x => (object)x.Key, x => (IIdentifiable)x.Value);

        public static IVariable<Dictionary<string, IVariable>> WithNewValue(Dictionary<string, IVariable> value)
        {
            return new DictVariable(value);
        }

        public override string ToString() => StringValue;
    }
}
