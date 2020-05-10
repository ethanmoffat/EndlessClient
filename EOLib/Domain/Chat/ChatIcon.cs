namespace EOLib.Domain.Chat
{
    /// <summary>
    /// Represents the different icons displayed next to lines of chat text.
    /// These go in numerical order for how they are in the sprite sheet in the GFX file
    /// </summary>
    public enum ChatIcon
    {
        None = -1, //blank icon - trying to load will return empty texture
        SpeechBubble = 0,
        Note,
        Error,
        NoteLeftArrow,
        GlobalAnnounce,
        Star,
        Exclamation,
        LookingDude,
        Heart,
        Player,
        PlayerParty,
        PlayerPartyDark,
        GM,
        GMParty,
        HGM,
        HGMParty,
        DownArrow,
        UpArrow,
        DotDotDotDot,
        GSymbol,
        Skeleton,
        WhatTheFuck,
        Information,
        QuestMessage
    }
}