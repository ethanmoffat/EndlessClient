// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.Net.API;

namespace EOLib.Domain.BLL
{
	public class NPC : IMapElement
	{
		public byte Index { get; private set; }
		public byte X { get; private set; }
		public byte Y { get; private set; }
		public EODirection Direction { get; private set; }
		public NPCRecord Data { get; private set; }

		public byte DestX { get; private set; }
		public byte DestY { get; private set; }
		public object Opponent { get; private set; }

		public bool Walking { get; private set; }
		public bool Attacking { get; private set; }

		public bool Dying { get; private set; }
		public bool DeathCompleted { get; private set; }

		private bool AllowMultipleOpponents
		{
			get { return (Data.Type == NPCType.Passive || Data.Type == NPCType.Aggressive) && Data.VendorID == 1; }
		}

		public NPC(NPCData serverNPCData, NPCRecord localNPCData)
		{
			Index = serverNPCData.Index;
			X = serverNPCData.X;
			Y = serverNPCData.Y;
			Direction = serverNPCData.Direction;
			Data = localNPCData;
		}

		public void BeginWalking(EODirection direction, byte destX, byte destY)
		{
			Walking = true;

			Direction = direction;
			DestX = destX;
			DestY = destY;
		}

		public void EndWalking()
		{
			Walking = false;

			X = DestX;
			Y = DestY;
			DestX = DestY = 0;
		}

		public void BeginAttacking(EODirection direction)
		{
			Attacking = true;

			Direction = direction;
		}

		public void EndAttacking()
		{
			Attacking = false;
		}

		public void BeginDying()
		{
			Dying = true;
		}

		public void EndDying()
		{
			DeathCompleted = true;
		}

		//todo: move character to EOLib; use Character instead of object
		public void SetOpponent(object opponent)
		{
			if (AllowMultipleOpponents)
				return;

			Opponent = opponent;
		}
	}
}
