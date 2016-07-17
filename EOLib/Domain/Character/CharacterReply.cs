// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Character
{
    public enum CharacterReply : short
    {
        Exists = 1,
        Full = 2,
        NotApproved = 4,
        Ok = 5,
        Deleted = 6,
        THIS_IS_WRONG = 255
    }
}