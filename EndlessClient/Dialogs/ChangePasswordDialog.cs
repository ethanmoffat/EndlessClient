// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class ChangePasswordDialog : EODialogBase
	{
		private readonly XNATextBox[] inputBoxes = new XNATextBox[4];
		private readonly KeyboardDispatcher dispatch;

		public string Username { get { return inputBoxes[0].Text; } }
		public string OldPassword { get { return inputBoxes[1].Text; } }
		public string NewPassword { get { return inputBoxes[2].Text; } }

		public ChangePasswordDialog(Texture2D cursorTexture, KeyboardDispatcher dispatcher)
		{
			dispatch = dispatcher;

			bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 21);
			_setSize(bgTexture.Width, bgTexture.Height);

			for (int i = 0; i < inputBoxes.Length; ++i)
			{
				XNATextBox tb = new XNATextBox(new Rectangle(198, 60 + i * 30, 137, 19), cursorTexture, Constants.FontSize08)
				{
					LeftPadding = 5,
					DefaultText = " ",
					MaxChars = i == 0 ? 16 : 12,
					PasswordBox = i > 1,
					Selected = i == 0,
					TextColor = Constants.LightBeigeText,
					Visible = true
				};

				tb.OnTabPressed += (s, e) =>
				{
					List<XNATextBox> list = inputBoxes.ToList();
					int tbIndex = list.FindIndex(txt => txt == s);

					int next = tbIndex + 1 > 3 ? 0 : tbIndex + 1;
					inputBoxes[tbIndex].Selected = false;
					inputBoxes[next].Selected = true;
					dispatch.Subscriber = inputBoxes[next];
				};

				tb.OnClicked += (s, e) =>
				{
					dispatch.Subscriber.Selected = false;
					dispatch.Subscriber = (s as XNATextBox);
					dispatcher.Subscriber.Selected = true;
				};

				tb.SetParent(this);
				inputBoxes[i] = tb;
			}

			dispatch.Subscriber = inputBoxes[0];

			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(157, 195), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok))
			{
				Visible = true
			};
			ok.OnClick += (s, e) =>
			{ //does some input validation before trying to call Close
				//check that all fields are filled in, otherwise: return
				if (inputBoxes.Any(tb => string.IsNullOrWhiteSpace(tb.Text))) return;

				if (Username != OldWorld.Instance.MainPlayer.AccountName)
				{
					EOMessageBox.Show(DATCONST1.CHANGE_PASSWORD_MISMATCH);
					return;
				}

				//check that passwords match, otherwise: return
				if (inputBoxes[2].Text.Length != inputBoxes[3].Text.Length || inputBoxes[2].Text != inputBoxes[3].Text)
				{
					EOMessageBox.Show(DATCONST1.ACCOUNT_CREATE_PASSWORD_MISMATCH);
					return;
				}

				//check that password is > 6 chars, otherwise: return
				if (inputBoxes[2].Text.Length < 6)
				{
					EOMessageBox.Show(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);
					return;
				}

				Close(ok, XNADialogResult.OK);
			};
			ok.SetParent(this);
			dlgButtons.Add(ok);

			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(250, 195), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel))
			{
				Visible = true
			};
			cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
			cancel.SetParent(this);
			dlgButtons.Add(cancel);

			endConstructor();
		}
	}
}
