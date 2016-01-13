// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.IO;
using EOLib.Net;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient
{
	public class ActiveSpells : XNAControl
	{
		//number of skills to display in a row
		public const int SPELL_NUM_ROWS = 2;
		public const int SPELL_ROW_LENGTH = 8;

		private readonly PacketAPI m_api;
		private readonly RegistryKey m_spellsKey;

		private readonly List<ISpellIcon> _childItems;

		public ActiveSpells(XNAPanel parent, PacketAPI api)
			: base(null, null, parent)
		{
			m_api = api;

			_childItems = new List<ISpellIcon>(SPELL_NUM_ROWS * SPELL_ROW_LENGTH);
			for (int slot = 0; slot < SPELL_NUM_ROWS*SPELL_ROW_LENGTH; ++slot)
				_childItems.Add(new EmptySpellIcon(this, slot));

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
					: _getNextOpenSlot();

				if (slot < 0 || !SetItemSlot(slot, rec, spell.level))
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
			var item = _childItems.OfType<SpellIcon>().Single(x => x.Slot == slot);
			if (item != null && !item.Selected)
				item.Selected = true;
		}

		public void ClearActiveSpell()
		{
			_childItems.Single(x => x.Selected).Selected = false;
		}

		public SpellRecord GetSpellRecordBySlot(int slot)
		{
			var icon = _childItems.OfType<SpellIcon>().SingleOrDefault(x => x.Slot == slot);

			return icon == null ? null : icon.SpellData;
		}

		public bool AnySpellsDragging()
		{
			return _childItems.Any(x => x.IsDragging);
		}

		public int GetCurrentHoverSlot()
		{
			if (!_childItems.Any(x => x.MouseOver))
				return -1;
			return _childItems.Single(x => x.MouseOver).Slot;
		}

		public bool SetItemSlot(int slot, SpellRecord data, short level)
		{
			var childItem = _childItems.Single(x => x.Slot == slot);
			if (slot < 0 || !(childItem is EmptySpellIcon))
				return false;

			m_spellsKey.SetValue(string.Format("item{0}", slot), data.ID, RegistryValueKind.String);
			_childItems.Remove(childItem);
			_childItems.Add(new SpellIcon(this, data, slot) { Level = level });

			return true;
		}

		public override void Update(GameTime gameTime)
		{
			if (MouseOver && MouseOverPreviously &&
			    _childItems.Any(x => x.Selected) &&
			    _childItems.All(x => !x.MouseOver) &&
				!AnySpellsDragging() &&
				Mouse.GetState().LeftButton == ButtonState.Released &&
				PreviousMouseState.LeftButton == ButtonState.Pressed)
			{
				ClearActiveSpell();
			}

			base.Update(gameTime);
		}

		private int _getNextOpenSlot()
		{
			return _childItems.OfType<EmptySpellIcon>().Select(x => x.Slot).Min();
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
	}
}
