// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EOLib.Domain.NPC;

namespace EOLib.Domain.Extensions
{
    public static class NPCExtensions
    {
        public static bool IsFacing(this INPC npc, params EODirection[] directions)
        {
            return directions.Contains(npc.Direction);
        }

        public static bool IsActing(this INPC npc, NPCActionState action)
        {
            switch (action)
            {
                case NPCActionState.Standing:
                    return npc.Frame == NPCFrame.Standing || npc.Frame == NPCFrame.StandingFrame1;
                case NPCActionState.Walking:
                    return npc.Frame == NPCFrame.WalkFrame1 ||
                           npc.Frame == NPCFrame.WalkFrame2 ||
                           npc.Frame == NPCFrame.WalkFrame3 ||
                           npc.Frame == NPCFrame.WalkFrame4;
                case NPCActionState.Attacking:
                    return npc.Frame == NPCFrame.Attack1 ||
                           npc.Frame == NPCFrame.Attack2;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public static int GetWalkFrame(this INPC npc)
        {
            if (!npc.IsActing(NPCActionState.Walking))
                return 0;
            return npc.Frame - NPCFrame.WalkFrame1 + 1;
        }

        public static INPC WithNextWalkFrame(this INPC npc)
        {
            if (npc.Frame == NPCFrame.WalkFrame4)
                return npc.WithFrame(NPCFrame.Standing);
            if (npc.Frame == NPCFrame.Standing)
                return npc.WithFrame(NPCFrame.WalkFrame1);

            return npc.WithFrame(npc.Frame + 1);
        }

        public static int GetAttackFrame(this INPC npc)
        {
            if (!npc.IsActing(NPCActionState.Attacking))
                return 0;
            return npc.Frame - NPCFrame.Attack1 + 1;
        }

        public static int GetDestinationX(this INPC npc)
        {
            var offset = GetXOffset(npc.Direction);
            return npc.X + offset;
        }

        public static int GetDestinationY(this INPC npc)
        {
            var offset = GetYOffset(npc.Direction);
            return npc.Y + offset;
        }

        private static int GetXOffset(EODirection direction)
        {
            return direction == EODirection.Right ? 1 :
                   direction == EODirection.Left ? -1 : 0;
        }

        private static int GetYOffset(EODirection direction)
        {
            return direction == EODirection.Down ? 1 :
                   direction == EODirection.Up ? -1 : 0;
        }
    }
}
