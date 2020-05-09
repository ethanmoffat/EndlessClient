using System;

namespace EOLib.IO.Map
{
    public class SignMapEntity : IMapEntity
    {
        public const int DATA_SIZE = 5;

        public int X { get; private set; }

        public int Y { get; private set; }

        public string Title { get; private set; }

        public string Message { get; private set; }

        public SignMapEntity()
            : this(-1, -1, string.Empty, String.Empty)
        { }

        private SignMapEntity(int x, int y, string title, string message)
        {
            X = x;
            Y = y;
            Title = title;
            Message = message;
        }

        public SignMapEntity WithX(int x)
        {
            var newEntity = MakeCopy(this);
            newEntity.X = x;
            return newEntity;
        }

        public SignMapEntity WithY(int y)
        {
            var newEntity = MakeCopy(this);
            newEntity.Y = y;
            return newEntity;
        }

        public SignMapEntity WithTitle(string title)
        {
            var newEntity = MakeCopy(this);
            newEntity.Title = title;
            return newEntity;
        }

        public SignMapEntity WithMessage(string message)
        {
            var newEntity = MakeCopy(this);
            newEntity.Message = message;
            return newEntity;
        }

        private static SignMapEntity MakeCopy(SignMapEntity src)
        {
            return new SignMapEntity(src.X, src.Y, src.Title, src.Message);
        }
    }
}
