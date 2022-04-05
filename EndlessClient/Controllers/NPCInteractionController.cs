using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EOLib.Domain.Interact;
using EOLib.Domain.NPC;
using EOLib.IO.Repositories;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class NPCInteractionController : INPCInteractionController
    {
        private readonly IMapNPCActions _mapNpcActions;
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IENFFileProvider _enfFileProvider;

        public NPCInteractionController(IMapNPCActions mapNpcActions,
                                        IInGameDialogActions inGameDialogActions,
                                        IENFFileProvider enfFileProvider)
        {
            _mapNpcActions = mapNpcActions;
            _inGameDialogActions = inGameDialogActions;
            _enfFileProvider = enfFileProvider;
        }

        public void ShowNPCDialog(INPC npc)
        {
            var data = _enfFileProvider.ENFFile[npc.ID];

            switch(data.Type)
            {
                case EOLib.IO.NPCType.Shop:
                    _mapNpcActions.RequestShop(npc.Index);
                    _inGameDialogActions.ShowShopDialog();
                    break;
            }
        }
    }

    public interface INPCInteractionController
    {
        void ShowNPCDialog(INPC npc);
    }
}
