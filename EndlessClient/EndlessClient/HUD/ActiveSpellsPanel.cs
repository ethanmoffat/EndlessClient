// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Controls;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Spells;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.Net.API;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.HUD
{
	public class ActiveSpells : XNAControl
	{
		//number of skills to display in a row
		public const int SPELL_NUM_ROWS = 4;
		public const int SPELL_ROW_LENGTH = 8;

		private readonly PacketAPI _api;
		private readonly RegistryKey _spellsKey;

		private readonly List<ISpellIcon> _childItems;
		private readonly Texture2D _functionKeyGraphics;
		private Rectangle _functionKeyRow1SourceRect;
		private Rectangle _functionKeyRow2SourceRect;

		private Texture2D _activeSpellIcon;

		private readonly XNALabel _selectedSpellName, _selectedSpellLevel, _totalSkillPoints;
		private readonly XNAButton _levelUpButton1, _levelUpButton2;

		private int _lastScrollOffset;
		private readonly ScrollBar _scroll;

		private bool _trainWarningShown;

		public ActiveSpells(XNAPanel parent, PacketAPI api)
			: base(null, null, parent)
		{
			_api = api;

			_childItems = new List<ISpellIcon>(SPELL_NUM_ROWS * SPELL_ROW_LENGTH);
			RemoveAllSpells();

			var localSpellSlotMap = new Dictionary<int, int>();
			_spellsKey = _tryGetCharacterRegKey();
			if (_spellsKey != null)
			{
				const string spellFmt = "item{0}";
				for (int i = 0; i < SPELL_ROW_LENGTH*4; ++i)
				{
					int id;
					try
					{
						id = Convert.ToInt32(_spellsKey.GetValue(String.Format(spellFmt, i)));
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

				if (slot < 0 || !_addNewSpellToSlot(slot, rec, spell.level))
				{
					EOMessageBox.Show("You have too many spells! They don't all fit.", "Warning", XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
					break;
				}

				if (slot >= SPELL_ROW_LENGTH*(SPELL_NUM_ROWS/2))
					_childItems.Last().Visible = false;
			}

			_setSize(parent.DrawArea.Width, parent.DrawArea.Height);

			_functionKeyGraphics = ((EOGame) Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 58, true);
			_functionKeyRow1SourceRect = new Rectangle(148, 51, 18, 13);
			_functionKeyRow2SourceRect = new Rectangle(148 + 18*8, 51, 18, 13);

			_selectedSpellName = new XNALabel(new Rectangle(9, 50, 81, 13), Constants.FontSize08pt5)
			{
				Visible = false,
				Text = "",
				AutoSize = false,
				TextAlign = LabelAlignment.MiddleCenter,
				ForeColor = Constants.LightGrayText
			};
			_selectedSpellName.SetParent(this);

			_selectedSpellLevel = new XNALabel(new Rectangle(32, 78, 42, 15), Constants.FontSize08pt5)
			{
				Visible = true,
				Text = "0",
				AutoSize = false,
				TextAlign = LabelAlignment.MiddleLeft,
				ForeColor = Constants.LightGrayText
			};
			_selectedSpellLevel.SetParent(this);

			var skillPoints = World.Instance.MainPlayer.ActiveCharacter.Stats.SkillPoints;
			_totalSkillPoints = new XNALabel(new Rectangle(32, 96, 42, 15), Constants.FontSize08pt5)
			{
				Visible = true,
				Text = skillPoints.ToString(),
				AutoSize = false,
				TextAlign = LabelAlignment.MiddleLeft,
				ForeColor = Constants.LightGrayText
			};
			_totalSkillPoints.SetParent(this);

			var buttonSheet = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 27, true);

			_levelUpButton1 = new XNAButton(buttonSheet, new Vector2(71, 77), new Rectangle(215, 386, 19, 15), new Rectangle(234, 386, 19, 15))
			{
				FlashSpeed = 500,
				Visible = false
			};
			_levelUpButton1.OnClick += LevelUp_Click;
			_levelUpButton1.SetParent(this);
			_levelUpButton2 = new XNAButton(buttonSheet, new Vector2(71, 95), new Rectangle(215, 386, 19, 15), new Rectangle(234, 386, 19, 15))
			{
				FlashSpeed = 500,
				Visible = false
			};
			_levelUpButton2.OnClick += LevelUp_Click;
			_levelUpButton2.SetParent(this);

			_scroll = new ScrollBar(this, new Vector2(467, 2), new Vector2(16, 115), ScrollBarColors.LightOnMed) { LinesToRender = 2 };
			_scroll.UpdateDimensions(4);

			foreach (var child in children.Where(x => !(x is EmptySpellIcon)))
				World.IgnoreDialogs(child);
		}

		public void AddNewSpellToNextOpenSlot(int spellID)
		{
			if (_childItems.OfType<SpellIcon>().Any(x => x.SpellData.ID == spellID))
				return;

			var slot = _getNextOpenSlot();
			var record = World.Instance.ESF.GetSpellRecordByID((short) spellID);
			_addNewSpellToSlot(slot, record, 0);
		}

		public void SetSelectedSpellBySlot(int slot)
		{
			ClearSelectedSpell();
			var item = _childItems.Single(x => x.Slot == slot);
			if (item != null)
			{
				item.Selected = true;
				if (item is SpellIcon)
				{
					_activeSpellIcon = ((EOGame) Game).GFXManager.TextureFromResource(GFXTypes.SpellIcons, item.SpellData.Icon);
					_selectedSpellName.Text = item.SpellData.Name;
					_selectedSpellName.Visible = true;
					_selectedSpellLevel.Text = item.Level.ToString();

					UpdateLevelUpButtonsVisible();
				}
			}
		}

		public void ClearSelectedSpell()
		{
			if (!AnySpellsSelected()) return;

			_childItems.Single(x => x.Selected).Selected = false;

			_selectedSpellName.Visible = false;
			_selectedSpellLevel.Text = "0";

			_levelUpButton1.Visible = false;
			_levelUpButton2.Visible = false;

			_activeSpellIcon = null;
		}

		public SpellRecord GetSpellRecordBySlot(int slot)
		{
			var icon = _childItems.SingleOrDefault(x => x.Slot == slot);

			return icon == null ? null : icon.SpellData;
		}

		public void RemoveSpellByID(int spellID)
		{
			var spellToRemove = _childItems.OfType<SpellIcon>().Single(x => x.SpellData.ID == spellID);
			if (spellToRemove.Selected)
				ClearSelectedSpell();

			_childItems.Remove(spellToRemove);
			spellToRemove.SetParent(null);
			spellToRemove.Close();

			var newEmptySpell = new EmptySpellIcon(this, spellToRemove.Slot);
			newEmptySpell.SetDisplaySlot(GetDisplaySlotFromSlot(newEmptySpell.Slot));
			_childItems.Add(newEmptySpell);
		}

		public void UpdateSpellLevelByID(short spellID, short spellLevel)
		{
			var spell = _childItems.OfType<SpellIcon>().Single(x => x.SpellData.ID == spellID);
			spell.Level = spellLevel;
			if (spell.Selected)
			{
				_selectedSpellLevel.Text = spell.Level.ToString();
				UpdateLevelUpButtonsVisible();
			}
		}

		public bool AnySpellsDragging()
		{
			return _childItems.Any(x => x.IsDragging);
		}

		public bool AnySpellsSelected()
		{
			return _childItems.Any(x => x.Selected);
		}

		public int GetCurrentHoverSlot()
		{
			if (!_childItems.Any(x => x.MouseOver && x.Visible))
				return -1;
			return _childItems.Single(x => x.MouseOver && x.Visible).Slot;
		}

		public void MoveItem(ISpellIcon spellIcon, int newSlot)
		{
			if (spellIcon.Slot == newSlot || newSlot < 0 || newSlot > SPELL_NUM_ROWS * SPELL_ROW_LENGTH)
				return;

			if (!_childItems.Contains(spellIcon))
				throw new ArgumentException("The spell was not found!", "spellIcon");

			//update the registry
			var spellInDestinationSlot = _childItems.Find(x => x.Slot == newSlot);
			if (spellInDestinationSlot is SpellIcon)
				_setSpellSlotInRegistry(spellIcon.Slot, spellInDestinationSlot.SpellData.ID);
			else
				_clearSlotInRegistry(spellIcon.Slot);
			_setSpellSlotInRegistry(newSlot, spellIcon.SpellData.ID);

			//set the slots of old/new items
			spellInDestinationSlot.Slot = spellIcon.Slot;
			spellInDestinationSlot.SetDisplaySlot(GetDisplaySlotFromSlot(spellIcon.Slot));
			spellIcon.Slot = newSlot;
			spellIcon.SetDisplaySlot(GetDisplaySlotFromSlot(newSlot));
		}

		public void RefreshTotalSkillPoints()
		{
			var skillPoints = World.Instance.MainPlayer.ActiveCharacter.Stats.SkillPoints;
			_totalSkillPoints.Text = skillPoints.ToString();

			UpdateLevelUpButtonsVisible();
		}

		public void RemoveAllSpells()
		{
			ClearSelectedSpell();

			_childItems.OfType<XNAControl>().ToList()
				.ForEach(x =>
				{
					x.SetParent(null);
					x.Close();
				});
			_childItems.Clear();

			for (int slot = 0; slot < SPELL_NUM_ROWS * SPELL_ROW_LENGTH; ++slot)
				_childItems.Add(new EmptySpellIcon(this, slot));
		}

		public override void Update(GameTime gameTime)
		{
			if (!ShouldUpdate()) return;

			if ((Keyboard.GetState().IsKeyDown(Keys.RightShift) && PreviousKeyState.IsKeyUp(Keys.RightShift)) ||
			    (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && PreviousKeyState.IsKeyUp(Keys.LeftShift)) ||
			    (Keyboard.GetState().IsKeyUp(Keys.RightShift) && PreviousKeyState.IsKeyDown(Keys.RightShift)) ||
			    (Keyboard.GetState().IsKeyUp(Keys.LeftShift) && PreviousKeyState.IsKeyDown(Keys.LeftShift)))
				_swapFunctionKeySourceRectangles();

			if (_lastScrollOffset != _scroll.ScrollOffset)
				UpdateIconsForScroll();

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible) return;

			//draw spell icons first
			base.Draw(gameTime);

			SpriteBatch.Begin();
			DrawFunctionKeyLabels();
			DrawActiveSpell();
			SpriteBatch.End();
		}

		private void DrawFunctionKeyLabels()
		{
			for (int i = 0; i < 8; ++i)
			{
				var offset = (float) _functionKeyRow1SourceRect.Width*i;

				if (_scroll.ScrollOffset == 0)
				{
					SpriteBatch.Draw(_functionKeyGraphics,
						new Vector2(202 + 45*i, 338),
						_functionKeyRow1SourceRect.WithPosition(new Vector2(_functionKeyRow1SourceRect.X + offset,
							_functionKeyRow1SourceRect.Y)),
						Color.White);
				}

				if (_scroll.ScrollOffset < 2)
				{
					var yCoord = _scroll.ScrollOffset == 0 ? 390 : 338;
					SpriteBatch.Draw(_functionKeyGraphics,
						new Vector2(202 + 45*i, yCoord),
						_functionKeyRow2SourceRect.WithPosition(new Vector2(_functionKeyRow2SourceRect.X + offset,
							_functionKeyRow2SourceRect.Y)),
						Color.White);
				}
			}
		}

		private void DrawActiveSpell()
		{
			if (_activeSpellIcon == null)
				return;

			var srcRect = new Rectangle(0, 0, _activeSpellIcon.Width/2, _activeSpellIcon.Height);
			var dstRect = new Rectangle(DrawAreaWithOffset.X + 32, DrawAreaWithOffset.Y + 14, srcRect.Width, srcRect.Height);
			SpriteBatch.Draw(_activeSpellIcon, dstRect, srcRect, Color.White);
		}

		private void LevelUp_Click(object sender, EventArgs args)
		{
			if (!_trainWarningShown)
			{
				//apparently this is NOT stored in the edf files...
				//NOTE: copy-pasted from EOCharacterStats button event handler. Should probably be in some shared function somewhere.
				EOMessageBox dlg = new EOMessageBox("Do you want to train?", "Skill training", XNADialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader);
				dlg.DialogClosing += (s, e) =>
				{
					if (e.Result != XNADialogResult.OK) return;
					_trainWarningShown = true;
				};

				return;
			}

			var selectedSpell = _childItems.Single(x => x.Selected);
			if (selectedSpell == null || !_api.LevelUpSpell((short) selectedSpell.SpellData.ID))
				((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
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

		private bool _addNewSpellToSlot(int slot, SpellRecord data, short level)
		{
			var emptySpellInDestinationSlot = _childItems.Single(x => x.Slot == slot);
			if (slot < 0 || !(emptySpellInDestinationSlot is EmptySpellIcon))
				return false;

			_setSpellSlotInRegistry(slot, data.ID);
			_childItems.Remove(emptySpellInDestinationSlot);
			((XNAControl)emptySpellInDestinationSlot).SetParent(null);
			((XNAControl)emptySpellInDestinationSlot).Close();

			var newSpell = new SpellIcon(this, data, slot) {Level = level};

			var displaySlot = GetDisplaySlotFromSlot(slot);
			newSpell.SetDisplaySlot(displaySlot);

			var scrollOffset = _scroll == null ? 0 : _scroll.ScrollOffset;
			newSpell.Visible = displaySlot >= scrollOffset*SPELL_ROW_LENGTH &&
			                   displaySlot < scrollOffset*SPELL_ROW_LENGTH + 2*SPELL_ROW_LENGTH;

			_childItems.Add(newSpell);

			return true;
		}

		private int _getNextOpenSlot()
		{
			//SpellIcon is EmptySpellIcon, so can't compare with EmptySpellIcon or we'll get SpellIcon too
			if (_childItems.All(x => x is SpellIcon))
				return -1;
			return _childItems.Where(x => !(x is SpellIcon)).Select(x => x.Slot).Min();
		}

		private void _setSpellSlotInRegistry(int slot, int id)
		{
			_spellsKey.SetValue(string.Format("item{0}", slot), id, RegistryValueKind.String);
		}

		private void _clearSlotInRegistry(int slot)
		{
			_setSpellSlotInRegistry(slot, 0);
		}

		private void _swapFunctionKeySourceRectangles()
		{
			var tmpRect = _functionKeyRow2SourceRect;
			_functionKeyRow2SourceRect = _functionKeyRow1SourceRect;
			_functionKeyRow1SourceRect = tmpRect;
		}

		private void UpdateLevelUpButtonsVisible()
		{
			var pts = World.Instance.MainPlayer.ActiveCharacter.Stats.SkillPoints;
			_levelUpButton1.Visible = pts > 0 && AnySpellsSelected();
			_levelUpButton2.Visible = pts > 0 && AnySpellsSelected();
		}

		private void UpdateIconsForScroll()
		{
			var firstValidSlot = _scroll.ScrollOffset*SPELL_ROW_LENGTH;
			var lastValidSlot = firstValidSlot + 2*SPELL_ROW_LENGTH;

			var itemsToHide = _childItems.Where(x => x.Slot < firstValidSlot || x.Slot >= lastValidSlot).ToList();
			foreach (var item in itemsToHide)
			{
				item.Visible = false;
				item.SetDisplaySlot(GetDisplaySlotFromSlot(item.Slot));
			}

			foreach (var item in _childItems.Except(itemsToHide))
			{
				item.Visible = true;
				item.SetDisplaySlot(item.Slot - firstValidSlot);
			}

			_lastScrollOffset = _scroll.ScrollOffset;
		}

		private int GetDisplaySlotFromSlot(int newSlot)
		{
			var offset = _scroll == null ? 0 : _scroll.ScrollOffset;
			return newSlot - SPELL_ROW_LENGTH * offset;
		}
	}
}
