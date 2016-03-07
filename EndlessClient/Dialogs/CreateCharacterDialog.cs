// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Dialogs;
using EndlessClient.Rendering;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient
{
	public class EOCreateCharacterDialog : EODialogBase
	{
		private readonly XNATextBox inputBox;
		private readonly XNAButton[] arrowButtons = new XNAButton[4];
		private readonly Rectangle[] srcRects = new Rectangle[4]; //these are rectangles for the sprite sheet with the different parameter colors (hair colors, styles, etc)
		private readonly OldCharacterRenderer charRender;

		public byte Gender { get { return charRender.Gender; } }
		public byte HairType { get { return charRender.HairType; } }
		public byte HairColor { get { return charRender.HairColor; } }
		public byte SkinColor { get { return charRender.SkinColor; } }
		public string Name { get { return inputBox.Text; } }

		private Rectangle rotClickArea;

		private readonly Texture2D charCreateSheet;

		public EOCreateCharacterDialog(Texture2D cursorTexture, KeyboardDispatcher dispatcher)
		{
			bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 20);
			_setSize(bgTexture.Width, bgTexture.Height);

			charCreateSheet = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 22);

			inputBox = new XNATextBox(new Rectangle(80, 57, 138, 19), cursorTexture, Constants.FontSize08)
			{
				LeftPadding = 5,
				DefaultText = " ",
				MaxChars = 12,
				Selected = true,
				TextColor = Constants.LightBeigeText,
				Visible = true
			};
			inputBox.SetParent(this);
			dispatcher.Subscriber = inputBox;

			//four arrow buttons
			for (int i = 0; i < arrowButtons.Length; ++i)
			{
				XNAButton btn = new XNAButton(charCreateSheet, new Vector2(196, 85 + i * 26), new Rectangle(185, 38, 19, 19), new Rectangle(206, 38, 19, 19))
				{
					Visible = true
				};
				btn.OnClick += ArrowButtonClick;
				btn.SetParent(this);
				arrowButtons[i] = btn;
			}

			charRender = new OldCharacterRenderer(new Vector2(269, 83), new CharRenderData { gender = 0, hairstyle = 1, haircolor = 0, race = 0 });
			charRender.SetParent(this);
			srcRects[0] = new Rectangle(0, 38, 23, 19);
			srcRects[1] = new Rectangle(0, 19, 23, 19);
			srcRects[2] = new Rectangle(0, 0, 23, 19);
			srcRects[3] = new Rectangle(46, 38, 23, 19);

			//ok/cancel buttons
			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(157, 195), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok))
			{
				Visible = true
			};
			ok.OnClick += (s, e) =>
			{
				if (inputBox.Text.Length < 4)
				{
					EOMessageBox.Show(DATCONST1.CHARACTER_CREATE_NAME_TOO_SHORT);
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

		private void ArrowButtonClick(object sender, EventArgs e)
		{
			if (sender == arrowButtons[0])
			{
				//change gender
				charRender.Gender = (byte)(charRender.Gender == 0 ? 1 : 0);
				srcRects[0] = new Rectangle(0 + 23 * charRender.Gender, 38, 23, 19);
			}
			else if (sender == arrowButtons[1])
			{
				//change hair type
				charRender.HairType = (byte)(charRender.HairType == 20 ? 1 : charRender.HairType + 1);
				srcRects[1] = new Rectangle(0 + 23 * (charRender.HairType - 1), 19, 23, 19);
			}
			else if (sender == arrowButtons[2])
			{
				//change hair color
				charRender.HairColor = (byte)(charRender.HairColor == 9 ? 0 : charRender.HairColor + 1);
				srcRects[2] = new Rectangle(0 + 23 * charRender.HairColor, 0, 23, 19);
			}
			else if (sender == arrowButtons[3])
			{
				//change skin color
				charRender.SkinColor = (byte)(charRender.SkinColor == 5 ? 0 : charRender.SkinColor + 1);
				srcRects[3] = new Rectangle(46 + 23 * charRender.SkinColor, 38, 23, 19);
			}
		}

		public override void Update(GameTime gt)
		{
			if ((Dialogs.Count > 0 && Dialogs.Peek() != this) || !Visible || !Game.IsActive)
				return;

			rotClickArea = new Rectangle(235 + DrawAreaWithOffset.X, 58 + DrawAreaWithOffset.Y, 99, 123);

			if (((Mouse.GetState().LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed) ||
				(Mouse.GetState().RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)) &&
				rotClickArea.ContainsPoint(Mouse.GetState().X, Mouse.GetState().Y))
			{
				charRender.Facing++;
			}

			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			base.Draw(gt);

			SpriteBatch.Begin();

			SpriteBatch.Draw(charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 84 + DrawAreaWithOffset.Y), srcRects[0], Color.White);
			SpriteBatch.Draw(charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 111 + DrawAreaWithOffset.Y), srcRects[1], Color.White);
			SpriteBatch.Draw(charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 138 + DrawAreaWithOffset.Y), srcRects[2], Color.White);
			SpriteBatch.Draw(charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 165 + DrawAreaWithOffset.Y), srcRects[3], Color.White);

			SpriteBatch.End();
		}
	}
}
