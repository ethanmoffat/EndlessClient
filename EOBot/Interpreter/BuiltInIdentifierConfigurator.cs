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
using EOLib.Domain.Extensions;
using EOLib.Domain.Item;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.Domain.Party;
using EOLib.Domain.Pathing;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;
using EOLib.Shared;
using Moffat.EndlessOnline.SDK.Packet;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOBot.Interpreter
{
    public class BuiltInIdentifierConfigurator
    {
        private const int ATTACK_BACKOFF_MS = 600;
        private const int WALK_BACKOFF_MS = 720;
        private const int FACE_BACKOFF_MS = 120;

        private readonly int _botIndex;
        private readonly ArgumentsParser _parsedArgs;
        private readonly BotHelper _botHelper;
        private readonly Random _random;

        public BuiltInIdentifierConfigurator(int botIndex, ArgumentsParser parsedArgs)
        {
            _botIndex = botIndex;
            _parsedArgs = parsedArgs;
            _botHelper = new BotHelper(_botIndex);
            _random = new Random();
        }

        public void SetupBuiltInFunctions(ProgramState programState)
        {
            programState.SymbolTable[PredefinedIdentifiers.PRINT_FUNC] = Readonly(new VoidFunction<object>(PredefinedIdentifiers.PRINT_FUNC, param1 => ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, param1.ToString())));
            programState.SymbolTable[PredefinedIdentifiers.LEN_FUNC] = Readonly(new Function<ArrayVariable, int>(PredefinedIdentifiers.LEN_FUNC, param1 => param1.Value.Count));
            programState.SymbolTable[PredefinedIdentifiers.ARRAY_FUNC] = Readonly(new Function<int, List<IVariable>>(PredefinedIdentifiers.ARRAY_FUNC, param1 => Enumerable.Repeat(UndefinedVariable.Instance, param1).Cast<IVariable>().ToList()));
            programState.SymbolTable[PredefinedIdentifiers.DICT_FUNC] = Readonly(new Function<Dictionary<string, IVariable>>(PredefinedIdentifiers.DICT_FUNC, () => []));
            programState.SymbolTable[PredefinedIdentifiers.APPEND_FUNC] = Readonly(new VoidFunction<ArrayVariable, IVariable>(PredefinedIdentifiers.APPEND_FUNC, (array, var) => array.Value.Add(var)));
            programState.SymbolTable[PredefinedIdentifiers.CLEAR_FUNC] = Readonly(new VoidFunction<ArrayVariable>(PredefinedIdentifiers.CLEAR_FUNC, array => array.Value.Clear()));
            programState.SymbolTable[PredefinedIdentifiers.SLEEP_FUNC] = Readonly(new AsyncVoidFunction<int>(PredefinedIdentifiers.SLEEP_FUNC, Task.Delay));
            programState.SymbolTable[PredefinedIdentifiers.TIME_FUNC] = Readonly(new Function<string>(PredefinedIdentifiers.TIME_FUNC, DateTime.Now.ToLongTimeString));
            programState.SymbolTable[PredefinedIdentifiers.OBJECT_FUNC] = Readonly(new Function<ObjectVariable>(PredefinedIdentifiers.OBJECT_FUNC, () => new ObjectVariable()));
            programState.SymbolTable[PredefinedIdentifiers.SETENV_FUNC] = Readonly(new VoidFunction<string, string>(PredefinedIdentifiers.SETENV_FUNC, (varName, varValue) => Environment.SetEnvironmentVariable(varName, varValue, EnvironmentVariableTarget.User)));
            programState.SymbolTable[PredefinedIdentifiers.GETENV_FUNC] = Readonly(new Function<string, string>(PredefinedIdentifiers.GETENV_FUNC, varName => Environment.GetEnvironmentVariable(varName, EnvironmentVariableTarget.User)));
            programState.SymbolTable[PredefinedIdentifiers.ERROR_FUNC] = Readonly(new VoidFunction<string>(PredefinedIdentifiers.ERROR_FUNC, message => throw new BotScriptErrorException(message)));
            programState.SymbolTable[PredefinedIdentifiers.LOWER_FUNC] = Readonly(new Function<string, string>(PredefinedIdentifiers.LOWER_FUNC, s => s.ToLower()));
            programState.SymbolTable[PredefinedIdentifiers.UPPER_FUNC] = Readonly(new Function<string, string>(PredefinedIdentifiers.UPPER_FUNC, s => s.ToUpper()));
            programState.SymbolTable[PredefinedIdentifiers.RAND_FUNC] = Readonly(new Function<int, int, int>(PredefinedIdentifiers.RAND_FUNC, (min, max) => min + _random.Next(max) % (max - min)));
            programState.SymbolTable[PredefinedIdentifiers.ABS_FUNC] = Readonly(new Function<int, int>(PredefinedIdentifiers.ABS_FUNC, Math.Abs));

            BotDependencySetup();

            // pre-game flow
            programState.SymbolTable[PredefinedIdentifiers.CONNECT_FUNC] = Readonly(new AsyncVoidFunction<string, int>(PredefinedIdentifiers.CONNECT_FUNC, (host, port, ct) => ConnectAsync(programState, host, port, ct)));
            programState.SymbolTable[PredefinedIdentifiers.DISCONNECT_FUNC] = Readonly(new VoidFunction(PredefinedIdentifiers.DISCONNECT_FUNC, Disconnect));
            programState.SymbolTable[PredefinedIdentifiers.CREATE_ACCOUNT_FUNC] = Readonly(new AsyncFunction<string, string, int>(PredefinedIdentifiers.CREATE_ACCOUNT_FUNC, CreateAccountAsync));
            programState.SymbolTable[PredefinedIdentifiers.LOGIN_FUNC] = Readonly(new AsyncFunction<string, string, int>(PredefinedIdentifiers.LOGIN_FUNC, LoginAsync));
            programState.SymbolTable[PredefinedIdentifiers.CREATE_AND_LOGIN_FUNC] = Readonly(new AsyncFunction<string, string, int>(PredefinedIdentifiers.CREATE_AND_LOGIN_FUNC, CreateAndLoginAsync));
            programState.SymbolTable[PredefinedIdentifiers.CHANGE_PASS_FUNC] = Readonly(new AsyncFunction<string, string, string, int>(PredefinedIdentifiers.CHANGE_PASS_FUNC, ChangePasswordAsync));
            programState.SymbolTable[PredefinedIdentifiers.CREATE_CHARACTER_FUNC] = Readonly(new AsyncFunction<string, int>(PredefinedIdentifiers.CREATE_CHARACTER_FUNC, CreateCharacterAsync));
            programState.SymbolTable[PredefinedIdentifiers.DELETE_CHARACTER_FUNC] = Readonly(new AsyncFunction<string, bool, int>(PredefinedIdentifiers.DELETE_CHARACTER_FUNC, DeleteCharacterAsync));
            programState.SymbolTable[PredefinedIdentifiers.LOGIN_CHARACTER_FUNC] = Readonly(new AsyncVoidFunction<string>(PredefinedIdentifiers.LOGIN_CHARACTER_FUNC, LoginToCharacterAsync));

            // game flow
            programState.SymbolTable[PredefinedIdentifiers.TICK] = Readonly(new AsyncVoidFunction(PredefinedIdentifiers.TICK, ct => Tick()));
            programState.SymbolTable[PredefinedIdentifiers.GETPATHTO] = Readonly(new Function<int, int, List<IVariable>>(PredefinedIdentifiers.GETPATHTO, GetPathTo));

            // in-game stuff
            programState.SymbolTable[PredefinedIdentifiers.JOIN_PARTY] = Readonly(new VoidFunction<int>(PredefinedIdentifiers.JOIN_PARTY, JoinParty));
            programState.SymbolTable[PredefinedIdentifiers.CHAT] = Readonly(new AsyncVoidFunction<string>(PredefinedIdentifiers.CHAT, Chat));

            // character inputs
            programState.SymbolTable[PredefinedIdentifiers.FACE] = Readonly(new AsyncVoidFunction<int>(PredefinedIdentifiers.FACE, Face));
            programState.SymbolTable[PredefinedIdentifiers.WALK] = Readonly(new AsyncVoidFunction(PredefinedIdentifiers.WALK, Walk));
            programState.SymbolTable[PredefinedIdentifiers.ATTACK] = Readonly(new AsyncVoidFunction(PredefinedIdentifiers.ATTACK, Attack));
            programState.SymbolTable[PredefinedIdentifiers.SIT] = Readonly(new AsyncVoidFunction(PredefinedIdentifiers.SIT, Sit));

            // items
            programState.SymbolTable[PredefinedIdentifiers.USEITEM] = Readonly(new AsyncVoidFunction<int>(PredefinedIdentifiers.USEITEM, UseItem));
            programState.SymbolTable[PredefinedIdentifiers.DROP] = Readonly(new AsyncVoidFunction<int, int>(PredefinedIdentifiers.DROP, Drop));
            programState.SymbolTable[PredefinedIdentifiers.PICKUP] = Readonly(new AsyncVoidFunction<int>(PredefinedIdentifiers.PICKUP, Pickup));
            programState.SymbolTable[PredefinedIdentifiers.JUNK] = Readonly(new AsyncVoidFunction<int, int>(PredefinedIdentifiers.JUNK, Junk));
        }

        public void SetupBuiltInVariables(ProgramState programState)
        {
            programState.SymbolTable[PredefinedIdentifiers.HOST] = Readonly(new StringVariable(_parsedArgs.Host));
            programState.SymbolTable[PredefinedIdentifiers.PORT] = Readonly(new IntVariable(_parsedArgs.Port));
            programState.SymbolTable[PredefinedIdentifiers.USER] = Readonly(new StringVariable(_parsedArgs.Account));
            programState.SymbolTable[PredefinedIdentifiers.PASS] = Readonly(new StringVariable(_parsedArgs.Password));
            programState.SymbolTable[PredefinedIdentifiers.BOTINDEX] = Readonly(new IntVariable(_botIndex));
            programState.SymbolTable[PredefinedIdentifiers.ARGS] = Readonly(new ArrayVariable(
                (_parsedArgs.UserArgs ?? []).Select(x => (IVariable)new StringVariable(x)).ToList()));

            // default to version 0.0.28
            programState.SymbolTable[PredefinedIdentifiers.VERSION] = (false, new IntVariable(28));

            programState.SymbolTable[PredefinedIdentifiers.RESULT] = (false, UndefinedVariable.Instance);
            programState.SymbolTable[PredefinedIdentifiers.ACCOUNT] = SetupAccountObject();
            programState.SymbolTable[PredefinedIdentifiers.CHARACTER] = SetupCharacterObject();
            programState.SymbolTable[PredefinedIdentifiers.MAPSTATE] = SetupMapStateObject();
            programState.SymbolTable[PredefinedIdentifiers.MAP] = SetupMapObject();
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

        private async Task ConnectAsync(ProgramState programState, string host, int port, CancellationToken ct)
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];

            var configRepo = c.Resolve<IConfigurationRepository>();
            configRepo.Host = host;
            configRepo.Port = port;

            configRepo.VersionBuild = ((IntVariable)programState.SymbolTable[PredefinedIdentifiers.VERSION].Identifiable).Value;

            var connectionActions = c.Resolve<INetworkConnectionActions>();
            var connectResult = await connectionActions.ConnectToServer().ConfigureAwait(false);
            if (connectResult != ConnectResult.Success)
                throw new ArgumentException($"Bot {_botIndex}: Unable to connect to server! Host={host} Port={port}");

            var backgroundReceiveActions = c.Resolve<IBackgroundReceiveActions>();
            backgroundReceiveActions.RunBackgroundReceiveLoop();

            var handshakeResult = await connectionActions.BeginHandshake(_random.Next(Constants.MaxChallenge)).ConfigureAwait(false);

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

        private async Task<int> CreateAccountAsync(string user, string pass, CancellationToken ct)
        {
            return (int)await _botHelper.CreateAccountAsync(user, pass).ConfigureAwait(false);
        }

        private async Task<int> LoginAsync(string user, string pass, CancellationToken ct)
        {
            return (int)await _botHelper.LoginToAccountAsync(user, pass).ConfigureAwait(false);
        }

        private async Task<int> CreateAndLoginAsync(string user, string pass, CancellationToken ct)
        {
            var accountReply = (AccountReply)await CreateAccountAsync(user, pass, ct).ConfigureAwait(false);
            if (accountReply == AccountReply.Created || accountReply == AccountReply.Exists)
            {
                return await LoginAsync(user, pass, ct);
            }

            return (int)LoginReply.WrongUser;
        }

        private async Task<int> ChangePasswordAsync(string user, string oldPass, string newPass, CancellationToken ct)
        {
            return (int)await _botHelper.ChangePasswordAsync(user, oldPass, newPass).ConfigureAwait(false);
        }

        private async Task<int> CreateCharacterAsync(string charName, CancellationToken ct)
        {
            return (int)await _botHelper.CreateCharacterAsync(charName).ConfigureAwait(false);
        }

        private async Task<int> DeleteCharacterAsync(string charName, bool force, CancellationToken ct)
        {
            return (int)await _botHelper.DeleteCharacterAsync(charName, force).ConfigureAwait(false);
        }

        private Task LoginToCharacterAsync(string charName, CancellationToken ct)
        {
            return _botHelper.LoginToCharacterAsync(charName);
        }

        private async Task Tick(int delay = 0)
        {
            if (delay > 0)
            {
                DependencyMaster.TypeRegistry[_botIndex].Resolve<IFixedTimeStepRepository>().Tick((uint)delay / 10);
                await Task.Delay(delay).ConfigureAwait(false);
            }

            DependencyMaster
                .TypeRegistry[_botIndex]
                .Resolve<IOutOfBandPacketHandler>()
                .PollForPacketsAndHandle();
        }

        private List<IVariable> GetPathTo(int x, int y)
        {
            var c = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterProvider>().MainCharacter;

            var astarPathFinder = DependencyMaster.TypeRegistry[_botIndex].Resolve<IPathFinder>();
            var path = astarPathFinder.FindPath(c.RenderProperties.Coordinates(), new MapCoordinate(x, y));

            return path.Select(
                entry => (IVariable)new ObjectVariable(
                    new Dictionary<string, (bool, IIdentifiable)>
                    {
                        { "x", Readonly(new IntVariable(entry.X)) },
                        { "y", Readonly(new IntVariable(entry.Y)) },
                    }
                )
            ).ToList();
        }

        private void JoinParty(int characterId)
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];
            c.Resolve<IPartyActions>().RequestParty(PartyRequestType.Join, characterId);
        }

        private Task Chat(string chatText, CancellationToken ct)
        {
            var c = DependencyMaster.TypeRegistry[_botIndex];
            c.Resolve<IChatActions>().SendChatToServer(chatText, string.Empty, ChatType.Local);

            return Tick(FACE_BACKOFF_MS);
        }

        private Task Face(int direction, CancellationToken ct)
        {
            DependencyMaster
                .TypeRegistry[_botIndex]
                .Resolve<ICharacterActions>()
                .Face((EODirection)direction);

            return Tick(FACE_BACKOFF_MS);
        }

        private Task Walk(CancellationToken ct)
        {
            var walkValidationActions = DependencyMaster.TypeRegistry[_botIndex].Resolve<IWalkValidationActions>();
            if (walkValidationActions.CanMoveToDestinationCoordinates() != WalkValidationResult.Walkable)
            {
                return Task.CompletedTask;
            }

            // MainCharacter is normally updated with destination coordinates by CharacterAnimator, not by reply packet
            var cr = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterRepository>();
            cr.MainCharacter = cr.MainCharacter.WithRenderProperties(
                cr.MainCharacter.RenderProperties.WithCoordinates(cr.MainCharacter.RenderProperties.DestinationCoordinates())
            );

            DependencyMaster
                .TypeRegistry[_botIndex]
                .Resolve<ICharacterActions>()
                .Walk(ghosted: false);

            return Tick(WALK_BACKOFF_MS);
        }

        private Task Attack(CancellationToken ct)
        {
            DependencyMaster
                .TypeRegistry[_botIndex]
                .Resolve<ICharacterActions>()
                .Attack();

            return Tick(ATTACK_BACKOFF_MS);
        }

        private Task Sit(CancellationToken ct)
        {
            var c = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterProvider>().MainCharacter;

            DependencyMaster
                .TypeRegistry[_botIndex]
                .Resolve<ICharacterActions>()
                .Sit(c.RenderProperties.Coordinates());

            return Tick(ATTACK_BACKOFF_MS);
        }

        private Task UseItem(int itemId, CancellationToken ct)
        {
            DependencyMaster
                .TypeRegistry[_botIndex]
                .Resolve<IItemActions>()
                .UseItem(itemId);


            return Tick(ATTACK_BACKOFF_MS);
        }

        private Task Drop(int itemId, int amount, CancellationToken ct)
        {
            var c = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterProvider>().MainCharacter;

            DependencyMaster
                .TypeRegistry[_botIndex]
                .Resolve<IItemActions>()
                .DropItem(itemId, amount, c.RenderProperties.Coordinates());

            return Tick(ATTACK_BACKOFF_MS);
        }

        private Task Pickup(int itemIndex, CancellationToken ct)
        {
            var items = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICurrentMapStateProvider>().MapItems;
            if (!items.TryGetValue(itemIndex, out var item))
                throw new BotScriptErrorException("Invalid item index for item pickup");

            DependencyMaster
                .TypeRegistry[_botIndex]
                .Resolve<IMapActions>()
                .PickUpItem(item);

            return Tick(ATTACK_BACKOFF_MS);
        }

        private Task Junk(int itemId, int amount, CancellationToken ct)
        {
            DependencyMaster
                .TypeRegistry[_botIndex]
                .Resolve<IItemActions>()
                .JunkItem(itemId, amount);

            return Tick(ATTACK_BACKOFF_MS);
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
            var mapStateObj = new RuntimeEvaluatedMemberObjectVariable();

            var provider = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICurrentMapStateProvider>();

            mapStateObj.SymbolTable["characters"] = (true, () => new ArrayVariable(provider.Characters.Select(GetMapStateCharacter).ToList()));
            mapStateObj.SymbolTable["npcs"] = (true, () => new ArrayVariable(provider.NPCs.Select(GetMapStateNPC).ToList()));
            mapStateObj.SymbolTable["items"] = (true, () => new ArrayVariable(provider.MapItems.Select(GetMapStateItem).ToList()));

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
            npcObj.SymbolTable["index"] = Readonly(new IntVariable(npc.Index));
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
            itemObj.SymbolTable["index"] = Readonly(new IntVariable(item.UniqueID));
            return itemObj;
        }

        private (bool, IIdentifiable) SetupMapObject()
        {
            var mapObj = new RuntimeEvaluatedMemberObjectVariable();

            var provider = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICurrentMapProvider>();

            mapObj.SymbolTable["id"] = (true, () => new IntVariable(provider.CurrentMap.Properties.MapID));
            mapObj.SymbolTable["warps"] = (true, () => new ArrayVariable(provider.CurrentMap.Warps.SelectMany(GetWarps).ToList()));

            return Readonly(mapObj);

            static IEnumerable<IVariable> GetWarps(IList<WarpMapEntity> warps)
            {
                foreach (var warp in warps.Where(x => x != null))
                {
                    var obj = new ObjectVariable();
                    obj.SymbolTable["x"] = Readonly(new IntVariable(warp.X));
                    obj.SymbolTable["y"] = Readonly(new IntVariable(warp.Y));
                    obj.SymbolTable["map"] = Readonly(new IntVariable(warp.DestinationMapID));
                    yield return obj;
                }
            }
        }
    }
}
