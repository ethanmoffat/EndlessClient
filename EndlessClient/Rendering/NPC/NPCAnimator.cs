// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
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
        private const int WALK_FRAME_TIME_MS = 100;

        private readonly List<RenderFrameActionTime> _npcStartWalkingTimes;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public NPCAnimator(IEndlessGameProvider gameProvider,
                           ICurrentMapStateRepository currentMapStateRepository)
            : base((Game)gameProvider.Game)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _npcStartWalkingTimes = new List<RenderFrameActionTime>();
        }

        public override void Update(GameTime gameTime)
        {
            var now = DateTime.Now;

            AnimateNPCWalking(now);

            base.Update(gameTime);
        }

        public void StartWalkAnimation(int npcIndex)
        {
            var startWalkingTimeAndID = new RenderFrameActionTime(npcIndex, DateTime.Now);

            _npcStartWalkingTimes.Add(startWalkingTimeAndID);
            if (_npcStartWalkingTimes.Select(x => x.UniqueID).Distinct().Count() != _npcStartWalkingTimes.Count)
                throw new InvalidOperationException("NPC is trying to walk twice! figure something out for this case");
        }

        private void AnimateNPCWalking(DateTime now)
        {
            var playersDoneWalking = new List<RenderFrameActionTime>();
            foreach (var pair in _npcStartWalkingTimes)
            {
                if (pair.ActionStartTime.HasValue &&
                    (now - pair.ActionStartTime).TotalMilliseconds > WALK_FRAME_TIME_MS)
                {
                    var npc = _currentMapStateRepository.NPCs.Single(x => x.Index == pair.UniqueID);
                    var nextFrameNPC = AnimateOneWalkFrame(npc);

                    pair.UpdateActionStartTime(GetUpdatedStartWalkingTime(now, nextFrameNPC));
                    if (!pair.ActionStartTime.HasValue)
                        playersDoneWalking.Add(pair);

                    _currentMapStateRepository.NPCs.Remove(npc);
                    _currentMapStateRepository.NPCs.Add(nextFrameNPC);
                }
            }

            _npcStartWalkingTimes.RemoveAll(playersDoneWalking.Contains);
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

        private static Optional<DateTime> GetUpdatedStartWalkingTime(DateTime now, INPC nextFrameNPC)
        {
            return nextFrameNPC.IsActing(NPCActionState.Standing)
                ? Optional<DateTime>.Empty
                : new Optional<DateTime>(now);
        }
    }

    public interface INPCAnimator : IGameComponent
    {
        void StartWalkAnimation(int npcIndex);
    }
}
