using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

#pragma warning disable 162

namespace EndlessClient
{
	public class EODialogBase : XNADialog
	{
		protected Texture2D smallButtonSheet;

		protected EODialogBase(Game encapsulatingGame)
			: base(encapsulatingGame)
		{
			smallButtonSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 15, true);
		}

		protected void endConstructor()
		{
			//center dialog based on txtSize of background texture
			Center(Game.GraphicsDevice);
			_fixDrawOrder();
			XNAControl.ModalDialogs.Push(this);

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
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 18, false);
			_setSize(bgTexture.Width, bgTexture.Height);
			
			message = new XNALabel(encapsulatingGame, new Rectangle(18, 57, 1, 1)); //label is auto-sized
			message.Font = new System.Drawing.Font("Arial", 10.0f);
			message.ForeColor = System.Drawing.Color.FromArgb(255, 0xf0, 0xf0, 0xc8);
			message.Text = msgText;
			message.TextWidth = 254;
			message.SetParent(this);

			caption = new XNALabel(encapsulatingGame, new Rectangle(59, 23, 1, 1));
			caption.Font = new System.Drawing.Font("Arial", 10.0f);
			caption.ForeColor = System.Drawing.Color.FromArgb(255, 0xf0, 0xf0, 0xc8);
			caption.Text = captionText;
			caption.SetParent(this);

			XNAButton ok, cancel;
			switch(whichButtons)
			{
				case XNADialogButtons.Ok:
					ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
					ok.OnClick += (object sender, EventArgs e) => { Close(); };
					ok.SetParent(this);

					cancel = null;

					dlgButtons.Add(ok);
					break;
				case XNADialogButtons.Cancel:
					cancel = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
					cancel.OnClick += (object sender, EventArgs e) => { Close(true); };
					cancel.SetParent(this);

					ok = null;

					dlgButtons.Add(cancel);
					break;
				case XNADialogButtons.OkCancel:
					//implement this more fully when it is needed
					//update draw location of ok button to be on left?
					ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(89, 113), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
					ok.OnClick += (object sender, EventArgs e) => { Close(true); };
					ok.SetParent(this);

					cancel = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
					cancel.OnClick += (s, e) => { Close(); };
					cancel.SetParent(this);

					dlgButtons.Add(ok);
					dlgButtons.Add(cancel);
					break;
			}

			base.endConstructor();
		}

		public void Close(bool okButtonWasPressed)
		{
			Close();

			if (CloseAction != null)
				CloseAction(true);
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
		public int ScrollOffset { get; set; }

		private XNAButton up, down, scroll; //buttons

		private int _rowHeight, _totalHeight;

		private Texture2D scrollSpriteSheet;
		
		public EOScrollBar(Game encapsulatingGame, EOScrollingDialog parent, Vector2 relativeLoc, Vector2 size, ScrollColors palette)
			: base(encapsulatingGame, relativeLoc, new Rectangle((int)relativeLoc.X, (int)relativeLoc.Y, (int)size.X, (int)size.Y))
		{
			SetParent(parent);
			scrollArea = new Rectangle(0, 15, 0, (int)size.Y - 15);
			DrawLocation = relativeLoc;
			ScrollOffset = 0;

			scrollSpriteSheet = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 29, false);
			Rectangle[] upArrows = new Rectangle[2];
			Rectangle[] downArrows = new Rectangle[2];
			Rectangle scrollBox;
			int vertOff;
			switch (palette)
			{
				case ScrollColors.LightOnLight: vertOff = 0; break;
				case ScrollColors.LightOnMed: vertOff = 105; break;
				case ScrollColors.LightOnDark: vertOff = 180; break;
				case ScrollColors.DarkOnDark: vertOff = 255; break;
				default:
					throw new ArgumentOutOfRangeException("Unrecognized palette!");
			}

			//regions based on verticle offset (which is based on the chosen palette)
			upArrows[0] = new Rectangle(0, vertOff + 15 * 3, 16, 15);
			upArrows[1] = new Rectangle(0, vertOff + 15 * 4, 16, 15);
			downArrows[0] = new Rectangle(0, vertOff + 15, 16, 15);
			downArrows[1] = new Rectangle(0, vertOff + 15 * 2, 16, 15);
			scrollBox = new Rectangle(0, vertOff, 16, 15);

			Texture2D[] upButton = new Texture2D[2];
			Texture2D[] downButton = new Texture2D[2];
			Texture2D[] scrollButton = new Texture2D[2];
			for (int i = 0; i < 2; ++i)
			{
				upButton[i] = new Texture2D(scrollSpriteSheet.GraphicsDevice, upArrows[i].Width, upArrows[i].Height);
				Color[] upData = new Color[upArrows[i].Width * upArrows[i].Height];
				scrollSpriteSheet.GetData<Color>(0, upArrows[i], upData, 0, upData.Length);
				upButton[i].SetData<Color>(upData);

				downButton[i] = new Texture2D(scrollSpriteSheet.GraphicsDevice, downArrows[i].Width, downArrows[i].Height);
				Color[] downData = new Color[downArrows[i].Width * downArrows[i].Height];
				scrollSpriteSheet.GetData<Color>(0, downArrows[i], downData, 0, downData.Length);
				downButton[i].SetData<Color>(downData);

				//same texture for hover, AFAIK
				scrollButton[i] = new Texture2D(scrollSpriteSheet.GraphicsDevice, scrollBox.Width, scrollBox.Height);
				Color[] scrollData = new Color[scrollBox.Width * scrollBox.Height];
				scrollSpriteSheet.GetData<Color>(0, scrollBox, scrollData, 0, scrollData.Length);
				scrollButton[i].SetData<Color>(scrollData);
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
		}

		public void UpdateDimensions(int totalHeight, int rowHeight)
		{
			_totalHeight = totalHeight;
			_rowHeight = rowHeight;
		}

		//the point of arrowClicked and scrollDragged is to respond to input on the three buttons in such
		//	 a way that ScrollOffset is updated and the Y coordinate for the scroll box is updated.
		//	 ScrollOffset provides a value that is used within the EOScrollDialog.Draw method.
		//	 The Y coordinate for the scroll box determines where it is drawn.
		private void arrowClicked(object btn, EventArgs e)
		{
			int step = (_totalHeight / 4) / _rowHeight;
						
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
			
			//update the y coordinate of the scroll button
			int y = (int)((ScrollOffset / (float)(_totalHeight / 8)) * (scrollArea.Height - scroll.DrawArea.Height)) + up.DrawArea.Height;

			if (y < up.DrawAreaWithOffset.Height)
				y = up.DrawAreaWithOffset.Height + 1;
			else if (y > scrollArea.Height - scroll.DrawArea.Height)
				y = scrollArea.Height - scroll.DrawArea.Height;

			scroll.DrawLocation = new Vector2(0, y);
		}

		private void scrollDragged(object btn, EventArgs e)
		{
			int y = Mouse.GetState().Y - DrawAreaWithOffset.Y;

			if (y < up.DrawAreaWithOffset.Height)
				y = up.DrawAreaWithOffset.Height + 1;
			else if (y > scrollArea.Height - scroll.DrawArea.Height)
				y = scrollArea.Height - scroll.DrawArea.Height;

			scroll.DrawLocation = new Vector2(0, y);

			ScrollOffset = (int)Math.Round(((float)_totalHeight / 8.0f) * ((y - up.DrawArea.Height) / (float)(scrollArea.Height - scroll.DrawArea.Height)));
		}

		public override void Update(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible || (XNAControl.ModalDialogs.Count != 0 && XNAControl.ModalDialogs.Peek() != TopParent as XNADialog))
				return;
			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;
			base.Update(gt);
		}
	}

	/// <summary>
	/// ScrollingDialog is a derived EODialog that has scrolling text like for account creation message
	/// Right now its pretty much just designed to do only that.
	/// </summary>
	public class EOScrollingDialog : EODialogBase
	{
		private EOScrollBar scroll;

		public EOScrollingDialog(Game encapsulatingGame, string msgText, string captionText = "")
			: base(encapsulatingGame)
		{
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 40, false);
			_setSize(bgTexture.Width, bgTexture.Height);

			message = new XNALabel(encapsulatingGame, new Rectangle(18, 57, 1, 1)); //label is auto-sized
			message.Font = new System.Drawing.Font("Arial", 8.0f);
			message.ForeColor = System.Drawing.Color.FromArgb(255, 0xc8, 0xc8, 0xc8);
			message.Text = msgText;
			message.TextWidth = 293;
			message.SetParent(this);
			message.Visible = false; //the label doesn't handle its own drawing/updating: the dialog does that so text is clipped
			message.RowSpacing = 4;

			XNAButton ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(138, 197), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
			ok.OnClick += (sender, e) => { Close(); };
			ok.SetParent(this);
			dlgButtons.Add(ok);

			scroll = new EOScrollBar(encapsulatingGame, this, new Vector2(320, 66), new Vector2(16, 119), EOScrollBar.ScrollColors.LightOnMed);

			base.endConstructor();
		}

		public override void Draw(GameTime gt)
		{
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
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 18, false);
			_setSize(bgTexture.Width, bgTexture.Height);

			message = new XNALabel(encapsulatingGame, new Rectangle(18, 57, 1, 1)); //label is auto-sized
			message.Font = new System.Drawing.Font("Arial", 10.0f);
			message.ForeColor = System.Drawing.Color.FromArgb(255, 0xf0, 0xf0, 0xc8);
			message.Text = msgText;
			message.TextWidth = 254;
			message.SetParent(this);

			caption = new XNALabel(encapsulatingGame, new Rectangle(59, 23, 1, 1));
			caption.Font = new System.Drawing.Font("Arial", 10.0f);
			caption.ForeColor = System.Drawing.Color.FromArgb(255, 0xf0, 0xf0, 0xc8);
			caption.Text = captionText;
			caption.SetParent(this);

			XNAButton ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
			ok.OnClick += (sender, e) => { Close(false); };
			ok.SetParent(this);
			dlgButtons.Add(ok);

			pbBackText = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 19, false);

			pbForeText = new Texture2D(encapsulatingGame.GraphicsDevice, 1, pbBackText.Height - 2); //foreground texture is just a fill
			Color[] pbForeFill = new Color[pbForeText.Width * pbForeText.Height];
			for (int i = 0; i < pbForeFill.Length; ++i)
				pbForeFill[i] = Color.FromNonPremultiplied(0xb4, 0xdc, 0xe6, 255);
			pbForeText.SetData<Color>(pbForeFill);

			base.endConstructor();
		}

		public override void Update(GameTime gt)
		{
			if (timeOpened == null) //set timeOpened on first call to Update
				timeOpened = gt.TotalGameTime;

			int pbPercent = (int)(((gt.TotalGameTime.TotalSeconds - timeOpened.Value.TotalSeconds) / 10.0f) * 100);

			pbWidth = (int)Math.Round((pbPercent / 100.0f) * pbBackText.Width);
			if (pbPercent >= 100)
				Close(true);

			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			base.Draw(gt);

			SpriteBatch.Begin();
			SpriteBatch.Draw(pbBackText, new Vector2(15 + DrawAreaWithOffset.X, 95 + DrawAreaWithOffset.Y), Color.White);
			SpriteBatch.Draw(pbForeText, new Rectangle(18 + DrawAreaWithOffset.X, 98 + DrawAreaWithOffset.Y, pbWidth - 6, pbForeText.Height - 4), Color.White);
			SpriteBatch.End();
		}

		//called with parameter finished=true when the progress bar reaches 100%
		public void Close(bool finished)
		{
			this.Close();

			if (CloseAction != null)
				CloseAction(finished);
		}
	}

	public class EOChangePasswordDialog : EODialogBase
	{
		private XNATextBox[] inputBoxes = new XNATextBox[4];
		private KeyboardDispatcher dispatch;

		public string Username { get { return inputBoxes[0].Text; } }
		public string OldPassword { get { return inputBoxes[1].Text; } }
		public string NewPassword { get { return inputBoxes[2].Text; } }

		public EOChangePasswordDialog(Game encapsulatingGame, Texture2D cursorTexture, KeyboardDispatcher dispatcher)
			: base(encapsulatingGame)
		{
			dispatch = dispatcher;

			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 21, false);
			_setSize(bgTexture.Width, bgTexture.Height);

			for(int i = 0; i < inputBoxes.Length; ++i)
			{
				XNATextBox tb = new XNATextBox(encapsulatingGame, new Rectangle(198, 60 + i * 30, 137, 19), cursorTexture, "Arial", 8.0f);
				tb.LeftPadding = 5;
				tb.DefaultText = " ";
				tb.MaxChars = i == 0 ? 16 : 12;
				tb.PasswordBox = i > 1;
				tb.Selected = i == 0;
				tb.TextColor = System.Drawing.Color.FromArgb(0xff, 0xdc, 0xc8, 0xb4);
				tb.Visible = true;

				tb.OnTabPressed += (s, e) =>
				{
					List<XNATextBox> list = inputBoxes.ToList();
					int tbIndex = list.FindIndex((txt) => { return txt == s; });

					int next = tbIndex + 1 > 3 ? 0 : tbIndex + 1;
					inputBoxes[tbIndex].Selected = false;
					inputBoxes[next].Selected = true;
					dispatch.Subscriber = inputBoxes[next];
				};

				tb.Clicked += (s, e) =>
					{
						dispatch.Subscriber.Selected = false;
						dispatch.Subscriber = (s as XNATextBox);
						(s as XNATextBox).Selected = true;
					};

				tb.SetParent(this);
				inputBoxes[i] = tb;
			}

			dispatch.Subscriber = inputBoxes[0];

			XNAButton ok = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(157, 195), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
			ok.Visible = true;
			ok.OnClick += (s, e) =>
			{
				//if account doesn't match logged in account: return (TODO)

				//check that all fields are filled in, otherwise: return
				foreach (XNATextBox tb in inputBoxes)
				{
					if (string.IsNullOrWhiteSpace(tb.Text))
						return;
				}

				//check that passwords match, otherwise: return
				if (inputBoxes[2].Text.Length != inputBoxes[3].Text.Length || inputBoxes[2].Text != inputBoxes[3].Text)
				{
					EODialog errDlg = new EODialog(encapsulatingGame, "The two passwords you provided are not the same, please try again.", "Wrong password");
					return;
				} //check that password is > 6 chars, otherwise: return
				else if (inputBoxes[2].Text.Length < 6)
				{
					//Make sure passwords are good enough
					EODialog errDlg = new EODialog(encapsulatingGame, "For your own safety use a longer password (try 6 or more characters)", "Wrong password");
					return;
				}

				if (!Handlers.Account.AccountChangePassword(Username, OldPassword, NewPassword))
				{
					Close(true); //signal the game to return to the initial state

					EODialog errDlg = new EODialog(encapsulatingGame, "The connection to the game server was lost, please try again at a later time.", "Lost connection");
					if (World.Instance.Client.Connected)
						World.Instance.Client.Disconnect();
					return;
				}

				string caption, msg = Handlers.Account.ResponseMessage(out caption);
				if (!Handlers.Account.CanProceed)
				{
					EODialog response = new EODialog(encapsulatingGame, msg, caption);
					return;
				}
				else
				{
					Close();
					EODialog response = new EODialog(encapsulatingGame, msg, caption);
				}
			};
			ok.SetParent(this);
			dlgButtons.Add(ok);

			XNAButton cancel = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(250, 194), new Rectangle(0, 28, 90, 28), new Rectangle(91, 28, 90, 28));
			cancel.Visible = true;
			cancel.OnClick += (s, e) => { Close(); };
			cancel.SetParent(this);
			dlgButtons.Add(cancel);

			base.endConstructor();
		}

		public void Close(bool finished)
		{
			if (CloseAction != null)
				CloseAction(finished);
			Close();
		}
	}

	public class EOCreateCharacterDialog : EODialogBase
	{
		private XNATextBox inputBox;
		private KeyboardDispatcher dispatch;
		private XNAButton[] arrowButtons = new XNAButton[4];
		private Rectangle[] srcRects = new Rectangle[4]; //these are rectangles for the sprite sheet with the different parameter colors (hair colors, styles, etc)
		private EOCharacterRenderer charRender;

		private Rectangle rotClickArea;

		private Texture2D charCreateSheet;

		public EOCreateCharacterDialog(Game encapsulatingGame, Texture2D cursorTexture, KeyboardDispatcher dispatcher)
			: base(encapsulatingGame)
		{
			dispatch = dispatcher;

			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 20, false);
			_setSize(bgTexture.Width, bgTexture.Height);

			charCreateSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 22, false);

			inputBox = new XNATextBox(encapsulatingGame, new Rectangle(80, 57, 138, 19), cursorTexture, "Arial", 8.0f);
			inputBox.LeftPadding = 5;
			inputBox.DefaultText = " ";
			inputBox.MaxChars = 12;
			inputBox.Selected = true;
			inputBox.TextColor = System.Drawing.Color.FromArgb(0xff, 0xdc, 0xc8, 0xb4);
			inputBox.Visible = true;
			inputBox.SetParent(this);
			dispatch.Subscriber = inputBox;

			//four arrow buttons
			for(int i = 0; i < arrowButtons.Length; ++i)
			{
				XNAButton btn = new XNAButton(encapsulatingGame, charCreateSheet, new Vector2(196, 85 + i * 26), new Rectangle(185, 38, 19, 19), new Rectangle(206, 38, 19, 19));
				btn.Visible = true;
				btn.OnClick += ArrowButtonClick;
				btn.SetParent(this);
				arrowButtons[i] = btn;
			}

			charRender = new EOCharacterRenderer(encapsulatingGame, new Vector2(269, 83), new CharRenderData() { gender = 0, hairstyle = 1, haircolor = 0, race = 0 }, false);
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

				if(!Handlers.Character.CharacterCreate(charRender.Gender, charRender.HairType, charRender.HairColor, charRender.SkinColor, inputBox.Text))
				{
					Close(true); //tell game to reset to inital state
					EODialog errDlg = new EODialog(encapsulatingGame, "The connection to the game server was lost, please try again at a later time.", "Lost connection");
					if (World.Instance.Client.Connected)
						World.Instance.Client.Disconnect();
					return;
				}

				//validate character name w/ server (todo)
				if(!Handlers.Character.CanProceed)
				{
					if (Handlers.Character.TooManyCharacters)
						Close();
					string caption, message = Handlers.Character.ResponseMessage(out caption);
					EODialog fail = new EODialog(encapsulatingGame, message, caption);
					return;
				}
				
				Close();
				EODialog dlg = new EODialog(encapsulatingGame, "Your character has been created and is ready to explore a new world.", "Character created");
			};
			ok.SetParent(this);
			dlgButtons.Add(ok);

			XNAButton cancel = new XNAButton(encapsulatingGame, smallButtonSheet, new Vector2(250, 194), new Rectangle(0, 28, 90, 28), new Rectangle(91, 28, 90, 28));
			cancel.Visible = true;
			cancel.OnClick += (s, e) => { Close(); };
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
			if (ModalDialogs.Count > 0 && ModalDialogs.Peek() != this)
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
			base.Draw(gt);

			SpriteBatch.Begin();

			SpriteBatch.Draw(charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 84 + DrawAreaWithOffset.Y), srcRects[0], Color.White);
			SpriteBatch.Draw(charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 111 + DrawAreaWithOffset.Y), srcRects[1], Color.White);
			SpriteBatch.Draw(charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 138 + DrawAreaWithOffset.Y), srcRects[2], Color.White);
			SpriteBatch.Draw(charCreateSheet, new Vector2(170 + DrawAreaWithOffset.X, 165 + DrawAreaWithOffset.Y), srcRects[3], Color.White);

			SpriteBatch.End();
		}

		public void Close(bool finished)
		{
			if (CloseAction != null)
				CloseAction(true);
			base.Close();
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

		Texture2D bgSprites;
		int bgSrcIndex;
		TimeSpan? timeOpened = null;

		bool updatingFiles = false;

		public EOConnectingDialog(Game encapsulatingGame)
			: base (encapsulatingGame)
		{
			bgTexture = null; //don't use the built in bgtexture, we're going to use a sprite sheet for the BG
			bgSprites = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 33, false);
			this.DrawLocation = new Vector2(Game.GraphicsDevice.PresentationParameters.BackBufferWidth - (bgSprites.Width / 4) - 10, 
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight - bgSprites.Height - 10);
			_setSize(bgSprites.Width / 4, bgSprites.Height);
			bgSrcIndex = 0;

			caption = new XNALabel(Game, new Rectangle(12, 9, 1, 1), "Arial", 10.0f);
			caption.Text = wait;
			caption.ForeColor = System.Drawing.Color.FromArgb(0xf0, 0xf0, 0xc8);
			caption.SetParent(this);

			message = new XNALabel(Game, new Rectangle(18, 61, 1, 1), "Arial", 8.0f);
			message.TextWidth = 175; //there is a constraint on the size for this
			message.ForeColor = System.Drawing.Color.FromArgb(0xb9, 0xb9, 0xb9);
			//there are a number of messages that are shown, a static one will do for now
			message.Text = "Make sure noone is watching your keyboard, while entering your password";
			message.SetParent(this);


			//normally would call base.endConstructor();
			_fixDrawOrder();
			XNAControl.ModalDialogs.Push(this);
			Game.Components.Add(this);
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

				new Thread(new ThreadStart(() =>
				{
					Console.WriteLine("Entering thread...");

					if (World.Instance.NeedMap != -1)
					{
						caption.Text = map;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Map))
						{
							Close();
							return;
						}
						Thread.Sleep(1000); //computers are fast
					}

					if (World.Instance.NeedEIF)
					{
						caption.Text = item;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Item))
						{
							Close();
							return;
						}
						Thread.Sleep(1000);
					}

					if (World.Instance.NeedENF)
					{
						caption.Text = npc;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Npc))
						{
							Close();
							return;
						}
						Thread.Sleep(1000);
					}

					if (World.Instance.NeedESF)
					{
						caption.Text = skill;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Spell))
						{
							Close();
							return;
						}
						Thread.Sleep(1000);
					}

					if (World.Instance.NeedECF)
					{
						caption.Text = classes;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Class))
						{
							Close();
							return;
						}
						Thread.Sleep(1000);
					}

					caption.Text = loading;
					if(!Handlers.Welcome.WelcomeMessage(World.Instance.MainPlayer.ActiveCharacter.ID))
					{
						Close();
						return;
					}
					Thread.Sleep(1000);

					Close();
					CloseAction(true);
				})).Start();
			}

			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			base.Draw(gt);

			SpriteBatch.Begin();
			SpriteBatch.Draw(bgSprites, DrawAreaWithOffset, new Rectangle(bgSrcIndex * (bgSprites.Width / 4), 0, bgSprites.Width / 4, bgSprites.Height), Color.White);
			SpriteBatch.End();
		}
	}
}
