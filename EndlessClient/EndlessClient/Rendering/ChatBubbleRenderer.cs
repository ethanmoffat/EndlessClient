// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace EndlessClient.Rendering
{
	public class EOChatBubble : DrawableGameComponent
	{
		private XNALabel m_label;
		private readonly DrawableGameComponent m_ref;
		private readonly bool m_isChar; //true if character, false if npc
		private bool m_useGroupChatColor;

		private SpriteBatch m_sb;

		private Vector2 m_drawLoc;

		private DateTime? m_startTime;

		//texture stuff
		private const int TL = 0, TM = 1, TR = 2;
		private const int ML = 3, MM = 4, MR = 5;
		private const int RL = 6, RM = 7, RR = 8, NUB = 9;
		private static bool s_textsLoaded;
		private static readonly object s_textlocker = new object();
		private static Texture2D[] s_textures;

		public EOChatBubble(EOCharacterRenderer following)
			: base(EOGame.Instance)
		{
			m_ref = following;
			m_isChar = true;
			DrawOrder = following.Character.ID + (int)ControlDrawLayer.BaseLayer + 1; //use ID for draw order
			_initLabel();
			Visible = false;
			EOGame.Instance.Components.Add(this);
		}

		public EOChatBubble(NPC following)
			: base(EOGame.Instance)
		{
			m_ref = following;
			m_isChar = false;
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
			m_label = new XNALabel(new Rectangle(1, 1, 1, 1), Constants.FontSize08pt5)
			{
				Visible = true,
				DrawOrder = DrawOrder + 1, //will be based on either NPC index or character renderer ID
				TextWidth = 165,
				TextAlign = LabelAlignment.MiddleCenter,
				ForeColor = Color.Black,
				AutoSize = true,
				Text = ""
			};

			_setLabelDrawLoc();
		}

		private void _setLabelDrawLoc()
		{
			Rectangle refArea = m_isChar ? ((EOCharacterRenderer)m_ref).DrawAreaWithOffset : ((NPC)m_ref).DrawArea;
			int extra = s_textsLoaded ? s_textures[ML].Width : 0;
			if(m_label != null) //really missing the ?. operator :-/
				m_label.DrawLocation = new Vector2(refArea.X + (refArea.Width / 2.0f) - (m_label.ActualWidth / 2.0f) + extra, refArea.Y - m_label.ActualHeight - 5);
		}

		public new void LoadContent()
		{
			if (m_sb == null)
				m_sb = new SpriteBatch(GraphicsDevice);

			//race condition: if 2 speech bubbles are created simultaneously it may try to load textures twice
			lock (s_textlocker)
			{
				if (!s_textsLoaded)
				{
					s_textures = new Texture2D[10];
					s_textures[TL] = Game.Content.Load<Texture2D>("ChatBubble\\TL");
					s_textures[TM] = Game.Content.Load<Texture2D>("ChatBubble\\TM");
					s_textures[TR] = Game.Content.Load<Texture2D>("ChatBubble\\TR");
					s_textures[ML] = Game.Content.Load<Texture2D>("ChatBubble\\ML");
					s_textures[MM] = Game.Content.Load<Texture2D>("ChatBubble\\MM");
					s_textures[MR] = Game.Content.Load<Texture2D>("ChatBubble\\MR");
					//typed an R instead of a B. I'm tired; somehow bot=R made more sense than bot=B
					s_textures[RL] = Game.Content.Load<Texture2D>("ChatBubble\\RL");
					s_textures[RM] = Game.Content.Load<Texture2D>("ChatBubble\\RM");
					s_textures[RR] = Game.Content.Load<Texture2D>("ChatBubble\\RR");
					s_textures[NUB] = Game.Content.Load<Texture2D>("ChatBubble\\NUB");
					s_textsLoaded = true;
				}
			}

			base.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible || m_ref == null || !s_textsLoaded || m_label == null)
				return;

			_setLabelDrawLoc();
			m_drawLoc = m_label.DrawLocation - new Vector2(s_textures[TL].Width, s_textures[TL].Height);

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
			if (!s_textsLoaded || !Visible) return;
			int xCov = s_textures[TL].Width;
			int yCov = s_textures[TL].Height;
			if (m_sb == null) return;

			Color col = m_useGroupChatColor ? Color.Tan : Color.FromNonPremultiplied(255, 255, 255, 232);

			m_sb.Begin();

			//top row
			m_sb.Draw(s_textures[TL], m_drawLoc, col);
			int xCur;
			for (xCur = xCov; xCur < m_label.ActualWidth + 6; xCur += s_textures[TM].Width)
			{
				m_sb.Draw(s_textures[TM], m_drawLoc + new Vector2(xCur, 0), col);
			}
			m_sb.Draw(s_textures[TR], m_drawLoc + new Vector2(xCur, 0), col);

			//middle area
			int y;
			for (y = yCov; y < m_label.ActualHeight; y += s_textures[ML].Height)
			{
				m_sb.Draw(s_textures[ML], m_drawLoc + new Vector2(0, y), col);
				int x;
				for (x = xCov; x < xCur; x += s_textures[MM].Width)
				{
					m_sb.Draw(s_textures[MM], m_drawLoc + new Vector2(x, y), col);
				}
				m_sb.Draw(s_textures[MR], m_drawLoc + new Vector2(xCur, y), col);
			}

			//bottom row
			m_sb.Draw(s_textures[RL], m_drawLoc + new Vector2(0, y), col);
			int x2;
			for (x2 = xCov; x2 < xCur; x2 += s_textures[RM].Width)
			{
				m_sb.Draw(s_textures[RM], m_drawLoc + new Vector2(x2, y), col);
			}
			m_sb.Draw(s_textures[RR], m_drawLoc + new Vector2(x2, y), col);
			y += s_textures[RM].Height;
			m_sb.Draw(s_textures[NUB], m_drawLoc + new Vector2((x2 + s_textures[RR].Width - s_textures[NUB].Width) / 2f, y - 1), col);

			try
			{
				m_sb.End();
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
				if (m_sb != null)
				{
					m_sb.Dispose();
					m_sb = null;
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
