using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using Microsoft.Xna.Framework;
using Optional.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Rendering.NPC
{
    public class NPCAnimator : GameComponent, INPCAnimator
    {
        private const int TICKS_PER_ACTION_FRAME = 8; // 8 x10ms ticks per action frame

        private readonly List<RenderFrameActionTime> _npcStartWalkingTimes;
        private readonly List<RenderFrameActionTime> _npcStartAttackingTimes;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IFixedTimeStepRepository _fixedTimeStepRepository;

        public NPCAnimator(IEndlessGameProvider gameProvider,
                           ICurrentMapStateRepository currentMapStateRepository,
                           IFixedTimeStepRepository fixedTimeStepRepository)
            : base((Game)gameProvider.Game)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _fixedTimeStepRepository = fixedTimeStepRepository;
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

            var startWalkingTimeAndID = new RenderFrameActionTime(npcIndex, _fixedTimeStepRepository.TickCount);

            _npcStartWalkingTimes.Add(startWalkingTimeAndID);
        }

        public void StartAttackAnimation(int npcIndex)
        {
            if (_npcStartAttackingTimes.Any(x => x.UniqueID == npcIndex))
                return;

            var startAttackingTimeAndID = new RenderFrameActionTime(npcIndex, _fixedTimeStepRepository.TickCount);

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
                if ((_fixedTimeStepRepository.TickCount - pair.ActionTick) >= TICKS_PER_ACTION_FRAME)
                {
                    var npc = _currentMapStateRepository.NPCs.SingleOrNone(x => x.Index == pair.UniqueID);

                    npc.Match(
                        some: n =>
                        {
                            var nextFrameNPC = AnimateOneWalkFrame(n);
                            pair.UpdateActionStartTime(_fixedTimeStepRepository.TickCount);

                            if (nextFrameNPC.Frame == NPCFrame.Standing)
                                npcsDoneWalking.Add(pair);

                            _currentMapStateRepository.NPCs.Remove(n);
                            _currentMapStateRepository.NPCs.Add(nextFrameNPC);
                        },
                        none: () => npcsDoneWalking.Add(pair));
                }
            }

            _npcStartWalkingTimes.RemoveAll(npcsDoneWalking.Contains);
        }

        private void AnimateNPCAttacking()
        {
            var npcsDoneAttacking = new List<RenderFrameActionTime>();
            foreach (var pair in _npcStartAttackingTimes)
            {
                if ((_fixedTimeStepRepository.TickCount - pair.ActionTick) >= TICKS_PER_ACTION_FRAME)
                {
                    var npc = _currentMapStateRepository.NPCs.SingleOrNone(x => x.Index == pair.UniqueID);

                    npc.Match(
                        some: n =>
                        {
                            var nextFrameNPC = n.WithNextAttackFrame();
                            pair.UpdateActionStartTime(_fixedTimeStepRepository.TickCount);

                            if (nextFrameNPC.Frame == NPCFrame.Standing)
                                npcsDoneAttacking.Add(pair);

                            _currentMapStateRepository.NPCs.Remove(n);
                            _currentMapStateRepository.NPCs.Add(nextFrameNPC);

                        },
                        none: () => npcsDoneAttacking.Add(pair));
                }
            }

            _npcStartAttackingTimes.RemoveAll(npcsDoneAttacking.Contains);
        }

        private static EOLib.Domain.NPC.NPC AnimateOneWalkFrame(EOLib.Domain.NPC.NPC npc)
        {
            var nextFrameNPC = npc.WithNextWalkFrame();

            if (nextFrameNPC.IsActing(NPCActionState.Standing))
            {
                nextFrameNPC = nextFrameNPC
                    .WithX(nextFrameNPC.GetDestinationX())
                    .WithY(nextFrameNPC.GetDestinationY());
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