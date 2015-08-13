using XNAFramework = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;

namespace XNAControls
{
	public class XNAHyperLink : XNALabel
	{
		Color _highlightColor;
		Color _backupColor;
		public Color HighlightColor
		{
			get { return _highlightColor; }
			set { _highlightColor = value; }
		}

		public event EventHandler OnClick;

		[Obsolete("Passing a font as a parameter is deprecated. Specify font family and font size instead, and set additional parameters using the .Font property.")]
		public XNAHyperLink(XNAFramework.Rectangle area, Font font, FontStyle style)
			: base(area)
		{
			Font = new Font(font, style);
		}

		[Obsolete("Passing a font as a parameter is deprecated. Specify font family and font size instead, and set additional parameters using the .Font property.")]
		public XNAHyperLink(XNAFramework.Rectangle area, Font font)
			: base(area)
		{
			Font = new Font(font, FontStyle.Underline);
		}

		public XNAHyperLink(XNAFramework.Rectangle area, string fontFamily, float fontSize, FontStyle style)
			: base(area)
		{
			Font = new Font(fontFamily, fontSize, style);
		}

		public XNAHyperLink(XNAFramework.Rectangle area, string fontFamily, float fontSize = 12.0f)
			: base(area)
		{
			Font = new Font(fontFamily, fontSize);
		}

		public override void Initialize()
		{
			ForeColor = Color.Blue;
			HighlightColor = Color.Blue;

			OnMouseEnter += (o, e) =>
			{
				_backupColor = ForeColor;
				ForeColor = HighlightColor;
			};
			OnMouseLeave += (o, e) =>
			{
				ForeColor = _backupColor;
			};
			base.Initialize();
		}

		public override void Update(XNAFramework.GameTime gameTime)
		{
			if (!Visible || !ShouldUpdate())
				return;

			if (MouseOver && MouseOverPreviously && OnClick != null && PreviousMouseState.LeftButton == ButtonState.Pressed &&
			    Mouse.GetState().LeftButton == ButtonState.Released)
				OnClick(this, null);

			base.Update(gameTime);
		}

		public void Click()
		{
			if (OnClick != null)
				OnClick(this, null);
		}
	}
}
