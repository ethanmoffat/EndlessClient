// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EndlessClient.Rendering;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.Net;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class ShopDialog : ScrollingListDialog
	{
		/* Process for this:
		 * 1. Click shopkeeper, calls Show()
		 * 2. Show constructs instance and sends packet
		 * 3. When packet response is received, data is populated in dialog
		 * 4. Dialog 'closing' event resets Instance to null
		 */

		/* STATIC INTERFACE */
		public static ShopDialog Instance { get; private set; }

		public static void Show(PacketAPI api, NPCRenderer shopKeeper)
		{
			if (Instance != null)
				return;

			Instance = new ShopDialog(api, shopKeeper.Data.ID);

			//request from server is based on the map index
			if (!api.RequestShop(shopKeeper.Index))
			{
				Instance.Close();
				Instance = null;
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			}
		}

		private enum ShopState
		{
			None,
			Initial,
			Buying,
			Selling,
			Crafting
		}

		public int ID { get; private set; }

		private ShopState m_state;
		private List<ShopItem> m_tradeItems;
		private List<CraftItem> m_craftItems;

		private static Texture2D BuyIcon, SellIcon, CraftIcon;

		private ShopDialog(PacketAPI api, int id)
			: base(api)
		{
			Buttons = ScrollingListDialogButtons.Cancel;
			ListItemType = ListDialogItem.ListItemStyle.Large;

			ID = id;
			DialogClosing += (o, e) =>
			{
				if (e.Result == XNADialogResult.Cancel)
				{
					Instance = null;
					ID = 0;
				}
				else if (e.Result == XNADialogResult.Back)
				{
					e.CancelClose = true;
					_setState(ShopState.Initial);
				}
			};
			m_state = ShopState.None;

			//note - may need to lock around these.
			//other note - no good way to dispose static textures like this
			if (BuyIcon == null || SellIcon == null || CraftIcon == null)
			{
				BuyIcon = _getDlgIcon(ListIcon.Buy);
				SellIcon = _getDlgIcon(ListIcon.Sell);
				CraftIcon = _getDlgIcon(ListIcon.Craft);
			}
		}

		public void SetShopData(int id, string Name, List<ShopItem> tradeItems, List<CraftItem> craftItems)
		{
			if (Instance == null || this != Instance || ID != id) return;
			Title = Name;

			m_tradeItems = tradeItems;
			m_craftItems = craftItems;

			_setState(ShopState.Initial);
		}

		private void _setState(ShopState newState)
		{
			ShopState old = m_state;

			if (old == newState) return;

			int buyNumInt = m_tradeItems.FindAll(x => x.Buy > 0).Count;
			int sellNumInt = m_tradeItems.FindAll(x => World.Instance.MainPlayer.ActiveCharacter.Inventory.FindIndex(item => item.id == x.ID) >= 0 && x.Sell > 0).Count;

			if (newState == ShopState.Buying && buyNumInt <= 0)
			{
				EOMessageBox.Show(DATCONST1.SHOP_NOTHING_IS_FOR_SALE, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
				return;
			}

			if (newState == ShopState.Selling && sellNumInt <= 0)
			{
				EOMessageBox.Show(DATCONST1.SHOP_NOT_BUYING_YOUR_ITEMS, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
				return;
			}

			ClearItemList();
			switch (newState)
			{
				case ShopState.Initial:
					{
						string buyNum = string.Format("{0} {1}", m_tradeItems.FindAll(x => x.Buy > 0).Count, World.GetString(DATCONST2.DIALOG_SHOP_ITEMS_IN_STORE));
						string sellNum = string.Format("{0} {1}", sellNumInt, World.GetString(DATCONST2.DIALOG_SHOP_ITEMS_ACCEPTED));
						string craftNum = string.Format("{0} {1}", m_craftItems.Count, World.GetString(DATCONST2.DIALOG_SHOP_ITEMS_ACCEPTED));

						ListDialogItem buy = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
						{
							Text = World.GetString(DATCONST2.DIALOG_SHOP_BUY_ITEMS),
							SubText = buyNum,
							IconGraphic = BuyIcon,
							OffsetY = 45
						};
						buy.OnLeftClick += (o, e) => _setState(ShopState.Buying);
						buy.OnRightClick += (o, e) => _setState(ShopState.Buying);
						buy.ShowItemBackGround = false;
						AddItemToList(buy, false);
						ListDialogItem sell = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
						{
							Text = World.GetString(DATCONST2.DIALOG_SHOP_SELL_ITEMS),
							SubText = sellNum,
							IconGraphic = SellIcon,
							OffsetY = 45
						};
						sell.OnLeftClick += (o, e) => _setState(ShopState.Selling);
						sell.OnRightClick += (o, e) => _setState(ShopState.Selling);
						sell.ShowItemBackGround = false;
						AddItemToList(sell, false);
						if (m_craftItems.Count > 0)
						{
							ListDialogItem craft = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
							{
								Text = World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_ITEMS),
								SubText = craftNum,
								IconGraphic = CraftIcon,
								OffsetY = 45
							};
							craft.OnLeftClick += (o, e) => _setState(ShopState.Crafting);
							craft.OnRightClick += (o, e) => _setState(ShopState.Crafting);
							craft.ShowItemBackGround = false;
							AddItemToList(craft, false);
						}
						_setButtons(ScrollingListDialogButtons.Cancel);
					}
					break;
				case ShopState.Buying:
				case ShopState.Selling:
					{
						//re-use the logic for Buying/Selling: it is almost identical
						bool buying = newState == ShopState.Buying;

						List<ListDialogItem> itemList = new List<ListDialogItem>();
						foreach (ShopItem si in m_tradeItems)
						{
							if (si.ID <= 0 || (buying && si.Buy <= 0) ||
								(!buying && (si.Sell <= 0 || World.Instance.MainPlayer.ActiveCharacter.Inventory.FindIndex(inv => inv.id == si.ID) < 0)))
								continue;

							ShopItem localItem = si;
							ItemRecord rec = World.Instance.EIF.GetItemRecordByID(si.ID);
							string secondary = string.Format("{2}: {0} {1}", buying ? si.Buy : si.Sell,
								rec.Type == ItemType.Armor ? "(" + (rec.Gender == 0 ? World.GetString(DATCONST2.FEMALE) : World.GetString(DATCONST2.MALE)) + ")" : "",
								World.GetString(DATCONST2.DIALOG_SHOP_PRICE));

							ListDialogItem nextItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large)
							{
								Text = rec.Name,
								SubText = secondary,
								IconGraphic = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.Items, 2 * rec.Graphic - 1, true),
								OffsetY = 45
							};
							nextItem.OnLeftClick += (o, e) => _buySellItem(localItem);
							nextItem.OnRightClick += (o, e) => _buySellItem(localItem);

							itemList.Add(nextItem);
						}
						SetItemList(itemList);
						_setButtons(ScrollingListDialogButtons.BackCancel);
					}
					break;
				case ShopState.Crafting:
					{
						List<ListDialogItem> itemList = new List<ListDialogItem>(m_craftItems.Count);
						foreach (CraftItem ci in m_craftItems)
						{
							if (ci.Ingredients.Count <= 0) continue;

							CraftItem localItem = ci;
							ItemRecord rec = World.Instance.EIF.GetItemRecordByID(ci.ID);
							string secondary = string.Format("{2}: {0} {1}", ci.Ingredients.Count,
								rec.Type == ItemType.Armor ? "(" + (rec.Gender == 0 ? World.GetString(DATCONST2.FEMALE) : World.GetString(DATCONST2.MALE)) + ")" : "",
								World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_INGREDIENTS));

							ListDialogItem nextItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large)
							{
								Text = rec.Name,
								SubText = secondary,
								IconGraphic = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.Items, 2 * rec.Graphic - 1, true),
								OffsetY = 45
							};
							nextItem.OnLeftClick += (o, e) => _craftItem(localItem);
							nextItem.OnRightClick += (o, e) => _craftItem(localItem);

							itemList.Add(nextItem);
						}
						SetItemList(itemList);
						_setButtons(ScrollingListDialogButtons.BackCancel);
					}
					break;
			}

			m_state = newState;
		}

		private void _buySellItem(ShopItem item)
		{
			if (m_state != ShopState.Buying && m_state != ShopState.Selling)
				return;
			bool isBuying = m_state == ShopState.Buying;

			InventoryItem ii = World.Instance.MainPlayer.ActiveCharacter.Inventory.Find(x => (isBuying ? x.id == 1 : x.id == item.ID));
			ItemRecord rec = World.Instance.EIF.GetItemRecordByID(item.ID);
			if (isBuying)
			{
				if (!EOGame.Instance.Hud.InventoryFits((short)item.ID))
				{
					EOMessageBox.Show(World.GetString(DATCONST2.DIALOG_TRANSFER_NOT_ENOUGH_SPACE),
						World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
						XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
					return;
				}

				if (rec.Weight + World.Instance.MainPlayer.ActiveCharacter.Weight >
					World.Instance.MainPlayer.ActiveCharacter.MaxWeight)
				{
					EOMessageBox.Show(World.GetString(DATCONST2.DIALOG_TRANSFER_NOT_ENOUGH_WEIGHT),
						World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
						XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
					return;
				}

				if (ii.amount < item.Buy)
				{
					EOMessageBox.Show(DATCONST1.WARNING_YOU_HAVE_NOT_ENOUGH, " gold.", XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
					return;
				}
			}
			else if (ii.amount == 0)
				return; //can't sell if amount of item is 0

			//special case: no need for prompting if selling an item with count == 1 in inventory
			if (!isBuying && ii.amount == 1)
			{
				string _message = string.Format("{0} 1 {1} {2} {3} gold?",
					World.GetString(DATCONST2.DIALOG_WORD_SELL),
					rec.Name,
					World.GetString(DATCONST2.DIALOG_WORD_FOR),
					item.Sell);
				EOMessageBox.Show(_message, World.GetString(DATCONST2.DIALOG_SHOP_SELL_ITEMS), XNADialogButtons.OkCancel,
					EOMessageBoxStyle.SmallDialogSmallHeader, (oo, ee) =>
					{
						if (ee.Result == XNADialogResult.OK && !m_api.SellItem((short)item.ID, 1))
						{
							EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
						}
					});
			}
			else
			{
				ItemTransferDialog dlg = new ItemTransferDialog(rec.Name, ItemTransferDialog.TransferType.ShopTransfer,
					isBuying ? item.MaxBuy : ii.amount, isBuying ? DATCONST2.DIALOG_TRANSFER_BUY : DATCONST2.DIALOG_TRANSFER_SELL);
				dlg.DialogClosing += (o, e) =>
				{
					if (e.Result == XNADialogResult.OK)
					{
						string _message = string.Format("{0} {1} {2} {3} {4} gold?",
							World.GetString(isBuying ? DATCONST2.DIALOG_WORD_BUY : DATCONST2.DIALOG_WORD_SELL),
							dlg.SelectedAmount, rec.Name,
							World.GetString(DATCONST2.DIALOG_WORD_FOR),
							(isBuying ? item.Buy : item.Sell) * dlg.SelectedAmount);

						EOMessageBox.Show(_message,
							World.GetString(isBuying ? DATCONST2.DIALOG_SHOP_BUY_ITEMS : DATCONST2.DIALOG_SHOP_SELL_ITEMS),
							XNADialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader, (oo, ee) =>
							{
								if (ee.Result == XNADialogResult.OK)
								{
									//only actually do the buy/sell if the user then clicks "OK" in the second prompt
									if (isBuying && !m_api.BuyItem((short)item.ID, dlg.SelectedAmount) ||
										!isBuying && !m_api.SellItem((short)item.ID, dlg.SelectedAmount))
									{
										EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
									}
								}
							});
					}
				};
			}
		}

		private void _craftItem(CraftItem item)
		{
			if (m_state != ShopState.Crafting)
				return;

			ItemRecord craftItemRec = World.Instance.EIF.GetItemRecordByID(item.ID);
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var ingredient in item.Ingredients)
			{
				if (World.Instance.MainPlayer.ActiveCharacter.Inventory.FindIndex(_item => _item.id == ingredient.Item1 && _item.amount >= ingredient.Item2) < 0)
				{
					string _message = World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_MISSING_INGREDIENTS) + "\n\n";
					foreach (var ingred in item.Ingredients)
					{
						ItemRecord localRec = World.Instance.EIF.GetItemRecordByID(ingred.Item1);
						_message += string.Format("+  {0}  {1}\n", ingred.Item2, localRec.Name);
					}
					string _caption = string.Format("{0} {1} {2}", World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_INGREDIENTS),
						World.GetString(DATCONST2.DIALOG_WORD_FOR),
						craftItemRec.Name);
					EOMessageBox.Show(_message, _caption, XNADialogButtons.Cancel, EOMessageBoxStyle.LargeDialogSmallHeader);
					return;
				}
			}

			if (!EOGame.Instance.Hud.InventoryFits((short)item.ID))
			{
				EOMessageBox.Show(World.GetString(DATCONST2.DIALOG_TRANSFER_NOT_ENOUGH_SPACE),
					World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
					XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
				return;
			}

			string _message2 = World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_PUT_INGREDIENTS_TOGETHER) + "\n\n";
			foreach (var ingred in item.Ingredients)
			{
				ItemRecord localRec = World.Instance.EIF.GetItemRecordByID(ingred.Item1);
				_message2 += string.Format("+  {0}  {1}\n", ingred.Item2, localRec.Name);
			}
			string _caption2 = string.Format("{0} {1} {2}", World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_INGREDIENTS),
				World.GetString(DATCONST2.DIALOG_WORD_FOR),
				craftItemRec.Name);
			EOMessageBox.Show(_message2, _caption2, XNADialogButtons.OkCancel, EOMessageBoxStyle.LargeDialogSmallHeader, (o, e) =>
			{
				if (e.Result == XNADialogResult.OK && !m_api.CraftItem((short)item.ID))
				{
					EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
				}
			});
		}
	}
}
