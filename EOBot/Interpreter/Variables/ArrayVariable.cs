using System;
using System.Collections.Generic;
using System.Linq;

namespace EOBot.Interpreter.Variables;

public class ArrayVariable : IVariable<List<IVariable>>
{
    public List<IVariable> Value { get; }

    public string StringValue => "[" + string.Join(", ", Value.Select(x => x.StringValue)) + "]";

    public ArrayVariable(List<IVariable> value)
    {
        Value = value;
    }

    // todo: should arrays be comparable?
    public int CompareTo(object obj) => -1;

    public IVariable<List<IVariable>> WithNewValue(List<IVariable> value)
    {
        return new ArrayVariable(value);
    }

    public override string ToString() => StringValue;
}