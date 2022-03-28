using EOLib;
using EOLib.Domain.Account;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Item;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EOBot
{
    internal class TrainerBot : BotBase
    {
        private const int CONSECUTIVE_ATTACK_COUNT = 150;
        private const int ATTACK_BACKOFF_MS = 500;
        private const int WALK_BACKOFF_MS = 400;
        private const int FACE_BACKOFF_MS = 120;

        private static readonly int[] JunkItemIds = new[]
        {
            // Dragon Blade, enchanted boots (red/green/blue)
            //37, 124, 125, 126
            // cava staff
            329 
        };

        private readonly string _account;
        private readonly string _password;
        private readonly string _character;

        private ICharacterActions _characterActions;
        private IMapActions _mapActions;
        private IItemActions _itemActions;

        private ICharacterRepository _characterRepository;

        private IPubFile<EIFRecord> _itemData;
        private IPubFile<ENFRecord> _npcData;
        private IPubFile<ESFRecord> _spellData;

        public TrainerBot(int botIndex, string account, string password, string character)
            : base(botIndex)
        {
            _account = account;
            _password = password;
            _character = character;
        }

        protected override async Task DoWorkAsync(CancellationToken ct)
        {
            var helper = new BotHelper(_index);

            var res = await helper.LoginToAccountAsync(_account, _password);
            if (res == LoginReply.WrongUser)
            {
                var createRes = await helper.CreateAccountAsync(_account, _password);
                if (createRes != AccountReply.Created)
                    throw new InvalidOperationException($"Unable to create account: invalid response {Enum.GetName(typeof(AccountReply), createRes)}");

                res = await helper.LoginToAccountAsync(_account, _password);
            }

            if (res != LoginReply.Ok)
                throw new InvalidOperationException($"Unable to log in to account: invalid response {Enum.GetName(typeof(LoginReply), res)}");

            await helper.LoginToCharacterAsync(_character);

            var c = DependencyMaster.TypeRegistry[_index];

            _mapActions = c.Resolve<IMapActions>();
            _itemActions = c.Resolve<IItemActions>();
            _characterRepository = c.Resolve<ICharacterRepository>();
            _characterActions = c.Resolve<ICharacterActions>();
            var mapCellStateProvider = c.Resolve<IMapCellStateProvider>();
            var mapStateProvider = c.Resolve<ICurrentMapStateProvider>();
            var handler = c.Resolve<IOutOfBandPacketHandler>();
            _itemData = c.Resolve<IEIFFileProvider>().EIFFile;
            _npcData = c.Resolve<IENFFileProvider>().ENFFile;
            _spellData = c.Resolve<IESFFileProvider>().ESFFile;
            var charInventoryRepo = c.Resolve<ICharacterInventoryRepository>();
            var walkValidator = c.Resolve<IWalkValidationActions>();

            var healItems = new List<IInventoryItem>();
            var healSpells = new List<IInventorySpell>();

            int attackCount = 0, cachedPlayerCount = 0;
            bool time_to_die = false;

            MapCoordinate? priorityCoord = null;
            while (!ct.IsCancellationRequested)
            {
                handler.PollForPacketsAndHandle();

                var character = _characterRepository.MainCharacter;
                var charRenderProps = character.RenderProperties;

                var currentPositionCellState = mapCellStateProvider.GetCellStateAt(charRenderProps.MapX, charRenderProps.MapY);

                if (cachedPlayerCount != mapStateProvider.Characters.Count)
                {
                    cachedPlayerCount = mapStateProvider.Characters.Count;
                    if (cachedPlayerCount > 0)
                    {
                        ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, $"{cachedPlayerCount,7} - Players on map - You may not be able to train here", ConsoleColor.DarkYellow);
                    }
                }

                var coords = new MapCoordinate[]
                {
                    new MapCoordinate(charRenderProps.MapX + 1, charRenderProps.MapY),
                    new MapCoordinate(charRenderProps.MapX - 1, charRenderProps.MapY),
                    new MapCoordinate(charRenderProps.MapX, charRenderProps.MapY + 1),
                    new MapCoordinate(charRenderProps.MapX, charRenderProps.MapY - 1)
                };

                if (priorityCoord != null)
                {
                    // ensure if there's an attack in progress that we finish killing that NPC before moving around
                    var tmp = new[] { priorityCoord.Value };
                    coords = tmp.Concat(coords.Except(tmp)).ToArray();
                }

                var action_taken = false;
                foreach (var coord in coords)
                {
                    if (action_taken)
                        break;

                    if (!time_to_die)
                    {
                        var cellState = mapCellStateProvider.GetCellStateAt(coord.X, coord.Y);

                        if (priorityCoord.HasValue && !cellState.NPC.HasValue)
                            priorityCoord = null;

                        if (character.Stats[CharacterStat.Weight] >= character.Stats[CharacterStat.MaxWeight])
                        {
                            ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, $"OVER WEIGHT LIMIT - TIME TO DIE", ConsoleColor.DarkYellow);
                            await ToggleSit();
                            action_taken = true;
                            time_to_die = true;
                        }
                        else if ((attackCount < CONSECUTIVE_ATTACK_COUNT && !currentPositionCellState.NPC.HasValue) || cellState.NPC.HasValue)
                        {
                            if (cellState.NPC.HasValue && character.Stats[CharacterStat.HP] > character.Stats[CharacterStat.MaxHP] * .1)
                            {
                                await FaceCoordinateIfNeeded(new MapCoordinate(charRenderProps.MapX, charRenderProps.MapY), coord);
                                await Attack(cellState);
                                attackCount++;
                                action_taken = true;
                                priorityCoord = coord;
                            }
                            else if (cellState.Items.Any())
                            {
                                await PickUpItems(cellState);
                                action_taken = true;
                            }
                            else if (healItems.Any() && character.Stats[CharacterStat.HP] < character.Stats[CharacterStat.MaxHP] * .3)
                            {
                                var stats = _characterRepository.MainCharacter.Stats;
                                while (!ct.IsCancellationRequested && stats[CharacterStat.HP] < stats[CharacterStat.MaxHP] * .6)
                                {
                                    await UseHealItem(healItems);
                                    handler.PollForPacketsAndHandle();
                                    stats = _characterRepository.MainCharacter.Stats;
                                }

                                action_taken = true;
                            }
                            else if (healSpells.Any() && character.Stats[CharacterStat.HP] < character.Stats[CharacterStat.MaxHP] * .9
                                && character.Stats[CharacterStat.TP] > character.Stats[CharacterStat.MaxTP] * .5)
                            {
                                await CastHealSpell(healSpells);
                                action_taken = true;
                            }

                            healItems = charInventoryRepo.ItemInventory
                                .Where(x => _itemData.Any(y => y.ID == x.ItemID && y.Type == ItemType.Heal))
                                .ToList();

                            healSpells = charInventoryRepo.SpellInventory
                                .Where(x => _spellData.Any(y => y.ID == x.ID && y.Type == SpellType.Heal))
                                .ToList();
                        }
                        else
                        {
                            if (!currentPositionCellState.NPC.HasValue)
                                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Move, $"Walking due to consecutive attacks: {attackCount}");
                            else
                                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Move, $"Killing NPC at player location");

                            // find a direction that's open to walk to
                            var targetWalkCell = coords.Select(z => mapCellStateProvider.GetCellStateAt(z.X, z.Y))
                                .FirstOrDefault(z => walkValidator.IsCellStateWalkable(z));
                            if (targetWalkCell == null)
                            {
                                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, $"Couldn't find open space to walk!", ConsoleColor.DarkYellow);
                                break;
                            }

                            await FaceCoordinateIfNeeded(new MapCoordinate(charRenderProps.MapX, charRenderProps.MapY), targetWalkCell.Coordinate);

                            var originalDirection = charRenderProps.Direction;
                            await Walk();
                            await Face(originalDirection.Opposite());

                            // kill NPC if it was in our starting point
                            while (currentPositionCellState.NPC.HasValue && !ct.IsCancellationRequested)
                            {
                                await Attack(currentPositionCellState);

                                handler.PollForPacketsAndHandle();
                                currentPositionCellState = mapCellStateProvider.GetCellStateAt(charRenderProps.MapX, charRenderProps.MapY);
                            }

                            await PickUpItems(currentPositionCellState);

                            await Walk();
                            await Face(originalDirection);

                            attackCount = 0;
                            action_taken = true;
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1.0 / 8.0));
            }
        }

        private async Task Attack(IMapCellState cellState)
        {
            cellState.NPC.MatchSome(npc => ConsoleHelper.WriteMessage(ConsoleHelper.Type.Attack, $"{npc.Index,7} - {_npcData.Single(x => x.ID == npc.ID).Name}"));
            await TrySend(_characterActions.Attack);
            await Task.Delay(TimeSpan.FromMilliseconds(ATTACK_BACKOFF_MS));
        }

        private async Task Walk()
        {
            var renderProps = _characterRepository.MainCharacter.RenderProperties;
            ConsoleHelper.WriteMessage(ConsoleHelper.Type.Walk, $"{renderProps.GetDestinationX(),3},{renderProps.GetDestinationY(),3}");
            await TrySend(_characterActions.Walk);
            await Task.Delay(TimeSpan.FromMilliseconds(WALK_BACKOFF_MS));
        }

        private async Task Face(EODirection direction)
        {
            var rp = _characterRepository.MainCharacter.RenderProperties;
            ConsoleHelper.WriteMessage(ConsoleHelper.Type.Face, $"{Enum.GetName(typeof(EODirection), direction),7} - at {rp.MapX},{rp.MapY}");
            await TrySend(() => _characterActions.Face(direction));

            // todo: character actions Face() should also change the character's direction instead of relying on client to update it separately
            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithRenderProperties(_characterRepository.MainCharacter.RenderProperties.WithDirection(direction));

            await Task.Delay(TimeSpan.FromMilliseconds(FACE_BACKOFF_MS));
        }

        private async Task FaceCoordinateIfNeeded(MapCoordinate originalCoord, MapCoordinate targetCoord)
        {
            var charRenderProps = _characterRepository.MainCharacter.RenderProperties;
            var nextCoord = new MapCoordinate(charRenderProps.GetDestinationX(), charRenderProps.GetDestinationY());
            if (nextCoord != targetCoord)
            {
                var diff = targetCoord - originalCoord;
                var direction = diff.X > 0 ? EODirection.Right
                    : diff.X < 0 ? EODirection.Left
                        : diff.Y > 0 ? EODirection.Down
                            : diff.Y < 0 ? EODirection.Up : charRenderProps.Direction;

                if (direction != charRenderProps.Direction)
                    await Face(direction);
            }
        }

        private async Task PickUpItems(IMapCellState cellState)
        {
            foreach (var item in cellState.Items)
            {
                await PickUpItem(item);

                if (JunkItemIds.Contains(item.ItemID))
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await JunkItem(item);
                }
            }
        }

        private async Task PickUpItem(IItem item)
        {
            await TrySend(() =>
            {
                var itemName = _itemData.Single(x => x.ID == item.ItemID).Name;
                var pickupResult = _mapActions.PickUpItem(item);
                if (pickupResult == ItemPickupResult.Ok)
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.TakeItem, $"{item.Amount,7} - {itemName}");
                else
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, $"Ignoring item {itemName} x{item.Amount} due to pickup error {pickupResult}", ConsoleColor.DarkYellow);
            });
            await Task.Delay(TimeSpan.FromMilliseconds(ATTACK_BACKOFF_MS));
        }

        private async Task JunkItem(IItem item)
        {
            ConsoleHelper.WriteMessage(ConsoleHelper.Type.JunkItem, $"{item.Amount,7} - {_itemData.Single(x => x.ID == item.ItemID).Name}");
            await TrySend(() => _itemActions.JunkItem(item));
            await Task.Delay(TimeSpan.FromMilliseconds(ATTACK_BACKOFF_MS));
        }

        private async Task CastHealSpell(IEnumerable<IInventorySpell> healSpells)
        {
            var spellToUse = _spellData
                .Where(x => healSpells.Any(y => y.ID == x.ID) && x.Target != SpellTarget.Group)
                .OrderByDescending(x => x.HP)
                .First();

            var stats = _characterRepository.MainCharacter.Stats.Stats;
            ConsoleHelper.WriteMessage(ConsoleHelper.Type.Cast, $"{spellToUse.HP,4} HP - {spellToUse.Name} - TP {stats[CharacterStat.TP]}/{stats[CharacterStat.MaxTP]}");

            await TrySend(() => _characterActions.PrepareCastSpell(spellToUse.ID));
            await Task.Delay((int)Math.Round(spellToUse.CastTime / 2.0 * 950)); // ?

            await TrySend(() => _characterActions.CastSpell(spellToUse.ID, _characterRepository.MainCharacter));
            await Task.Delay((int)Math.Round(spellToUse.CastTime / 2.0 * 950)); // ?
        }

        private async Task UseHealItem(IEnumerable<IInventoryItem> healItems)
        {
            var itemToUse = _itemData
                .Where(x => healItems.Any(y => y.ItemID == x.ID))
                .OrderBy(x => x.HP)
                .First();
            var amount = healItems.Single(x => x.ItemID == itemToUse.ID).Amount;

            ConsoleHelper.WriteMessage(ConsoleHelper.Type.UseItem, $"{itemToUse.Name} - {itemToUse.HP} HP - inventory: {amount - 1} - (other heal item types: {healItems.Count() - 1})");

            await TrySend(() => _characterActions.UseItem((short)itemToUse.ID));

            await Task.Delay(ATTACK_BACKOFF_MS);
        }

        private async Task ToggleSit()
        {
            var renderProps = _characterRepository.MainCharacter.RenderProperties;
            var nextState = renderProps.SitState == SitState.Standing ? "Floor" : "Stand";
            ConsoleHelper.WriteMessage(ConsoleHelper.Type.Sit, $"{nextState,7} - Toggling from: {Enum.GetName(typeof(SitState), renderProps.SitState)}");
            await TrySend(_characterActions.ToggleSit);
        }

        private async Task TrySend(Action action, int attempts = 3)
        {
            for (int i = 1; i <= attempts; i++)
            {
                try
                {
                    action();
                    break;
                }
                catch (NoDataSentException)
                {
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, $"{i} / {attempts}   - No data sent", ConsoleColor.DarkRed);
                    if (i == attempts)
                        throw;

                    await Task.Delay(TimeSpan.FromSeconds(i * i));
                }
            }
        }
    }
}
