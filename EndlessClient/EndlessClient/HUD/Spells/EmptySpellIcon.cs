// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.HUD.Spells
{
	public class EmptySpellIcon : XNAControl, ISpellIcon
	{
		private const int ICON_AREA_WIDTH = 42, ICON_AREA_HEIGHT = 36;

		private int _slot;
		public int Slot
		{
			get { return _slot; }
			set
			{
				_slot = value;
				OnSlotChanged();
			}
		}

		public virtual bool Selected
		{
			get { return false; }
			set
			{
				if (value)
				{
					((EOGame) Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.SPELL_NOTHING_WAS_SELECTED);
				}
			}
		}

		public virtual short Level
		{
			get { return 0; }
			set { throw new InvalidOperationException("Unable to set Level on an EmptySpellIcon"); }
		}

		public virtual bool IsDragging { get { return false; } }

		public virtual SpellRecord SpellData { get { return null; } }

		private bool _doUpdateLogic = true;
		protected virtual bool DoEmptySpellIconUpdateLogic { get { return _doUpdateLogic; } }

		protected readonly ActiveSpells _parentSpellContainer;
		private readonly Texture2D _highlightColor;
		private bool _doDrawLogic;

		public EmptySpellIcon(ActiveSpells parent, int slot)
			: base(null, null, parent)
		{
			Slot = slot;
			_parentSpellContainer = parent;

			_highlightColor = new Texture2D(Game.GraphicsDevice, 1, 1);
			_highlightColor.SetData(new[] { Color.FromNonPremultiplied(200, 200, 200, 60) });

			_setSize(ICON_AREA_WIDTH, ICON_AREA_HEIGHT);

			World.IgnoreDialogs(this);
		}

		public void SetDisplaySlot(int displaySlot)
		{
			var currentSlot = _slot;
			Slot = displaySlot;
			_slot = currentSlot;
		}

		public override void Update(GameTime gameTime)
		{
			if (!ShouldUpdate()) return;

			if (DoEmptySpellIconUpdateLogic)
			{
				if (MouseOver && MouseOverPreviously &&
					_parentSpellContainer.AnySpellsSelected() &&
					!_parentSpellContainer.AnySpellsDragging() &&
					Mouse.GetState().LeftButton == ButtonState.Released &&
					PreviousMouseState.LeftButton == ButtonState.Pressed)
				{
					_parentSpellContainer.ClearSelectedSpell();
				}
			}

			if (MouseOver && _parentSpellContainer.AnySpellsDragging())
				_doDrawLogic = true;
			else if (_doDrawLogic)
				_doDrawLogic = false;

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible) return;

			if (_doDrawLogic)
			{
				SpriteBatch.Begin();
				DrawHighlight();
				SpriteBatch.End();
			}

			base.Draw(gameTime);
		}

		protected virtual void OnSlotChanged()
		{
			//start pos: 101, 97
			//xdelta: 45; ydelta: 52
			var row = Slot / ActiveSpells.SPELL_ROW_LENGTH;
			var col = Slot % ActiveSpells.SPELL_ROW_LENGTH;
			DrawLocation = new Vector2(101 + col * 45, 9 + row * 52);
		}

		private void DrawHighlight()
		{
			SpriteBatch.Draw(_highlightColor, DrawAreaWithOffset, Color.White);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_highlightColor.Dispose();
			base.Dispose(disposing);
		}
	}
}
