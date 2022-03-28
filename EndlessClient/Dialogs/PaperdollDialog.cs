using EndlessClient.Controllers;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Inventory;
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
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class PaperdollDialog : BaseEODialog
    {
        private static readonly Rectangle _iconDrawRect = new Rectangle(227, 258, 44, 21);

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IInventoryController _inventoryController;
        private readonly IPaperdollProvider _paperdollProvider;
        private readonly IPubFileProvider _pubFileProvider;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly bool _isMainCharacter;
        private readonly Texture2D _characterIconSheet;
        private readonly Texture2D _background;
        private Option<Rectangle> _characterIconSourceRect;

        private Option<IPaperdollData> _paperdollData;

        private readonly IXNALabel _name,
            _home,
            _class,
            _partner,
            _title,
            _guild,
            _rank;

        public ICharacter Character { get; }

        public PaperdollDialog(IGameStateProvider gameStateProvider,
                               INativeGraphicsManager nativeGraphicsManager,
                               IInventoryController inventoryController,
                               IPaperdollProvider paperdollProvider,
                               IPubFileProvider pubFileProvider,
                               IEODialogButtonService eoDialogButtonService,
                               IInventorySpaceValidator inventorySpaceValidator,
                               IEOMessageBoxFactory eoMessageBoxFactory,
                               IStatusLabelSetter statusLabelSetter,
                               ICharacter character, bool isMainCharacter)
            : base(gameStateProvider)
        {
            _paperdollProvider = paperdollProvider;
            _pubFileProvider = pubFileProvider;
            _inventorySpaceValidator = inventorySpaceValidator;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
            _nativeGraphicsManager = nativeGraphicsManager;
            _inventoryController = inventoryController;
            Character = character;
            _isMainCharacter = isMainCharacter;
            _characterIconSheet = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true);
            _characterIconSourceRect = Option.None<Rectangle>();

            _background = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 49);
            SetSize(_background.Width, _background.Height / 2);

            // this needs to be set for CenterInGameView to work properly
            // todo: fix
            BackgroundTexture = new Texture2D(GraphicsDevice, DrawArea.Width, DrawArea.Height);

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
            DrawPosition = new Vector2(DrawPosition.X, 15);

            _paperdollData = Option.None<IPaperdollData>();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            base.OnUpdateControl(gameTime);

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

            // todo: dragging to equip/unequip from inventory
            /*
            if (EOGame.Instance.Hud.IsInventoryDragging())
            {
                shouldClickDrag = false;
                SuppressParentClickDrag(true);
            }
            else
            {
                shouldClickDrag = true;
                SuppressParentClickDrag(false);
            }
            */
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _spriteBatch.Draw(_background, DrawAreaWithParentOffset, new Rectangle(0, DrawArea.Height * Character.RenderProperties.Gender, DrawArea.Width, DrawArea.Height), Color.White);

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

            base.OnDrawControl(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                BackgroundTexture.Dispose();

            base.Dispose(disposing);
        }

        private void UpdateDisplayedData(IPaperdollData paperdollData)
        {
            _name.Text = Capitalize(paperdollData.Name);
            _home.Text = Capitalize(paperdollData.Home);
            _class.Text = Capitalize(_pubFileProvider.ECFFile[paperdollData.Class].Name);
            _partner.Text = Capitalize(paperdollData.Partner);
            _title.Text = Capitalize(paperdollData.Title);
            _guild.Text = Capitalize(paperdollData.Guild);
            _rank.Text = Capitalize(paperdollData.Rank);

            var paperdollDialogItems = ChildControls.OfType<PaperdollDialogItem>().ToList();

            foreach (var control in paperdollDialogItems)
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
                var paperdollItem = new PaperdollDialogItem(_nativeGraphicsManager, _isMainCharacter, equipLocation, eifRecord)
                {
                    DrawArea = GetEquipLocationRectangle(equipLocation)
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
                            if (!_inventorySpaceValidator.ItemFits(rec.Size))
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

        private static Rectangle GetEquipLocationRectangle(EquipLocation loc)
        {
            switch (loc)
            {
                case EquipLocation.Boots: return new Rectangle(87, 220, 56, 54);
                case EquipLocation.Accessory: return new Rectangle(55, 250, 23, 23);
                case EquipLocation.Gloves: return new Rectangle(22, 188, 56, 54);
                case EquipLocation.Belt: return new Rectangle(87, 188, 56, 23);
                case EquipLocation.Armor: return new Rectangle(86, 82, 56, 98);
                case EquipLocation.Necklace: return new Rectangle(152, 51, 56, 23);
                case EquipLocation.Hat: return new Rectangle(87, 21, 56, 54);
                case EquipLocation.Shield: return new Rectangle(152, 82, 56, 98);
                case EquipLocation.Weapon: return new Rectangle(22, 82, 56, 98);
                case EquipLocation.Ring1: return new Rectangle(152, 190, 23, 23);
                case EquipLocation.Ring2: return new Rectangle(185, 190, 23, 23);
                case EquipLocation.Armlet1: return new Rectangle(152, 220, 23, 23);
                case EquipLocation.Armlet2: return new Rectangle(185, 220, 23, 23);
                case EquipLocation.Bracer1: return new Rectangle(152, 250, 23, 23);
                case EquipLocation.Bracer2: return new Rectangle(185, 250, 23, 23);
                default: throw new ArgumentOutOfRangeException(nameof(loc), "That is not a valid equipment location");
            }
        }
    }
}
