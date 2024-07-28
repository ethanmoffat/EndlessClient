using AutomaticTypeMapper;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType]
    public class PacketFactoryFactory : IPacketFactoryFactory
    {
        public IPacketFactory Create(string name_space)
        {
            return new PacketFactory(name_space);
        }
    }

    public interface IPacketFactoryFactory
    {
        IPacketFactory Create(string name_space);
    }
}