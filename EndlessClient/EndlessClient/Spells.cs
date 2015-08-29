using EOLib.Data;
using EOLib.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient
{
	public class EOSpellIcon : XNAControl
	{
		public bool Selected { get; set; }
		public int Slot { get; set; }

		private Texture2D m_texture;
		private Rectangle m_textureSrcRect;

		private readonly SpellRecord m_spellData;
		//private bool m_dragging;

		public EOSpellIcon(EOSpellsPanel parent, CharacterSpell spell)
			: base(null, null, parent)
		{
			m_spellData = (SpellRecord) World.Instance.ESF.Data[spell.id];
			m_texture = GFXLoader.TextureFromResource(GFXTypes.SpellIcons, m_spellData.Icon);
			m_textureSrcRect = new Rectangle(0, 0, m_texture.Width / 2, m_texture.Height);
		}

		public override void Update(GameTime gameTime)
		{
			if (!ShouldUpdate()) return;

			//need to check for - 
			//	 - double click: primes/uses spell
			//	 - single click: move it w/o mouse button being held
			//	 - drag + drop: move it by dragging with button held

			//set correct texture on mouseover - occurs regardless of whether or not its being dragged
			if (MouseOver && !MouseOverPreviously)
			{
				m_textureSrcRect = new Rectangle(m_texture.Width / 2, 0, m_texture.Width / 2, m_texture.Height);
			}
			else if (!MouseOver && MouseOverPreviously)
			{
				m_textureSrcRect = new Rectangle(0, 0, m_texture.Width / 2, m_texture.Height);
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch.Begin();
			SpriteBatch.Draw(m_texture, DrawAreaWithOffset, m_textureSrcRect,/* m_dragging ? Color.FromNonPremultiplied(255, 255, 255, 128):*/ Color.White);
			if (MouseOver)
			{
				//SpriteBatch.Draw(m_highlightBG, DrawAreaWithOffset, Color.FromNonPremultiplied(255, 255, 255, 64));
			}
			SpriteBatch.End();
			base.Draw(gameTime);
		}
	}

	public class EOSpellsPanel : XNAControl
	{
		public EOSpellsPanel(XNAPanel parent)
			: base(null, null, parent)
		{
			_setSize(parent.BackgroundImage.Width, parent.BackgroundImage.Height);
		}
	}
}
