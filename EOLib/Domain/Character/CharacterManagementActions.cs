// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.Translators;

namespace EOLib.Domain.Character
{
	public class CharacterManagementActions : ICharacterManagementActions
	{
		private readonly IPacketSendService _packetSendService;
		private readonly IPacketTranslator<ICharacterCreateData> _characterCreatePacketTranslator;

		public CharacterManagementActions(IPacketSendService packetSendService,
										  IPacketTranslator<ICharacterCreateData> characterCreatePacketTranslator)
		{
			_packetSendService = packetSendService;
			_characterCreatePacketTranslator = characterCreatePacketTranslator;
		}

		public async Task<CharacterReply> RequestCharacterCreation()
		{
			var packet = new PacketBuilder(PacketFamily.Character, PacketAction.Request).Build();
			var responsePacket = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
			return (CharacterReply)responsePacket.ReadShort();
		}

		public async Task<ICharacterCreateData> CreateCharacter(ICharacterCreateParameters parameters)
		{
			var packet = new PacketBuilder(PacketFamily.Character, PacketAction.Create)
				.AddShort(255)
				.AddShort((short)parameters.Gender)
				.AddShort((short)parameters.HairStyle)
				.AddShort((short)parameters.HairColor)
				.AddShort((short)parameters.Race)
				.AddByte(255)
				.AddBreakString(parameters.Name)
				.Build();
			var responsePacket = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
			return _characterCreatePacketTranslator.TranslatePacket(responsePacket);
		}
	}
}