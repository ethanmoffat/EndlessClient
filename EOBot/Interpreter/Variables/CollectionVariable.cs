using System;
using System.Collections.Generic;

namespace EOBot.Interpreter.Variables
{
    /// <summary>
    /// A variable whose value contains other variables. Loose equality (==) compares contents recursively; strict equality (===) compares instances.
    /// </summary>
    public abstract class CollectionVariable : ICollectionVariable
    {
        public abstract string StringValue { get; }

        public abstract string TypeName { get; }

        protected abstract object Contents { get; }

        /// <summary>
        /// The members compared by loose equality, keyed by array index, dict key, or object member name.
        /// </summary>
        protected abstract IReadOnlyDictionary<object, IIdentifiable> Members { get; }

        public virtual int CompareTo(object obj) => throw new NotSupportedException($"{TypeName} values are not ordered");

        public EqualityResult LooseEquals(IVariable other) => LooseEquals(other, new ComparisonPath());

        public bool StrictEquals(IVariable other) =>
            other is CollectionVariable otherCollection && other.GetType() == GetType() && ReferenceEquals(Contents, otherCollection.Contents);

        protected virtual bool IsAlwaysUnequal(IVariable other) => other is UndefinedVariable;

        private EqualityResult LooseEquals(IVariable other, ComparisonPath path)
        {
            if (IsAlwaysUnequal(other))
                return EqualityResult.Create(false);

            if (other is not CollectionVariable otherCollection || other.GetType() != GetType())
                return EqualityResult.IncompatibleComparison(this, other);

            if (!path.TryAdd(Contents, otherCollection.Contents))
                return EqualityResult.SelfContaining(this);

            try
            {
                return MembersEqual(otherCollection, path);
            }
            finally
            {
                path.Remove(Contents, otherCollection.Contents);
            }
        }

        private EqualityResult MembersEqual(CollectionVariable other, ComparisonPath path)
        {
            // Members builds a new dictionary on each access
            var lhs = Members;
            var rhs = other.Members;
            if (lhs.Count != rhs.Count)
                return EqualityResult.Create(false);

            foreach (var (key, lhsMember) in lhs)
            {
                if (!rhs.TryGetValue(key, out var rhsMember))
                    return EqualityResult.Create(false);

                var result = (lhsMember, rhsMember) switch
                {
                    (CollectionVariable lhsCollection, IVariable rhsVariable) => lhsCollection.LooseEquals(rhsVariable, path),
                    (IVariable lhsVariable, IVariable rhsVariable) => lhsVariable.LooseEquals(rhsVariable),
                    _ => EqualityResult.Create(ReferenceEquals(lhsMember, rhsMember)),
                };
                if (!result.Ok || !result.Equal)
                    return result;
            }

            return EqualityResult.Create(true);
        }

        /// <summary>
        /// The collections on the current comparison path, so self-containing collections are reported instead of recursing forever.
        /// </summary>
        private sealed class ComparisonPath
        {
            private readonly HashSet<object> _lhs = new(ReferenceEqualityComparer.Instance);
            private readonly HashSet<object> _rhs = new(ReferenceEqualityComparer.Instance);

            public bool TryAdd(object lhs, object rhs)
            {
                return _lhs.Add(lhs) && _rhs.Add(rhs);
            }

            public void Remove(object lhs, object rhs)
            {
                _lhs.Remove(lhs);
                _rhs.Remove(rhs);
            }
        }
    }
}
