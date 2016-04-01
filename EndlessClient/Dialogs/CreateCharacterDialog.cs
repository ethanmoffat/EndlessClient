// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Data.BLL;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class EOCreateCharacterDialog : EODialogBase
	{
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

		public EOCreateCharacterDialog(Texture2D cursorTexture, KeyboardDispatcher dispatcher)
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

			_characterControl = new CreateCharacterControl();
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
	}
}
