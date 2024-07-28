using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Init
{
    public interface IInitPacketHandler
    {
        InitReply Reply { get; }

        bool HandleData(InitInitServerPacket.IReplyCodeData data);
    }

    public interface IInitPacketHandler<TData> : IInitPacketHandler
        where TData : InitInitServerPacket.IReplyCodeData
    {
        bool HandleData(TData data);
    }

    public abstract class BaseInGameInitPacketHandler<TData> : IInitPacketHandler<TData>
        where TData : InitInitServerPacket.IReplyCodeData
    {
        public abstract InitReply Reply { get; }

        public bool HandleData(InitInitServerPacket.IReplyCodeData data)
            => HandleData((TData)data);

        public abstract bool HandleData(TData data);
    }
}