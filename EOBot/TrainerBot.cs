using EOLib;
using EOLib.Domain.Account;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
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

        private readonly string _account;
        private readonly string _password;
        private readonly string _character;

        private ICharacterActions _characterActions;
        private IMapActions _mapActions;

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
            _characterRepository = c.Resolve<ICharacterRepository>();
            _characterActions = c.Resolve<ICharacterActions>();
            var mapCellStateProvider = c.Resolve<IMapCellStateProvider>();
            var handler = c.Resolve<IOutOfBandPacketHandler>();
            _itemData = c.Resolve<IEIFFileProvider>().EIFFile;
            _npcData = c.Resolve<IENFFileProvider>().ENFFile;
            _spellData = c.Resolve<IESFFileProvider>().ESFFile;
            var charInventoryRepo = c.Resolve<ICharacterInventoryRepository>();

            var healItems = new List<IInventoryItem>();
            var healSpells = new List<IInventorySpell>();

            int attackCount = 0;
            bool time_to_die = false;

            while (!TerminationRequested)
            {
                handler.PollForPacketsAndHandle();

                var character = _characterRepository.MainCharacter;
                var charRenderProps = character.RenderProperties;

                var nextX = charRenderProps.GetDestinationX();
                var nextY = charRenderProps.GetDestinationY();
                var currentCellState = mapCellStateProvider.GetCellStateAt(charRenderProps.MapX, charRenderProps.MapY);
                var cellState = mapCellStateProvider.GetCellStateAt(nextX, nextY);

                if (!time_to_die)
                {
                    if ((attackCount < CONSECUTIVE_ATTACK_COUNT && !currentCellState.NPC.HasValue) || cellState.NPC.HasValue)
                    {
                        if (cellState.NPC.HasValue && character.Stats[CharacterStat.HP] > character.Stats[CharacterStat.MaxHP] * .1)
                        {
                            await Attack(cellState);
                            attackCount++;
                        }
                        else if (cellState.Items.Any())
                        {
                            await PickUpItems(cellState);
                        }
                        else if (healSpells.Any() && character.Stats[CharacterStat.HP] < character.Stats[CharacterStat.MaxHP]
                            && character.Stats[CharacterStat.TP] > character.Stats[CharacterStat.MaxTP] * .5)
                        {
                            await CastHealSpell(healSpells);
                        }
                        else if (healItems.Any() && character.Stats[CharacterStat.HP] < character.Stats[CharacterStat.MaxHP] * .3)
                        {
                            await UseHealItem(healItems, targetHealthPercent: .6);
                        }
                        else if (character.Stats[CharacterStat.Weight] >= character.Stats[CharacterStat.MaxWeight])
                        {
                            await SitDown();
                            time_to_die = true;
                        }

                        healItems = charInventoryRepo.ItemInventory
                            .Where(x => _itemData.Data.Any(y => y.ID == x.ItemID && y.Type == ItemType.Heal))
                            .ToList();

                        healSpells = charInventoryRepo.SpellInventory
                            .Where(x => _spellData.Data.Any(y => y.ID == x.ID && y.Type == SpellType.Heal))
                            .ToList();
                    }
                    else
                    {
                        if (!currentCellState.NPC.HasValue)
                            Console.WriteLine($"[MOVE] Walking due to consecutive attacks: {attackCount}");
                        else
                            Console.WriteLine($"[ATTK] Killing NPC at player location");

                        var originalDirection = charRenderProps.Direction;
                        await Walk();
                        await Face(originalDirection.Opposite());

                        // kill NPC if it was in our starting point
                        while (currentCellState.NPC.HasValue && !TerminationRequested)
                        {
                            await Attack(currentCellState);

                            handler.PollForPacketsAndHandle();
                            currentCellState = mapCellStateProvider.GetCellStateAt(charRenderProps.MapX, charRenderProps.MapY);
                        }

                        await PickUpItems(currentCellState);

                        await Walk();
                        await Face(originalDirection);

                        attackCount = 0;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1.0 / 8.0));
            }
        }

        private async Task Attack(IMapCellState cellState)
        {
            Console.WriteLine($"[ATTK] {cellState.NPC.Value.Index,7} - {_npcData.Data.Single(x => x.ID == cellState.NPC.Value.ID).Name}");
            await TrySend(_characterActions.Attack);
            await Task.Delay(TimeSpan.FromMilliseconds(ATTACK_BACKOFF_MS));
        }

        private async Task Walk()
        {
            var renderProps = _characterRepository.MainCharacter.RenderProperties;
            Console.WriteLine($"[WALK] {renderProps.GetDestinationX(),3},{renderProps.GetDestinationY(),3}");
            await TrySend(_characterActions.Walk);
            await Task.Delay(TimeSpan.FromMilliseconds(WALK_BACKOFF_MS));
        }

        private async Task Face(EODirection direction)
        {
            Console.WriteLine($"[FACE] {Enum.GetName(typeof(EODirection), direction),7}");
            await TrySend(() => _characterActions.Face(direction.Opposite()));

            // todo: character actions Face() should also change the character's direction instead of relying on client to update it separately
            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithRenderProperties(_characterRepository.MainCharacter.RenderProperties.WithDirection(direction.Opposite()));

            await Task.Delay(TimeSpan.FromMilliseconds(FACE_BACKOFF_MS));
        }

        private async Task PickUpItems(IMapCellState cellState)
        {
            foreach (var item in cellState.Items)
            {
                Console.WriteLine($"[TAKE] {item.Amount,7} - {_itemData.Data.Single(x => x.ID == item.ItemID).Name}");
                await TrySend(() => _mapActions.PickUpItem(item));

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }

        private async Task CastHealSpell(IEnumerable<IInventorySpell> healSpells)
        {
            var spellToUse = _spellData.Data
                .Where(x => healSpells.Any(y => y.ID == x.ID) && x.Target != SpellTarget.Group)
                .OrderByDescending(x => x.HP)
                .First();

            Console.WriteLine($"[CAST] {spellToUse.HP,4} HP - {spellToUse.Name}");

            await TrySend(() => _characterActions.PrepareCastSpell(spellToUse.ID));
            await Task.Delay((int)Math.Round(spellToUse.CastTime / 2.0 * 950)); // ?

            await TrySend(() => _characterActions.CastSpell(spellToUse.ID, _characterRepository.MainCharacter));
            await Task.Delay((int)Math.Round(spellToUse.CastTime / 2.0 * 950)); // ?
        }

        private async Task UseHealItem(IEnumerable<IInventoryItem> healItems, double targetHealthPercent)
        {
            while (_characterRepository.MainCharacter.Stats[CharacterStat.HP] < _characterRepository.MainCharacter.Stats[CharacterStat.MaxHP] * targetHealthPercent)
            {
                var itemToUse = _itemData.Data
                    .Where(x => healItems.Any(y => y.ItemID == x.ID))
                    .OrderBy(x => x.HP)
                    .First();
                var amount = healItems.Single(x => x.ItemID == itemToUse.ID).Amount;

                Console.WriteLine($"[USE ] {itemToUse.Name} - {itemToUse.HP} HP - inventory: {amount - 1} - (other heal item types: {healItems.Count() - 1})");

                await TrySend(() => _characterActions.UseItem(itemToUse.ID));

                await Task.Delay(ATTACK_BACKOFF_MS);
            }
        }

        private async Task SitDown()
        {
            Console.WriteLine($"[SIT ] OVER WEIGHT LIMIT - TIME TO DIE");
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
                    Console.WriteLine($"[EX  ] {i} / {attempts}   - No data sent");
                    if (i == attempts)
                        throw;

                    await Task.Delay(TimeSpan.FromSeconds(i * i));
                }
            }
        }
    }
}
