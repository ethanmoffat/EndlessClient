// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Chat.Commands
{
    public interface IPlayerCommand
    {
        string CommandText { get; }

        bool Execute(string parameter);
    }
}
