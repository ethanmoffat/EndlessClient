// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Services;

namespace EOLib.IO.Map
{
    public interface IMapEntity
    {
        int DataSize { get; }

        int X { get; }

        int Y { get; }

        //todo: find some way so that not all map entities have to know about IMapStringEncoderService
        byte[] SerializeToByteArray(INumberEncoderService numberEncoderService,
                                    IMapStringEncoderService mapStringEncoderService);

        void DeserializeFromByteArray(byte[] data,
                                      INumberEncoderService numberEncoderService,
                                      IMapStringEncoderService mapStringEncoderService);
    }
}
