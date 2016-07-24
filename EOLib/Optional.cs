// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib
{
    public class Optional<T>
    {
        public T Value { get; private set; }

        public bool HasValue { get; private set; }

        public Optional()
        {
            Value = default(T);
            HasValue = false;
        }

        public Optional(T value)
        {
            Value = value;
            HasValue = true;
        }
    }
}
