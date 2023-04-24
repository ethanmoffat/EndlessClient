using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Skill;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class NPCInteractionActions : INPCInteractionNotifier
    {
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IECFFileProvider _ecfFileProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public NPCInteractionActions(IInGameDialogActions inGameDialogActions,
                                     IEOMessageBoxFactory messageBoxFactory,
                                     IECFFileProvider ecfFileProvider,
                                     ISfxPlayer sfxPlayer)
        {
            _inGameDialogActions = inGameDialogActions;
            _messageBoxFactory = messageBoxFactory;
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
    }
}
