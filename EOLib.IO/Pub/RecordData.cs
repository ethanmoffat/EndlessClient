namespace EOLib.IO.Pub
{
    public struct RecordData
    {
        public static RecordData Default { get; } = new RecordData(0, 0, 0);

        public int Offset { get; }

        public int Length { get; }

        public int Value { get; }

        public RecordData(int offset, int length, int value)
            : this()
        {
            Offset = offset;
            Length = length;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(RecordData))
                return false;

            RecordData other = (RecordData)obj;

            return Offset == other.Offset &&
                Length == other.Length &&
                Value == other.Value;
        }

        public override int GetHashCode()
        {
            var hash = 397 ^ Offset.GetHashCode();
            hash = (hash * 397) ^ Length.GetHashCode();
            hash = (hash * 397) ^ Value.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return $"{Offset} {Length} {Value}";
        }
    }
}
