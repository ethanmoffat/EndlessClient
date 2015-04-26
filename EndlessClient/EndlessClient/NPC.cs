using System;
using System.Linq;
using System.Threading;
using EOLib;
using EOLib.Data;
using EOLib.Net;
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
		/// <summary>
		/// The Y coordinate of the first non-transparent pixel in the NPC's standing-still sprite sheet
		/// <para>Used for calculating distance above the NPC's head</para>
		/// </summary>
		public int TopPixel { get; private set; }
		public bool Walking { get; private set; }
		public bool Attacking { get; private set; }
		public NPCFrame Frame { get; private set; }

		private SpriteBatch sb;
		private readonly EONPCSpriteSheet npcSheet;
		private Rectangle npcArea;

		private Timer walkTimer, attackTimer;

		private int offX
		{
			get { return X * 32 - Y * 32 + adjX; }
		}

		private int offY
		{
			get { return X * 16 + Y * 16 + adjY; }
		}

		public byte DestX { get; set; }
		public byte DestY { get; private set; }
		public Character Opponent { get; set; }
		public int HP { get; set; }

		public bool Dying { get { return _startFadeAway; } }

		//updated when NPC is walking
		private int adjX, adjY;

		private readonly bool hasStandFrame1;
		private bool _startFadeAway;
		private int _fadeAwayAlpha = 255;

		private readonly EOChatBubble m_chatBubble;
		private readonly DamageCounter m_damageCounter;
		private Texture2D baseFrame;

		private TimeSpan? m_lastAnimUpdateTime;

		public NPC(NPCData data)
			: base(EOGame.Instance)
		{
			Index = data.Index;
			X = data.X;
			Y = data.Y;
			Direction = data.Direction;

			Data = (NPCRecord)World.Instance.ENF.Data[data.ID];
			HP = Data.HP;

			bool success = true;
			npcSheet = new EONPCSpriteSheet(this);
			int tries = 0;
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

					//get the first non-transparent pixel to determine offsets for name labels and damage counters
					Frame = NPCFrame.Standing;
					tmp = npcSheet.GetNPCTexture();
					tmpData = new Color[tmp.Width*tmp.Height];
					tmp.GetData(tmpData);
					int i = 0;
					while (i < tmpData.Length && tmpData[i].A == 0) i++;
					TopPixel = i == tmpData.Length - 1 ? 0 : i/tmp.Height;

				} //this block throws errors sometimes..no idea why. Keep looping until it works.
				catch (InvalidOperationException)
				{
					success = false;
					tries++;
				}
			} while (!success && tries < 3);

			if(tries >= 3)
				throw new InvalidOperationException("Something weird happened initializing this NPC.");

			m_chatBubble = new EOChatBubble(this);
			m_damageCounter = new DamageCounter(this, GetType());
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
			Attacking = false;
			attackTimer = new Timer(_attackCallback, null, Timeout.Infinite, Timeout.Infinite);

			m_chatBubble.Initialize();
			m_chatBubble.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			DrawArea = new Rectangle(
					 offX + 320 - World.Instance.MainPlayer.ActiveCharacter.OffsetX - (int)((baseFrame.Width / 6.4) * 3.2),
					 offY + 168 - World.Instance.MainPlayer.ActiveCharacter.OffsetY - baseFrame.Height,
					 npcArea.Width, npcArea.Height);

			if (!Game.IsActive) return;

			//switch the standing animation for NPCs every 500ms, if they're standing still
			if (hasStandFrame1 && m_lastAnimUpdateTime.HasValue && (gameTime.TotalGameTime - m_lastAnimUpdateTime.Value).TotalMilliseconds > 250)
			{
				if (Frame == NPCFrame.Standing)
				{
					Frame = NPCFrame.StandingFrame1;
				}
				else if (Frame == NPCFrame.StandingFrame1)
				{
					Frame = NPCFrame.Standing;
				}
				m_lastAnimUpdateTime = gameTime.TotalGameTime;
			}
			else if(!m_lastAnimUpdateTime.HasValue)
			{
				m_lastAnimUpdateTime = gameTime.TotalGameTime;
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
				World.Instance.ActiveMapRenderer.RemoveOtherNPC(Index);
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

		public void Attack(EODirection dir)
		{
			if (Attacking) return;
			attackTimer.Change(0, 100);
			Direction = dir;
		}

		private void _attackCallback(object state)
		{
			switch (Frame)
			{
				case NPCFrame.Standing:
				case NPCFrame.StandingFrame1:
					Frame = NPCFrame.Attack1;
					break;
				case NPCFrame.Attack1:
					Frame = NPCFrame.Attack2;
					break;
				case NPCFrame.Attack2:
					Frame = NPCFrame.Standing;
					attackTimer.Change(Timeout.Infinite, Timeout.Infinite);
					break;
			}
		}

		public void SetChatBubbleText(string message)
		{
			m_chatBubble.SetMessage(message);
		}

		public void SetDamageCounterValue(int value, int pctHealth)
		{
			m_damageCounter.SetValue(value, pctHealth); //NPCs don't know heal spells
		}

		public void FadeAway()
		{
			_startFadeAway = true;
		}
	}
}
