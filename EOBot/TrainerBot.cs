using EOLib;
using EOLib.Domain.Account;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Net.Handlers;
using System;
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
            var mapActions = c.Resolve<IMapActions>();
            var charRepo = c.Resolve<ICharacterRepository>();
            var charActions = c.Resolve<ICharacterActions>();
            var mapCellStateProvider = c.Resolve<IMapCellStateProvider>();
            var handler = c.Resolve<IOutOfBandPacketHandler>();
            var itemData = c.Resolve<IEIFFileProvider>().EIFFile;
            var npcData = c.Resolve<IENFFileProvider>().ENFFile;
            var spellData = c.Resolve<IESFFileProvider>().ESFFile;
            var charInventoryRepo = c.Resolve<ICharacterInventoryRepository>();

            var healItems = charInventoryRepo.ItemInventory
                .Where(x => itemData.Data.Any(y => y.ID == x.ItemID && y.Type == ItemType.Heal))
                .ToList();

            var healSpells = charInventoryRepo.SpellInventory
                .Where(x => spellData.Data.Any(y => y.ID == x.ID && y.Type == SpellType.Heal))
                .ToList();

            int attackCount = 0;

            while (!TerminationRequested)
            {
                handler.PollForPacketsAndHandle();

                var charRenderProps = charRepo.MainCharacter.RenderProperties;
                var nextX = charRenderProps.GetDestinationX();
                var nextY = charRenderProps.GetDestinationY();
                var currentCellState = mapCellStateProvider.GetCellStateAt(charRenderProps.MapX, charRenderProps.MapY);
                var cellState = mapCellStateProvider.GetCellStateAt(nextX, nextY);

                if ((attackCount < CONSECUTIVE_ATTACK_COUNT && !currentCellState.NPC.HasValue) || cellState.NPC.HasValue)
                {
                    if (cellState.NPC.HasValue && charRepo.MainCharacter.Stats[CharacterStat.HP] > charRepo.MainCharacter.Stats[CharacterStat.MaxHP] * .1)
                    {
                        Console.WriteLine($"[ATTK] {cellState.NPC.Value.Index,7} - {npcData.Data.Single(x => x.ID == cellState.NPC.Value.ID).Name}");
                        charActions.Attack();
                        attackCount++;
                    }
                    else if (cellState.Items.Any())
                    {
                        foreach (var item in cellState.Items)
                        {
                            Console.WriteLine($"[TAKE] {item.Amount,7} {itemData.Data.Single(x => x.ID == item.ItemID).Name}");
                            mapActions.PickUpItem(item);
                        }
                    }
                    else if (healSpells.Any() && charRepo.MainCharacter.Stats[CharacterStat.HP] < charRepo.MainCharacter.Stats[CharacterStat.MaxHP]
                        && charRepo.MainCharacter.Stats[CharacterStat.TP] > charRepo.MainCharacter.Stats[CharacterStat.MaxTP] * .5)
                    {
                        var spellToUse = spellData.Data
                            .Where(x => healSpells.Any(y => y.ID == x.ID) && x.Target != SpellTarget.Group)
                            .OrderByDescending(x => x.HP)
                            .First();

                        Console.WriteLine($"[CAST] {spellToUse.HP,4} HP - {spellToUse.Name}");

                        charActions.PrepareCastSpell(spellToUse.ID);
                        await Task.Delay((int)Math.Round(spellToUse.CastTime / 2.0 * 950)); // ?

                        charActions.CastSpell(spellToUse.ID, charRepo.MainCharacter);
                        await Task.Delay((int)Math.Round(spellToUse.CastTime / 2.0 * 950)); // ?
                    }
                    else if (healItems.Any() && charRepo.MainCharacter.Stats[CharacterStat.HP] < charRepo.MainCharacter.Stats[CharacterStat.MaxHP] * .3)
                    {
                        while (charRepo.MainCharacter.Stats[CharacterStat.HP] < charRepo.MainCharacter.Stats[CharacterStat.MaxHP] * .6)
                        {
                            var itemToUse = itemData.Data
                                .Where(x => healItems.Any(y => y.ItemID == x.ID))
                                .OrderBy(x => x.HP)
                                .First();
                            var amount = healItems.Single(x => x.ItemID == itemToUse.ID).Amount;

                            Console.WriteLine($"[USE ] {itemToUse.Name} - {itemToUse.HP} HP - inventory: {amount - 1} - (other heal item types: {healItems.Count - 1})");

                            charActions.UseItem(itemToUse.ID);
                            await Task.Delay(ATTACK_BACKOFF_MS);
                        }
                    }

                    await Task.Delay(ATTACK_BACKOFF_MS);

                    healItems = charInventoryRepo.ItemInventory
                        .Where(x => itemData.Data.Any(y => y.ID == x.ItemID && y.Type == ItemType.Heal))
                        .ToList();

                    healSpells = charInventoryRepo.SpellInventory
                        .Where(x => spellData.Data.Any(y => y.ID == x.ID && y.Type == SpellType.Heal))
                        .ToList();
                }
                else
                {
                    if (!currentCellState.NPC.HasValue)
                        Console.WriteLine($"[MOVE] Walking due to consecutive attacks: {attackCount}");
                    else
                        Console.WriteLine($"[ATTK] Killing NPC at player location");

                    await Task.Delay(TimeSpan.FromMilliseconds(300));

                    var direction = charRenderProps.Direction;
                    charActions.Walk();
                    await Task.Delay(TimeSpan.FromMilliseconds(WALK_BACKOFF_MS));

                    charActions.Face(direction.Opposite());
                    // todo: character actions Face() should also change the character's direction instead of relying on client to update it separately
                    charRepo.MainCharacter = charRepo.MainCharacter.WithRenderProperties(charRepo.MainCharacter.RenderProperties.WithDirection(direction.Opposite()));
                    await Task.Delay(TimeSpan.FromMilliseconds(FACE_BACKOFF_MS));

                    // kill NPC if it was in our starting point
                    while (currentCellState.NPC.HasValue && !TerminationRequested)
                    {
                        Console.WriteLine($"[ATTK] {currentCellState.NPC.Value.Index,7} : {npcData.Data.Single(x => x.ID == currentCellState.NPC.Value.ID).Name}");
                        charActions.Attack();
                        await Task.Delay(TimeSpan.FromMilliseconds(ATTACK_BACKOFF_MS));
                        currentCellState = mapCellStateProvider.GetCellStateAt(charRenderProps.MapX, charRenderProps.MapY);
                    }

                    if (currentCellState.Items.Any())
                    {
                        foreach (var item in currentCellState.Items)
                        {
                            Console.WriteLine($"[TAKE] {item.Amount,7} {itemData.Data.Single(x => x.ID == item.ItemID).Name}");
                            mapActions.PickUpItem(item);
                        }
                    }

                    charActions.Walk();
                    await Task.Delay(TimeSpan.FromMilliseconds(WALK_BACKOFF_MS));

                    charActions.Face(direction);
                    charRepo.MainCharacter = charRepo.MainCharacter.WithRenderProperties(charRepo.MainCharacter.RenderProperties.WithDirection(direction));
                    await Task.Delay(TimeSpan.FromMilliseconds(FACE_BACKOFF_MS));

                    attackCount = 0;
                }

                await Task.Delay(TimeSpan.FromSeconds(1.0 / 30.0));
            }
        }
    }
}
