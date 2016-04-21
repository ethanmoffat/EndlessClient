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

		public LoginActions(IPacketSendService packetSendService,
							IPacketTranslator<IAccountLoginData> loginPacketTranslator)
		{
			_packetSendService = packetSendService;
			_loginPacketTranslator = loginPacketTranslator;
		}

		public bool LoginParametersAreValid(ILoginParameters parameters)
		{
			return !string.IsNullOrEmpty(parameters.Username) &&
			       !string.IsNullOrEmpty(parameters.Password);
		}

		public async Task<IAccountLoginData> LoginToServer(ILoginParameters parameters)
		{
			var packet = new PacketBuilder(PacketFamily.Login, PacketAction.Request)
				.AddBreakString(parameters.Username)
				.AddBreakString(parameters.Password)
				.Build();

			var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
			if (IsInvalidResponse(response))
				throw new EmptyPacketReceivedException();
			return _loginPacketTranslator.TranslatePacket(response);
		}

		private bool IsInvalidResponse(IPacket response)
		{
			return response.Family != PacketFamily.Login && response.Action != PacketAction.Reply;
		}
	}
}