using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using EndlessClient.Handlers;
using EOLib.Data;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;

namespace EndlessClient
{
	//This is going to be a single item in the inventory. will handle it's own drag/drop, onmouseover, etc.
	public class EOInventoryItem : XNAControl
	{
		private readonly ItemRecord m_itemData;
		public ItemRecord ItemData
		{
			get { return m_itemData; }
		}

		private InventoryItem m_inventory;
		public InventoryItem Inventory { get { return m_inventory; } set { m_inventory = value; } }

		public int Slot { get; private set; }

		private readonly Texture2D m_itemgfx, m_highlightBG;
		private XNALabel m_nameLabel;

		private bool m_beingDragged;
		public bool Dragging
		{
			get { return m_beingDragged; }
		}

		private int m_recentClickCount;
		private readonly Timer m_recentClickTimer;

		public EOInventoryItem(Game g, int slot, ItemRecord itemData, InventoryItem itemInventoryInfo, EOInventory inventory)
			: base(g, null, null, inventory)
		{
			m_itemData = itemData;
			m_inventory = itemInventoryInfo;
			Slot = slot;

			UpdateItemLocation(Slot);

			m_itemgfx = GFXLoader.TextureFromResource(GFXTypes.Items, 2 * itemData.Graphic, true);

			m_highlightBG = new Texture2D(g.GraphicsDevice, drawArea.Width - 3, drawArea.Height - 3);
			Color[] highlight = new Color[(drawArea.Width - 3) * (drawArea.Height - 3)];
			for (int i = 0; i < highlight.Length; ++i) { highlight[i] = Color.FromNonPremultiplied(200, 200, 200, 60); }
			m_highlightBG.SetData(highlight);
			
			UpdateItemLabel(m_inventory);

			m_recentClickTimer = new Timer(
				_state => { if (m_recentClickCount > 0) Interlocked.Decrement(ref m_recentClickCount); }, null, 0, 500);
		}

		public override void Update(GameTime gameTime)
		{
			//check for drag-drop here
			MouseState currentState = Mouse.GetState();

			if (MouseOverPreviously && MouseOver && PreviousMouseState.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Pressed)
			{
				//Conditions for starting are the mouse is over, the button is pressed, and no other items are being dragged
				if (((EOInventory) parent).NoItemsDragging())
				{
					//start the drag operation and hide the item label
					m_beingDragged = true;
					m_nameLabel.Visible = false;
				}
			}

			if (m_beingDragged && PreviousMouseState.LeftButton == ButtonState.Pressed &&
			    currentState.LeftButton == ButtonState.Pressed)
			{
				//dragging has started. continue dragging until mouse is released, update position based on mouse location
				DrawLocation = new Vector2(DrawLocation.X + (currentState.X - PreviousMouseState.X), DrawLocation.Y + (currentState.Y - PreviousMouseState.Y));
			}
			else if (m_beingDragged && PreviousMouseState.LeftButton == ButtonState.Pressed &&
			         currentState.LeftButton == ButtonState.Released)
			{
				//need to check for: drop on map (drop action)
				//					 drop on button junk/drop
				//					 drop on grid (inventory move action)
				if (true) //todo: how to check if on grid?
				{
					UpdateItemLocation(ItemCurrentSlot());
				}

				//mouse has been released. finish dragging.
				m_beingDragged = false;
				m_nameLabel.Visible = true;
			}

			if (!m_beingDragged && PreviousMouseState.LeftButton == ButtonState.Pressed &&
			    currentState.LeftButton == ButtonState.Released && MouseOver && MouseOverPreviously)
			{
				Interlocked.Increment(ref m_recentClickCount);
				if (m_recentClickCount == 2)
				{
					_handleDoubleClick();
				}
			}

			if (!MouseOverPreviously && MouseOver && !m_beingDragged)
			{
				m_nameLabel.Visible = true;
				EOGame.Instance.Hud.SetStatusLabel("[ Item ] " + m_nameLabel.Text);
			}
			else if (MouseOverPreviously && !MouseOver && !m_beingDragged)
			{
				m_nameLabel.Visible = false;
			}

			base.Update(gameTime); //sets mouseoverpreviously = mouseover, among other things
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			SpriteBatch.Begin();
			if (MouseOver)
			{
				int currentSlot = ItemCurrentSlot();
				Vector2 drawLoc = m_beingDragged
					? new Vector2(xOff + 13 + 26*(currentSlot%EOInventory.INVENTORY_ROW_LENGTH),
						yOff + 9 + 26*(currentSlot/EOInventory.INVENTORY_ROW_LENGTH)) //recalculate the top-left point for the highlight based on the current drag position
					: new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y);
				SpriteBatch.Draw(m_highlightBG, drawLoc, Color.White);
			}
			SpriteBatch.Draw(m_itemgfx, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), Color.White);
			SpriteBatch.End();
		}

		public void UpdateItemLabel(InventoryItem newInventory)
		{
			if (newInventory.id != m_inventory.id) return;
			m_inventory = newInventory;

			if (m_nameLabel != null) m_nameLabel.Dispose();

			m_nameLabel = new XNALabel(Game, new Rectangle((int)DrawLocation.X + DrawArea.Width, (int)DrawLocation.Y, 150, 23), "Microsoft Sans MS", 8f)
			{
				Visible = false,
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleCenter,
				ForeColor = System.Drawing.Color.FromArgb(255, 200, 200, 200),
				BackColor = System.Drawing.Color.FromArgb(160, 30, 30, 30)
			};

			switch (m_itemData.ID)
			{
				case 1: m_nameLabel.Text = string.Format("{0} {1}", m_inventory.amount, m_itemData.Name); break;
				default:
					if (m_inventory.amount == 1)
						m_nameLabel.Text = m_itemData.Name;
					else if (m_inventory.amount > 1)
						m_nameLabel.Text = string.Format("{0} x{1}", m_itemData.Name, m_inventory.amount);
					else
						throw new Exception("There shouldn't be an item in the inventory with amount zero");
					break;
			}

			switch (m_itemData.Special)
			{
				case ItemSpecial.Lore:
					m_nameLabel.ForeColor = System.Drawing.Color.FromArgb(0xff, 0xff, 0xf0, 0xa5);
					break;
				//other special types have different forecolors (rare items?)
			}

			m_nameLabel.SetParent(this);
			m_nameLabel.ResizeBasedOnText(16, 9);
		}

		public void UpdateItemLocation(int newSlot)
		{
			int oldSlot = Slot;
			Slot = newSlot;
			if (!((EOInventory) parent).SaveItemLocation(this, oldSlot))
			{
				Slot = oldSlot;
			}

			//top-left grid slot in the inventory is 115, 339
			//parent top-left is 103, 330
			//grid size is 26*26 (w/o borders 23*23)
			int width, height;
			EOInventory._getItemSizeDeltas(m_itemData.Size, out width, out height);
			drawArea = new Rectangle(13 + 26 * (Slot % EOInventory.INVENTORY_ROW_LENGTH),
				9 + 26 * (Slot / EOInventory.INVENTORY_ROW_LENGTH), width * 26, height * 26);
		}

		public int ItemCurrentSlot()
		{
			if (!m_beingDragged) return Slot;

			//convert the current draw area to a slot number (for when the item is dragged)
			return (int)((DrawLocation.X - 13)/26) + EOInventory.INVENTORY_ROW_LENGTH * (int)((DrawLocation.Y - 9)/26);
		}

		protected override void Dispose(bool disposing)
		{
			if(m_recentClickTimer != null) m_recentClickTimer.Dispose();
			if(m_nameLabel != null) m_nameLabel.Dispose();
			if(m_highlightBG != null) m_highlightBG.Dispose();

			base.Dispose(disposing);
		}

		private void _handleDoubleClick()
		{
			string whichAction = "";
			//double-click!
			switch (m_itemData.Type) //different types of items do different things when acted on
			{
				case ItemType.Accessory:
				case ItemType.Armlet:
				case ItemType.Armor:
				case ItemType.Belt:
				case ItemType.Boots:
				case ItemType.Bracer:
				case ItemType.Gloves:
				case ItemType.Hat:
				case ItemType.Necklace:
				case ItemType.Ring:
				case ItemType.Shield:
				case ItemType.Weapon:
					Paperdoll.EquipItem((short)m_itemData.ID);
					break;
				case ItemType.Beer:
					whichAction = "Got hella drunk on";
					break;
				case ItemType.CureCurse:
					whichAction = "Cured curse using";
					break;
				case ItemType.EXPReward:
					whichAction = "Experience reward from ";
					break;
				case ItemType.EffectPotion:
					whichAction = "Effect potion: ";
					break;
				case ItemType.HairDye:
					whichAction = "Dyed hair with";
					break;
				case ItemType.Heal:
					whichAction = "Restored health with";
					break;
				case ItemType.SkillReward:
					whichAction = "Skill reward with";
					break;
				case ItemType.StatReward:
					whichAction = "Stat reward with";
					break;
				case ItemType.Teleport:
					whichAction = "Preparing to teleport using";
					break;
			}

			if (whichAction != "")
			{
				EODialog tst = new EODialog(Game, whichAction + " item " + m_itemData.Name, "Equip action");

				if (false)
				{
					//todo: implement the 'use' action for item types
				}
			}

			m_recentClickCount = 0;
		}
	}

	public class EOInventory : XNAControl
	{
		public const int INVENTORY_ROW_LENGTH = 14;

		private readonly Dictionary<int, ItemRecord> m_regData = new Dictionary<int, ItemRecord>();
		private readonly bool[,] m_filledSlots = new bool[4, INVENTORY_ROW_LENGTH]; //4 rows, 14 columns = 56 total in grid
		private readonly RegistryKey m_inventoryKey;
		private readonly List<EOInventoryItem> m_childItems = new List<EOInventoryItem>();

		private readonly XNALabel m_lblWeight;

		public EOInventory(Game g)
			: base(g)
		{
			//load info from registry
			Dictionary<int, int> localItemSlotMap = new Dictionary<int, int>();
			m_inventoryKey = _tryGetCharacterRegKey();
			if (m_inventoryKey != null)
			{
				const string itemFmt = "item{0}";
				for (int i = 0; i < 56; ++i)
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

			foreach (InventoryItem item in localInv)
			{
				ItemRecord rec = World.Instance.EIF.GetItemRecordByID(item.id);
				int slot = localItemSlotMap.ContainsValue(item.id)
					? localItemSlotMap.First(_pair => _pair.Value == item.id).Key
					: GetNextOpenSlot(rec.Size);
				if (!AddItemToSlot(slot, rec, item.amount)) throw new Exception("Too many items in inventory! (they don't fit)");
			}

			//coordinates for parent of EOInventory: 102, 330 (pnlInventory in InGameHud)
			//extra in photoshop right now: 8, 31

			//current weight label (member variable, needs to have text updated when item amounts change)
			m_lblWeight = new XNALabel(g, new Rectangle(385, 37, 88, 18), "Microsoft Sans MS", 8f)
			{
				ForeColor = System.Drawing.Color.FromArgb(255, 0xc8, 0xc8, 0xc8),
				TextAlign = ContentAlignment.MiddleCenter,
				Visible = true,
				AutoSize = false
			};
			m_lblWeight.SetParent(this);
			UpdateWeightLabel();

			Texture2D thatWeirdSheet = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 27, true); //oh my gawd the offsets on this bish

			//(local variables, added to child controls)
			//'paperdoll' button
			XNAButton paperdoll = new XNAButton(g, thatWeirdSheet, new Vector2(385, 9), /*new Rectangle(39, 385, 88, 19)*/null, new Rectangle(126, 385, 88, 19));
			paperdoll.SetParent(this);
			paperdoll.OnClick += (s, e) => { }; //todo: make event handler that shows a paperdoll dialog
			//'drop' button
			//'junk' button
		}

		//-----------------------------------------------------
		// Overrides / Control Interface
		//-----------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			m_inventoryKey.Dispose();
		}

		//-----------------------------------------------------
		// Public Access methods
		//-----------------------------------------------------
		public bool AddItemToSlot(int slot, ItemRecord item, int count = 1)
		{
			// if the item is being moved, it is already in the inventory and must be cleared out/re-saved in new location
			if (m_regData.ContainsValue(item))
			{
				RemoveItemFromSlot(m_regData.First(_x => _x.Value == item).Key);
			}

			if (slot < 0) return false;

			//check the 'filled slots' array to see if the item can go in 'slot' based on its size
			int y = slot / INVENTORY_ROW_LENGTH, x = slot % INVENTORY_ROW_LENGTH;
			if (m_filledSlots[y, x]) return false;

			int width, height;
			_getItemSizeDeltas(item.Size, out width, out height);

			List<Tuple<int, int>> points = new List<Tuple<int, int>>();
			for (int row = y; row < height + y; ++row)
			{
				for (int col = x; col < width + x; ++col)
				{
					if (row >= 4 || col >= INVENTORY_ROW_LENGTH || m_filledSlots[row, col]) return false;
					points.Add(new Tuple<int, int>(row, col));
				}
			}
			points.ForEach(point => m_filledSlots[point.Item1, point.Item2] = true); //flag that the spaces are taken

			m_regData.Add(slot, item); //add data mapping
			m_inventoryKey.SetValue(string.Format("item{0}", slot), item.ID, RegistryValueKind.String); //update the registry
			m_childItems.Add(new EOInventoryItem(Game, slot, item, new InventoryItem {amount = count, id = (short) item.ID}, this)); //add the control wrapper for the item
			return true;
		}

		public void RemoveItemFromSlot(int slot, int count = 1)
		{
			if (!m_regData.ContainsKey(slot) || slot < 0) return;

			ItemSize sz = m_regData[slot].Size;

			int width, height;
			_getItemSizeDeltas(sz, out width, out height);

			for (int row = slot / INVENTORY_ROW_LENGTH; row < height + (slot / INVENTORY_ROW_LENGTH); ++row)
			{
				for (int col = slot/4; col < width + (slot/4); ++col)
				{
					m_filledSlots[row, col] = false;
				}
			}

			m_regData.Remove(slot);
			m_inventoryKey.SetValue(string.Format("item{0}", slot), 0, RegistryValueKind.String);
			m_childItems.RemoveAll(_control => _control.Slot == slot);
		}

		public int GetNextOpenSlot(ItemSize size)
		{
			int width, height;
			_getItemSizeDeltas(size, out width, out height);

			//outer loops: iterating over every grid space (56 spaces total)
			for (int row = 0; row < 4; ++row)
			{
				for (int col = 0; col < INVENTORY_ROW_LENGTH; ++col)
				{
					if (m_filledSlots[row, col]) continue;

					if (!m_filledSlots[row, col] && size == ItemSize.Size1x1)
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
							if (x >= INVENTORY_ROW_LENGTH || m_filledSlots[y, x]) ok = false;
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

		public bool SaveItemLocation(EOInventoryItem item, int oldSlot)
		{
			if (item.Slot == oldSlot) return true;

			if (m_regData.ContainsKey(item.Slot) || 
				m_filledSlots[item.Slot/INVENTORY_ROW_LENGTH, item.Slot%INVENTORY_ROW_LENGTH]) return false;

			m_inventoryKey.SetValue("item" + oldSlot, 0, RegistryValueKind.String);
			m_inventoryKey.SetValue(string.Format("item{0}", item.Slot), item.ItemData.ID, RegistryValueKind.String);
			m_regData.Remove(oldSlot);
			m_regData.Add(item.Slot, item.ItemData);

			return true;
		}

		public void UpdateItem(InventoryItem item)
		{
			EOInventoryItem ctrl;
			if((ctrl = m_childItems.Find(_ctrl => _ctrl.ItemData.ID == item.id)) != null)
			{
				ctrl.Inventory = item;
			}
		}

		public void RemoveItem(int id)
		{
			EOInventoryItem ctrl;
			if ((ctrl = m_childItems.Find(_ctrl => _ctrl.ItemData.ID == id)) != null)
			{
				RemoveItemFromSlot(ctrl.Slot);
			}
		}

		//-----------------------------------------------------
		// Helper methods
		//-----------------------------------------------------
// ReSharper disable PossibleNullReferenceException
		private static RegistryKey _tryGetCharacterRegKey()
		{	
			try
			{
				using (RegistryKey currentUser = Registry.CurrentUser)
				{
					using (RegistryKey software = currentUser.OpenSubKey("Software", true))
					{
						using (RegistryKey client = software.OpenSubKey("EndlessClient", true) ??
						                            software.CreateSubKey("EndlessClient", RegistryKeyPermissionCheck.ReadWriteSubTree))
						{
							using (RegistryKey eoAccount = client.OpenSubKey(World.Instance.MainPlayer.AccountName, true) ??
							                               client.CreateSubKey(World.Instance.MainPlayer.AccountName,
								                               RegistryKeyPermissionCheck.ReadWriteSubTree))
							{
								using (RegistryKey character = eoAccount.OpenSubKey(World.Instance.MainPlayer.ActiveCharacter.Name, true) ??
								       eoAccount.CreateSubKey(World.Instance.MainPlayer.ActiveCharacter.Name,
									       RegistryKeyPermissionCheck.ReadWriteSubTree))
								{
									return character.OpenSubKey("inventory", true) ??
									       character.CreateSubKey("inventory", RegistryKeyPermissionCheck.ReadWriteSubTree);
								}
							}
						}
					}
				}
			}
			catch (NullReferenceException) { }
			return null;
		}
// ReSharper restore PossibleNullReferenceException

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
	}
}
