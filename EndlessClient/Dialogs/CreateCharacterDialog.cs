// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Factories;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.BLL;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class CreateCharacterDialog : EODialogBase
	{
		private readonly IEOMessageBoxFactory _messageBoxFactory;
		private readonly ManualResetEventSlim _doneEvent;

		private readonly XNATextBox _inputBox;
		private readonly XNAButton[] _arrowButtons = new XNAButton[4];

		private readonly CreateCharacterControl _characterControl;

		private readonly Rectangle[] _srcRectangles = new Rectangle[4];
		private readonly Texture2D _charCreateSheet;

		public ICharacterRenderProperties RenderProperties
		{
			get { return _characterControl.RenderProperties; }
		}

		public string Name { get { return _inputBox.Text; } }

		private XNADialogResult _result;

		public CreateCharacterDialog(Texture2D cursorTexture, KeyboardDispatcher dispatcher)
			: base((PacketAPI)null)
		{
			bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 20);
			_setSize(bgTexture.Width, bgTexture.Height);

			_charCreateSheet = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 22);

			_inputBox = new XNATextBox(new Rectangle(80, 57, 138, 19), cursorTexture, Constants.FontSize08)
			{
				LeftPadding = 5,
				DefaultText = " ",
				Text = " ",
				MaxChars = 12,
				Selected = true,
				TextColor = Constants.LightBeigeText,
				Visible = true
			};
			_inputBox.SetParent(this);
			dispatcher.Subscriber = _inputBox;

			for (int i = 0; i < _arrowButtons.Length; ++i)
			{
				var btn = new XNAButton(_charCreateSheet, new Vector2(196, 85 + i * 26), new Rectangle(185, 38, 19, 19), new Rectangle(206, 38, 19, 19))
				{
					Visible = true
				};
				btn.OnClick += ArrowButtonClick;
				btn.SetParent(this);
				_arrowButtons[i] = btn;
			}

			_characterControl = new CreateCharacterControl(null); //todo: this needs to be injected
			_characterControl.SetParent(this);
			_characterControl.DrawLocation = new Vector2(235, 58);

			_srcRectangles[0] = new Rectangle(0, 38, 23, 19);
			_srcRectangles[1] = new Rectangle(0, 19, 23, 19);
			_srcRectangles[2] = new Rectangle(0, 0, 23, 19);
			_srcRectangles[3] = new Rectangle(46, 38, 23, 19);

			var okButton = new XNAButton(smallButtonSheet, new Vector2(157, 195), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok))
			{
				Visible = true
			};
			okButton.OnClick += (s, e) =>
			{
				if (_inputBox.Text.Length < 4)
				{
					EOMessageBox.Show(DATCONST1.CHARACTER_CREATE_NAME_TOO_SHORT);
					return;
				}

				Close(okButton, XNADialogResult.OK);
			};
			okButton.SetParent(this);
			dlgButtons.Add(okButton);

			var _cancelButton = new XNAButton(smallButtonSheet, new Vector2(250, 195), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel))
			{
				Visible = true
			};
			_cancelButton.OnClick += (s, e) => Close(_cancelButton, XNADialogResult.Cancel);
			_cancelButton.SetParent(this);
			dlgButtons.Add(_cancelButton);

			endConstructor();
		}

		public CreateCharacterDialog(
			INativeGraphicsManager nativeGraphicsManager,
			IGraphicsDeviceProvider graphicsDeviceProvider,
			IGameStateProvider gameStateProvider,
			ICharacterRendererFactory rendererFactory,
			ContentManager contentManager,
			KeyboardDispatcher dispatcher,
			IEOMessageBoxFactory messageBoxFactory)
			: base(nativeGraphicsManager)
		{
			_messageBoxFactory = messageBoxFactory;
			bgTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 20);
			_setSize(bgTexture.Width, bgTexture.Height);

			_charCreateSheet = nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 22);

			dispatcher.Subscriber.Selected = false;
			var cursorTexture = contentManager.Load<Texture2D>("cursor");
			_inputBox = new XNATextBox(new Rectangle(80, 57, 138, 19), cursorTexture, Constants.FontSize08)
			{
				LeftPadding = 5,
				DefaultText = " ",
				Text = " ",
				MaxChars = 12,
				Selected = true,
				TextColor = Constants.LightBeigeText,
				Visible = true
			};
			_inputBox.SetParent(this);
			dispatcher.Subscriber = _inputBox;

			for (int i = 0; i < _arrowButtons.Length; ++i)
			{
				var btn = new XNAButton(_charCreateSheet,
					new Vector2(196, 85 + i * 26),
					new Rectangle(185, 38, 19, 19),
					new Rectangle(206, 38, 19, 19));
				btn.OnClick += ArrowButtonClick;
				btn.SetParent(this);
				_arrowButtons[i] = btn;
			}

			_characterControl = new CreateCharacterControl(rendererFactory);
			_characterControl.SetParent(this);
			_characterControl.DrawLocation = new Vector2(235, 58);

			_srcRectangles[0] = new Rectangle(0, 38, 23, 19);
			_srcRectangles[1] = new Rectangle(0, 19, 23, 19);
			_srcRectangles[2] = new Rectangle(0, 0, 23, 19);
			_srcRectangles[3] = new Rectangle(46, 38, 23, 19);

			var okButton = new XNAButton(smallButtonSheet,
				new Vector2(157, 195),
				_getSmallButtonOut(SmallButton.Ok),
				_getSmallButtonOver(SmallButton.Ok));
			okButton.OnClick += (s, e) => Close(okButton, XNADialogResult.OK);
			okButton.SetParent(this);
			dlgButtons.Add(okButton);

			var cancelButton = new XNAButton(smallButtonSheet,
				new Vector2(250, 195),
				_getSmallButtonOut(SmallButton.Cancel),
				_getSmallButtonOver(SmallButton.Cancel));
			cancelButton.OnClick += (s, e) => Close(cancelButton, XNADialogResult.Cancel);
			cancelButton.SetParent(this);
			dlgButtons.Add(cancelButton);

			_doneEvent = new ManualResetEventSlim(false);
			DialogClosing += DialogClosingHandler;

			CenterAndFixDrawOrder(graphicsDeviceProvider, gameStateProvider);
		}

		public override void Initialize()
		{
			_characterControl.Initialize();
			base.Initialize();
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			base.Draw(gt);

			SpriteBatch.Begin();

			SpriteBatch.Draw(_charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 84 + DrawAreaWithOffset.Y), _srcRectangles[0], Color.White);
			SpriteBatch.Draw(_charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 111 + DrawAreaWithOffset.Y), _srcRectangles[1], Color.White);
			SpriteBatch.Draw(_charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 138 + DrawAreaWithOffset.Y), _srcRectangles[2], Color.White);
			SpriteBatch.Draw(_charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 165 + DrawAreaWithOffset.Y), _srcRectangles[3], Color.White);

			SpriteBatch.End();
		}

		public new async Task<ICharacterCreateParameters> Show()
		{
			await Task.Run(() => _doneEvent.Wait());

			if (_result == XNADialogResult.Cancel)
				throw new OperationCanceledException();

			return new CharacterCreateParameters(Name,
				RenderProperties.Gender,
				RenderProperties.HairStyle,
				RenderProperties.HairColor,
				RenderProperties.Race);
		}

		private void ArrowButtonClick(object sender, EventArgs e)
		{
			if (sender == _arrowButtons[0])
			{
				_characterControl.NextGender();
				_srcRectangles[0] = new Rectangle(0 + 23 * RenderProperties.Gender, 38, 23, 19);
			}
			else if (sender == _arrowButtons[1])
			{
				_characterControl.NextHairStyle();
				_srcRectangles[1] = new Rectangle(0 + 23 * (RenderProperties.HairStyle - 1), 19, 23, 19);
			}
			else if (sender == _arrowButtons[2])
			{
				_characterControl.NextHairColor();
				_srcRectangles[2] = new Rectangle(0 + 23 * RenderProperties.HairColor, 0, 23, 19);
			}
			else if (sender == _arrowButtons[3])
			{
				_characterControl.NextRace();
				_srcRectangles[3] = new Rectangle(46 + 23 * RenderProperties.Race, 38, 23, 19);
			}
		}

		private void DialogClosingHandler(object sender, CloseDialogEventArgs args)
		{
			if (_inputBox.Text.Length < 4)
			{
				_messageBoxFactory.CreateMessageBox(DATCONST1.CHARACTER_CREATE_NAME_TOO_SHORT,
					XNADialogButtons.Ok,
					EOMessageBoxStyle.SmallDialogLargeHeader);
				args.CancelClose = true;
				return;
			}

			_result = args.Result;
			_doneEvent.Set();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_doneEvent.Dispose();
			base.Dispose(disposing);
		}
	}
}
