using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Citizen;
using EOLib.Domain.Interact.Skill;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class NPCInteractionActions : INPCInteractionNotifier
    {
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly ICitizenActions _citizenActions;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IECFFileProvider _ecfFileProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public NPCInteractionActions(IInGameDialogActions inGameDialogActions,
                                     ICitizenActions citizenActions,
                                     IEOMessageBoxFactory messageBoxFactory,
                                     ILocalizedStringFinder localizedStringFinder,
                                     IECFFileProvider ecfFileProvider,
                                     ISfxPlayer sfxPlayer)
        {
            _inGameDialogActions = inGameDialogActions;
            _citizenActions = citizenActions;
            _messageBoxFactory = messageBoxFactory;
            _localizedStringFinder = localizedStringFinder;
            _ecfFileProvider = ecfFileProvider;
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
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SKILL_LEARN_WRONG_CLASS, $" {_ecfFileProvider.ECFFile[classId].Name}!");
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

        public void NotifyCitizenUnsubscribe(CitizenUnsubscribeReply reply)
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
    }
}
