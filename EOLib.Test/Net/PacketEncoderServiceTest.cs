using EOLib.Domain.Interact.Quest;
using EOLib.IO.Services;
using EOLib.Net;
using EOLib.Net.PacketProcessing;
using NUnit.Framework;

namespace EOLib.IO.Test.Services
{
    [TestFixture]
    public class PacketEncoderServiceTest
    {
        [Test]
        public void PacketEncoderService_Encode_127Byte_DoesNotProduceAny0ValueBytes()
        {
            var svc = new PacketEncoderService(new NumberEncoderService(), new DataEncoderService());

            var packet = new PacketBuilder(PacketFamily.Quest, PacketAction.Accept)
                .AddShort(0)
                .AddShort(0)
                .AddShort(111)
                .AddShort(127)
                .AddChar((byte)DialogReply.Link)
                .AddChar(1)
                .Build();

            var encoded = svc.Encode(packet, 5);

            Assert.That(encoded, Has.All.Not.EqualTo(0));
        }

        [Test]
        public void PacketEncoderService_Encode_0Byte_DoesNotProduceAny128ValueBytes()
        {
            var svc = new PacketEncoderService(new NumberEncoderService(), new DataEncoderService());

            var packet = new PacketBuilder(PacketFamily.Quest, PacketAction.Accept)
                .AddByte(0)
                .Build();

            var encoded = svc.Encode(packet, 5);

            Assert.That(encoded, Has.All.Not.EqualTo(128));
        }
    }
}
