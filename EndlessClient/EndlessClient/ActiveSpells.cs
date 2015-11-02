// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.Data;
using EOLib.Graphics;
using EOLib.Net;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient
{
	//Responsible for:
	//	Selection
	//	Showing 'level' progress bar
	//	drag+drop
	public class SpellIcon : XNAControl
	{
		private int m_slot;
		public int Slot
		{
			get { return m_slot; }
			/*private */set
			{
				m_slot = value;
				//OnSlotChanged();
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

		public SpellIcon(ActiveSpells parent, SpellRecord data, int slot)
			: base(null, null, parent)
		{
			Level = 0;
			Slot = 0;
			SpellData = data;

			m_spellGraphic = ((EOGame)Game).GFXLoader.TextureFromResource(GFXTypes.SpellIcons, SpellData.Icon);
			m_highlightColor = new Texture2D(Game.GraphicsDevice, 1, 1);
			//todo: figure out color of background
			m_highlightColor.SetData(new[] {Color.FromNonPremultiplied(0, 0, 0, 0)});

			Selected = false; //calls OnSelected

			//set up label for display spell name

			Slot = slot;
		}

		//-----------------------------------------------------
		// Helper methods
		//-----------------------------------------------------
		//private void OnSlotChanged() //todo: determine if this is required
		//{
		//}

		private void OnSelected()
		{
			var halfWidth = m_spellGraphic.Width/2;
			m_spellGraphicSourceRect = new Rectangle(Selected ? halfWidth : 0, 0, halfWidth, m_spellGraphic.Height);
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

		public void SetActiveSpellByID(short spellID)
		{
			ClearActiveSpell();
			var item = m_childItems.Find(x => x.SpellData.ID == spellID);
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

			int row = slot / SPELL_NUM_ROWS;
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
