using System;
using System.Linq;
using System.Threading;
using EOLib;
using EOLib.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient
{
	//Map NPCs are coupled with draw/update operations
	//Note: this is like character, in that it isn't actually drawn or updated by the framework
	//		Map Renderer handles all draw/update operations
	public class NPC : DrawableGameComponent
	{
		/* Default NPC speed table for eoserv, corresponding to the speed stored in the NPC spawn */
		/* Not sure if this is ever transferred to the client at all, but it is a config value... */
		private static readonly  int[] SPEED_TABLE = {900, 600, 1300, 1900, 3700, 7500, 15000, 0};

		public byte Index { get; private set; }
		public byte X { get; private set; }
		public byte Y { get; private set; }
		public EODirection Direction { get; set; }
		public NPCRecord Data { get; private set; }

		public Rectangle DrawArea;
		public bool Walking { get; private set; }
		public NPCFrame npcFrame { get; private set; }

		private SpriteBatch sb;
		private readonly EONPCSpriteSheet npcSheet;
		private Rectangle npcArea;

		private Timer walkTimer;

		private int offX
		{
			get { return X * 32 - Y * 32 + adjX; }
		}

		private int offY
		{
			get { return X * 16 + Y * 16 + adjY; }
		}

		private byte destX, destY;

		//updated when NPC is walking
		private int adjX, adjY;

		private bool hasStandFrame1;

		private static readonly object chatBubbleLock = new object();
		private EOChatBubble m_chatBubble;

		public NPC(Packet pkt)
			: base(EOGame.Instance)
		{
			Index = pkt.GetChar();
			short id = pkt.GetShort();
			Data = World.Instance.ENF.Data[id] as NPCRecord;
			X = pkt.GetChar();
			Y = pkt.GetChar();
			Direction = (EODirection)pkt.GetChar();

			npcSheet = new EONPCSpriteSheet(this);
			Texture2D tmp = npcSheet.GetNPCTexture(NPCFrame.StandingFrame1, Direction);
			Color[] tmpData = new Color[tmp.Width * tmp.Height];
			tmp.GetData(tmpData);
			hasStandFrame1 = tmpData.Any(_c => _c.R != 0 || _c.G != 0 || _c.B != 0);
		}

		public override void Initialize()
		{
			base.Initialize();
			sb = new SpriteBatch(GraphicsDevice);

			npcFrame = NPCFrame.Standing;

			Texture2D tmpText = npcSheet.GetNPCTexture(NPCFrame.Standing, EODirection.Down);
			npcArea = new Rectangle(0, 0, tmpText.Width, tmpText.Height);

			DrawArea = new Rectangle(
					 offX + 320 - World.Instance.MainPlayer.ActiveCharacter.OffsetX - (int)((tmpText.Width / 6.4) * 3.2),
					 offY + 176 - World.Instance.MainPlayer.ActiveCharacter.OffsetY - tmpText.Height,
					 npcArea.Width, npcArea.Height);
			Walking = false;
			walkTimer = new Timer(_walkCallback, null, Timeout.Infinite, Timeout.Infinite);
		}

		public override void Update(GameTime gameTime)
		{
			Texture2D tmpText = npcSheet.GetNPCTexture(NPCFrame.Standing, Direction);
			DrawArea = new Rectangle(
					 offX + 320 - World.Instance.MainPlayer.ActiveCharacter.OffsetX - (int)((tmpText.Width / 6.4) * 3.2),
					 offY + 176 - World.Instance.MainPlayer.ActiveCharacter.OffsetY - tmpText.Height,
					 npcArea.Width, npcArea.Height);

			//switch the standing animation for NPCs every 500ms, if they're standing still
			if (hasStandFrame1 && (int) gameTime.TotalGameTime.TotalMilliseconds%500 == 0)
			{
				if (npcFrame == NPCFrame.Standing)
				{
					npcFrame = NPCFrame.StandingFrame1;
				}
				else if (npcFrame == NPCFrame.StandingFrame1)
				{
					npcFrame = NPCFrame.Standing;
				}
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteEffects effects = Direction == EODirection.Left || Direction == EODirection.Down
				? SpriteEffects.None
				: SpriteEffects.FlipHorizontally;

			sb.Begin();
			sb.Draw(npcSheet.GetNPCTexture(npcFrame, Direction),
				DrawArea,
				null,
				Color.White,
				0f,
				Vector2.Zero,
				effects,
				1f);
			sb.End();

			base.Draw(gameTime);
		}

		protected override void Dispose(bool disposing)
		{
			walkTimer.Change(Timeout.Infinite, Timeout.Infinite);
			walkTimer.Dispose();
			sb.Dispose();

			base.Dispose(disposing);
		}

		public void Walk(byte x, byte y, EODirection dir)
		{
			if (Walking) return;
			walkTimer.Change(0, 100); //use 100ms for now....
			
			//the direction is required for the walk callback
			Direction = dir;
			destX = x;
			destY = y;
		}

		private void _walkCallback(object state)
		{
			switch (Direction)
			{
				case EODirection.Down: adjX += -8; adjY += 4; break;
				case EODirection.Left: adjX += -8; adjY += -4; break;
				case EODirection.Up: adjX += 8; adjY += -4; break;
				case EODirection.Right: adjX += 8; adjY += 4; break;
			}

			switch (npcFrame)
			{
				case NPCFrame.Standing:
				case NPCFrame.StandingFrame1:
					npcFrame = NPCFrame.WalkFrame1;
					break;
				case NPCFrame.WalkFrame1:
					npcFrame = NPCFrame.WalkFrame2;
					break;
				case NPCFrame.WalkFrame2:
					npcFrame = NPCFrame.WalkFrame3;
					break;
				case NPCFrame.WalkFrame3:
					npcFrame = NPCFrame.WalkFrame4;
					break;
				case NPCFrame.WalkFrame4:
					npcFrame = NPCFrame.Standing;
					walkTimer.Change(Timeout.Infinite, Timeout.Infinite);
					X = destX;
					Y = destY;
					adjX = adjY = 0;
					break;
			}
		}

		public void SetChatBubble(EOChatBubble bb)
		{
			lock (chatBubbleLock)
			{
				if (m_chatBubble != null)
				{
					m_chatBubble.Dispose();
					m_chatBubble = null;
				}

				bb.Initialize();
				bb.LoadContent();
				m_chatBubble = bb;
			}
		}
	}
}
