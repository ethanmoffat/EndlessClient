using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EOLib;
using EOLib.Data;
using EOLib.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

#pragma warning disable 162

namespace EndlessClient
{
	/// <summary>
	/// Which buttons should be displayed at the bottom of the EOScrollingListDialog
	/// </summary>
	public enum ScrollingListDialogButtons
	{
		AddCancel,
		Cancel,
		BackCancel,
	}

	public enum EODialogStyle
	{
		SmallDialogLargeHeader,
		SmallDialogSmallHeader,
		LargeDialogSmallHeader
	}

	public class EODialogBase : XNADialog
	{
		protected readonly Texture2D smallButtonSheet;
		protected readonly PacketAPI m_api;

		protected EODialogBase(PacketAPI apiHandle = null)
		{
			if (apiHandle != null)
			{
				if (!apiHandle.Initialized)
					throw new ArgumentException("The API is not initialzied. Data transfer will not work.");
				m_api = apiHandle;
			}
			smallButtonSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 15, true);
		}

		protected void endConstructor(bool centerDialog = true)
		{
			//center dialog based on txtSize of background texture
			if (centerDialog)
			{
				Center(Game.GraphicsDevice);
				if (EOGame.Instance.State == GameStates.PlayingTheGame)
				{
					DrawLocation = new Vector2(DrawLocation.X, (330 - DrawArea.Height) / 2f);
				}
			}
			_fixDrawOrder();
			DrawOrder += 100;
			Dialogs.Push(this);

			Game.Components.Add(this);
		}

		protected enum SmallButton
		{
			Connect = 0,
			Cancel,
			Login,
			Delete,
			Ok,
			Back,
			Add,
			Next,
			History,
			Progress,
			NUM_BUTTONS
		}
		protected Rectangle _getSmallButtonOut(SmallButton whichOne)
		{
			int widthDelta = smallButtonSheet.Width/2;
			int heightDelta = smallButtonSheet.Height/(int) SmallButton.NUM_BUTTONS;
			return new Rectangle(0, heightDelta * (int)whichOne, widthDelta, heightDelta);
		}
		protected Rectangle _getSmallButtonOver(SmallButton whichOne)
		{
			int widthDelta = smallButtonSheet.Width / 2;
			int heightDelta = smallButtonSheet.Height / (int)SmallButton.NUM_BUTTONS;
			return new Rectangle(widthDelta, heightDelta * (int)whichOne, widthDelta, heightDelta);
		}

		protected enum ListIcon
		{
			Buy = 0,
			Sell,
			BankDeposit,
			BankWithdraw,
			Craft,
			BankLockerUpgrade,

			Learn = 20,
			Forget = 21,
		}
		protected Texture2D _getDlgIcon(ListIcon whichOne)
		{
			const int NUM_PER_ROW = 9;
			const int ICON_SIZE = 31;

			Texture2D weirdSheet = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 27);
			Color[] dat = new Color[ICON_SIZE*ICON_SIZE];

			Rectangle src = new Rectangle(((int) whichOne%NUM_PER_ROW)*ICON_SIZE, 291 + ((int) whichOne/NUM_PER_ROW)*ICON_SIZE, ICON_SIZE, ICON_SIZE);
			weirdSheet.GetData(0, src, dat, 0, dat.Length);
			
			Texture2D ret = new Texture2D(EOGame.Instance.GraphicsDevice, ICON_SIZE, ICON_SIZE);
			ret.SetData(dat);
			return ret;
		}
	}

	/// <summary>
	/// EODialog is a basic dialog representation (like Windows MessageBox)
	/// </summary>
	public class EODialog : EODialogBase
	{
		public EODialog(string msgText, string captionText = "", XNADialogButtons whichButtons = XNADialogButtons.Ok, EODialogStyle style = EODialogStyle.SmallDialogLargeHeader)
		{
			// ReSharper disable once RedundantBaseQualifier
			base.whichButtons = whichButtons;

			bool useSmallHeader = style == EODialogStyle.LargeDialogSmallHeader || style == EODialogStyle.SmallDialogSmallHeader;

			if(style == EODialogStyle.SmallDialogLargeHeader)
				bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 18);
			else if (style == EODialogStyle.SmallDialogSmallHeader)
				bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 23);
			else if (style == EODialogStyle.LargeDialogSmallHeader)
				bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 25);
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
					ok = new XNAButton(smallButtonSheet, new Vector2(181, 113), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
					ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
					ok.SetParent(this);
					dlgButtons.Add(ok);
					break;
				case XNADialogButtons.Cancel:
					cancel = new XNAButton(smallButtonSheet, new Vector2(181, 113), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
					cancel.OnClick += (sender, e) => Close(cancel, XNADialogResult.Cancel);
					cancel.SetParent(this);
					dlgButtons.Add(cancel);
					break;
				case XNADialogButtons.OkCancel:
					//implement this more fully when it is needed
					//update draw location of ok button to be on left?
					ok = new XNAButton(smallButtonSheet, new Vector2(89, 113), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
					ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
					ok.SetParent(this);

					cancel = new XNAButton(smallButtonSheet, new Vector2(181, 113), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
					cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
					cancel.SetParent(this);

					dlgButtons.Add(ok);
					dlgButtons.Add(cancel);
					break;
			}

			if(useSmallHeader)
			{
				if(style == EODialogStyle.SmallDialogSmallHeader)
					foreach (XNAButton btn in dlgButtons)
						btn.DrawLocation = new Vector2(btn.DrawLocation.X, 82);
				else
					foreach (XNAButton btn in dlgButtons)
						btn.DrawLocation = new Vector2(btn.DrawLocation.X, 148);
			}

			endConstructor();
		}

		public static void Show(string message, string caption = "", XNADialogButtons buttons = XNADialogButtons.Ok, EODialogStyle style = EODialogStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			EODialog dlg = new EODialog(message, caption, buttons, style);
			if(closingEvent != null)
				dlg.DialogClosing += closingEvent;
		}

		public static void Show(DATCONST1 resource, XNADialogButtons whichButtons = XNADialogButtons.Ok,
			EODialogStyle style = EODialogStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			if(!World.Initialized)
				throw new WorldLoadException("Unable to create dialog! World must be loaded and initialized.");

			EDFFile file = World.Instance.DataFiles[World.Instance.Localized1];

			Show(file.Data[(int)resource + 1], file.Data[(int)resource], whichButtons, style, closingEvent);
		}

		public static void Show(string prependData, DATCONST1 resource, XNADialogButtons whichButtons = XNADialogButtons.Ok,
			EODialogStyle style = EODialogStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			if (!World.Initialized)
				throw new WorldLoadException("Unable to create dialog! World must be loaded and initialized.");

			EDFFile file = World.Instance.DataFiles[World.Instance.Localized1];

			string message = prependData + file.Data[(int) resource + 1];
			Show(message, file.Data[(int)resource], whichButtons, style, closingEvent);
		}

		public static void Show(DATCONST1 resource, string extraData, XNADialogButtons whichButtons = XNADialogButtons.Ok,
			EODialogStyle style = EODialogStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			if (!World.Initialized)
				throw new WorldLoadException("Unable to create dialog! World must be loaded and initialized.");

			EDFFile file = World.Instance.DataFiles[World.Instance.Localized1];

			string message = file.Data[(int)resource + 1] + extraData;
			Show(message, file.Data[(int)resource], whichButtons, style, closingEvent);
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
			scroll.OnMouseEnter += (o, e) => { SuppressParentClickDrag(true); };
			scroll.OnMouseLeave += (o, e) => { SuppressParentClickDrag(false); };
			scroll.SetParent(this);

			_totalHeight = DrawAreaWithOffset.Height;
		}

		public new void IgnoreDialog(Type t)
		{
			base.IgnoreDialog(t);
			up.IgnoreDialog(t);
			down.IgnoreDialog(t);
			scroll.IgnoreDialog(t);
		}
		
		public void UpdateDimensions(int numberOfLines)
		{
			_totalHeight = numberOfLines;
		}

		public void ScrollToTop()
		{
			ScrollOffset = 0;
			float pixelsPerLine = (float)(scrollArea.Height - scroll.DrawArea.Height * 2) / (_totalHeight - LinesToRender);
			scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scroll.DrawArea.Height + pixelsPerLine * ScrollOffset);
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
			if ((parent != null && !parent.Visible) || !ShouldUpdate())
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
				int dif = (currentState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue) / -120; //otherwise you get "Natural" scroll. We'll have none of that.
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
		private readonly TextSplitter textSplitter;

		public new string MessageText
		{
			set
			{
				chatStrings.Clear();
				textSplitter.Text = value;

				//special case: blank line, like in the news panel between news items
				if (string.IsNullOrWhiteSpace(value))
				{
					chatStrings.Add(" ");
					scrollBar.UpdateDimensions(chatStrings.Count);
					return;
				}

				//don't do multi-line processing if we don't need to
				if (!textSplitter.NeedsProcessing)
				{
					chatStrings.Add(value);
					scrollBar.UpdateDimensions(chatStrings.Count);
					return;
				}

				chatStrings.AddRange(textSplitter.SplitIntoLines());

				scrollBar.UpdateDimensions(chatStrings.Count);
				scrollBar.LinesToRender = (int)Math.Round(110.0f / 13); //draw area for the text is 117px, 13px per line
				if(scrollBar.LinesToRender < chatStrings.Count)
					scrollBar.SetDownArrowFlashSpeed(500);
			}
		}

		public EOScrollingDialog(string msgText)
		{
			textSplitter = new TextSplitter("", EOGame.Instance.DBGFont) { LineLength = 275 };

			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 40);
			_setSize(bgTexture.Width, bgTexture.Height);

			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(138, 197), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
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

				SpriteBatch.DrawString(EOGame.Instance.DBGFont, strToDraw, new Vector2(pos.X, pos.Y + (i - scrollBar.ScrollOffset) * 13), Color.FromNonPremultiplied(0xc8, 0xc8, 0xc8, 0xff));
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

			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(181, 113), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
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

			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(157, 195), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok))
			{
				Visible = true
			};
			ok.OnClick += (s, e) =>
			{ //does some input validation before trying to call Close
				//check that all fields are filled in, otherwise: return
				if (inputBoxes.Any(tb => string.IsNullOrWhiteSpace(tb.Text))) return;

				if (Username != World.Instance.MainPlayer.AccountName)
				{
					EODialog.Show(DATCONST1.CHANGE_PASSWORD_MISMATCH);
					return;
				}
				
				//check that passwords match, otherwise: return
				if (inputBoxes[2].Text.Length != inputBoxes[3].Text.Length || inputBoxes[2].Text != inputBoxes[3].Text)
				{
					EODialog.Show(DATCONST1.ACCOUNT_CREATE_PASSWORD_MISMATCH);
					return;
				}
				
				//check that password is > 6 chars, otherwise: return
				if (inputBoxes[2].Text.Length < 6)
				{
					EODialog.Show(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);
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
			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(157, 195), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok))
			{
				Visible = true
			};
			ok.OnClick += (s, e) =>
			{
				if(inputBox.Text.Length < 4)
				{
					EODialog.Show(DATCONST1.CHARACTER_CREATE_NAME_TOO_SHORT);
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
		private readonly string map;
		private readonly string item;
		private readonly string npc;
		private readonly string skill;
		private readonly string classes;
		private readonly string loading;

		private readonly Texture2D bgSprites;
		private int bgSrcIndex;
		private TimeSpan? timeOpened;

		private bool updatingFiles;

		public WelcomeMessageData WelcomeData { get; private set; }

		public EOConnectingDialog(PacketAPI apiHandle)
			: base(apiHandle)
		{
			bgTexture = null; //don't use the built in bgtexture, we're going to use a sprite sheet for the BG
			bgSprites = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 33);
			DrawLocation = new Vector2(Game.GraphicsDevice.PresentationParameters.BackBufferWidth - (bgSprites.Width / 4) - 10, 
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight - bgSprites.Height - 10);
			_setSize(bgSprites.Width / 4, bgSprites.Height);
			bgSrcIndex = 0;

			EDFFile file = World.Instance.DataFiles[World.Instance.Localized2];
			map = file.Data[(int) DATCONST2.LOADING_GAME_UPDATING_MAP];
			item = file.Data[(int) DATCONST2.LOADING_GAME_UPDATING_ITEMS];
			npc = file.Data[(int) DATCONST2.LOADING_GAME_UPDATING_NPCS];
			skill = file.Data[(int) DATCONST2.LOADING_GAME_UPDATING_SKILLS];
			classes = file.Data[(int) DATCONST2.LOADING_GAME_UPDATING_CLASSES];
			loading = file.Data[(int) DATCONST2.LOADING_GAME_LOADING_GAME];

			caption = new XNALabel(new Rectangle(12, 9, 1, 1), "Microsoft Sans Serif", 10.0f)
			{
				Text = file.Data[(int) DATCONST2.LOADING_GAME_PLEASE_WAIT],
				ForeColor = System.Drawing.Color.FromArgb(0xf0, 0xf0, 0xc8)
			};
			caption.SetParent(this);

			Random gen = new Random();
			int msgTxt = gen.Next((int) DATCONST2.LOADING_GAME_HINT_FIRST, (int) DATCONST2.LOADING_GAME_HINT_LAST);

			message = new XNALabel(new Rectangle(18, 61, 1, 1), "Microsoft Sans Serif", 8.0f)
			{
				TextWidth = 175,
				ForeColor = System.Drawing.Color.FromArgb(0xb9, 0xb9, 0xb9),
				Text = file.Data[msgTxt]
			};
			message.SetParent(this);

			endConstructor(false);
		}

		public override async void Update(GameTime gt)
		{
			if (timeOpened == null)
			{
				timeOpened = gt.TotalGameTime;
			}

#if DEBUG
			const int intialTimeDelay = 0; //set to zero on debug builds
#else
			const int intialTimeDelay = 5; //I think the client waits 5 seconds?
#endif

			if (!updatingFiles && ((int) gt.TotalGameTime.TotalSeconds - (int) (timeOpened.Value.TotalSeconds)) >= intialTimeDelay)
			{
				updatingFiles = true;

				bool result = await FetchFilesAsync();
				Close(null, result ? XNADialogResult.OK : XNADialogResult.NO_BUTTON_PRESSED);
			}

			//every half a second
			if (((int) gt.TotalGameTime.TotalMilliseconds - (int) (timeOpened.Value.TotalMilliseconds))%500 == 0)
			{
				//switch the background image to the next one
				bgSrcIndex = bgSrcIndex == 3 ? 0 : bgSrcIndex + 1;
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

		private async Task<bool> FetchFilesAsync()
		{
			if (!await _getMapTask())
				return false;
			if (!await _getEIFTask())
				return false;
			if (!await _getENFTask())
				return false;
			if (!await _getESFTask())
				return false;
			if (!await _getECFTask())
				return false;

			caption.Text = loading;
			WelcomeMessageData data;
			if (!m_api.WelcomeMessage(World.Instance.MainPlayer.ActiveCharacter.ID, out data))
				return false;
			WelcomeData = data;

			await TaskFramework.Delay(1000);
			return true;
		}

		#region Task Methods

		private async Task<bool> _getMapTask()
		{
			if (World.Instance.NeedMap != -1)
			{
				caption.Text = map;
				int tries = 0;
				do
				{
					if (tries++ == 3)
						return false;

					if (!m_api.RequestFile(InitFileType.Map, World.Instance.MainPlayer.ActiveCharacter.CurrentMap))
						return false;
				} while (!_isMapValid());
				await TaskFramework.Delay(1000);
			}
			return true;
		}

		private async Task<bool> _getEIFTask()
		{
			if (World.Instance.NeedEIF)
			{
				caption.Text = item;
				int tries = 0;
				do
				{
					if (tries++ == 3)
						return false;

					if (!m_api.RequestFile(InitFileType.Item))
						return false;
				} while (!_isPubValid(InitFileType.Item));
				await TaskFramework.Delay(1000);
			}
			return true;
		}

		private async Task<bool> _getENFTask()
		{
			if (World.Instance.NeedENF)
			{
				caption.Text = npc;
				int tries = 0;
				do
				{
					if (tries++ == 3)
						return false;

					if (!m_api.RequestFile(InitFileType.Npc))
						return false;
				} while (!_isPubValid(InitFileType.Npc));
				await TaskFramework.Delay(1000);
			}
			return true;
		}

		private async Task<bool> _getESFTask()
		{
			if (World.Instance.NeedESF)
			{
				caption.Text = skill;
				int tries = 0;
				do
				{
					if (tries++ == 3)
						return false;

					if (!m_api.RequestFile(InitFileType.Spell))
						return false;
				} while (!_isPubValid(InitFileType.Spell));
				await TaskFramework.Delay(1000);
			}
			return true;
		}

		private async Task<bool> _getECFTask()
		{
			if (World.Instance.NeedECF)
			{
				caption.Text = classes;
				int tries = 0;
				do
				{
					if (tries++ == 3)
						return false;

					if (!m_api.RequestFile(InitFileType.Class))
						return false;
				} while (!_isPubValid(InitFileType.Class));
				await TaskFramework.Delay(1000);
			}
			return true;
		}

		#endregion

		#region File Validation

		private static bool _isMapValid()
		{
			try
			{
				MapFile mapFile = new MapFile(string.Format(Constants.MapFileFormatString, World.Instance.NeedMap));
			}
			catch { return false; }

			return true;
		}

		private static bool _isPubValid(InitFileType fileType)
		{
			try
			{
				EODataFile file;
				switch (fileType)
				{
					case InitFileType.Item:
						file = new ItemFile();
						break;
					case InitFileType.Npc:
						file = new NPCFile();
						break;
					case InitFileType.Spell:
						file = new SpellFile();
						break;
					case InitFileType.Class:
						file = new ClassFile();
						break;
					default:
						return false;
				}

				if (file.Data.Count <= 1) return false;
			}
			catch
			{
				return false;
			}

			return true;
		}

		#endregion
	}

	public class EOPaperdollItem : XNAControl
	{
		private ItemRecord m_info;
		private Texture2D m_gfx;
		private Rectangle m_area;

		public EquipLocation EquipLoc { get; private set; }
		//public short ItemID { get { return (short)(m_info ?? new ItemRecord()).ID; } }

		private readonly PacketAPI m_api;

		public EOPaperdollItem(PacketAPI api, Rectangle location, EOPaperdollDialog parent, ItemRecord info, EquipLocation locationEnum)
			: base(null, null, parent)
		{
			m_api = api;
			SetInfo(location, info);
			EquipLoc = locationEnum;
		}

		public override void Update(GameTime gameTime)
		{
			if (!Game.IsActive) return;

			MouseState currentState = Mouse.GetState();

			if (MouseOver && !MouseOverPreviously)
			{
				DATCONST2 msg;
				switch (EquipLoc)
				{
					case EquipLocation.Boots: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_BOOTS_EQUIPMENT; break;
					case EquipLocation.Accessory: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_MISC_EQUIPMENT; break;
					case EquipLocation.Gloves: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_GLOVES_EQUIPMENT; break;
					case EquipLocation.Belt: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_BELT_EQUIPMENT; break;
					case EquipLocation.Armor: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_ARMOR_EQUIPMENT; break;
					case EquipLocation.Necklace: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_NECKLACE_EQUIPMENT; break;
					case EquipLocation.Hat: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_HAT_EQUIPMENT; break;
					case EquipLocation.Shield: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_SHIELD_EQUIPMENT; break;
					case EquipLocation.Weapon: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_WEAPON_EQUIPMENT; break;
					case EquipLocation.Ring1:
					case EquipLocation.Ring2: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_RING_EQUIPMENT; break;
					case EquipLocation.Armlet1:
					case EquipLocation.Armlet2: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_ARMLET_EQUIPMENT; break;
					case EquipLocation.Bracer1:
					case EquipLocation.Bracer2: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_BRACER_EQUIPMENT; break;
					default: throw new ArgumentOutOfRangeException();
				}

				if (m_info != null)
					EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, msg, ", " + m_info.Name);
				else
					EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, msg);
			}

			//unequipping an item via right-click
			if (m_info != null && MouseOver && currentState.RightButton == ButtonState.Released &&
			    PreviousMouseState.RightButton == ButtonState.Pressed)
			{
				if (((EOPaperdollDialog) parent).CharRef == World.Instance.MainPlayer.ActiveCharacter)
				{ //the parent dialog must show equipment for mainplayer
					if (m_info.Special == ItemSpecial.Cursed)
					{
						EODialog.Show(DATCONST1.ITEM_IS_CURSED_ITEM, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					}
					else if (!((EOGame) Game).Hud.InventoryFits((short) m_info.ID))
					{
						((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_ITEM_UNEQUIP_NO_SPACE_LEFT);
					}
					else
					{
						_setSize(m_area.Width, m_area.Height);
						DrawLocation = new Vector2(m_area.X + (m_area.Width/2 - DrawArea.Width/2),
							m_area.Y + (m_area.Height/2 - DrawArea.Height/2));

						//put back in the inventory by the packet handler response
						string locName = Enum.GetName(typeof (EquipLocation), EquipLoc);
						if (!string.IsNullOrEmpty(locName))
							m_api.UnequipItem((short) m_info.ID, (byte) (locName.Contains("2") ? 1 : 0));

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
		public static EOPaperdollDialog Instance { get; private set; }

		public static void Show(PacketAPI api, Character character, PaperdollDisplayData data)
		{
			if (Instance != null)
				return;
			Instance = new EOPaperdollDialog(api, character, data);
			Instance.DialogClosing += (o, e) => Instance = null;
		}

		public Character CharRef { get; private set; }

		private readonly Texture2D m_characterIcon;

		private static readonly Rectangle m_characterIconRect = new Rectangle(227, 258, 44, 21);

		private EOPaperdollDialog(PacketAPI api, Character character, PaperdollDisplayData data)
			: base(api)
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
			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(276, 253), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok)) { Visible = true };
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
					EOPaperdollItem nextItem = new EOPaperdollItem(m_api, itemArea, this, info, (EquipLocation)i); //auto-added as child of this dialog
				}
				else
				{
// ReSharper disable once UnusedVariable
					EOPaperdollItem nextItem = new EOPaperdollItem(m_api, itemArea, this, null, (EquipLocation)i);
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
					Text = data.Home.Length > 0 ? char.ToUpper(data.Home[0]) + data.Home.Substring(1) : ""
				}, //home
				new XNALabel(new Rectangle(228, 82, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = ((ClassRecord)(World.Instance.ECF.Data.Find(_dat => ((ClassRecord)_dat).ID == CharRef.Class) ?? new ClassRecord())).Name //Check for nulls, for teh lolz
				}, //class
				new XNALabel(new Rectangle(228, 112, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = data.Partner.Length > 0 ? char.ToUpper(data.Partner[0]) + data.Partner.Substring(1) : ""
				}, //partner
				new XNALabel(new Rectangle(228, 142, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = CharRef.Title.Length > 0 ? char.ToUpper(CharRef.Title[0]) + CharRef.Title.Substring(1) : ""
				}, //title
				new XNALabel(new Rectangle(228, 202, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = data.Guild.Length > 0 ? char.ToUpper(data.Guild[0]) + data.Guild.Substring(1) : ""
				}, //guild
				new XNALabel(new Rectangle(228, 232, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					Text = data.Rank.Length > 0 ? char.ToUpper(data.Rank[0]) + data.Rank.Substring(1) : ""
				} //rank
			};

			labels.ToList().ForEach(_l => { _l.ForeColor = System.Drawing.Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8); _l.SetParent(this); });

			ChatType iconType = EOChatRenderer.GetChatTypeFromPaperdollIcon(data.Icon);
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
			GiveItems,
			TradeItems,
			ShopTransfer,
			BankTransfer
		}

		private readonly Texture2D m_titleBarGfx;
		private readonly int m_totalAmount;

		private readonly XNATextBox m_amount;
		public int SelectedAmount
		{
			get { return int.Parse(m_amount.Text); }
		}

		private readonly IKeyboardSubscriber m_prevSubscriber;

		private static bool s_sliderDragging;

		/// <summary>
		/// Create a new item transfer dialog
		/// </summary>
		/// <param name="itemName">Name of the item to be displayed</param>
		/// <param name="transferType">Which transfer is being done (controls title)</param>
		/// <param name="totalAmount">Maximum amount that can be transferred</param>
		/// <param name="message">Resource ID of message to control displayed text</param>
		public EOItemTransferDialog(string itemName, TransferType transferType, int totalAmount, DATCONST2 message = DATCONST2.DIALOG_TRANSFER_DROP)
		{
			_validateMessage(message);

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
			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(60, 125), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok))
			{
				Visible = true
			};
			ok.OnClick += (s, e) => Close(ok, XNADialogResult.OK);
			ok.SetParent(this);
			dlgButtons.Add(ok);

			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(153, 125), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel))
			{
				Visible = true
			};
			cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
			cancel.SetParent(this);
			dlgButtons.Add(cancel);

			XNALabel descLabel = new XNALabel(new Rectangle(20, 42, 231, 33), "Microsoft Sans Serif", 10.0f)
			{
				ForeColor = System.Drawing.Color.FromArgb(0xe6, 0xe6, 0xd6),
				TextWidth = 200,
				Text = string.Format("{0} {1} {2}", World.GetString(DATCONST2.DIALOG_TRANSFER_HOW_MUCH), itemName, World.GetString(message))
			};
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
			m_amount.SetParent(this);
			m_prevSubscriber = EOGame.Instance.Dispatcher.Subscriber;
			EOGame.Instance.Dispatcher.Subscriber = m_amount;
			DialogClosing += (o, e) => EOGame.Instance.Dispatcher.Subscriber = m_prevSubscriber;

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
				s_sliderDragging = true; //ignores updates to slider location during text change
				MouseState st = Mouse.GetState();
				Rectangle sliderArea = new Rectangle(25, 96, 122 - slider.DrawArea.Width, 15);
				int newX = (st.X - PreviousMouseState.X) + (int)slider.DrawLocation.X;
				if (newX < sliderArea.X) newX = sliderArea.X;
				else if (newX > sliderArea.Width + sliderArea.X) newX = sliderArea.Width + sliderArea.X;
				slider.DrawLocation = new Vector2(newX, slider.DrawLocation.Y); //unchanged y coordinate, slides along x-axis

				float ratio = (newX - sliderArea.X)/(float)sliderArea.Width;
				m_amount.Text = ((int) Math.Round(ratio*m_totalAmount) + 1).ToString(CultureInfo.InvariantCulture);
				s_sliderDragging = false;
			};
			slider.SetParent(this);

			m_amount.OnTextChanged += (sender, args) =>
			{
				int amt = 0;
				if (m_amount.Text != "" && (!int.TryParse(m_amount.Text, out amt) || amt > m_totalAmount))
				{
					amt = m_totalAmount;
					m_amount.Text = string.Format("{0}", m_totalAmount);
				}
				else if (m_amount.Text != "" && amt < 0)
				{
					amt = 1;
					m_amount.Text = string.Format("{0}", amt);
				}

				if (s_sliderDragging) return; //slider is being dragged - don't move its position

				//adjust the slider (created after m_amount) when the text changes
				if (amt <= 1) //NOT WORKING
				{
					slider.DrawLocation = new Vector2(25, 96);
				}
				else
				{
					int xCoord = (int)Math.Round((amt / (double)m_totalAmount) * (122 - slider.DrawArea.Width));
					slider.DrawLocation = new Vector2(25 + xCoord, 96);
				}
			};

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

		private void _validateMessage(DATCONST2 msg)
		{
			switch (msg)
			{
				case DATCONST2.DIALOG_TRANSFER_DROP:
				case DATCONST2.DIALOG_TRANSFER_GIVE:
				case DATCONST2.DIALOG_TRANSFER_JUNK:
				case DATCONST2.DIALOG_TRANSFER_BUY:
				case DATCONST2.DIALOG_TRANSFER_SELL:
				case DATCONST2.DIALOG_TRANSFER_TRANSFER:
				case DATCONST2.DIALOG_TRANSFER_DEPOSIT:
				case DATCONST2.DIALOG_TRANSFER_WITHDRAW:
				case DATCONST2.DIALOG_TRANSFER_OFFER:
					break;
				default: throw new ArgumentOutOfRangeException("msg", "Use one of the approved messages.");
			}
		}
	}

	public class EODialogListItem : XNAControl
	{
		private static readonly object disposingLock = new object();
		private bool m_disposing;

		/// <summary>
		/// Optional item ID to use for this List Item Record
		/// </summary>
		public short ID { get; set; }
		/// <summary>
		/// Optional item amount to use for this List Item Record
		/// </summary>
		public int Amount { get; set; }
		
		private int m_index;
		/// <summary>
		/// Get or Set the index within the parent control. 
		/// </summary>
		public int Index
		{
			get { return m_index; }
			set
			{
				m_index = value;
				DrawLocation = new Vector2(DrawLocation.X, OffsetY + (m_index * (Style == ListItemStyle.Large ? 36 : 16)));
			}
		}

		private int m_xOffset, m_yOffset;

		public int OffsetX
		{
			get
			{
				return m_xOffset;
			}
			set
			{
				int oldOff = m_xOffset;
				m_xOffset = value;
				DrawLocation = DrawLocation + new Vector2(m_xOffset - oldOff, 0);
			}
		}

		/// <summary>
		/// Starting Y Offset to draw list item controls
		/// </summary>
		public int OffsetY
		{
			get
			{
				return m_yOffset;
			}
			set
			{
				int oldOff = m_yOffset;
				m_yOffset = value;
				DrawLocation = DrawLocation + new Vector2(0, m_yOffset - oldOff);
			}
		}

		/// <summary>
		/// Style of the control - either small (single text row) or large (graphic w/two rows of text)
		/// </summary>
		public ListItemStyle Style { get; set; }

		/// <summary>
		/// For Large style control, sets whether or not the item graphic has a background image (ie red pad thing)
		/// </summary>
		public bool ShowItemBackGround { get; set; }

		/// <summary>
		/// Get or set the primary text
		/// </summary>
		public string Text
		{
			get { return m_primaryText.Text; }
			set
			{
				m_primaryText.Text = value;
				m_primaryText.ResizeBasedOnText();
			}
		}

		/// <summary>
		/// Get or set the secondary text
		/// </summary>
		public string SubText
		{
			get { return m_secondaryText.Text; }
			set
			{
				m_secondaryText.Text = value;
				m_secondaryText.ResizeBasedOnText();
			}
		}

		public Texture2D IconGraphic
		{
			get { return m_gfxItem; }
			set { m_gfxItem = value; }
		}

		public event EventHandler OnRightClick;
		public event EventHandler OnLeftClick;

		protected XNALabel m_primaryText;
		protected XNALabel m_secondaryText;

		private readonly Texture2D m_gfxPadThing;
		private Texture2D m_gfxItem;
		private readonly Texture2D m_backgroundColor;
		private bool m_drawBackground;
		private bool m_rightClicked;

		public enum ListItemStyle
		{
			Small,
			Large
		}

		public EODialogListItem(EODialogBase parent, ListItemStyle style, int listIndex = -1)
		{
			DrawLocation = new Vector2(17, DrawLocation.Y); //the base X coordinate is 17 - this can be adjusted with OffsetX property
			
			Style = style;
			if(listIndex >= 0)
				Index = listIndex;

			_setSize(232, Style == ListItemStyle.Large ? 36 : 13);

			int colorFactor = Style == ListItemStyle.Large ? 0xc8 : 0xb4;

			m_primaryText = new XNALabel(new Rectangle(Style == ListItemStyle.Large ? 56 : 2, Style == ListItemStyle.Large ? 5 : 0, 1, 1), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = false,
				BackColor = System.Drawing.Color.Transparent,
				ForeColor = System.Drawing.Color.FromArgb(255, colorFactor, colorFactor, colorFactor),
				TextAlign = ContentAlignment.TopLeft,
				Text = " "
			};
			m_primaryText.ResizeBasedOnText();

			if (Style == ListItemStyle.Large)
			{
				m_secondaryText = new XNALabel(new Rectangle(56, 20, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					AutoSize = true,
					BackColor = m_primaryText.BackColor,
					ForeColor = m_primaryText.ForeColor,
					Text = " "
				};
				m_secondaryText.ResizeBasedOnText();

				m_gfxPadThing = GFXLoader.TextureFromResource(GFXTypes.MapTiles, 0, true);
				ShowItemBackGround = true;
			}
			m_backgroundColor = new Texture2D(Game.GraphicsDevice, 1, 1);
			m_backgroundColor.SetData(new [] {Color.FromNonPremultiplied(0xff, 0xff, 0xff, 64)});

			SetParent(parent);
			m_primaryText.SetParent(this);
			if (Style == ListItemStyle.Large)
			{
				m_secondaryText.SetParent(this);
			}
			OffsetY = Style == ListItemStyle.Large ? 25 : 45;
		}

		/// <summary>
		/// turns the primary text into a link that performs the specified action. When Style is Small, the entire item becomes clickable.
		/// </summary>
		/// <param name="onClickAction">The action to perform</param>
		public void SetPrimaryTextLink(Action onClickAction)
		{
			if (m_primaryText == null)
				return;
			XNALabel oldText = m_primaryText;
			m_primaryText = new XNAHyperLink(oldText.DrawArea, oldText.Font.FontFamily.Name, 8.5f)
			{
				AutoSize = false,
				BackColor = oldText.BackColor,
				ForeColor = oldText.ForeColor,
				HighlightColor = oldText.ForeColor,
				Text = oldText.Text,
				Style = FontStyle.Underline
			};
			m_primaryText.ResizeBasedOnText();
			((XNAHyperLink) m_primaryText).OnClick += (o, e) => onClickAction();
			m_primaryText.SetParent(this);
			oldText.Close();

			if (Style == ListItemStyle.Small)
				OnLeftClick += (o, e) => onClickAction();
		}

		//turns the subtext into a link that performs the specified action
		public void SetSubtextLink(Action onClickAction)
		{
			if (m_secondaryText == null || Style == ListItemStyle.Small)
				return;
			XNALabel oldText = m_secondaryText;
			m_secondaryText = new XNAHyperLink(oldText.DrawArea, oldText.Font.FontFamily.Name, 8.5f)
			{
				AutoSize = false,
				BackColor = oldText.BackColor,
				ForeColor = oldText.ForeColor,
				HighlightColor = oldText.ForeColor,
				Text = oldText.Text,
				Style = FontStyle.Underline
			};
			m_secondaryText.ResizeBasedOnText();
			((XNAHyperLink) m_secondaryText).OnClick += (o, e) => onClickAction();
			m_secondaryText.SetParent(this);
			oldText.Close();
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible || !Game.IsActive) return;

			lock (disposingLock)
			{
				if (m_disposing) return;

				MouseState ms = Mouse.GetState();

				if (MouseOver && MouseOverPreviously)
				{
					m_drawBackground = true;
					if (ms.RightButton == ButtonState.Pressed)
					{
						m_rightClicked = true;
					}

					if (m_rightClicked && ms.RightButton == ButtonState.Released && OnRightClick != null)
					{
						OnRightClick(this, null);
						m_rightClicked = false;
					}
					else if (PreviousMouseState.LeftButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released &&
					         OnLeftClick != null)
					{
						//If the sub text is a hyperlink and the mouse is over it do the click event for the sub text and not for this item
						if (m_secondaryText is XNAHyperLink && m_secondaryText.MouseOver)
							((XNAHyperLink) m_secondaryText).Click();
						else
							OnLeftClick(this, null);
					}
				}
				else
				{
					m_drawBackground = false;
				}

				base.Update(gameTime);
			}
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible) return;

			lock (disposingLock)
			{
				if (m_disposing)
					return;
				SpriteBatch.Begin();
				if (m_drawBackground)
				{
					//Rectangle backgroundRect = new Rectangle(DrawAreaWithOffset.X + OffsetX, DrawAreaWithOffset.Y + OffsetY, DrawAreaWithOffset.Width, DrawAreaWithOffset.Height);
					SpriteBatch.Draw(m_backgroundColor, DrawAreaWithOffset, Color.White);
				}
				if (Style == ListItemStyle.Large)
				{
					//The area for showing these is 64x36px: center the icon and background accordingly
					Vector2 offset = new Vector2(xOff + OffsetX + 14/*not sure of the significance of this offset*/, yOff + OffsetY + 36*Index);
					if (ShowItemBackGround)
						SpriteBatch.Draw(m_gfxPadThing, new Vector2(offset.X + ((64 - m_gfxPadThing.Width)/2f), offset.Y + (36 - m_gfxPadThing.Height)/2f), Color.White);
					if (m_gfxItem != null)
						SpriteBatch.Draw(m_gfxItem,
							new Vector2((float) Math.Round(offset.X + ((64 - m_gfxItem.Width)/2f)),
								(float) Math.Round(offset.Y + (36 - m_gfxItem.Height)/2f)),
							Color.White);
				}
				SpriteBatch.End();
				base.Draw(gameTime);
			}
		}

		public void SetActive()
		{
			m_primaryText.ForeColor = System.Drawing.Color.FromArgb(0xff, 0xf0, 0xf0, 0xf0);
		}

		protected override void Dispose(bool disposing)
		{
			lock (disposingLock)
			{
				m_disposing = true;
				if (disposing)
				{
					m_backgroundColor.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}

	public class EOChestDialog : EODialogBase
	{
		public static EOChestDialog Instance { get; private set; }

		public static void Show(PacketAPI apiHandle, byte chestX, byte chestY)
		{
			if (Instance != null)
				return;

			Instance = new EOChestDialog(apiHandle, chestX, chestY);
			Instance.DialogClosing += (o, e) => Instance = null;

			if (!apiHandle.ChestOpen(chestX, chestY))
			{
				Instance.Close(null, XNADialogResult.NO_BUTTON_PRESSED);
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			}
		}

		public byte CurrentChestX { get; private set; }
		public byte CurrentChestY { get; private set; }

		private EODialogListItem[] m_items;

		private EOChestDialog(PacketAPI api, byte chestX, byte chestY)
			: base(api)
		{
			CurrentChestX = chestX;
			CurrentChestY = chestY;

			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(92, 227), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
			cancel.OnClick += (sender, e) => Close(cancel, XNADialogResult.Cancel);
			dlgButtons.Add(cancel);
			whichButtons = XNADialogButtons.Cancel;

			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 51);
			_setSize(bgTexture.Width, bgTexture.Height);
			
			endConstructor(false);
			DrawLocation = new Vector2((Game.GraphicsDevice.PresentationParameters.BackBufferWidth - DrawArea.Width) / 2f, 15);
			cancel.SetParent(this);

			EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_CHEST_YOU_OPENED,
				World.GetString(DATCONST2.STATUS_LABEL_DRAG_AND_DROP_ITEMS));
		}

		public void InitializeItems(IList<Tuple<short, int>> initialItems)
		{
			if(m_items == null)
				m_items = new EODialogListItem[5];

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

					ItemRecord rec = World.Instance.EIF.GetItemRecordByID(item.Item1);
					string secondary = string.Format("x {0}  {1}", item.Item2, rec.Type == ItemType.Armor
						? "(" + (rec.Gender == 0 ? World.GetString(DATCONST2.FEMALE) : World.GetString(DATCONST2.MALE)) + ")"
						: "");

					m_items[i] = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, i)
					{
						Text = rec.Name,
						SubText = secondary,
						IconGraphic = GFXLoader.TextureFromResource(GFXTypes.Items, 2 * rec.Graphic - 1, true),
						ID = item.Item1
					};
					m_items[i].OnRightClick += (o, e) =>
					{
						EODialogListItem sender = o as EODialogListItem;
						if (sender == null) return;

						if (!EOGame.Instance.Hud.InventoryFits(sender.ID))
						{
							string _message = World.GetString(DATCONST2.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
							string _caption = World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING);
							EODialog.Show(_message, _caption, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
							((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
						}
						else if (rec.Weight*item.Item2 + World.Instance.MainPlayer.ActiveCharacter.Weight >
						         World.Instance.MainPlayer.ActiveCharacter.MaxWeight)
						{
							EODialog.Show(World.GetString(DATCONST2.DIALOG_ITS_TOO_HEAVY_WEIGHT),
								World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
								XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
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
			foreach(XNAControl child in children)
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
	}

	/// <summary>
	/// Implements a dialog that scrolls through a list of items and has a title and configurable buttons.
	/// </summary>
	public class EOScrollingListDialog : EODialogBase
	{
		//needs: - title label
		//		 - scroll bar
		//		 - lower buttons (configurable)
		//		 - list of items (names, like friend list; items, like shop)
		//EODialogListItem needs way to set width and offsets

		private readonly List<EODialogListItem> m_listItems = new List<EODialogListItem>();
		private readonly object m_listItemLock = new object();
		protected EOScrollBar m_scrollBar;

		/// <summary>
		/// List of strings containing the primary text field of each child item
		/// </summary>
		public List<string> NamesList
		{
			get
			{
				lock(m_listItemLock)
					return m_listItems.Select(item => item.Text).ToList();
			}
		}

		protected XNALabel m_titleText;
		private EODialogListItem.ListItemStyle _listItemType;
		private ScrollingListDialogButtons _buttons;

		public string Title
		{
			get { return m_titleText.Text; }
			set { m_titleText.Text = value; }
		}

		/// <summary>
		/// The number of items to display in the scrolling view at one time when ListItemType is Large
		/// </summary>
		public int LargeItemStyleMaxItemDisplay { get; set; }

		/// <summary>
		/// The number of items to display in the scrolling view at one time when ListItemType is Small
		/// </summary>
		public int SmallItemStyleMaxItemDisplay { get; set; }

		public EODialogListItem.ListItemStyle ListItemType
		{
			get { return _listItemType; }
			set
			{
				_listItemType = value;
				m_scrollBar.LinesToRender = ListItemType == EODialogListItem.ListItemStyle.Small
					? SmallItemStyleMaxItemDisplay
					: LargeItemStyleMaxItemDisplay;
			}
		}

		public ScrollingListDialogButtons Buttons
		{
			get { return _buttons; }
			set
			{
				_buttons = value;
				_setButtons(_buttons);
			}
		}

		public EOScrollingListDialog(PacketAPI api = null)
			: base(api)
		{
			_setBackgroundTexture(GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 52));

			//defaults
			LargeItemStyleMaxItemDisplay = 5;
			SmallItemStyleMaxItemDisplay = 12;

			m_titleText = new XNALabel(new Rectangle(16, 13, 253, 19), "Microsoft Sans Serif", 8.75f)
			{
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				ForeColor = System.Drawing.Color.FromArgb(0xff,0xc8,0xc8,0xc8)
			};
			m_titleText.SetParent(this);

			m_scrollBar = new EOScrollBar(this, new Vector2(252, 44), new Vector2(16, 199), EOScrollBar.ScrollColors.LightOnMed);

			Center(Game.GraphicsDevice);
			DrawLocation = new Vector2(DrawLocation.X, 15);
			endConstructor(false);
		}

		public void SetItemList(List<EODialogListItem> itemList)
		{
			if (itemList.Count == 0) return;

			if (m_listItems.Count == 0)
				m_scrollBar.LinesToRender = itemList[0].Style == EODialogListItem.ListItemStyle.Large ? 5 : 12;

			m_scrollBar.UpdateDimensions(itemList.Count);

			EODialogListItem.ListItemStyle firstStyle = itemList[0].Style;
			lock (m_listItemLock)
				for (int i = 0; i < itemList.Count; ++i)
				{
					m_listItems.Add(itemList[i]);
					m_listItems[i].Style = firstStyle;
					m_listItems[i].Index = i;
					if (i > m_scrollBar.LinesToRender)
						m_listItems[i].Visible = false;
				}
		}

		public void AddItemToList(EODialogListItem item, bool sortList)
		{
			if (m_listItems.Count == 0)
				m_scrollBar.LinesToRender = item.Style == EODialogListItem.ListItemStyle.Large ? LargeItemStyleMaxItemDisplay : SmallItemStyleMaxItemDisplay;
			lock (m_listItemLock)
			{
				m_listItems.Add(item);
				if (sortList)
					m_listItems.Sort((item1, item2) => item1.Text.CompareTo(item2.Text));
				for (int i = 0; i < m_listItems.Count; ++i)
					m_listItems[i].Index = i;
			}
			m_scrollBar.UpdateDimensions(m_listItems.Count);
		}

		public void RemoveFromList(EODialogListItem item)
		{
			int ndx;
			lock (m_listItemLock)
				ndx = m_listItems.FindIndex(_item => _item == item);
			if (ndx < 0) return;

			item.Close();

			lock (m_listItemLock)
			{
				m_listItems.RemoveAt(ndx);

				m_scrollBar.UpdateDimensions(m_listItems.Count);
				if (m_listItems.Count <= m_scrollBar.LinesToRender)
					m_scrollBar.ScrollToTop();

				for (int i = 0; i < m_listItems.Count; ++i)
				{
					//adjust indices (determines drawing position)
					m_listItems[i].Index = i;
				}
			}
		}

		public void SetActiveItemList(List<string> activeLabels)
		{
			lock (m_listItemLock)
				foreach (EODialogListItem item in m_listItems)
				{
					if (activeLabels.Select(x => x.ToLower()).Contains(item.Text.ToLower()))
					{
						item.SetActive();
					}
				}
		}

		protected void ClearItemList()
		{
			lock (m_listItemLock)
			{
				foreach (EODialogListItem item in m_listItems)
				{
					item.SetParent(null);
					item.Close();
				}
				m_listItems.Clear();
			}
			m_scrollBar.UpdateDimensions(0);
			m_scrollBar.ScrollToTop();
		}

		protected void _setBackgroundTexture(Texture2D text)
		{
			bgTexture = text;
			_setSize(bgTexture.Width, bgTexture.Height);
		}

		protected void _setButtons(ScrollingListDialogButtons setButtons)
		{
			if (dlgButtons.Count > 0)
			{
				dlgButtons.ForEach(_btn =>
				{
					_btn.SetParent(null);
					_btn.Close();
				});

				dlgButtons.Clear();
			}

			_buttons = setButtons;
			switch (setButtons)
			{
				case ScrollingListDialogButtons.BackCancel:
				case ScrollingListDialogButtons.AddCancel:
				{
					SmallButton which = setButtons == ScrollingListDialogButtons.BackCancel ? SmallButton.Back : SmallButton.Add;
					XNAButton add = new XNAButton(smallButtonSheet, new Vector2(48, 252), _getSmallButtonOut(which), _getSmallButtonOver(which));
					add.SetParent(this);
					add.OnClick += (o, e) => Close(add, setButtons == ScrollingListDialogButtons.BackCancel ? XNADialogResult.Back : XNADialogResult.Add);
					XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(144, 252), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
					cancel.SetParent(this);
					cancel.OnClick += (o, e) => Close(cancel, XNADialogResult.Cancel);

					dlgButtons.Add(add);
					dlgButtons.Add(cancel);
				}
					break;
				case ScrollingListDialogButtons.Cancel:
				{
					XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(96, 252), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
					cancel.SetParent(this);
					cancel.OnClick += (o, e) => Close(cancel, XNADialogResult.Cancel);

					dlgButtons.Add(cancel);
				}
					break;
			}
		}

		public override void Update(GameTime gt)
		{
			//which items should we render?
			lock (m_listItemLock)
			{
				if (m_listItems.Count > m_scrollBar.LinesToRender)
				{
					for (int i = 0; i < m_listItems.Count; ++i)
					{
						EODialogListItem curr = m_listItems[i];
						if (i < m_scrollBar.ScrollOffset)
						{
							curr.Visible = false;
							continue;
						}

						if (i < m_scrollBar.LinesToRender + m_scrollBar.ScrollOffset)
						{
							curr.Visible = true;
							curr.Index = i - m_scrollBar.ScrollOffset;
						}
						else
						{
							curr.Visible = false;
						}
					}
				}
				else if (m_listItems.Any(_item => !_item.Visible))
					m_listItems.ForEach(_item => _item.Visible = true); //all items visible if less than # lines to render
			}

			base.Update(gt);
		}
	}

	public class EOInputDialog : EODialogBase
	{
		private readonly XNATextBox m_inputBox;
		private readonly IKeyboardSubscriber previousSubscriber;

		public string ResponseText { get { return m_inputBox.Text; } }

		public EOInputDialog(string prompt, int maxInputChars = 12)
		{
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 54);
			_setSize(bgTexture.Width, bgTexture.Height);

			XNALabel lblPrompt = new XNALabel(new Rectangle(16, 20, 235, 49), "Microsoft Sans Serif", 10f)
			{
				AutoSize = false,
				ForeColor = System.Drawing.Color.FromArgb(255, 0xe6, 0xe6, 0xd6),
				TextWidth = 230,
				RowSpacing = 3,
				Text = prompt
			};
			lblPrompt.SetParent(this);

			//set this back once the dialog is closed.
			previousSubscriber = ((EOGame)Game).Dispatcher.Subscriber;
			DialogClosing += (o, e) => ((EOGame)Game).Dispatcher.Subscriber = previousSubscriber;

			m_inputBox = new XNATextBox(new Rectangle(37, 74, 192, 19), EOGame.Instance.Content.Load<Texture2D>("cursor"), "Microsoft Sans Serif", 8.0f)
			{
				MaxChars = maxInputChars,
				LeftPadding = 4,
				TextColor = System.Drawing.Color.FromArgb(0xff, 0xdc, 0xc8, 0xb4)
			};
			m_inputBox.SetParent(this);
			EOGame.Instance.Dispatcher.Subscriber = m_inputBox;

			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(41, 103), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok)),
				cancel = new XNAButton(smallButtonSheet, new Vector2(134, 103), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
			ok.OnClick += (o, e) => Close(ok, XNADialogResult.OK);
			cancel.OnClick += (o, e) => Close(cancel, XNADialogResult.Cancel);
			ok.SetParent(this);
			cancel.SetParent(this);

			Center(Game.GraphicsDevice);
			DrawLocation = new Vector2(DrawLocation.X, 107);
			endConstructor(false);
		}

		public void SetAsKeyboardSubscriber()
		{
			((EOGame) Game).Dispatcher.Subscriber = m_inputBox;
		}
	}

	public static class EOFriendIgnoreListDialog
	{
		private static EOScrollingListDialog Instance;

		public static void Show(PacketAPI apiHandle, bool isIgnoreList)
		{
			if (Instance != null)
				return;

			List<string> allLines = isIgnoreList ? InteractList.LoadAllIgnore() : InteractList.LoadAllFriend();

			string charName = World.Instance.MainPlayer.ActiveCharacter.Name;
			charName = char.ToUpper(charName[0]) + charName.Substring(1);
			string titleText = string.Format("{0}'s {2} [{1}]", charName, allLines.Count,
				World.GetString(isIgnoreList ? DATCONST2.STATUS_LABEL_IGNORE_LIST : DATCONST2.STATUS_LABEL_FRIEND_LIST));

			EOScrollingListDialog dlg = new EOScrollingListDialog
			{
				Title = titleText,
				Buttons = ScrollingListDialogButtons.AddCancel,
				ListItemType = EODialogListItem.ListItemStyle.Small
			};

			List<EODialogListItem> characters = allLines.Select(character => new EODialogListItem(dlg, EODialogListItem.ListItemStyle.Small) { Text= character }).ToList();
			characters.ForEach(character =>
			{
				character.OnLeftClick += (o, e) => EOGame.Instance.Hud.SetChatText("!" + character.Text + " ");
				character.OnRightClick += (o, e) =>
				{
					dlg.RemoveFromList(character);
					dlg.Title = string.Format("{0}'s {2} [{1}]", charName, dlg.NamesList.Count,
						World.GetString(isIgnoreList ? DATCONST2.STATUS_LABEL_IGNORE_LIST : DATCONST2.STATUS_LABEL_FRIEND_LIST));
				};
			});
			dlg.SetItemList(characters);

			dlg.DialogClosing += (o, e) =>
			{
				if (e.Result == XNADialogResult.Cancel)
				{
					Instance = null;
					if (isIgnoreList)
						InteractList.WriteIgnoreList(dlg.NamesList);
					else
						InteractList.WriteFriendList(dlg.NamesList);
				}
				else if (e.Result == XNADialogResult.Add)
				{
					e.CancelClose = true;
					string prompt = World.GetString(isIgnoreList ? DATCONST2.DIALOG_WHO_TO_MAKE_IGNORE : DATCONST2.DIALOG_WHO_TO_MAKE_FRIEND);
					EOInputDialog dlgInput = new EOInputDialog(prompt);
					dlgInput.DialogClosing += (_o, _e) =>
					{
						if (_e.Result == XNADialogResult.Cancel) return;

						if (dlgInput.ResponseText.Length < 4)
						{
							_e.CancelClose = true;
							EODialog.Show(DATCONST1.CHARACTER_CREATE_NAME_TOO_SHORT);
							dlgInput.SetAsKeyboardSubscriber();
							return;
						}

						if (dlg.NamesList.FindIndex(name => name.ToLower() == dlgInput.ResponseText.ToLower()) >= 0)
						{
							_e.CancelClose = true;
							EODialog.Show("You are already friends with that person!", "Invalid entry!", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
							dlgInput.SetAsKeyboardSubscriber();
							return;
						}

						EODialogListItem newItem = new EODialogListItem(dlg, EODialogListItem.ListItemStyle.Small)
						{
							Text = dlgInput.ResponseText
						};
						newItem.OnLeftClick += (oo, ee) => EOGame.Instance.Hud.SetChatText("!" + newItem.Text + " ");
						newItem.OnRightClick += (oo, ee) =>
						{
							dlg.RemoveFromList(newItem);
							dlg.Title = string.Format("{0}'s {2} [{1}]",
								charName,
								dlg.NamesList.Count,
								World.GetString(isIgnoreList ? DATCONST2.STATUS_LABEL_IGNORE_LIST : DATCONST2.STATUS_LABEL_FRIEND_LIST));
						};
						dlg.AddItemToList(newItem, true);
						dlg.Title = string.Format("{0}'s {2} [{1}]", charName, dlg.NamesList.Count,
							World.GetString(isIgnoreList ? DATCONST2.STATUS_LABEL_IGNORE_LIST : DATCONST2.STATUS_LABEL_FRIEND_LIST));
					};
				}
			};
			
			Instance = dlg;

			List<OnlineEntry> onlineList;
			apiHandle.RequestOnlinePlayers(false, out onlineList);
			Instance.SetActiveItemList(onlineList.Select(_oe => _oe.Name).ToList());

			EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, isIgnoreList ? DATCONST2.STATUS_LABEL_IGNORE_LIST : DATCONST2.STATUS_LABEL_FRIEND_LIST, 
				World.GetString(DATCONST2.STATUS_LABEL_USE_RIGHT_MOUSE_CLICK_DELETE));
			//show the dialog
		}
	}

	public class EOShopDialog : EOScrollingListDialog
	{
		/* Process for this:
		 * 1. Click shopkeeper, calls Show()
		 * 2. Show constructs instance and sends packet
		 * 3. When packet response is received, data is populated in dialog
		 * 4. Dialog 'closing' event resets Instance to null
		 */

		/* STATIC INTERFACE */
		public static EOShopDialog Instance { get; private set; }

		public static void Show(PacketAPI api, NPC shopKeeper)
		{
			if (Instance != null)
				return;

			Instance = new EOShopDialog(api, shopKeeper.Data.ID);

			//request from server is based on the map index
			if (!api.RequestShop(shopKeeper.Index))
			{
				Instance.Close();
				Instance = null;
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			}
		}

		private enum ShopState
		{
			None,
			Initial,
			Buying,
			Selling,
			Crafting
		}

		public int ID { get; private set; }

		private ShopState m_state;
		private List<ShopItem> m_tradeItems;
		private List<CraftItem> m_craftItems;

		private static Texture2D BuyIcon, SellIcon, CraftIcon;

		private EOShopDialog(PacketAPI api, int id)
			: base(api)
		{
			Buttons = ScrollingListDialogButtons.Cancel;
			ListItemType = EODialogListItem.ListItemStyle.Large;

			ID = id;
			DialogClosing += (o, e) =>
			{
				if (e.Result == XNADialogResult.Cancel)
				{
					Instance = null;
					ID = 0;
				}
				else if (e.Result == XNADialogResult.Back)
				{
					e.CancelClose = true;
					_setState(ShopState.Initial);
				}
			};
			m_state = ShopState.None;

			//note - may need to lock around these.
			//other note - no good way to dispose static textures like this
			if (BuyIcon == null || SellIcon == null || CraftIcon == null)
			{
				BuyIcon = _getDlgIcon(ListIcon.Buy);
				SellIcon = _getDlgIcon(ListIcon.Sell);
				CraftIcon = _getDlgIcon(ListIcon.Craft);
			}
		}

		public void SetShopData(int id, string Name, List<ShopItem> tradeItems, List<CraftItem> craftItems)
		{
			if (Instance == null || this != Instance || ID != id) return;
			Title = Name;

			m_tradeItems = tradeItems;
			m_craftItems = craftItems;

			_setState(ShopState.Initial);
		}

		private void _setState(ShopState newState)
		{
			ShopState old = m_state;

			if (old == newState) return;
			
			int buyNumInt = m_tradeItems.FindAll(x => x.Buy > 0).Count;
			int sellNumInt = m_tradeItems.FindAll(x => World.Instance.MainPlayer.ActiveCharacter.Inventory.FindIndex(item => item.id == x.ID) >= 0 && x.Sell > 0).Count;

			if (newState == ShopState.Buying && buyNumInt <= 0)
			{
				EODialog.Show(DATCONST1.SHOP_NOTHING_IS_FOR_SALE, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			if (newState == ShopState.Selling && sellNumInt <= 0)
			{
				EODialog.Show(DATCONST1.SHOP_NOT_BUYING_YOUR_ITEMS, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			ClearItemList();
			switch (newState)
			{
				case ShopState.Initial:
				{
					string buyNum = string.Format("{0} {1}", m_tradeItems.FindAll(x => x.Buy > 0).Count, World.GetString(DATCONST2.DIALOG_SHOP_ITEMS_IN_STORE));
					string sellNum = string.Format("{0} {1}", sellNumInt, World.GetString(DATCONST2.DIALOG_SHOP_ITEMS_ACCEPTED));
					string craftNum = string.Format("{0} {1}", m_craftItems.Count, World.GetString(DATCONST2.DIALOG_SHOP_ITEMS_ACCEPTED));

					EODialogListItem buy = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, 0)
					{
						Text = World.GetString(DATCONST2.DIALOG_SHOP_BUY_ITEMS),
						SubText = buyNum,
						IconGraphic = BuyIcon,
						OffsetY = 45
					};
					buy.OnLeftClick += (o, e) => _setState(ShopState.Buying);
					buy.OnRightClick += (o, e) => _setState(ShopState.Buying);
					buy.ShowItemBackGround = false;
					AddItemToList(buy, false);
					EODialogListItem sell = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, 1)
					{
						Text = World.GetString(DATCONST2.DIALOG_SHOP_SELL_ITEMS),
						SubText = sellNum,
						IconGraphic = SellIcon,
						OffsetY = 45
					};
					sell.OnLeftClick += (o, e) => _setState(ShopState.Selling);
					sell.OnRightClick += (o, e) => _setState(ShopState.Selling);
					sell.ShowItemBackGround = false;
					AddItemToList(sell, false);
					if (m_craftItems.Count > 0)
					{
						EODialogListItem craft = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, 2) 
						{
							Text = World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_ITEMS),
							SubText = craftNum,
							IconGraphic = CraftIcon,
							OffsetY = 45
						};
						craft.OnLeftClick += (o, e) => _setState(ShopState.Crafting);
						craft.OnRightClick += (o, e) => _setState(ShopState.Crafting);
						craft.ShowItemBackGround = false;
						AddItemToList(craft, false);
					}
					_setButtons(ScrollingListDialogButtons.Cancel);
				}
					break;
				case ShopState.Buying:
				case ShopState.Selling:
				{
					//re-use the logic for Buying/Selling: it is almost identical
					bool buying = newState == ShopState.Buying;

					List<EODialogListItem> itemList = new List<EODialogListItem>();
					foreach (ShopItem si in m_tradeItems)
					{
						if (si.ID <= 0 || (buying && si.Buy <= 0) || 
							(!buying && (si.Sell <= 0 || World.Instance.MainPlayer.ActiveCharacter.Inventory.FindIndex(inv => inv.id == si.ID) < 0)))
							continue;

						ShopItem localItem = si;
						ItemRecord rec = World.Instance.EIF.GetItemRecordByID(si.ID);
						string secondary = string.Format("{2}: {0} {1}", buying ? si.Buy : si.Sell,
							rec.Type == ItemType.Armor ? "(" + (rec.Gender == 0 ? World.GetString(DATCONST2.FEMALE) : World.GetString(DATCONST2.MALE)) + ")" : "",
							World.GetString(DATCONST2.DIALOG_SHOP_PRICE));

						EODialogListItem nextItem = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large)
						{
							Text = rec.Name,
							SubText = secondary,
							IconGraphic = GFXLoader.TextureFromResource(GFXTypes.Items, 2*rec.Graphic - 1, true),
							OffsetY = 45
						};
						nextItem.OnLeftClick += (o, e) => _buySellItem(localItem);
						nextItem.OnRightClick += (o, e) => _buySellItem(localItem);

						itemList.Add(nextItem);
					}
					SetItemList(itemList);
					_setButtons(ScrollingListDialogButtons.BackCancel);
				}
					break;
				case ShopState.Crafting:
				{
					List<EODialogListItem> itemList = new List<EODialogListItem>(m_craftItems.Count);
					foreach (CraftItem ci in m_craftItems)
					{
						if (ci.Ingredients.Count <= 0) continue;

						CraftItem localItem = ci;
						ItemRecord rec = World.Instance.EIF.GetItemRecordByID(ci.ID);
						string secondary = string.Format("{2}: {0} {1}", ci.Ingredients.Count,
							rec.Type == ItemType.Armor ? "(" + (rec.Gender == 0 ? World.GetString(DATCONST2.FEMALE) : World.GetString(DATCONST2.MALE)) + ")" : "",
							World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_INGREDIENTS));

						EODialogListItem nextItem = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large)
						{
							Text = rec.Name,
							SubText = secondary,
							IconGraphic = GFXLoader.TextureFromResource(GFXTypes.Items, 2*rec.Graphic - 1, true),
							OffsetY = 45
						};
						nextItem.OnLeftClick += (o, e) => _craftItem(localItem);
						nextItem.OnRightClick += (o, e) => _craftItem(localItem);

						itemList.Add(nextItem);
					}
					SetItemList(itemList);
					_setButtons(ScrollingListDialogButtons.BackCancel);
				}
					break;
			}

			m_state = newState;
		}

		private void _buySellItem(ShopItem item)
		{
			if (m_state != ShopState.Buying && m_state != ShopState.Selling)
				return;
			bool isBuying = m_state == ShopState.Buying;

			InventoryItem ii = World.Instance.MainPlayer.ActiveCharacter.Inventory.Find(x => (isBuying ? x.id == 1 : x.id == item.ID));
			ItemRecord rec = World.Instance.EIF.GetItemRecordByID(item.ID);
			if (isBuying)
			{
				if (!EOGame.Instance.Hud.InventoryFits((short)item.ID))
				{
					EODialog.Show(World.GetString(DATCONST2.DIALOG_TRANSFER_NOT_ENOUGH_SPACE),
						World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
						XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					return;
				}

				if (rec.Weight + World.Instance.MainPlayer.ActiveCharacter.Weight >
				    World.Instance.MainPlayer.ActiveCharacter.MaxWeight)
				{
					EODialog.Show(World.GetString(DATCONST2.DIALOG_TRANSFER_NOT_ENOUGH_WEIGHT), 
						World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
						XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					return;
				}

				if (ii.amount < item.Buy)
				{
					EODialog.Show(DATCONST1.WARNING_YOU_HAVE_NOT_ENOUGH, " gold.", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					return;
				}
			}
			else if (ii.amount == 0)
				return; //can't sell if amount of item is 0

			//special case: no need for prompting if selling an item with count == 1 in inventory
			if(!isBuying && ii.amount == 1)
			{
				string _message = string.Format("{0} 1 {1} {2} {3} gold?", 
					World.GetString(DATCONST2.DIALOG_WORD_SELL),
					rec.Name,
					World.GetString(DATCONST2.DIALOG_WORD_FOR),
					item.Sell);
				EODialog.Show(_message, World.GetString(DATCONST2.DIALOG_SHOP_SELL_ITEMS), XNADialogButtons.OkCancel,
					EODialogStyle.SmallDialogSmallHeader, (oo, ee) =>
					{
						if (ee.Result == XNADialogResult.OK && !m_api.SellItem((short) item.ID, 1))
						{
							EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
						}
					});
			}
			else
			{
				EOItemTransferDialog dlg = new EOItemTransferDialog(rec.Name, EOItemTransferDialog.TransferType.ShopTransfer,
					isBuying ? item.MaxBuy : ii.amount, isBuying ? DATCONST2.DIALOG_TRANSFER_BUY : DATCONST2.DIALOG_TRANSFER_SELL);
				dlg.DialogClosing += (o, e) =>
				{
					if (e.Result == XNADialogResult.OK)
					{
						string _message = string.Format("{0} {1} {2} {3} {4} gold?",
							World.GetString(isBuying ? DATCONST2.DIALOG_WORD_BUY : DATCONST2.DIALOG_WORD_SELL),
							dlg.SelectedAmount, rec.Name,
							World.GetString(DATCONST2.DIALOG_WORD_FOR),
							(isBuying ? item.Buy : item.Sell) * dlg.SelectedAmount);

						EODialog.Show(_message,
							World.GetString(isBuying ? DATCONST2.DIALOG_SHOP_BUY_ITEMS : DATCONST2.DIALOG_SHOP_SELL_ITEMS),
							XNADialogButtons.OkCancel, EODialogStyle.SmallDialogSmallHeader, (oo, ee) =>
							{
								if (ee.Result == XNADialogResult.OK)
								{
									//only actually do the buy/sell if the user then clicks "OK" in the second prompt
									if (isBuying && !m_api.BuyItem((short) item.ID, dlg.SelectedAmount) ||
									    !isBuying && !m_api.SellItem((short) item.ID, dlg.SelectedAmount))
									{
										EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
									}
								}
							});
					}
				};
			}
		}

		private void _craftItem(CraftItem item)
		{
			if (m_state != ShopState.Crafting)
				return;

			ItemRecord craftItemRec = World.Instance.EIF.GetItemRecordByID(item.ID);
// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var ingredient in item.Ingredients)
			{
				if (World.Instance.MainPlayer.ActiveCharacter.Inventory.FindIndex(_item => _item.id == ingredient.Item1 && _item.amount >= ingredient.Item2) < 0)
				{
					string _message = World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_MISSING_INGREDIENTS) + "\n\n";
					foreach (var ingred in item.Ingredients)
					{
						ItemRecord localRec = World.Instance.EIF.GetItemRecordByID(ingred.Item1);
						_message += string.Format("+  {0}  {1}\n", ingred.Item2, localRec.Name);
					}
					string _caption = string.Format("{0} {1} {2}", World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_INGREDIENTS),
						World.GetString(DATCONST2.DIALOG_WORD_FOR),
						craftItemRec.Name);
					EODialog.Show(_message, _caption, XNADialogButtons.Cancel, EODialogStyle.LargeDialogSmallHeader);
					return;
				}
			}

			if (!EOGame.Instance.Hud.InventoryFits((short)item.ID))
			{
				EODialog.Show(World.GetString(DATCONST2.DIALOG_TRANSFER_NOT_ENOUGH_SPACE),
					World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
					XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			string _message2 = World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_PUT_INGREDIENTS_TOGETHER) + "\n\n";
			foreach (var ingred in item.Ingredients)
			{
				ItemRecord localRec = World.Instance.EIF.GetItemRecordByID(ingred.Item1);
				_message2 += string.Format("+  {0}  {1}\n", ingred.Item2, localRec.Name);
			}
			string _caption2 = string.Format("{0} {1} {2}", World.GetString(DATCONST2.DIALOG_SHOP_CRAFT_INGREDIENTS),
				World.GetString(DATCONST2.DIALOG_WORD_FOR),
				craftItemRec.Name);
			EODialog.Show(_message2, _caption2, XNADialogButtons.OkCancel, EODialogStyle.LargeDialogSmallHeader, (o, e) =>
			{
				if (e.Result == XNADialogResult.OK && !m_api.CraftItem((short)item.ID))
				{
					EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
				}
			});
		}
	}

	public class EOLockerDialog : EOScrollingListDialog
	{
		public static EOLockerDialog Instance { get; private set; }

		public static void Show(PacketAPI api, byte x, byte y)
		{
			if (Instance != null) return;

			Instance = new EOLockerDialog(api, x, y);

			if(!api.OpenLocker(x, y))
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
		}

		private static readonly string TITLE_FMT = World.Instance.MainPlayer.ActiveCharacter.Name + "'s " + World.GetString(DATCONST2.DIALOG_TITLE_PRIVATE_LOCKER) + " [{0}]";

		//map location of the currently open locker
		public byte X { get; private set; }
		public byte Y { get; private set; }

		private List<InventoryItem> items = new List<InventoryItem>(); 

		private EOLockerDialog(PacketAPI api, byte x, byte y)
			: base(api)
		{
			Title = string.Format(TITLE_FMT, 0);
			Buttons = ScrollingListDialogButtons.Cancel;
			ListItemType = EODialogListItem.ListItemStyle.Large;
			X = x;
			Y = y;
			
			DialogClosing += (o, e) => { Instance = null; X = 0; Y = 0; };
		}

		public void SetLockerData(List<InventoryItem> lockerItems)
		{
			ClearItemList();
			items = lockerItems;
			Title = string.Format(TITLE_FMT, lockerItems.Count);

			List<EODialogListItem> listItems = new List<EODialogListItem>();
			foreach (InventoryItem item in lockerItems)
			{
				ItemRecord rec = World.Instance.EIF.GetItemRecordByID(item.id);
				int amount = item.amount;
				EODialogListItem newItem = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large)
				{
					Text = rec.Name,
					SubText = string.Format("x{0}  {1}", item.amount,
						rec.Type == ItemType.Armor
							? "(" + (rec.Gender == 0 ? World.GetString(DATCONST2.FEMALE) : World.GetString(DATCONST2.MALE)) + ")"
							: ""),
					IconGraphic = GFXLoader.TextureFromResource(GFXTypes.Items, 2*rec.Graphic - 1, true),
					OffsetY = 45
				};
				newItem.OnRightClick += (o, e) => _removeItem(rec, amount);

				listItems.Add(newItem);
			}

			SetItemList(listItems);
		}

		public int GetNewItemAmount(short id, int amount)
		{
			int matchIndex = items.FindIndex(_ii => _ii.id == id);
			if (matchIndex < 0) return amount;
			return items[matchIndex].amount + amount;
		}

		private void _removeItem(ItemRecord item, int amount)
		{
			if (!EOGame.Instance.Hud.InventoryFits((short)item.ID))
			{
				EODialog.Show(World.GetString(DATCONST2.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT),
					World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
					XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			if (World.Instance.MainPlayer.ActiveCharacter.Weight + item.Weight*amount > World.Instance.MainPlayer.ActiveCharacter.MaxWeight)
			{
				EODialog.Show(World.GetString(DATCONST2.DIALOG_ITS_TOO_HEAVY_WEIGHT),
					World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
					XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			if(!m_api.LockerTakeItem(X, Y, (short)item.ID))
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
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

	public class EOSessionExpDialog : EODialogBase
	{
		private static EOSessionExpDialog inst;
		public static void Show()
		{
			if (inst != null) return;

			inst = new EOSessionExpDialog();
			inst.DialogClosing += (o, e) => inst = null;
		}

		private readonly Texture2D m_icons;
		private readonly Rectangle m_signal;
		private readonly Rectangle m_icon;

		private EOSessionExpDialog()
		{
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 61);
			_setSize(bgTexture.Width, bgTexture.Height);

			m_icons = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 68, true);
			m_signal = new Rectangle(0, 15, 15, 15);
			m_icon = new Rectangle(0, 0, 15, 15);

			XNAButton okButton = new XNAButton(smallButtonSheet, new Vector2(98, 214), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
			okButton.OnClick += (sender, args) => Close(okButton, XNADialogResult.OK);
			okButton.SetParent(this);

			XNALabel title = new XNALabel(new Rectangle(20, 17, 1, 1), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = false,
				Text = World.GetString(DATCONST2.DIALOG_TITLE_PERFORMANCE),
				ForeColor = System.Drawing.Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8)
			};
			title.SetParent(this);

			XNALabel[] leftSide = new XNALabel[8], rightSide = new XNALabel[8];
			for (int i = 48; i <= 160; i += 16)
			{
				leftSide[(i - 48)/16] = new XNALabel(new Rectangle(38, i, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					AutoSize = false,
					ForeColor = System.Drawing.Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8)
				};
				leftSide[(i - 48) / 16].SetParent(this);
				rightSide[(i - 48) / 16] = new XNALabel(new Rectangle(158, i, 1, 1), "Microsoft Sans Serif", 8.5f)
				{
					AutoSize = false,
					ForeColor = System.Drawing.Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8)
				};
				rightSide[(i - 48) / 16].SetParent(this);
			}

			leftSide[0].Text = World.GetString(DATCONST2.DIALOG_PERFORMANCE_TOTALEXP);
			leftSide[1].Text = World.GetString(DATCONST2.DIALOG_PERFORMANCE_NEXT_LEVEL);
			leftSide[2].Text = World.GetString(DATCONST2.DIALOG_PERFORMANCE_EXP_NEEDED);
			leftSide[3].Text = World.GetString(DATCONST2.DIALOG_PERFORMANCE_TODAY_EXP);
			leftSide[4].Text = World.GetString(DATCONST2.DIALOG_PERFORMANCE_TOTAL_AVG);
			leftSide[5].Text = World.GetString(DATCONST2.DIALOG_PERFORMANCE_TODAY_AVG);
			leftSide[6].Text = World.GetString(DATCONST2.DIALOG_PERFORMANCE_BEST_KILL);
			leftSide[7].Text = World.GetString(DATCONST2.DIALOG_PERFORMANCE_LAST_KILL);
			Character c = World.Instance.MainPlayer.ActiveCharacter;
			rightSide[0].Text = string.Format("{0}", c.Stats.Experience);
			rightSide[1].Text = string.Format("{0}", World.Instance.exp_table[c.Stats.Level + 1]);
			rightSide[2].Text = string.Format("{0}", World.Instance.exp_table[c.Stats.Level + 1] - c.Stats.Experience);
			rightSide[3].Text = string.Format("{0}", c.TodayExp);
			rightSide[4].Text = string.Format("{0}", (int) (c.Stats.Experience/(c.Stats.Usage/60.0)));
			int sessionTime = (int)(DateTime.Now - EOGame.Instance.Hud.SessionStartTime).TotalMinutes;
			rightSide[5].Text = string.Format("{0}", sessionTime > 0 ? (c.TodayExp/sessionTime) : 0);
			rightSide[6].Text = string.Format("{0}", c.TodayBestKill);
			rightSide[7].Text = string.Format("{0}", c.TodayLastKill);

			Array.ForEach(leftSide, lbl=>lbl.ResizeBasedOnText());
			Array.ForEach(rightSide, lbl => lbl.ResizeBasedOnText());

			Center(Game.GraphicsDevice);
			DrawLocation = new Vector2(DrawLocation.X, 15);
			endConstructor(false);
		}

		public override void Draw(GameTime gt)
		{
			//base draw logic handles drawing the background + child controls
			base.Draw(gt);

			SpriteBatch.Begin();
			//icons next to labels
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 48), m_icon, Color.White);
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 64), m_icon, Color.White);
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 80), m_icon, Color.White);
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 96), m_icon, Color.White);
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 112), m_icon, Color.White);
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 128), m_icon, Color.White);
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 144), m_icon, Color.White);
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 22, DrawAreaWithOffset.Y + 160), m_icon, Color.White);

			//signal next to exp labels
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 142, DrawAreaWithOffset.Y + 48), m_signal, Color.White);
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 142, DrawAreaWithOffset.Y + 64), m_signal, Color.White);
			SpriteBatch.Draw(m_icons, new Vector2(DrawAreaWithOffset.X + 142, DrawAreaWithOffset.Y + 80), m_signal, Color.White);
			SpriteBatch.End();
		}
	}

	public class EOBankAccountDialog : EODialogBase
	{
		public static EOBankAccountDialog Instance { get; private set; }

		public static void Show(PacketAPI api, short npcID)
		{
			if (Instance != null)
				return;

			Instance = new EOBankAccountDialog(api);

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

		private EOBankAccountDialog(PacketAPI api)
			: base(api)
		{
			//this uses EODialogListItems but does not inherit from EOScrollingListDialog since it is a different size
			//offsety 50
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 53);
			_setSize(bgTexture.Width, bgTexture.Height);

			m_accountBalance = new XNALabel(new Rectangle(129, 20, 121, 16), "Microsoft Sans Serif", 8.5f)
			{
				ForeColor = System.Drawing.Color.FromArgb(255, 0xc8, 0xc8, 0xc8),
				Text = "",
				TextAlign = ContentAlignment.MiddleRight,
				AutoSize = false
			};
			m_accountBalance.SetParent(this);

			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(92, 191), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
			cancel.SetParent(this);
			cancel.OnClick += (o, e) => Close(cancel, XNADialogResult.Cancel);

			EODialogListItem deposit = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, 0)
			{
				Text = World.GetString(DATCONST2.DIALOG_BANK_DEPOSIT),
				SubText = string.Format("{0} gold {1}", World.GetString(DATCONST2.DIALOG_BANK_TRANSFER),
					World.GetString(DATCONST2.DIALOG_BANK_TO_ACCOUNT)),
				IconGraphic = _getDlgIcon(ListIcon.BankDeposit),
				OffsetY = 55,
				ShowItemBackGround = false
			};
			deposit.OnLeftClick += (o, e) => _deposit();
			deposit.OnRightClick += (o, e) => _deposit();
			EODialogListItem withdraw = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, 1)
			{
				Text = World.GetString(DATCONST2.DIALOG_BANK_WITHDRAW),
				SubText = string.Format("{0} gold {1}", World.GetString(DATCONST2.DIALOG_BANK_TAKE),
					World.GetString(DATCONST2.DIALOG_BANK_FROM_ACCOUNT)),
				IconGraphic = _getDlgIcon(ListIcon.BankWithdraw),
				OffsetY = 55,
				ShowItemBackGround = false
			};
			withdraw.OnLeftClick += (o, e) => _withdraw();
			withdraw.OnRightClick += (o, e) => _withdraw();
			EODialogListItem upgrade = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, 2)
			{
				Text = World.GetString(DATCONST2.DIALOG_BANK_LOCKER_UPGRADE),
				SubText = World.GetString(DATCONST2.DIALOG_BANK_MORE_SPACE),
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
			InventoryItem item = World.Instance.MainPlayer.ActiveCharacter.Inventory.Find(i => i.id == 1);
			if (item.amount == 0)
			{
				EODialog.Show(DATCONST1.BANK_ACCOUNT_UNABLE_TO_DEPOSIT, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}
			if (item.amount == 1)
			{
				if (!m_api.BankDeposit(1))
				{
					Close(null, XNADialogResult.NO_BUTTON_PRESSED);
					EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
				}
				return;
			}

			EOItemTransferDialog dlg = new EOItemTransferDialog(World.Instance.EIF.GetItemRecordByID(1).Name,
				EOItemTransferDialog.TransferType.BankTransfer, item.amount, DATCONST2.DIALOG_TRANSFER_DEPOSIT);
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
			if(balance == 0)
			{
				EODialog.Show(DATCONST1.BANK_ACCOUNT_UNABLE_TO_WITHDRAW, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
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

			EOItemTransferDialog dlg = new EOItemTransferDialog(World.Instance.EIF.GetItemRecordByID(1).Name,
				EOItemTransferDialog.TransferType.BankTransfer, balance, DATCONST2.DIALOG_TRANSFER_WITHDRAW);
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
				EODialog.Show(DATCONST1.LOCKER_UPGRADE_IMPOSSIBLE, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			int requiredGold = (LockerUpgrades + 1)*1000;
			InventoryItem item = World.Instance.MainPlayer.ActiveCharacter.Inventory.Find(i => i.id == 1);
			if (item.amount < requiredGold)
			{
				EODialog.Show(DATCONST1.WARNING_YOU_HAVE_NOT_ENOUGH, "gold", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			EODialog.Show(DATCONST1.LOCKER_UPGRADE_UNIT, string.Format("{0} gold?", requiredGold), XNADialogButtons.OkCancel,
				EODialogStyle.SmallDialogSmallHeader,
				(o, e) =>
				{
					if (e.Result == XNADialogResult.Cancel)
						return;

					Packet pkt = new Packet(PacketFamily.Locker, PacketAction.Buy);
					World.Instance.Client.SendPacket(pkt);
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

	public class EOTradeDialog : EODialogBase
	{
		//dialog has:
		// - 2 lists of items on each side
		// - 2 scroll bars (1 each side)
		// - 2 name labels (1 each side)
		// - 2 agree/trading labels (1 each side)
		// - Ok/cancel buttons

		private short m_leftPlayerID, m_rightPlayerID;
		private string m_leftNameStr, m_rightNameStr;
		private readonly XNALabel m_leftPlayerName, m_rightPlayerName;
		private readonly XNALabel m_leftPlayerStatus, m_rightPlayerStatus;
		private readonly EOScrollBar m_leftScroll, m_rightScroll;
		private bool m_leftAgrees, m_rightAgrees;
		private readonly List<EODialogListItem> m_leftItems, m_rightItems;

		private readonly Character m_main; //local reference

		private int m_recentPartnerRemoves;

		public static EOTradeDialog Instance { get; private set; }

		public bool MainPlayerAgrees
		{
			get
			{
				return (m_main.ID == m_leftPlayerID && m_leftAgrees) ||
				       (m_main.ID == m_rightPlayerID && m_rightAgrees);
			}
		}

		public EOTradeDialog(PacketAPI apiHandle)
			: base(apiHandle)
		{
			bgTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 50);
			_setSize(bgTexture.Width, bgTexture.Height);

			Instance = this;
			DialogClosing += (sender, args) => Instance = null;
			m_main = World.Instance.MainPlayer.ActiveCharacter;

			m_leftItems = new List<EODialogListItem>();
			m_rightItems = new List<EODialogListItem>();

			m_leftPlayerID = 0;
			m_rightPlayerID = 0;

			m_leftPlayerName = new XNALabel(new Rectangle(20, 14, 166, 20), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				ForeColor = System.Drawing.Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8)
			};
			m_leftPlayerName.SetParent(this);
			m_rightPlayerName = new XNALabel(new Rectangle(285, 14, 166, 20), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				ForeColor = System.Drawing.Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8)
			};
			m_rightPlayerName.SetParent(this);
			m_leftPlayerStatus = new XNALabel(new Rectangle(195, 14, 79, 20), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				Text = World.GetString(DATCONST2.DIALOG_TRADE_WORD_TRADING),
				ForeColor = System.Drawing.Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8)
			};
			m_leftPlayerStatus.SetParent(this);
			m_rightPlayerStatus = new XNALabel(new Rectangle(462, 14, 79, 20), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				Text = World.GetString(DATCONST2.DIALOG_TRADE_WORD_TRADING),
				ForeColor = System.Drawing.Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8)
			};
			m_rightPlayerStatus.SetParent(this);

			m_leftScroll = new EOScrollBar(this, new Vector2(252, 44), new Vector2(16, 199), EOScrollBar.ScrollColors.LightOnMed) { LinesToRender = 5 };
			m_rightScroll = new EOScrollBar(this, new Vector2(518, 44), new Vector2(16, 199), EOScrollBar.ScrollColors.LightOnMed) { LinesToRender = 5 };

			//BUTTONSSSS
			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(356, 252), _getSmallButtonOut(SmallButton.Ok),
				_getSmallButtonOver(SmallButton.Ok));
			ok.OnClick += _buttonOkClicked;
			ok.SetParent(this);
			dlgButtons.Add(ok);
			XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(449, 252), _getSmallButtonOut(SmallButton.Cancel),
				_getSmallButtonOver(SmallButton.Cancel));
			cancel.OnClick += _buttonCancelClicked;
			cancel.SetParent(this);
			dlgButtons.Add(cancel);

			Timer localTimer = new Timer(state =>
			{
				if (m_recentPartnerRemoves > 0)
					m_recentPartnerRemoves--;
			}, null, 0, 5000);

			DialogClosing += (o, e) =>
			{
				if (e.Result == XNADialogResult.Cancel)
				{
					if(!m_api.TradeClose())
						((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
					((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_TRADE_ABORTED);
				}

				localTimer.Dispose();
			};

			Center(Game.GraphicsDevice);
			DrawLocation = new Vector2(DrawLocation.X, 30);
			endConstructor(false);
		}

		public void InitPlayerInfo(short player1, string player1Name, short player2, string player2Name)
		{
			m_leftPlayerID = player1;
			m_rightPlayerID = player2;
			m_leftNameStr = m_leftPlayerName.Text = char.ToUpper(player1Name[0]) + player1Name.Substring(1);
			m_rightNameStr = m_rightPlayerName.Text = char.ToUpper(player2Name[0]) + player2Name.Substring(1);
		}

		public void SetPlayerItems(short playerID, List<InventoryItem> items)
		{
			int xOffset;
			List<EODialogListItem> collectionRef;
			EOScrollBar scrollRef;

			if (playerID == m_leftPlayerID)
			{
				collectionRef = m_leftItems;
				scrollRef = m_leftScroll;
				xOffset = -3;
				m_leftPlayerName.Text = string.Format("{0} {1}", m_leftNameStr, items.Count > 0 ? "[" + items.Count + "]" : "");

				if (m_leftAgrees)
				{
					m_leftAgrees = false;
					m_leftPlayerStatus.Text = World.GetString(DATCONST2.DIALOG_TRADE_WORD_TRADING);
				}

				//left player is NOT main, and right player (ie main) agrees, and the item count is different for left player
				//cancel the offer for the main player since the other player changed the offer
				if (m_main.ID != playerID && m_rightAgrees && collectionRef.Count != items.Count)
				{
					m_rightAgrees = false;
					m_rightPlayerStatus.Text = World.GetString(DATCONST2.DIALOG_TRADE_WORD_TRADING);
					EODialog.Show(DATCONST1.TRADE_ABORTED_OFFER_CHANGED, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_TRADE_OTHER_PLAYER_CHANGED_OFFER);
				}
			}
			else if (playerID == m_rightPlayerID)
			{
				collectionRef = m_rightItems;
				scrollRef = m_rightScroll;
				xOffset = 263;
				m_rightPlayerName.Text = string.Format("{0} {1}", m_rightNameStr, items.Count > 0 ? "[" + items.Count + "]" : "");

				if (m_rightAgrees)
				{
					m_rightAgrees = false;
					m_rightPlayerStatus.Text = World.GetString(DATCONST2.DIALOG_TRADE_WORD_TRADING);
				}

				//right player is NOT main, and left player (ie main) agrees, and the item count is different for right player
				//cancel the offer for the main player since the other player changed the offer
				if (m_main.ID != playerID && m_leftAgrees && collectionRef.Count != items.Count)
				{
					m_leftAgrees = false;
					m_leftPlayerStatus.Text = World.GetString(DATCONST2.DIALOG_TRADE_WORD_TRADING);
					EODialog.Show(DATCONST1.TRADE_ABORTED_OFFER_CHANGED, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_TRADE_OTHER_PLAYER_CHANGED_OFFER);
				}
			}
			else
				throw new ArgumentException("Invalid Player ID for trade session!", "playerID");

			if (m_main.ID != playerID && collectionRef.Count > items.Count)
				m_recentPartnerRemoves++;
			if (m_recentPartnerRemoves == 3)
			{
				EODialog.Show(DATCONST1.TRADE_OTHER_PLAYER_TRICK_YOU, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				m_recentPartnerRemoves = -1000; //this will prevent the message from showing more than once (I'm too lazy to find something more elegant)
			}

			foreach (var oldItem in collectionRef) oldItem.Close();
			collectionRef.Clear();

			int index = 0;
			foreach (InventoryItem item in items)
			{
				int localID = item.id;

				ItemRecord rec = World.Instance.EIF.GetItemRecordByID(item.id);
				string secondary = string.Format("x {0}  {1}", item.amount, rec.Type == ItemType.Armor
					? "(" + (rec.Gender == 0 ? World.GetString(DATCONST2.FEMALE) : World.GetString(DATCONST2.MALE)) + ")"
					: "");

				int gfxNum = item.id == 1
					? 269 + 2*(item.amount >= 100000 ? 4 : (item.amount >= 10000? 3 : (item.amount >= 100 ? 2 : (item.amount >= 2 ? 1 : 0))))
					: 2*rec.Graphic - 1;

				var nextItem = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, index++)
				{
					Text = rec.Name,
					SubText = secondary,
					IconGraphic = GFXLoader.TextureFromResource(GFXTypes.Items, gfxNum, true),
					ID = item.id,
					Amount = item.amount,
					OffsetX = xOffset,
					OffsetY = 46
				};
				if (playerID == m_main.ID)
					nextItem.OnRightClick += (sender, args) => _removeItem(localID);
				collectionRef.Add(nextItem);
			}

			scrollRef.UpdateDimensions(collectionRef.Count);
		}

		public void SetPlayerAgree(bool isMain, bool agrees)
		{
			short playerID = isMain ? (short)m_main.ID : (m_leftPlayerID == m_main.ID ? m_rightPlayerID : m_leftPlayerID);
			if (playerID == m_leftPlayerID)
			{
				if (agrees && !m_leftAgrees)
					((EOGame) Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION,
						isMain ? DATCONST2.STATUS_LABEL_TRADE_YOU_ACCEPT : DATCONST2.STATUS_LABEL_TRADE_OTHER_ACCEPT);
				else if (!agrees && m_leftAgrees)
					((EOGame) Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION,
						isMain ? DATCONST2.STATUS_LABEL_TRADE_YOU_CANCEL : DATCONST2.STATUS_LABEL_TRADE_OTHER_CANCEL);

				m_leftAgrees = agrees;
				m_leftPlayerStatus.Text =
					World.GetString(agrees ? DATCONST2.DIALOG_TRADE_WORD_AGREE : DATCONST2.DIALOG_TRADE_WORD_TRADING);
			}
			else if (playerID == m_rightPlayerID)
			{
				if (agrees && !m_rightAgrees)
					((EOGame) Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION,
						isMain ? DATCONST2.STATUS_LABEL_TRADE_YOU_ACCEPT : DATCONST2.STATUS_LABEL_TRADE_OTHER_ACCEPT);
				else if (!agrees && m_rightAgrees)
					((EOGame) Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION,
						isMain ? DATCONST2.STATUS_LABEL_TRADE_YOU_CANCEL : DATCONST2.STATUS_LABEL_TRADE_OTHER_CANCEL);

				m_rightAgrees = agrees;
				m_rightPlayerStatus.Text =
					World.GetString(agrees ? DATCONST2.DIALOG_TRADE_WORD_AGREE : DATCONST2.DIALOG_TRADE_WORD_TRADING);
			}
			else
				throw new ArgumentException("Invalid Player ID for trade session!");
		}

		public void CompleteTrade(short p1, List<InventoryItem> p1items, short p2, List<InventoryItem> p2items)
		{
			List<InventoryItem> mainCollection, otherCollection;
			if (p1 == m_main.ID)
			{
				mainCollection = p1items;
				otherCollection = p2items;
			}
			else if (p2 == m_main.ID)
			{
				mainCollection = p2items;
				otherCollection = p1items;
			}
			else
				throw new ArgumentException("Invalid player ID for trade session!");

			int weightDelta = 0;
			foreach (var item in mainCollection)
			{
				m_main.UpdateInventoryItem(item.id, -item.amount, true);
				weightDelta -= World.Instance.EIF.GetItemRecordByID(item.id).Weight*item.amount;
			}
			foreach (var item in otherCollection)
			{
				m_main.UpdateInventoryItem(item.id, item.amount, true);
				weightDelta += World.Instance.EIF.GetItemRecordByID(item.id).Weight*item.amount;
			}
			m_main.Weight += (byte) weightDelta;
			((EOGame) Game).Hud.RefreshStats();

			Close(null, XNADialogResult.NO_BUTTON_PRESSED);
			EODialog.Show(DATCONST1.TRADE_SUCCESS, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
		}

		private void _buttonOkClicked(object sender, EventArgs e)
		{
			if (m_leftPlayerID == m_main.ID)
			{
				if (m_leftAgrees) return; //main already agrees
			}
			else if (m_rightPlayerID == m_main.ID)
			{
				if (m_rightAgrees) return; //main already agrees
			}
			else
				throw new InvalidOperationException("Invalid Player ID for trade session!");

			if (m_leftItems.Count == 0 || m_rightItems.Count == 0)
			{
				EODialog.Show(World.GetString(DATCONST2.DIALOG_TRADE_BOTH_PLAYERS_OFFER_ONE_ITEM),
					World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING), XNADialogButtons.Ok,
					EODialogStyle.SmallDialogSmallHeader);
				((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.DIALOG_TRADE_BOTH_PLAYERS_OFFER_ONE_ITEM);
				return;
			}

			List<EODialogListItem> mainCollection = m_main.ID == m_leftPlayerID ? m_leftItems : m_rightItems;
			List<EODialogListItem> otherCollection = m_main.ID == m_leftPlayerID ? m_rightItems : m_leftItems;

			//make sure that the items will fit!
			if (!((EOGame) Game).Hud.ItemsFit(
					otherCollection.Select(_item => new InventoryItem {id = _item.ID, amount = _item.Amount}).ToList(),
					mainCollection.Select(_item => new InventoryItem {id = _item.ID, amount = _item.Amount}).ToList()))
			{
				EODialog.Show(World.GetString(DATCONST2.DIALOG_TRANSFER_NOT_ENOUGH_SPACE),
					World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING), XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			//make sure the change in weight + existing weight is not greater than the max weight!
			int weightDelta = otherCollection.Sum(itemRef => (World.Instance.EIF.GetItemRecordByID(itemRef.ID).Weight*itemRef.Amount));
			weightDelta = mainCollection.Aggregate(weightDelta, (current, itemRef) => current - (World.Instance.EIF.GetItemRecordByID(itemRef.ID).Weight*itemRef.Amount));
			if (weightDelta + m_main.Weight > m_main.MaxWeight)
			{
				EODialog.Show(World.GetString(DATCONST2.DIALOG_TRANSFER_NOT_ENOUGH_WEIGHT),
					World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING), XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			EODialog.Show(DATCONST1.TRADE_DO_YOU_AGREE, XNADialogButtons.OkCancel, EODialogStyle.SmallDialogSmallHeader,
				(o, dlgArgs) =>
				{
					if (dlgArgs.Result == XNADialogResult.OK && !m_api.TradeAgree(true))
					{
						Close(null, XNADialogResult.NO_BUTTON_PRESSED);
						((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
					}
				});
		}

		private void _buttonCancelClicked(object sender, EventArgs e)
		{
			if (m_main.ID == m_leftPlayerID)
			{
				if (!m_leftAgrees) //just quit
					Close(dlgButtons[1], XNADialogResult.Cancel);
				else if (!m_api.TradeAgree(false)) //cancel agreement
					((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
			}
			else if (m_main.ID == m_rightPlayerID)
			{
				if (!m_rightAgrees) //just quit
					Close(dlgButtons[1], XNADialogResult.Cancel);
				else if (!m_api.TradeAgree(false))
						((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
			}
			else
				throw new InvalidOperationException("Invalid player ID for trade session!");
		}

		//item right-click event handler
		private void _removeItem(int id)
		{
			if (!m_api.TradeRemoveItem((short) id))
			{
				Close(null, XNADialogResult.NO_BUTTON_PRESSED);
				((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
			}
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

			//do the hiding logic for both sides
			List<EOScrollBar> scrollBars = new List<EOScrollBar> {m_leftScroll, m_rightScroll};
			List<List<EODialogListItem>> lists = new List<List<EODialogListItem>> {m_leftItems, m_rightItems};
			for (int ndx = 0; ndx < 2; ++ndx)
			{
				var list = lists[ndx];
				var scroll = scrollBars[ndx];

				//which items should we render?
				if (list.Count > scroll.LinesToRender)
				{
					for (int i = 0; i < list.Count; ++i)
					{
						EODialogListItem curr = list[i];
						if (i < scroll.ScrollOffset)
						{
							curr.Visible = false;
							continue;
						}

						if (i < scroll.LinesToRender + scroll.ScrollOffset)
						{
							curr.Visible = true;
							curr.Index = i - scroll.ScrollOffset;
						}
						else
						{
							curr.Visible = false;
						}
					}
				}
				else if (list.Any(_item => !_item.Visible))
					list.ForEach(_item => _item.Visible = true); //all items visible if less than # lines to render
			}

			base.Update(gt);
		}

		public void Close(XNADialogResult result)
		{
			Close(null, result);
			Close();
		}
	}

	public class EOSkillmasterDialog : EOScrollingListDialog
	{
		public static EOSkillmasterDialog Instance { get; private set; }

		public static void Show(PacketAPI api, short npcIndex)
		{
			if (Instance != null)
				return;

			Instance = new EOSkillmasterDialog(api);

			if (!api.RequestSkillmaster(npcIndex))
			{
				Instance.Close();
				Instance = null;
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			}
		}

		private enum SkillState
		{
			None,
			Initial,
			Learn,
			Forget,
			ForgetAll
		}

		private SkillState m_state;
		private List<Skill> m_skills;
		private bool m_showingRequirements;

		private static Texture2D LearnIcon, ForgetIcon;

		private EOSkillmasterDialog(PacketAPI api)
			: base(api)
		{
			Buttons = ScrollingListDialogButtons.Cancel;
			ListItemType = EODialogListItem.ListItemStyle.Large;
			DialogClosing += (o, e) =>
			{
				if (e.Result == XNADialogResult.Cancel)
				{
					Instance = null;
				}
				else if (e.Result == XNADialogResult.Back)
				{
					e.CancelClose = true;
					if (m_state == SkillState.Learn && m_showingRequirements)
					{
						m_state = SkillState.Initial; //force it to re-generate the list items
						_setState(SkillState.Learn);
						m_showingRequirements = false;
					}
					else
						_setState(SkillState.Initial);
				}
			};
			m_state = SkillState.None;

			if (LearnIcon == null || ForgetIcon == null)
			{
				//getDlgIcon
				LearnIcon = _getDlgIcon(ListIcon.Learn);
				ForgetIcon = _getDlgIcon(ListIcon.Forget);
			}
		}

		public void SetSkillmasterData(SkillmasterData data)
		{
			if (Instance == null || this != Instance) return;

			Title = data.Title;
			m_skills = new List<Skill>(data.Skills);

			_setState(SkillState.Initial);
		}

		public void RemoveSkillByIDFromLearnList(short id)
		{
			if (Instance == null || this != Instance) return;

			EODialogListItem itemToRemove = children.Find(_x => _x is EODialogListItem && (_x as EODialogListItem).ID == id) as EODialogListItem;
			if(itemToRemove != null)
				RemoveFromList(itemToRemove);
		}

		private void _setState(SkillState newState)
		{
			SkillState old = m_state;

			if (old == newState) return;

			int numToLearn = m_skills.Count(_skill => !World.Instance.MainPlayer.ActiveCharacter.Spells.Exists(_spell => _spell.id == _skill.ID));
			int numToForget = World.Instance.MainPlayer.ActiveCharacter.Spells.Count;

			if (newState == SkillState.Learn && numToLearn == 0)
			{
				EODialog.Show(DATCONST1.SKILL_NOTHING_MORE_TO_LEARN, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			ClearItemList();
			switch (newState)
			{
				case SkillState.Initial:
				{
					string learnNum = string.Format("{0}{1}", numToLearn, World.GetString(DATCONST2.SKILLMASTER_ITEMS_TO_LEARN));
					string forgetNum = string.Format("{0}{1}", numToForget, World.GetString(DATCONST2.SKILLMASTER_ITEMS_LEARNED));

					EODialogListItem learn = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, 0)
					{
						Text = World.GetString(DATCONST2.SKILLMASTER_WORD_LEARN),
						SubText = learnNum,
						IconGraphic = LearnIcon,
						ShowItemBackGround = false,
						OffsetY = 45
					};
					learn.OnLeftClick += (o, e) => _setState(SkillState.Learn);
					learn.OnRightClick += (o, e) => _setState(SkillState.Learn);
					AddItemToList(learn, false);

					EODialogListItem forget = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, 1)
					{
						Text = World.GetString(DATCONST2.SKILLMASTER_WORD_FORGET),
						SubText = forgetNum,
						IconGraphic = ForgetIcon,
						ShowItemBackGround = false,
						OffsetY = 45
					};
					forget.OnLeftClick += (o, e) => _setState(SkillState.Forget);
					forget.OnRightClick += (o, e) => _setState(SkillState.Forget);
					AddItemToList(forget, false);

					EODialogListItem forgetAll = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, 2)
					{
						Text = World.GetString(DATCONST2.SKILLMASTER_FORGET_ALL),
						SubText = World.GetString(DATCONST2.SKILLMASTER_RESET_YOUR_CHARACTER),
						IconGraphic = ForgetIcon,
						ShowItemBackGround = false,
						OffsetY = 45
					};
					forgetAll.OnLeftClick += (o, e) => _setState(SkillState.ForgetAll);
					forgetAll.OnRightClick += (o, e) => _setState(SkillState.ForgetAll);
					AddItemToList(forgetAll, false);

					_setButtons(ScrollingListDialogButtons.Cancel);
				}
					break;
				case SkillState.Learn:
				{
					int index = 0;
					for (int i = 0; i < m_skills.Count; ++i)
					{
						if (World.Instance.MainPlayer.ActiveCharacter.Spells.FindIndex(_sp => m_skills[i].ID == _sp.id) >= 0)
							continue;
						int localI = i;

						SpellRecord spellData = (SpellRecord) World.Instance.ESF.Data[m_skills[localI].ID];

						EODialogListItem nextListItem = new EODialogListItem(this, EODialogListItem.ListItemStyle.Large, index++)
						{
							Visible = false,
							Text = spellData.Name,
							SubText = World.GetString(DATCONST2.SKILLMASTER_WORD_REQUIREMENTS),
							IconGraphic = World.GetSpellIcon(spellData.Icon, false),
							ShowItemBackGround = false,
							OffsetY = 45,
							ID = m_skills[localI].ID
						};
						nextListItem.OnLeftClick += (o, e) => _learn(m_skills[localI]);
						nextListItem.OnRightClick += (o, e) => _learn(m_skills[localI]);
						nextListItem.OnMouseEnter += (o, e) => _showRequirementsLabel(m_skills[localI]);
						nextListItem.SetSubtextLink(() => _showRequirements(m_skills[localI]));
						AddItemToList(nextListItem, false);
					}

					_setButtons(ScrollingListDialogButtons.BackCancel);
				}
					break;
				case SkillState.Forget:
				{
					EOInputDialog input = new EOInputDialog(World.GetString(DATCONST1.SKILL_PROMPT_TO_FORGET, false), 32);
					input.SetAsKeyboardSubscriber();
					input.DialogClosing += (sender, args) =>
					{
						if (args.Result == XNADialogResult.Cancel) return;
						bool found =
							World.Instance.MainPlayer.ActiveCharacter.Spells.Any(
								_spell => ((SpellRecord) World.Instance.ESF.Data[_spell.id]).Name.ToLower() == input.ResponseText.ToLower());

						if (!found)
						{
							args.CancelClose = true;
							EODialog.Show(DATCONST1.SKILL_FORGET_ERROR_NOT_LEARNED, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
							input.SetAsKeyboardSubscriber();
						}

						if (!m_api.ForgetSpell(
								World.Instance.MainPlayer.ActiveCharacter.Spells.Find(
									_spell => ((SpellRecord) World.Instance.ESF.Data[_spell.id]).Name.ToLower() == input.ResponseText.ToLower()).id))
						{
							Close();
							((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
						}
					};

					//should show initial info in the actual dialog since this uses a pop-up input box
					//	to select a skill to remove
					newState = SkillState.Initial;
					goto case SkillState.Initial;
				}
				case SkillState.ForgetAll:
				{
					_showForgetAllMessage(_forgetAllAction);
					_setButtons(ScrollingListDialogButtons.BackCancel);
				}
					break;
			}

			m_state = newState;
		}

		private void _learn(Skill skill)
		{
			Character c = World.Instance.MainPlayer.ActiveCharacter;

			bool skillReqsMet = true;
			foreach(short x in skill.SkillReq)
				if (x != 0 && c.Spells.FindIndex(_sp => _sp.id == x) < 0)
					skillReqsMet = false;

			//check the requirements
			if (c.Stats.Str < skill.StrReq || c.Stats.Int < skill.IntReq || c.Stats.Wis < skill.WisReq ||
			    c.Stats.Agi < skill.AgiReq || c.Stats.Con < skill.ConReq || c.Stats.Cha < skill.ChaReq ||
			    c.Stats.Level < skill.LevelReq || c.Inventory.Find(_ii => _ii.id == 1).amount < skill.GoldReq || !skillReqsMet)
			{
				EODialog.Show(DATCONST1.SKILL_LEARN_REQS_NOT_MET, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			if (skill.ClassReq > 0 && c.Class != skill.ClassReq)
			{
				EODialog.Show(DATCONST1.SKILL_LEARN_WRONG_CLASS, " " + ((ClassRecord)World.Instance.ECF.Data[skill.ClassReq]).Name + "!", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
				return;
			}

			EODialog.Show(DATCONST1.SKILL_LEARN_CONFIRMATION, " " + ((SpellRecord)World.Instance.ESF.Data[skill.ID]).Name + "?", XNADialogButtons.OkCancel, EODialogStyle.SmallDialogSmallHeader,
				(o, e) =>
				{
					if (e.Result != XNADialogResult.OK)
						return;

					if (!m_api.LearnSpell(skill.ID))
					{
						Close();
						((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
					}
				});
		}

		private void _forgetAllAction()
		{
			EODialog.Show(DATCONST1.SKILL_RESET_CHARACTER_CONFIRMATION, XNADialogButtons.OkCancel, EODialogStyle.SmallDialogSmallHeader,
				(sender, args) =>
				{
					if (args.Result == XNADialogResult.Cancel) return;

					if (!m_api.ResetCharacterStatSkill())
					{
						Close();
						((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
					}
				});
		}

		private void _showRequirements(Skill skill)
		{
			m_showingRequirements = true;
			ClearItemList();

			List<string> drawStrings = new List<string>(15)
			{
				((SpellRecord) World.Instance.ESF.Data[skill.ID]).Name + (skill.ClassReq > 0 ? " [" + ((ClassRecord) World.Instance.ECF.Data[skill.ClassReq]).Name + "]" : ""),
				" "
			};
			if (skill.SkillReq.Any(x => x != 0))
			{
				drawStrings.AddRange(from req in skill.SkillReq where req != 0 select World.GetString(DATCONST2.SKILLMASTER_WORD_SKILL) + ": " + ((SpellRecord) World.Instance.ESF.Data[req]).Name);
				drawStrings.Add(" ");
			}

			if(skill.StrReq > 0)
				drawStrings.Add(skill.StrReq + " " + World.GetString(DATCONST2.SKILLMASTER_WORD_STRENGTH));
			if (skill.IntReq > 0)
				drawStrings.Add(skill.IntReq + " " + World.GetString(DATCONST2.SKILLMASTER_WORD_INTELLIGENCE));
			if (skill.WisReq > 0)
				drawStrings.Add(skill.WisReq + " " + World.GetString(DATCONST2.SKILLMASTER_WORD_WISDOM));
			if (skill.AgiReq > 0)
				drawStrings.Add(skill.AgiReq + " " + World.GetString(DATCONST2.SKILLMASTER_WORD_AGILITY));
			if (skill.ConReq > 0)
				drawStrings.Add(skill.ConReq + " " + World.GetString(DATCONST2.SKILLMASTER_WORD_CONSTITUTION));
			if (skill.ChaReq > 0)
				drawStrings.Add(skill.ChaReq + " " + World.GetString(DATCONST2.SKILLMASTER_WORD_CHARISMA));

			drawStrings.Add(" ");
			drawStrings.Add(skill.LevelReq + " " + World.GetString(DATCONST2.SKILLMASTER_WORD_LEVEL));
			drawStrings.Add(skill.GoldReq + " " + World.Instance.EIF.GetItemRecordByID(1).Name);

			foreach (string s in drawStrings)
			{
				EODialogListItem nextLine = new EODialogListItem(this, EODialogListItem.ListItemStyle.Small) { Text = s };
				AddItemToList(nextLine, false);
			}
		}

		private void _showRequirementsLabel(Skill skill)
		{
			string full = string.Format("{0} {1} LVL, ", ((SpellRecord)World.Instance.ESF.Data[skill.ID]).Name, skill.LevelReq);
			if (skill.StrReq > 0)
				full += string.Format("{0} STR, ", skill.StrReq);
			if (skill.IntReq > 0)
				full += string.Format("{0} INT, ", skill.IntReq);
			if (skill.WisReq > 0)
				full += string.Format("{0} WIS, ", skill.WisReq);
			if (skill.AgiReq > 0)
				full += string.Format("{0} AGI, ", skill.AgiReq);
			if (skill.ConReq > 0)
				full += string.Format("{0} CON, ", skill.ConReq);
			if (skill.ChaReq > 0)
				full += string.Format("{0} CHA, ", skill.ChaReq);
			if (skill.GoldReq > 0)
				full += string.Format("{0} Gold", skill.GoldReq);
			if (skill.ClassReq > 0)
				full += string.Format(", {0}", ((ClassRecord) World.Instance.ECF.Data[skill.ClassReq]).Name);

			((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, full);
		}

		private void _showForgetAllMessage(Action forgetAllAction)
		{
			List<string> drawStrings = new List<string>();

			using (Font f = new Font("Microsoft Sans Serif", 8.5f))
			{
				string[] messages =
				{
					World.GetString(DATCONST2.SKILLMASTER_FORGET_ALL),
					World.GetString(DATCONST2.SKILLMASTER_FORGET_ALL_MSG_1),
					World.GetString(DATCONST2.SKILLMASTER_FORGET_ALL_MSG_2),
					World.GetString(DATCONST2.SKILLMASTER_FORGET_ALL_MSG_3),
					World.GetString(DATCONST2.SKILLMASTER_CLICK_HERE_TO_FORGET_ALL)
				};

				TextSplitter ts = new TextSplitter("", f) { LineLength = 200 };
				foreach (string s in messages)
				{
					ts.Text = s;
					if (!ts.NeedsProcessing)
					{
						//no text clipping needed
						drawStrings.Add(s);
						drawStrings.Add(" ");
						continue;
					}

					drawStrings.AddRange(ts.SplitIntoLines());
					drawStrings.Add(" ");
				}
			}

			//now need to take the processed draw strings and make an EODialogListItem for each one
			foreach (string s in drawStrings)
			{
				string next = s;
				bool link = false;
				if (next.Length > 0 && next[0] == '*')
				{
					next = next.Remove(0, 1);
					link = true;
				}
				EODialogListItem nextItem = new EODialogListItem(this, EODialogListItem.ListItemStyle.Small) { Text = next };
				if (link) nextItem.SetPrimaryTextLink(forgetAllAction);
				AddItemToList(nextItem, false);
			}
		}
	}
}
