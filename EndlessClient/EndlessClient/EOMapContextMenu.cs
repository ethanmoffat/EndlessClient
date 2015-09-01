using System;
using System.Collections.Generic;
using EOLib;
using EOLib.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using Color = Microsoft.Xna.Framework.Color;

namespace EndlessClient
{
	public class EOMapContextMenu : XNAControl
	{
		private enum MenuAction
		{
			Paperdoll,
			Book,
			Join,
			Invite,
			Trade,
			Whisper,
			Friend,
			Ignore,
			NUM_MENU_ACTIONS
		}

		//rules for draw location:
		// 1. try to the right first - if it doesn't fit (width-wise), go to the left
		// 2. if it will be out of the game area, move it up/down so that it is not clipped by the screen bounds

		//gfx 41 for text/overlay text
		//hide and disable as soon as mouse click either on or out of bounds -- obviously handle event for mouse click on particular text
		private readonly Texture2D m_bg, m_bgOver, m_fill;
		private readonly Dictionary<Rectangle, Action<object, EventArgs>> m_menuActions = new Dictionary<Rectangle, Action<object, EventArgs>>();
		private Action<object, EventArgs> m_clickEvent; //event for when a 'click' is done
		private Rectangle? m_overRect; //rectangle for hover region
		private EOCharacterRenderer m_rend;

		private DateTime? m_lastPartyRequestedTime, m_lastTradeRequestedTime;

		private readonly PacketAPI m_api;

		public EOMapContextMenu(PacketAPI api)
		{
			m_api = api;
			//first, load up the images. split in half: the right half is the 'over' text
			Texture2D bgImage = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 41, true);

			//this GFX is stupid. a bunch of white space throws off coordinates so I have to use hard-coded values
			const int W = 96, H = 137;
			Color[] dat = new Color[W*H];
			m_bg = new Texture2D(EOGame.Instance.GraphicsDevice, W, H);
			m_bgOver = new Texture2D(EOGame.Instance.GraphicsDevice, W, H);

			bgImage.GetData(0, new Rectangle(0, 0, W, H), dat, 0, dat.Length);
			m_bg.SetData(dat);

			bgImage.GetData(0, new Rectangle(W, 0, W, H), dat, 0, dat.Length);
			m_bgOver.SetData(dat);

			//define regions for clicking and their associated actions
			//6,11,86,14
			for (int i = 0; i < (int) MenuAction.NUM_MENU_ACTIONS; ++i)
			{
				Rectangle region = new Rectangle(6, (i < 5 ? 11 : 13)+ 14*i, 86, 14);
				m_menuActions.Add(region, _getActionFromMenuAction((MenuAction) i));
			}

			//set the fill color
			m_fill = new Texture2D(EOGame.Instance.GraphicsDevice, 1, 1);
			m_fill.SetData(new [] { Color.White });

			//set default control stuff;
			_setSize(W, H);
			Visible = false;
			SetParent(null);
		}

		public void SetCharacterRenderer(EOCharacterRenderer rend)
		{
			m_rend = rend;
			//update draw location
			Rectangle rendRect = rend.DrawAreaWithOffset;

			DrawLocation = new Vector2(rendRect.Right + 20, rendRect.Y + rend.TopPixel);

			if (DrawAreaWithOffset.Right > Game.GraphicsDevice.PresentationParameters.BackBufferWidth - 15)
			{
				//case: goes off the right side of the screen, show on the left
				DrawLocation = new Vector2(rendRect.X - DrawArea.Width - 20, drawLocation.Y);
			}
			//MAGIC NUMBER: 308px is the bottom of the display area for map stuff
			if (DrawAreaWithOffset.Bottom > 308)
			{
				//case: goes off bottom of the screen, adjust new rectangle so it is above 308
				DrawLocation = new Vector2(drawLocation.X, 298 - DrawArea.Height);
			}
			else if (DrawAreaWithOffset.Y < 25)
			{
				//case: goes off top of screen, adjust new rectangle so it aligns with top of character head
				DrawLocation = new Vector2(drawLocation.X, 35);
			}

			EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_MENU_BELONGS_TO_PLAYER, m_rend.Character.Name);

			Visible = true;
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible || !Game.IsActive)
				return;

			//get the X and Y to check: take DrawAreaWithOffset into account
			MouseState currentState = Mouse.GetState();
			int checkX = currentState.X - DrawAreaWithOffset.X;
			int checkY = currentState.Y - DrawAreaWithOffset.Y;

			//find the region that currently is being hovered over (or, none)
			bool found = false;
			foreach (Rectangle r in m_menuActions.Keys)
			{
				if (r.Contains(checkX, checkY))
				{
					m_overRect = r;
					m_clickEvent = m_menuActions[r];
					found = true;
					break;
				}
			}

			if (!found)
			{
				m_clickEvent = null;
				m_overRect = null;
			}

			//left clicking will do the selected action if available, otherwise hide the menu
			if (currentState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
			{
				if(m_clickEvent != null) //item selected: do click event
					m_clickEvent(this, null);
				Visible = false;
			}

			//any right clicking will hide the menu
			if (currentState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)
				Visible = false;

			if (!Visible)
				PreviousMouseState = currentState;

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			SpriteBatch.Begin();

			//White @ 75% opacity background
			SpriteBatch.Draw(m_fill, DrawAreaWithOffset, Color.FromNonPremultiplied(0xff, 0xff, 0xff, 192));

			SpriteBatch.Draw(m_bg, DrawAreaWithOffset, Color.White);
			if (m_overRect != null)
			{
				SpriteBatch.Draw(m_bgOver,
					new Vector2(m_overRect.Value.X + DrawAreaWithOffset.X, m_overRect.Value.Y + DrawAreaWithOffset.Y), 
					m_overRect,
					Color.White);
			}

			SpriteBatch.End();

			base.Draw(gameTime);
		}

		/* Helper maps MenuAction enum value to a member method for easy initialization */
		private Action<object, EventArgs> _getActionFromMenuAction(MenuAction menuAction)
		{
			switch (menuAction)
			{
				case MenuAction.Paperdoll: return _eventShowPaperdoll;
				case MenuAction.Book: return _eventShowBook;
				case MenuAction.Join: return _eventJoinParty;
				case MenuAction.Invite: return _eventInviteToParty;
				case MenuAction.Trade: return _eventTrade;
				case MenuAction.Whisper: return _eventPrivateMessage;
				case MenuAction.Friend: return _eventAddFriend;
				case MenuAction.Ignore: return _eventAddIgnore;
				default: throw new ArgumentOutOfRangeException("menuAction");
			}
		}

		/* EVENT HANDLERS FOR THE DIFFERENT MENU ITEMS */
		//todo: finish the todo items below
		private void _eventShowPaperdoll(object sender, EventArgs e)
		{
			if (!m_api.RequestPaperdoll((short) m_rend.Character.ID))
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
		}
		private void _eventShowBook(object arg1, EventArgs arg2)
		{
			EODialog.Show("TODO: Show quest info", "TODO ITEM", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
		}
		private void _eventJoinParty(object arg1, EventArgs arg2)
		{
			if (((EOGame) Game).Hud.PlayerIsPartyMember((short)m_rend.Character.ID))
			{
				((EOGame) Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, m_rend.Character.Name, DATCONST2.STATUS_LABEL_PARTY_IS_ALREADY_MEMBER);
				return;
			}

			if (m_lastPartyRequestedTime != null && (DateTime.Now - m_lastPartyRequestedTime.Value).TotalSeconds < Constants.PartyRequestTimeoutSeconds)
			{
				((EOGame) Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_PARTY_RECENTLY_REQUESTED);
				return;
			}

			if (!m_api.PartyRequest(PartyRequestType.Join, (short) m_rend.Character.ID))
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			m_lastPartyRequestedTime = DateTime.Now;
			((EOGame) Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_PARTY_REQUESTED_TO_JOIN);
		}
		private void _eventInviteToParty(object arg1, EventArgs arg2)
		{
			if (((EOGame)Game).Hud.PlayerIsPartyMember((short)m_rend.Character.ID))
			{
				((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, m_rend.Character.Name, DATCONST2.STATUS_LABEL_PARTY_IS_ALREADY_MEMBER);
				return;
			}

			if (m_lastPartyRequestedTime != null && (DateTime.Now - m_lastPartyRequestedTime.Value).TotalSeconds < Constants.PartyRequestTimeoutSeconds)
			{
				((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_PARTY_RECENTLY_REQUESTED);
				return;
			}

			if (!m_api.PartyRequest(PartyRequestType.Invite, (short)m_rend.Character.ID))
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			m_lastPartyRequestedTime = DateTime.Now;
			((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, m_rend.Character.Name, DATCONST2.STATUS_LABEL_PARTY_IS_INVITED);
		}
		private void _eventTrade(object arg1, EventArgs arg2)
		{
			if (World.Instance.MainPlayer.ActiveCharacter.CurrentMap == World.Instance.JailMap)
				EODialog.Show(World.GetString(DATCONST2.JAIL_WARNING_CANNOT_TRADE),
					World.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
					XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
			else
			{
				if(m_lastTradeRequestedTime != null && (DateTime.Now - m_lastTradeRequestedTime.Value).TotalSeconds < Constants.TradeRequestTimeoutSeconds)
				{
					((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_TRADE_RECENTLY_REQUESTED);
					return;
				}
				m_lastTradeRequestedTime = DateTime.Now;
				if (!m_api.TradeRequest((short)m_rend.Character.ID))
					((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
				//todo: is this correct text?
				((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_TRADE_REQUESTED_TO_TRADE);
			}
		}
		private void _eventPrivateMessage(object arg1, EventArgs arg2)
		{
			EOGame.Instance.Hud.SetChatText("!" + m_rend.Character.Name);
		}
		private void _eventAddFriend(object arg1, EventArgs arg2)
		{
			EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, m_rend.Character.Name, DATCONST2.STATUS_LABEL_WILL_BE_YOUR_FRIEND);
			InteractList.WriteNewFriend(m_rend.Character.Name);
		}
		private void _eventAddIgnore(object arg1, EventArgs arg2)
		{
			EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, m_rend.Character.Name, DATCONST2.STATUS_LABEL_WILL_BE_IGNORED);
			InteractList.WriteNewIgnore(m_rend.Character.Name);
		}

		/* DISPOSABLE PATTERN */
		public new void Dispose()
		{
			Dispose(true);
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_bg.Dispose();
				m_bgOver.Dispose();
				m_fill.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
