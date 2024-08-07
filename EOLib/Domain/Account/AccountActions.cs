using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.PacketProcessing;
using Moffat.EndlessOnline.SDK.Packet;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Account
{
    [AutoMappedType]
    public class AccountActions : IAccountActions
    {
        private readonly ICreateAccountParameterValidator _createAccountParameterValidator;
        private readonly IPacketSendService _packetSendService;
        private readonly IHDSerialNumberService _hdSerialNumberService;
        private readonly ISequenceRepository _sequenceRepository;
        private readonly IPlayerInfoRepository _playerInfoRepository;

        public AccountActions(ICreateAccountParameterValidator createAccountParameterValidator,
                              IPacketSendService packetSendService,
                              IHDSerialNumberService hdSerialNumberService,
                              ISequenceRepository sequenceRepository,
                              IPlayerInfoRepository playerInfoRepository)
        {
            _createAccountParameterValidator = createAccountParameterValidator;
            _packetSendService = packetSendService;
            _hdSerialNumberService = hdSerialNumberService;
            _sequenceRepository = sequenceRepository;
            _playerInfoRepository = playerInfoRepository;
        }

        public CreateAccountParameterResult CheckAccountCreateParameters(ICreateAccountParameters parameters)
        {
            if (AnyFieldsStillEmpty(parameters))
                return new CreateAccountParameterResult(WhichParameter.All, DialogResourceID.ACCOUNT_CREATE_FIELDS_STILL_EMPTY);

            if (_createAccountParameterValidator.AccountNameIsNotLongEnough(parameters.AccountName))
                return new CreateAccountParameterResult(WhichParameter.AccountName, DialogResourceID.ACCOUNT_CREATE_NAME_TOO_SHORT);

            if (_createAccountParameterValidator.AccountNameIsTooObvious(parameters.AccountName))
                return new CreateAccountParameterResult(WhichParameter.AccountName, DialogResourceID.ACCOUNT_CREATE_NAME_TOO_OBVIOUS);

            if (_createAccountParameterValidator.PasswordMismatch(parameters.Password, parameters.ConfirmPassword))
                return new CreateAccountParameterResult(WhichParameter.Confirm, DialogResourceID.ACCOUNT_CREATE_PASSWORD_MISMATCH);

            if (_createAccountParameterValidator.PasswordIsTooShort(parameters.Password))
                return new CreateAccountParameterResult(WhichParameter.Password, DialogResourceID.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);

            if (_createAccountParameterValidator.PasswordIsTooObvious(parameters.Password))
                return new CreateAccountParameterResult(WhichParameter.Password, DialogResourceID.ACCOUNT_CREATE_PASSWORD_TOO_OBVIOUS);

            if (_createAccountParameterValidator.EmailIsInvalid(parameters.Email))
                return new CreateAccountParameterResult(WhichParameter.Email, DialogResourceID.ACCOUNT_CREATE_EMAIL_INVALID);

            return new CreateAccountParameterResult(WhichParameter.None);
        }

        public async Task<AccountReply> CheckAccountNameWithServer(string accountName)
        {
            var nameCheckPacket = new AccountRequestClientPacket { Username = accountName };

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(nameCheckPacket);
            if (IsInvalidResponse(response, out var responsePacket))
                throw new EmptyPacketReceivedException();

            if (responsePacket.ReplyCode > (AccountReply)9)
            {
                var defaultData = (AccountReplyServerPacket.ReplyCodeDataDefault)responsePacket.ReplyCodeData;
                _sequenceRepository.Sequencer = _sequenceRepository.Sequencer.WithSequenceStart(AccountReplySequenceStart.FromValue(defaultData.SequenceStart));
            }

            return responsePacket.ReplyCode;
        }

        public async Task<AccountReply> CreateAccount(ICreateAccountParameters parameters, int sessionID)
        {
            var createAccountPacket = new AccountCreateClientPacket
            {
                SessionId = sessionID,
                Username = parameters.AccountName,
                Password = parameters.Password,
                FullName = parameters.RealName,
                Location = parameters.Location,
                Email = parameters.Email,
                Computer = Dns.GetHostName(),
                Hdid = _hdSerialNumberService.GetHDSerialNumber(),
            };

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(createAccountPacket);
            if (IsInvalidResponse(response, out var responsePacket))
                throw new EmptyPacketReceivedException();

            return responsePacket.ReplyCode;
        }

        public async Task<AccountReply> ChangePassword(IChangePasswordParameters parameters)
        {
            var changePasswordPacket = new AccountAgreeClientPacket
            {
                Username = parameters.AccountName,
                OldPassword = parameters.OldPassword,
                NewPassword = parameters.NewPassword,
            };

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(changePasswordPacket);
            if (IsInvalidResponse(response, out var responsePacket))
                throw new EmptyPacketReceivedException();

            return responsePacket.ReplyCode;
        }

        private bool AnyFieldsStillEmpty(ICreateAccountParameters parameters)
        {
            return new[]
            {
                parameters.AccountName,
                parameters.Password,
                parameters.ConfirmPassword,
                parameters.RealName,
                parameters.Location,
                parameters.Email
            }.Any(string.IsNullOrWhiteSpace);
        }

        private bool IsInvalidResponse(IPacket response, out AccountReplyServerPacket responsePacket)
        {
            responsePacket = response as AccountReplyServerPacket;
            return responsePacket != null && response.Family != PacketFamily.Account || response.Action != PacketAction.Reply;
        }
    }

    public interface IAccountActions
    {
        CreateAccountParameterResult CheckAccountCreateParameters(ICreateAccountParameters createAccountCreateparameters);

        Task<AccountReply> CheckAccountNameWithServer(string accountName);

        Task<AccountReply> CreateAccount(ICreateAccountParameters parameters, int sessionID);

        Task<AccountReply> ChangePassword(IChangePasswordParameters parameters);
    }
}
