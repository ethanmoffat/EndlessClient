// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Account;
using EOLib.Domain.Login;
using EOLib.Graphics;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class ChangePasswordDialog : EODialogBase
	{
		private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
		private readonly ILoggedInAccountNameProvider _loggedInAccountNameProvider;
		private readonly XNATextBox[] inputBoxes = new XNATextBox[4];
		
		private readonly KeyboardDispatcher _dispatcher;
		private readonly TextBoxClickEventHandler _clickEventHandler;
		private readonly TextBoxTabEventHandler _tabEventHandler;

		private readonly TaskCompletionSource<XNADialogResult> _dialogResultCompletionSource;

		public string Username { get { return inputBoxes[0].Text; } }
		public string OldPassword { get { return inputBoxes[1].Text; } }
		public string NewPassword { get { return inputBoxes[2].Text; } }
		public string ConfirmPassword { get { return inputBoxes[3].Text; } }

		public ChangePasswordDialog(Texture2D cursorTexture, KeyboardDispatcher dispatcher)
			: base((PacketAPI)null)
		{
			_dispatcher = dispatcher;

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
					_dispatcher.Subscriber = inputBoxes[next];
				};

				tb.OnClicked += (s, e) =>
				{
					_dispatcher.Subscriber.Selected = false;
					_dispatcher.Subscriber = (s as XNATextBox);
					dispatcher.Subscriber.Selected = true;
				};

				tb.SetParent(this);
				inputBoxes[i] = tb;
			}

			_dispatcher.Subscriber = inputBoxes[0];

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

		public ChangePasswordDialog(INativeGraphicsManager nativeGraphicsManager,
									IGraphicsDeviceProvider graphicsDeviceProvider,
									IGameStateProvider gameStateProvider,
									IContentManagerProvider contentManagerProvider,
									IEOMessageBoxFactory eoMessageBoxFactory,
									IKeyboardDispatcherProvider keyboardDispatcherProvider,
									ILoggedInAccountNameProvider loggedInAccountNameProvider)
			: base(nativeGraphicsManager)
		{
			_eoMessageBoxFactory = eoMessageBoxFactory;
			_loggedInAccountNameProvider = loggedInAccountNameProvider;
			_dispatcher = keyboardDispatcherProvider.Dispatcher;

			bgTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 21);
			_setSize(bgTexture.Width, bgTexture.Height);

			var cursorTexture = contentManagerProvider.Content.Load<Texture2D>("Cursor");

			for (int i = 0; i < inputBoxes.Length; ++i)
			{
				var tb = new XNATextBox(new Rectangle(198, 60 + i * 30, 137, 19), cursorTexture, Constants.FontSize08)
				{
					LeftPadding = 5,
					DefaultText = " ",
					MaxChars = i == 0 ? 16 : 12,
					PasswordBox = i > 1,
					Selected = i == 0,
					TextColor = Constants.LightBeigeText,
					Visible = true
				};
				tb.SetParent(this);
				inputBoxes[i] = tb;
			}

			_clickEventHandler = new TextBoxClickEventHandler(_dispatcher, inputBoxes);
			_tabEventHandler = new TextBoxTabEventHandler(_dispatcher, inputBoxes);

			_dispatcher.Subscriber.Selected = false;
			_dispatcher.Subscriber = inputBoxes[0];
			_dispatcher.Subscriber.Selected = true;

			var ok = new XNAButton(smallButtonSheet, new Vector2(157, 195), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
			ok.OnClick += (s, e) => Close(ok, XNADialogResult.OK);
			ok.SetParent(this);
			dlgButtons.Add(ok);

			var cancel = new XNAButton(smallButtonSheet, new Vector2(250, 195), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
			cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
			cancel.SetParent(this);
			dlgButtons.Add(cancel);

			DialogClosing += OnDialogClosing;
			_dialogResultCompletionSource = new TaskCompletionSource<XNADialogResult>();

			CenterAndFixDrawOrder(graphicsDeviceProvider, gameStateProvider);
		}

		private void OnDialogClosing(object sender, CloseDialogEventArgs e)
		{
			if (e.Result == XNADialogResult.OK)
			{
				if (inputBoxes.Any(tb => string.IsNullOrWhiteSpace(tb.Text)))
				{
					e.CancelClose = true;
					return;
				}

				if (Username != _loggedInAccountNameProvider.LoggedInAccountName)
				{
					e.CancelClose = true;
					_eoMessageBoxFactory.CreateMessageBox(DATCONST1.CHANGE_PASSWORD_MISMATCH);
					return;
				}

				if (NewPassword.Length != ConfirmPassword.Length || NewPassword != ConfirmPassword)
				{
					e.CancelClose = true;
					_eoMessageBoxFactory.CreateMessageBox(DATCONST1.ACCOUNT_CREATE_PASSWORD_MISMATCH);
					return;
				}

				if (NewPassword.Length < 6)
				{
					e.CancelClose = true;
					_eoMessageBoxFactory.CreateMessageBox(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);
					return;
				}
			}

			_dialogResultCompletionSource.SetResult(e.Result);
		}

		public new async Task<IChangePasswordParameters> Show()
		{
			var result = await _dialogResultCompletionSource.Task;
			if (result != XNADialogResult.OK)
				throw new OperationCanceledException();

			return new ChangePasswordParameters(Username, OldPassword, NewPassword);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_clickEventHandler.Dispose();
				_tabEventHandler.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
