using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Online;
using EOLib.Extensions;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Optional;
using Optional.Unsafe;
using System;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class PaperdollDialog : BaseEODialog
    {
        private static readonly Rectangle _iconDrawRect = new Rectangle(227, 258, 44, 21);

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IPaperdollProvider _paperdollProvider;
        private readonly IECFFileProvider _ecfFileProvider;
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
                               IPaperdollProvider paperdollProvider,
                               IECFFileProvider ecfFileProvider,
                               IEODialogButtonService eoDialogButtonService,
                               ICharacter character)
            : base(gameStateProvider)
        {
            _paperdollProvider = paperdollProvider;
            _ecfFileProvider = ecfFileProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            Character = character;

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

        // todo: equip/unequip
        //public void SetItem(EquipLocation loc, EIFRecord info)
        //{
        //    PaperdollDialogItem itemToUpdate = (PaperdollDialogItem)children.Find(_ctrl =>
        //    {
        //        PaperdollDialogItem item = _ctrl as PaperdollDialogItem;
        //        if (item == null) return false;
        //        return item.EquipLoc == loc;
        //    });
        //    if (itemToUpdate != null)
        //        itemToUpdate.SetInfo(_getEquipLocRectangle(loc), info);
        //}

        protected override void OnUpdateControl(GameTime gameTime)
        {
            base.OnUpdateControl(gameTime);

            _paperdollData.Match(
                some: paperdollData =>
                {
                    _paperdollData = paperdollData
                        .SomeWhen(d => _paperdollProvider.VisibleCharacterPaperdolls.ContainsKey(Character.ID) &&
                                      !_paperdollProvider.VisibleCharacterPaperdolls[Character.ID].Equals(d))
                        .Map(d =>
                        {
                            UpdateDisplayedData(d);
                            return d;
                        });
                },
                none: () =>
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
            _class.Text = Capitalize(_ecfFileProvider.ECFFile[paperdollData.Class].Name);
            _partner.Text = Capitalize(paperdollData.Partner);
            _title.Text = Capitalize(paperdollData.Title);
            _guild.Text = Capitalize(paperdollData.Guild);
            _rank.Text = Capitalize(paperdollData.Rank);

            // todo: item icons

            _characterIconSourceRect = Option.Some(GetOnlineIconSourceRectangle(paperdollData.Icon));
        }

        private static string Capitalize(string input) => 
            string.IsNullOrEmpty(input) ? string.Empty : char.ToUpper(input[0]) + input[1..].ToLower();

        private static Rectangle GetOnlineIconSourceRectangle(OnlineIcon icon)
        {
            var (x, y, width, height) = icon.ToChatIcon().GetChatIconRectangleBounds().ValueOrDefault();
            return new Rectangle(x, y, width, height);
        }

        private static Rectangle _getEquipLocRectangle(EquipLocation loc)
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
