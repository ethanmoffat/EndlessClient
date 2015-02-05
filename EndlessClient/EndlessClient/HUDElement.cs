using System;
using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient
{
	//base hud element class
	public abstract class HUDElement : XNAControl
	{
		protected readonly CharStatData m_stats;
		protected readonly XNALabel m_label;
		protected readonly Texture2D m_textSheet;
		protected Rectangle m_elemSourceRect;
		private DateTime m_labelShowTime;
		
		protected HUDElement()
		{
			m_stats = World.Instance.MainPlayer.ActiveCharacter.Stats;
			
			m_textSheet = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 58, true);
			m_elemSourceRect = new Rectangle(0, 0, 110, 14);

			if(!Game.Components.Contains(this))
				Game.Components.Add(this);
		
			m_label = new XNALabel(drawArea.SetPosition(new Vector2(2, 14)), "Microsoft Sans Serif", 8.0f)
			{
				AutoSize = false,
				BackColor = System.Drawing.Color.Transparent,
				ForeColor = System.Drawing.Color.FromArgb(0xc8, 0xc8, 0xc8),
				Visible = false
			};
			m_label.SetParent(this);
		}

		protected abstract void updateLabelText();
		protected abstract void drawHudElement();

		private void OnClick()
		{
			m_label.Visible = !m_label.Visible;
			if(m_label.Visible)
				m_labelShowTime = DateTime.Now;
		}

		public override void Update(GameTime gameTime)
		{
			MouseState currentState = Mouse.GetState();
			if (MouseOver && MouseOverPreviously && 
				currentState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
			{
				OnClick();
			}

			if (m_label != null)
			{
				if (m_label.Visible)
					updateLabelText();

				//toggle off after 3 seconds
				if ((DateTime.Now - m_labelShowTime).TotalSeconds >= 4)
					m_label.Visible = false;
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			drawHudElement();
			//draw the background for the label if it is visible
			if (m_label != null && m_label.Visible)
			{
				Vector2 dest = new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y + m_elemSourceRect.Height - 3); //3px offset so gfx fit flush
				SpriteBatch.Begin();
				SpriteBatch.Draw(m_textSheet, dest, new Rectangle(220, 30, 110, 21), Color.White);
				SpriteBatch.End();
			}

			base.Draw(gameTime);
		}
	}

	public class HudElementHP : HUDElement
	{
		public HudElementHP()
		{
			DrawLocation = new Vector2(100, 0);
			drawArea = new Rectangle((int)DrawLocation.X, (int)DrawLocation.Y, m_elemSourceRect.Width, m_elemSourceRect.Height);
		}

		protected override void updateLabelText()
		{
			m_label.Text = string.Format("{0}/{1}", m_stats.hp, m_stats.maxhp);
		}

		protected override void drawHudElement()
		{
			int srcWidth = 25+(int) Math.Round((m_stats.hp/(double) m_stats.maxhp)*79);
			Rectangle maskSrc = new Rectangle(m_elemSourceRect.X, m_elemSourceRect.Height, srcWidth, m_elemSourceRect.Height);

			SpriteBatch.Begin();
			SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), m_elemSourceRect, Color.White);
			SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), maskSrc, Color.White);
			SpriteBatch.End();
		}
	}

	public class HudElementTP : HUDElement
	{
		public HudElementTP()
		{
			DrawLocation = new Vector2(210, 0);
			drawArea = new Rectangle((int)DrawLocation.X, (int)DrawLocation.Y, m_elemSourceRect.Width, m_elemSourceRect.Height);
			m_elemSourceRect.Offset(m_elemSourceRect.Width, 0);
		}

		protected override void updateLabelText()
		{
			m_label.Text = string.Format("{0}/{1}", m_stats.tp, m_stats.maxtp);
		}

		protected override void drawHudElement()
		{
			int srcWidth = 24+(int) Math.Round((m_stats.tp/(double) m_stats.maxtp)*79);
			Rectangle maskSrc = new Rectangle(m_elemSourceRect.X, m_elemSourceRect.Height, srcWidth, m_elemSourceRect.Height);

			SpriteBatch.Begin();
			SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), m_elemSourceRect, Color.White);
			SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), maskSrc, Color.White);
			SpriteBatch.End();
		}
	}

	public class HudElementSP : HUDElement
	{
		public HudElementSP()
		{
			DrawLocation = new Vector2(320, 0);
			drawArea = new Rectangle((int)DrawLocation.X, (int)DrawLocation.Y, m_elemSourceRect.Width, m_elemSourceRect.Height);
			m_elemSourceRect.Offset(m_elemSourceRect.Width * 2, 0);
		}

		protected override void updateLabelText()
		{
			m_label.Text = string.Format("{0}/{1}", m_stats.sp, m_stats.maxsp);
		}

		protected override void drawHudElement()
		{
			int srcWidth = 25 + (int) Math.Round((m_stats.sp/(double) m_stats.maxsp)*79);
			Rectangle maskSrc = new Rectangle(m_elemSourceRect.X, m_elemSourceRect.Height, srcWidth, m_elemSourceRect.Height);

			SpriteBatch.Begin();
			SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), m_elemSourceRect, Color.White);
			SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), maskSrc, Color.White);
			SpriteBatch.End();
		}
	}

	public class HudElementTNL : HUDElement
	{
		public HudElementTNL()
		{
			DrawLocation = new Vector2(430, 0);
			drawArea = new Rectangle((int)DrawLocation.X, (int)DrawLocation.Y, m_elemSourceRect.Width, m_elemSourceRect.Height);
			m_elemSourceRect = new Rectangle(m_elemSourceRect.Width * 3 - 1, 0, m_elemSourceRect.Width + 1, m_elemSourceRect.Height);
		}

		protected override void updateLabelText()
		{
			m_label.Text = string.Format("{0}", World.Instance.exp_table[m_stats.level + 1] - m_stats.exp);
		}

		protected override void drawHudElement()
		{
			int thisLevel = World.Instance.exp_table[m_stats.level];
			int nextLevel = World.Instance.exp_table[m_stats.level + 1];
			int srcWidth = 25 + (int) Math.Round(((m_stats.exp - thisLevel)/(double)(nextLevel-thisLevel))*79);
			Rectangle maskSrc = new Rectangle(m_elemSourceRect.X, m_elemSourceRect.Height, srcWidth, m_elemSourceRect.Height);

			SpriteBatch.Begin();
			SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), m_elemSourceRect, Color.White);
			SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), maskSrc, Color.White);
			SpriteBatch.End();
		}
	}
}
