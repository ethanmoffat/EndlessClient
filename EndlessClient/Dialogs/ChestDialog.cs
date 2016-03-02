// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	//todo: this should derive from ListDialog
	public class ChestDialog : EODialogBase
	{
		public static ChestDialog Instance { get; private set; }

		public static void Show(PacketAPI apiHandle, byte chestX, byte chestY)
		{
			if (Instance != null)
				return;

			Instance = new ChestDialog(apiHandle, chestX, chestY);
			Instance.DialogClosing += (o, e) => Instance = null;

			if (!apiHandle.ChestOpen(chestX, chestY))
			{
				Instance.Close(null, XNADialogResult.NO_BUTTON_PRESSED);
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			}
		}

		public byte CurrentChestX { get; private set; }
		public byte CurrentChestY { get; private set; }

		private ListDialogItem[] m_items;

		private ChestDialog(PacketAPI api, byte chestX, byte chestY)
			: base(api)
		{
			CurrentChestX = chestX;
			CurrentChestY = chestY;

			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(92, 227), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
			cancel.OnClick += (sender, e) => Close(cancel, XNADialogResult.Cancel);
			dlgButtons.Add(cancel);
			whichButtons = XNADialogButtons.Cancel;

			bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 51);
			_setSize(bgTexture.Width, bgTexture.Height);

			endConstructor(false);
			DrawLocation = new Vector2((Game.GraphicsDevice.PresentationParameters.BackBufferWidth - DrawArea.Width) / 2f, 15);
			cancel.SetParent(this);

			EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_CHEST_YOU_OPENED,
				OldWorld.GetString(DATCONST2.STATUS_LABEL_DRAG_AND_DROP_ITEMS));
		}

		public void InitializeItems(IList<InventoryItem> initialItems)
		{
			if (m_items == null)
				m_items = new ListDialogItem[5];

			int i = 0;
			if (initialItems.Count > 0)
			{
				for (; i < initialItems.Count && i < 5; ++i)
				{
					var item = initialItems[i];
					if (m_items[i] != null)
					{
						m_items[i].Close();
						m_items[i] = null;
					}

					ItemRecord rec = OldWorld.Instance.EIF.GetItemRecordByID(item.id);
					string secondary = string.Format("x {0}  {1}", item.amount, rec.Type == ItemType.Armor
						? "(" + (rec.Gender == 0 ? OldWorld.GetString(DATCONST2.FEMALE) : OldWorld.GetString(DATCONST2.MALE)) + ")"
						: "");

					m_items[i] = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, i)
					{
						Text = rec.Name,
						SubText = secondary,
						IconGraphic = GetItemGraphic(rec, item.amount),
						ID = item.id
					};
					m_items[i].OnRightClick += (o, e) =>
					{
						ListDialogItem sender = o as ListDialogItem;
						if (sender == null) return;

						if (!EOGame.Instance.Hud.InventoryFits(sender.ID))
						{
							string _message = OldWorld.GetString(DATCONST2.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
							string _caption = OldWorld.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING);
							EOMessageBox.Show(_message, _caption, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
							((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
						}
						else if (rec.Weight * item.amount + OldWorld.Instance.MainPlayer.ActiveCharacter.Weight >
								 OldWorld.Instance.MainPlayer.ActiveCharacter.MaxWeight)
						{
							EOMessageBox.Show(OldWorld.GetString(DATCONST2.DIALOG_ITS_TOO_HEAVY_WEIGHT),
								OldWorld.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
								XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
						}
						else
						{
							if (!m_api.ChestTakeItem(CurrentChestX, CurrentChestY, sender.ID))
							{
								Close();
								EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
							}
						}
					};
				}
			}

			for (; i < m_items.Length; ++i)
			{
				if (m_items[i] != null)
				{
					m_items[i].Close();
					m_items[i] = null;
				}
			}
		}

		public override void Initialize()
		{
			//make sure the offsets are correct
			foreach (XNAControl child in children)
				child.SetParent(this);
			base.Initialize();
		}

		public override void Update(GameTime gt)
		{
			if (!Game.IsActive) return;

			if (EOGame.Instance.Hud.IsInventoryDragging())
			{
				shouldClickDrag = false;
				SuppressParentClickDrag(true);
			}
			else
			{
				shouldClickDrag = true;
				SuppressParentClickDrag(false);
			}

			base.Update(gt);
		}

		//todo: find a better place for this
		public static Texture2D GetItemGraphic(ItemRecord item, int amount)
		{
			if (item.Type == ItemType.Money)
			{
				var gfx = amount >= 100000 ? 4 : (
					amount >= 10000 ? 3 : (
					amount >= 100 ? 2 : (
					amount >= 2 ? 1 : 0)));
				return EOGame.Instance.GFXManager.TextureFromResource(GFXTypes.Items, 269 + 2*gfx, true);
			}

			return EOGame.Instance.GFXManager.TextureFromResource(GFXTypes.Items, 2*item.Graphic - 1, true);
		}
	}
}
