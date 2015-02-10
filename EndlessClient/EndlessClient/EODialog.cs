using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using EOLib;
using EOLib.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

#pragma warning disable 162

namespace EndlessClient
{
	public class EODialogBase : XNADialog
	{
		protected readonly Texture2D smallButtonSheet;

		protected EODialogBase()
		{
			smallButtonSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 15, true);
		}

		protected void endConstructor(bool centerDialog = true)
		{
			//center dialog based on txtSize of background texture
			if (centerDialog)
				Center(Game.GraphicsDevice);
			_fixDrawOrder();
			Dialogs.Push(this);

			Game.Components.Add(this);
		}
	}

	/// <summary>
	/// EODialog is a basic dialog representation (like Windows MessageBox)
	/// </summary>
	public class EODialog : EODialogBase
	{
		public EODialog(string msgText, string captionText = "", XNADialogButtons whichButtons = XNADialogButtons.Ok, bool useSmallHeader = false)
		{
			base.whichButtons = whichButtons;

			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, useSmallHeader ? 23 : 18);
			_setSize(bgTexture.Width, bgTexture.Height);
			
			message = new XNALabel(new Rectangle(18, 57, 1, 1), "Microsoft Sans Serif", 10.0f); //label is auto-sized
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

			caption = new XNALabel(new Rectangle(59, 23, 1, 1), "Microsoft Sans Serif", 10.0f);
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
					ok = new XNAButton(smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
					ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
					ok.SetParent(this);
					dlgButtons.Add(ok);
					break;
				case XNADialogButtons.Cancel:
					cancel = new XNAButton(smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
					cancel.OnClick += (sender, e) => Close(cancel, XNADialogResult.Cancel);
					cancel.SetParent(this);
					dlgButtons.Add(cancel);
					break;
				case XNADialogButtons.OkCancel:
					//implement this more fully when it is needed
					//update draw location of ok button to be on left?
					ok = new XNAButton(smallButtonSheet, new Vector2(89, 113), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
					ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
					ok.SetParent(this);

					cancel = new XNAButton(smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
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

			endConstructor();
		}

		public static void Show(string message, string caption = "", XNADialogButtons buttons = XNADialogButtons.Ok, bool SmallHeader = false)
		{
// ReSharper disable once UnusedVariable
			EODialog dlg = new EODialog(message, caption, buttons, SmallHeader);
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
		public int LinesToRender { get; set; }

		private readonly XNAButton up, down, scroll; //buttons

		private int _totalHeight;
		
		public EOScrollBar(XNAControl parent, Vector2 relativeLoc, Vector2 size, ScrollColors palette)
			: base(relativeLoc, new Rectangle((int)relativeLoc.X, (int)relativeLoc.Y, (int)size.X, (int)size.Y))
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

			up = new XNAButton(upButton, new Vector2(0, 0));
			up.OnClick += arrowClicked;
			up.SetParent(this);
			down = new XNAButton(downButton, new Vector2(0, size.Y - 15)); //update coordinates!!!!
			down.OnClick += arrowClicked;
			down.SetParent(this);
			scroll = new XNAButton(scrollButton, new Vector2(0, 15)); //update coordinates!!!!
			scroll.OnClickDrag += scrollDragged;
			scroll.SetParent(this);

			_totalHeight = DrawAreaWithOffset.Height;
		}
		
		public void UpdateDimensions(int numberOfLines)
		{
			_totalHeight = numberOfLines;
		}

		public void ScrollToEnd()
		{
			while(ScrollOffset < _totalHeight - LinesToRender)
				arrowClicked(down, new EventArgs());
		}

		public void SetDownArrowFlashSpeed(int milliseconds)
		{
			down.FlashSpeed = milliseconds;
		}

		//the point of arrowClicked and scrollDragged is to respond to input on the three buttons in such
		//	 a way that ScrollOffset is updated and the Y coordinate for the scroll box is updated.
		//	 ScrollOffset provides a value that is used within the EOScrollDialog.Draw method.
		//	 The Y coordinate for the scroll box determines where it is drawn.
		private void arrowClicked(object btn, EventArgs e)
		{
			//_totalHeight contains the number of lines to render
			//any less than LinesToRender shouldn't scroll
			if (_totalHeight <= LinesToRender)
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
				if(down.FlashSpeed.HasValue)
					down.FlashSpeed = null; //as soon as it is clicked, stop flashing

				if (ScrollOffset < _totalHeight - LinesToRender)
					ScrollOffset++;
				else
					return;
			}
			else
			{
				return;
			}

			float pixelsPerLine = (float) (scrollArea.Height - scroll.DrawArea.Height*2)/(_totalHeight - LinesToRender);
			scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scroll.DrawArea.Height + pixelsPerLine*ScrollOffset);
			if (scroll.DrawLocation.Y > scrollArea.Height - scroll.DrawArea.Height)
			{
				scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scrollArea.Height - scroll.DrawArea.Height);
			}
		}

		private void scrollDragged(object btn, EventArgs e)
		{
			if (down.FlashSpeed.HasValue)
				down.FlashSpeed = null; //as soon as we are dragged, stop flashing

			int y = Mouse.GetState().Y - (DrawAreaWithOffset.Y + scroll.DrawArea.Height / 2);

			if (y < up.DrawAreaWithOffset.Height)
				y = up.DrawAreaWithOffset.Height + 1;
			else if (y > scrollArea.Height - scroll.DrawArea.Height)
				y = scrollArea.Height - scroll.DrawArea.Height;

			scroll.DrawLocation = new Vector2(0, y);
			if (_totalHeight <= LinesToRender)
				return;

			double pixelsPerLine = (double) (scrollArea.Height - scroll.DrawArea.Height*2)/(_totalHeight - LinesToRender);
			ScrollOffset = (int) Math.Round((y - scroll.DrawArea.Height)/pixelsPerLine);
		}

		public override void Update(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible || (Dialogs.Count != 0 && Dialogs.Peek() != TopParent as XNADialog))
				return;

			//handle mouse wheel scrolling, but only if the cursor is over the parent control of the scroll bar
			MouseState currentState = Mouse.GetState();

			//scroll wheel will only work for news because it is constructed with a panel as the parent
			//so for all other tabs, need to get the tab that it is being rendered in for mouseover to work properly
			XNAControl tempParent;
			if (parent is EOChatRenderer)
				tempParent = parent.GetParent();
			else
				tempParent = parent;

			if (currentState.ScrollWheelValue != PreviousMouseState.ScrollWheelValue
				&& tempParent != null && tempParent.MouseOver && tempParent.MouseOverPreviously
				&& _totalHeight > LinesToRender)
			{
				int dif = (currentState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue) / 120;
				dif *= -1;//otherwise its that stupid-ass apple bullshit with the fucking natural scroll WHY IS IT EVEN A THING WHAT THE FUCK APPLE
				if ((dif < 0 && dif + ScrollOffset >= 0) || (dif > 0 && ScrollOffset + dif <= _totalHeight - LinesToRender))
				{
					ScrollOffset += dif;
					float pixelsPerLine = (float) (scrollArea.Height - scroll.DrawArea.Height*2)/
										  (_totalHeight - LinesToRender);
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
		private readonly EOScrollBar scrollBar;
		private readonly List<string> chatStrings = new List<string>();

		private readonly SpriteFont font;
		private const int LINE_LEN = 275;

		//private string _msg;
		public new string MessageText
		{
			//not in use: commenting out
			//get { return _msg; }
			set
			{
				chatStrings.Clear();
				string tmp = value;

				//special case: blank line, like in the news panel between news items
				if (string.IsNullOrWhiteSpace(tmp))
				{
					chatStrings.Add(" ");
					scrollBar.UpdateDimensions(chatStrings.Count);
					return;
				}

				//don't do multi-line processing if we don't need to
				if (font.MeasureString(tmp).X < LINE_LEN)
				{
					chatStrings.Add(tmp);
					scrollBar.UpdateDimensions(chatStrings.Count);
					return;
				}

				string buffer = tmp, newLine = "";

				List<string> chatStringsToAdd = new List<string>();
				char[] whiteSpace = { ' ', '\t', '\n' };
				string nextWord = "";
				while (buffer.Length > 0) //keep going until the buffer is empty
				{
					//get the next word
					bool endOfWord = true, lineOverFlow = true; //these are negative logic booleans: will be set to false when flagged
					while (buffer.Length > 0 && (endOfWord = !whiteSpace.Contains(buffer[0])) &&
						   (lineOverFlow = font.MeasureString(newLine + nextWord).X < LINE_LEN))
					{
						nextWord += buffer[0];
						buffer = buffer.Remove(0, 1);
					}

					//flip the bools so the program reads more logically
					//because double negatives aren't never not fun
					endOfWord = !endOfWord;
					lineOverFlow = !lineOverFlow;

					if (endOfWord)
					{
						newLine += nextWord + buffer[0];
						buffer = buffer.Remove(0, 1);
						nextWord = "";
					}
					else if (lineOverFlow)
					{
						//for line overflow: slightly different than chat, start the next line with the partial word
						if (newLine.Contains('\n'))
						{
							chatStringsToAdd.AddRange(newLine.Split(new[] {'\n'}, StringSplitOptions.None));
						}
						else
							chatStringsToAdd.Add(newLine);
						newLine = nextWord;
						nextWord = "";
					}
					else
					{
						newLine += nextWord;
						chatStringsToAdd.Add(newLine);
					}
				}

				foreach (string chatString in chatStringsToAdd)
				{
					chatStrings.Add(chatString);
				}

				scrollBar.UpdateDimensions(chatStrings.Count);
				scrollBar.LinesToRender = (int)Math.Round(110.0f / 13); //draw area for the text is 117px, 13px per line
				if(scrollBar.LinesToRender < chatStrings.Count)
					scrollBar.SetDownArrowFlashSpeed(500);
				//_msg = value;
			}
		}

		public EOScrollingDialog(string msgText)
		{
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 40);
			_setSize(bgTexture.Width, bgTexture.Height);

			font = Game.Content.Load<SpriteFont>("dbg");
			
			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(138, 197), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28));
			ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
			ok.SetParent(this);
			dlgButtons.Add(ok);

			scrollBar = new EOScrollBar(this, new Vector2(320, 66), new Vector2(16, 119), EOScrollBar.ScrollColors.LightOnMed);
			MessageText = msgText;

			endConstructor();
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			base.Draw(gt);
			if (scrollBar == null) return; //prevent nullreferenceexceptions

			SpriteBatch.Begin();
			Vector2 pos = new Vector2(27 + (int)DrawLocation.X, 69 + (int)DrawLocation.Y);

			for (int i = scrollBar.ScrollOffset; i < scrollBar.ScrollOffset + scrollBar.LinesToRender; ++i)
			{
				if (i >= chatStrings.Count)
					break;
				
				string strToDraw = chatStrings[i];

				SpriteBatch.DrawString(font, strToDraw, new Vector2(pos.X, pos.Y + (i - scrollBar.ScrollOffset) * 13), Color.FromNonPremultiplied(0xc8, 0xc8, 0xc8, 0xff));
			}

			SpriteBatch.End();
		}
	}

	/// <summary>
	/// Progress Bar dialog box that is shown to the user when their account creation is pending
	/// </summary>
	public class EOProgressDialog : EODialogBase
	{
		private TimeSpan? timeOpened;
		private readonly Texture2D pbBackText, pbForeText;
		private int pbWidth;

		public EOProgressDialog(string msgText, string captionText = "")
		{
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 18);
			_setSize(bgTexture.Width, bgTexture.Height);

			message = new XNALabel(new Rectangle(18, 57, 1, 1))//label is auto-sized
			{
				Font = new Font("Microsoft Sans Serif", 10.0f),
				ForeColor = System.Drawing.Color.FromArgb(255, 0xf0, 0xf0, 0xc8),
				Text = msgText,
				TextWidth = 254
			}; 
			message.SetParent(this);

			caption = new XNALabel(new Rectangle(59, 23, 1, 1))
			{
				Font = new Font("Microsoft Sans Serif", 10.0f),
				ForeColor = System.Drawing.Color.FromArgb(255, 0xf0, 0xf0, 0xc8),
				Text = captionText
			};
			caption.SetParent(this);

			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(181, 113), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
			ok.OnClick += (sender, e) => Close(ok, XNADialogResult.Cancel);
			ok.SetParent(this);
			dlgButtons.Add(ok);

			pbBackText = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 19);

			pbForeText = new Texture2D(Game.GraphicsDevice, 1, pbBackText.Height - 2); //foreground texture is just a fill
			Color[] pbForeFill = new Color[pbForeText.Width * pbForeText.Height];
			for (int i = 0; i < pbForeFill.Length; ++i)
				pbForeFill[i] = Color.FromNonPremultiplied(0xb4, 0xdc, 0xe6, 255);
			pbForeText.SetData(pbForeFill);

			endConstructor();
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

		public EOChangePasswordDialog(Texture2D cursorTexture, KeyboardDispatcher dispatcher)
		{
			dispatch = dispatcher;

			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 21);
			_setSize(bgTexture.Width, bgTexture.Height);

			for(int i = 0; i < inputBoxes.Length; ++i)
			{
				XNATextBox tb = new XNATextBox(new Rectangle(198, 60 + i * 30, 137, 19), cursorTexture, "Microsoft Sans Serif", 8.0f)
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

			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(157, 195), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28))
			{
				Visible = true
			};
			ok.OnClick += (s, e) =>
			{ //does some input validation before trying to call Close
				//check that all fields are filled in, otherwise: return
				if (inputBoxes.Any(tb => string.IsNullOrWhiteSpace(tb.Text))) return;

				if (Username != World.Instance.MainPlayer.AccountName)
				{
					EODialog.Show("The username or password you specified is incorrect", "Wrong info");
					return;
				}
				
				//check that passwords match, otherwise: return
				if (inputBoxes[2].Text.Length != inputBoxes[3].Text.Length || inputBoxes[2].Text != inputBoxes[3].Text)
				{
					EODialog.Show("The two passwords you provided are not the same, please try again.", "Wrong password");
					return;
				}
				
				//check that password is > 6 chars, otherwise: return
				if (inputBoxes[2].Text.Length < 6)
				{
					//Make sure passwords are good enough
					EODialog.Show("For your own safety use a longer password (try 6 or more characters)", "Wrong password");
					return;
				}

				Close(ok, XNADialogResult.OK);
			};
			ok.SetParent(this);
			dlgButtons.Add(ok);

			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(250, 194), new Rectangle(0, 28, 90, 28), new Rectangle(91, 28, 90, 28))
			{
				Visible = true
			};
			cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
			cancel.SetParent(this);
			dlgButtons.Add(cancel);

			endConstructor();
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

		public EOCreateCharacterDialog(Texture2D cursorTexture, KeyboardDispatcher dispatcher)
		{
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 20);
			_setSize(bgTexture.Width, bgTexture.Height);

			charCreateSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 22);

			inputBox = new XNATextBox(new Rectangle(80, 57, 138, 19), cursorTexture, "Microsoft Sans Serif", 8.0f)
			{
				LeftPadding = 5,
				DefaultText = " ",
				MaxChars = 12,
				Selected = true,
				TextColor = System.Drawing.Color.FromArgb(0xff, 0xdc, 0xc8, 0xb4),
				Visible = true
			};
			inputBox.SetParent(this);
			dispatcher.Subscriber = inputBox;

			//four arrow buttons
			for(int i = 0; i < arrowButtons.Length; ++i)
			{
				XNAButton btn = new XNAButton(charCreateSheet, new Vector2(196, 85 + i * 26), new Rectangle(185, 38, 19, 19), new Rectangle(206, 38, 19, 19))
				{
					Visible = true
				};
				btn.OnClick += ArrowButtonClick;
				btn.SetParent(this);
				arrowButtons[i] = btn;
			}

			charRender = new EOCharacterRenderer(new Vector2(269, 83), new CharRenderData { gender = 0, hairstyle = 1, haircolor = 0, race = 0 });
			charRender.SetParent(this);
			srcRects[0] = new Rectangle(0, 38, 23, 19);
			srcRects[1] = new Rectangle(0, 19, 23, 19);
			srcRects[2] = new Rectangle(0, 0, 23, 19);
			srcRects[3] = new Rectangle(46, 38, 23, 19);
			
			//ok/cancel buttons
			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(157, 195), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28))
			{
				Visible = true
			};
			ok.OnClick += (s, e) =>
			{
				if(inputBox.Text.Length < 4)
				{
					EODialog.Show("The name you provided for this character is too short (try 4 or more characters)", "Wrong name");
					return;
				}

				Close(ok, XNADialogResult.OK);
			};
			ok.SetParent(this);
			dlgButtons.Add(ok);

			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(250, 194), new Rectangle(0, 28, 90, 28), new Rectangle(91, 28, 90, 28))
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
			if ((Dialogs.Count > 0 && Dialogs.Peek() != this) || !Visible)
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

		public EOConnectingDialog()
		{
			bgTexture = null; //don't use the built in bgtexture, we're going to use a sprite sheet for the BG
			bgSprites = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 33);
			DrawLocation = new Vector2(Game.GraphicsDevice.PresentationParameters.BackBufferWidth - (bgSprites.Width / 4) - 10, 
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight - bgSprites.Height - 10);
			_setSize(bgSprites.Width / 4, bgSprites.Height);
			bgSrcIndex = 0;

			caption = new XNALabel(new Rectangle(12, 9, 1, 1), "Microsoft Sans Serif", 10.0f)
			{
				Text = wait,
				ForeColor = System.Drawing.Color.FromArgb(0xf0, 0xf0, 0xc8)
			};
			caption.SetParent(this);

			message = new XNALabel(new Rectangle(18, 61, 1, 1), "Microsoft Sans Serif", 8.0f)
			{
				TextWidth = 175,
				ForeColor = System.Drawing.Color.FromArgb(0xb9, 0xb9, 0xb9),
				Text = "Make sure noone is watching your keyboard, while entering your password"
			};
			//there are a number of messages that are shown, a static one will do for now
			message.SetParent(this);

			endConstructor(false);
		}

		public override void Update(GameTime gt)
		{
			if (timeOpened == null)
				timeOpened = gt.TotalGameTime;

			if(((int)gt.TotalGameTime.TotalMilliseconds - (int)(timeOpened.Value.TotalMilliseconds)) % 500 == 0) //every half a second
			{
				//switch the background image to the next one
				bgSrcIndex = bgSrcIndex == 3 ? 0 : bgSrcIndex + 1;
			}

#if DEBUG
			const int waitTime = 0; //set to zero on debug builds
#else
			const int waitTime = 5; //I think the client waits 5 seconds?
#endif
			if (!updatingFiles && ((int)gt.TotalGameTime.TotalSeconds - (int)(timeOpened.Value.TotalSeconds)) >= waitTime)
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
#if !DEBUG //only sleep on release builds!
						Thread.Sleep(1000); //computers are fast: I think the actual client sleeps at this point in its logic too because there is no way it should take as long as it does
#endif
					}

					if (World.Instance.NeedEIF)
					{
						caption.Text = item;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Item))
						{
							Close(null, XNADialogResult.NO_BUTTON_PRESSED);
							return;
						}
#if !DEBUG //only sleep on release builds!
						Thread.Sleep(1000);
#endif
					}

					if (World.Instance.NeedENF)
					{
						caption.Text = npc;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Npc))
						{
							Close(null, XNADialogResult.NO_BUTTON_PRESSED);
							return;
						}
#if !DEBUG //only sleep on release builds!
						Thread.Sleep(1000);
#endif
					}

					if (World.Instance.NeedESF)
					{
						caption.Text = skill;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Spell))
						{
							Close(null, XNADialogResult.NO_BUTTON_PRESSED);
							return;
						}
#if !DEBUG //only sleep on release builds!
						Thread.Sleep(1000);
#endif
					}

					if (World.Instance.NeedECF)
					{
						caption.Text = classes;
						if (!Handlers.Init.RequestFile(Handlers.InitFileType.Class))
						{
							Close(null, XNADialogResult.NO_BUTTON_PRESSED);
							return;
						}
#if !DEBUG //only sleep on release builds!
						Thread.Sleep(1000);
#endif
					}

					caption.Text = loading;
					if(!Handlers.Welcome.WelcomeMessage(World.Instance.MainPlayer.ActiveCharacter.ID))
					{
						Close(null, XNADialogResult.NO_BUTTON_PRESSED);
						return;
					}
#if !DEBUG //only sleep on release builds!
					Thread.Sleep(1000);
#endif

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

	public class EOPaperdollItem : XNAControl
	{
		private ItemRecord m_info;
		private Texture2D m_gfx;
		private Rectangle m_area;

		public EquipLocation EquipLoc { get; private set; }
		//public short ItemID { get { return (short)(m_info ?? new ItemRecord()).ID; } }

		public EOPaperdollItem(Rectangle location, EOPaperdollDialog parent, ItemRecord info, EquipLocation locationEnum)
			: base(null, null, parent)
		{
			SetInfo(location, info);
			EquipLoc = locationEnum;
		}

		public override void Update(GameTime gameTime)
		{
			//At this time, not going to support drag/drop between inventory and paperdoll dialog.
			//Benefit is not worth the work effort involved.

			MouseState currentState = Mouse.GetState();

			if (MouseOver && !MouseOverPreviously)
			{
				string typename = Enum.GetName(typeof (EquipLocation), EquipLoc);
				if (string.IsNullOrEmpty(typename))
					return;
				if (typename.Contains("1") || typename.Contains("2"))
					typename = typename.Substring(0, typename.Length - 1);

				string toSet;
				if (m_info != null)
					toSet = "[ Information ] " + typename + " equipment, " + m_info.Name;
				else
					toSet = "[ Information ] " + typename + " equipment";
				EOGame.Instance.Hud.SetStatusLabel(toSet);
			}

			//unequipping an item via right-click
			if (m_info != null && MouseOver && currentState.RightButton == ButtonState.Released &&
			    PreviousMouseState.RightButton == ButtonState.Pressed)
			{
				if (((EOPaperdollDialog) parent).CharRef == World.Instance.MainPlayer.ActiveCharacter)
				{ //the parent dialog must show equipment for mainplayer
					if (m_info.Special == ItemSpecial.Cursed)
					{
						EODialog.Show("Oh no, this item is cursed! Only a cure potion/spell can remove it.", "Cursed Item");
					}
					else
					{
						_setSize(m_area.Width, m_area.Height);
						DrawLocation = new Vector2(m_area.X + (m_area.Width/2 - DrawArea.Width/2),
							m_area.Y + (m_area.Height/2 - DrawArea.Height/2));

						//put back in the inventory by the packet handler response
						string locName = Enum.GetName(typeof (EquipLocation), EquipLoc);
						if (!string.IsNullOrEmpty(locName))
							Handlers.Paperdoll.UnequipItem((short) m_info.ID, (byte) (locName.Contains("2") ? 1 : 0));

						m_info = null;
						m_gfx = null;
					}
				}
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
			if (m_gfx == null) return;
			SpriteBatch.Begin();
			SpriteBatch.Draw(m_gfx, DrawAreaWithOffset, Color.White);
			SpriteBatch.End();
		}

		public void SetInfo(Rectangle location, ItemRecord info)
		{
			m_info = info;
			if (info != null)
			{
				m_gfx = GFXLoader.TextureFromResource(GFXTypes.Items, 2 * info.Graphic, true);
			}
			m_area = location;

			if (m_gfx != null)
				_setSize(m_gfx.Width, m_gfx.Height);
			else
				_setSize(location.Width, location.Height);

			DrawLocation = new Vector2(location.X + (m_area.Width / 2 - DrawArea.Width / 2), location.Y + (m_area.Height / 2 - DrawArea.Height / 2));
		}
	}

	public class EOPaperdollDialog : EODialogBase
	{
		public static EOPaperdollDialog Instance;

		public Character CharRef { get; private set; }

		private readonly Texture2D m_characterIcon;

		private static readonly Rectangle m_characterIconRect = new Rectangle(227, 258, 44, 21);

		public enum IconType
		{
			Normal = 0,
			GM = 4,
			HGM = 5,
			Party = 6,
			GMParty = 9,
			HGMParty = 10,
			SLNBot = 20
		}

		public EOPaperdollDialog(Character character, string home, string partner, string guild, string guildRank, IconType whichIcon)
		{
			if(Instance != null)
				throw new InvalidOperationException("Paperdoll is already open!");
			Instance = this;

			CharRef = character;

			Texture2D bgSprites = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 49);
			_setSize(bgSprites.Width, bgSprites.Height / 2);

			Color[] dat = new Color[DrawArea.Width*DrawArea.Height];
			bgTexture = new Texture2D(Game.GraphicsDevice, DrawArea.Width, DrawArea.Height);
			bgSprites.GetData(0, DrawArea.SetPosition(new Vector2(0, CharRef.RenderData.gender * DrawArea.Height)), dat, 0, dat.Length);
			bgTexture.SetData(dat);

			//not using caption/message since we have other shit to take care of

			//ok button
			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(276, 253), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28)) { Visible = true };
			ok.OnClick += (s, e) => Close(ok, XNADialogResult.OK);
			ok.SetParent(this);
			dlgButtons.Add(ok);

			//items
			for (int i = (int) EquipLocation.Boots; i < (int) EquipLocation.PAPERDOLL_MAX; ++i)
			{
				ItemRecord info = World.Instance.EIF.GetItemRecordByID(CharRef.PaperDoll[i]);

				Rectangle itemArea = _getEquipLocRectangle((EquipLocation) i);

				//create item using itemArea
				if (CharRef.PaperDoll[i] > 0)
				{
// ReSharper disable once UnusedVariable
					EOPaperdollItem nextItem = new EOPaperdollItem(itemArea, this, info, (EquipLocation)i); //auto-added as child of this dialog
				}
				else
				{
// ReSharper disable once UnusedVariable
					EOPaperdollItem nextItem = new EOPaperdollItem(itemArea, this, null, (EquipLocation)i);
				}
			}

			//labels next
			XNALabel[] labels =
			{
				new XNALabel(new Rectangle(228, 22, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = CharRef.Name.Length > 0 ? char.ToUpper(CharRef.Name[0]) + CharRef.Name.Substring(1) : ""
				}, //name
				new XNALabel(new Rectangle(228, 52, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = home.Length > 0 ? char.ToUpper(home[0]) + home.Substring(1) : ""
				}, //home
				new XNALabel(new Rectangle(228, 82, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = ((ClassRecord)(World.Instance.ECF.Data.Find(_dat => ((ClassRecord)_dat).ID == CharRef.Class) ?? new ClassRecord())).Name //Check for nulls, for teh lolz
				}, //class
				new XNALabel(new Rectangle(228, 112, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = partner.Length > 0 ? char.ToUpper(partner[0]) + partner.Substring(1) : ""
				}, //partner
				new XNALabel(new Rectangle(228, 142, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = CharRef.Title.Length > 0 ? char.ToUpper(CharRef.Title[0]) + CharRef.Title.Substring(1) : ""
				}, //title
				new XNALabel(new Rectangle(228, 202, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = guild.Length > 0 ? char.ToUpper(guild[0]) + guild.Substring(1) : ""
				}, //guild
				new XNALabel(new Rectangle(228, 232, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = guildRank.Length > 0 ? char.ToUpper(guild[0]) + guildRank.Substring(1) : ""
				} //rank
			};

			labels.ToList().ForEach(_l => { _l.ForeColor = System.Drawing.Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8); _l.SetParent(this); });

			ChatType iconType;
			switch (whichIcon)
			{
				case IconType.Normal:
					iconType = ChatType.Player;
					break;
				case IconType.GM:
					iconType = ChatType.GM;
					break;
				case IconType.HGM:
					iconType = ChatType.HGM;
					break;
				case IconType.Party:
					iconType = ChatType.PlayerParty;
					break;
				case IconType.GMParty:
					iconType = ChatType.GMParty;
					break;
				case IconType.HGMParty:
					iconType = ChatType.HGMParty;
					break;
				case IconType.SLNBot:
					iconType = ChatType.PlayerPartyDark;
					break;
				default:
					throw new ArgumentOutOfRangeException("whichIcon", "Invalid Icon type specified.");
			}
			m_characterIcon = ChatTab.GetChatIcon(iconType);

			//should not be centered vertically: only display in game window
			//first center in the game display window, then move it 15px from top, THEN call end constructor logic
			//if not done in this order some items DrawAreaWithOffset field does not get updated properly when setting DrawLocation
			Center(Game.GraphicsDevice);
			DrawLocation = new Vector2(DrawLocation.X, 15);
			endConstructor(false);
		}

		public override void Update(GameTime gt)
		{
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

		public override void Draw(GameTime gt)
		{
			base.Draw(gt);
			SpriteBatch.Begin();
			SpriteBatch.Draw(m_characterIcon,
				new Vector2(
					DrawAreaWithOffset.X + m_characterIconRect.X + (m_characterIconRect.Width/2) - (m_characterIcon.Width/2),
					DrawAreaWithOffset.Y + m_characterIconRect.Y + (m_characterIconRect.Height/2) - (m_characterIcon.Height/2)),
				Color.White);
			SpriteBatch.End();
		}

		public void SetItem(EquipLocation loc, ItemRecord info)
		{
			EOPaperdollItem itemToUpdate = (EOPaperdollItem)children.Find(_ctrl =>
			{
				EOPaperdollItem item = _ctrl as EOPaperdollItem;
				if (item == null) return false;
				return item.EquipLoc == loc;
			});
			if(itemToUpdate != null)
				itemToUpdate.SetInfo(_getEquipLocRectangle(loc), info);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Instance = null;
		}

		private static Rectangle _getEquipLocRectangle(EquipLocation loc)
		{
			Rectangle itemArea;
			switch (loc)
			{
				case EquipLocation.Boots:
					itemArea = new Rectangle(87, 220, 56, 54);
					break;
				case EquipLocation.Accessory:
					itemArea = new Rectangle(55, 250, 23, 23);
					break;
				case EquipLocation.Gloves:
					itemArea = new Rectangle(22, 188, 56, 54);
					break;
				case EquipLocation.Belt:
					itemArea = new Rectangle(87, 188, 56, 23);
					break;
				case EquipLocation.Armor:
					itemArea = new Rectangle(86, 82, 56, 98);
					break;
				case EquipLocation.Necklace:
					itemArea = new Rectangle(152, 51, 56, 23);
					break;
				case EquipLocation.Hat:
					itemArea = new Rectangle(87, 21, 56, 54);
					break;
				case EquipLocation.Shield:
					itemArea = new Rectangle(152, 82, 56, 98);
					break;
				case EquipLocation.Weapon:
					itemArea = new Rectangle(22, 82, 56, 98);
					break;
				case EquipLocation.Ring1:
					itemArea = new Rectangle(152, 190, 23, 23);
					break;
				case EquipLocation.Ring2:
					itemArea = new Rectangle(185, 190, 23, 23);
					break;
				case EquipLocation.Armlet1:
					itemArea = new Rectangle(152, 220, 23, 23);
					break;
				case EquipLocation.Armlet2:
					itemArea = new Rectangle(185, 220, 23, 23);
					break;
				case EquipLocation.Bracer1:
					itemArea = new Rectangle(152, 250, 23, 23);
					break;
				case EquipLocation.Bracer2:
					itemArea = new Rectangle(185, 250, 23, 23);
					break;
				default:
					throw new ArgumentOutOfRangeException("loc", "That is not a valid equipment location");
			}
			return itemArea;
		}
	}

	public class EOItemTransferDialog : EODialogBase
	{
		public enum TransferType
		{
			DropItems,
			JunkItems,
// ReSharper disable UnusedMember.Global
			GiveItems,
			TradeItems,
			ShopTransfer,
			BankTransfer
// ReSharper restore UnusedMember.Global
		}

		private readonly Texture2D m_titleBarGfx;
		private readonly int m_totalAmount;

		private readonly XNATextBox m_amount;
		public int SelectedAmount
		{
			get { return int.Parse(m_amount.Text); }
		}

		public EOItemTransferDialog(string itemName, TransferType transferType, int totalAmount)
		{
			Texture2D weirdSpriteSheet = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 27);
			Rectangle sourceArea = new Rectangle(38, 0, 265, 170);
			
			//get bgTexture
			Color[] textureData = new Color[sourceArea.Width*sourceArea.Height];
			bgTexture = new Texture2D(Game.GraphicsDevice, sourceArea.Width, sourceArea.Height);
			weirdSpriteSheet.GetData(0, sourceArea, textureData, 0, textureData.Length);
			bgTexture.SetData(textureData);

			//get the title bar - for when it isn't drop items
			if (transferType != TransferType.DropItems)
			{
				Rectangle titleBarArea = new Rectangle(40, 172 + ((int)transferType - 1) * 24, 241, 22);
				Color[] titleBarData = new Color[titleBarArea.Width*titleBarArea.Height];
				m_titleBarGfx = new Texture2D(Game.GraphicsDevice, titleBarArea.Width, titleBarArea.Height);
				weirdSpriteSheet.GetData(0, titleBarArea, titleBarData, 0, titleBarData.Length);
				m_titleBarGfx.SetData(titleBarData);
			}

			//set the buttons here

			//ok/cancel buttons
			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(60, 125), new Rectangle(0, 116, 90, 28), new Rectangle(91, 116, 90, 28))
			{
				Visible = true
			};
			ok.OnClick += (s, e) => Close(ok, XNADialogResult.OK);
			ok.SetParent(this);
			dlgButtons.Add(ok);

			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(153, 124), new Rectangle(0, 28, 90, 28), new Rectangle(91, 28, 90, 28))
			{
				Visible = true
			};
			cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
			cancel.SetParent(this);
			dlgButtons.Add(cancel);

			XNALabel descLabel = new XNALabel(new Rectangle(20, 42, 231, 33), "Microsoft Sans Serif", 10.0f)
			{
				ForeColor = System.Drawing.Color.FromArgb(0xe6, 0xe6, 0xd6),
				TextWidth = 200
			};

			switch (transferType)
			{
				case TransferType.DropItems:
					descLabel.Text = string.Format("How much {0} would you like to drop?", itemName);
					break;
				case TransferType.JunkItems:
					descLabel.Text = string.Format("How much {0} would you like to junk?", itemName);
					break;
				default:
					descLabel.Text = "(not implemented)";
					break;
			}
			descLabel.SetParent(this);

			//set the text box here
			//starting coords are 163, 97
			m_amount = new XNATextBox(new Rectangle(163, 95, 77, 19), Game.Content.Load<Texture2D>("cursor"), "Microsoft Sans Serif", 8.0f)
			{
				Visible = true,
				Enabled = true,
				MaxChars = 8, //max drop/junk at a time will be 99,999,999
				TextColor = System.Drawing.Color.FromArgb(0xdc, 0xC8, 0xb4),
				Text = "1"
			};
			m_amount.OnTextChanged += (sender, args) =>
			{
				int amt;
				if (m_amount.Text != "" && (!int.TryParse(m_amount.Text, out amt) || amt > m_totalAmount))
				{
					m_amount.Text = m_totalAmount.ToString(CultureInfo.InvariantCulture);
				}
			};
			m_amount.SetParent(this);
			(Game as EOGame ?? EOGame.Instance).Dispatcher.Subscriber = m_amount;

			m_totalAmount = totalAmount;

			//slider control
			Texture2D src = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 29);
			//5th index when 'out', 6th index when 'over'
			Rectangle outText = new Rectangle(0, 15 * 5, 16, 15);
			Rectangle ovrText = new Rectangle(0, 15 * 6, 16, 15);
			Color[] outData = new Color[16*15];
			Color[] ovrData = new Color[16*15];
			Texture2D[] sliderTextures = new Texture2D[2];
			
			src.GetData(0, outText, outData, 0, outData.Length);
			src.GetData(0, ovrText, ovrData, 0, ovrData.Length);
			(sliderTextures[0] = new Texture2D(Game.GraphicsDevice, 16, 15)).SetData(outData);
			(sliderTextures[1] = new Texture2D(Game.GraphicsDevice, 16, 15)).SetData(ovrData);

			//starting coords are 25, 96; range rectangle is 122, 15
			XNAButton slider = new XNAButton(sliderTextures, new Vector2(25, 96));
			slider.OnClickDrag += (o, e) =>
			{
				MouseState st = Mouse.GetState();
				Rectangle sliderArea = new Rectangle(25, 96, 122 - slider.DrawArea.Width, 15);
				int newX = (st.X - PreviousMouseState.X) + (int)slider.DrawLocation.X;
				if (newX < sliderArea.X) newX = sliderArea.X;
				else if (newX > sliderArea.Width + sliderArea.X) newX = sliderArea.Width + sliderArea.X;
				slider.DrawLocation = new Vector2(newX, slider.DrawLocation.Y); //unchanged y coordinate, slides along x-axis

				float ratio = (newX - sliderArea.X)/(float)sliderArea.Width;
				m_amount.Text = ((int) Math.Round(ratio*m_totalAmount) + 1).ToString(CultureInfo.InvariantCulture);
			};
			slider.SetParent(this);

			_setSize(bgTexture.Width, bgTexture.Height);
			DrawLocation = new Vector2(Game.GraphicsDevice.PresentationParameters.BackBufferWidth/2 - bgTexture.Width/2, 40); //only centered horizontally
			endConstructor(false);
		}

		public override void Draw(GameTime gt)
		{
			base.Draw(gt);

			if (m_titleBarGfx != null)
			{
				SpriteBatch.Begin();
				SpriteBatch.Draw(m_titleBarGfx, new Vector2(DrawAreaWithOffset.X + 10, DrawAreaWithOffset.Y + 10), Color.White);
				SpriteBatch.End();
			}
		}
	}

	public class EOChestItem : XNAControl
	{
		private static readonly object disposingLock = new object();
		private bool m_disposing;

		public short ID { get; private set; }
		public int Amount { get; private set; }
		public int Index { get; set; }

		private readonly ItemRecord m_rec;

		private readonly XNALabel m_nameLabel;
		private readonly XNALabel m_amountLabel;

		private readonly Texture2D m_gfxPadThing;
		private readonly Texture2D m_gfxItem;
		private readonly Texture2D m_backgroundColor;
		private bool m_drawBackground;

		public EOChestItem(short id, int amount, int index, EOChestDialog parent)
		{
			ID = id;
			Amount = amount;
			Index = index;
			m_rec = World.Instance.EIF.GetItemRecordByID(id);

			DrawLocation = new Vector2(19, 25 + (index * 36));
			_setSize(232, 36);

			m_nameLabel = new XNALabel(new Rectangle(56, 5, 1, 1), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = true,
				BackColor = System.Drawing.Color.Transparent,
				ForeColor = System.Drawing.Color.FromArgb(255, 0xc8,0xc8,0xc8),
				Text = m_rec.Name
			};
			m_nameLabel.ResizeBasedOnText();

			m_amountLabel = new XNALabel(new Rectangle(56, 20, 1, 1), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = true,
				BackColor = m_nameLabel.BackColor,
				ForeColor = m_nameLabel.ForeColor,
				Text = string.Format("x {0}  {1}", Amount, m_rec.Type == ItemType.Armor ? (m_rec.Gender == 0 ? "Female" : "Male") : "")
			};
			m_amountLabel.ResizeBasedOnText();

			m_gfxPadThing = GFXLoader.TextureFromResource(GFXTypes.MapTiles, 0, true);
			m_gfxItem = GFXLoader.TextureFromResource(GFXTypes.Items, 2*m_rec.Graphic - 1, true);
			m_backgroundColor = new Texture2D(Game.GraphicsDevice, 1, 1);
			m_backgroundColor.SetData(new [] {Color.FromNonPremultiplied(0xff, 0xff, 0xff, 64)});

			SetParent(parent);
			m_amountLabel.SetParent(this);
			m_nameLabel.SetParent(this);
		}

		public override void Update(GameTime gameTime)
		{
			MouseState ms = Mouse.GetState();

			if (MouseOver && MouseOverPreviously)
			{
				m_drawBackground = true;
				//right clicked this item
				if (PreviousMouseState.RightButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released)
				{
					if (!EOGame.Instance.Hud.InventoryFits(ID))
					{
						EODialog.Show("You could not pick up this item because you have no more space left.", "Warning", XNADialogButtons.Ok, true);
						PreviousMouseState = ms;
					}
					else
					{
						EOChestDialog localParent = (EOChestDialog) parent;
						if (!Handlers.Chest.ChestTake(localParent.CurrentChestX, localParent.CurrentChestY, ID))
						{
							localParent.Close();
							EOGame.Instance.LostConnectionDialog();
							return;
						}
					}
				}
			}
			else
			{
				m_drawBackground = false;
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			lock (disposingLock)
			{
				if (m_disposing)
					return;
				SpriteBatch.Begin();
				if (m_drawBackground)
				{
					SpriteBatch.Draw(m_backgroundColor, DrawAreaWithOffset, Color.White);
				}
				SpriteBatch.Draw(m_gfxPadThing, new Vector2(xOff + 19, yOff + 29 + 36*Index), Color.White);
				SpriteBatch.Draw(m_gfxItem, new Vector2(xOff + 27, yOff + 24 + 36*Index), Color.White);
				SpriteBatch.End();
				base.Draw(gameTime);
			}
		}

		public new void Dispose()
		{
			Dispose(true);
		}

		protected override void Dispose(bool disposing)
		{
			lock (disposingLock)
			{
				m_disposing = true;
				if (disposing)
				{
					m_nameLabel.Dispose();
					m_amountLabel.Dispose();
					m_backgroundColor.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}

	public class EOChestDialog : EODialogBase
	{
		public static EOChestDialog Instance;

		public byte CurrentChestX { get; private set; }
		public byte CurrentChestY { get; private set; }

		private EOChestItem[] m_items;

		public EOChestDialog(byte chestX, byte chestY, List<Tuple<short, int>> initialItems)
		{
			if (Instance != null)
				throw new InvalidOperationException("Chest is already open!");
			
			Instance = this;

			InitializeItems(initialItems);

			CurrentChestX = chestX;
			CurrentChestY = chestY;

			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(92, 227), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
			cancel.OnClick += (sender, e) => Close(cancel, XNADialogResult.Cancel);
			dlgButtons.Add(cancel);
			whichButtons = XNADialogButtons.Cancel;

			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 51);
			_setSize(bgTexture.Width, bgTexture.Height);
			
			endConstructor(false);
			DrawLocation = new Vector2((Game.GraphicsDevice.PresentationParameters.BackBufferWidth - DrawArea.Width) / 2f, 15);
			cancel.SetParent(this);
		}

		public void InitializeItems(List<Tuple<short, int>> initialItems)
		{
			if(m_items == null)
				m_items = new EOChestItem[5];

			int i = 0;
			if (initialItems.Count > 0)
			{
				for (; i < initialItems.Count && i < 5; ++i)
				{
					Tuple<short, int> item = initialItems[i];
					if (m_items[i] != null)
					{
						m_items[i].Close();
						m_items[i] = null;
					}

					m_items[i] = new EOChestItem(item.Item1, item.Item2, i, this);
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
			foreach(XNAControl child in children)
				child.SetParent(this);
			base.Initialize();
		}

		public override void Update(GameTime gt)
		{
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

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Instance = null;
		}
	}
}
