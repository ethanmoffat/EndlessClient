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
            var printFunc = new VoidFunctionRef<object>(PredefinedIdentifiers.PRINT_FUNC, param1 => ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, param1.ToString()));
            _state.SymbolTable[PredefinedIdentifiers.PRINT_FUNC] = (true, printFunc);

            var lenFunc = new FunctionRef<ArrayVariable, int>(PredefinedIdentifiers.LEN_FUNC, param1 => param1.Value.Count);
            _state.SymbolTable[PredefinedIdentifiers.LEN_FUNC] = (true, lenFunc);

            var arrayFunc = new FunctionRef<int, List<IVariable>>(PredefinedIdentifiers.ARRAY_FUNC,
                param1 => Enumerable.Repeat(UndefinedVariable.Instance, param1).Cast<IVariable>().ToList());
            _state.SymbolTable[PredefinedIdentifiers.ARRAY_FUNC] = (true, arrayFunc);

            BotSetup();
            _state.SymbolTable[PredefinedIdentifiers.CONNECT_FUNC] = (true, new VoidFunctionRef<string, int>(PredefinedIdentifiers.CONNECT_FUNC, ConnectDefinition));
            _state.SymbolTable[PredefinedIdentifiers.DISCONNECT_FUNC] = (true, new VoidFunction(PredefinedIdentifiers.DISCONNECT_FUNC, DisconnectDefinition));
            _state.SymbolTable[PredefinedIdentifiers.CREATE_ACCOUNT_FUNC] = (true, new FunctionRef<string, string, int>(PredefinedIdentifiers.CREATE_ACCOUNT_FUNC, (user, pass) => (int)_botHelper.CreateAccountAsync(user, pass).GetAwaiter().GetResult()));
            _state.SymbolTable[PredefinedIdentifiers.LOGIN_FUNC] = (true, new FunctionRef<string, string, int>(PredefinedIdentifiers.LOGIN_FUNC, (user, pass) => (int)_botHelper.LoginToAccountAsync(user, pass).GetAwaiter().GetResult()));
            _state.SymbolTable[PredefinedIdentifiers.CREATE_AND_LOGIN_FUNC] = (true, new FunctionRef<string, string, int>(PredefinedIdentifiers.CREATE_AND_LOGIN_FUNC, CreateAndLoginDefinition));
            _state.SymbolTable[PredefinedIdentifiers.CHANGE_PASS_FUNC] = (true, new FunctionRef<string, string, string, int>(PredefinedIdentifiers.CHANGE_PASS_FUNC, (user, oldPass, newPass) => (int)_botHelper.ChangePasswordAsync(user, oldPass, newPass).GetAwaiter().GetResult()));
            _state.SymbolTable[PredefinedIdentifiers.CREATE_CHARACTER_FUNC] = (true, new FunctionRef<string, int>(PredefinedIdentifiers.CREATE_CHARACTER_FUNC, name => (int)_botHelper.CreateCharacterAsync(name).GetAwaiter().GetResult()));
            _state.SymbolTable[PredefinedIdentifiers.DELETE_CHARACTER_FUNC] = (true, new FunctionRef<string, bool, int>(PredefinedIdentifiers.DELETE_CHARACTER_FUNC, (name, force) => (int)_botHelper.DeleteCharacterAsync(name, force).GetAwaiter().GetResult()));
            _state.SymbolTable[PredefinedIdentifiers.LOGIN_CHARACTER_FUNC] = (true, new VoidFunctionRef<string>(PredefinedIdentifiers.LOGIN_CHARACTER_FUNC, name => _botHelper.LoginToCharacterAsync(name).GetAwaiter().GetResult()));
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
            _state.SymbolTable[PredefinedIdentifiers.ACCOUNT] = (true, UndefinedVariable.Instance);
            _state.SymbolTable[PredefinedIdentifiers.CHARACTER] = (true, UndefinedVariable.Instance);
            _state.SymbolTable[PredefinedIdentifiers.MAPSTATE] = (true, UndefinedVariable.Instance);
        }

        private void BotSetup()
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];
            var networkClientRepository = c.Resolve<INetworkClientRepository>();
            var networkClientFactory = c.Resolve<INetworkClientFactory>();
            networkClientRepository.NetworkClient = networkClientFactory.CreateNetworkClient();
        }

        private void ConnectDefinition(string host, int port)
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];

            var configRepo = c.Resolve<IConfigurationRepository>();
            configRepo.Host = host;
            configRepo.Port = port;

            configRepo.VersionBuild = (byte)((IntVariable)_state.SymbolTable[PredefinedIdentifiers.VERSION].Identifiable).Value;

            var connectionActions = c.Resolve<INetworkConnectionActions>();
            var connectResult = connectionActions.ConnectToServer().GetAwaiter().GetResult();
            if (connectResult != ConnectResult.Success)
                throw new ArgumentException($"Bot {_botIndex}: Unable to connect to server! Host={host} Port={port}");

            var backgroundReceiveActions = c.Resolve<IBackgroundReceiveActions>();
            backgroundReceiveActions.RunBackgroundReceiveLoop();

            var handshakeResult = connectionActions.BeginHandshake().GetAwaiter().GetResult();

            if (handshakeResult.Response != InitReply.Success)
                throw new InvalidOperationException($"Bot {_botIndex}: Invalid response from server or connection failed! Must receive an OK reply.");

            var packetProcessActions = c.Resolve<IPacketProcessActions>();

            packetProcessActions.SetInitialSequenceNumber(handshakeResult[InitializationDataKey.SequenceByte1],
                handshakeResult[InitializationDataKey.SequenceByte2]);
            packetProcessActions.SetEncodeMultiples((byte)handshakeResult[InitializationDataKey.ReceiveMultiple],
                (byte)handshakeResult[InitializationDataKey.SendMultiple]);

            connectionActions.CompleteHandshake(handshakeResult);
        }

        private void DisconnectDefinition()
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];
            var backgroundReceiveActions = c.Resolve<IBackgroundReceiveActions>();
            var connectionActions = c.Resolve<INetworkConnectionActions>();

            backgroundReceiveActions.CancelBackgroundReceiveLoop();
            connectionActions.DisconnectFromServer();
        }

        private int CreateAndLoginDefinition(string user, string pass)
        {
            var accountReply = _botHelper.CreateAccountAsync(user, pass).GetAwaiter().GetResult();
            if (accountReply == AccountReply.Created || accountReply == AccountReply.Exists)
            {
                return (int)_botHelper.LoginToAccountAsync(user, pass).GetAwaiter().GetResult();
            }

            return (int)LoginReply.WrongUser;
        }
    }
}
