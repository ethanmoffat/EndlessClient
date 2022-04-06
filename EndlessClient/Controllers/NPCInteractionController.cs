﻿using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.NPC;
using EOLib.IO.Repositories;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class NPCInteractionController : INPCInteractionController
    {
        private readonly IMapNPCActions _mapNpcActions;
        private readonly IENFFileProvider _enfFileProvider;

        public NPCInteractionController(IMapNPCActions mapNpcActions,
                                        IENFFileProvider enfFileProvider)
        {
            _mapNpcActions = mapNpcActions;
            _enfFileProvider = enfFileProvider;
        }

        public void ShowNPCDialog(INPC npc)
        {
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
