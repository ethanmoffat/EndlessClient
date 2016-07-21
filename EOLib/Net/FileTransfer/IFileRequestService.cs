// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Protocol;
using EOLib.IO.Map;
using EOLib.IO.OldMap;
using EOLib.IO.Pub;

namespace EOLib.Net.FileTransfer
{
    public interface IFileRequestService
    {
        Task<IMapFile> RequestMapFile(short mapID);

        Task<IPubFile> RequestFile(InitFileType fileType);
    }
}
