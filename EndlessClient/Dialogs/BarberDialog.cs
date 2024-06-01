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
using System;
using EOLib.Domain.Notifiers;
using System.Collections.Generic;
using EndlessClient.GameExecution;
namespace EndlessClient.Dialogs
{
    public class BarberDialog : BaseEODialog
    {
        private readonly string[] _hairColorNames = { "brown", "green", "pink", "red", "blonde", "blue", "purple", "luna", "white", "black" };
        private readonly CreateCharacterControl _characterControl;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IBarberActions _barberActions;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;
        private const byte BuySellSfxId = 26;

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
                            IEnumerable<ISoundNotifier> soundNotifiers)
            : base(nativeGraphicsManager, isInGame: true)
        {
            _characterRepository = characterLvRepository;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _barberActions = barberActions;
            _characterInventoryProvider = characterInventoryProvider;
            _messageBoxFactory = messageBoxFactory;
            _eifFileProvider = eifFileProvider;
            _soundNotifiers = soundNotifiers;

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 56);
            _characterControl = new CreateCharacterControl(rendererFactory)
            {
                DrawPosition = new Vector2(210, 20)
            };

            InitializeCharacterControl();
            InitializeDialogItems(dialogButtonService);
            CenterInGameView();
        }

        private void InitializeCharacterControl()
        {
            var currentProperties = _characterRepository.MainCharacter.RenderProperties;
            _characterControl.SetParentControl(this);
            _characterControl.UpdateRenderProperties(currentProperties.HairStyle, currentProperties.HairColor,
                                                     currentProperties.Race, currentProperties.Gender,
                                                     currentProperties.ArmorGraphic, currentProperties.WeaponGraphic,
                                                     currentProperties.BootsGraphic, currentProperties.ShieldGraphic);
        }

        private void InitializeDialogItems(IEODialogButtonService dialogButtonService)
        {
            var cancel = CreateButton(dialogButtonService, new Vector2(215, 150), SmallButton.Cancel);
            cancel.OnClick += (_, _) => Close(XNADialogResult.Cancel);

            CreateListItems();
        }

        public override void Initialize()
        {
            base.Initialize();
            _characterControl.Initialize();
            UpdateListItems(_characterRepository.MainCharacter.RenderProperties);

        }
        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        private void CreateListItems()
        {
            _changeHairItem = CreateListDialogItem(DialogIcon.BarberHairModel, 25, Hair);
            _changeHairColor = CreateListDialogItem(DialogIcon.BarberChangeHairColor, 60, HairColor);
            _changeBuyHairStyleOrColor = CreateListDialogItem(DialogIcon.BarberOk, 95, BuyHairStyleOrColor);
        }

        private ListDialogItem CreateListDialogItem(DialogIcon icon, int offsetY, EventHandler<MonoGame.Extended.Input.InputListeners.MouseEventArgs> clickHandler)
        {
            var item = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                OffsetY = offsetY,
                IconGraphic = _dialogIconService.IconSheet,
                IconGraphicSource = _dialogIconService.GetDialogIconSource(icon),
                ShowIconBackGround = false,
                EnableBarberMode = true,
            };
            item.LeftClick += clickHandler;
            return item;
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

        private void Hair(object sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e) => UpdateHairStyle(1);
        private void HairColor(object sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e) => UpdateHairColor(1);

        private void UpdateHairStyle(int offset)
        {
            var currentProperties = _characterControl.RenderProperties;
            var newHairStyle = (currentProperties.HairStyle + offset) % 21;
            _characterControl.UpdateRenderProperties(newHairStyle, currentProperties.HairColor,
                                                     currentProperties.Race, currentProperties.Gender,
                                                     currentProperties.ArmorGraphic, currentProperties.WeaponGraphic,
                                                     currentProperties.BootsGraphic, currentProperties.ShieldGraphic);
            _changeHairItem.SubText = GetCurrentHairStyleText(newHairStyle);
        }

        private void UpdateHairColor(int offset)
        {
            var currentProperties = _characterControl.RenderProperties;
            var newHairColor = (currentProperties.HairColor + offset) % _hairColorNames.Length;
            _characterControl.UpdateRenderProperties(currentProperties.HairStyle, newHairColor,
                                                     currentProperties.Race, currentProperties.Gender,
                                                     currentProperties.ArmorGraphic, currentProperties.WeaponGraphic,
                                                     currentProperties.BootsGraphic, currentProperties.ShieldGraphic);
            _changeHairColor.SubText = GetCurrentHairColorText(newHairColor);
        }

        private void BuyHairStyleOrColor(object sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e) => PurchaseHairStyleOrColor(_characterControl.RenderProperties.HairStyle, _characterControl.RenderProperties.HairColor);

        private void PurchaseHairStyleOrColor(int hairStyle, int hairColor)
        {
            int totalCost = CalculateCost();
            int currentGold = _characterInventoryProvider.ItemInventory.SingleOrNone(i => i.ItemID == 1)
                                         .Map(i => i.Amount)
                                         .ValueOr(0);

            if (currentGold >= totalCost)
            {
                ShowConfirmationDialog(hairStyle, hairColor, totalCost);
            }
            else
            {
                ShowInsufficientFundsDialog();
            }
        }

        private void ShowConfirmationDialog(int hairStyle, int hairColor, int cost)
        {
            var message = $"{_localizedStringFinder.GetString(EOResourceID.DIALOG_BARBER_DO_YOU_WANT_TO_BUY_A_NEW_HAIRSTYLE)}, {cost} {_eifFileProvider.EIFFile[1].Name}";
            var title = _localizedStringFinder.GetString(EOResourceID.DIALOG_BARBER_BUY_HAIRSTYLE);
            var msgBox = _messageBoxFactory.CreateMessageBox(message, title, EODialogButtons.OkCancel);

            msgBox.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _barberActions.Purchase(hairStyle, hairColor);
                    foreach (var notifier in _soundNotifiers)
                        notifier.NotifySoundEffect(BuySellSfxId);
                }
            };

            msgBox.ShowDialog();
        }

        private void ShowInsufficientFundsDialog()
        {
            var msgBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.WARNING_YOU_HAVE_NOT_ENOUGH, $" {_eifFileProvider.EIFFile[1].Name}");
            msgBox.ShowDialog();
        }

    }
}