namespace EOLib.Net.Translators
{
    public interface IPacketTranslator<out T>
        where T : ITranslatedData
    {
        T TranslatePacket(IPacket packet);
    }
}
