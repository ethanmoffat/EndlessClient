using System;
using EndlessClient.Dialogs.Services;
using EOLib.Graphics;
using EndlessClient.UIControls;
using EndlessClient.Rendering.Factories;
using Microsoft.Xna.Framework;
using XNAControls;
using EOLib.Domain.Character;
using EOLib.Localization;
using EOLib.Domain.Interact.Barber;
using EndlessClient.Dialogs.Factories;
using Optional.Collections;
using EOLib.IO.Repositories;
using EndlessClient.Audio;

namespace EndlessClient.Dialogs
{
    public class BarberDialog : BaseEODialog
    {
        private const int AdjustedWidth = 175;
        private const int AdjustedHighlightXOffset = 3;
        private readonly string[] _hairColorNames = { "brown", "green", "pink", "red", "blonde", "blue", "purple", "luna", "white", "black" };
        private readonly CreateCharacterControl _characterControl;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IBarberActions _barberActions;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ISfxPlayer _sfxPlayer;

        private CharacterRenderProperties RenderProperties => _characterControl.RenderProperties;
        private ListDialogItem _changeHairItem, _changeHairColor, _changeBuyHairStyleOrColor;

        public BarberDialog(INativeGraphicsManager nativeGraphicsManager,
                            ICharacterRendererFactory rendererFactory,
                            IEODialogButtonService dialogButtonService,
                            ICharacterRepository characterLvRepository,
                            IEODialogIconService dialogIconService,
                            ILocalizedStringFinder localizedStringFinder,
                            IBarberActions barberActions,
                            ICharacterInventoryProvider characterInventoryProvider,
                            IEOMessageBoxFactory messageBoxFactory,
                            IEIFFileProvider eifFileProvider,
                            ISfxPlayer sfxPlayer)
            : base(nativeGraphicsManager, isInGame: true)
        {
            _characterRepository = characterLvRepository;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _barberActions = barberActions;
            _characterInventoryProvider = characterInventoryProvider;
            _messageBoxFactory = messageBoxFactory;
            _eifFileProvider = eifFileProvider;
            _sfxPlayer = sfxPlayer;

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 56);

            var mainCharacterRenderProperties = _characterRepository.MainCharacter.RenderProperties;
            _characterControl = new CreateCharacterControl(mainCharacterRenderProperties, rendererFactory)
            {
                DrawPosition = new Vector2(210, 19),
            };

            InitializeCharacterControl();
            InitializeDialogItems(dialogButtonService);
            CenterInGameView();
        }

        private void InitializeCharacterControl()
        {
            _characterControl.SetParentControl(this);
        }

        private void InitializeDialogItems(IEODialogButtonService dialogButtonService)
        {
            var cancel = CreateButton(dialogButtonService, new Vector2(215, 151), SmallButton.Cancel);
            cancel.OnClick += (_, _) => Close(XNADialogResult.Cancel);

            _changeHairItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.BarberHairModel),
                ShowIconBackGround = false,
                OffsetX = AdjustedHighlightXOffset,
            };
            _changeHairItem.DrawArea = _changeHairItem.DrawArea.WithSize(AdjustedWidth, _changeHairItem.DrawArea.Size.Y);
            _changeHairItem.LeftClick += ChangeHairStyle_Click;

            _changeHairColor = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
            {
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.BarberChangeHairColor),
                ShowIconBackGround = false,
                OffsetX = AdjustedHighlightXOffset,
            };
            _changeHairColor.DrawArea = _changeHairColor.DrawArea.WithSize(AdjustedWidth, _changeHairColor.DrawArea.Size.Y);
            _changeHairColor.LeftClick += ChangeHairColor_Click;

            _changeBuyHairStyleOrColor = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
            {
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.BarberOk),
                ShowIconBackGround = false,
                OffsetX = AdjustedHighlightXOffset,
            };
            _changeBuyHairStyleOrColor.DrawArea = _changeBuyHairStyleOrColor.DrawArea.WithSize(AdjustedWidth, _changeBuyHairStyleOrColor.DrawArea.Size.Y);
            _changeBuyHairStyleOrColor.LeftClick += BuyHair_Click;
        }

        public override void Initialize()
        {
            base.Initialize();
            _characterControl.Initialize();
            UpdateListItems(_characterRepository.MainCharacter.RenderProperties);
        }

        private void UpdateListItems(CharacterRenderProperties currentProperties)
        {
            _changeHairItem.PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_BARBER_CHANGE_MODAL);
            _changeHairItem.SubText = GetCurrentHairStyleText(currentProperties.HairStyle);

            _changeHairColor.PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_BARBER_CHANGE_HAIR_COLOR);
            _changeHairColor.SubText = GetCurrentHairColorText(currentProperties.HairColor);

            _changeBuyHairStyleOrColor.PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_BARBER_BUY_HAIRSTYLE);
            _changeBuyHairStyleOrColor.SubText = GetCostText();
        }

        private string GetCurrentHairStyleText(int hairStyle) => $"{_localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_CURRENT)}: {hairStyle}";
        private string GetCurrentHairColorText(int hairColor) => $"{_localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_CURRENT)}: {_hairColorNames[hairColor % _hairColorNames.Length]}";
        private string GetCostText() => $"{_localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_CURRENT)}: {CalculateCost()} gold";

        private int CalculateCost()
        {
            var level = (int)_characterRepository.MainCharacter.Stats[CharacterStat.Level];
            return 200 + Math.Max(level - 1, 0) * 200;
        }

        private void ChangeHairStyle_Click(object sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
        {
            _characterControl.NextHairStyle();
            _changeHairItem.SubText = GetCurrentHairStyleText(_characterControl.RenderProperties.HairStyle);
        }

        private void ChangeHairColor_Click(object sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
        {
            _characterControl.NextHairColor();
            _changeHairColor.SubText = GetCurrentHairColorText(_characterControl.RenderProperties.HairColor);
        }

        private void BuyHair_Click(object sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
        {
            int hairStyle = _characterControl.RenderProperties.HairStyle;
            int hairColor = _characterControl.RenderProperties.HairColor;

            int totalCost = CalculateCost();
            int currentGold = _characterInventoryProvider.ItemInventory.SingleOrNone(i => i.ItemID == 1)
                                         .Map(i => i.Amount)
                                         .ValueOr(0);

            if (currentGold >= totalCost)
            {
                var message = $"{_localizedStringFinder.GetString(EOResourceID.DIALOG_BARBER_DO_YOU_WANT_TO_BUY_A_NEW_HAIRSTYLE)}, {totalCost} {_eifFileProvider.EIFFile[1].Name}";
                var title = _localizedStringFinder.GetString(EOResourceID.DIALOG_BARBER_BUY_HAIRSTYLE);
                var msgBox = _messageBoxFactory.CreateMessageBox(message, title, EODialogButtons.OkCancel);

                msgBox.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                    {
                        _barberActions.Purchase(hairStyle, hairColor);
                        _sfxPlayer.PlaySfx(SoundEffectID.BuySell);
                    }
                };

                msgBox.ShowDialog();
            }
            else
            {
                var msgBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.WARNING_YOU_HAVE_NOT_ENOUGH, $" {_eifFileProvider.EIFFile[1].Name}");
                msgBox.ShowDialog();
            }
        }

        private XNAButton CreateButton(IEODialogButtonService dialogButtonService, Vector2 position, SmallButton buttonType)
        {
            var button = new XNAButton(dialogButtonService.SmallButtonSheet, position,
                                       dialogButtonService.GetSmallDialogButtonOutSource(buttonType),
                                       dialogButtonService.GetSmallDialogButtonOverSource(buttonType));
            button.Initialize();
            button.SetParentControl(this);
            return button;
        }
    }
}
