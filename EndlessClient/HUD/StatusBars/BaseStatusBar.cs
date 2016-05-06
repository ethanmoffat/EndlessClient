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

namespace EndlessClient.HUD.StatusBars
{
	public abstract class BaseStatusBar : XNAControl
	{
		protected readonly CharStatData m_stats;
		protected readonly XNALabel m_label;
		protected readonly Texture2D m_textSheet;
		protected Rectangle m_elemSourceRect;
		private DateTime m_labelShowTime;
		
		protected BaseStatusBar()
		{
			m_stats = OldWorld.Instance.MainPlayer.ActiveCharacter.Stats;
			
			m_textSheet = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 58, true);
			m_elemSourceRect = new Rectangle(0, 0, 110, 14);

			if(!Game.Components.Contains(this))
				Game.Components.Add(this);
		
			m_label = new XNALabel(drawArea.WithPosition(new Vector2(2, 14)), Constants.FontSize08)
			{
				AutoSize = false,
				BackColor = Color.Transparent,
				ForeColor = ColorConstants.LightGrayText,
				Visible = false
			};
			m_label.SetParent(this);
		}

		protected abstract void UpdateLabelText();
		protected abstract void DrawStatusBar();

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
					UpdateLabelText();

				//toggle off after 3 seconds
				if ((DateTime.Now - m_labelShowTime).TotalSeconds >= 4)
					m_label.Visible = false;
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			DrawStatusBar();
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
}
