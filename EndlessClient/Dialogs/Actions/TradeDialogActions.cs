using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Config;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Trade;
using EOLib.Localization;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class TradeDialogActions : ITradeEventNotifier
    {
        private readonly ITradeActions _tradeActions;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IConfigurationProvider _configurationProvider;

        public TradeDialogActions(ITradeActions tradeActions,
                                  IEOMessageBoxFactory messageBoxFactory,
                                  IConfigurationProvider configurationProvider)
        {
            _tradeActions = tradeActions;
            _messageBoxFactory = messageBoxFactory;
            _configurationProvider = configurationProvider;
        }

        public void NotifyTradeRequest(short playerId, string name)
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
            // todo: show trade dialog

            // todo: status label
            //m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_TRADE_YOU_ARE_TRADING_WITH,
            //        otherName + " " + OldWorld.GetString(EOResourceID.STATUS_LABEL_DRAG_AND_DROP_ITEMS));
        }

        public void NotifyTradeClose(bool cancel)
        {
            // todo: close trade dialog

            if (cancel)
            {
                //m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_TRADE_ABORTED);
            }
            else
            {
                //EOMessageBox.Show(DialogResourceID.TRADE_SUCCESS, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
            }
        }
    }
}
