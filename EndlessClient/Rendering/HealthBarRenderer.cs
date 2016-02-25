// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Rendering
{
	/// <summary>
	/// Numeric damage counter shows a damage value above an actor
	/// </summary>
	public class DamageCounter : XNAControl
	{
		private const int CHR_ADDITIONAL_OFFSET = 30;
		private const int NPC_ADDITIONAL_OFFSET = 22;

		private readonly bool m_isCharacter;
		private readonly DrawableGameComponent m_ref;
		private int m_value, m_percentHealth;
		private bool m_isHeal;

		private static Texture2D s_NumberSprites, s_HealthBarSprites;
		private static readonly object gfx_init_lock = new object();

		private readonly List<Rectangle> m_numbersToDraw;
		private float m_additionalOffset;

		private Vector2 m_healthBarPos;

		/// <summary>
		/// This constructor makes the DamageCounter follow 'actor' on the screen.
		/// </summary>
		/// <param name="actor">CharacterRenderer or NPCRenderer</param>
		public DamageCounter(DrawableGameComponent actor)
		{
			m_ref = actor;
			if (m_ref is NPCRenderer)
			{
				m_isCharacter = false;
			}
			else if (m_ref is CharacterRenderer)
			{
				m_isCharacter = true;
			}
			else
			{
				throw new ArgumentException("Invalid actor type. Use Character or NPCRenderer", "actor");
			}

			lock (gfx_init_lock)
			{
				Texture2D wholeSheet = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 58, true);
				//lazy init of spritesheet - static so same texture in use for all damage counters
				//this sheet is a subsheet of GFX002/158 that has only the numbers and 'miss' text
				if (s_NumberSprites == null)
				{
					Color[] data = new Color[123*24];
					wholeSheet.GetData(0, new Rectangle(40, 28, 123, 24), data, 0, data.Length);

					s_NumberSprites = new Texture2D(Game.GraphicsDevice, 123, 24);
					s_NumberSprites.SetData(data);
				}

				//same with health bars - subsheet of GFX002/158 that has only the health bars
				if (s_HealthBarSprites == null)
				{
					Color[] data = new Color[40*35];
					wholeSheet.GetData(0, new Rectangle(0, 28, 40, 35), data, 0, data.Length);

					s_HealthBarSprites = new Texture2D(Game.GraphicsDevice, 40, 35);
					s_HealthBarSprites.SetData(data);
				}
			}

			m_numbersToDraw = new List<Rectangle>();
			Visible = false;
			if(!Game.Components.Contains(this))
				Game.Components.Add(this);
		}

		/// <summary>
		/// Call this function to show a number above the player/npc
		/// </summary>
		/// <param name="val">damage</param>
		/// <param name="pctHealth">percent health remaining</param>
		/// <param name="isHeal">if it is a heal spell</param>
		public void SetValue(int val, int pctHealth, bool isHeal = false)
		{
			m_value = val;
			m_percentHealth = pctHealth;
			m_isHeal = isHeal;
			m_additionalOffset = 0;
			Visible = true;
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible)
				return;

			m_numbersToDraw.Clear();
			if (m_value == 0 && !m_isHeal)
			{
				m_numbersToDraw.Add(new Rectangle(92, 0, 30, 11));
			}
			else
			{
				//using convert function is a lot easier than dealing with % and / semantics
				string digits = Convert.ToString(m_value);
				for (int i = 0; i < digits.Length; ++i)
				{
					int next = int.Parse(digits.Substring(i, 1));
					m_numbersToDraw.Add(new Rectangle(next*9, m_isHeal ? 11 : 0, 8, 11));
				}
			}

			m_additionalOffset += .1f; //there are lots of update calls...

			if (m_additionalOffset >= 4) //arbitrary. This will allow 40 updates to the offset before hiding this again.
				Visible = false;

			if (m_isCharacter)
			{
				Rectangle tmp = ((CharacterRenderer)m_ref).DrawAreaWithOffset;
				m_healthBarPos = new Vector2(tmp.X - 3, tmp.Y - 15);
			}
			else
			{
				Rectangle tmp = ((NPCRenderer)m_ref).DrawArea;
				m_healthBarPos = new Vector2(tmp.X + (tmp.Width - s_HealthBarSprites.Width) / 2f, tmp.Y + ((NPCRenderer)m_ref).TopPixel - 10);
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			//Please note: DrawAreaWithOffset is not used for this XNAControl

			if (!Visible)
				return;

			//first, get the position that digits should start being drawn at
			int nDigits = m_numbersToDraw.Count;
			Vector2 pos;
			if (m_value == 0)
			{
				//misses should show dead center
				if (m_isCharacter)
				{
					Rectangle tmp = ((CharacterRenderer) m_ref).DrawAreaWithOffset;
					pos = new Vector2(tmp.X + 1, tmp.Y - m_additionalOffset - CHR_ADDITIONAL_OFFSET);
				}
				else
				{
					Rectangle tmp = ((NPCRenderer) m_ref).DrawArea;
					pos = new Vector2(tmp.X + tmp.Width/2f - 15, tmp.Y + ((NPCRenderer) m_ref).TopPixel - m_additionalOffset - NPC_ADDITIONAL_OFFSET);
				}
			}
			else
			{
				if (m_isCharacter)
				{
					Rectangle tmp = ((CharacterRenderer) m_ref).DrawAreaWithOffset;
					pos = new Vector2(tmp.X + 16 - (nDigits * 9) / 2f, tmp.Y - m_additionalOffset - CHR_ADDITIONAL_OFFSET);
				}
				else
				{
					Rectangle tmp = ((NPCRenderer) m_ref).DrawArea;
					pos = new Vector2(tmp.X + tmp.Width / 2f - (nDigits * 9) / 2f, tmp.Y + ((NPCRenderer)m_ref).TopPixel - m_additionalOffset - NPC_ADDITIONAL_OFFSET);
				}
			}
			
			SpriteBatch.Begin();
			//don't bother drawing if we don't need to...
			//tall NPCs will go less than zero on the numbers
			if (pos.Y >= 0)
			{
				//then, draw the digits
				int ndx = 0;
				foreach (Rectangle r in m_numbersToDraw)
				{
					SpriteBatch.Draw(s_NumberSprites, pos + new Vector2(ndx*9, 0), r, Color.White);
					ndx++;
				}
			}

			SpriteBatch.Draw(s_HealthBarSprites, m_healthBarPos, new Rectangle(0, 28, 40, 7), Color.White); //draw health bar background container

			//the percent health is represented as an int 0-100.
			//Need to divide by 100 to get decimal and multiply by 40 (max width of health bar)
			//(x / 100) * 40 == x * .4
			Rectangle healthSrcRect;
			if (m_percentHealth >= 50)
				healthSrcRect = new Rectangle(0, 7, (int)Math.Round(m_percentHealth * .4), 7);
			else if (m_percentHealth >= 25)
				healthSrcRect = new Rectangle(0, 14, (int)Math.Round(m_percentHealth * .4), 7);
			else
				healthSrcRect = new Rectangle(0, 21, (int)Math.Round(m_percentHealth * .4), 7);

			SpriteBatch.Draw(s_HealthBarSprites, m_healthBarPos, healthSrcRect, Color.White);

			SpriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
