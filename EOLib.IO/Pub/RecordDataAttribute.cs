using System;

namespace EOLib.IO.Pub
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class RecordDataAttribute : Attribute
    {
        public int Offset { get; }

        public int Length { get; }

        public RecordDataAttribute(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }
    }
}
