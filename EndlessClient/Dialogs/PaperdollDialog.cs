using EndlessClient.Audio;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Extensions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Inventory;
using EndlessClient.HUD.Panels;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Online;
using EOLib.Extensions;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Optional;
using Optional.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class PaperdollDialog : BaseEODialog
    {
        private static readonly Rectangle _iconDrawRect = new Rectangle(227, 258, 44, 21);

        private readonly IInventoryController _inventoryController;
        private readonly IPaperdollProvider _paperdollProvider;
        private readonly IPubFileProvider _pubFileProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly bool _isMainCharacter;
        private readonly Texture2D _characterIconSheet;
        private Option<Rectangle> _characterIconSourceRect;
        private readonly InventoryPanel _inventoryPanel;

        private Option<PaperdollData> _paperdollData;

        private readonly List<PaperdollDialogItem> _childItems;

        private readonly IXNALabel _name,
            _home,
            _class,
            _partner,
            _title,
            _guild,
            _rank;

        public Character Character { get; }

        public PaperdollDialog(IGameStateProvider gameStateProvider,
                               INativeGraphicsManager nativeGraphicsManager,
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
            : base(nativeGraphicsManager, gameStateProvider)
        {
            _paperdollProvider = paperdollProvider;
            _pubFileProvider = pubFileProvider;
            _hudControlProvider = hudControlProvider;
            _inventorySpaceValidator = inventorySpaceValidator;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
            _sfxPlayer = sfxPlayer;
            _inventoryController = inventoryController;
            Character = character;
            _isMainCharacter = isMainCharacter;
            _characterIconSheet = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true);
            _characterIconSourceRect = Option.None<Rectangle>();

            _inventoryPanel = _hudControlProvider.GetComponent<InventoryPanel>(HudControlIdentifier.InventoryPanel);

            _childItems = new List<PaperdollDialogItem>();

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 49);
            BackgroundTextureSource = new Rectangle(0, BackgroundTexture.Height / 2 * Character.RenderProperties.Gender, DrawArea.Width, BackgroundTexture.Height / 2);

            var okButton = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                new Vector2(276, 253),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            okButton.OnClick += (_, _) => Close(XNADialogResult.OK);
            okButton.Initialize();
            okButton.SetParentControl(this);

            _name = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 22, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _name.Initialize();
            _name.SetParentControl(this);

            _home = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 52, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _home.Initialize();
            _home.SetParentControl(this);

            _class = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 82, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _class.Initialize();
            _class.SetParentControl(this);

            _partner = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 112, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _partner.Initialize();
            _partner.SetParentControl(this);

            _title = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 142, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _title.Initialize();
            _title.SetParentControl(this);

            _guild = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 202, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _guild.Initialize();
            _guild.SetParentControl(this);

            _rank = new XNALabel(Constants.FontSize08pt5) { DrawArea = new Rectangle(228, 232, 1, 1), ForeColor = ColorConstants.LightGrayText };
            _rank.Initialize();
            _rank.SetParentControl(this);

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

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);

            _spriteBatch.Begin();

            _characterIconSourceRect.MatchSome(sourceRect =>
            {
                _spriteBatch.Draw(_characterIconSheet,
                    new Vector2(
                        DrawAreaWithParentOffset.X + _iconDrawRect.X + (_iconDrawRect.Width / 2) - (sourceRect.Width / 2),
                        DrawAreaWithParentOffset.Y + _iconDrawRect.Y + (_iconDrawRect.Height / 2) - (sourceRect.Height / 2)),
                    sourceRect,
                    Color.White);
            });

            _spriteBatch.End();
        }

        private void UpdateDisplayedData(PaperdollData paperdollData)
        {
            _name.Text = Capitalize(paperdollData.Name);
            _home.Text = Capitalize(paperdollData.Home);

            paperdollData.Class.SomeWhen(x => x != 0)
                .MatchSome(classId => _class.Text = Capitalize(_pubFileProvider.ECFFile[classId].Name));

            _partner.Text = Capitalize(paperdollData.Partner);
            _title.Text = Capitalize(paperdollData.Title);
            _guild.Text = Capitalize(paperdollData.Guild);
            _rank.Text = Capitalize(paperdollData.Rank);

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

                paperdollItem.RightClick += (_, rightClickedItem) =>
                {
                    if (rightClickedItem.Special == ItemSpecial.Cursed)
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

            _characterIconSourceRect = Option.Some(GetOnlineIconSourceRectangle(paperdollData.Icon));
        }

        private static string Capitalize(string input) => 
            string.IsNullOrEmpty(input) ? string.Empty : char.ToUpper(input[0]) + input[1..].ToLower();

        private static Rectangle GetOnlineIconSourceRectangle(OnlineIcon icon)
        {
            var (x, y, width, height) = icon.ToChatIcon().GetChatIconRectangleBounds().ValueOrDefault();
            return new Rectangle(x, y, width, height);
        }
    }
}
