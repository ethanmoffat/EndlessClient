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
            var charInventoryRepo = c.Resolve<ICharacterInventoryRepository>();

            var healItems = charInventoryRepo.ItemInventory
                .Where(x => itemData.Data.Any(y => y.ID == x.ItemID && y.Type == ItemType.Heal))
                .ToList();

            int attackCount = 0;

            while (!TerminationRequested)
            {
                handler.PollForPacketsAndHandle();

                var charRenderProps = charRepo.MainCharacter.RenderProperties;
                var nextX = charRenderProps.GetDestinationX();
                var nextY = charRenderProps.GetDestinationY();
                var cellState = mapCellStateProvider.GetCellStateAt(nextX, nextY);

                if (attackCount < CONSECUTIVE_ATTACK_COUNT || cellState.NPC.HasValue)
                {
                    if (cellState.NPC.HasValue)
                    {
                        Console.WriteLine($"Attacking NPC index {cellState.NPC.Value.Index,3} : {npcData.Data.Single(x => x.ID == cellState.NPC.Value.ID).Name}");
                        charActions.Attack();
                        attackCount++;
                    }
                    else if (cellState.Items.Any())
                    {
                        foreach (var item in cellState.Items)
                        {
                            Console.WriteLine($"Picking up item {itemData.Data.Single(x => x.ID == item.ItemID).Name}x{item.Amount}");
                            mapActions.PickUpItem(item);
                        }
                    }
                    else if (healItems.Any() && charRepo.MainCharacter.Stats[CharacterStat.HP] < charRepo.MainCharacter.Stats[CharacterStat.MaxHP] * .3)
                    {
                        var itemToUse = itemData.Data
                            .Where(x => healItems.Any(y => y.ItemID == x.ID))
                            .OrderBy(x => x.HP)
                            .First();
                        var amount = healItems.Single(x => x.ItemID == itemToUse.ID).Amount;

                        Console.WriteLine($"Using heal item {itemToUse.Name} (heal: {itemToUse.HP}) (remaining: {amount - 1}) (other heal item types: {healItems.Count - 1})");

                        charActions.UseItem(itemToUse.ID);
                        await Task.Delay(ATTACK_BACKOFF_MS);
                    }

                    await Task.Delay(ATTACK_BACKOFF_MS);

                    healItems = charInventoryRepo.ItemInventory
                        .Where(x => itemData.Data.Any(y => y.ID == x.ItemID && y.Type == ItemType.Heal))
                        .ToList();
                }
                else
                {
                    Console.WriteLine($"Walking due to consecutive attacks: {attackCount}");
                    await Task.Delay(TimeSpan.FromMilliseconds(300));

                    var direction = charRenderProps.Direction;
                    charActions.Walk();
                    await Task.Delay(TimeSpan.FromMilliseconds(WALK_BACKOFF_MS));

                    charActions.Face(direction.Opposite());
                    // todo: character actions Face() should also change the character's direction instead of relying on client to update it separately
                    charRepo.MainCharacter = charRepo.MainCharacter.WithRenderProperties(charRepo.MainCharacter.RenderProperties.WithDirection(direction.Opposite()));
                    await Task.Delay(TimeSpan.FromMilliseconds(FACE_BACKOFF_MS));
                    
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
