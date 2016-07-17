// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Login
{
    public enum CharacterLoginReply : short
    {
        RequestGranted = 1, //response from welcome_request
        RequestCompleted = 2, //response from welcome_message
    }
}