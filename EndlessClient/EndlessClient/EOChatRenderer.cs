using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAControls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient
{
	public enum ChatTabs
	{
		Private1,
		Private2,
		Local,
		Global,
		Group,
		System,
		None
	}

	/// <summary>
	/// These go in numerical order for how they are in the sprite sheet in the GFX file
	/// </summary>
	public enum ChatType
	{
		None = -1, //blank icon - trying to load will return empty texture
		SpeechBubble = 0,
		Note,
		Error,
		NoteLeftArrow,
		Audio,
		Star,
		Exclamation,
		LookingDude,
		Heart,
		SingleGuyLight,
		DoubleGuyLight,
		DoubleGuyDark,
		SingleAdminDark,
		DoubleAdminDark,
		SingleAdminLight,
		DoubleAdminLight,
		DownArrow,
		UpArrow,
		DotDotDotDot,
		GSymbol,
		Skeleton,
		WhatTheFuck,
		Information,
		NoteGenderThing
	}

	public enum ChatColor
	{
		/// <summary>
		/// 00 00 00
		/// </summary>
		Default,
		/// <summary>
		/// e6 d2 c8
		/// </summary>
		Server,
		/// <summary>
		/// 7d 0a 0a
		/// </summary>
		Error,
		/// <summary>
		/// 5a 3c 00
		/// </summary>
		PM
	}

	/// <summary>
	/// contains a scroll bar and handles the text display and storage for a particular tab
	/// </summary>
	public class ChatTab : XNAControl
	{
		private bool _selected;
		public bool Selected
		{
			get { return _selected; }
			set
			{
				_selected = value;
				scrollBar.Visible = _selected;
				tabLabel.ForeColor = _selected ? System.Drawing.Color.White : System.Drawing.Color.Black;
			}
		}
		public ChatTabs WhichTab { get; protected set; }

		private Rectangle? closeRect;
		private EOScrollBar scrollBar;
		private XNALabel message;

		private struct ChatIndex : IComparable
		{
			/// <summary>
			/// Used for sorting the entries in the chat window
			/// </summary>
			public int Index;
			/// <summary>
			/// Determines the type of special icon that should appear next to the chat message
			/// </summary>
			public ChatType Type;
			/// <summary>
			/// The entity that talked
			/// </summary>
			public string Who;

			public ChatColor col;

			public ChatIndex(int index = 0, ChatType type = ChatType.None, string who = "", ChatColor color = ChatColor.Default)
			{
				Index = index;
				Type = type;
				Who = who;
				col = color;
			}

			public int CompareTo(object other)
			{
				ChatIndex obj = (ChatIndex)other;
				return Index - obj.Index;
			}

			public Color GetColor()
			{
				switch(col)
				{
					case ChatColor.Default: return Color.Black;
					case ChatColor.Error: return Color.FromNonPremultiplied(0x7d, 0x0a, 0x0a, 0xff);
					case ChatColor.PM: return Color.FromNonPremultiplied(0x5a, 0x3c, 0x00, 0xff);
					case ChatColor.Server: return Color.FromNonPremultiplied(0xe6, 0xd2, 0xc8, 0xff);
					default: throw new IndexOutOfRangeException("ChatColor enumeration unhandled for index " + Index.ToString());
				}
			}
		}
		private SortedList<ChatIndex, string> chatStrings;

		private XNALabel tabLabel;
		private SpriteFont font;
		
		/// <summary>
		/// This Constructor should be used for all values in ChatTabs
		/// </summary>
		public ChatTab(Game g, ChatTabs tab, EOChatRenderer parentRenderer, bool selected = false)
			: base(g, null, null, parentRenderer)
		{
			WhichTab = tab;

			chatStrings = new SortedList<ChatIndex, string>();

			tabLabel = new XNALabel(g, new Rectangle(14, 2, 1, 1), "Microsoft Sans Serif", 8.0f);
			tabLabel.SetParent(this);

			switch(WhichTab)
			{
				case ChatTabs.Local: tabLabel.Text = "scr";  break;
				case ChatTabs.Global: tabLabel.Text = "glb"; break;
				case ChatTabs.Group: tabLabel.Text = "grp"; break;
				case ChatTabs.System: tabLabel.Text = "sys"; break;
				case ChatTabs.Private1:
				case ChatTabs.Private2:
					tabLabel.Text = "[priv " + ((int)WhichTab + 1).ToString() + "]";
					break;
			}
			_selected = selected;

			//enable close button based on which tab was specified
			switch(WhichTab)
			{
				case ChatTabs.Private1:
				case ChatTabs.Private2:
					{
						closeRect = new Rectangle(3, 3, 11, 11);
						drawArea = new Rectangle(drawArea.X, drawArea.Y, 132, 16);
						Visible = false;
					} break;
				default:
					{
						closeRect = null;
						drawArea = new Rectangle(drawArea.X, drawArea.Y, 43, 16);
						Visible = true;
					} break;
			}

			message = new XNALabel(g, new Rectangle(20, 3, 1, 1)); //label is auto-sized
			message.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.0f);
			message.TextWidth = 440;
			message.Visible = false; //the label doesn't handle its own drawing/updating: the chat tab does that so text is clipped
			message.RowSpacing = 0;

			//568 331
			scrollBar = new EOScrollBar(g, parentRenderer, new Vector2(467, 2), new Vector2(16, 97), EOScrollBar.ScrollColors.LightOnMed);
			scrollBar.Visible = selected;

			font = Game.Content.Load<SpriteFont>("dbg");
		}

		/// <summary>
		/// This constructor should be used for the news rendering
		/// </summary>
		public ChatTab(Game g, XNAControl parentControl)
			: base(g, null, null, parentControl)
		{
			WhichTab = ChatTabs.None;
			_selected = true;
			chatStrings = new SortedList<ChatIndex, string>();
			tabLabel = null;

			message = new XNALabel(g, new Rectangle(20, 23, 1, 1)); //label is auto-sized
			message.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.0f);
			message.TextWidth = 440;
			message.Visible = false; //the label doesn't handle its own drawing/updating: the chat tab does that so text is clipped
			message.RowSpacing = 0;

			//568 331
			scrollBar = new EOScrollBar(g, parent, new Vector2(467, 20), new Vector2(16, 97), EOScrollBar.ScrollColors.LightOnMed);
			scrollBar.Visible = true;

			font = Game.Content.Load<SpriteFont>("dbg");
		}

		public void AddText(string who, string text, ChatType icon = ChatType.None, ChatColor col = ChatColor.Default)
		{
			chatStrings.Add(new ChatIndex(chatStrings.Count, icon, who, col), text);
			if (message.Text.Length > 0)
				message.Text += "\n";
			if (string.IsNullOrEmpty(who))
				message.Text += text;
			else
				message.Text += (who + "  " + text);
		}

		public Texture2D GetChatIcon(ChatType type)
		{
			Texture2D ret = new Texture2D(Game.GraphicsDevice, 13, 13);
			if (type == ChatType.None)
				return ret;

			Color[] data = new Color[169]; //each icon is 13x13
			GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 32, true).GetData(0, new Rectangle(0, (int)type * 13, 13, 13), data, 0, 169);
			ret.SetData(data);

			return ret;
		}
		
		public override void Update(GameTime gameTime)
		{
			if (!this.Visible)
				return;

			MouseState mouseState = Mouse.GetState();
			//this is our own button press handler
			if (MouseOver && mouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
			{
				if (!this.Selected)
				{
					(parent as EOChatRenderer).SetSelectedTab(WhichTab);
				}

				if ((WhichTab == ChatTabs.Private1 || WhichTab == ChatTabs.Private2) && closeRect != null)
				{
					Rectangle withOffset = new Rectangle(DrawAreaWithOffset.X + closeRect.Value.X, DrawAreaWithOffset.Y + closeRect.Value.Y, closeRect.Value.Width, closeRect.Value.Height);
					if (withOffset.ContainsPoint(Mouse.GetState().X, Mouse.GetState().Y))
					{
						Visible = false;
						(parent as EOChatRenderer).SetSelectedTab(ChatTabs.Local);
					}
				}
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (Selected) //only draw this tab if it is selected
			{
				int row;
				if (scrollBar == null)
					return;
				scrollBar.UpdateDimensions(message.Texture.Height, row = (int)message.Font.GetHeight());
				Texture2D msg = message.Texture;
				Rectangle loc = new Rectangle(parent.DrawAreaWithOffset.X + message.DrawAreaWithOffset.X, parent.DrawAreaWithOffset.Y + message.DrawAreaWithOffset.Y, 445, 97);
				Rectangle src = new Rectangle(0, scrollBar.ScrollOffset, loc.Width, loc.Height);

				SpriteBatch.Begin();
				//Okay, this is really dumb, but I don't actually display the message label itself.
				//I use the SpriteFont and spritebatch to draw the text strings and use the label for the scroll bar.
				//Hopefully I can come up with a better way to do this in the future.
				//SpriteBatch.Draw(msg, loc, src, Color.White);

				//draw icons for the text strings based on the icon specified in the chatStrings Key of the pair
				int step = message.Texture.Height / row; //this is the step used for each 'notch' of ScrollOffset
				int start = scrollBar.ScrollOffset / step; //this is the start index in the chatStrings for drawing.
				for (int i = start; i < start + 7; ++i) //draw 7 icons
				{
					if (i >= chatStrings.Count)
						break;
					
					Vector2 pos = new Vector2(parent.DrawAreaWithOffset.X + 3, loc.Y + (i - start) * 13);
					int extra = (int)Math.Round(scrollBar.ScrollOffset / 13.0f); //figure out what this offset should be
					SpriteBatch.Draw(GetChatIcon(chatStrings.Keys[i].Type), pos, Color.White);

					string strToDraw = "";
					if (string.IsNullOrEmpty(chatStrings.Keys[i].Who))
						strToDraw = chatStrings.Values[i];
					else
						strToDraw = chatStrings.Keys[i].Who + "  " + chatStrings.Values[i];
					SpriteBatch.DrawString(font, strToDraw, pos + new Vector2(message.DrawLocation.X, 0), chatStrings.Keys[i].GetColor());
				}

				SpriteBatch.End();
			}
			base.Draw(gameTime); //draw child controls though
		}
	}

	/// <summary>
	/// Stores all the different tabs, draws their tab graphics, and handles switching between tabs.
	/// </summary>
	public class EOChatRenderer : XNAControl
	{
		private int currentSelTab;
		private ChatTab[] tabs;

		public EOChatRenderer(Game g) : base(g)
		{
			tabs = new ChatTab[Enum.GetNames(typeof(ChatTabs)).Length - 1]; // -1 skips the 'none' tab which is used for news
			for(int i = 0; i < tabs.Length; ++i)
			{
				tabs[i] = new ChatTab(g, (ChatTabs)i, this, (ChatTabs)i == ChatTabs.Local);
				if(i > (int)ChatTabs.Private2) //if it isn't private1 or private2
				{
					tabs[i].DrawLocation = new Vector2(289 + 44 * (i - 2), 102);
				}
				else
				{
					tabs[i].DrawLocation = new Vector2((ChatTabs)i == ChatTabs.Private1 ? 23 : 156, 102);
				}
			}

			currentSelTab = (int)ChatTabs.Local;
			tabs[currentSelTab].Selected = true;
		}

		public void SetSelectedTab(ChatTabs tabToSelect)
		{
			tabs[currentSelTab].Selected = false;
			tabs[currentSelTab = (int)tabToSelect].Selected = true;
		}

		public void AddTextToTab(ChatTabs tab, string who, string text, ChatType icon = ChatType.None, ChatColor col = ChatColor.Default)
		{
			tabs[(int)tab].AddText(who, text, icon, col);
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch.Begin();
			//264 16 43x16
			//307 16 43x16
			Color[] data;
			Texture2D drawTexture = null;

			//the parent renderer draws all the tabs
			foreach (ChatTab t in tabs)
			{
				switch (t.WhichTab)
				{
					case ChatTabs.Local: //391 433 need to see if this should be relative to top-left of existing chat panel or absolute from top-left of game screen
					case ChatTabs.Global:
					case ChatTabs.Group:
					case ChatTabs.System:
						{
							data = new Color[43 * 16];
							drawTexture = new Texture2D(Game.GraphicsDevice, 43, 16);
							if (t.Selected)
								GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 35, false).GetData<Color>(0, new Rectangle(307, 16, 43, 16), data, 0, data.Length);
							else
								GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 35, false).GetData<Color>(0, new Rectangle(264, 16, 43, 16), data, 0, data.Length);
							drawTexture.SetData<Color>(data);
						}
						break;
					case ChatTabs.Private1:
					case ChatTabs.Private2:
						{
							if (t.Visible)
							{
								data = new Color[132 * 16];
								drawTexture = new Texture2D(Game.GraphicsDevice, 132, 16);
								if (t.Selected)
									GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 35, false).GetData<Color>(0, new Rectangle(132, 16, 132, 16), data, 0, data.Length);
								else
									GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 35, false).GetData<Color>(0, new Rectangle(0, 16, 132, 16), data, 0, data.Length);
								drawTexture.SetData<Color>(data);
							}
							else
								drawTexture = null;
						}
						break;
					default:
						throw new InvalidOperationException("This is not a valid enumeration for WhichTab");
				}
				if(drawTexture != null)
					SpriteBatch.Draw(drawTexture, new Vector2(t.DrawAreaWithOffset.X, t.DrawAreaWithOffset.Y), Color.White);
			}
			SpriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
