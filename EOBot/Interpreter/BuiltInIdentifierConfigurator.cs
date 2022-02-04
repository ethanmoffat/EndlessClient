using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;
using EOLib.Config;
using EOLib.Domain.Account;
using EOLib.Domain.Login;
using EOLib.Domain.Protocol;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.PacketProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EOBot.Interpreter
{
    public class BuiltInIdentifierConfigurator
    {
        private readonly ProgramState _state;
        private readonly int _botIndex;
        private readonly ArgumentsParser _parsedArgs;
        private readonly BotHelper _botHelper;

        public BuiltInIdentifierConfigurator(ProgramState state, int botIndex, ArgumentsParser parsedArgs)
        {
            _state = state;
            _botIndex = botIndex;
            _parsedArgs = parsedArgs;
            _botHelper = new BotHelper(_botIndex);
        }

        public void SetupBuiltInFunctions()
        {
            _state.SymbolTable[PredefinedIdentifiers.PRINT_FUNC] = Readonly(new VoidFunction<object>(PredefinedIdentifiers.PRINT_FUNC, param1 => ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, param1.ToString())));
            _state.SymbolTable[PredefinedIdentifiers.LEN_FUNC] = Readonly(new Function<ArrayVariable, int>(PredefinedIdentifiers.LEN_FUNC, param1 => param1.Value.Count));
            _state.SymbolTable[PredefinedIdentifiers.ARRAY_FUNC] = Readonly(new Function<int, List<IVariable>>(PredefinedIdentifiers.ARRAY_FUNC, param1 => Enumerable.Repeat(UndefinedVariable.Instance, param1).Cast<IVariable>().ToList()));
            _state.SymbolTable[PredefinedIdentifiers.SLEEP_FUNC] = Readonly(new VoidFunction<int>(PredefinedIdentifiers.SLEEP_FUNC, param1 => Thread.Sleep(param1)));
            _state.SymbolTable[PredefinedIdentifiers.TIME_FUNC] = Readonly(new Function<string>(PredefinedIdentifiers.TIME_FUNC, () => DateTime.Now.ToLongTimeString()));
            _state.SymbolTable[PredefinedIdentifiers.OBJECT_FUNC] = Readonly(new Function<ObjectVariable>(PredefinedIdentifiers.OBJECT_FUNC, () => new ObjectVariable()));

            BotDependencySetup();
            _state.SymbolTable[PredefinedIdentifiers.CONNECT_FUNC] = Readonly(new AsyncVoidFunction<string, int>(PredefinedIdentifiers.CONNECT_FUNC, ConnectAsync));
            _state.SymbolTable[PredefinedIdentifiers.DISCONNECT_FUNC] = Readonly(new VoidFunction(PredefinedIdentifiers.DISCONNECT_FUNC, Disconnect));
            _state.SymbolTable[PredefinedIdentifiers.CREATE_ACCOUNT_FUNC] = Readonly(new AsyncFunction<string, string, int>(PredefinedIdentifiers.CREATE_ACCOUNT_FUNC, CreateAccountAsync));
            _state.SymbolTable[PredefinedIdentifiers.LOGIN_FUNC] = Readonly(new AsyncFunction<string, string, int>(PredefinedIdentifiers.LOGIN_FUNC, LoginAsync));
            _state.SymbolTable[PredefinedIdentifiers.CREATE_AND_LOGIN_FUNC] = Readonly(new AsyncFunction<string, string, int>(PredefinedIdentifiers.CREATE_AND_LOGIN_FUNC, CreateAndLoginAsync));
            _state.SymbolTable[PredefinedIdentifiers.CHANGE_PASS_FUNC] = Readonly(new AsyncFunction<string, string, string, int>(PredefinedIdentifiers.CHANGE_PASS_FUNC, ChangePasswordAsync));
            _state.SymbolTable[PredefinedIdentifiers.CREATE_CHARACTER_FUNC] = Readonly(new AsyncFunction<string, int>(PredefinedIdentifiers.CREATE_CHARACTER_FUNC, CreateCharacterAsync));
            _state.SymbolTable[PredefinedIdentifiers.DELETE_CHARACTER_FUNC] = Readonly(new AsyncFunction<string, bool, int>(PredefinedIdentifiers.DELETE_CHARACTER_FUNC, DeleteCharacterAsync));
            _state.SymbolTable[PredefinedIdentifiers.LOGIN_CHARACTER_FUNC] = Readonly(new AsyncVoidFunction<string>(PredefinedIdentifiers.LOGIN_CHARACTER_FUNC, LoginToCharacterAsync));
        }
        public void SetupBuiltInVariables()
        {
            _state.SymbolTable[PredefinedIdentifiers.HOST] = (true, new StringVariable(_parsedArgs.Host));
            _state.SymbolTable[PredefinedIdentifiers.PORT] = (true, new IntVariable(_parsedArgs.Port));
            _state.SymbolTable[PredefinedIdentifiers.USER] = (true, new StringVariable(_parsedArgs.Account));
            _state.SymbolTable[PredefinedIdentifiers.PASS] = (true, new StringVariable(_parsedArgs.Password));
            _state.SymbolTable[PredefinedIdentifiers.BOTINDEX] = (true, new IntVariable(_botIndex));
            _state.SymbolTable[PredefinedIdentifiers.ARGS] = (true, new ArrayVariable(
                (_parsedArgs.UserArgs ?? new List<string>()).Select(x => new StringVariable(x)).Cast<IVariable>().ToList()));

            // default to version 0.0.28
            _state.SymbolTable[PredefinedIdentifiers.VERSION] = (false, new IntVariable(28));

            _state.SymbolTable[PredefinedIdentifiers.RESULT] = (false, UndefinedVariable.Instance);
            _state.SymbolTable[PredefinedIdentifiers.ACCOUNT] = SetupAccountObject();
            _state.SymbolTable[PredefinedIdentifiers.CHARACTER] = (true, UndefinedVariable.Instance);
            _state.SymbolTable[PredefinedIdentifiers.MAPSTATE] = (true, UndefinedVariable.Instance);
        }

        private static (bool, IIdentifiable) Readonly(IIdentifiable identifiable)
        {
            return (true, identifiable);
        }

        private void BotDependencySetup()
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];
            var networkClientRepository = c.Resolve<INetworkClientRepository>();
            var networkClientFactory = c.Resolve<INetworkClientFactory>();

            const int LongReceiveTimeout = 15000;
            networkClientRepository.NetworkClient = networkClientFactory.CreateNetworkClient(LongReceiveTimeout);
        }

        private async Task ConnectAsync(string host, int port)
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];

            var configRepo = c.Resolve<IConfigurationRepository>();
            configRepo.Host = host;
            configRepo.Port = port;

            configRepo.VersionBuild = (byte)((IntVariable)_state.SymbolTable[PredefinedIdentifiers.VERSION].Identifiable).Value;

            var connectionActions = c.Resolve<INetworkConnectionActions>();
            var connectResult = await connectionActions.ConnectToServer();
            if (connectResult != ConnectResult.Success)
                throw new ArgumentException($"Bot {_botIndex}: Unable to connect to server! Host={host} Port={port}");

            var backgroundReceiveActions = c.Resolve<IBackgroundReceiveActions>();
            backgroundReceiveActions.RunBackgroundReceiveLoop();

            var handshakeResult = await connectionActions.BeginHandshake();

            if (handshakeResult.Response != InitReply.Success)
                throw new InvalidOperationException($"Bot {_botIndex}: Invalid response from server or connection failed! Must receive an OK reply.");

            var packetProcessActions = c.Resolve<IPacketProcessActions>();

            packetProcessActions.SetInitialSequenceNumber(handshakeResult[InitializationDataKey.SequenceByte1],
                handshakeResult[InitializationDataKey.SequenceByte2]);
            packetProcessActions.SetEncodeMultiples((byte)handshakeResult[InitializationDataKey.ReceiveMultiple],
                (byte)handshakeResult[InitializationDataKey.SendMultiple]);

            connectionActions.CompleteHandshake(handshakeResult);
        }

        private void Disconnect()
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];
            var backgroundReceiveActions = c.Resolve<IBackgroundReceiveActions>();
            var connectionActions = c.Resolve<INetworkConnectionActions>();

            backgroundReceiveActions.CancelBackgroundReceiveLoop();
            connectionActions.DisconnectFromServer();
        }

        private async Task<int> CreateAccountAsync(string user, string pass)
        {
            return (int)await _botHelper.CreateAccountAsync(user, pass);
        }

        private async Task<int> LoginAsync(string user, string pass)
        {
            return (int)await _botHelper.LoginToAccountAsync(user, pass);
        }

        private async Task<int> CreateAndLoginAsync(string user, string pass)
        {
            var accountReply = (AccountReply)await CreateAccountAsync(user, pass);
            if (accountReply == AccountReply.Created || accountReply == AccountReply.Exists)
            {
                return await LoginAsync(user, pass);
            }

            return (int)LoginReply.WrongUser;
        }

        private async Task<int> ChangePasswordAsync(string user, string oldPass, string newPass)
        {
            return (int)await _botHelper.ChangePasswordAsync(user, oldPass, newPass);
        }

        private async Task<int> CreateCharacterAsync(string charName)
        {
            return (int)await _botHelper.CreateCharacterAsync(charName);
        }

        private async Task<int> DeleteCharacterAsync(string charName, bool force)
        {
            return (int)await _botHelper.DeleteCharacterAsync(charName, force);
        }

        private Task LoginToCharacterAsync(string charName)
        {
            return _botHelper.LoginToCharacterAsync(charName);
        }

        private (bool, IIdentifiable) SetupAccountObject()
        {
            var playerInfoProv = DependencyMaster.TypeRegistry[_botIndex].Resolve<IPlayerInfoProvider>();
            var charSelectProv = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterSelectorProvider>();

            var accountObj = new RuntimeEvaluatedMemberObjectVariable();
            accountObj.SymbolTable[PredefinedIdentifiers.NAME] = (true, () => new StringVariable(playerInfoProv.LoggedInAccountName));
            accountObj.SymbolTable[PredefinedIdentifiers.CHARACTERS] = (true,
                () => new ArrayVariable(
                    charSelectProv.Characters.Select(x =>
                    {
                        var retObj = new ObjectVariable();
                        retObj.SymbolTable[PredefinedIdentifiers.NAME] = Readonly(new StringVariable(x.Name));
                        return (IVariable)retObj;
                    }).ToList()));

            return Readonly(accountObj);
        }
    }
}
