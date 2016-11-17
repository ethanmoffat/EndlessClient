// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EOLib.Domain.Notifiers;

namespace EndlessClient.Rendering.NPC
{
    public class NPCAnimationActions : INPCAnimationNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly INPCStateCache _npcStateCache;
        private readonly INPCRendererRepository _npcRendererRepository;

        public NPCAnimationActions(IHudControlProvider hudControlProvider,
                                   INPCStateCache npcStateCache,
                                   INPCRendererRepository npcRendererRepository)
        {
            _hudControlProvider = hudControlProvider;
            _npcStateCache = npcStateCache;
            _npcRendererRepository = npcRendererRepository;
        }

        public void StartNPCWalkAnimation(int npcIndex)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            var npcAnimator = _hudControlProvider.GetComponent<INPCAnimator>(HudControlIdentifier.NPCAnimator);
            npcAnimator.StartWalkAnimation(npcIndex);
        }

        public void RemoveNPCFromView(int npcIndex, bool showDeathAnimation)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            _npcStateCache.RemoveStateByIndex(npcIndex);

            if (!showDeathAnimation)
            {
                _npcRendererRepository.NPCRenderers[npcIndex].Dispose();
                _npcRendererRepository.NPCRenderers.Remove(npcIndex);
            }
            else
            {
                _npcRendererRepository.NPCRenderers[npcIndex].StartDying();
            }
        }
    }
}
