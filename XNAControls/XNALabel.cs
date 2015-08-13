using XNAFramework = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using ContentAlignment = System.Drawing.ContentAlignment;
using Size = System.Drawing.Size;

namespace XNAControls
{
	public class XNALabel : XNAControl
	{
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
				GenerateImage();
			}
		}
		public Font Font
		{
			get
			{
				return _font;
			}
			set
			{
				_font = value;
				GenerateImage();
			}
		}
		public Color ForeColor
		{
			get
			{
				return _color;
			}
			set
			{
				_color = value;
				GenerateImage();
			}
		}

		public Color BackColor
		{
			get { return _backColor; }
			set
			{
				_backColor = value;

				if(!AutoSize)
					_backGround = new Texture2D(Game.GraphicsDevice, DrawArea.Width, DrawArea.Height);
				else
					_backGround = new Texture2D(Game.GraphicsDevice, _texture.Width, _texture.Height);

				XNAFramework.Color[] bgData = new XNAFramework.Color[_backGround.Width * _backGround.Height];
				for (int i = 0; i < bgData.Length; ++i)
				{
					bgData[i] = XNAFramework.Color.FromNonPremultiplied(_backColor.R, _backColor.G, _backColor.B, _backColor.A);
				}
				_backGround.SetData(bgData);
			}
		}

		public bool AutoSize
		{
			get
			{
				return _autoSize;
			}
			set
			{
				_autoSize = value;
				GenerateImage();
			}
		}
		public ContentAlignment TextAlign
		{
			get
			{
				return _align;
			}
			set
			{
				_align = value;
				GenerateImage();
			}
		}
		
		/// <summary>
		/// Get or set the text width in pixels
		/// </summary> 
		public int? TextWidth
		{
			get { return _textWidth; }
			set
			{
				_textWidth = value;
				GenerateImage();
			}
		}
		/// <summary>
		/// Get the actual text width of underlying texture
		/// </summary> 
		public int ActualWidth
		{
			get { return _texture.Width; }
		}

		/// <summary>
		/// The underlying texture of the label
		/// </summary>
		public Texture2D Texture
		{
			get { return _texture; }
		}

		public int? RowSpacing
		{
			get
			{
				return rowSpacing;
			}
			set
			{
				rowSpacing = value;
				GenerateImage();
			}
		}

		public FontStyle Style
		{
			get { return _style; }
			set
			{
				_style = value;
				Font oldfont = _font;
				_font = new Font(oldfont.FontFamily, oldfont.Size, _style);
				oldfont.Dispose();
				GenerateImage();
			}
		}

		public XNALabel(XNAFramework.Rectangle area)
			: base(new XNAFramework.Vector2(area.X, area.Y), area)
		{
			_autoSize = true;
			_text = "";
			_font = new Font("Arial", 12);
			_color = Color.Black;
			_align = ContentAlignment.TopLeft;
			_style = FontStyle.Regular;
		}

		public XNALabel(XNAFramework.Rectangle area, string fontFamily, float fontSize = 12.0f)
			: base(new XNAFramework.Vector2(area.X, area.Y), area)
		{
			_autoSize = true;
			_text = "";
			_font = new Font(fontFamily, fontSize);
			_color = Color.Black;
			_align = ContentAlignment.TopLeft;
			_style = FontStyle.Regular;
		}

		private int? rowSpacing;
		private string _text;
		private bool _autoSize;
		private Font _font;
		private Color _color, _backColor;
		private ContentAlignment _align;
		private int? _textWidth;
		private Texture2D _texture, _backGround;
		private FontStyle _style;

		public override void Initialize()
		{
			GenerateImage();
			base.Initialize();
		}

		public override void Update(XNAFramework.GameTime gameTime)
		{
			if (!Visible || !ShouldUpdate())
				return;

			base.Update(gameTime);
		}

		public override void Draw(XNAFramework.GameTime gameTime)
		{
			if (!Visible)
				return;

			SpriteBatch.Begin();

			int x = 0, y = 0;
			Size size = new Size(_texture.Width, _texture.Height);
			if (!AutoSize)
			{
				// Figure out alignment
				string align = Enum.GetName(typeof (ContentAlignment), _align) ?? "";
				if (align.Contains("Left"))
					x = 0;
				else if (align.Contains("Center"))
					x = DrawArea.Width/2 - size.Width/2;
				else
					x = DrawArea.Width - size.Width;
				if (align.Contains("Top"))
					y = 0;
				else if (align.Contains("Middle"))
					y = DrawArea.Height/2 - size.Height/2;
				else
					y = DrawArea.Height - size.Height;
			}

			if(_backGround != null)
				SpriteBatch.Draw(_backGround, DrawAreaWithOffset, XNAFramework.Color.White);

			SpriteBatch.Draw(_texture, new XNAFramework.Vector2(DrawAreaWithOffset.X + x, DrawAreaWithOffset.Y + y), XNAFramework.Color.White);
			SpriteBatch.End();

			base.Draw(gameTime);
		}

		/// <summary>
		/// Resizes the label using the current font to fit the current text
		/// <para>Not valid for when AutoSize=true</para>
		/// </summary>
		/// <param name="x_padding">Total extra space to add to the new width, in pixels</param>
		/// <param name="y_padding">Total extra space to add to the new height, in pixels</param>
		public void ResizeBasedOnText(uint x_padding = 0, uint y_padding = 0)
		{
			if (_font == null || AutoSize) return;

			using (Bitmap temp = new Bitmap(1, 1))
			{
				using (Graphics g = Graphics.FromImage(temp))
				{
					SizeF sz = g.MeasureString(_text, _font);
					drawArea = new XNAFramework.Rectangle(DrawArea.X, DrawArea.Y, (int)Math.Round(sz.Width) + (int)x_padding, (int)Math.Round(sz.Height) + (int)y_padding);
				}
			}
		}

		void GenerateImage()
		{
			_texture = Game.DrawText(Text, Font, ForeColor, TextWidth, rowSpacing ?? 0);
		}

		public new void Dispose()
		{
			if(_texture != null)
				_texture.Dispose();
			if(_backGround != null)
				_backGround.Dispose();
			_font.Dispose();
			base.Dispose();
		}
	}
}
