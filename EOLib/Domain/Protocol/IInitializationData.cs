using EOLib.Net.Translators;

namespace EOLib.Domain.Protocol
{
    public interface IInitializationData : ITranslatedData
    {
        InitReply Response { get; }

        int this[InitializationDataKey key] { get; }
    }

    public enum InitializationDataKey
    {
        //response: OK
        SequenceByte1,
        SequenceByte2,
        SendMultiple,
        ReceiveMultiple,
        ClientID,
        HashResponse,
        //response: Out of Date
        RequiredVersionNumber,
        //response: Banned
        BanType,
        BanTimeRemaining
    }
}
