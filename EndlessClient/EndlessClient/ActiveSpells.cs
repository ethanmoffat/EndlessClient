// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.Net;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient
{
	//Responsible for:
	//	Selection
	//	Showing 'level' progress bar
	//	drag+drop
	public class SpellIcon : XNAControl
	{
		private const int ICON_AREA_WIDTH = 42, ICON_AREA_HEIGHT = 36;

		private int m_slot;
		public int Slot
		{
			get { return m_slot; }
			private set
			{
				m_slot = value;
				OnSlotChanged();
			}
		}

		private bool m_selected;
		public bool Selected
		{
			get { return m_selected; }
			set
			{
				m_selected = value;
				OnSelected();
			}
		}

		public short Level { get; set; }
		public SpellRecord SpellData { get; private set; }

		private readonly Texture2D m_spellGraphic, m_highlightColor;
		private Rectangle m_spellGraphicSourceRect;

		private DateTime m_clickTime;
		private bool m_dragging, m_followMouse;

		public SpellIcon(ActiveSpells parent, SpellRecord data, int slot)
			: base(null, null, parent)
		{
			Level = 0;
			Slot = 0;
			SpellData = data;

			m_spellGraphic = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.SpellIcons, SpellData.Icon);
			m_highlightColor = new Texture2D(Game.GraphicsDevice, 1, 1);
			m_highlightColor.SetData(new[] {Color.FromNonPremultiplied(200, 200, 200, 60)});

			OnSelected();

			_setSize(ICON_AREA_WIDTH, ICON_AREA_HEIGHT);
			Slot = slot;

			m_clickTime = DateTime.Now;
		}

		public override void Update(GameTime gameTime)
		{
			UpdateIconSourceRect();
			CheckForMouseClick();

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible) return;

			SpriteBatch.Begin();
			if (MouseOver)
				DrawHighlight();
			DrawSpellIcon();
			SpriteBatch.End();

			base.Draw(gameTime);
		}

		//-----------------------------------------------------
		// Helper methods
		//-----------------------------------------------------
		private void OnSlotChanged()
		{
			//start pos: 101, 97
			//xdelta: 45; ydelta: 52
			var row = Slot/ActiveSpells.SPELL_ROW_LENGTH;
			var col = Slot%ActiveSpells.SPELL_ROW_LENGTH;
			DrawLocation = new Vector2(101 + col * 45, 9 + row * 52);
		}

		private void OnSelected()
		{
			var halfWidth = m_spellGraphic.Width / 2;
			m_spellGraphicSourceRect = new Rectangle(Selected ? halfWidth : 0, 0, halfWidth, m_spellGraphic.Height);
		}

		private void UpdateIconSourceRect()
		{
			if (MouseOver && !MouseOverPreviously ||
				MouseOverPreviously && !MouseOver)
			{
				var halfWidth = m_spellGraphic.Width / 2;
				m_spellGraphicSourceRect = new Rectangle(MouseOver ? halfWidth : 0, 0, halfWidth, m_spellGraphic.Height);
				if (MouseOver)
					((EOGame) Game).Hud.SetStatusLabel(DATCONST2.SKILLMASTER_WORD_SPELL, SpellData.Name);
			}
		}

		private void CheckForMouseClick()
		{
			var currentState = Mouse.GetState();
			if (LeftButtonDown(currentState))
			{
				if (!m_dragging)
				{
					m_followMouse = true;
					m_clickTime = DateTime.Now;
				}
				else
				{
					EndDragging(currentState);
				}
			}
			else if (LeftButtonUp(currentState))
			{
				if (!m_dragging)
				{
					var clickDelta = (DateTime.Now - m_clickTime).TotalMilliseconds;
					if (clickDelta < 75)
					{
						m_dragging = true;
					}
				}
				else
				{
					EndDragging(currentState);
				}
			}

			if (!m_dragging && m_followMouse && (DateTime.Now - m_clickTime).TotalMilliseconds >= 75)
				m_dragging = true;
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

		private void EndDragging(MouseState currentState)
		{
			m_dragging = false;
			m_followMouse = false;

			var newSlot = GetCurrentHoverSlot(currentState);
			if (((ActiveSpells)parent).GetSpellRecordBySlot(newSlot) == null)
				Slot = newSlot;
		}

		private int GetCurrentHoverSlot(MouseState currentState)
		{
			var col = (currentState.X - DrawAreaWithOffset.X) / 45;
			var row = (currentState.Y - DrawAreaWithOffset.Y) / 52;
			return row * ActiveSpells.SPELL_ROW_LENGTH + col;
		}

		private void DrawSpellIcon()
		{
			Rectangle targetDrawArea;
			Color alphaColor;
			if (!m_followMouse)
			{
				targetDrawArea = new Rectangle(
					DrawAreaWithOffset.X + (DrawAreaWithOffset.Width - m_spellGraphicSourceRect.Width)/2,
					DrawAreaWithOffset.Y + (DrawAreaWithOffset.Height - m_spellGraphicSourceRect.Height)/2,
					m_spellGraphicSourceRect.Width,
					m_spellGraphicSourceRect.Height);
				alphaColor = Color.White;
			}
			else
			{
				targetDrawArea = new Rectangle(
					Mouse.GetState().X - m_spellGraphicSourceRect.Width / 2,
					Mouse.GetState().Y - m_spellGraphicSourceRect.Height / 2,
					m_spellGraphicSourceRect.Width,
					m_spellGraphicSourceRect.Height
					);
				alphaColor = Color.FromNonPremultiplied(255, 255, 255, 128);
			}

			SpriteBatch.Draw(m_spellGraphic, targetDrawArea, m_spellGraphicSourceRect, alphaColor);
		}

		private void DrawHighlight()
		{
			SpriteBatch.Draw(m_highlightColor, DrawAreaWithOffset, Color.White);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_highlightColor.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	public class ActiveSpells : XNAControl
	{
		//number of skills to display in a row
		public const int SPELL_NUM_ROWS = 2;
		public const int SPELL_ROW_LENGTH = 8;

		private readonly PacketAPI m_api;
		private readonly RegistryKey m_spellsKey;

		private readonly bool[,] m_filledSlots = new bool[SPELL_NUM_ROWS, SPELL_ROW_LENGTH];
		private readonly List<SpellIcon> m_childItems;

		public ActiveSpells(XNAPanel parent, PacketAPI api)
			: base(null, null, parent)
		{
			m_api = api;

			m_childItems = new List<SpellIcon>(SPELL_NUM_ROWS * SPELL_ROW_LENGTH);

			var localSpellSlotMap = new Dictionary<int, int>();
			m_spellsKey = _tryGetCharacterRegKey();
			if (m_spellsKey != null)
			{
				const string spellFmt = "item{0}";
				for (int i = 0; i < SPELL_ROW_LENGTH*4; ++i)
				{
					int id;
					try
					{
						id = Convert.ToInt32(m_spellsKey.GetValue(String.Format(spellFmt, i)));
					}
					catch { continue; }
					localSpellSlotMap.Add(i, id);
				}
			}

			var localSpells = World.Instance.MainPlayer.ActiveCharacter.Spells;
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var spell in localSpells)
			{
				SpellRecord rec = World.Instance.ESF.GetSpellRecordByID(spell.id);
				int slot = localSpellSlotMap.ContainsValue(spell.id)
					? localSpellSlotMap.First(_pair => _pair.Value == spell.id).Key
					: _getNextOpenSlot(m_filledSlots);

				if (slot < 0 || !_addItemToSlot(slot, rec, spell.level))
				{
					EODialog.Show("You have too many spells! They don't all fit.", "Warning", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					break;
				}
			}

			//setup other controls that are required
			//level up button, selected spell label / image, etc
		}

		public void SetActiveSpellBySlot(int slot)
		{
			ClearActiveSpell();
			var item = m_childItems.Find(x => x.Slot == slot);
			if (item != null && !item.Selected)
				item.Selected = true;
		}

		public void ClearActiveSpell()
		{
			m_childItems.ForEach(x => x.Selected = false);
		}

		public SpellRecord GetSpellRecordBySlot(int slot)
		{
			var matchingItem = m_childItems.Find(x => x.Slot == slot);
			if (matchingItem != null)
				return matchingItem.SpellData;
			return null;
		}

		//-----------------------------------------------------
		// Helper methods
		//-----------------------------------------------------

		private bool _addItemToSlot(int slot, SpellRecord data, short level)
		{
			if (slot < 0 || m_childItems.Count(x => x.Slot == slot) > 0)
				return false;

			int row = slot / SPELL_ROW_LENGTH;
			int col = slot % SPELL_ROW_LENGTH;
			m_filledSlots[row, col] = true;

			m_spellsKey.SetValue(string.Format("item{0}", slot), data.ID, RegistryValueKind.String);
			m_childItems.Add(new SpellIcon(this, data, slot) { Level = level });
			return true;
		}

		private static RegistryKey _tryGetCharacterRegKey()
		{
			try
			{
				using (RegistryKey currentUser = Registry.CurrentUser)
				{
					return currentUser.CreateSubKey(string.Format("Software\\EndlessClient\\{0}\\{1}\\spells", World.Instance.MainPlayer.AccountName, World.Instance.MainPlayer.ActiveCharacter.Name),
						RegistryKeyPermissionCheck.ReadWriteSubTree);
				}
			}
			catch (NullReferenceException) { }
			return null;
		}

		private static int _getNextOpenSlot(bool[,] slots)
		{
			//just 2 rows for active skills
			for (int row = 0; row < SPELL_NUM_ROWS; ++row)
			{
				for (int col = 0; col < SPELL_ROW_LENGTH; ++col)
				{
					if (!slots[row, col])
						return row*SPELL_ROW_LENGTH + col;
				}
			}

			return -1;
		}
	}
}
