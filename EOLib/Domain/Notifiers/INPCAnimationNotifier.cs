// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Notifiers
{
    public interface INPCAnimationNotifier
    {
        void StartNPCWalkAnimation(int npcIndex);

        void StartNPCAttackAnimation(int npcIndex);

        void RemoveNPCFromView(int npcIndex, bool showDeathAnimation);

        void ShowNPCSpeechBubble(int npcIndex, string message);
    }

    public class NoOpNpcAnimationNotifier : INPCAnimationNotifier
    {
        public void StartNPCWalkAnimation(int npcIndex) { }

        public void StartNPCAttackAnimation(int npcIndex) { }

        public void RemoveNPCFromView(int npcIndex, bool showDeathAnimation) { }

        public void ShowNPCSpeechBubble(int npcIndex, string message) { }
    }
}
