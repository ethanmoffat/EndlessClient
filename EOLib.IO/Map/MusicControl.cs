namespace EOLib.IO.Map
{
    // Derived from work of Deaven#6116 on eoserv developers discord
    public enum MusicControl : byte
    {
        // Map Control 0 (INTERRUPT_IF_DIFFERENT_PLAY_ONCE)
        //    * If a different song is playing interrupt it and start playing music for this map once.
        InterruptIfDifferentPlayOnce = 0,

        // Map Control 1 (INTERRUPT_PLAY_ONCE)
        //    * Always interrupt current song and start playing music for this map once.
        InterruptPlayOnce = 1,

        // Map Control 2 (FINISH_PLAY_ONCE)
        //    * If a song is playing let it finish before playing music for this map once.
        FinishPlayOnce = 2,

        // Map Control 3 (INTERRUPT_IF_DIFFERENT_PLAY_REPEAT)
        //    * If a different song is playing interrupt it and start playing music for this map on repeat.
        InterruptIfDifferentPlayRepeat = 3,

        // Map Control 4 (INTERRUPT_PLAY_REPEAT)
        //    * Always interrupt current song and start playing music for this map on repeat.
        InterruptPlayRepeat = 4,

        // Map Control 5 (FINISH_PLAY_REPEAT)
        //    * If a song is playing let it finish before playing music for this map on repeat.
        FinishPlayRepeat = 5,

        // Map Control 6 (TURN_OFF)
        //    * Interrupt all music and does not play music for this map.
        TurnOff = 6,
    }
}
