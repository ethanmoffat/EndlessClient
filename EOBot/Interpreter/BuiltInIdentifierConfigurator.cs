using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.Domain.Party;
using EOLib.IO.Repositories;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.PacketProcessing;
using Moffat.EndlessOnline.SDK.Packet;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOBot.Interpreter
{
    public class BuiltInIdentifierConfigurator
    {
        private readonly ProgramState _state;
        private readonly int _botIndex;
        private readonly ArgumentsParser _parsedArgs;
        private readonly BotHelper _botHelper;
        private readonly Random _random;

        public BuiltInIdentifierConfigurator(ProgramState state, int botIndex, ArgumentsParser parsedArgs)
        {
            _state = state;
            _botIndex = botIndex;
            _parsedArgs = parsedArgs;
            _botHelper = new BotHelper(_botIndex);
            _random = new Random();
        }

        public void SetupBuiltInFunctions()
        {
            _state.SymbolTable[PredefinedIdentifiers.PRINT_FUNC] = Readonly(new VoidFunction<object>(PredefinedIdentifiers.PRINT_FUNC, param1 => ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, param1.ToString())));
            _state.SymbolTable[PredefinedIdentifiers.LEN_FUNC] = Readonly(new Function<ArrayVariable, int>(PredefinedIdentifiers.LEN_FUNC, param1 => param1.Value.Count));
            _state.SymbolTable[PredefinedIdentifiers.ARRAY_FUNC] = Readonly(new Function<int, List<IVariable>>(PredefinedIdentifiers.ARRAY_FUNC, param1 => Enumerable.Repeat(UndefinedVariable.Instance, param1).Cast<IVariable>().ToList()));
            _state.SymbolTable[PredefinedIdentifiers.SLEEP_FUNC] = Readonly(new VoidFunction<int>(PredefinedIdentifiers.SLEEP_FUNC, param1 => Thread.Sleep(param1)));
            _state.SymbolTable[PredefinedIdentifiers.TIME_FUNC] = Readonly(new Function<string>(PredefinedIdentifiers.TIME_FUNC, () => DateTime.Now.ToLongTimeString()));
            _state.SymbolTable[PredefinedIdentifiers.OBJECT_FUNC] = Readonly(new Function<ObjectVariable>(PredefinedIdentifiers.OBJECT_FUNC, () => new ObjectVariable()));
            _state.SymbolTable[PredefinedIdentifiers.SETENV_FUNC] = Readonly(new VoidFunction<string, string>(PredefinedIdentifiers.SETENV_FUNC, (varName, varValue) => Environment.SetEnvironmentVariable(varName, varValue, EnvironmentVariableTarget.User)));
            _state.SymbolTable[PredefinedIdentifiers.GETENV_FUNC] = Readonly(new Function<string, string>(PredefinedIdentifiers.GETENV_FUNC, varName => Environment.GetEnvironmentVariable(varName, EnvironmentVariableTarget.User)));
            _state.SymbolTable[PredefinedIdentifiers.ERROR_FUNC] = Readonly(new VoidFunction<string>(PredefinedIdentifiers.ERROR_FUNC, message => throw new BotScriptErrorException(message)));
            _state.SymbolTable[PredefinedIdentifiers.LOWER_FUNC] = Readonly(new Function<string, string>(PredefinedIdentifiers.LOWER_FUNC, s => s.ToLower()));
            _state.SymbolTable[PredefinedIdentifiers.UPPER_FUNC] = Readonly(new Function<string, string>(PredefinedIdentifiers.UPPER_FUNC, s => s.ToUpper()));

            BotDependencySetup();
            // pre-game flow
            _state.SymbolTable[PredefinedIdentifiers.CONNECT_FUNC] = Readonly(new AsyncVoidFunction<string, int>(PredefinedIdentifiers.CONNECT_FUNC, ConnectAsync));
            _state.SymbolTable[PredefinedIdentifiers.DISCONNECT_FUNC] = Readonly(new VoidFunction(PredefinedIdentifiers.DISCONNECT_FUNC, Disconnect));
            _state.SymbolTable[PredefinedIdentifiers.CREATE_ACCOUNT_FUNC] = Readonly(new AsyncFunction<string, string, int>(PredefinedIdentifiers.CREATE_ACCOUNT_FUNC, CreateAccountAsync));
            _state.SymbolTable[PredefinedIdentifiers.LOGIN_FUNC] = Readonly(new AsyncFunction<string, string, int>(PredefinedIdentifiers.LOGIN_FUNC, LoginAsync));
            _state.SymbolTable[PredefinedIdentifiers.CREATE_AND_LOGIN_FUNC] = Readonly(new AsyncFunction<string, string, int>(PredefinedIdentifiers.CREATE_AND_LOGIN_FUNC, CreateAndLoginAsync));
            _state.SymbolTable[PredefinedIdentifiers.CHANGE_PASS_FUNC] = Readonly(new AsyncFunction<string, string, string, int>(PredefinedIdentifiers.CHANGE_PASS_FUNC, ChangePasswordAsync));
            _state.SymbolTable[PredefinedIdentifiers.CREATE_CHARACTER_FUNC] = Readonly(new AsyncFunction<string, int>(PredefinedIdentifiers.CREATE_CHARACTER_FUNC, CreateCharacterAsync));
            _state.SymbolTable[PredefinedIdentifiers.DELETE_CHARACTER_FUNC] = Readonly(new AsyncFunction<string, bool, int>(PredefinedIdentifiers.DELETE_CHARACTER_FUNC, DeleteCharacterAsync));
            _state.SymbolTable[PredefinedIdentifiers.LOGIN_CHARACTER_FUNC] = Readonly(new AsyncVoidFunction<string>(PredefinedIdentifiers.LOGIN_CHARACTER_FUNC, LoginToCharacterAsync));

            // in-game stuff
            _state.SymbolTable[PredefinedIdentifiers.JOIN_PARTY] = Readonly(new VoidFunction<int>(PredefinedIdentifiers.JOIN_PARTY, JoinParty));
            _state.SymbolTable[PredefinedIdentifiers.CHAT] = Readonly(new VoidFunction<string>(PredefinedIdentifiers.CHAT, Chat));
        }

        public void SetupBuiltInVariables()
        {
            _state.SymbolTable[PredefinedIdentifiers.HOST] = Readonly(new StringVariable(_parsedArgs.Host));
            _state.SymbolTable[PredefinedIdentifiers.PORT] = Readonly(new IntVariable(_parsedArgs.Port));
            _state.SymbolTable[PredefinedIdentifiers.USER] = Readonly(new StringVariable(_parsedArgs.Account));
            _state.SymbolTable[PredefinedIdentifiers.PASS] = Readonly(new StringVariable(_parsedArgs.Password));
            _state.SymbolTable[PredefinedIdentifiers.BOTINDEX] = Readonly(new IntVariable(_botIndex));
            _state.SymbolTable[PredefinedIdentifiers.ARGS] = Readonly(new ArrayVariable(
                (_parsedArgs.UserArgs ?? new List<string>()).Select(x => new StringVariable(x)).Cast<IVariable>().ToList()));

            // default to version 0.0.28
            _state.SymbolTable[PredefinedIdentifiers.VERSION] = (false, new IntVariable(28));

            _state.SymbolTable[PredefinedIdentifiers.RESULT] = (false, UndefinedVariable.Instance);
            _state.SymbolTable[PredefinedIdentifiers.ACCOUNT] = SetupAccountObject();
            _state.SymbolTable[PredefinedIdentifiers.CHARACTER] = SetupCharacterObject();
            _state.SymbolTable[PredefinedIdentifiers.MAPSTATE] = SetupMapStateObject();
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

            configRepo.VersionBuild = ((IntVariable)_state.SymbolTable[PredefinedIdentifiers.VERSION].Identifiable).Value;

            var connectionActions = c.Resolve<INetworkConnectionActions>();
            var connectResult = await connectionActions.ConnectToServer();
            if (connectResult != ConnectResult.Success)
                throw new ArgumentException($"Bot {_botIndex}: Unable to connect to server! Host={host} Port={port}");

            var backgroundReceiveActions = c.Resolve<IBackgroundReceiveActions>();
            backgroundReceiveActions.RunBackgroundReceiveLoop();

            var handshakeResult = await connectionActions.BeginHandshake(_random.Next(Constants.MaxChallenge));

            if (handshakeResult.ReplyCode != InitReply.Ok)
                throw new InvalidOperationException($"Bot {_botIndex}: Invalid response from server or connection failed! Must receive an OK reply.");

            var handshakeData = (InitInitServerPacket.ReplyCodeDataOk)handshakeResult.ReplyCodeData;

            var packetProcessActions = c.Resolve<IPacketProcessActions>();
            packetProcessActions.SetSequenceStart(InitSequenceStart.FromInitValues(handshakeData.Seq1, handshakeData.Seq2));
            packetProcessActions.SetEncodeMultiples(handshakeData.ServerEncryptionMultiple, handshakeData.ClientEncryptionMultiple);

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

        private void JoinParty(int characterId)
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];
            c.Resolve<IPartyActions>().RequestParty(PartyRequestType.Join, characterId);
        }

        private void Chat(string chatText)
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];
            c.Resolve<IChatActions>().SendChatToServer(chatText, string.Empty, ChatType.Local);
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

        private (bool, IIdentifiable) SetupCharacterObject()
        {
            var cp = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterProvider>();
            var inventoryProvider = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterInventoryProvider>();
            var pubProvider = DependencyMaster.TypeRegistry[_botIndex].Resolve<IPubFileProvider>();

            var charObj = new RuntimeEvaluatedMemberObjectVariable();
            charObj.SymbolTable["id"] = (true, () => new IntVariable(cp.MainCharacter.ID));
            charObj.SymbolTable[PredefinedIdentifiers.NAME] = (true, () => new StringVariable(cp.MainCharacter.Name));
            charObj.SymbolTable["map"] = (true, () => new IntVariable(cp.MainCharacter.MapID));
            charObj.SymbolTable["x"] = (true, () => new IntVariable(cp.MainCharacter.RenderProperties.MapX));
            charObj.SymbolTable["y"] = (true, () => new IntVariable(cp.MainCharacter.RenderProperties.MapY));
            charObj.SymbolTable["direction"] = (true, () => new IntVariable((int)cp.MainCharacter.RenderProperties.Direction));
            charObj.SymbolTable["admin"] = (true, () => new IntVariable((int)cp.MainCharacter.AdminLevel));
            charObj.SymbolTable["inventory"] = (true,
                () => new ArrayVariable(
                    inventoryProvider.ItemInventory.Select(x =>
                    {
                        var itemName = pubProvider.EIFFile.Single(d => d.ID == x.ItemID).Name;

                        var retObj = new ObjectVariable();
                        retObj.SymbolTable[PredefinedIdentifiers.NAME] = Readonly(new StringVariable(itemName));
                        retObj.SymbolTable["id"] = Readonly(new IntVariable(x.ItemID));
                        retObj.SymbolTable["amount"] = Readonly(new IntVariable(x.Amount));
                        return (IVariable)retObj;
                    }).ToList()));
            charObj.SymbolTable["spells"] = (true,
                () => new ArrayVariable(
                    inventoryProvider.SpellInventory.Select(x =>
                    {
                        var spellName = pubProvider.ESFFile.Single(d => d.ID == x.ID).Name;

                        var retObj = new ObjectVariable();
                        retObj.SymbolTable[PredefinedIdentifiers.NAME] = Readonly(new StringVariable(spellName));
                        retObj.SymbolTable["id"] = Readonly(new IntVariable(x.ID));
                        retObj.SymbolTable["amount"] = Readonly(new IntVariable(x.Level));
                        return (IVariable)retObj;
                    }).ToList()));
            charObj.SymbolTable["stats"] = (true,
                () =>
                {
                    var statsObj = new ObjectVariable();
                    statsObj.SymbolTable["hp"] = Readonly(new IntVariable(cp.MainCharacter.Stats[CharacterStat.HP]));
                    statsObj.SymbolTable["maxhp"] = Readonly(new IntVariable(cp.MainCharacter.Stats[CharacterStat.MaxHP]));
                    statsObj.SymbolTable["weight"] = Readonly(new IntVariable(cp.MainCharacter.Stats[CharacterStat.Weight]));
                    statsObj.SymbolTable["maxweight"] = Readonly(new IntVariable(cp.MainCharacter.Stats[CharacterStat.MaxWeight]));
                    statsObj.SymbolTable["tp"] = Readonly(new IntVariable(cp.MainCharacter.Stats[CharacterStat.TP]));
                    statsObj.SymbolTable["maxtp"] = Readonly(new IntVariable(cp.MainCharacter.Stats[CharacterStat.MaxTP]));
                    return statsObj;
                }
            );

            return Readonly(charObj);
        }

        private (bool, IIdentifiable) SetupMapStateObject()
        {
            var ms = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICurrentMapStateProvider>();

            var mapStateObj = new RuntimeEvaluatedMemberObjectVariable();
            mapStateObj.SymbolTable["characters"] = (true, () => new ArrayVariable(ms.Characters.Select(GetMapStateCharacter).ToList()));
            mapStateObj.SymbolTable["npcs"] = (true, () => new ArrayVariable(ms.NPCs.Select(GetMapStateNPC).ToList()));
            mapStateObj.SymbolTable["items"] = (true, () => new ArrayVariable(ms.MapItems.Select(GetMapStateItem).ToList()));

            return Readonly(mapStateObj);
        }

        private IVariable GetMapStateCharacter(Character c)
        {
            var charObj = new ObjectVariable();
            charObj.SymbolTable["id"] = Readonly(new IntVariable(c.ID));
            charObj.SymbolTable[PredefinedIdentifiers.NAME] = Readonly(new StringVariable(c.Name));
            charObj.SymbolTable["map"] = Readonly(new IntVariable(c.MapID));
            charObj.SymbolTable["x"] = Readonly(new IntVariable(c.RenderProperties.MapX));
            charObj.SymbolTable["y"] = Readonly(new IntVariable(c.RenderProperties.MapY));
            charObj.SymbolTable["direction"] = Readonly(new IntVariable((int)c.RenderProperties.Direction));
            return charObj;
        }

        private IVariable GetMapStateNPC(NPC npc)
        {
            var npcFile = DependencyMaster.TypeRegistry[_botIndex].Resolve<IPubFileProvider>().ENFFile;

            var npcObj = new ObjectVariable();
            npcObj.SymbolTable[PredefinedIdentifiers.NAME] = Readonly(new StringVariable(npcFile.Single(x => x.ID == npc.ID).Name));
            npcObj.SymbolTable["x"] = Readonly(new IntVariable(npc.X));
            npcObj.SymbolTable["y"] = Readonly(new IntVariable(npc.Y));
            npcObj.SymbolTable["id"] = Readonly(new IntVariable(npc.ID));
            npcObj.SymbolTable["direction"] = Readonly(new IntVariable((int)npc.Direction));
            return npcObj;
        }

        private IVariable GetMapStateItem(MapItem item)
        {
            var itemFile = DependencyMaster.TypeRegistry[_botIndex].Resolve<IPubFileProvider>().EIFFile;

            var itemObj = new ObjectVariable();
            itemObj.SymbolTable[PredefinedIdentifiers.NAME] = Readonly(new StringVariable(itemFile.Single(x => x.ID == item.ItemID).Name));
            itemObj.SymbolTable["x"] = Readonly(new IntVariable(item.X));
            itemObj.SymbolTable["y"] = Readonly(new IntVariable(item.Y));
            itemObj.SymbolTable["id"] = Readonly(new IntVariable(item.ItemID));
            itemObj.SymbolTable["amount"] = Readonly(new IntVariable(item.Amount));
            return itemObj;
        }
    }
}