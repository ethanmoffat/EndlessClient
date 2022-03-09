using System.Collections.Generic;
using System.Linq;
using EndlessClient.GameExecution;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.Extensions;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.NPC
{
    public class NPCAnimator : GameComponent, INPCAnimator
    {
        private const int ACTION_FRAME_TIME_MS = 100;

        private readonly List<RenderFrameActionTime> _npcStartWalkingTimes;
        private readonly List<RenderFrameActionTime> _npcStartAttackingTimes;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public NPCAnimator(IEndlessGameProvider gameProvider,
                           ICurrentMapStateRepository currentMapStateRepository)
            : base((Game)gameProvider.Game)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _npcStartWalkingTimes = new List<RenderFrameActionTime>();
            _npcStartAttackingTimes = new List<RenderFrameActionTime>();
        }

        public override void Update(GameTime gameTime)
        {
            AnimateNPCWalking();
            AnimateNPCAttacking();

            base.Update(gameTime);
        }

        public void StartWalkAnimation(int npcIndex)
        {
            if (_npcStartWalkingTimes.Any(x => x.UniqueID == npcIndex))
                return;

            var startWalkingTimeAndID = new RenderFrameActionTime(npcIndex);

            _npcStartWalkingTimes.Add(startWalkingTimeAndID);
        }

        public void StartAttackAnimation(int npcIndex)
        {
            if (_npcStartAttackingTimes.Any(x => x.UniqueID == npcIndex))
                return;

            var startAttackingTimeAndID = new RenderFrameActionTime(npcIndex);

            _npcStartAttackingTimes.Add(startAttackingTimeAndID);
        }

        public void StopAllAnimations()
        {
            _npcStartWalkingTimes.Clear();
        }

        private void AnimateNPCWalking()
        {
            var npcsDoneWalking = new List<RenderFrameActionTime>();
            foreach (var pair in _npcStartWalkingTimes)
            {
                if (pair.ActionTimer.ElapsedMilliseconds >= ACTION_FRAME_TIME_MS)
                {
                    var npc = _currentMapStateRepository.NPCs.OptionalSingle(x => x.Index == pair.UniqueID);
                    if (!npc.HasValue)
                    {
                        npcsDoneWalking.Add(pair);
                        continue;
                    }

                    var nextFrameNPC = AnimateOneWalkFrame(npc.Value);
                    pair.UpdateActionStartTime();

                    if (nextFrameNPC.Frame == NPCFrame.Standing)
                        npcsDoneWalking.Add(pair);

                    _currentMapStateRepository.NPCs.Remove(npc.Value);
                    _currentMapStateRepository.NPCs.Add(nextFrameNPC);
                }
            }

            _npcStartWalkingTimes.RemoveAll(npcsDoneWalking.Contains);
        }

        private void AnimateNPCAttacking()
        {
            var npcsDoneAttacking = new List<RenderFrameActionTime>();
            foreach (var pair in _npcStartAttackingTimes)
            {
                if (pair.ActionTimer.ElapsedMilliseconds >= ACTION_FRAME_TIME_MS)
                {
                    var npc = _currentMapStateRepository.NPCs.OptionalSingle(x => x.Index == pair.UniqueID);
                    if (!npc.HasValue)
                    {
                        npcsDoneAttacking.Add(pair);
                        continue;
                    }

                    var nextFrameNPC = npc.Value.WithNextAttackFrame();
                    pair.UpdateActionStartTime();

                    if (nextFrameNPC.Frame == NPCFrame.Standing)
                        npcsDoneAttacking.Add(pair);

                    _currentMapStateRepository.NPCs.Remove(npc.Value);
                    _currentMapStateRepository.NPCs.Add(nextFrameNPC);
                }
            }

            _npcStartAttackingTimes.RemoveAll(npcsDoneAttacking.Contains);
        }

        private static INPC AnimateOneWalkFrame(INPC npc)
        {
            var nextFrameNPC = npc.WithNextWalkFrame();

            if (nextFrameNPC.IsActing(NPCActionState.Standing))
            {
                nextFrameNPC = nextFrameNPC
                    .WithX((byte)nextFrameNPC.GetDestinationX())
                    .WithY((byte)nextFrameNPC.GetDestinationY());
            }

            return nextFrameNPC;
        }
    }

    public interface INPCAnimator : IGameComponent
    {
        void StartWalkAnimation(int npcIndex);

        void StartAttackAnimation(int npcIndex);

        void StopAllAnimations();
    }
}
