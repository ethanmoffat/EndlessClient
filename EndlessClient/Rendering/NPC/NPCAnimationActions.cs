// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EOLib.Domain.NPC;

namespace EndlessClient.Rendering.NPC
{
    public class NPCAnimationActions : INPCAnimationNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;

        public NPCAnimationActions(IHudControlProvider hudControlProvider)
        {
            _hudControlProvider = hudControlProvider;
        }

        public void StartNPCWalkAnimation(int npcIndex)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            var npcAnimator = _hudControlProvider.GetComponent<INPCAnimator>(HudControlIdentifier.NPCAnimator);
            npcAnimator.StartWalkAnimation(npcIndex);
        }
    }
}
