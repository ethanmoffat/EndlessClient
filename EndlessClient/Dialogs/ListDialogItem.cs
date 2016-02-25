// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class ListDialogItem : XNAControl
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

		public ListDialogItem(EODialogBase parent, ListItemStyle style, int listIndex = -1)
		{
			DrawLocation = new Vector2(17, DrawLocation.Y); //the base X coordinate is 17 - this can be adjusted with OffsetX property

			Style = style;
			if (listIndex >= 0)
				Index = listIndex;

			_setSize(232, Style == ListItemStyle.Large ? 36 : 13);

			int colorFactor = Style == ListItemStyle.Large ? 0xc8 : 0xb4;

			m_primaryText = new XNALabel(new Rectangle(Style == ListItemStyle.Large ? 56 : 2, Style == ListItemStyle.Large ? 5 : 0, 1, 1), Constants.FontSize08pt5)
			{
				AutoSize = false,
				BackColor = Color.Transparent,
				ForeColor = Color.FromNonPremultiplied(colorFactor, colorFactor, colorFactor, 0xff),
				TextAlign = LabelAlignment.TopLeft,
				Text = " "
			};
			m_primaryText.ResizeBasedOnText();

			if (Style == ListItemStyle.Large)
			{
				m_secondaryText = new XNALabel(new Rectangle(56, 20, 1, 1), Constants.FontSize08pt5)
				{
					AutoSize = true,
					BackColor = m_primaryText.BackColor,
					ForeColor = m_primaryText.ForeColor,
					Text = " "
				};
				m_secondaryText.ResizeBasedOnText();

				m_gfxPadThing = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.MapTiles, 0, true);
				ShowItemBackGround = true;
			}
			m_backgroundColor = new Texture2D(Game.GraphicsDevice, 1, 1);
			m_backgroundColor.SetData(new[] { Color.FromNonPremultiplied(0xff, 0xff, 0xff, 64) });

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
			m_primaryText = new XNAHyperLink(oldText.DrawArea, Constants.FontSize08pt5)
			{
				AutoSize = false,
				BackColor = oldText.BackColor,
				ForeColor = oldText.ForeColor,
				HighlightColor = oldText.ForeColor,
				Text = oldText.Text,
				Underline = true
			};
			m_primaryText.ResizeBasedOnText();
			((XNAHyperLink)m_primaryText).OnClick += (o, e) => onClickAction();
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
			m_secondaryText = new XNAHyperLink(oldText.DrawArea, Constants.FontSize08pt5)
			{
				AutoSize = false,
				BackColor = oldText.BackColor,
				ForeColor = oldText.ForeColor,
				HighlightColor = oldText.ForeColor,
				Text = oldText.Text,
				Underline = true
			};
			m_secondaryText.ResizeBasedOnText();
			((XNAHyperLink)m_secondaryText).OnClick += (o, e) => onClickAction();
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
							((XNAHyperLink)m_secondaryText).Click();
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
					Vector2 offset = new Vector2(xOff + OffsetX + 14/*not sure of the significance of this offset*/, yOff + OffsetY + 36 * Index);
					if (ShowItemBackGround)
						SpriteBatch.Draw(m_gfxPadThing, new Vector2(offset.X + ((64 - m_gfxPadThing.Width) / 2f), offset.Y + (36 - m_gfxPadThing.Height) / 2f), Color.White);
					if (m_gfxItem != null)
						SpriteBatch.Draw(m_gfxItem,
							new Vector2((float)Math.Round(offset.X + ((64 - m_gfxItem.Width) / 2f)),
								(float)Math.Round(offset.Y + (36 - m_gfxItem.Height) / 2f)),
							Color.White);
				}
				SpriteBatch.End();
				base.Draw(gameTime);
			}
		}

		public void SetActive()
		{
			m_primaryText.ForeColor = Color.FromNonPremultiplied(0xf0, 0xf0, 0xf0, 0xff);
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
}
