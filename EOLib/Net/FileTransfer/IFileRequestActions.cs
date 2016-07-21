// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Protocol;

namespace EOLib.IO.Actions
{
    public interface IFileRequestActions
    {
        bool NeedsFile(InitFileType fileType, short optionalID = 0);

        Task GetMapFromServer(short mapID);

        Task GetItemFileFromServer();

        Task GetNPCFileFromServer();

        Task GetSpellFileFromServer();

        Task GetClassFileFromServer();
    }
}
