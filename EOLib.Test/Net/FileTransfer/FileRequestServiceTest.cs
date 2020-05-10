using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EOLib.Domain.Protocol;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.FileTransfer;
using EOLib.Test.TestHelpers;
using NUnit.Framework;
using Moq;

namespace EOLib.Test.Net.FileTransfer
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class FileRequestServiceTest
    {
        private IFileRequestService _fileRequestService;
        
        private IPacketSendService _packetSendService;
        private INumberEncoderService _numberEncoderService;
        private ISerializer<IMapFile> _mapFileSerializer;

        [SetUp]
        public void SetUp()
        {
            _packetSendService = Mock.Of<IPacketSendService>();
            _numberEncoderService = new NumberEncoderService();
            _mapFileSerializer = Mock.Of<ISerializer<IMapFile>>();

            _fileRequestService = new FileRequestService(_packetSendService,
                                                         _numberEncoderService,
                                                         _mapFileSerializer);
        }

        #region RequestFile Tests

        [Test]
        public void RequestFile_ResponsePacketHasInvalidHeader_ThrowsEmptyPacketReceivedException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader(PacketFamily.Account, PacketAction.Accept);
            Assert.ThrowsAsync<EmptyPacketReceivedException>(async () => await _fileRequestService.RequestFile(InitFileType.Item));
        }

        [Test]
        public void RequestFile_ResponsePacketInvalidExtraByte_ThrowsMalformedPacketException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader(PacketFamily.Init, PacketAction.Init, (byte)InitReply.ItemFile, 33);

            Assert.ThrowsAsync<MalformedPacketException>(async () => await _fileRequestService.RequestFile(InitFileType.Item));
        }

        [Test]
        public void RequestFile_SendsPacket_BasedOnSpecifiedType()
        {
            var types = new[] { InitFileType.Item, InitFileType.Npc, InitFileType.Spell, InitFileType.Class };
            foreach (var type in types)
            {
                var packetIsCorrect = false;
                var localType = type;
                Mock.Get(_packetSendService).Setup(x => x.SendEncodedPacketAndWaitAsync(It.IsAny<IPacket>()))
                    .Callback((IPacket packet) => packetIsCorrect = IsCorrectFileRequestPacket(packet, localType));

                _fileRequestService.RequestFile(type);

                Assert.IsTrue(packetIsCorrect, "Incorrect packet for {0}", type);
            }
        }

        [Test]
        public void RequestFile_CorrectResponse_ExecutesWithoutFault()
        {
            var types = new[] { InitFileType.Item, InitFileType.Npc, InitFileType.Spell, InitFileType.Class };
            foreach (var type in types)
            {
                Mock.Get(_packetSendService).SetupReceivedPacketHasHeader(PacketFamily.Init, PacketAction.Init, CreateFilePacket(type));

                AggregateException aggEx = null;
                switch (type)
                {
                    case InitFileType.Item: aggEx = _fileRequestService.RequestFile(type).Exception; break;
                    case InitFileType.Npc: aggEx = _fileRequestService.RequestFile(type).Exception; break;
                    case InitFileType.Spell: aggEx = _fileRequestService.RequestFile(type).Exception; break;
                    case InitFileType.Class: aggEx = _fileRequestService.RequestFile(type).Exception; break;
                }

                if (aggEx != null)
                    throw aggEx.InnerException;
            }
        }

        #endregion

        #region RequestMapFile Tests

        [Test]
        public void RequestMapFile_ResponsePacketHasInvalidHeader_ThrowsEmptyPacketReceivedException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader(PacketFamily.Account, PacketAction.Accept);
            Assert.ThrowsAsync<EmptyPacketReceivedException>(async () => await _fileRequestService.RequestMapFile(1));
        }

        [Test]
        public void RequestMapFile_ResponsePacketHasIncorrectFileType_ThrowsMalformedPacketException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader(PacketFamily.Init, PacketAction.Init, (byte) InitReply.SpellFile, 33);
            Assert.ThrowsAsync<MalformedPacketException>(async () => await _fileRequestService.RequestMapFile(1));
        }

        [Test]
        public void RequestMapFile_SendsPacket_BasedOnSpecifiedMap()
        {
            var packetIsCorrect = false;
            Mock.Get(_packetSendService).Setup(x => x.SendEncodedPacketAndWaitAsync(It.IsAny<IPacket>())).Callback((IPacket packet) => packetIsCorrect = IsCorrectFileRequestPacket(packet, InitFileType.Map));

            _fileRequestService.RequestMapFile(1);

            Assert.IsTrue(packetIsCorrect, "Incorrect packet for Map");
        }

        #endregion

        #region Helper Methods

        private static bool IsCorrectFileRequestPacket(IPacket packet, InitFileType type)
        {
            return packet.Family == PacketFamily.Welcome && packet.Action == PacketAction.Agree && packet.ReadChar() == (byte) type;
        }

        private static byte[] CreateFilePacket(InitFileType type)
        {
            IPacketBuilder packetBuilder = new PacketBuilder();

            var nes = new NumberEncoderService();

            switch (type)
            {
                case InitFileType.Item:
                    packetBuilder = packetBuilder
                        .AddChar((byte) InitReply.ItemFile).AddChar(1) //spacer
                        .AddString("EIF").AddInt(1) //RID
                        .AddShort(2) //Len
                        .AddByte(1) //filler byte
                        .AddBytes(new EIFRecord { ID = 1, Name = "Test1" }.SerializeToByteArray(nes))
                        .AddBytes(new EIFRecord { ID = 2, Name = "eof" }.SerializeToByteArray(nes));
                    break;
                case InitFileType.Npc:
                    packetBuilder = packetBuilder
                        .AddChar((byte) InitReply.NpcFile).AddChar(1) //spacer
                        .AddString("ENF").AddInt(1) //RID
                        .AddShort(2) //Len
                        .AddByte(1) //filler byte
                        .AddBytes(new ENFRecord { ID = 1, Name = "Test1" }.SerializeToByteArray(nes))
                        .AddBytes(new ENFRecord { ID = 2, Name = "eof" }.SerializeToByteArray(nes));
                    break;
                case InitFileType.Spell:
                    packetBuilder = packetBuilder
                        .AddChar((byte) InitReply.SpellFile).AddChar(1) //spacer
                        .AddString("ESF").AddInt(1) //RID
                        .AddShort(2) //Len
                        .AddByte(1) //filler byte
                        .AddBytes(new ESFRecord { ID = 1, Name = "Test1", Shout = "" }.SerializeToByteArray(nes))
                        .AddBytes(new ESFRecord { ID = 2, Name = "eof", Shout = "" }.SerializeToByteArray(nes));
                    break;
                case InitFileType.Class:
                    packetBuilder = packetBuilder
                        .AddChar((byte) InitReply.ClassFile).AddChar(1) //spacer
                        .AddString("ECF").AddInt(1) //RID
                        .AddShort(2) //Len
                        .AddByte(1) //filler byte
                        .AddBytes(new ECFRecord { ID = 1, Name = "Test1" }.SerializeToByteArray(nes))
                        .AddBytes(new ECFRecord { ID = 2, Name = "eof" }.SerializeToByteArray(nes));
                    break;
            }

            return packetBuilder.Build().RawData.ToArray();
        }

        #endregion
    }
}
