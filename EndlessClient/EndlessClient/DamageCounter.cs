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

		private static Texture2D SpriteSheet;

		private readonly List<Rectangle> m_numbersToDraw;
		private float m_additionalOffset;

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

			//lazy init of spritesheet - static so same texture in use for all damage counters
			//this sheet is a subsheet of GFX002/158 that has only the numbers and 'miss' text
			if (SpriteSheet == null)
			{
				Texture2D wholeSheet = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 58, true);
				Color[] data = new Color[122*23];
				wholeSheet.GetData(0, new Rectangle(41, 29, 122, 23), data, 0, data.Length);

				SpriteSheet = new Texture2D(Game.GraphicsDevice, 122, 23);
				SpriteSheet.SetData(data);
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

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			//Please note: DrawAreaWithOffset is not used for this XNAControl

			if (!Visible)
				return;

			int nDigits = m_numbersToDraw.Count;
			Vector2 pos;
			if (m_value == 0)
			{
				//misses should show dead center
				if (m_isCharacter)
				{
					Rectangle tmp = ((EOCharacterRenderer)m_ref).DrawAreaWithOffset;
					pos = new Vector2(tmp.X + tmp.Width / 2f - 15, tmp.Y - m_additionalOffset - 5);
				}
				else
				{
					Rectangle tmp = ((NPC)m_ref).DrawArea;
					pos = new Vector2(tmp.X + tmp.Width / 2f - 15, tmp.Y - m_additionalOffset - 5);
				}
			}
			else
			{
				if (m_isCharacter)
				{
					Rectangle tmp = ((EOCharacterRenderer) m_ref).DrawAreaWithOffset;
					pos = new Vector2(tmp.X + tmp.Width/2f - (nDigits*9)/2f, tmp.Y - m_additionalOffset - 5);
				}
				else
				{
					Rectangle tmp = ((NPC) m_ref).DrawArea;
					pos = new Vector2(tmp.X + tmp.Width/2f - (nDigits*9)/2f, tmp.Y - m_additionalOffset - 5);
				}
			}

			//don't bother drawing if we don't need to...
			//tall NPCs will do this
			if (pos.Y < 0)
				return;

			SpriteBatch.Begin();
			int ndx = 0;
			foreach (Rectangle r in m_numbersToDraw)
			{
				SpriteBatch.Draw(SpriteSheet, pos + new Vector2(ndx * 9, 0), r, Color.White);
				ndx++;
			}
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
