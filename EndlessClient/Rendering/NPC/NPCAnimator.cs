using System.Collections.Generic;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.NPC
{
    public class NPCAnimator : GameComponent, INPCAnimator
    {
        private const int TICKS_PER_ACTION_FRAME = 8; // 8 x10ms ticks per action frame

        private readonly Dictionary<int, RenderFrameActionTime> _npcStartWalkingTimes = [];
        private readonly Dictionary<int, RenderFrameActionTime> _npcStartAttackingTimes = [];
        private readonly Dictionary<int, (MapCoordinate Coord, EODirection Direction)> _queuedWalk = [];
        private readonly Dictionary<int, EODirection> _queuedAttack = [];

        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IFixedTimeStepRepository _fixedTimeStepRepository;

        public NPCAnimator(IEndlessGameProvider gameProvider,
                           ICurrentMapStateRepository currentMapStateRepository,
                           IFixedTimeStepRepository fixedTimeStepRepository)
            : base((Game)gameProvider.Game)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _fixedTimeStepRepository = fixedTimeStepRepository;
        }

        public override void Update(GameTime gameTime)
        {
            AnimateNPCWalking();
            AnimateNPCAttacking();

            base.Update(gameTime);
        }

        public void StartWalkAnimation(int npcIndex, MapCoordinate coords, EODirection direction)
        {
            if (_npcStartWalkingTimes.TryGetValue(npcIndex, out RenderFrameActionTime value))
            {
                value.SetReplay();
                _queuedWalk[npcIndex] = (coords, direction);
            }
            else if (_npcStartAttackingTimes.ContainsKey(npcIndex))
            {
                _queuedWalk[npcIndex] = (coords, direction);
            }
            else
            {
                var startWalkingTimeAndID = new RenderFrameActionTime(npcIndex, _fixedTimeStepRepository.TickCount);
                _npcStartWalkingTimes.Add(npcIndex, startWalkingTimeAndID);

                if (_currentMapStateRepository.NPCs.TryGetValue(npcIndex, out var npc))
                {
                    _currentMapStateRepository.NPCs.Update(npc, EnsureCorrectXAndY(npc, coords, direction));
                }
            }
        }

        public void StartAttackAnimation(int npcIndex, EODirection direction)
        {
            if (_npcStartAttackingTimes.TryGetValue(npcIndex, out RenderFrameActionTime value))
            {
                value.SetReplay();
                _queuedAttack[npcIndex] = direction;
            }
            else if (_npcStartWalkingTimes.ContainsKey(npcIndex))
            {
                _queuedAttack[npcIndex] = direction;
            }
            else
            {
                var startAttackingTimeAndID = new RenderFrameActionTime(npcIndex, _fixedTimeStepRepository.TickCount);
                _npcStartAttackingTimes.Add(npcIndex, startAttackingTimeAndID);

                if (_currentMapStateRepository.NPCs.TryGetValue(npcIndex, out var npc))
                {
                    _currentMapStateRepository.NPCs.Update(npc, npc.WithDirection(direction));
                }
            }
        }

        public void StopAllAnimations()
        {
            _npcStartWalkingTimes.Clear();
            _npcStartAttackingTimes.Clear();
            _queuedWalk.Clear();
            _queuedAttack.Clear();
        }

        private void AnimateNPCWalking()
        {
            var npcsDoneWalking = new List<int>();
            foreach (var pair in _npcStartWalkingTimes.Values)
            {
                if (_fixedTimeStepRepository.TickCount - pair.ActionTick >= TICKS_PER_ACTION_FRAME)
                {
                    if (_currentMapStateRepository.NPCs.TryGetValue(pair.UniqueID, out var npc))
                    {
                        var nextFrameNPC = AnimateOneWalkFrame(npc);
                        pair.UpdateActionStartTime(_fixedTimeStepRepository.TickCount);

                        if (nextFrameNPC.IsActing(NPCActionState.Standing))
                        {
                            if (pair.Replay)
                            {
                                nextFrameNPC = AnimateOneWalkFrame(nextFrameNPC);

                                if (_queuedWalk.TryGetValue(pair.UniqueID, out var update))
                                {
                                    nextFrameNPC = EnsureCorrectXAndY(nextFrameNPC, update.Coord, update.Direction);
                                    _queuedWalk.Remove(pair.UniqueID);
                                }

                                pair.ClearReplay();
                            }
                            else
                            {
                                npcsDoneWalking.Add(pair.UniqueID);

                                if (_queuedAttack.TryGetValue(pair.UniqueID, out var update))
                                {
                                    nextFrameNPC = nextFrameNPC.WithDirection(update);
                                    _npcStartAttackingTimes.Add(pair.UniqueID, pair);
                                    _queuedAttack.Remove(pair.UniqueID);
                                }
                            }
                        }

                        _currentMapStateRepository.NPCs.Update(npc, nextFrameNPC);
                    }
                    else
                    {
                        npcsDoneWalking.Add(pair.UniqueID);
                    }
                }
            }

            foreach (var index in npcsDoneWalking)
                _npcStartWalkingTimes.Remove(index);
        }

        private void AnimateNPCAttacking()
        {
            var npcsDoneAttacking = new List<int>();
            foreach (var pair in _npcStartAttackingTimes.Values)
            {
                if (_fixedTimeStepRepository.TickCount - pair.ActionTick >= TICKS_PER_ACTION_FRAME)
                {
                    if (_currentMapStateRepository.NPCs.TryGetValue(pair.UniqueID, out var npc))
                    {
                        var nextFrameNPC = npc.WithNextAttackFrame();
                        pair.UpdateActionStartTime(_fixedTimeStepRepository.TickCount);

                        if (nextFrameNPC.Frame == NPCFrame.Standing)
                        {
                            if (pair.Replay)
                            {
                                nextFrameNPC = npc.WithNextAttackFrame();

                                if (_queuedAttack.TryGetValue(pair.UniqueID, out var update))
                                {
                                    nextFrameNPC = nextFrameNPC.WithDirection(update);
                                    _queuedAttack.Remove(pair.UniqueID);
                                }

                                pair.ClearReplay();
                            }
                            else
                            {
                                npcsDoneAttacking.Add(pair.UniqueID);

                                if (_queuedWalk.TryGetValue(pair.UniqueID, out var update))
                                {
                                    nextFrameNPC = EnsureCorrectXAndY(nextFrameNPC, update.Coord, update.Direction);
                                    _queuedWalk.Remove(pair.UniqueID);

                                    _npcStartWalkingTimes.Add(pair.UniqueID, pair);
                                }
                            }
                        }

                        _currentMapStateRepository.NPCs.Remove(npc);
                        _currentMapStateRepository.NPCs.Add(nextFrameNPC);
                    }
                    else
                    {
                        npcsDoneAttacking.Add(pair.UniqueID);
                    }
                }
            }

            foreach (var index in npcsDoneAttacking)
                _npcStartAttackingTimes.Remove(index);
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

        private static EOLib.Domain.NPC.NPC EnsureCorrectXAndY(EOLib.Domain.NPC.NPC npc, MapCoordinate coordinate, EODirection direction)
        {
            var tmpNpc = npc.WithX(coordinate.X).WithY(coordinate.Y).WithDirection(direction.Opposite());
            return npc.WithDirection(direction).WithX(tmpNpc.GetDestinationX()).WithY(tmpNpc.GetDestinationY());
        }
    }

    public interface INPCAnimator : IGameComponent
    {
        void StartWalkAnimation(int npcIndex, MapCoordinate coords, EODirection direction);

        void StartAttackAnimation(int npcIndex, EODirection direction);

        void StopAllAnimations();
    }
}
