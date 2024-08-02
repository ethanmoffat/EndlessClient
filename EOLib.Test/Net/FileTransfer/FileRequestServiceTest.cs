using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using EOLib.Domain.Protocol;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.FileTransfer;
using EOLib.Test.TestHelpers;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Moq;
using NUnit.Framework;

namespace EOLib.Test.Net.FileTransfer
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class FileRequestServiceTest
    {
        private IFileRequestService _fileRequestService;

        private IPacketSendService _packetSendService;
        private INumberEncoderService _numberEncoderService;
        private IMapDeserializer<IMapFile> _mapFileSerializer;
        private IPubFileDeserializer _pubFileDeserializer;

        [SetUp]
        public void SetUp()
        {
            _packetSendService = Mock.Of<IPacketSendService>();
            _numberEncoderService = new NumberEncoderService();
            _mapFileSerializer = Mock.Of<IMapDeserializer<IMapFile>>();
            _pubFileDeserializer = Mock.Of<IPubFileDeserializer>();

            _fileRequestService = new FileRequestService(_packetSendService,
                                                         _mapFileSerializer,
                                                         _pubFileDeserializer);
        }

        #region RequestFile Tests

        [Test]
        public void RequestFile_ResponsePacketHasInvalidHeader_ThrowsEmptyPacketReceivedException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader<AccountReplyServerPacket>();
            Assert.ThrowsAsync<EmptyPacketReceivedException>(async () => await _fileRequestService.RequestFile<EIFRecord>(FileType.Eif, 1));
        }

        [Test]
        public void RequestFile_ResponsePacketInvalidExtraByte_ThrowsMalformedPacketException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader<InitInitServerPacket>((byte)InitReply.FileEif, 33);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _fileRequestService.RequestFile<EIFRecord>(FileType.Eif, 1));
        }

        [Test]
        public void RequestFile_SendsPacket_BasedOnSpecifiedType()
        {
            var types = new[] { FileType.Eif, FileType.Enf, FileType.Esf, FileType.Ecf };
            foreach (var type in types)
            {
                var packetIsCorrect = false;
                var localType = type;
                Mock.Get(_packetSendService).Setup(x => x.SendEncodedPacketAndWaitAsync(It.IsAny<IPacket>()))
                    .Callback((IPacket packet) => packetIsCorrect = IsCorrectFileRequestPacket(packet, localType));

                _fileRequestService.RequestFile<EIFRecord>(type, 1);

                Assert.IsTrue(packetIsCorrect, "Incorrect packet for {0}", type);
            }
        }

        [Test]
        public void RequestFile_CorrectResponse_ExecutesWithoutFault()
        {
            var types = new[] { FileType.Eif, FileType.Enf, FileType.Esf, FileType.Ecf };
            foreach (var type in types)
            {
                Mock.Get(_packetSendService).SetupReceivedPacketHasHeader<InitInitServerPacket>(CreateFilePacket(type));

                AggregateException aggEx = null;
                switch (type)
                {
                    case FileType.Eif:
                        Mock.Get(_pubFileDeserializer)
                            .Setup(x => x.DeserializeFromByteArray(1, It.IsAny<byte[]>(), It.IsAny<Func<IPubFile<EIFRecord>>>()))
                            .Returns(new EIFFile());
                        aggEx = _fileRequestService.RequestFile<EIFRecord>(type, 1).Exception;
                        break;
                    case FileType.Enf:
                        Mock.Get(_pubFileDeserializer)
                            .Setup(x => x.DeserializeFromByteArray(1, It.IsAny<byte[]>(), It.IsAny<Func<IPubFile<ENFRecord>>>()))
                            .Returns(new ENFFile());
                        aggEx = _fileRequestService.RequestFile<ENFRecord>(type, 1).Exception;
                        break;
                    case FileType.Esf:
                        Mock.Get(_pubFileDeserializer)
                            .Setup(x => x.DeserializeFromByteArray(1, It.IsAny<byte[]>(), It.IsAny<Func<IPubFile<ESFRecord>>>()))
                            .Returns(new ESFFile());
                        aggEx = _fileRequestService.RequestFile<ESFRecord>(type, 1).Exception;
                        break;
                    case FileType.Ecf:
                        Mock.Get(_pubFileDeserializer)
                            .Setup(x => x.DeserializeFromByteArray(1, It.IsAny<byte[]>(), It.IsAny<Func<IPubFile<ECFRecord>>>()))
                            .Returns(new ECFFile());
                        aggEx = _fileRequestService.RequestFile<ECFRecord>(type, 1).Exception;
                        break;
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
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader<AccountReplyServerPacket>();
            Assert.ThrowsAsync<EmptyPacketReceivedException>(async () => await _fileRequestService.RequestMapFile(1, 1));
        }

        [Test]
        public void RequestMapFile_ResponsePacketHasIncorrectFileType_ThrowsMalformedPacketException()
        {
            Mock.Get(_packetSendService).SetupReceivedPacketHasHeader<InitInitServerPacket>((byte)InitReply.FileEsf, 33);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _fileRequestService.RequestMapFile(1, 1));
        }

        [Test]
        public void RequestMapFile_SendsPacket_BasedOnSpecifiedMap()
        {
            var packetIsCorrect = false;
            Mock.Get(_packetSendService).Setup(x => x.SendEncodedPacketAndWaitAsync(It.IsAny<IPacket>())).Callback((IPacket packet) => packetIsCorrect = IsCorrectFileRequestPacket(packet, FileType.Emf));

            _fileRequestService.RequestMapFile(1, 1);

            Assert.That(packetIsCorrect, Is.True);
        }

        [Test]
        public void RequestMapFile_HasPlayerAndMapID()
        {
            const short PlayerID = 1234;
            const short MapID = 333;

            var packetIsCorrect = false;
            Mock.Get(_packetSendService)
                .Setup(x => x.SendEncodedPacketAndWaitAsync(It.IsAny<IPacket>()))
                .Callback((IPacket p) => packetIsCorrect = IsCorrectFileRequestPacket(p, FileType.Emf, PlayerID, MapID));

            _fileRequestService.RequestMapFile(MapID, PlayerID);

            Assert.That(packetIsCorrect, Is.True);
        }

        #endregion

        #region Helper Methods

        private static bool IsCorrectFileRequestPacket(IPacket packet, FileType type, short sessionId = 0, short mapId = 0)
        {
            var waPacket = packet as WelcomeAgreeClientPacket;
            var correctTyping = waPacket.FileType == type;

            var correctData = true;
            if (mapId > 0 && sessionId > 0)
            {
                var emfData = waPacket.FileTypeData as WelcomeAgreeClientPacket.FileTypeDataEmf;
                correctData = emfData.FileId == mapId && waPacket.SessionId == sessionId;
            }

            return correctTyping && correctData;
        }

        private static byte[] CreateFilePacket(FileType type)
        {
            var ret = new InitInitServerPacket();

            var nes = new NumberEncoderService();
            var rs = new PubRecordSerializer(nes);

            var eoWriter = new EoWriter();
            switch (type)
            {
                case FileType.Eif:
                    ret.ReplyCode = InitReply.FileEif;

                    eoWriter.AddString("EIF");
                    eoWriter.AddInt(1); // RID
                    eoWriter.AddShort(2); // length
                    eoWriter.AddByte(1); // version
                    eoWriter.AddBytes(rs.SerializeToByteArray(new EIFRecord().WithID(1).WithName("Test1")));
                    eoWriter.AddBytes(rs.SerializeToByteArray(new EIFRecord().WithID(2).WithName("eof")));

                    ret.ReplyCodeData = new InitInitServerPacket.ReplyCodeDataFileEif
                    {
                        PubFile = new PubFile
                        {
                            FileId = 1,
                            Content = eoWriter.ToByteArray()
                        }
                    };
                    break;
                case FileType.Enf:
                    ret.ReplyCode = InitReply.FileEnf;

                    eoWriter.AddString("ENF");
                    eoWriter.AddInt(1); // RID
                    eoWriter.AddShort(2); // length
                    eoWriter.AddByte(1); // version
                    eoWriter.AddBytes(rs.SerializeToByteArray(new ENFRecord().WithID(1).WithName("Test1")));
                    eoWriter.AddBytes(rs.SerializeToByteArray(new ENFRecord().WithID(2).WithName("eof")));

                    ret.ReplyCodeData = new InitInitServerPacket.ReplyCodeDataFileEnf
                    {
                        PubFile = new PubFile
                        {
                            FileId = 1,
                            Content = eoWriter.ToByteArray()
                        }
                    };
                    break;
                case FileType.Esf:
                    ret.ReplyCode = InitReply.FileEsf;

                    eoWriter.AddString("ESF");
                    eoWriter.AddInt(1); // RID
                    eoWriter.AddShort(2); // length
                    eoWriter.AddByte(1); // version
                    eoWriter.AddBytes(rs.SerializeToByteArray(new ESFRecord().WithID(1).WithName("Test1")));
                    eoWriter.AddBytes(rs.SerializeToByteArray(new ESFRecord().WithID(2).WithName("eof")));

                    ret.ReplyCodeData = new InitInitServerPacket.ReplyCodeDataFileEsf
                    {
                        PubFile = new PubFile
                        {
                            FileId = 1,
                            Content = eoWriter.ToByteArray()
                        }
                    };
                    break;
                case FileType.Ecf:
                    ret.ReplyCode = InitReply.FileEcf;

                    eoWriter.AddString("ECF");
                    eoWriter.AddInt(1); // RID
                    eoWriter.AddShort(2); // length
                    eoWriter.AddByte(1); // version
                    eoWriter.AddBytes(rs.SerializeToByteArray(new ECFRecord().WithID(1).WithName("Test1")));
                    eoWriter.AddBytes(rs.SerializeToByteArray(new ECFRecord().WithID(2).WithName("eof")));

                    ret.ReplyCodeData = new InitInitServerPacket.ReplyCodeDataFileEcf
                    {
                        PubFile = new PubFile
                        {
                            FileId = 1,
                            Content = eoWriter.ToByteArray()
                        }
                    };
                    break;
            }

            eoWriter = new EoWriter();
            ret.Serialize(eoWriter);
            return eoWriter.ToByteArray();
        }

        #endregion
    }
}