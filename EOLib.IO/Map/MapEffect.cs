namespace EOLib.IO.Map
{
    public enum MapEffect : byte
    {
        None = 0,
        HPDrain = 1,
        TPDrain = 2,
        Quake1 = 3,
        Quake2 = 4,
        Quake3 = 5,
        Quake4 = 6,

        // not a recognized value for IO; used internally
        Spikes = 0x7f
    }
}
