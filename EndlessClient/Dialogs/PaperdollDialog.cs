using EndlessClient.Audio;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Extensions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Inventory;
using EndlessClient.HUD.Panels;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class PaperdollDialog : PlayerInfoDialog<PaperdollData>
    {
        private readonly IInventoryController _inventoryController;
        private readonly IPaperdollProvider _paperdollProvider;
        private readonly IPubFileProvider _pubFileProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ISfxPlayer _sfxPlayer;

        private readonly InventoryPanel _inventoryPanel;

        private Option<PaperdollData> _paperdollData;

        private readonly List<PaperdollDialogItem> _childItems;

        public PaperdollDialog(INativeGraphicsManager nativeGraphicsManager,
                               IInventoryController inventoryController,
                               IPaperdollProvider paperdollProvider,
                               IPubFileProvider pubFileProvider,
                               IHudControlProvider hudControlProvider,
                               IEODialogButtonService eoDialogButtonService,
                               IInventorySpaceValidator inventorySpaceValidator,
                               IEOMessageBoxFactory eoMessageBoxFactory,
                               IStatusLabelSetter statusLabelSetter,
                               ISfxPlayer sfxPlayer,
                               Character character, bool isMainCharacter)
            : base(nativeGraphicsManager, eoDialogButtonService, pubFileProvider, character, isMainCharacter)
        {
            _paperdollProvider = paperdollProvider;
            _pubFileProvider = pubFileProvider;
            _hudControlProvider = hudControlProvider;
            _inventorySpaceValidator = inventorySpaceValidator;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
            _sfxPlayer = sfxPlayer;
            _inventoryController = inventoryController;

            _inventoryPanel = _hudControlProvider.GetComponent<InventoryPanel>(HudControlIdentifier.InventoryPanel);

            _childItems = new List<PaperdollDialogItem>();

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 49);
            BackgroundTextureSource = new Rectangle(0, BackgroundTexture.Height / 2 * Character.RenderProperties.Gender, DrawArea.Width, BackgroundTexture.Height / 2);

            CenterInGameView();

            if (!Game.Window.AllowUserResizing)
                DrawPosition = new Vector2(DrawPosition.X, 15);

            _paperdollData = Option.None<PaperdollData>();
        }

        public bool NoItemsDragging() => !_childItems.Any(x => x.IsBeingDragged);

        protected override void OnUpdateControl(GameTime gameTime)
        {
            _paperdollData = _paperdollData.FlatMap(paperdollData =>
                paperdollData.NoneWhen(d => _paperdollProvider.VisibleCharacterPaperdolls.ContainsKey(Character.ID) &&
                                           !_paperdollProvider.VisibleCharacterPaperdolls[Character.ID].Equals(d)));

            _paperdollData.MatchNone(() =>
                {
                    if (_paperdollProvider.VisibleCharacterPaperdolls.ContainsKey(Character.ID))
                    {
                        var paperdollData = _paperdollProvider.VisibleCharacterPaperdolls[Character.ID];
                        _paperdollData = Option.Some(paperdollData);
                        UpdateDisplayedData(paperdollData);
                    }
                });

            base.OnUpdateControl(gameTime);
        }

        protected override void UpdateDisplayedData(PaperdollData paperdollData)
        {
            base.UpdateDisplayedData(paperdollData);

            foreach (var control in _childItems)
            {
                control.SetControlUnparented();
                control.Dispose();
            }

            foreach (EquipLocation equipLocation in Enum.GetValues(typeof(EquipLocation)))
            {
                if (equipLocation == EquipLocation.PAPERDOLL_MAX)
                    break;

                var id = paperdollData.Paperdoll[equipLocation];
                var eifRecord = id.SomeWhen(i => i > 0).Map(i => _pubFileProvider.EIFFile[i]);
                var paperdollItem = new PaperdollDialogItem(GraphicsManager, _sfxPlayer, _inventoryPanel, this, _isMainCharacter, equipLocation, eifRecord)
                {
                    DrawArea = equipLocation.GetEquipLocationRectangle()
                };

                paperdollItem.OnMouseEnter += (_, _) =>
                {
                    EOResourceID msg;
                    switch (equipLocation)
                    {
                        case EquipLocation.Boots: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_BOOTS_EQUIPMENT; break;
                        case EquipLocation.Accessory: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_MISC_EQUIPMENT; break;
                        case EquipLocation.Gloves: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_GLOVES_EQUIPMENT; break;
                        case EquipLocation.Belt: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_BELT_EQUIPMENT; break;
                        case EquipLocation.Armor: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_ARMOR_EQUIPMENT; break;
                        case EquipLocation.Necklace: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_NECKLACE_EQUIPMENT; break;
                        case EquipLocation.Hat: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_HAT_EQUIPMENT; break;
                        case EquipLocation.Shield: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_SHIELD_EQUIPMENT; break;
                        case EquipLocation.Weapon: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_WEAPON_EQUIPMENT; break;
                        case EquipLocation.Ring1:
                        case EquipLocation.Ring2: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_RING_EQUIPMENT; break;
                        case EquipLocation.Armlet1:
                        case EquipLocation.Armlet2: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_ARMLET_EQUIPMENT; break;
                        case EquipLocation.Bracer1:
                        case EquipLocation.Bracer2: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_BRACER_EQUIPMENT; break;
                        default: throw new ArgumentOutOfRangeException();
                    }

                    var extra = eifRecord.Match(rec => $", {rec.Name}", () => string.Empty);
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, msg, extra);
                };

                paperdollItem.UnequipAction += itemInfo =>
                {
                    if (itemInfo.Special == ItemSpecial.Cursed)
                    {
                        var msgBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.ITEM_IS_CURSED_ITEM, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                        msgBox.ShowDialog();
                    }
                    else
                    {
                        eifRecord.MatchSome(rec =>
                        {
                            if (!_inventorySpaceValidator.ItemFits(rec.ID))
                            {
                                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_ITEM_UNEQUIP_NO_SPACE_LEFT);
                            }
                            else
                            {
                                // packet reply handles updating the paperdoll for the character which will unrender the equipment
                                _inventoryController.UnequipItem(equipLocation);
                            }
                        });
                    }
                };

                paperdollItem.SetParentControl(this);
                paperdollItem.Initialize();

                _childItems.Add(paperdollItem);
            }
        }
    }
}
