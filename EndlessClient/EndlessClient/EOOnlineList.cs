using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace EndlessClient
{
	public struct OnlineEntry
	{
		private readonly string m_name, m_title, m_guild, m_class;
		private readonly PaperdollIconType m_iconType;

		public string Name
		{
			get { return m_name; }
		}

		public string Title
		{
			get { return m_title; }
		}

		public string Guild
		{
			get { return m_guild; }
		}

		public string Class
		{
			get { return m_class; }
		}

		public PaperdollIconType Icon
		{
			get { return m_iconType; }
		}

		public OnlineEntry(string name, string title, string guild, string clss, PaperdollIconType icon)
		{
			m_name = name;
			m_title = title;
			m_guild = guild;
			m_class = clss;
			m_iconType = icon;
		}
	}

	public class EOOnlineList : XNAControl
	{
		//there may be more filters: these will be supported by default
		private enum Filter
		{
			All,
			Friends,
			Admins,
			//Party, //todo: Is this party or guild? either way, add it in once there is support for parties/guilds
			Max
		}

		private readonly List<OnlineEntry> m_onlineList;
		private readonly EOScrollBar m_scrollBar;
		private readonly XNALabel m_totalNumPlayers;

		private const int DRAW_ICON_X = 4,
			DRAW_NAME_X = 18,
			DRAW_TITLE_X = 133,
			DRAW_GUILD_X = 245,
			DRAW_CLASS_X = 359,
			DRAW_OFFSET_Y = 23;

		private readonly Rectangle m_filterClick;
		private Filter m_filter;
		private readonly Texture2D[] m_filterTextures = new Texture2D[4]; //there are 4 filter textures
		private List<string> m_friendList = new List<string>();

		public EOOnlineList(XNAPanel parent)
			: base(null, null, parent)
		{
			m_onlineList = new List<OnlineEntry>();
			//this enables scrolling with mouse wheel and mouseover for parent
			_setSize(parent.BackgroundImage.Width, parent.BackgroundImage.Height);

			m_totalNumPlayers = new XNALabel(new Rectangle(455, 2, 27, 14), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = false,
				ForeColor = System.Drawing.Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8),
				TextAlign = System.Drawing.ContentAlignment.MiddleRight
			};
			m_totalNumPlayers.SetParent(this);

			m_scrollBar = new EOScrollBar(this, new Vector2(467, 20), new Vector2(16, 97), EOScrollBar.ScrollColors.LightOnMed)
			{
				LinesToRender = 7,
				Visible = true
			};
			m_scrollBar.SetParent(this);
			m_scrollBar.IgnoreDialog(typeof(EOPaperdollDialog));
			m_scrollBar.IgnoreDialog(typeof(EOChestDialog));

			m_filterClick = new Rectangle(2 + DrawAreaWithOffset.X, 2 + DrawAreaWithOffset.Y, 14, 14);

			Texture2D weirdOffsets = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 27, true);
			for (int i = 0; i < m_filterTextures.Length; ++i)
			{
				Rectangle offsetsSource = new Rectangle(i % 2 == 0 ? 0 : 12, i >= 2 ? 246 : 233, 12, 13);

				m_filterTextures[i] = new Texture2D(EOGame.Instance.GraphicsDevice, 12, 13);
				Color[] dat = new Color[12*13];
				weirdOffsets.GetData(0, offsetsSource, dat, 0, dat.Length);
				m_filterTextures[i].SetData(dat);
			}
		}

		public void SetOnlinePlayerList(List<OnlineEntry> onlineList)
		{
			m_onlineList.Clear();
			m_onlineList.AddRange(onlineList);
// ReSharper disable once StringCompareToIsCultureSpecific
			m_onlineList.Sort((x, y) => x.Name.CompareTo(y.Name));
			m_totalNumPlayers.Text = onlineList.Count.ToString(CultureInfo.CurrentCulture);
			m_scrollBar.UpdateDimensions(onlineList.Count);
			m_friendList = InteractList.LoadAllFriend();
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible || !Game.IsActive)
				return;

			MouseState curState = Mouse.GetState();

			if (m_filterClick.ContainsPoint(curState.X, curState.Y) && curState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
			{
				m_filter++;
				if (m_filter == Filter.Max)
					m_filter = Filter.All;
				m_scrollBar.ScrollToTop();
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			int scrollOff = m_scrollBar.ScrollOffset;

			SpriteBatch.Begin();

			SpriteBatch.Draw(m_filterTextures[(int) m_filter], new Vector2(DrawAreaWithOffset.X + 4, DrawAreaWithOffset.Y + 2), Color.White);

			List<OnlineEntry> filtered = m_onlineList;
			if (m_filter != Filter.All)
			{
				switch (m_filter)
				{
					case Filter.Friends:
						filtered = m_onlineList.Where(oe => m_friendList.Contains(oe.Name)).ToList();
						break;
					case Filter.Admins: //show admins only for the admins view: other types will be continue'd
						filtered = m_onlineList.Where(oe =>
						{
							switch (oe.Icon)
							{
								case PaperdollIconType.Normal:
								case PaperdollIconType.Party:
								case PaperdollIconType.SLNBot:
									return false;
							}
							return true;
						}).ToList();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			for (int i = scrollOff; i < scrollOff + m_scrollBar.LinesToRender && i < filtered.Count; ++i)
			{
				int yCoord = DRAW_OFFSET_Y + DrawAreaWithOffset.Y + (i - m_scrollBar.ScrollOffset)*13;
				//draw icon
				SpriteBatch.Draw(ChatTab.GetChatIcon(EOChatRenderer.GetChatTypeFromPaperdollIcon(filtered[i].Icon)), new Vector2(DrawAreaWithOffset.X + DRAW_ICON_X, yCoord), Color.White);
				//drawtext name
				SpriteBatch.DrawString(EOGame.Instance.DBGFont, filtered[i].Name, new Vector2(DrawAreaWithOffset.X + DRAW_NAME_X, yCoord), Color.Black);
				//drawtext title
				SpriteBatch.DrawString(EOGame.Instance.DBGFont, filtered[i].Title, new Vector2(DrawAreaWithOffset.X + DRAW_TITLE_X, yCoord), Color.Black);
				//drawtext guild
				SpriteBatch.DrawString(EOGame.Instance.DBGFont, filtered[i].Guild, new Vector2(DrawAreaWithOffset.X + DRAW_GUILD_X, yCoord), Color.Black);
				//drawtext class
				SpriteBatch.DrawString(EOGame.Instance.DBGFont, filtered[i].Class, new Vector2(DrawAreaWithOffset.X + DRAW_CLASS_X, yCoord), Color.Black);
			}
			SpriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
