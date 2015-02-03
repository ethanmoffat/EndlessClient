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
		//private static readonly  int[] SPEED_TABLE = {900, 600, 1300, 1900, 3700, 7500, 15000, 0};

		public byte Index { get; private set; }
		public byte X { get; private set; }
		public byte Y { get; private set; }
		public EODirection Direction { get; private set; }
		public NPCRecord Data { get; private set; }

		public Rectangle DrawArea;
		public bool Walking { get; private set; }
		public NPCFrame Frame { get; private set; }

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

		private byte DestX { get; set; } //not needed outside this class (yet)
		public byte DestY { get; private set; }

		//updated when NPC is walking
		private int adjX, adjY;

		private readonly bool hasStandFrame1;
		private bool _startFadeAway;
		private int _fadeAwayAlpha = 255;

		private readonly EOChatBubble m_chatBubble;
		private Texture2D baseFrame;

		public NPC(Packet pkt)
			: base(EOGame.Instance)
		{
			Index = pkt.GetChar();
			short id = pkt.GetShort();
			Data = World.Instance.ENF.Data[id] as NPCRecord;
			X = pkt.GetChar();
			Y = pkt.GetChar();
			Direction = (EODirection)pkt.GetChar();

			bool success = true;
			npcSheet = new EONPCSpriteSheet(this);
			do
			{
				try
				{
					//attempt to get standing frame 1. It will have non-black pixels if it exists.
					Frame = NPCFrame.StandingFrame1;
					Texture2D tmp = npcSheet.GetNPCTexture();
					Color[] tmpData = new Color[tmp.Width*tmp.Height];
					tmp.GetData(tmpData);
					hasStandFrame1 = tmpData.Any(_c => _c.R != 0 || _c.G != 0 || _c.B != 0);
				} //this block throws errors sometimes..no idea why. Keep looping until it works.
				catch(InvalidOperationException)
				{
					success = false;
				}
			} while (!success);

			m_chatBubble = new EOChatBubble(this);
		}

		public override void Initialize()
		{
			base.Initialize();
			sb = new SpriteBatch(GraphicsDevice);

			Frame = NPCFrame.Standing;

			baseFrame = npcSheet.GetNPCTexture();
			npcArea = new Rectangle(0, 0, baseFrame.Width, baseFrame.Height);

			DrawArea = new Rectangle(
					 offX + 320 - World.Instance.MainPlayer.ActiveCharacter.OffsetX - (int)((baseFrame.Width / 6.4) * 3.2),
					 offY + 168 - World.Instance.MainPlayer.ActiveCharacter.OffsetY - baseFrame.Height,
					 npcArea.Width, npcArea.Height);
			Walking = false;
			walkTimer = new Timer(_walkCallback, null, Timeout.Infinite, Timeout.Infinite);

			m_chatBubble.Initialize();
			m_chatBubble.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			DrawArea = new Rectangle(
					 offX + 320 - World.Instance.MainPlayer.ActiveCharacter.OffsetX - (int)((baseFrame.Width / 6.4) * 3.2),
					 offY + 168 - World.Instance.MainPlayer.ActiveCharacter.OffsetY - baseFrame.Height,
					 npcArea.Width, npcArea.Height);

			//switch the standing animation for NPCs every 500ms, if they're standing still
			if (hasStandFrame1 && (int) gameTime.TotalGameTime.TotalMilliseconds%500 == 0)
			{
				if (Frame == NPCFrame.Standing)
				{
					Frame = NPCFrame.StandingFrame1;
				}
				else if (Frame == NPCFrame.StandingFrame1)
				{
					Frame = NPCFrame.Standing;
				}
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			DrawToSpriteBatch(sb);
			base.Draw(gameTime);
		}

		public void DrawToSpriteBatch(SpriteBatch batch, bool started = false)
		{
			SpriteEffects effects = Direction == EODirection.Left || Direction == EODirection.Down
				? SpriteEffects.None
				: SpriteEffects.FlipHorizontally;

			if(!started)
				batch.Begin();

			Color col = _startFadeAway ? Color.FromNonPremultiplied(255, 255, 255, _fadeAwayAlpha -= 3) : Color.White;

			batch.Draw(npcSheet.GetNPCTexture(),
				DrawArea,
				null,
				col,
				0f,
				Vector2.Zero,
				effects,
				1f);

			if (_startFadeAway && _fadeAwayAlpha <= 0)
			{
				if(!started) batch.End();
				World.Instance.ActiveMapRenderer.RemoveOtherNPC(Index, false);
				return;
			}

			if (!started)
				batch.End();
		}

		public new void Dispose()
		{
			Dispose(true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (walkTimer != null)
				{
					walkTimer.Change(Timeout.Infinite, Timeout.Infinite);
					walkTimer.Dispose();
				}

				if(sb != null)
					sb.Dispose();

				if (m_chatBubble != null)
					m_chatBubble.Dispose();
			}

			base.Dispose(disposing);
		}

		public void Walk(byte x, byte y, EODirection dir)
		{
			if (Walking) return;
			walkTimer.Change(0, 100); //use 100ms for now....
			
			//the direction is required for the walk callback
			Direction = dir;
			DestX = x;
			DestY = y;
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

			switch (Frame)
			{
				case NPCFrame.Standing:
				case NPCFrame.StandingFrame1:
					Frame = NPCFrame.WalkFrame1;
					break;
				case NPCFrame.WalkFrame1:
					Frame = NPCFrame.WalkFrame2;
					break;
				case NPCFrame.WalkFrame2:
					Frame = NPCFrame.WalkFrame3;
					break;
				case NPCFrame.WalkFrame3:
					Frame = NPCFrame.WalkFrame4;
					break;
				case NPCFrame.WalkFrame4:
					Frame = NPCFrame.Standing;
					walkTimer.Change(Timeout.Infinite, Timeout.Infinite);
					X = DestX;
					Y = DestY;
					adjX = adjY = 0;
					break;
			}
		}

		public void SetChatBubbleText(string message)
		{
			m_chatBubble.SetMessage(message);
		}

		public void FadeAway()
		{
			_startFadeAway = true;
		}
	}
}
