// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.HUD
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
			
			m_textSheet = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 58, true);
			m_elemSourceRect = new Rectangle(0, 0, 110, 14);

			if(!Game.Components.Contains(this))
				Game.Components.Add(this);
		
			m_label = new XNALabel(drawArea.WithPosition(new Vector2(2, 14)), Constants.FontSize08)
			{
				AutoSize = false,
				BackColor = Color.Transparent,
				ForeColor = Constants.LightGrayText,
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
			if (!Game.IsActive) return;

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
			m_label.Text = string.Format("{0}/{1}", m_stats.HP, m_stats.MaxHP);
		}

		protected override void drawHudElement()
		{
			int srcWidth = 25+(int) Math.Round((m_stats.HP/(double) m_stats.MaxHP)*79);
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
			m_label.Text = string.Format("{0}/{1}", m_stats.TP, m_stats.MaxTP);
		}

		protected override void drawHudElement()
		{
			int srcWidth = 24+(int) Math.Round((m_stats.TP/(double) m_stats.MaxTP)*79);
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
			m_label.Text = string.Format("{0}/{1}", m_stats.SP, m_stats.MaxSP);
		}

		protected override void drawHudElement()
		{
			int srcWidth = 25 + (int) Math.Round((m_stats.SP/(double) m_stats.MaxSP)*79);
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
			m_label.Text = string.Format("{0}", World.Instance.exp_table[m_stats.Level + 1] - m_stats.Experience);
		}

		protected override void drawHudElement()
		{
			int thisLevel = World.Instance.exp_table[m_stats.Level];
			int nextLevel = World.Instance.exp_table[m_stats.Level + 1];
			int srcWidth = 25 + (int) Math.Round(((m_stats.Experience - thisLevel)/(double)(nextLevel-thisLevel))*79);
			Rectangle maskSrc = new Rectangle(m_elemSourceRect.X, m_elemSourceRect.Height, srcWidth, m_elemSourceRect.Height);

			SpriteBatch.Begin();
			SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), m_elemSourceRect, Color.White);
			SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), maskSrc, Color.White);
			SpriteBatch.End();
		}
	}
}
