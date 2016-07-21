// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.IO.Services;

namespace EOLib.IO.Map
{
    public interface IMapFile
    {
        IMapFileProperties Properties { get; }

        IReadOnlyMatrix<TileSpec> Tiles { get; }
        IReadOnlyMatrix<WarpMapEntity> Warps { get; }
        IReadOnlyDictionary<MapLayer, IReadOnlyMatrix<int>> GFX { get; }

        IReadOnlyList<NPCSpawnMapEntity> NPCSpawns { get; }
        IReadOnlyList<byte[]> Unknowns { get; }
        IReadOnlyList<ChestSpawnMapEntity> Chests { get; }
        IReadOnlyList<SignMapEntity> Signs { get; }

        byte[] SerializeToByteArray(INumberEncoderService numberEncoderService,
                                    IMapStringEncoderService mapStringEncoderService);

        void DeserializeFromByteArray(byte[] data,
                                      INumberEncoderService numberEncoderService,
                                      IMapStringEncoderService mapStringEncoderService);
    }
}
