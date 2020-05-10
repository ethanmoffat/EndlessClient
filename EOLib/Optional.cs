namespace EOLib
{
    public struct Optional<T>
    {
        private static readonly Optional<T> _readOnlyEmpty = new Optional<T>();
        public static Optional<T> Empty => _readOnlyEmpty;

        public T Value { get; }

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
