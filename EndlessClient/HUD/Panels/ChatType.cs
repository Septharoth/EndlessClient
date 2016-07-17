// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.HUD.Panels
{
    /// <summary>
    /// Represents the different icons displayed next to lines of chat text.
    /// These go in numerical order for how they are in the sprite sheet in the GFX file
    /// </summary>
    public enum ChatType
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