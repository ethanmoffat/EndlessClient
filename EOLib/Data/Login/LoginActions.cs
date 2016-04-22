// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.Translators;

namespace EOLib.Data.Login
{
	public class LoginActions : ILoginActions
	{
		private readonly IPacketSendService _packetSendService;
		private readonly IPacketTranslator<IAccountLoginData> _loginPacketTranslator;
		private readonly ICharacterSelectorRepository _characterSelectorRepository;

		public LoginActions(IPacketSendService packetSendService,
							IPacketTranslator<IAccountLoginData> loginPacketTranslator,
							ICharacterSelectorRepository characterSelectorRepository)
		{
			_packetSendService = packetSendService;
			_loginPacketTranslator = loginPacketTranslator;
			_characterSelectorRepository = characterSelectorRepository;
		}

		public bool LoginParametersAreValid(ILoginParameters parameters)
		{
			return !string.IsNullOrEmpty(parameters.Username) &&
			       !string.IsNullOrEmpty(parameters.Password);
		}

		public async Task<LoginReply> LoginToServer(ILoginParameters parameters)
		{
			var packet = new PacketBuilder(PacketFamily.Login, PacketAction.Request)
				.AddBreakString(parameters.Username)
				.AddBreakString(parameters.Password)
				.Build();

			var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
			if (IsInvalidResponse(response))
				throw new EmptyPacketReceivedException();

			var data = _loginPacketTranslator.TranslatePacket(response);
			_characterSelectorRepository.Characters = data.Characters;
			return data.Response;
		}

		private bool IsInvalidResponse(IPacket response)
		{
			return response.Family != PacketFamily.Login || response.Action != PacketAction.Reply;
		}
	}
}