using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

#pragma warning disable 162

namespace EndlessClient
{
	public class EODialogBase : XNADialog
	{
		protected readonly Texture2D smallButtonSheet;

		protected EODialogBase(Game encapsulatingGame)
			: base(encapsulatingGame)
		{
			smallButtonSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 15, true);
		}

		protected void endConstructor(bool centerDialog = true)
		{
			//center dialog based on txtSize of background texture
			if (centerDialog)
				Center(Game.GraphicsDevice);
			_fixDrawOrder();
			XNAControl.Dialogs.Push(this);

			Game.Components.Add(this);
		}
	}

	/// <summary>
	/// EODialog is a basic dialog representation (like Windows MessageBox)
	/// </summary>
	public class EODialog : EODialogBase
	{
		public EODialog(Game encapsulatingGame, string msgText, string captionText = "", XNADialogButtons whichButtons = XNADialogButtons.Ok, bool useSmallHeader = false)
			: base(encapsulatingGame)
		{
			if (!useSmallHeader)
				bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 18);
			else
				bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 23);
			_setSize(bgTexture.Width, bgTexture.Height);
			
			message = new XNALabel(encapsulatingGame, new Rectangle(18, 57, 1, 1), "Microsoft Sans Serif", 10.0f); //label is auto-sized
			if(useSmallHeader)
			{
				//179, 119
				//caption 197, 128
				//message 197, 156
				//ok: 270, 201
				//cancel: 363, 201
				message.DrawLocation = new Vector2(18, 40);
			}
			message.ForeColor = System.Drawing.Color.FromArgb(255, 0xf0, 0xf0, 0xc8);
			message.Text = msgText;
			message.TextWidth = 254;
			message.SetParent(this);

			caption = new XNALabel(encapsulatingGame, new Rectangle(59, 23, 1, 1), "Microsoft Sans Serif", 10.0f);
			if(useSmallHeader)
			{
				caption.DrawLocation = new Vector2(18, 12);
			}
			caption.ForeColor = System.Drawing.Color.FromArgb(255, 0xf0, 0xf0, 0xc8);
			caption.Text = captionText;
			caption.SetParent(this);

			XNAButton ok, cancel;
			switch(whichButtons)
			{
				case XNADialogButtons.Ok:
					ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
					ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
					ok.SetParent(this);
					dlgButtons.Add(ok);
					break;
				case XNADialogButtons.Cancel:
					cancel = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
					cancel.OnClick += (sender, e) => Close(cancel, XNADialogResult.Cancel);
					cancel.SetParent(this);
					dlgButtons.Add(cancel);
					break;
				case XNADialogButtons.OkCancel:
					//implement this more fully when it is needed
					//update draw location of ok button to be on left?
					ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(89, 113), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
					ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
					ok.SetParent(this);

					cancel = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
					cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
					cancel.SetParent(this);

					dlgButtons.Add(ok);
					dlgButtons.Add(cancel);
					break;
			}

			if(useSmallHeader)
			{
				foreach (XNAButton btn in dlgButtons)
					btn.DrawLocation = new Vector2(btn.DrawLocation.X, 82);
			}

			base.endConstructor();
		}
	}

	public class EOScrollBar : XNAControl
	{
		public enum ScrollColors
		{
			LightOnDark, //bottom set of light
			LightOnLight, //top set of light
			LightOnMed, //middle set of light
			DarkOnDark //very bottom set
		}

		private Rectangle scrollArea; //area valid for scrolling: always 16 from top and 16 from bottom
		public int ScrollOffset { get; private set; }

		private readonly XNAButton up, down, scroll; //buttons

		private int _rowHeight, _totalHeight;

		private enum Mode
		{
			LineByLineRender,
			/// <summary>
			/// LabelRender is being deprecated. Soon, mode will not exist and LabelRender will be gone
			/// </summary>
			LabelRender
		}
		private Mode _mode;

		public EOScrollBar(Game encapsulatingGame, XNAControl parent, Vector2 relativeLoc, Vector2 size, ScrollColors palette)
			: base(encapsulatingGame, relativeLoc, new Rectangle((int)relativeLoc.X, (int)relativeLoc.Y, (int)size.X, (int)size.Y))
		{
			SetParent(parent);
			scrollArea = new Rectangle(0, 15, 0, (int)size.Y - 15);
			DrawLocation = relativeLoc;
			ScrollOffset = 0;

			Texture2D scrollSpriteSheet = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 29);
			Rectangle[] upArrows = new Rectangle[2];
			Rectangle[] downArrows = new Rectangle[2];
			int vertOff;
			switch (palette)
			{
				case ScrollColors.LightOnLight: vertOff = 0; break;
				case ScrollColors.LightOnMed: vertOff = 105; break;
				case ScrollColors.LightOnDark: vertOff = 180; break;
				case ScrollColors.DarkOnDark: vertOff = 255; break;
				default:
					throw new ArgumentOutOfRangeException("palette");
			}

			//regions based on verticle offset (which is based on the chosen palette)
			upArrows[0] = new Rectangle(0, vertOff + 15 * 3, 16, 15);
			upArrows[1] = new Rectangle(0, vertOff + 15 * 4, 16, 15);
			downArrows[0] = new Rectangle(0, vertOff + 15, 16, 15);
			downArrows[1] = new Rectangle(0, vertOff + 15 * 2, 16, 15);
			Rectangle scrollBox = new Rectangle(0, vertOff, 16, 15);

			Texture2D[] upButton = new Texture2D[2];
			Texture2D[] downButton = new Texture2D[2];
			Texture2D[] scrollButton = new Texture2D[2];
			for (int i = 0; i < 2; ++i)
			{
				upButton[i] = new Texture2D(scrollSpriteSheet.GraphicsDevice, upArrows[i].Width, upArrows[i].Height);
				Color[] upData = new Color[upArrows[i].Width * upArrows[i].Height];
				scrollSpriteSheet.GetData(0, upArrows[i], upData, 0, upData.Length);
				upButton[i].SetData(upData);

				downButton[i] = new Texture2D(scrollSpriteSheet.GraphicsDevice, downArrows[i].Width, downArrows[i].Height);
				Color[] downData = new Color[downArrows[i].Width * downArrows[i].Height];
				scrollSpriteSheet.GetData(0, downArrows[i], downData, 0, downData.Length);
				downButton[i].SetData(downData);

				//same texture for hover, AFAIK
				scrollButton[i] = new Texture2D(scrollSpriteSheet.GraphicsDevice, scrollBox.Width, scrollBox.Height);
				Color[] scrollData = new Color[scrollBox.Width * scrollBox.Height];
				scrollSpriteSheet.GetData(0, scrollBox, scrollData, 0, scrollData.Length);
				scrollButton[i].SetData(scrollData);
			}

			up = new XNAButton(encapsulatingGame, upButton, new Vector2(0, 0));
			up.OnClick += arrowClicked;
			up.SetParent(this);
			down = new XNAButton(encapsulatingGame, downButton, new Vector2(0, size.Y - 15)); //update coordinates!!!!
			down.OnClick += arrowClicked;
			down.SetParent(this);
			scroll = new XNAButton(encapsulatingGame, scrollButton, new Vector2(0, 15)); //update coordinates!!!!
			scroll.OnClickDrag += scrollDragged;
			scroll.SetParent(this);

			_rowHeight = 20;
			_totalHeight = DrawAreaWithOffset.Height;
			_mode = Mode.LabelRender;
		}

		public void UpdateDimensions(int totalHeight, int rowHeight)
		{
			_totalHeight = totalHeight;
			_rowHeight = rowHeight;
			_mode = Mode.LabelRender;
		}

		public void UpdateDimensions(int numberOfLines)
		{
			_totalHeight = numberOfLines;
			_mode = Mode.LineByLineRender;
		}

		public void ScrollToEnd()
		{
			while(ScrollOffset < _totalHeight - 7)
				arrowClicked(down, new EventArgs());
		}

		//the point of arrowClicked and scrollDragged is to respond to input on the three buttons in such
		//	 a way that ScrollOffset is updated and the Y coordinate for the scroll box is updated.
		//	 ScrollOffset provides a value that is used within the EOScrollDialog.Draw method.
		//	 The Y coordinate for the scroll box determines where it is drawn.
		private void arrowClicked(object btn, EventArgs e)
		{
			switch (_mode)
			{
				case Mode.LabelRender:
				{
					if (_totalHeight < drawArea.Height)
						return;

					int step = _totalHeight/_rowHeight;

					if (btn == up)
					{
						if (ScrollOffset <= 0)
						{
							ScrollOffset = 0;
							return;
						}

						ScrollOffset -= step;
					}
					else if (btn == down)
					{
						if (ScrollOffset >= scrollArea.Height - scroll.DrawArea.Height)
							return;

						ScrollOffset += step;
					}
					else
						return; //no other buttons should send this event

					if (ScrollOffset < 0)
						ScrollOffset = 0;
					else if (ScrollOffset > scrollArea.Height - scroll.DrawArea.Height)
						ScrollOffset = scrollArea.Height - scroll.DrawArea.Height;

					//update the y coordinate of the scroll button
					//this function is basically reversed to solve for ScrollOffset in the scrollDragged event below
					//the 2.5 is a magic constant that I'm not sure why it works.
					int y = (int) ((ScrollOffset/(float) (_totalHeight/2.5))*(scrollArea.Height - scroll.DrawArea.Height)) +
					        up.DrawArea.Height;

					if (y < up.DrawAreaWithOffset.Height)
						y = up.DrawAreaWithOffset.Height + 1;
					else if (y > scrollArea.Height - scroll.DrawArea.Height)
					{
						y = scrollArea.Height - scroll.DrawArea.Height;
						if ((int) scroll.DrawLocation.Y == y)
						{
							ScrollOffset -= step;
							//undo the step if it is out of bounds so the text doesn't keep going after scroll has reached the bottom
						}
					}

					scroll.DrawLocation = new Vector2(0, y);
				}
					break;
				case Mode.LineByLineRender:
				{
					//_totalHeight contains the number of lines to render
					//7 or less shouldn't scroll
					if (_totalHeight <= Constants.NUM_LINES_RENDERED)
						return;

					if (btn == up)
					{
						if (ScrollOffset > 0)
							ScrollOffset--;
						else
							return;
					}
					else if (btn == down)
					{
						if (ScrollOffset < _totalHeight - Constants.NUM_LINES_RENDERED)
							ScrollOffset++;
						else
							return;
					}
					else
					{
						return;
					}

					float pixelsPerLine = (float)(scrollArea.Height - scroll.DrawArea.Height * 2) / (_totalHeight - Constants.NUM_LINES_RENDERED);
					scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scroll.DrawArea.Height + pixelsPerLine * ScrollOffset);
					if (scroll.DrawLocation.Y > scrollArea.Height - scroll.DrawArea.Height)
					{
						scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scrollArea.Height - scroll.DrawArea.Height);
					}
				}
					break;
			}
		}

		private void scrollDragged(object btn, EventArgs e)
		{
			int y = Mouse.GetState().Y - DrawAreaWithOffset.Y;

			if (y < up.DrawAreaWithOffset.Height)
				y = up.DrawAreaWithOffset.Height + 1;
			else if (y > scrollArea.Height - scroll.DrawArea.Height)
				y = scrollArea.Height - scroll.DrawArea.Height;

			scroll.DrawLocation = new Vector2(0, y);

			switch (_mode)
			{
				case Mode.LabelRender:
				{
					if (_totalHeight > drawArea.Height) //only scroll the actual text if it is larger than the drawArea
						ScrollOffset = (int)Math.Round((_totalHeight/2.5f)*((y - up.DrawArea.Height)/(float) (scrollArea.Height - scroll.DrawArea.Height)));
				}
					break;
				case Mode.LineByLineRender:
				{
					const int NUM_LINES_RENDERED = 7;

					if (_totalHeight <= NUM_LINES_RENDERED)
						return;
					
					double pixelsPerLine = (double)(scrollArea.Height - scroll.DrawArea.Height * 2) / (_totalHeight - NUM_LINES_RENDERED);
					ScrollOffset = (int)Math.Round((y - scroll.DrawArea.Height)/pixelsPerLine);
				}
					break;
			}
		}

		public override void Update(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible || (XNAControl.Dialogs.Count != 0 && XNAControl.Dialogs.Peek() != TopParent as XNADialog))
				return;

			//handle mouse wheel scrolling, but only if the cursor is over the parent control of the scroll bar
			MouseState currentState = Mouse.GetState();
			if (currentState.ScrollWheelValue != PreviousMouseState.ScrollWheelValue
				&& parent != null && parent.MouseOver && parent.MouseOverPreviously
				&& _mode == Mode.LineByLineRender
				&& _totalHeight > Constants.NUM_LINES_RENDERED)
			{
				int dif = (currentState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue) / 120;
				dif *= -1;//otherwise its that stupid-ass apple bullshit with the fucking natural scroll WHY IS IT EVEN A THING JESUS CHRIST APPLE
				if ((dif < 0 && dif + ScrollOffset >= 0) || (dif > 0 && ScrollOffset + dif <= _totalHeight - Constants.NUM_LINES_RENDERED))
				{
					ScrollOffset += dif;
					float pixelsPerLine = (float) (scrollArea.Height - scroll.DrawArea.Height*2)/
					                      (_totalHeight - Constants.NUM_LINES_RENDERED);
					scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scroll.DrawArea.Height + pixelsPerLine*ScrollOffset);
					if (scroll.DrawLocation.Y > scrollArea.Height - scroll.DrawArea.Height)
					{
						scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scrollArea.Height - scroll.DrawArea.Height);
					}
				}
			}
			
			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;
			base.Draw(gt);
		}
	}

	/// <summary>
	/// ScrollingDialog is a derived EODialog that has scrolling text like for account creation message
	/// Right now its pretty much just designed to do only that.
	/// </summary>
	public class EOScrollingDialog : EODialogBase
	{
		private readonly EOScrollBar scroll;

		public EOScrollingDialog(Game encapsulatingGame, string msgText)
			: base(encapsulatingGame)
		{
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 40);
			_setSize(bgTexture.Width, bgTexture.Height);

			message = new XNALabel(encapsulatingGame, new Rectangle(18, 57, 1, 1)) //label is auto-sized
			{
				Font = new System.Drawing.Font("Microsoft Sans Serif", 8.0f),
				ForeColor = System.Drawing.Color.FromArgb(255, 0xc8, 0xc8, 0xc8),
				Text = msgText,
				TextWidth = 293,
				Visible = false, //doesn't draw itself (hacky workaround in progress refactoring)
				RowSpacing = 4
			}; 
			message.SetParent(this);

			XNAButton ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(138, 197), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
			ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
			ok.SetParent(this);
			dlgButtons.Add(ok);

			scroll = new EOScrollBar(encapsulatingGame, this, new Vector2(320, 66), new Vector2(16, 119), EOScrollBar.ScrollColors.LightOnMed);

			base.endConstructor();
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			base.Draw(gt);

			scroll.UpdateDimensions(message.Texture.Height, (int)message.Font.GetHeight());
			Texture2D msg = message.Texture;
			Rectangle loc = new Rectangle(27 + (int)DrawLocation.X, 69 + (int)DrawLocation.Y, 293, 117);
			Rectangle src = new Rectangle(0, scroll.ScrollOffset, loc.Width, loc.Height);

			SpriteBatch.Begin();
			SpriteBatch.Draw(msg, loc, src, Color.White);
			SpriteBatch.End();
		}
	}

	/// <summary>
	/// Progress Bar dialog box that is shown to the user when their account creation is pending
	/// </summary>
	public class EOProgressDialog : EODialogBase
	{
		TimeSpan? timeOpened;
		Texture2D pbBackText, pbForeText;
		int pbWidth;

		public EOProgressDialog(Game encapsulatingGame, string msgText, string captionText = "")
			: base(encapsulatingGame)
		{
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 18);
			_setSize(bgTexture.Width, bgTexture.Height);

			message = new XNALabel(encapsulatingGame, new Rectangle(18, 57, 1, 1)); //label is auto-sized
			message.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.0f);
			message.ForeColor = System.Drawing.Color.FromArgb(255, 0xf0, 0xf0, 0xc8);
			message.Text = msgText;
			message.TextWidth = 254;
			message.SetParent(this);

			caption = new XNALabel(encapsulatingGame, new Rectangle(59, 23, 1, 1));
			caption.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.0f);
			caption.ForeColor = System.Drawing.Color.FromArgb(255, 0xf0, 0xf0, 0xc8);
			caption.Text = captionText;
			caption.SetParent(this);

			XNAButton ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
			ok.OnClick += (sender, e) => Close(ok, XNADialogResult.Cancel);
			ok.SetParent(this);
			dlgButtons.Add(ok);

			pbBackText = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 19);

			pbForeText = new Texture2D(encapsulatingGame.GraphicsDevice, 1, pbBackText.Height - 2); //foreground texture is just a fill
			Color[] pbForeFill = new Color[pbForeText.Width * pbForeText.Height];
			for (int i = 0; i < pbForeFill.Length; ++i)
				pbForeFill[i] = Color.FromNonPremultiplied(0xb4, 0xdc, 0xe6, 255);
			pbForeText.SetData(pbForeFill);

			base.endConstructor();
		}

		public override void Update(GameTime gt)
		{
			if (timeOpened == null) //set timeOpened on first call to Update
				timeOpened = gt.TotalGameTime;

			int pbPercent = (int)(((gt.TotalGameTime.TotalSeconds - timeOpened.Value.TotalSeconds) / 10.0f) * 100);

			pbWidth = (int)Math.Round((pbPercent / 100.0f) * pbBackText.Width);
			if (pbPercent >= 100)
				Close(null, XNADialogResult.NO_BUTTON_PRESSED);

			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			base.Draw(gt);

			SpriteBatch.Begin();
			SpriteBatch.Draw(pbBackText, new Vector2(15 + DrawAreaWithOffset.X, 95 + DrawAreaWithOffset.Y), Color.White);
			SpriteBatch.Draw(pbForeText, new Rectangle(18 + DrawAreaWithOffset.X, 98 + DrawAreaWithOffset.Y, pbWidth - 6, pbForeText.Height - 4), Color.White);
			SpriteBatch.End();
		}
	}

	public class EOChangePasswordDialog : EODialogBase
	{
		private readonly XNATextBox[] inputBoxes = new XNATextBox[4];
		private readonly KeyboardDispatcher dispatch;

		public string Username { get { return inputBoxes[0].Text; } }
		public string OldPassword { get { return inputBoxes[1].Text; } }
		public string NewPassword { get { return inputBoxes[2].Text; } }

		public EOChangePasswordDialog(Game encapsulatingGame, Texture2D cursorTexture, KeyboardDispatcher dispatcher)
			: base(encapsulatingGame)
		{
			dispatch = dispatcher;

			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 21);
			_setSize(bgTexture.Width, bgTexture.Height);

			for(int i = 0; i < inputBoxes.Length; ++i)
			{
				XNATextBox tb = new XNATextBox(encapsulatingGame, new Rectangle(198, 60 + i * 30, 137, 19), cursorTexture, "Microsoft Sans Serif", 8.0f)
				{
					LeftPadding = 5,
					DefaultText = " ",
					MaxChars = i == 0 ? 16 : 12,
					PasswordBox = i > 1,
					Selected = i == 0,
					TextColor = System.Drawing.Color.FromArgb(0xff, 0xdc, 0xc8, 0xb4),
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

				tb.Clicked += (s, e) =>
					{
						dispatch.Subscriber.Selected = false;
						dispatch.Subscriber = (s as XNATextBox);
						dispatcher.Subscriber.Selected = true;
					};

				tb.SetParent(this);
				inputBoxes[i] = tb;
			}

			dispatch.Subscriber = inputBoxes[0];

			XNAButton ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(157, 195), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28))
			{
				Visible = true
			};
			ok.OnClick += (s, e) =>
			{ //does some input validation before trying to call Close
				//check that all fields are filled in, otherwise: return
				if (inputBoxes.Any(tb => string.IsNullOrWhiteSpace(tb.Text))) return;

				if (Username != World.Instance.MainPlayer.AccountName)
				{
					EODialog errDlg = new EODialog(Game, "The username or password you specified is incorrect", "Wrong info");
					return;
				}
				
				//check that passwords match, otherwise: return
				if (inputBoxes[2].Text.Length != inputBoxes[3].Text.Length || inputBoxes[2].Text != inputBoxes[3].Text)
				{
					EODialog errDlg = new EODialog(Game, "The two passwords you provided are not the same, please try again.", "Wrong password");
					return;
				}
				
				//check that password is > 6 chars, otherwise: return
				if (inputBoxes[2].Text.Length < 6)
				{
					//Make sure passwords are good enough
					EODialog errDlg = new EODialog(Game, "For your own safety use a longer password (try 6 or more characters)", "Wrong password");
					return;
				}

				Close(ok, XNADialogResult.OK);
			};
			ok.SetParent(this);
			dlgButtons.Add(ok);

			XNAButton cancel = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(250, 194), new Rectangle(0, 28, 90, 28), new Rectangle(91, 28, 90, 28));
			cancel.Visible = true;
			cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
			cancel.SetParent(this);
			dlgButtons.Add(cancel);

			base.endConstructor();
		}
	}

	public class EOCreateCharacterDialog : EODialogBase
	{
		private readonly XNATextBox inputBox;
		private readonly XNAButton[] arrowButtons = new XNAButton[4];
		private readonly Rectangle[] srcRects = new Rectangle[4]; //these are rectangles for the sprite sheet with the different parameter colors (hair colors, styles, etc)
		private readonly EOCharacterRenderer charRender;

		public byte Gender { get { return charRender.Gender; } }
		public byte HairType { get { return charRender.HairType; } }
		public byte HairColor { get { return charRender.HairColor; } }
		public byte SkinColor { get { return charRender.SkinColor; } }
		public string Name { get { return inputBox.Text; } }

		private Rectangle rotClickArea;

		private readonly Texture2D charCreateSheet;

		public EOCreateCharacterDialog(Game encapsulatingGame, Texture2D cursorTexture, KeyboardDispatcher dispatcher)
			: base(encapsulatingGame)
		{
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 20);
			_setSize(bgTexture.Width, bgTexture.Height);

			charCreateSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 22);

			inputBox = new XNATextBox(encapsulatingGame, new Rectangle(80, 57, 138, 19), cursorTexture, "Microsoft Sans Serif", 8.0f);
			inputBox.LeftPadding = 5;
			inputBox.DefaultText = " ";
			inputBox.MaxChars = 12;
			inputBox.Selected = true;
			inputBox.TextColor = System.Drawing.Color.FromArgb(0xff, 0xdc, 0xc8, 0xb4);
			inputBox.Visible = true;
			inputBox.SetParent(this);
			dispatcher.Subscriber = inputBox;

			//four arrow buttons
			for(int i = 0; i < arrowButtons.Length; ++i)
			{
				XNAButton btn = new XNAButton(encapsulatingGame, charCreateSheet, new Vector2(196, 85 + i * 26), new Rectangle(185, 38, 19, 19), new Rectangle(206, 38, 19, 19));
				btn.Visible = true;
				btn.OnClick += ArrowButtonClick;
				btn.SetParent(this);
				arrowButtons[i] = btn;
			}

			charRender = new EOCharacterRenderer(encapsulatingGame, new Vector2(269, 83), new CharRenderData { gender = 0, hairstyle = 1, haircolor = 0, race = 0 }, false);
			charRender.SetParent(this);
			srcRects[0] = new Rectangle(0, 38, 23, 19);
			srcRects[1] = new Rectangle(0, 19, 23, 19);
			srcRects[2] = new Rectangle(0, 0, 23, 19);
			srcRects[3] = new Rectangle(46, 38, 23, 19);
			
			//ok/cancel buttons
			XNAButton ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(157, 195), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
			ok.Visible = true;
			ok.OnClick += (s, e) =>
			{
				if(inputBox.Text.Length < 4)
				{
					EODialog fail = new EODialog(encapsulatingGame, "The name you provided for this character is too short (try 4 or more characters)", "Wrong name");
					return;
				}

				Close(ok, XNADialogResult.OK);
			};
			ok.SetParent(this);
			dlgButtons.Add(ok);

			XNAButton cancel = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(250, 194), new Rectangle(0, 28, 90, 28), new Rectangle(91, 28, 90, 28));
			cancel.Visible = true;
			cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
			cancel.SetParent(this);
			dlgButtons.Add(cancel);

			base.endConstructor();
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
			if ((XNAControl.Dialogs.Count > 0 && XNAControl.Dialogs.Peek() != this) || !Visible)
				return;

			rotClickArea = new Rectangle(235 + DrawAreaWithOffset.X, 58 + DrawAreaWithOffset.Y, 99, 123);

			if(((Mouse.GetState().LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed) ||
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

	public class EOConnectingDialog : EODialogBase
	{
		//different captions as the dialog progresses through different states
		const string wait = "Please wait";
		const string map = "Updating map";
		const string item = "Updating items";
		const string npc = "Updating npc's";
		const string skill = "Updating skills";
		const string classes = "Updating classes";
		const string loading = "Loading game";

		private readonly Texture2D bgSprites;
		private int bgSrcIndex;
		private TimeSpan? timeOpened;

		private bool updatingFiles;

		public EOConnectingDialog(Game encapsulatingGame)
			: base (encapsulatingGame)
		{
			bgTexture = null; //don't use the built in bgtexture, we're going to use a sprite sheet for the BG
			bgSprites = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 33);
			DrawLocation = new Vector2(Game.GraphicsDevice.PresentationParameters.BackBufferWidth - (bgSprites.Width / 4) - 10, 
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight - bgSprites.Height - 10);
			_setSize(bgSprites.Width / 4, bgSprites.Height);
			bgSrcIndex = 0;

			caption = new XNALabel(Game, new Rectangle(12, 9, 1, 1), "Microsoft Sans Serif", 10.0f);
			caption.Text = wait;
			caption.ForeColor = System.Drawing.Color.FromArgb(0xf0, 0xf0, 0xc8);
			caption.SetParent(this);

			message = new XNALabel(Game, new Rectangle(18, 61, 1, 1), "Microsoft Sans Serif", 8.0f);
			message.TextWidth = 175; //there is a constraint on the size for this
			message.ForeColor = System.Drawing.Color.FromArgb(0xb9, 0xb9, 0xb9);
			//there are a number of messages that are shown, a static one will do for now
			message.Text = "Make sure noone is watching your keyboard, while entering your password";
			message.SetParent(this);

			base.endConstructor(false);
		}

		public override void Update(GameTime gt)
		{
			if (timeOpened == null)
				timeOpened = gt.TotalGameTime;

			if(((int)gt.TotalGameTime.TotalMilliseconds - (int)(timeOpened.Value.TotalMilliseconds)) % 750 == 0) //every 750msec
			{
				//switch the background image to the next one
				bgSrcIndex = bgSrcIndex == 3 ? 0 : bgSrcIndex + 1;
			}

			if (!updatingFiles && ((int)gt.TotalGameTime.TotalSeconds - (int)(timeOpened.Value.TotalSeconds)) >= 5) //I think the client waits 5 seconds?
			{
				updatingFiles = true;

				//I hate putting this on a new thread but otherwise the Update call would block while this is all happening...meaning the dialog would freeze
				new Thread(() =>
				{
					if (World.Instance.NeedMap != -1)
					{
						caption.Text = map;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Map))
						{
							Close(null, XNADialogResult.NO_BUTTON_PRESSED);
							return;
						}
						Thread.Sleep(1000); //computers are fast: I think the actual client sleeps at this point in its logic too because there is no way it should take as long as it does
					}

					if (World.Instance.NeedEIF)
					{
						caption.Text = item;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Item))
						{
							Close(null, XNADialogResult.NO_BUTTON_PRESSED);
							return;
						}
						Thread.Sleep(1000);
					}

					if (World.Instance.NeedENF)
					{
						caption.Text = npc;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Npc))
						{
							Close(null, XNADialogResult.NO_BUTTON_PRESSED);
							return;
						}
						Thread.Sleep(1000);
					}

					if (World.Instance.NeedESF)
					{
						caption.Text = skill;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Spell))
						{
							Close(null, XNADialogResult.NO_BUTTON_PRESSED);
							return;
						}
						Thread.Sleep(1000);
					}

					if (World.Instance.NeedECF)
					{
						caption.Text = classes;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Class))
						{
							Close(null, XNADialogResult.NO_BUTTON_PRESSED);
							return;
						}
						Thread.Sleep(1000);
					}

					caption.Text = loading;
					if(!Handlers.Welcome.WelcomeMessage(World.Instance.MainPlayer.ActiveCharacter.ID))
					{
						Close(null, XNADialogResult.NO_BUTTON_PRESSED);
						return;
					}
					Thread.Sleep(1000);

					Close(null, XNADialogResult.OK); //using OK here to mean everything was successful. NO_BUTTON_PRESSED means unsuccessful.
				}).Start();
			}

			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			SpriteBatch.Begin();
			SpriteBatch.Draw(bgSprites, DrawAreaWithOffset, new Rectangle(bgSrcIndex * (bgSprites.Width / 4), 0, bgSprites.Width / 4, bgSprites.Height), Color.White);
			SpriteBatch.End();

			base.Draw(gt);
		}
	}
}
