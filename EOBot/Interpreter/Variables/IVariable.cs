using System;

namespace EOBot.Interpreter.Variables
{
    // marker interface so we don't have to implement CompareTo for a million different function/action interfaces
    public interface IVariable : IIdentifiable, IComparable
    {
    }

    public interface IVariable<T> : IVariable
    {
        T Value { get; }
    }
}