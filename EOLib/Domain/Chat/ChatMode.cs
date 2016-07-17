// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Chat
{
    /// <summary>
    /// Represents the graphic displayed next to the text in the chat bar
    /// These go in numerical order for how they are in the sprite sheet in the GFX file
    /// </summary>
    public enum ChatMode
    {
        NoText,
        Public,
        Private,
        Global,
        Group,
        Admin,
        Muted,
        Guild
    }
}