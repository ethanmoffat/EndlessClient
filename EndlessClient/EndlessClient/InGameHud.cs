using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XNAControls;

namespace EndlessClient
{
	public enum InGameStates
	{
		News = -1,
		Inventory = 0,
		Active = 2,
		Passive = 3,
		Chat = 4,
		Stats = 5,
		Online = 6,
		Party = 7,
		Settings = 9,
		Help = 10
	}
	
	/// <summary>
	/// Note that this is NOT an XNAControl - it is just a DrawableGameComponent
	/// </summary>
	public class HUD : DrawableGameComponent
	{
		private const int NUM_BTN = 11;
		private Texture2D mainFrame, mainButtonTexture;
		//might need to consider making an EOPanels file and deriving from XNAPanel
		//	to support eo-specific functionality that I'm going to need...
		private XNAPanel pnlInventory, pnlActiveSpells, pnlPassiveSpells, pnlChat, pnlStats;
		private XNAPanel pnlNews, pnlOnline, pnlParty, pnlSettings, pnlHelp;
		private XNAButton[] mainBtn;
		private SpriteBatch SpriteBatch;
		private InGameStates state;
		private EOChatRenderer chatRenderer;
		private ChatTab newsTab;

		public HUD(Game g)
			: base(g)
		{
			mainFrame = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 1, true);
			mainButtonTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 25);
			mainBtn = new XNAButton[NUM_BTN];

			//set up panels
			Texture2D invBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 44);
			pnlInventory = new XNAPanel(g, new Rectangle(102, 330, invBG.Width, invBG.Height));
			pnlInventory.BackgroundImage = invBG;
			pnlInventory.Visible = false;

			Texture2D spellsBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 62);
			pnlActiveSpells = new XNAPanel(g, new Rectangle(102, 330, spellsBG.Width, spellsBG.Height));
			pnlActiveSpells.BackgroundImage = spellsBG;
			pnlActiveSpells.Visible = false;

			pnlPassiveSpells = new XNAPanel(g, new Rectangle(102, 330, spellsBG.Width, spellsBG.Height));
			pnlPassiveSpells.BackgroundImage = spellsBG;
			pnlPassiveSpells.Visible = false;

			Texture2D chatBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 28);
			pnlChat = new XNAPanel(g, new Rectangle(102, 330, chatBG.Width, chatBG.Height));
			pnlChat.BackgroundImage = chatBG;
			pnlChat.Visible = false;

			Texture2D statsBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 34);
			pnlStats = new XNAPanel(g, new Rectangle(102, 330, statsBG.Width, statsBG.Height));
			pnlStats.BackgroundImage = statsBG;
			pnlStats.Visible = false;

			Texture2D onlineBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 36);
			pnlOnline = new XNAPanel(g, new Rectangle(102, 330, onlineBG.Width, onlineBG.Height));
			pnlOnline.BackgroundImage = onlineBG;
			pnlOnline.Visible = false;

			Texture2D partyBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 42);
			pnlParty = new XNAPanel(g, new Rectangle(102, 330, partyBG.Width, partyBG.Height));
			pnlParty.BackgroundImage = partyBG;
			pnlParty.Visible = false;

			Texture2D settingsBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 47);
			pnlSettings = new XNAPanel(g, new Rectangle(102, 330, settingsBG.Width, settingsBG.Height));
			pnlSettings.BackgroundImage = settingsBG;
			pnlSettings.Visible = false;

			Texture2D helpBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 63);
			pnlHelp = new XNAPanel(g, new Rectangle(102, 330, helpBG.Width, helpBG.Height));
			pnlHelp.BackgroundImage = helpBG;
			pnlHelp.Visible = false;

			Texture2D newsBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 48);
			pnlNews = new XNAPanel(g, new Rectangle(102, 330, newsBG.Width, newsBG.Height));
			pnlNews.BackgroundImage = newsBG;

			for (int i = 0; i < NUM_BTN; ++i)
			{
				Texture2D _out = new Texture2D(g.GraphicsDevice, mainButtonTexture.Width / 2, mainButtonTexture.Height / NUM_BTN);
				Texture2D _ovr = new Texture2D(g.GraphicsDevice, mainButtonTexture.Width / 2, mainButtonTexture.Height / NUM_BTN);

				Rectangle _outRec = new Rectangle(0, i * _out.Height, _out.Width, _out.Height);
				Rectangle _ovrRec = new Rectangle(_ovr.Width, i * _ovr.Height, _ovr.Width, _ovr.Height);

				Color[] _outBuf = new Color[_outRec.Width * _outRec.Height];
				Color[] _ovrBuf = new Color[_ovrRec.Width * _ovrRec.Height];

				mainButtonTexture.GetData(0, _outRec, _outBuf, 0, _outBuf.Length);
				_out.SetData<Color>(_outBuf);

				mainButtonTexture.GetData(0, _ovrRec, _ovrBuf, 0, _ovrBuf.Length);
				_ovr.SetData<Color>(_ovrBuf);

				//0-5: left side, starting at 59, 327 with increments of 20
				//6-10: right side, starting at 587, 347
				Vector2 btnLoc = new Vector2(i < 6 ? 62 : 590, (i < 6 ? 330 : 350) + ((i < 6 ? i : i - 6) * 20));

				mainBtn[i] = new XNAButton(g, new Texture2D[] { _out, _ovr }, btnLoc);
				//mainBtn[i].Visible = false;
			}

			//left button onclick events
			mainBtn[0].OnClick += OnViewInventory;
			mainBtn[1].OnClick += OnViewMap;
			mainBtn[2].OnClick += OnViewActiveSkills;
			mainBtn[3].OnClick += OnViewPassiveSkills;
			mainBtn[4].OnClick += OnViewChat;
			mainBtn[5].OnClick += OnViewStats;

			//right button onclick events
			mainBtn[6].OnClick += OnViewPlayers;
			mainBtn[7].OnClick += OnViewParty;
			//mainBtn[8].OnClick += OnViewMacro; //not implemented in EO client
			mainBtn[9].OnClick += OnViewSettings;
			mainBtn[10].OnClick += OnViewHelp;

			SpriteBatch = new SpriteBatch(g.GraphicsDevice);

			state = InGameStates.News;

			chatRenderer = new EOChatRenderer(g);
			chatRenderer.SetParent(pnlChat);
			//for (int i = 1; i <= 8; ++i)
			//{
			//	bool icon = (i == 1 || i == 4 || i == 5 || i == 6 || i == 8);
			//	chatRenderer.AddTextToTab(ChatTabs.Local, string.Format("Player{0}", i), string.Format("Test string {1} {0}", i, "Local"), icon ? ChatType.Note : ChatType.None);
			//	chatRenderer.AddTextToTab(ChatTabs.Global, string.Format("Player{0}", i), string.Format("Test string {1} {0}", i, "Global"), icon ? ChatType.Note : ChatType.None);
			//	chatRenderer.AddTextToTab(ChatTabs.Group, string.Format("Player{0}", i), string.Format("Test string {1} {0}", i, "Group"), icon ? ChatType.Note : ChatType.None);
			//	chatRenderer.AddTextToTab(ChatTabs.System, string.Format("Player{0}", i), string.Format("Test string {1} {0}", i, "System"), icon ? ChatType.Note : ChatType.None);
			//}

			newsTab = new ChatTab(g, pnlNews);
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch.Begin();
			SpriteBatch.Draw(mainFrame, new Vector2(0, 0), Color.White);

			switch(state)
			{
				default:
					break;
			}

			SpriteBatch.End();

			base.Draw(gameTime);
		}

		#region HelperMethods
		private void _doStateChange(InGameStates newGameState)
		{
			state = newGameState;

			pnlNews.Visible = false;

			pnlInventory.Visible = false;
			pnlActiveSpells.Visible = false;
			pnlPassiveSpells.Visible = false;
			pnlChat.Visible = false;
			pnlStats.Visible = false;

			pnlOnline.Visible = false;
			pnlParty.Visible = false;
			pnlSettings.Visible = false;
			pnlHelp.Visible = false;

			switch(state)
			{
				case InGameStates.Inventory:
					pnlInventory.Visible = true;
					break;
				case InGameStates.Active:
					pnlActiveSpells.Visible = true;
					break;
				case InGameStates.Passive:
					pnlPassiveSpells.Visible = true;
					break;
				case InGameStates.Chat:
					pnlChat.Visible = true;
					break;
				case InGameStates.Stats:
					pnlStats.Visible = true;
					break;
				case InGameStates.Online:
					pnlOnline.Visible = true;
					break;
				case InGameStates.Party:
					pnlParty.Visible = true;
					break;
				case InGameStates.Settings:
					pnlSettings.Visible = true;
					break;
				case InGameStates.Help:
					pnlHelp.Visible = true;
					break;
			}
		}
		#endregion

		public void SetNews(IList<string> lines)
		{
			if(lines.Count == 0)
				return;

			if(lines.Count == 1)
			{
				_doStateChange(InGameStates.Chat);
			}
			else
			{
				for(int i = 1; i < lines.Count; ++i)
				{
					newsTab.AddText(null, lines[i], ChatType.Note);
					if(i != lines.Count - 1)
						newsTab.AddText(null, " ");
				}
			}
			
			chatRenderer.AddTextToTab(ChatTabs.Local, "Server", lines[0], ChatType.Note, ChatColor.Server);
		}

		#region ButtonClickEventHandlers
		private void OnViewInventory(object sender, EventArgs e)
		{
			_doStateChange(InGameStates.Inventory);
		}

		private void OnViewMap(object sender, EventArgs e)
		{
			/* Check if map file allows minimap viewing */
			/* set flag accordingly */
		}

		private void OnViewActiveSkills(object sender, EventArgs e)
		{
			_doStateChange(InGameStates.Active);
		}
		private void OnViewPassiveSkills(object sender, EventArgs e)
		{
			_doStateChange(InGameStates.Passive);
		}
		private void OnViewChat(object sender, EventArgs e)
		{
			_doStateChange(InGameStates.Chat);
		}
		private void OnViewStats(object sender, EventArgs e)
		{
			_doStateChange(InGameStates.Stats);
		}
		private void OnViewPlayers(object sender, EventArgs e)
		{
			_doStateChange(InGameStates.Online);
		}
		private void OnViewParty(object sender, EventArgs e)
		{
			_doStateChange(InGameStates.Party);
		}
		private void OnViewSettings(object sender, EventArgs e)
		{
			_doStateChange(InGameStates.Settings);
		}
		private void OnViewHelp(object sender, EventArgs e)
		{
			/* pop up the help dialog */
			_doStateChange(InGameStates.Help);
		}
		#endregion

		protected override void Dispose(bool disposing)
		{
			foreach (XNAButton btn in mainBtn)
				btn.Close();

			SpriteBatch.Dispose();

			pnlInventory.Close();
			pnlActiveSpells.Close();
			pnlPassiveSpells.Close();
			pnlChat.Close();
			pnlStats.Close();
			pnlOnline.Close();
			pnlParty.Close();
			pnlSettings.Close();

			base.Dispose(disposing);
		}
	}
}
