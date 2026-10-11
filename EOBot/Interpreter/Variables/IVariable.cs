using System;

namespace EOBot.Interpreter.Variables
{
    // marker interface so we don't have to implement CompareTo for a million different function/action interfaces
    public interface IVariable : IIdentifiable, IComparable
    {
        string TypeName { get; }

        /// <summary>
        /// Loose equality (==). The result is not Ok, with a reason, if the values cannot be compared.
        /// </summary>
        EqualityResult LooseEquals(IVariable other);

        /// <summary>
        /// Strict equality (===).
        /// </summary>
        bool StrictEquals(IVariable other);
    }

    public interface IVariable<T> : IVariable
    {
        T Value { get; }
    }

    /// <summary>
    /// A variable whose value contains other variables. Collections can only be loosely compared with collections of the same kind or undefined.
    /// </summary>
    public interface ICollectionVariable : IVariable
    {
    }
}
