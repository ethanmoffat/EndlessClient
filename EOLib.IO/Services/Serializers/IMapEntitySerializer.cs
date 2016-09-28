// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    public interface IMapEntitySerializer<T> where T : IMapEntity
    {
        //todo: refactor into constants within MapFileSerializer
        int DataSize { get; }

        MapEntitySerializeType MapEntitySerializeType { get; }

        byte[] SerializeToByteArray(T mapEntity);

        T DeserializeFromByteArray(byte[] data);
    }
}
