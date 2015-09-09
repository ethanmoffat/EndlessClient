using System;
using System.Drawing;
using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace EndlessClient
{
	public class EOChatBubble : DrawableGameComponent
	{
		private XNALabel m_label;
		private readonly DrawableGameComponent m_ref;
		private readonly bool isChar; //true if character, false if npc
		private bool m_useGroupChatColor;

		private SpriteBatch sb;

		private Vector2 drawLoc;

		private const int TL = 0, TM = 1, TR = 2;
		private const int ML = 3, MM = 4, MR = 5;
		private const int RL = 6, RM = 7, RR = 8, NUB = 9;
		private static bool textsLoaded;
		private static readonly object _textlocker_ = new object();
		private static Texture2D[] texts;

		private DateTime? m_startTime;

		public EOChatBubble(EOCharacterRenderer following)
			: base(EOGame.Instance)
		{
			m_ref = following;
			isChar = true;
			DrawOrder = following.Character.ID + (int)ControlDrawLayer.BaseLayer + 1; //use ID for draw order
			_initLabel();
			Visible = false;
			EOGame.Instance.Components.Add(this);
		}

		public EOChatBubble(NPC following)
			: base(EOGame.Instance)
		{
			m_ref = following;
			isChar = false;
			DrawOrder = following.Index + (int)ControlDrawLayer.BaseLayer + 1; //use index for draw order
			_initLabel();
			Visible = false;
			EOGame.Instance.Components.Add(this);
		}

		public void HideBubble()
		{
			Visible = false;
			m_label.Text = "";
			m_label.Visible = false;
		}

		public void SetMessage(string message, bool useGroupChatColor)
		{
			m_label.Text = message;
			m_label.Visible = true;
			if (!Game.Components.Contains(m_label))
				Game.Components.Add(m_label);

			Visible = true;
			if (!Game.Components.Contains(this))
				Game.Components.Add(this);

			m_startTime = DateTime.Now;
			m_useGroupChatColor = useGroupChatColor;
		}

		private void _initLabel()
		{
			m_label = new XNALabel(new Rectangle(1, 1, 1, 1), "Microsoft Sans Serif", 8.5f)
			{
				Visible = true,
				DrawOrder = DrawOrder + 1, //will be based on either NPC index or character renderer ID
				TextWidth = 165,
				TextAlign = ContentAlignment.MiddleCenter,
				ForeColor = System.Drawing.Color.Black,
				AutoSize = true,
				Text = ""
			};

			_setLabelDrawLoc();
		}

		private void _setLabelDrawLoc()
		{
			Rectangle refArea = isChar ? ((EOCharacterRenderer)m_ref).DrawAreaWithOffset : ((NPC)m_ref).DrawArea;
			int extra = textsLoaded ? texts[ML].Width : 0;
			m_label.DrawLocation = new Vector2(refArea.X + (refArea.Width / 2.0f) - (m_label.ActualWidth / 2.0f) + extra, refArea.Y - m_label.Texture.Height - 5);
		}

		public new void LoadContent()
		{
			if (sb == null)
				sb = new SpriteBatch(GraphicsDevice);

			//race condition: if 2 speech bubbles are created simultaneously it may try to load textures twice
			lock (_textlocker_)
			{
				if (!textsLoaded)
				{
					texts = new Texture2D[10];
					texts[TL] = Game.Content.Load<Texture2D>("ChatBubble\\TL");
					texts[TM] = Game.Content.Load<Texture2D>("ChatBubble\\TM");
					texts[TR] = Game.Content.Load<Texture2D>("ChatBubble\\TR");
					texts[ML] = Game.Content.Load<Texture2D>("ChatBubble\\ML");
					texts[MM] = Game.Content.Load<Texture2D>("ChatBubble\\MM");
					texts[MR] = Game.Content.Load<Texture2D>("ChatBubble\\MR");
					//typed an R instead of a B. I'm tired; somehow bot=R made more sense than bot=B
					texts[RL] = Game.Content.Load<Texture2D>("ChatBubble\\RL");
					texts[RM] = Game.Content.Load<Texture2D>("ChatBubble\\RM");
					texts[RR] = Game.Content.Load<Texture2D>("ChatBubble\\RR");
					texts[NUB] = Game.Content.Load<Texture2D>("ChatBubble\\NUB");
					textsLoaded = true;
				}
			}

			base.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible)
				return;

			if (!(m_ref is EOCharacterRenderer || m_ref is NPC))
				Dispose(); //"It's over, Anakin, I have the high ground!" "Don't try it!"

			_setLabelDrawLoc();
			try
			{
				drawLoc = m_label.DrawLocation - new Vector2(texts[TL].Width, texts[TL].Height);
			}
			catch (NullReferenceException)
			{
				return; //nullreference here means that the textures haven't been loaded yet...try it on the next pass
			}

			//This replaces the goAway timer.
			if (m_startTime.HasValue && (DateTime.Now - m_startTime.Value).TotalMilliseconds > Constants.ChatBubbleTimeout)
			{
				Visible = false;
				m_label.Visible = false;
				m_startTime = null;
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!textsLoaded || !Visible) return;
			int xCov = texts[TL].Width;
			int yCov = texts[TL].Height;
			if (sb == null) return;

			Color col = m_useGroupChatColor ? Color.Tan : Color.White;

			sb.Begin();

			//top row
			sb.Draw(texts[TL], drawLoc, col);
			int xCur;
			for (xCur = xCov; xCur < m_label.ActualWidth; xCur += texts[TM].Width)
			{
				sb.Draw(texts[TM], drawLoc + new Vector2(xCur, 0), col);
			}
			sb.Draw(texts[TR], drawLoc + new Vector2(xCur, 0), col);

			//middle area
			int y;
			for (y = yCov; y < m_label.Texture.Height - (m_label.Texture.Height % texts[ML].Height); y += texts[ML].Height)
			{
				sb.Draw(texts[ML], drawLoc + new Vector2(0, y), col);
				int x;
				for (x = xCov; x < xCur; x += texts[MM].Width)
				{
					sb.Draw(texts[MM], drawLoc + new Vector2(x, y), col);
				}
				sb.Draw(texts[MR], drawLoc + new Vector2(xCur, y), col);
			}

			//bottom row
			sb.Draw(texts[RL], drawLoc + new Vector2(0, y), col);
			int x2;
			for (x2 = xCov; x2 < xCur; x2 += texts[RM].Width)
			{
				sb.Draw(texts[RM], drawLoc + new Vector2(x2, y), col);
			}
			sb.Draw(texts[RR], drawLoc + new Vector2(x2, y), col);
			y += texts[RM].Height;
			sb.Draw(texts[NUB], drawLoc + new Vector2((x2 + texts[RR].Width - texts[NUB].Width) / 2f, y - 1), col);

			try
			{
				sb.End();
			}
			catch (ObjectDisposedException) { }
			base.Draw(gameTime);
		}

		public new void Dispose()
		{
			Dispose(true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (sb != null)
				{
					sb.Dispose();
					sb = null;
				}
				if (m_label != null)
				{
					m_label.Close();
					m_label = null;
				}
			}

			base.Dispose(disposing);
		}
	}
}
