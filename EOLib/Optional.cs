// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib
{
    public struct Optional<T>
    {
        private static readonly Optional<T> _readOnlyEmpty = new Optional<T>();
        public static Optional<T> Empty { get { return _readOnlyEmpty; } }

        public T Value { get; private set; }

        public bool HasValue { get; private set; }

        public Optional(T value)
            : this()
        {
            Value = value;
            HasValue = true;
        }

        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }

        public static implicit operator T(Optional<T> optional)
        {
            return optional.Value;
        }
    }
}
