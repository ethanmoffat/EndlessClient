// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.Net;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class BankAccountDialog : EODialogBase
	{
		public static BankAccountDialog Instance { get; private set; }

		public static void Show(PacketAPI api, short npcID)
		{
			if (Instance != null)
				return;

			Instance = new BankAccountDialog(api);

			if (!api.BankOpen(npcID))
			{
				Instance.Close();
				Instance = null;
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			}
		}

		private readonly XNALabel m_accountBalance;

		public string AccountBalance
		{
			get { return m_accountBalance.Text; }
			set { m_accountBalance.Text = value; }
		}

		public int LockerUpgrades { get; set; }

		private BankAccountDialog(PacketAPI api)
			: base(api)
		{
			//this uses EODialogListItems but does not inherit from ListDialog since it is a different size
			//offsety 50
			bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 53);
			_setSize(bgTexture.Width, bgTexture.Height);

			m_accountBalance = new XNALabel(new Rectangle(129, 20, 121, 16), Constants.FontSize08pt5)
			{
				ForeColor = ColorConstants.LightGrayText,
				Text = "",
				TextAlign = LabelAlignment.MiddleRight,
				AutoSize = false
			};
			m_accountBalance.SetParent(this);

			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(92, 191), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
			cancel.SetParent(this);
			cancel.OnClick += (o, e) => Close(cancel, XNADialogResult.Cancel);

			ListDialogItem deposit = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
			{
				Text = OldWorld.GetString(DATCONST2.DIALOG_BANK_DEPOSIT),
				SubText = string.Format("{0} gold {1}", OldWorld.GetString(DATCONST2.DIALOG_BANK_TRANSFER),
					OldWorld.GetString(DATCONST2.DIALOG_BANK_TO_ACCOUNT)),
				IconGraphic = _getDlgIcon(ListIcon.BankDeposit),
				OffsetY = 55,
				ShowItemBackGround = false
			};
			deposit.OnLeftClick += (o, e) => _deposit();
			deposit.OnRightClick += (o, e) => _deposit();
			ListDialogItem withdraw = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
			{
				Text = OldWorld.GetString(DATCONST2.DIALOG_BANK_WITHDRAW),
				SubText = string.Format("{0} gold {1}", OldWorld.GetString(DATCONST2.DIALOG_BANK_TAKE),
					OldWorld.GetString(DATCONST2.DIALOG_BANK_FROM_ACCOUNT)),
				IconGraphic = _getDlgIcon(ListIcon.BankWithdraw),
				OffsetY = 55,
				ShowItemBackGround = false
			};
			withdraw.OnLeftClick += (o, e) => _withdraw();
			withdraw.OnRightClick += (o, e) => _withdraw();
			ListDialogItem upgrade = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
			{
				Text = OldWorld.GetString(DATCONST2.DIALOG_BANK_LOCKER_UPGRADE),
				SubText = OldWorld.GetString(DATCONST2.DIALOG_BANK_MORE_SPACE),
				IconGraphic = _getDlgIcon(ListIcon.BankLockerUpgrade),
				OffsetY = 55,
				ShowItemBackGround = false
			};
			upgrade.OnLeftClick += (o, e) => _upgrade();
			upgrade.OnRightClick += (o, e) => _upgrade();

			DialogClosing += (o, e) => { Instance = null; };

			Center(Game.GraphicsDevice);
			DrawLocation = new Vector2(DrawLocation.X, 50);
			endConstructor(false);
		}

		private void _deposit()
		{
			InventoryItem item = OldWorld.Instance.MainPlayer.ActiveCharacter.Inventory.Find(i => i.ItemID == 1);
			if (item.Amount == 0)
			{
				EOMessageBox.Show(DATCONST1.BANK_ACCOUNT_UNABLE_TO_DEPOSIT, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
				return;
			}
			if (item.Amount == 1)
			{
				if (!m_api.BankDeposit(1))
				{
					Close(null, XNADialogResult.NO_BUTTON_PRESSED);
					EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
				}
				return;
			}

			ItemTransferDialog dlg = new ItemTransferDialog(OldWorld.Instance.EIF.GetRecordByID(1).Name,
				ItemTransferDialog.TransferType.BankTransfer, item.Amount, DATCONST2.DIALOG_TRANSFER_DEPOSIT);
			dlg.DialogClosing += (o, e) =>
			{
				if (e.Result == XNADialogResult.Cancel)
					return;

				if (!m_api.BankDeposit(dlg.SelectedAmount))
				{
					Close(null, XNADialogResult.NO_BUTTON_PRESSED);
					EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
				}
			};
		}

		private void _withdraw()
		{
			int balance = int.Parse(AccountBalance);
			if (balance == 0)
			{
				EOMessageBox.Show(DATCONST1.BANK_ACCOUNT_UNABLE_TO_WITHDRAW, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
				return;
			}
			if (balance == 1)
			{
				if (!m_api.BankWithdraw(1))
				{
					Close(null, XNADialogResult.NO_BUTTON_PRESSED);
					EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
				}
				return;
			}

			ItemTransferDialog dlg = new ItemTransferDialog(OldWorld.Instance.EIF.GetRecordByID(1).Name,
				ItemTransferDialog.TransferType.BankTransfer, balance, DATCONST2.DIALOG_TRANSFER_WITHDRAW);
			dlg.DialogClosing += (o, e) =>
			{
				if (e.Result == XNADialogResult.Cancel)
					return;

				if (!m_api.BankWithdraw(dlg.SelectedAmount))
				{
					Close(null, XNADialogResult.NO_BUTTON_PRESSED);
					EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
				}
			};
		}

		private void _upgrade()
		{
			if (LockerUpgrades == 7)
			{
				EOMessageBox.Show(DATCONST1.LOCKER_UPGRADE_IMPOSSIBLE, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
				return;
			}

			int requiredGold = (LockerUpgrades + 1) * 1000;
			InventoryItem item = OldWorld.Instance.MainPlayer.ActiveCharacter.Inventory.Find(i => i.ItemID == 1);
			if (item.Amount < requiredGold)
			{
				EOMessageBox.Show(DATCONST1.WARNING_YOU_HAVE_NOT_ENOUGH, "gold", XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
				return;
			}

			EOMessageBox.Show(DATCONST1.LOCKER_UPGRADE_UNIT, string.Format("{0} gold?", requiredGold), XNADialogButtons.OkCancel,
				EOMessageBoxStyle.SmallDialogSmallHeader,
				(o, e) =>
				{
					if (e.Result == XNADialogResult.Cancel)
						return;

					OldPacket pkt = new OldPacket(PacketFamily.Locker, PacketAction.Buy);
					OldWorld.Instance.Client.SendPacket(pkt);
				});
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
	}
}
