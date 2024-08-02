using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EndlessClient.HUD
{
    [AutoMappedType]
    public class UserInterfaceActions : IUserInterfaceNotifier
    {
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public UserInterfaceActions(IInGameDialogActions inGameDialogActions,
                                    IEIFFileProvider eifFileProvider,
                                    ILocalizedStringFinder localizedStringFinder)
        {
            _inGameDialogActions = inGameDialogActions;
            _eifFileProvider = eifFileProvider;
            _localizedStringFinder = localizedStringFinder;
        }

        public void NotifyPacketDialog(PacketFamily packetFamily)
        {
            switch (packetFamily)
            {
                case PacketFamily.Locker: _inGameDialogActions.ShowLockerDialog(); break;
                case PacketFamily.Chest: _inGameDialogActions.ShowChestDialog(); break;
                case PacketFamily.Board: _inGameDialogActions.ShowBoardDialog(); break;
                case PacketFamily.Jukebox: _inGameDialogActions.ShowJukeboxDialog(); break;
            }
        }

        public void NotifyMessageDialog(string title, IReadOnlyList<string> messages)
        {
            _inGameDialogActions.ShowMessageDialog(title, messages);
        }

        public void NotifyCharacterInfo(string name, int mapId, MapCoordinate mapCoords, CharacterStats stats)
        {
            var messages = new List<(string, string)>
            {
                (_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_LEVEL), $"{stats[CharacterStat.Experience]} [{stats[CharacterStat.Level]}]"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_LOCATION), mapId.ToString()),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_COORDINATES), $"{mapCoords.X},{mapCoords.Y}"),
                (string.Empty, string.Empty),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_HITPOINTS), $"{stats[CharacterStat.HP]}/{stats[CharacterStat.MaxHP]}"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_TECHPOINTS), $"{stats[CharacterStat.TP]}/{stats[CharacterStat.MaxTP]}"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_WEIGHT), $"{stats[CharacterStat.Weight]}/{stats[CharacterStat.MaxWeight]}"),
                (string.Empty, string.Empty),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_DAMAGE), $"{stats[CharacterStat.MinDam]}-{stats[CharacterStat.MaxDam]}"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_HITRATE), $"{stats[CharacterStat.Accuracy]}"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_ARMOR), $"{stats[CharacterStat.Armor]}"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_DODGE), $"{stats[CharacterStat.Evade]}"),
                (string.Empty, string.Empty),
                (_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_STRENGTH), $"{stats[CharacterStat.Strength]}"),
                (_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_INTELLIGENCE), $"{stats[CharacterStat.Intelligence]}"),
                (_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_WISDOM), $"{stats[CharacterStat.Wisdom]}"),
                (_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_AGILITY), $"{stats[CharacterStat.Agility]}"),
                (_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_CONSTITUTION), $"{stats[CharacterStat.Constitution]}"),
                (_localizedStringFinder.GetString(EOResourceID.SKILLMASTER_WORD_CHARISMA), $"{stats[CharacterStat.Charisma]}"),
                (string.Empty, string.Empty),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_LIGHT), $"{stats[CharacterStat.Light]}"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_DARK), $"{stats[CharacterStat.Dark]}"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_FIRE), $"{stats[CharacterStat.Fire]}"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_WATER), $"{stats[CharacterStat.Water]}"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_EARTH), $"{stats[CharacterStat.Earth]}"),
                (_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_WIND), $"{stats[CharacterStat.Wind]}"),
            };

            var title = $"{name} - {(int)Math.Floor(stats[CharacterStat.Usage] / 60.0)}h.{stats[CharacterStat.Usage] % 60}m.";
            _inGameDialogActions.ShowKeyValueMessageDialog(title, messages);
        }

        public void NotifyCharacterInventory(string name, int usage, int gold, IReadOnlyList<InventoryItem> inventory, IReadOnlyList<InventoryItem> bank)
        {
            var inventoryDisplay = inventory
                .Select(x => ((string, string))(_eifFileProvider.EIFFile[x.ItemID].Name, x.Amount.ToString()))
                .OrderBy(x => x.Item1);

            var bankDisplay = bank
                .Select(x => ((string, string))(_eifFileProvider.EIFFile[x.ItemID].Name, x.Amount.ToString()))
                .OrderBy(x => x.Item1);

            var messages = new List<(string, string)>();
            messages.AddRange(inventoryDisplay);
            messages.Add((string.Empty, string.Empty));
            messages.AddRange(bankDisplay);
            messages.Add((string.Empty, string.Empty));
            messages.Add((_localizedStringFinder.GetString(EOResourceID.ADMIN_INFO_WORD_BANK_ACCOUNT), gold.ToString()));
            messages.Add((string.Empty, string.Empty));

            var title = $"{name} - {(int)Math.Floor(usage / 60.0)}h.{usage % 60}m.";
            _inGameDialogActions.ShowKeyValueMessageDialog(title, messages);
        }
    }
}