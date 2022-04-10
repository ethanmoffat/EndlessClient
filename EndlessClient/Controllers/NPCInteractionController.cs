using AutomaticTypeMapper;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Actions;
using EOLib.Domain.Interact;
using EOLib.Domain.NPC;
using EOLib.IO.Repositories;
using System.Linq;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class NPCInteractionController : INPCInteractionController
    {
        private readonly IMapNPCActions _mapNpcActions;
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly IActiveDialogProvider _activeDialogProvider;

        public NPCInteractionController(IMapNPCActions mapNpcActions,
                                        IInGameDialogActions inGameDialogActions,
                                        IENFFileProvider enfFileProvider,
                                        IActiveDialogProvider activeDialogProvider)
        {
            _mapNpcActions = mapNpcActions;
            _inGameDialogActions = inGameDialogActions;
            _enfFileProvider = enfFileProvider;
            _activeDialogProvider = activeDialogProvider;
        }

        public void ShowNPCDialog(INPC npc)
        {
            if (_activeDialogProvider.ActiveDialogs.Any(x => x.HasValue))
                return;

            var data = _enfFileProvider.ENFFile[npc.ID];

            switch(data.Type)
            {
                case EOLib.IO.NPCType.Shop:
                    _mapNpcActions.RequestShop(npc);
                    break;
                case EOLib.IO.NPCType.Quest:
                    _mapNpcActions.RequestQuest(npc);
                    break;
                case EOLib.IO.NPCType.Bank:
                    _mapNpcActions.RequestBank(npc);
                    // note: the npc action types rely on a server response to show the dialog because they are driven
                    //       by config data on the server. Bank account dialog does not have this restriction;
                    //       interaction with the NPC should *always* show the dialog
                    _inGameDialogActions.ShowBankAccountDialog();
                    break;
            }
        }
    }

    public interface INPCInteractionController
    {
        void ShowNPCDialog(INPC npc);
    }
}
