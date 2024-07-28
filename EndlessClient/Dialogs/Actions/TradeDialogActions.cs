using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Trade;
using EOLib.Localization;
using XNAControls;

namespace EndlessClient.Dialogs.Actions;

[AutoMappedType]
public class TradeDialogActions : ITradeEventNotifier
{
    private readonly ITradeActions _tradeActions;
    private readonly IInGameDialogActions _inGameDialogActions;
    private readonly IEOMessageBoxFactory _messageBoxFactory;
    private readonly IStatusLabelSetter _statusLabelSetter;
    private readonly ILocalizedStringFinder _localizedStringFinder;
    private readonly ITradeProvider _tradeProvider;
    private readonly ICharacterProvider _characterProvider;
    private readonly IConfigurationProvider _configurationProvider;

    public TradeDialogActions(ITradeActions tradeActions,
                              IInGameDialogActions inGameDialogActions,
                              IEOMessageBoxFactory messageBoxFactory,
                              IStatusLabelSetter statusLabelSetter,
                              ILocalizedStringFinder localizedStringFinder,
                              ITradeProvider tradeProvider,
                              ICharacterProvider characterProvider,
                              IConfigurationProvider configurationProvider)
    {
        _tradeActions = tradeActions;
        _inGameDialogActions = inGameDialogActions;
        _messageBoxFactory = messageBoxFactory;
        _statusLabelSetter = statusLabelSetter;
        _localizedStringFinder = localizedStringFinder;
        _tradeProvider = tradeProvider;
        _characterProvider = characterProvider;
        _configurationProvider = configurationProvider;
    }

    public void NotifyTradeRequest(int playerId, string name)
    {
        if (!_configurationProvider.Interaction)
            return;

        var dlg = _messageBoxFactory.CreateMessageBox(char.ToUpper(name[0]) + name[1..] + " ", DialogResourceID.TRADE_REQUEST, EODialogButtons.OkCancel);
        dlg.DialogClosing += (_, e) =>
        {
            if (e.Result == XNADialogResult.OK)
            {
                _tradeActions.AcceptTradeRequest(playerId);
            }
        };

        dlg.ShowDialog();
    }

    public void NotifyTradeAccepted()
    {
        _inGameDialogActions.ShowTradeDialog();

        var otherName = _tradeProvider.PlayerOneOffer.PlayerID == _characterProvider.MainCharacter.ID
            ? _tradeProvider.PlayerOneOffer.PlayerName
            : _tradeProvider.PlayerTwoOffer.PlayerName;
        _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
            EOResourceID.STATUS_LABEL_TRADE_YOU_ARE_TRADING_WITH,
            $"{otherName} {_localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_DRAG_AND_DROP_ITEMS)}");
    }

    public void NotifyTradeClose(bool cancel)
    {
        _inGameDialogActions.CloseTradeDialog();

        if (cancel)
        {
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_TRADE_ABORTED);
        }
        else
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.TRADE_SUCCESS);
            dlg.ShowDialog();
        }
    }
}