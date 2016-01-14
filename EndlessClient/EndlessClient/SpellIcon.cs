// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient
{
	public class SpellIcon : EmptySpellIcon
	{
		private bool _selected;
		public new bool Selected
		{
			get { return _selected; }
			set
			{
				_selected = value;
				OnSelected();
			}
		}

		public override short Level { get; set; }

		public override bool IsDragging { get { return _dragging; } }

		public override SpellRecord SpellData { get { return _spellData; } }

		//stops the base class update logic from being called
		protected override bool DoEmptySpellIconUpdateLogic { get { return false; } }
		private readonly Texture2D _spellGraphic;
		private readonly SpellRecord _spellData;

		private Rectangle _spellGraphicSourceRect;
		private DateTime _clickTime;
		private bool _dragging, _followMouse;

		public SpellIcon(ActiveSpells parent, SpellRecord data, int slot)
			: base(parent, slot)
		{
			_spellData = data;
			_spellGraphic = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.SpellIcons, _spellData.Icon);

			OnSelected();

			_clickTime = DateTime.Now;
		}

		public override void Update(GameTime gameTime)
		{
			UpdateIconSourceRect();
			DoClickAndDragLogic();

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible) return;

			SpriteBatch.Begin();
			DrawSpellIcon();
			SpriteBatch.End();

			base.Draw(gameTime);
		}

		private void OnSelected()
		{
			var halfWidth = _spellGraphic.Width / 2;
			_spellGraphicSourceRect = new Rectangle(Selected ? halfWidth : 0, 0, halfWidth, _spellGraphic.Height);
		}

		private void UpdateIconSourceRect()
		{
			if (MouseOver && !MouseOverPreviously ||
				MouseOverPreviously && !MouseOver)
			{
				var halfWidth = _spellGraphic.Width / 2;
				_spellGraphicSourceRect = new Rectangle(MouseOver ? halfWidth : 0, 0, halfWidth, _spellGraphic.Height);
				if (MouseOver && !_parentSpellContainer.AnySpellsDragging())
					((EOGame) Game).Hud.SetStatusLabel(DATCONST2.SKILLMASTER_WORD_SPELL, SpellData.Name);
			}
		}

		private void DoClickAndDragLogic()
		{
			if (!_dragging && _parentSpellContainer.AnySpellsDragging())
				return;

			var currentState = Mouse.GetState();
			if (LeftButtonDown(currentState))
			{
				if (!_dragging)
				{
					_followMouse = true;
					_clickTime = DateTime.Now;
				}
				else
				{
					EndDragging();
				}
			}
			else if (LeftButtonUp(currentState))
			{
				if (!_dragging)
				{
					var clickDelta = (DateTime.Now - _clickTime).TotalMilliseconds;
					if (clickDelta < 75)
					{
						_dragging = true;
					}
				}
				else
				{
					EndDragging();
				}
			}

			if (!_dragging && _followMouse && (DateTime.Now - _clickTime).TotalMilliseconds >= 75)
				_dragging = true;
		}

		private bool LeftButtonDown(MouseState currentState)
		{
			return MouseOver && MouseOverPreviously &&
				   currentState.LeftButton == ButtonState.Pressed &&
				   PreviousMouseState.LeftButton == ButtonState.Released;
		}

		private bool LeftButtonUp(MouseState currentState)
		{
			return currentState.LeftButton == ButtonState.Released &&
				   PreviousMouseState.LeftButton == ButtonState.Pressed;
		}

		private void EndDragging()
		{
			_dragging = false;
			_followMouse = false;

			var newSlot = GetCurrentHoverSlot();
			if (_parentSpellContainer.GetSpellRecordBySlot(newSlot) == null)
			{
				_parentSpellContainer.MoveItem(this, newSlot);
			}
		}

		private int GetCurrentHoverSlot()
		{
			return _parentSpellContainer.GetCurrentHoverSlot();
		}

		private void DrawSpellIcon()
		{
			Rectangle targetDrawArea;
			Color alphaColor;
			if (!_followMouse)
			{
				targetDrawArea = new Rectangle(
					DrawAreaWithOffset.X + (DrawAreaWithOffset.Width - _spellGraphicSourceRect.Width) / 2,
					DrawAreaWithOffset.Y + (DrawAreaWithOffset.Height - _spellGraphicSourceRect.Height) / 2,
					_spellGraphicSourceRect.Width,
					_spellGraphicSourceRect.Height);
				alphaColor = Color.White;
			}
			else
			{
				targetDrawArea = new Rectangle(
					Mouse.GetState().X - _spellGraphicSourceRect.Width / 2,
					Mouse.GetState().Y - _spellGraphicSourceRect.Height / 2,
					_spellGraphicSourceRect.Width,
					_spellGraphicSourceRect.Height
					);
				alphaColor = Color.FromNonPremultiplied(255, 255, 255, 128);
			}

			SpriteBatch.Draw(_spellGraphic, targetDrawArea, _spellGraphicSourceRect, alphaColor);
		}
	}
}
