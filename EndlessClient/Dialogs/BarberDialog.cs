using EndlessClient.Dialogs.Services;
using EOLib.Graphics;
using EndlessClient.UIControls;
using EndlessClient.Rendering.Factories;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using XNAControls;
using EOLib.Domain.Character;
using System.Diagnostics;
using EndlessClient.Dialogs;
using EOLib.Localization;
using System;
using EOLib.Domain.Interact.Barber;

namespace EndlessClient.Dialogs
{
    public class BarberDialog : BaseEODialog
    {
        private readonly string[] _hairColorNames = new string[] {
        "brown", "green", "pink", "red", "blonde", "blue", "purple", "luna", "white", "black"
    };

        private readonly CreateCharacterControl _characterControl;
        private readonly ICharacterRepository _characterRepository; 
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder; 
        private readonly IBarberActions _barberActions;

        private CharacterRenderProperties RenderProperties => _characterControl.RenderProperties;

        public BarberDialog(INativeGraphicsManager nativeGraphicsManager,
                            ICharacterRendererFactory rendererFactory,
                            IEODialogButtonService dialogButtonService,
                            ICharacterRepository characterRepository,
                            IEODialogIconService dialogIconService,
                            ILocalizedStringFinder localizedStringFinder,
                            IBarberActions barberActions)
            : base(nativeGraphicsManager, isInGame: true)
        {
            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 56);
            _characterRepository = characterRepository;

            ListItemType = ListDialogItem.ListItemStyle.Large;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _barberActions = barberActions;

            _characterControl = new CreateCharacterControl(rendererFactory)
            {
                DrawPosition = new Vector2(210, 20)
            };
            _characterControl.SetParentControl(this);

            var cancel = new XNAButton(dialogButtonService.SmallButtonSheet, new Vector2(215, 150),
                                       dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                                       dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            cancel.Initialize();
            cancel.SetParentControl(this);
            cancel.OnClick += (_, _) => Close(XNADialogResult.Cancel);

            CenterInGameView();
        }

        private ListDialogItem _changeHairItem;
        private ListDialogItem _changeHairColor;
        private ListDialogItem _changeBuyHairStyleOrColor;

        public override void Initialize()
        {
            base.Initialize();
            var currentProperties = _characterRepository.MainCharacter.RenderProperties;
            var stats = _characterRepository.MainCharacter.Stats;
            int level = (int)stats[CharacterStat.Level];
            int base_cost = 200;
            int cost_per_level = 200;

            _changeHairItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_BARBER_CHANGE_MODAL),
                SubText = _localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_CURRENT) + $": {currentProperties.HairStyle}",
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.BarberHairModel),
                OffsetX = -2,
                OffsetY = 25,
                DrawingOffsetX = 3,
                ShowIconBackGround = false,
            };
            _changeHairItem.LeftClick += Hair;

            _changeHairColor = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_BARBER_CHANGE_HAIR_COLOR),
                SubText = _localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_CURRENT) + $": {_hairColorNames[currentProperties.HairColor % _hairColorNames.Length]}",
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.BarberChangeHairColor),
                OffsetX = -2,
                OffsetY = 60,
                DrawingOffsetX = 3,
                ShowIconBackGround = false,
            };
            _changeHairColor.LeftClick += HairColor;

            _changeBuyHairStyleOrColor = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_BARBER_BUY_HAIRSTYLE),
                SubText = _localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_CURRENT) + $": {base_cost + Math.Max(level - 1, 0) * cost_per_level}",
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.BarberOk),
                OffsetX = -2,
                OffsetY = 95,
                DrawingOffsetX = 3,
                ShowIconBackGround = false,
            };

            _changeBuyHairStyleOrColor.LeftClick += BuyHairStyleOrColor;

            _characterControl.Initialize();
            _characterControl.SetParentControl(this);

            _characterControl.UpdateRenderProperties(currentProperties.HairStyle, currentProperties.HairColor,
                                                     currentProperties.Race, currentProperties.Gender);
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            _characterControl?.Draw(gameTime);
        }

        private void Hair(object sender, EventArgs e)
        {
            var currentProperties = _characterControl.RenderProperties;
            var newHairStyle = (currentProperties.HairStyle + 1) % 10;
            _characterControl.UpdateRenderProperties(newHairStyle, currentProperties.HairColor, currentProperties.Race, currentProperties.Gender);
            _changeHairItem.SubText = _localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_CURRENT) + $": {newHairStyle}";
        }

        private void HairColor(object sender, EventArgs e)
        {
            var currentProperties = _characterControl.RenderProperties;
            var newHairColor = (currentProperties.HairColor + 1) % 10;

            _characterControl.UpdateRenderProperties(currentProperties.HairStyle, newHairColor, currentProperties.Race, currentProperties.Gender);
            _changeHairColor.SubText = _localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_CURRENT) + $": {_hairColorNames[newHairColor]}";
        }

        private void BuyHairStyleOrColor(object sender, EventArgs e)
        {
            var currentProperties = _characterControl.RenderProperties;

            PurchaseHairStyleOrColor(currentProperties.HairStyle, currentProperties.HairColor);
        }

        private void PurchaseHairStyleOrColor(int hairStyle, int hairColor)
        {
            _barberActions.SayHello(hairStyle, hairColor);
        }
    }
}
