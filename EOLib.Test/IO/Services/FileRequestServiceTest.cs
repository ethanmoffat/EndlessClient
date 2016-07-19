// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using EOLib.Domain.Protocol;
using EOLib.IO;
using EOLib.IO.Services;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Test.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EOLib.Test.IO.Services
{
    [TestClass]
    public class FileRequestServiceTest
    {
        private IFileRequestService _fileRequestService;
        private IPacketSendService _packetSendService;

        [TestInitialize]
        public void TestInitialize()
        {
            _packetSendService = Mock.Of<IPacketSendService>();

            _fileRequestService = new FileRequestService(_packetSendService);
        }

        #region RequestFile Tests

        [TestMethod]
        public void RequestFile_ResponsePacketHasInvalidHeader_ThrowsEmptyPacketReceivedException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader(PacketFamily.Account, PacketAction.Accept);

            var task = _fileRequestService.RequestFile<ItemRecord>(InitFileType.Item);

            Assert.IsTrue(task.IsFaulted);
            Assert.IsInstanceOfType((task.Exception ?? new AggregateException()).InnerExceptions.Single(),
                                    typeof(EmptyPacketReceivedException));
        }

        [TestMethod]
        public void RequestFile_ResponsePacketInvalidExtraByte_ThrowsMalformedPacketException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader(PacketFamily.Init, PacketAction.Init, (byte)InitReply.ItemFile, 33);

            var task = _fileRequestService.RequestFile<ItemRecord>(InitFileType.Item);

            Assert.IsTrue(task.IsFaulted);
            Assert.IsInstanceOfType((task.Exception ?? new AggregateException()).InnerExceptions.Single(),
                                    typeof(MalformedPacketException));
        }

        [TestMethod]
        public void RequestFile_SendsPacket_BasedOnSpecifiedType()
        {
            var types = new[] { InitFileType.Item, InitFileType.Npc, InitFileType.Spell, InitFileType.Class };
            foreach (var type in types)
            {
                var packetIsCorrect = false;
                var localType = type;
                Mock.Get(_packetSendService).Setup(x => x.SendEncodedPacketAndWaitAsync(It.IsAny<IPacket>()))
                    .Callback((IPacket packet) => packetIsCorrect = IsCorrectFileRequestPacket(packet, localType));

                _fileRequestService.RequestFile<ItemRecord>(type);

                Assert.IsTrue(packetIsCorrect, "Incorrect packet for {0}", type);
            }
        }

        [TestMethod]
        public void RequestFile_CorrectResponse_ExecutesWithoutFault()
        {
            var types = new[] { InitFileType.Item, InitFileType.Npc, InitFileType.Spell, InitFileType.Class };
            foreach (var type in types)
            {
                Mock.Get(_packetSendService).SetupReceivedPacketHasHeader(PacketFamily.Init, PacketAction.Init, CreateFilePacket(type));

                AggregateException aggEx = null;
                switch (type)
                {
                    case InitFileType.Item: aggEx = _fileRequestService.RequestFile<ItemRecord>(type).Exception; break;
                    case InitFileType.Npc: aggEx = _fileRequestService.RequestFile<NPCRecord>(type).Exception; break;
                    case InitFileType.Spell: aggEx = _fileRequestService.RequestFile<SpellRecord>(type).Exception; break;
                    case InitFileType.Class: aggEx = _fileRequestService.RequestFile<ClassRecord>(type).Exception; break;
                }

                if (aggEx != null)
                    throw aggEx.InnerException;
            }
        }

        #endregion

        #region RequestMapFile Tests

        [TestMethod]
        public void RequestMapFile_ResponsePacketHasInvalidHeader_ThrowsEmptyPacketReceivedException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader(PacketFamily.Account, PacketAction.Accept);

            var task = _fileRequestService.RequestFile<ItemRecord>(InitFileType.Item);

            Assert.IsTrue(task.IsFaulted);
            Assert.IsInstanceOfType((task.Exception ?? new AggregateException()).InnerExceptions.Single(), typeof (EmptyPacketReceivedException));
        }

        [TestMethod]
        public void RequestMapFile_ResponsePacketInvalidExtraByte_ThrowsMalformedPacketException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader(PacketFamily.Init, PacketAction.Init, (byte) InitReply.MapFile, 33);

            var task = _fileRequestService.RequestFile<ItemRecord>(InitFileType.Item);

            Assert.IsTrue(task.IsFaulted);
            Assert.IsInstanceOfType((task.Exception ?? new AggregateException()).InnerExceptions.Single(), typeof (MalformedPacketException));
        }

        [TestMethod]
        public void RequestMapFile_SendsPacket_BasedOnSpecifiedMap()
        {
            var packetIsCorrect = false;
            Mock.Get(_packetSendService).Setup(x => x.SendEncodedPacketAndWaitAsync(It.IsAny<IPacket>())).Callback((IPacket packet) => packetIsCorrect = IsCorrectFileRequestPacket(packet, InitFileType.Map));

            _fileRequestService.RequestFile<ItemRecord>(InitFileType.Map);

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

            switch (type)
            {
                case InitFileType.Item:
                    packetBuilder = packetBuilder
                        .AddChar((byte) InitReply.ItemFile).AddChar(1) //spacer
                        .AddString("EIF").AddInt(1) //RID
                        .AddShort(2) //Len
                        .AddByte(1) //filler byte
                        .AddBytes(new ItemRecord(1) {Name = "Test1"}.SerializeToByteArray()).AddBytes(new ItemRecord(2) {Name = "EOF"}.SerializeToByteArray());
                    break;
                case InitFileType.Npc:
                    packetBuilder = packetBuilder
                        .AddChar((byte) InitReply.NpcFile).AddChar(1) //spacer
                        .AddString("ENF").AddInt(1) //RID
                        .AddShort(2) //Len
                        .AddByte(1) //filler byte
                        .AddBytes(new NPCRecord(1) {Name = "Test1"}.SerializeToByteArray()).AddBytes(new NPCRecord(2) {Name = "EOF"}.SerializeToByteArray());
                    break;
                case InitFileType.Spell:
                    packetBuilder = packetBuilder
                        .AddChar((byte) InitReply.SpellFile).AddChar(1) //spacer
                        .AddString("ESF").AddInt(1) //RID
                        .AddShort(2) //Len
                        .AddByte(1) //filler byte
                        .AddBytes(new SpellRecord(1) {Name = "Test1", Shout = ""}.SerializeToByteArray())
                        .AddBytes(new SpellRecord(2) {Name = "EOF", Shout = ""}.SerializeToByteArray());
                    break;
                case InitFileType.Class:
                    packetBuilder = packetBuilder
                        .AddChar((byte) InitReply.ClassFile).AddChar(1) //spacer
                        .AddString("ECF").AddInt(1) //RID
                        .AddShort(2) //Len
                        .AddByte(1) //filler byte
                        .AddBytes(new ClassRecord(1) {Name = "Test1"}.SerializeToByteArray()).AddBytes(new ClassRecord(2) {Name = "EOF"}.SerializeToByteArray());
                    break;
            }

            return packetBuilder.Build().RawData.ToArray();
        }

        #endregion
    }
}
