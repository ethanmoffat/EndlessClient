using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Citizen;
using EOLib.Domain.Interact.Priest;
using EOLib.Domain.Interact.Skill;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System;
using XNAControls;


namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class NPCInteractionActions : INPCInteractionNotifier
    {
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly ICitizenActions _citizenActions;
        private readonly IPriestActions _priestActions;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IPubFileProvider _pubFileProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public NPCInteractionActions(IInGameDialogActions inGameDialogActions,
                                     ICitizenActions citizenActions,
                                     IPriestActions priestActions,
                                     IEOMessageBoxFactory messageBoxFactory,
                                     ITextInputDialogFactory textInputDialogFactory,
                                     ILocalizedStringFinder localizedStringFinder,
                                     IPubFileProvider pubFileProvider,
                                     ISfxPlayer sfxPlayer)
        {
            _inGameDialogActions = inGameDialogActions;
            _citizenActions = citizenActions;
            _priestActions = priestActions;
            _messageBoxFactory = messageBoxFactory;
            _textInputDialogFactory = textInputDialogFactory;
            _localizedStringFinder = localizedStringFinder;
            _pubFileProvider = pubFileProvider;
            _sfxPlayer = sfxPlayer;
        }

        public void NotifyInteractionFromNPC(NPCType npcType)
        {
            // originally, these methods were called directly from NPCInteractionController
            // however, this resulted in empty responses (e.g. no shop or quest) showing an empty dialog
            // instead, wait for the response packet to notify this class and then show the dialog
            //    once data has been received from the server
            switch (npcType)
            {
                case NPCType.Shop: _inGameDialogActions.ShowShopDialog(); break;
                case NPCType.Quest: _inGameDialogActions.ShowQuestDialog(); break;
                case NPCType.Skills: _inGameDialogActions.ShowSkillmasterDialog(); break;
                case NPCType.Inn: _inGameDialogActions.ShowInnkeeperDialog(); break;
                case NPCType.Law: _inGameDialogActions.ShowLawDialog(); break;
                case NPCType.Barber: _inGameDialogActions.ShowBarberDialog(); break;
                case NPCType.Priest: ShowPriestDialog(); break;
                case NPCType.Guild: _inGameDialogActions.ShowGuildDialog(); break;
            }
        }

        public void NotifySkillLearnSuccess(int spellId, int characterGold)
        {
            _sfxPlayer.PlaySfx(SoundEffectID.LearnNewSpell);
        }

        public void NotifySkillLearnFail(SkillmasterReply skillmasterReply, int classId)
        {
            switch (skillmasterReply)
            {
                //not sure if this will ever actually be sent because client validates data before trying to learn a skill
                case SkillmasterReply.ErrorWrongClass:
                    {
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_LEARN_WRONG_CLASS, $" {_pubFileProvider.ECFFile[classId].Name}!");
                        dlg.ShowDialog();
                    }
                    break;
                case SkillmasterReply.ErrorRemoveItems:
                    {
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_RESET_CHARACTER_CLEAR_PAPERDOLL);
                        dlg.ShowDialog();
                    }
                    break;
            }
        }

        public void NotifySkillForget()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_FORGET_SUCCESS);
            dlg.ShowDialog();
        }

        public void NotifyStatReset()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_RESET_CHARACTER_COMPLETE);
            dlg.ShowDialog();

            _sfxPlayer.PlaySfx(SoundEffectID.LearnNewSpell);
        }

        public void NotifyCitizenUnsubscribe(InnUnsubscribeReply reply)
        {
            var message = EOResourceID.INN_YOU_ARE_NOT_A_CITIZEN + (int)reply;
            var dlg = _messageBoxFactory.CreateMessageBox(message, EOResourceID.INN_REGISTRATION_SERVICE);
            dlg.ShowDialog();
        }

        public void NotifyCitizenSignUp(int questionsWrong)
        {
            if (questionsWrong == 0)
            {
                _sfxPlayer.PlaySfx(SoundEffectID.InnSignUp);

                var dlg = _messageBoxFactory.CreateMessageBox(EOResourceID.INN_BECOME_CITIZEN_CONGRATULATIONS, EOResourceID.INN_REGISTRATION_SERVICE);
                dlg.ShowDialog();
            }
            else
            {
                var dlg = _messageBoxFactory.CreateMessageBox(
                    $"{questionsWrong} {_localizedStringFinder.GetString(EOResourceID.INN_YOUR_ANSWERS_WERE_INCORRECT)}",
                    _localizedStringFinder.GetString(EOResourceID.INN_REGISTRATION_SERVICE));
                dlg.ShowDialog();
            }
        }

        public void NotifyCitizenRequestSleep(int sleepCost)
        {
            var message = $"{_localizedStringFinder.GetString(EOResourceID.INN_A_GOOD_NIGHT_REST_WILL_COST_YOU)} {sleepCost} Gold";

            var dlg = _messageBoxFactory.CreateMessageBox(message, _localizedStringFinder.GetString(EOResourceID.INN_SLEEP), EODialogButtons.OkCancel);
            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _citizenActions.ConfirmSleep();
                }
            };

            dlg.ShowDialog();
        }

        public void NotifyPriestReply(PriestReply reply)
        {
            var message = reply switch
            {
                PriestReply.NotDressed => DialogResourceID.WEDDING_NEED_PROPER_CLOTHES,
                PriestReply.LowLevel => DialogResourceID.WEDDING_NEED_HIGHER_LEVEL,
                PriestReply.PartnerNotPresent => DialogResourceID.WEDDING_PARTNER_IS_MISSING,
                PriestReply.PartnerNotDressed => DialogResourceID.WEDDING_PARTNER_NEED_CLOTHES,
                PriestReply.Busy => DialogResourceID.WEDDING_PRIEST_IS_BUSY,
                PriestReply.DoYou => DialogResourceID.WEDDING_DO_YOU_ACCEPT,
                PriestReply.PartnerAlreadyMarried => DialogResourceID.WEDDING_PARTNER_ALREADY_MARRIED,
                PriestReply.NoPermission => DialogResourceID.WEDDING_NO_PERMISSION_TO_COMPLETE,
                _ => throw new ArgumentOutOfRangeException(nameof(reply))
            };

            var dlg = _messageBoxFactory.CreateMessageBox(message, whichButtons: reply == PriestReply.DoYou ? EODialogButtons.OkCancel : EODialogButtons.Ok);
            if (reply == PriestReply.DoYou)
            {
                dlg.DialogClosing += (_, e) =>
                {
                    if (e.Result != XNADialogResult.OK)
                        return;

                    _priestActions.ConfirmMarriage();
                };
            }
            dlg.ShowDialog();
        }

        public void NotifyPriestRequest(string partnerName)
        {
            // The dialog title "Ceremony Notice" is not paired with the message, which is in EOResourceID and not a proper dialog text (????)
            var ceremonyNoticeCaption = _localizedStringFinder.GetString(DialogResourceID.WEDDING_NEED_PROPER_CLOTHES);
            var ceremonyNoticeMessage = $"{char.ToUpper(partnerName[0]) + partnerName[1..]} {_localizedStringFinder.GetString(EOResourceID.WEDDING_IS_ASKING_YOU_TO_MARRY)}";

            var dlg = _messageBoxFactory.CreateMessageBox(ceremonyNoticeMessage, ceremonyNoticeCaption, EODialogButtons.OkCancel);
            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result != XNADialogResult.OK)
                    return;

                _priestActions.AcceptRequest();
            };
            dlg.ShowDialog();
        }

        public void NotifyMarriageReply(MarriageReply reply)
        {
            var message = reply switch
            {
                MarriageReply.AlreadyMarried => DialogResourceID.WEDDING_YOU_ALREADY_MARRIED,
                MarriageReply.NotMarried => DialogResourceID.WEDDING_CANNOT_DIVORCE_NO_PARTNER,
                MarriageReply.Success => DialogResourceID.WEDDING_REGISTRATION_COMPLETE,
                MarriageReply.NotEnoughGold => DialogResourceID.WARNING_YOU_HAVE_NOT_ENOUGH,
                MarriageReply.WrongName => DialogResourceID.WEDDING_PARTNER_NOT_MATCH,
                MarriageReply.ServiceBusy => DialogResourceID.WEDDING_REGISTRATION_TOO_BUSY,
                MarriageReply.DivorceNotification => DialogResourceID.WEDDING_DIVORCE_NO_MORE_PARTNER,
                _ => throw new ArgumentOutOfRangeException(nameof(reply))
            };

            var dlg = _messageBoxFactory.CreateMessageBox(message, reply == MarriageReply.NotEnoughGold ? $" {_pubFileProvider.EIFFile[1].Name}" : string.Empty);
            dlg.ShowDialog();
        }

        private void ShowPriestDialog()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.WEDDING_PRIEST_CAN_PERFORM, EODialogButtons.OkCancel, EOMessageBoxStyle.LargeDialogSmallHeader);
            dlg.DialogClosing += (_, e1) =>
            {
                if (e1.Result != XNADialogResult.OK)
                    return;

                var inputDlg = _textInputDialogFactory.Create(_localizedStringFinder.GetString(EOResourceID.WEDDING_PLEASE_ENTER_NAME));
                inputDlg.DialogClosing += (_, e2) =>
                {
                    if (e2.Result != XNADialogResult.OK)
                        return;

                    _priestActions.RequestMarriage(inputDlg.ResponseText);
                };

                inputDlg.ShowDialog();
            };
            dlg.ShowDialog();
        }
    }
}
