// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public abstract class BasePubFile<T> : IPubFile<T>
        where T : IPubRecord
    {
        protected readonly List<T> _data;

        public abstract string FileType { get; }

        public int CheckSum { get; set; }

        public int Length { get { return _data.Count; } }

        public T this[int id]
        {
            get { return _data[id - 1]; }
            set { _data[id - 1] = value; }
        }

        protected BasePubFile()
        {
            _data = new List<T>();
        }

        public abstract byte[] SerializeToByteArray(INumberEncoderService numberEncoderService);

        public abstract void DeserializeFromByteArray(byte[] bytes, INumberEncoderService numberEncoderService);
    }
}
