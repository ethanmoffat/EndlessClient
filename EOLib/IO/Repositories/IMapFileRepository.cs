// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using IMapFile = EOLib.IO.OldMap.IMapFile;

namespace EOLib.IO.Repositories
{
    public interface IMapFileRepository
    {
        Dictionary<int, IMapFile> MapFiles { get; }
    }
}