using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using SD = System.Drawing;

namespace XNAControls
{
	public class XNAMenu : XNAControl
	{
		public List<XNAMenuItem> Items { get; private set; }

		int itemHeight = 30;
		public int ItemHeight
		{
			get { return itemHeight; }
			set { itemHeight = value; }
		}

		SD.Font _font;
		public SD.Font Font
		{
			get
			{
				return _font;
			}
			set
			{
				_font = value;

				foreach (XNAMenuItem mi in Items)
					mi.Font = _font;
			}
		}
		SD.Color _foreColor;
		public SD.Color ForeColor
		{
			get
			{
				return _foreColor;
			}
			set
			{
				_foreColor = value;

				foreach (XNAMenuItem mi in Items)
					mi.ForeColor = _foreColor;
			}
		}
		SD.Color _highlightColor;
		public SD.Color HighlightColor
		{
			get
			{
				return _highlightColor;
			}
			set
			{
				_highlightColor = value;

				foreach (XNAMenuItem mi in Items)
					mi.ForeColor = _highlightColor;
			}
		}

		int selectedIndex = 0;
		public int SelectedIndex
		{
			get
			{
				return selectedIndex;
			}
			set
			{
				if (value < 0 || value > Items.Count - 1)
					throw new IndexOutOfRangeException();

				if (value == selectedIndex)
					return;

				Items[selectedIndex].SelectionChanged(false);
				Items[selectedIndex = value].SelectionChanged(true);
			}
		}

		SD.Color? backgroundColor = null;
		public SD.Color? BackgroundColor
		{
			get
			{
				return backgroundColor;
			}
			set
			{
				backgroundColor = value;
				RenderBackground();
			}
		}
		SD.Color? borderColor = null;
		public SD.Color? BorderColor
		{
			get
			{
				return borderColor;
			}
			set
			{
				borderColor = value;
				RenderBackground();
			}
		}
		Texture2D background = null;

		public Texture2D SelectionEmphasisTexture { get; set; }

		public EventHandler SelectionChanged = null;

		public XNAMenu(Rectangle area)
			: base(new Vector2(area.X, area.Y), area)
		{
			Items = new List<XNAMenuItem>();
			Font = new SD.Font("Arial", 12);
			ForeColor = SD.Color.Black;

			SizeChanged += (o, e) => RenderBackground();
		}

		void RenderBackground()
		{
			if (backgroundColor != null || borderColor != null)
				background = Game.DrawRectangle(
					new SD.Size(drawArea.Width, drawArea.Height),
					BackgroundColor ?? SD.Color.Transparent,
					BorderColor ?? SD.Color.Transparent);
		}

		public void AddMenuItem(string text, EventHandler chooseAction)
		{
			XNAMenuItem menuItem = new XNAMenuItem(
				new Rectangle(DrawAreaWithOffset.X, DrawAreaWithOffset.Y + ItemHeight * Items.Count, DrawAreaWithOffset.Width, ItemHeight), Font.FontFamily.Name, Font.Size)
				{
					TextAlign = SD.ContentAlignment.TopCenter,
					Text = text,
					Font = this.Font,
					RegularColor = this.ForeColor,
					HighlightColor = this.HighlightColor
				};
			menuItem.OnClick += chooseAction;
			menuItem.OnMouseOver += MenuItemHovered;
			Items.Add(menuItem);

			menuItem.SelectionChanged(Items.Count == 1);
		}

		void MenuItemHovered(object sender, EventArgs args)
		{
			if (!(sender is XNAMenuItem))
				throw new Exception("Invalid sender!");

			XNAMenuItem senderItem = sender as XNAMenuItem;

			if (!senderItem.Selected && SelectionChanged != null)
				SelectionChanged(this, null);

			SelectedIndex = Items.IndexOf(senderItem);
		}

		public void ClearMenuItems()
		{
			Items.Clear();
		}

		public override void Update(GameTime gameTime)
		{
			if (!ShouldUpdate())
				return;

			foreach (XNAMenuItem menuItem in Items)
				menuItem.Visible = Visible;

			if (!Visible)
				return;

			KeyboardState state = Keyboard.GetState();

			if (state.IsKeyDown(Keys.Enter) && !PreviousKeyState.IsKeyDown(Keys.Enter))
			{
				Items[SelectedIndex].Click();
			}

			if (state.IsKeyDown(Keys.Up) && !PreviousKeyState.IsKeyDown(Keys.Up))
			{
				if (SelectedIndex > 0)
				{
					SelectedIndex--;

					if (SelectionChanged != null)
						SelectionChanged(this, null);
				}
			}

			if (state.IsKeyDown(Keys.Down) && !PreviousKeyState.IsKeyDown(Keys.Down))
			{
				if (SelectedIndex < Items.Count - 1)
				{
					SelectedIndex++;

					if (SelectionChanged != null)
						SelectionChanged(this, null);
				}
			}

			foreach (XNAMenuItem menuItem in Items)
				menuItem.Update(gameTime);

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			SpriteBatch.Begin();

			if (background != null)
				SpriteBatch.Draw(background, drawLocation, Color.White);

			SpriteBatch.End();

			int i = DrawAreaWithOffset.Y + drawArea.Height / 2 - (Items.Count * ItemHeight) / 2;
			foreach (XNAMenuItem item in Items)
			{
				item.DrawLocation = new Vector2(item.DrawLocation.X, i);
				item.Draw(gameTime);

				if (Items.IndexOf(item) == SelectedIndex && SelectionEmphasisTexture != null)
				{
					SpriteBatch.Begin();
					SpriteBatch.Draw(SelectionEmphasisTexture,
						new Vector2((float)(DrawAreaWithOffset.X + drawArea.Width / 2 - item.ActualWidth / 2 - SelectionEmphasisTexture.Width + 25), i + ItemHeight / 2 - SelectionEmphasisTexture.Height / 2),
						null,
						Color.White,
						0f,
						Vector2.Zero,
						0.6f,
						SpriteEffects.FlipHorizontally,
						0);
					SpriteBatch.Draw(SelectionEmphasisTexture,
						new Vector2((float)(DrawAreaWithOffset.X + drawArea.Width / 2 + item.ActualWidth / 2), i + ItemHeight / 2 - SelectionEmphasisTexture.Height / 2),
						null,
						Color.White,
						0f,
						Vector2.Zero,
						0.6f,
						SpriteEffects.None,
						0);
					SpriteBatch.End();
				}

				i += ItemHeight;
			}

			base.Draw(gameTime);
		}

		protected override void Dispose(bool disposing)
		{
			_font.Dispose();
			base.Dispose(disposing);
		}
	}
}
