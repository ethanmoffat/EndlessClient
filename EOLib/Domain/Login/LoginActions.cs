// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.BLL;
using EOLib.Domain.Character;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.Translators;

namespace EOLib.Domain.Login
{
	public class LoginActions : ILoginActions
	{
		private readonly IPacketSendService _packetSendService;
		private readonly IPacketTranslator<IAccountLoginData> _loginPacketTranslator;
		private readonly IPacketTranslator<ILoginRequestGrantedData> _loginRequestGrantedPacketTranslator;
		private readonly IPacketTranslator<ILoginRequestCompletedData> _loginRequestCompletedPacketTranslator;
		private readonly ICharacterSelectorRepository _characterSelectorRepository;
		private readonly ILoggedInAccountNameRepository _loggedInAccountNameRepository;
		private readonly ICharacterRepository _characterRepository;
		private readonly IPubFileProvider _pubFileProvider;

		public LoginActions(IPacketSendService packetSendService,
							IPacketTranslator<IAccountLoginData> loginPacketTranslator,
							IPacketTranslator<ILoginRequestGrantedData> loginRequestGrantedPacketTranslator,
							IPacketTranslator<ILoginRequestCompletedData> loginRequestCompletedPacketTranslator,
							ICharacterSelectorRepository characterSelectorRepository,
							ILoggedInAccountNameRepository loggedInAccountNameRepository,
							ICharacterRepository characterRepository,
							IPubFileProvider pubFileProvider)
		{
			_packetSendService = packetSendService;
			_loginPacketTranslator = loginPacketTranslator;
			_loginRequestGrantedPacketTranslator = loginRequestGrantedPacketTranslator;
			_loginRequestCompletedPacketTranslator = loginRequestCompletedPacketTranslator;
			_characterSelectorRepository = characterSelectorRepository;
			_loggedInAccountNameRepository = loggedInAccountNameRepository;
			_characterRepository = characterRepository;
			_pubFileProvider = pubFileProvider;
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

			if (data.Response == LoginReply.Ok)
				_loggedInAccountNameRepository.LoggedInAccountName = parameters.Username;

			return data.Response;
		}

		public async Task<ILoginRequestGrantedData> RequestCharacterLogin(ICharacter character)
		{
			var packet = new PacketBuilder(PacketFamily.Welcome, PacketAction.Request)
				.AddInt(character.ID)
				.Build();

			var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
			if (IsInvalidWelcome(response))
				throw new EmptyPacketReceivedException();

			var data = _loginRequestGrantedPacketTranslator.TranslatePacket(response);

			_characterRepository.ActiveCharacter = character
				.WithID(data.CharacterID)
				.WithName(data.Name)
				.WithTitle(data.Title)
				.WithGuildName(data.GuildName)
				.WithGuildRank(data.GuildRank)
				.WithGuildTag(data.GuildTag)
				.WithClassID(data.ClassID)
				.WithAdminLevel(data.AdminLevel)
				.WithStats(data.CharacterStats)
				.WithPaperdoll(data.Paperdoll);

			return data;
		}

		public async Task<ILoginRequestCompletedData> CompleteCharacterLogin(ICharacter character)
		{
			var packet = new PacketBuilder(PacketFamily.Welcome, PacketAction.Message)
				.AddThree(0x00123456) //?
				.AddInt(character.ID)
				.Build();

			var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
			if (IsInvalidWelcome(response))
				throw new EmptyPacketReceivedException();

			//todo: put data into required repositories
			return _loginRequestCompletedPacketTranslator.TranslatePacket(response);
		}

		private bool IsInvalidResponse(IPacket response)
		{
			return response.Family != PacketFamily.Login || response.Action != PacketAction.Reply;
		}

		private bool IsInvalidWelcome(IPacket response)
		{
			return response.Family != PacketFamily.Welcome || response.Action != PacketAction.Reply;
		}
	}
}