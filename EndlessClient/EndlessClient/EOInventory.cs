using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
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
		private int m_alpha = 255;
		private int m_preDragDrawOrder;
		private XNAControl m_preDragParent;
		private int m_oldOffX, m_oldOffY;
		public bool Dragging
		{
			get { return m_beingDragged; }
		}

		private int m_recentClickCount;
		private readonly Timer m_recentClickTimer;

		public EOInventoryItem(int slot, ItemRecord itemData, InventoryItem itemInventoryInfo, EOInventory inventory)
			: base(null, null, inventory)
		{
			m_itemData = itemData;
			m_inventory = itemInventoryInfo;
			Slot = slot;

			UpdateItemLocation(Slot);

			m_itemgfx = GFXLoader.TextureFromResource(GFXTypes.Items, 2 * itemData.Graphic, true);

			m_highlightBG = new Texture2D(Game.GraphicsDevice, DrawArea.Width - 3, DrawArea.Height - 3);
			Color[] highlight = new Color[(drawArea.Width - 3) * (drawArea.Height - 3)];
			for (int i = 0; i < highlight.Length; ++i) { highlight[i] = Color.FromNonPremultiplied(200, 200, 200, 60); }
			m_highlightBG.SetData(highlight);
			
			_initItemLabel();

			m_recentClickTimer = new Timer(
				_state => { if (m_recentClickCount > 0) Interlocked.Decrement(ref m_recentClickCount); }, null, 0, 1000);
		}

		public override void Update(GameTime gameTime)
		{
			//check for drag-drop here
			MouseState currentState = Mouse.GetState();

			if (!m_beingDragged && MouseOverPreviously && MouseOver && PreviousMouseState.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Pressed)
			{
				//Conditions for starting are the mouse is over, the button is pressed, and no other items are being dragged
				if (((EOInventory) parent).NoItemsDragging())
				{
					//start the drag operation and hide the item label
					m_beingDragged = true;
					m_nameLabel.Visible = false;
					m_preDragDrawOrder = DrawOrder;
					m_preDragParent = parent;
					
					//make sure the offsets are maintained!
					//required to enable dragging past bounds of the inventory panel
					m_oldOffX = xOff;
					m_oldOffY = yOff;
					SetParent(null);
					
					m_alpha = 128;
					DrawOrder = 100; //arbitrarily large constant so drawn on top while dragging
				}
			}

			if (m_beingDragged && PreviousMouseState.LeftButton == ButtonState.Pressed &&
			    currentState.LeftButton == ButtonState.Pressed)
			{
				//dragging has started. continue dragging until mouse is released, update position based on mouse location
				DrawLocation = new Vector2(currentState.X - (DrawArea.Width/2), currentState.Y - (DrawArea.Height/2));
			}
			else if (m_beingDragged && PreviousMouseState.LeftButton == ButtonState.Pressed &&
			         currentState.LeftButton == ButtonState.Released)
			{
				//need to check for: drop on map (drop action)
				//					 drop on button junk/drop
				//					 drop on grid (inventory move action)
				//					 drop on chest dialog (chest add action)

				DrawOrder = m_preDragDrawOrder;
				m_alpha = 255;
				SetParent(m_preDragParent);

				if (((EOInventory) parent).IsOverDrop() || (World.Instance.ActiveMapRenderer.MouseOver && 
					EOChestDialog.Instance == null && EOPaperdollDialog.Instance == null && EOBankVaultDialog.Instance == null))
				{
					Microsoft.Xna.Framework.Point loc = World.Instance.ActiveMapRenderer.MouseOver ? World.Instance.ActiveMapRenderer.GridCoords:
						new Microsoft.Xna.Framework.Point(World.Instance.MainPlayer.ActiveCharacter.X, World.Instance.MainPlayer.ActiveCharacter.Y);

					//in range if maximum coordinate difference is <= 2 away
					bool inRange = Math.Abs(Math.Max(World.Instance.MainPlayer.ActiveCharacter.X - loc.X, World.Instance.MainPlayer.ActiveCharacter.Y - loc.Y)) <= 2;

					if (m_itemData.Special == ItemSpecial.Lore)
					{
						EODialog.Show("It is not possible to drop or trade this item.", "Lore Item", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					}
					else if (m_inventory.amount > 1 && inRange)
					{
						EOItemTransferDialog dlg = new EOItemTransferDialog(m_itemData.Name, EOItemTransferDialog.TransferType.DropItems,
							m_inventory.amount);
						dlg.DialogClosing += (sender, args) =>
						{
							if (args.Result == XNADialogResult.OK)
								Handlers.Item.DropItem(m_inventory.id, dlg.SelectedAmount, (byte)loc.X, (byte)loc.Y);
						};
					}
					else if(inRange)
					{
						Handlers.Item.DropItem(m_inventory.id, 1, (byte)loc.X, (byte)loc.Y);
					}
				}
				else if (((EOInventory) parent).IsOverJunk())
				{
					if (m_inventory.amount > 1)
					{
						EOItemTransferDialog dlg = new EOItemTransferDialog(m_itemData.Name, EOItemTransferDialog.TransferType.JunkItems,
							m_inventory.amount);
						dlg.DialogClosing += (sender, args) =>
						{
							if (args.Result == XNADialogResult.OK)
								Handlers.Item.JunkItem(m_inventory.id, dlg.SelectedAmount);
						};
					}
					else
					{
						Handlers.Item.JunkItem(m_inventory.id, 1);
					}
				}
				else if (EOChestDialog.Instance != null && EOChestDialog.Instance.MouseOver && EOChestDialog.Instance.MouseOverPreviously)
				{
					if (m_itemData.Special == ItemSpecial.Lore)
					{
						EODialog.Show("It is not possible to drop or trade this item.", "Lore Item", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					}
					else if (m_inventory.amount > 1)
					{
						EOItemTransferDialog dlg = new EOItemTransferDialog(m_itemData.Name, EOItemTransferDialog.TransferType.DropItems, m_inventory.amount);
						dlg.DialogClosing += (sender, args) =>
						{
							if (args.Result == XNADialogResult.OK && !Handlers.Chest.AddItem(m_inventory.id, dlg.SelectedAmount))
								EOGame.Instance.LostConnectionDialog();
						};
					}
					else
					{
						if(!Handlers.Chest.AddItem(m_inventory.id, 1))
							EOGame.Instance.LostConnectionDialog();
					}
				}
				else if (EOPaperdollDialog.Instance != null && EOPaperdollDialog.Instance.MouseOver && EOPaperdollDialog.Instance.MouseOverPreviously)
				{
					//equipable items should be equipped
					//other item types should do nothing
					switch (m_itemData.Type)
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
							_handleDoubleClick();
							break;
					}
				}
				else if (EOBankVaultDialog.Instance != null && EOBankVaultDialog.Instance.MouseOver && EOBankVaultDialog.Instance.MouseOverPreviously)
				{
					if (m_inventory.id == 1)
					{
						//There was a localization error in the original client - it had the italian word for currency, "valuta"
						EODialog.Show("Please deposit currency on your bank account.", "Refused", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					}
					else if (m_inventory.amount > 1)
					{
						EOItemTransferDialog dlg = new EOItemTransferDialog(m_itemData.Name, EOItemTransferDialog.TransferType.ShopTransfer, m_inventory.amount, "transfer");
						dlg.DialogClosing += (sender, args) =>
						{
							if (args.Result == XNADialogResult.OK && !Handlers.Locker.AddItem(m_inventory.id, dlg.SelectedAmount))
								EOGame.Instance.LostConnectionDialog();
						};
					}
					else
					{
						if (!Handlers.Locker.AddItem(m_inventory.id, 1))
						{
							EOGame.Instance.LostConnectionDialog();
							return;
						}
					}
				}

				//update the location - if it isn't on the grid, the bounds check will set it back to where it used to be originally
				//Item amount will be updated or item will be removed in packet response to the drop operation
				UpdateItemLocation(ItemCurrentSlot());

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
			else if (!MouseOver && !m_beingDragged && m_nameLabel != null && m_nameLabel.Visible)
			{
				m_nameLabel.Visible = false;
			}

			base.Update(gameTime); //sets mouseoverpreviously = mouseover, among other things
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible) return;

			SpriteBatch.Begin();
			if (MouseOver)
			{
				int currentSlot = ItemCurrentSlot();
				Vector2 drawLoc = m_beingDragged
					? new Vector2(m_oldOffX + 13 + 26*(currentSlot%EOInventory.INVENTORY_ROW_LENGTH),
						m_oldOffY + 9 + 26*(currentSlot/EOInventory.INVENTORY_ROW_LENGTH)) //recalculate the top-left point for the highlight based on the current drag position
					: new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y);

				if (EOInventory.GRID_AREA.Contains(new Rectangle((int) drawLoc.X, (int) drawLoc.Y, DrawArea.Width, DrawArea.Height)))
					SpriteBatch.Draw(m_highlightBG, drawLoc, Color.White);
			}
			if(m_itemgfx != null)
				SpriteBatch.Draw(m_itemgfx, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), Color.FromNonPremultiplied(255, 255, 255, m_alpha));
			SpriteBatch.End();
			base.Draw(gameTime);
		}

		private void UpdateItemLocation(int newSlot)
		{
			if (Slot != newSlot && ((EOInventory) parent).MoveItem(this, newSlot)) Slot = newSlot;

			//top-left grid slot in the inventory is 115, 339
			//parent top-left is 103, 330
			//grid size is 26*26 (w/o borders 23*23)
			int width, height;
			EOInventory._getItemSizeDeltas(m_itemData.Size, out width, out height);
			DrawLocation = new Vector2(13 + 26 * (Slot % EOInventory.INVENTORY_ROW_LENGTH), 9 + 26 * (Slot / EOInventory.INVENTORY_ROW_LENGTH));
			_setSize(width * 26, height * 26);

			if (m_nameLabel != null) //fix the position of the name label too if we aren't creating the inventoryitem
			{
				m_nameLabel.DrawLocation = new Vector2(DrawArea.Width, 0);
				if(!EOInventory.GRID_AREA.Contains(m_nameLabel.DrawAreaWithOffset))
					m_nameLabel.DrawLocation = new Vector2(-m_nameLabel.DrawArea.Width, 0); //show on the right if it isn't in bounds!
				m_nameLabel.ResizeBasedOnText(16, 9);
			}
		}

		private int ItemCurrentSlot()
		{
			if (!m_beingDragged) return Slot;

			//convert the current draw area to a slot number (for when the item is dragged)
			return (int)((DrawLocation.X - m_oldOffX - 13)/26) + EOInventory.INVENTORY_ROW_LENGTH * (int)((DrawLocation.Y - m_oldOffY - 9)/26);
		}

		public void UpdateItemLabel()
		{
			m_nameLabel.Text = GetNameString(m_inventory.id, m_inventory.amount);
		}

		protected override void Dispose(bool disposing)
		{
			if(m_recentClickTimer != null) m_recentClickTimer.Dispose();
			if(m_nameLabel != null) m_nameLabel.Dispose();
			if(m_highlightBG != null) m_highlightBG.Dispose();

			base.Dispose(disposing);
		}

		private void _initItemLabel()
		{
			if (m_nameLabel != null) m_nameLabel.Dispose();

			m_nameLabel = new XNALabel(new Rectangle(DrawArea.Width, 0, 150, 23), "Microsoft Sans MS", 8f)
			{
				Visible = false,
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleCenter,
				ForeColor = System.Drawing.Color.FromArgb(255, 200, 200, 200),
				BackColor = System.Drawing.Color.FromArgb(160, 30, 30, 30)
			};
			
			UpdateItemLabel();

			m_nameLabel.ForeColor = GetItemTextColor((short)m_itemData.ID);

			m_nameLabel.SetParent(this);
			m_nameLabel.ResizeBasedOnText(16, 9);
		}

		private void _handleDoubleClick()
		{
			switch (m_itemData.Type)
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
					byte subLoc = 0;
					if (m_itemData.Type == ItemType.Armlet || m_itemData.Type == ItemType.Ring || m_itemData.Type == ItemType.Bracer)
					{
						if (World.Instance.MainPlayer.ActiveCharacter.PaperDoll[(int) m_itemData.GetEquipLocation()] == 0)
							subLoc = 0;
						else if (World.Instance.MainPlayer.ActiveCharacter.PaperDoll[(int) m_itemData.GetEquipLocation() + 1] == 0)
							subLoc = 1;
						else
						{
							EOGame.Instance.Hud.SetStatusLabel("[ Information ] You already have an item of this type equipped.");
							break;
						}
					}

					if (World.Instance.MainPlayer.ActiveCharacter.EquipItem(m_itemData.Type, (short)m_itemData.ID, (short)m_itemData.DollGraphic))
						Handlers.Paperdoll.EquipItem((short)m_itemData.ID, subLoc);
					else
						EOGame.Instance.Hud.SetStatusLabel("[ Information ] You already have an item of this type equipped.");
					break;
				case ItemType.Beer:
					break;
				case ItemType.CureCurse:
					break;
				case ItemType.EXPReward:
					break;
				case ItemType.EffectPotion:
					break;
				case ItemType.HairDye:
					break;
				case ItemType.Heal:
					break;
				case ItemType.SkillReward:
					break;
				case ItemType.StatReward:
					break;
				case ItemType.Teleport:
					break;
			}

			m_recentClickCount = 0;
		}

		public static System.Drawing.Color GetItemTextColor(short id) //also used in map renderer for mapitems
		{
			ItemRecord data = World.Instance.EIF.GetItemRecordByID(id);
			switch (data.Special)
			{
				case ItemSpecial.Lore:
				case ItemSpecial.Unique:
					return System.Drawing.Color.FromArgb(0xff, 0xff, 0xf0, 0xa5);
				case ItemSpecial.Rare:
					return System.Drawing.Color.FromArgb(0xff, 0xf5, 0xc8, 0x9c);
			}

			return System.Drawing.Color.White;
		}

		public static string GetNameString(short id, int amount)
		{
			ItemRecord data = World.Instance.EIF.GetItemRecordByID(id);
			switch (data.ID)
			{
				case 1:
					return string.Format("{0} {1}", amount, data.Name);
				default:
					if (amount == 1)
						return data.Name;
					if (amount > 1)
						return string.Format("{0} x{1}", data.Name, amount);
					throw new Exception("There shouldn't be an item in the inventory with amount zero");
			}
		}
	}

	public class EOInventory : XNAControl
	{
		/// <summary>
		/// number of slots in an inventory row
		/// </summary>
		public const int INVENTORY_ROW_LENGTH = 14;

		/// <summary>
		/// Area of the grid portion of the inventory (uses absolute coordinates)
		/// </summary>
		public static Rectangle GRID_AREA = new Rectangle(115, 339, 367, 106);

		private readonly bool[,] m_filledSlots = new bool[4, INVENTORY_ROW_LENGTH]; //4 rows, 14 columns = 56 total in grid
		private readonly RegistryKey m_inventoryKey;
		private readonly List<EOInventoryItem> m_childItems = new List<EOInventoryItem>();

		private readonly XNALabel m_lblWeight;
		private readonly XNAButton m_btnDrop, m_btnJunk, m_btnPaperdoll;
		
		public EOInventory(XNAPanel parent)
			: base(null, null, parent)
		{
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
					: GetNextOpenSlot(rec.Size);
				if (!dialogShown && !AddItemToSlot(slot, rec, item.amount))
				{
					dialogShown = true;
					EODialog.Show("Something doesn't fit in the inventory. Rearrange items or get rid of them.", "Warning", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				}
			}

			//coordinates for parent of EOInventory: 102, 330 (pnlInventory in InGameHud)
			//extra in photoshop right now: 8, 31

			//current weight label (member variable, needs to have text updated when item amounts change)
			m_lblWeight = new XNALabel(new Rectangle(385, 37, 88, 18), "Microsoft Sans MS", 8f)
			{
				ForeColor = System.Drawing.Color.FromArgb(255, 0xc8, 0xc8, 0xc8),
				TextAlign = ContentAlignment.MiddleCenter,
				Visible = true,
				AutoSize = false
			};
			m_lblWeight.SetParent(this);
			UpdateWeightLabel();

			Texture2D thatWeirdSheet = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 27); //oh my gawd the offsets on this bish

			//(local variables, added to child controls)
			//'paperdoll' button
			m_btnPaperdoll = new XNAButton(thatWeirdSheet, new Vector2(385, 9), /*new Rectangle(39, 385, 88, 19)*/null, new Rectangle(126, 385, 88, 19));
			m_btnPaperdoll.SetParent(this);
			m_btnPaperdoll.OnClick += (s, e) => Handlers.Paperdoll.RequestPaperdoll((short)World.Instance.MainPlayer.ActiveCharacter.ID);
			//'drop' button
			//491, 398 -> 389, 68
			//0,15,38,37
			//0,52,38,37
			m_btnDrop = new XNAButton(thatWeirdSheet, new Vector2(389, 68), new Rectangle(0, 15, 38, 37), new Rectangle(0, 52, 38, 37));
			m_btnDrop.SetParent(this);
			m_btnDrop.IgnoreDialog(typeof(EOPaperdollDialog));
			m_btnDrop.IgnoreDialog(typeof(EOChestDialog));
			//'junk' button - 4 + 38 on the x away from drop
			m_btnJunk = new XNAButton(thatWeirdSheet, new Vector2(431, 68), new Rectangle(0, 89, 38, 37), new Rectangle(0, 126, 38, 37));
			m_btnJunk.SetParent(this);
			m_btnJunk.IgnoreDialog(typeof(EOPaperdollDialog));
			m_btnJunk.IgnoreDialog(typeof(EOChestDialog));
		}

		//-----------------------------------------------------
		// Overrides / Control Interface
		//-----------------------------------------------------
		public override void Update(GameTime gameTime)
		{
			if (IsOverDrop())
			{
				EOGame.Instance.Hud.SetStatusLabel("[ Button ] Drag an item to this button to drop it on the ground.");
			}
			else if (IsOverJunk())
			{
				EOGame.Instance.Hud.SetStatusLabel("[ Button ] Drag an item to this button to destroy it forever.");
			}
			else if (m_btnPaperdoll.MouseOver && !m_btnPaperdoll.MouseOverPreviously)
			{
				EOGame.Instance.Hud.SetStatusLabel("[ Button ] Click here to show your paperdoll.");
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

		private bool AddItemToSlot(int slot, ItemRecord item, int count = 1)
		{
			//this is ADD item - don't allow adding items that have been added already
			if (slot < 0 || m_childItems.Count(_item => _item.Slot == slot) > 0) return false;
			
			List<Tuple<int, int>> points;
			if (!_fitsInSlot(slot, item.Size, out points)) return false;
			points.ForEach(point => m_filledSlots[point.Item1, point.Item2] = true); //flag that the spaces are taken

			m_inventoryKey.SetValue(string.Format("item{0}", slot), item.ID, RegistryValueKind.String); //update the registry
			m_childItems.Add(new EOInventoryItem(slot, item, new InventoryItem { amount = count, id = (short)item.ID }, this)); //add the control wrapper for the item
			m_childItems[m_childItems.Count - 1].DrawOrder = (int) ControlDrawLayer.DialogLayer - (2 + slot%INVENTORY_ROW_LENGTH);
			return true;
		}

		public bool ItemFits(short id)
		{
			//it will fit if the inventory already has it.
			if (m_childItems.Find(_i => _i.ItemData.ID == id) != null)
				return true;

			ItemRecord rec = World.Instance.EIF.GetItemRecordByID(id);
			int nextSlot = GetNextOpenSlot(rec.Size);
			List<Tuple<int, int>> dummy;
			return _fitsInSlot(nextSlot, rec.Size, out dummy);
		}

		private void RemoveItemFromSlot(int slot, int count = 1)
		{
			EOInventoryItem control = m_childItems.Find(_control => _control.Slot == slot);
			if (control == null || slot < 0) return;

			int numLeft = control.Inventory.amount - count;

			if (numLeft <= 0)
			{
				ItemSize sz = control.ItemData.Size;
				List<Tuple<int, int>> points = _getTakenSlots(control.Slot, sz);
				points.ForEach(_p => m_filledSlots[_p.Item1, _p.Item2] = false);

				m_inventoryKey.SetValue(string.Format("item{0}", slot), 0, RegistryValueKind.String);
				m_childItems.Remove(control);
				control.Visible = false;
				control.Close();
			}
			else
				control.Inventory = new InventoryItem {amount = numLeft, id = control.Inventory.id};
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

		private int GetNextOpenSlot(ItemSize size)
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
				return AddItemToSlot(GetNextOpenSlot(rec.Size), rec, item.amount);
			}

			return true;
		}

		public void RemoveItem(int id)
		{
			EOInventoryItem ctrl;
			if ((ctrl = m_childItems.Find(_ctrl => _ctrl.ItemData.ID == id)) != null)
			{
				RemoveItemFromSlot(ctrl.Slot, ctrl.Inventory.amount);
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

		private List<Tuple<int, int>> _getTakenSlots(int slot, ItemSize sz)
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

		/// <summary>
		/// Returns whether or not a slot can support an item of the specified size
		/// </summary>
		/// <param name="slot">The slot to check</param>
		/// <param name="sz">The size of the item we're trying to fit</param>
		/// <param name="points">List of coordinates that the new item will take</param>
		/// <param name="itemPoints">List of coordinates of the item that is moving</param>
		/// <returns></returns>
		private bool _fitsInSlot(int slot, ItemSize sz, out List<Tuple<int, int>> points, List<Tuple<int, int>> itemPoints = null)
		{
			points = new List<Tuple<int, int>>();

			if (slot < 0 || slot >= 4*INVENTORY_ROW_LENGTH) return false;

			//check the 'filled slots' array to see if the item can go in 'slot' based on its size
			int y = slot / INVENTORY_ROW_LENGTH, x = slot % INVENTORY_ROW_LENGTH;
			if (y >= 4 || x >= INVENTORY_ROW_LENGTH) return false;
			if (itemPoints == null && m_filledSlots[y, x]) return false;

			points = _getTakenSlots(slot, sz);
			if (points.Count(_t => _t.Item1 < 0 || _t.Item1 > 3 || _t.Item2 < 0 || _t.Item2 >= INVENTORY_ROW_LENGTH) > 0)
				return false; //some of the coordinates are out of bounds of the maximum inventory length

			List<Tuple<int,int>> overLaps = points.FindAll(_pt => m_filledSlots[_pt.Item1, _pt.Item2]);
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
	}
}
