using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient
{
	/// <summary>
	/// Numeric damage counter shows a damage value above an actor
	/// </summary>
	public class DamageCounter : XNAControl
	{
		private readonly bool m_isCharacter;
		private readonly DrawableGameComponent m_ref;
		private int m_value;
		private bool m_isHeal;

		private static Texture2D s_NumberSprites, s_HealthBarSprites;
		private static readonly object gfx_init_lock = new object();

		private readonly List<Rectangle> m_numbersToDraw;
		private float m_additionalOffset;

		private Vector2 m_healthBarPos;

		/// <summary>
		/// This constructor makes the DamageCounter follow 'actor' on the screen.
		/// </summary>
		/// <param name="actor">EOCharacterRenderer or NPC</param>
		/// <param name="actorType">actor.GetType()</param>
		public DamageCounter(DrawableGameComponent actor, Type actorType)
		{
			m_ref = actor;
			if (actorType == typeof (NPC))
			{
				m_isCharacter = false;
			}
			else if (actorType == typeof (EOCharacterRenderer))
			{
				m_isCharacter = true;
			}
			else
			{
				throw new ArgumentException("Invalid actor type. Use Character or NPC", "actorType");
			}

			lock (gfx_init_lock)
			{
				Texture2D wholeSheet = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 58, true);
				//lazy init of spritesheet - static so same texture in use for all damage counters
				//this sheet is a subsheet of GFX002/158 that has only the numbers and 'miss' text
				if (s_NumberSprites == null)
				{
					Color[] data = new Color[122*23];
					wholeSheet.GetData(0, new Rectangle(41, 29, 122, 23), data, 0, data.Length);

					s_NumberSprites = new Texture2D(Game.GraphicsDevice, 122, 23);
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
		/// <param name="isHeal">if it is a heal spell</param>
		public void SetValue(int val, bool isHeal = false)
		{
			m_value = val;
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
				Rectangle tmp = ((EOCharacterRenderer)m_ref).DrawAreaWithOffset;
				m_healthBarPos = new Vector2(tmp.X + (tmp.Width - s_HealthBarSprites.Width) / 2f, tmp.Y - 10);
			}
			else
			{
				Rectangle tmp = ((NPC)m_ref).DrawArea;
				m_healthBarPos = new Vector2(tmp.X + (tmp.Width - s_HealthBarSprites.Width) / 2f, tmp.Y + ((NPC)m_ref).TopPixel - 10);
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
					Rectangle tmp = ((EOCharacterRenderer) m_ref).DrawAreaWithOffset;
					pos = new Vector2(tmp.X + tmp.Width/2f - 15, tmp.Y - m_additionalOffset - 9);
				}
				else
				{
					Rectangle tmp = ((NPC) m_ref).DrawArea;
					pos = new Vector2(tmp.X + tmp.Width/2f - 15, tmp.Y + ((NPC) m_ref).TopPixel - m_additionalOffset - 19);
				}
			}
			else
			{
				if (m_isCharacter)
				{
					Rectangle tmp = ((EOCharacterRenderer) m_ref).DrawAreaWithOffset;
					pos = new Vector2(tmp.X + tmp.Width/2f - (nDigits*9)/2f, tmp.Y - m_additionalOffset - 9);
				}
				else
				{
					Rectangle tmp = ((NPC) m_ref).DrawArea;
					pos = new Vector2(tmp.X + tmp.Width/2f - (nDigits*9)/2f, tmp.Y + ((NPC) m_ref).TopPixel - m_additionalOffset - 19);
				}
			}

			//don't bother drawing if we don't need to...
			//tall NPCs will do this
			if (pos.Y < 0)
				return;

			//then, draw the digits
			SpriteBatch.Begin();
			int ndx = 0;
			foreach (Rectangle r in m_numbersToDraw)
			{
				SpriteBatch.Draw(s_NumberSprites, pos + new Vector2(ndx * 9, 0), r, Color.White);
				ndx++;
			}

			SpriteBatch.Draw(s_HealthBarSprites, m_healthBarPos, new Rectangle(0, 28, 40, 7), Color.White); //draw health bar background container

			double pct;
			if (m_isCharacter)
				pct = ((EOCharacterRenderer) m_ref).Character.Stats.hp/(double)((EOCharacterRenderer) m_ref).Character.Stats.maxhp;
			else
				pct = ((NPC)m_ref).HP / (double)((NPC)m_ref).Data.HP;

			Rectangle healthSrcRect;
			if (pct >= .5)
				healthSrcRect = new Rectangle(0, 7, (int)Math.Round(pct * 40), 7);
			else if (pct >= .25)
				healthSrcRect = new Rectangle(0, 14, (int)Math.Round(pct * 40), 7);
			else
				healthSrcRect = new Rectangle(0, 21, (int)Math.Round(pct * 40), 7);

			SpriteBatch.Draw(s_HealthBarSprites, m_healthBarPos, healthSrcRect, Color.White);

			SpriteBatch.End();
			base.Draw(gameTime);
		}
	}

	/// <summary>
	/// Health bar shows red, yellow, or green health bar above the actor based on their percentage of health
	/// <para>Shows only when actor takes damage</para>
	/// </summary>
	public class HealthBar : XNAControl
	{
		
	}
}
