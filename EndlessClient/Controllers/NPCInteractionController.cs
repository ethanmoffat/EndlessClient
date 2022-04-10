using AutomaticTypeMapper;
using EndlessClient.Dialogs;
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
        private readonly IENFFileProvider _enfFileProvider;
        private readonly IActiveDialogProvider _activeDialogProvider;

        public NPCInteractionController(IMapNPCActions mapNpcActions,
                                        IENFFileProvider enfFileProvider,
                                        IActiveDialogProvider activeDialogProvider)
        {
            _mapNpcActions = mapNpcActions;
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
            }
        }
    }

    public interface INPCInteractionController
    {
        void ShowNPCDialog(INPC npc);
    }
}
