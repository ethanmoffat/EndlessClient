// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Inventory;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.Net.API;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.HUD
{
	public class EOInventory : XNAControl
	{
		/// <summary>
		/// number of slots in an inventory row
		/// </summary>
		public const int INVENTORY_ROW_LENGTH = 14;

		/// <summary>
		/// Area of the grid portion of the inventory (uses absolute coordinates)
		/// </summary>
		public static Rectangle GRID_AREA = new Rectangle(110, 334, 377, 116);

		private readonly bool[,] m_filledSlots = new bool[4, INVENTORY_ROW_LENGTH]; //4 rows, 14 columns = 56 total in grid
		private readonly RegistryKey m_inventoryKey;
		private readonly List<EOInventoryItem> m_childItems = new List<EOInventoryItem>();

		private readonly XNALabel m_lblWeight;
		private readonly XNAButton m_btnDrop, m_btnJunk, m_btnPaperdoll;

		private readonly PacketAPI m_api;
		
		public EOInventory(XNAPanel parent, PacketAPI api)
			: base(null, null, parent)
		{
			m_api = api;

			//load info from registry
			Dictionary<int, int> localItemSlotMap = new Dictionary<int, int>();
			m_inventoryKey = _tryGetCharacterRegKey();
			if (m_inventoryKey != null)
			{
				const string itemFmt = "item{0}";
				for (int i = 0; i < INVENTORY_ROW_LENGTH * 4; ++i)
				{
					int id;
					try
					{
						id = Convert.ToInt32(m_inventoryKey.GetValue(string.Format(itemFmt, i)));
					}
					catch { continue; }
					localItemSlotMap.Add(i, id);
				}
			}

			//add the inventory items that were retrieved from the server
			List<InventoryItem> localInv = World.Instance.MainPlayer.ActiveCharacter.Inventory;
			if (localInv.Find(_item => _item.id == 1).id != 1)
				localInv.Insert(0, new InventoryItem {amount = 0, id = 1}); //add 0 gold if there isn't any gold

			bool dialogShown = false;
			foreach (InventoryItem item in localInv)
			{
				ItemRecord rec = World.Instance.EIF.GetItemRecordByID(item.id);
				int slot = localItemSlotMap.ContainsValue(item.id)
					? localItemSlotMap.First(_pair => _pair.Value == item.id).Key
					: _getNextOpenSlot(rec.Size);

				List<Tuple<int, int>> points;
				if (!_fitsInSlot(slot, rec.Size, out points))
					slot = _getNextOpenSlot(rec.Size);

				if (!_addItemToSlot(slot, rec, item.amount) && !dialogShown)
				{
					dialogShown = true;
					EOMessageBox.Show("Something doesn't fit in the inventory. Rearrange items or get rid of them.", "Warning", XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
				}
			}

			//coordinates for parent of EOInventory: 102, 330 (pnlInventory in InGameHud)
			//extra in photoshop right now: 8, 31

			//current weight label (member variable, needs to have text updated when item amounts change)
			m_lblWeight = new XNALabel(new Rectangle(385, 37, 88, 18), Constants.FontSize08pt5)
			{
				ForeColor = Constants.LightGrayText,
				TextAlign = LabelAlignment.MiddleCenter,
				Visible = true,
				AutoSize = false
			};
			m_lblWeight.SetParent(this);
			UpdateWeightLabel();

			Texture2D thatWeirdSheet = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 27); //oh my gawd the offsets on this bish

			//(local variables, added to child controls)
			//'paperdoll' button
			m_btnPaperdoll = new XNAButton(thatWeirdSheet, new Vector2(385, 9), /*new Rectangle(39, 385, 88, 19)*/null, new Rectangle(126, 385, 88, 19));
			m_btnPaperdoll.SetParent(this);
			m_btnPaperdoll.OnClick += (s, e) => m_api.RequestPaperdoll((short)World.Instance.MainPlayer.ActiveCharacter.ID);
			//'drop' button
			//491, 398 -> 389, 68
			//0,15,38,37
			//0,52,38,37
			m_btnDrop = new XNAButton(thatWeirdSheet, new Vector2(389, 68), new Rectangle(0, 15, 38, 37), new Rectangle(0, 52, 38, 37));
			m_btnDrop.SetParent(this);
			World.IgnoreDialogs(m_btnDrop);
			//'junk' button - 4 + 38 on the x away from drop
			m_btnJunk = new XNAButton(thatWeirdSheet, new Vector2(431, 68), new Rectangle(0, 89, 38, 37), new Rectangle(0, 126, 38, 37));
			m_btnJunk.SetParent(this);
			World.IgnoreDialogs(m_btnJunk);
		}

		//-----------------------------------------------------
		// Overrides / Control Interface
		//-----------------------------------------------------
		public override void Update(GameTime gameTime)
		{
			if (!Visible || !Game.IsActive) return;

			if (IsOverDrop())
			{
				EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_BUTTON, DATCONST2.STATUS_LABEL_INVENTORY_DROP_BUTTON);
			}
			else if (IsOverJunk())
			{
				EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_BUTTON, DATCONST2.STATUS_LABEL_INVENTORY_JUNK_BUTTON);
			}
			else if (m_btnPaperdoll.MouseOver && !m_btnPaperdoll.MouseOverPreviously)
			{
				EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_BUTTON, DATCONST2.STATUS_LABEL_INVENTORY_SHOW_YOUR_PAPERDOLL);
			}

			base.Update(gameTime);
		}

		protected override void Dispose(bool disposing)
		{
			m_inventoryKey.Dispose();
			m_btnPaperdoll.Dispose();
			m_btnJunk.Dispose();
			m_btnDrop.Dispose();
		}

		private bool _addItemToSlot(int slot, ItemRecord item, int count = 1)
		{
			return _addItemToSlot(m_filledSlots, slot, item, count);
		}

		private bool _addItemToSlot(bool[,] filledSlots, int slot, ItemRecord item, int count = 1)
		{
			//this is ADD item - don't allow adding items that have been added already
			if (slot < 0 || m_childItems.Count(_item => _item.Slot == slot) > 0) return false;
			
			List<Tuple<int, int>> points;
			if (!_fitsInSlot(filledSlots, slot, item.Size, out points)) return false;
			points.ForEach(point => filledSlots[point.Item1, point.Item2] = true); //flag that the spaces are taken

			m_inventoryKey.SetValue(string.Format("item{0}", slot), item.ID, RegistryValueKind.String); //update the registry
			m_childItems.Add(new EOInventoryItem(m_api, slot, item, new InventoryItem { amount = count, id = (short)item.ID }, this)); //add the control wrapper for the item
			m_childItems[m_childItems.Count - 1].DrawOrder = (int) ControlDrawLayer.DialogLayer - (2 + slot%INVENTORY_ROW_LENGTH);
			return true;
		}

		public bool ItemFits(short id)
		{
			//it will fit if the inventory already has it.
			if (m_childItems.Find(_i => _i.ItemData.ID == id) != null)
				return true;

			ItemRecord rec = World.Instance.EIF.GetItemRecordByID(id);
			int nextSlot = _getNextOpenSlot(rec.Size);
			List<Tuple<int, int>> dummy;
			return _fitsInSlot(nextSlot, rec.Size, out dummy);
		}

		/// <summary>
		/// Checks if a list of Item IDs will fit in the inventory based on their item record sizes. Does not modify current inventory.
		/// </summary>
		/// <param name="newItems">List of Items to check</param>
		/// <param name="oldItems">Optional list of items to remove from filled slots before checking new IDs (ie items that will be traded)</param>
		/// <returns>True if everything fits, false otherwise.</returns>
		public bool ItemsFit(List<InventoryItem> newItems, List<InventoryItem> oldItems = null)
		{
			bool[,] tempFilledSlots = new bool[4, INVENTORY_ROW_LENGTH];
			for (int row = 0; row < 4; ++row)
			{
				for (int col = 0; col < INVENTORY_ROW_LENGTH; ++col)
				{
					tempFilledSlots[row, col] = m_filledSlots[row, col];
				}
			}

			if(oldItems != null)
				foreach (InventoryItem item in oldItems)
				{
					EOInventoryItem control = m_childItems.Find(_item => _item.ItemData.ID == item.id);
					if (control != null && control.Inventory.amount - item.amount <= 0)
						_unmarkItemSlots(tempFilledSlots, _getTakenSlots(control.Slot, control.ItemData.Size));
				}

			foreach (InventoryItem item in newItems)
			{
				if (oldItems != null && oldItems.FindIndex(_itm => _itm.id == item.id) < 0)
				{
					if (item.id == 1 || m_childItems.Find(_item => _item.ItemData.ID == item.id) != null)
						continue; //already in inventory: skip, since it isn't a new item
				}

				ItemRecord rec = World.Instance.EIF.GetItemRecordByID(item.id);

				int nextSlot = _getNextOpenSlot(tempFilledSlots, rec.Size);
				List<Tuple<int, int>> points;
				if (nextSlot < 0 || !_fitsInSlot(tempFilledSlots, nextSlot, rec.Size, out points))
					return false;

				foreach (var pt in points)
					tempFilledSlots[pt.Item1, pt.Item2] = true;
			}
			return true;
		}

		private void _unmarkItemSlots(bool[,] slots, List<Tuple<int, int>> points)
		{
			points.ForEach(_p => slots[_p.Item1, _p.Item2] = false);
		}

		private void _removeItemFromSlot(int slot, int count = 1)
		{
			EOInventoryItem control = m_childItems.Find(_control => _control.Slot == slot);
			if (control == null || slot < 0) return;

			int numLeft = control.Inventory.amount - count;

			if (numLeft <= 0 && control.Inventory.id != 1)
			{
				ItemSize sz = control.ItemData.Size;
				_unmarkItemSlots(m_filledSlots, _getTakenSlots(control.Slot, sz));

				m_inventoryKey.SetValue(string.Format("item{0}", slot), 0, RegistryValueKind.String);
				m_childItems.Remove(control);
				control.Visible = false;
				control.Close();
			}
			else
			{
				control.Inventory = new InventoryItem {amount = numLeft, id = control.Inventory.id};
				control.UpdateItemLabel();
			}
		}

		public bool MoveItem(EOInventoryItem childItem, int newSlot)
		{
			if (childItem.Slot == newSlot) return true; // We did it, Reddit!

			List<Tuple<int, int>> oldPoints = _getTakenSlots(childItem.Slot, childItem.ItemData.Size);
			List<Tuple<int, int>> points;
			if (!_fitsInSlot(newSlot, childItem.ItemData.Size, out points, oldPoints)) return false;

			oldPoints.ForEach(_p => m_filledSlots[_p.Item1, _p.Item2] = false);
			points.ForEach(_p => m_filledSlots[_p.Item1, _p.Item2] = true);

			m_inventoryKey.SetValue(string.Format("item{0}", childItem.Slot), 0, RegistryValueKind.String);
			m_inventoryKey.SetValue(string.Format("item{0}", newSlot), childItem.ItemData.ID, RegistryValueKind.String);

			childItem.DrawOrder = (int)ControlDrawLayer.DialogLayer - (2 + childItem.Slot % INVENTORY_ROW_LENGTH);

			return true;
		}

		private int _getNextOpenSlot(ItemSize size)
		{
			return _getNextOpenSlot(m_filledSlots, size);
		}

		private int _getNextOpenSlot(bool[,] slots, ItemSize size)
		{
			int width, height;
			_getItemSizeDeltas(size, out width, out height);

			//outer loops: iterating over every grid space (56 spaces total)
			for (int row = 0; row < 4; ++row)
			{
				for (int col = 0; col < INVENTORY_ROW_LENGTH; ++col)
				{
					if (slots[row, col]) continue;

					if (!slots[row, col] && size == ItemSize.Size1x1)
						return row*INVENTORY_ROW_LENGTH + col;

					//inner loops: iterating over grid spaces starting at (row, col) for the item size (width, height)
					bool ok = true;
					for (int y = row; y < row + height; ++y)
					{
						if (y >= 4)
						{
							ok = false;
							continue;
						}
						for (int x = col; x < col + width; ++x)
							if (x >= INVENTORY_ROW_LENGTH || slots[y, x]) ok = false;
					}

					if (ok) return row*INVENTORY_ROW_LENGTH + col;
				}
			}

			return -1;
		}

		public void UpdateWeightLabel(string text = "")
		{
			if (string.IsNullOrEmpty(text))
				m_lblWeight.Text = string.Format("{0} / {1}", World.Instance.MainPlayer.ActiveCharacter.Weight,
					World.Instance.MainPlayer.ActiveCharacter.MaxWeight);
			else
				m_lblWeight.Text = text;
		}

		public bool NoItemsDragging()
		{
			return m_childItems.Count(invItem => invItem.Dragging) == 0;
		}

		public bool UpdateItem(InventoryItem item)
		{
			EOInventoryItem ctrl;
			if((ctrl = m_childItems.Find(_ctrl => _ctrl.ItemData.ID == item.id)) != null)
			{
				ctrl.Inventory = item;
				ctrl.UpdateItemLabel();
			}
			else
			{
				ItemRecord rec = World.Instance.EIF.GetItemRecordByID(item.id);
				return _addItemToSlot(_getNextOpenSlot(rec.Size), rec, item.amount);
			}

			return true;
		}

		public void RemoveItem(int id)
		{
			EOInventoryItem ctrl;
			if ((ctrl = m_childItems.Find(_ctrl => _ctrl.ItemData.ID == id)) != null)
			{
				_removeItemFromSlot(ctrl.Slot, ctrl.Inventory.amount);
			}
		}

		public bool IsOverDrop()
		{
			return m_btnDrop.MouseOver && m_btnDrop.MouseOverPreviously;
		}

		public bool IsOverJunk()
		{
			return m_btnJunk.MouseOver && m_btnJunk.MouseOverPreviously;
		}

		//-----------------------------------------------------
		// Helper methods
		//-----------------------------------------------------
		private static RegistryKey _tryGetCharacterRegKey()
		{	
			try
			{
				using (RegistryKey currentUser = Registry.CurrentUser)
				{
					return currentUser.CreateSubKey(string.Format("Software\\EndlessClient\\{0}\\{1}\\inventory", World.Instance.MainPlayer.AccountName, World.Instance.MainPlayer.ActiveCharacter.Name),
						RegistryKeyPermissionCheck.ReadWriteSubTree);
				}
			}
			catch (NullReferenceException) { }
			return null;
		}

		private static List<Tuple<int, int>> _getTakenSlots(int slot, ItemSize sz)
		{
			var ret = new List<Tuple<int, int>>();

			int width, height;
			_getItemSizeDeltas(sz, out width, out height);
			int y = slot / INVENTORY_ROW_LENGTH, x = slot % INVENTORY_ROW_LENGTH;
			for (int row = y; row < height + y; ++row)
			{
				for (int col = x; col < width + x; ++col)
				{
					ret.Add(new Tuple<int, int>(row, col));
				}
			}

			return ret;
		}

		private bool _fitsInSlot(int slot, ItemSize sz, out List<Tuple<int, int>> points, List<Tuple<int, int>> itemPoints = null)
		{
			return _fitsInSlot(m_filledSlots, slot, sz, out points, itemPoints);
		}

		private bool _fitsInSlot(bool[,] slots, int slot, ItemSize sz, out List<Tuple<int, int>> points, List<Tuple<int, int>> itemPoints = null)
		{
			points = new List<Tuple<int, int>>();

			if (slot < 0 || slot >= 4*INVENTORY_ROW_LENGTH) return false;

			//check the 'filled slots' array to see if the item can go in 'slot' based on its size
			int y = slot / INVENTORY_ROW_LENGTH, x = slot % INVENTORY_ROW_LENGTH;
			if (y >= 4 || x >= INVENTORY_ROW_LENGTH) return false;
			if (itemPoints == null && slots[y, x]) return false;

			points = _getTakenSlots(slot, sz);
			if (points.Count(_t => _t.Item1 < 0 || _t.Item1 > 3 || _t.Item2 < 0 || _t.Item2 >= INVENTORY_ROW_LENGTH) > 0)
				return false; //some of the coordinates are out of bounds of the maximum inventory length

			List<Tuple<int, int>> overLaps = points.FindAll(_pt => slots[_pt.Item1, _pt.Item2]);
			if (overLaps.Count > 0 && (itemPoints == null || overLaps.Count(itemPoints.Contains) != overLaps.Count))
				return false; //more than one overlapping point, and the points in overLaps are not contained in itemPoints

			return true;
		}

		//this is public because C# doesn't have a 'friend' keyword and I need it in EOInventoryItem
		public static void _getItemSizeDeltas(ItemSize size, out int width, out int height)
		{
			//enum ItemSize: Size[width]x[height], 
			//	where [width] is index 4 and [height] is index 6 (string of length 7)
			string sizeStr = Enum.GetName(typeof(ItemSize), size);
			if (sizeStr == null || sizeStr.Length != 7)
			{
				width = height = 0;
				return;
			}

			width = Convert.ToInt32(sizeStr.Substring(4, 1));
			height = Convert.ToInt32(sizeStr.Substring(6, 1));
		}

		public void EnableEffectPotions()
		{
			var effectPotions = m_childItems.Where(x => x.ItemData.Type == ItemType.EffectPotion && !x.Enabled);
			foreach (var potion in effectPotions)
				potion.Enabled = true;
		}

		public void DisableEffectPotions()
		{
			var effectPotions = m_childItems.Where(x => x.ItemData.Type == ItemType.EffectPotion && x.Enabled);
			foreach (var potion in effectPotions)
				potion.Enabled = false;
		}
	}
}
