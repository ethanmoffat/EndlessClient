using System;
using System.Collections.Generic;
using System.Threading;
using EndlessClient.Handlers;
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
		private static readonly object clockLock = new object();

		private static readonly string[] ButtonStatusStrings = 
		{
			"[ Button ] See the inventory",
			"[ Button ] Look at the map",
			"[ Button ] Active Skills",
			"[ Button ] Passive Skills",
			"[ Button ] Talk with other people",
			"[ Button ] Character status",
			"[ Button ] Who is online ?",
			"[ Button ] Group with other people",
			"[ Button ] Keyboard macro and hotkeys",
			"[ Button ] Game settings and options",
			"[ Button ] Game help"
		};

		private const int NUM_BTN = 11;
		private readonly Texture2D mainFrame;
		//might need to consider making an EOPanels file and deriving from XNAPanel
		//	to support eo-specific functionality that I'm going to need...
		private readonly XNAPanel pnlInventory, pnlActiveSpells, pnlPassiveSpells, pnlChat, pnlStats;
		private readonly XNAPanel pnlNews, pnlOnline, pnlParty, pnlSettings, pnlHelp;
		private readonly XNAButton[] mainBtn;
		private readonly SpriteBatch SpriteBatch;
		private readonly EOChatRenderer chatRenderer;
		private EOInventory inventory;
		private EOCharacterStats stats;
		private readonly ChatTab newsTab;

		private readonly XNALabel statusLabel; //label for status (mouse-over buttons)
		private readonly XNALabel clockLabel; //label that is updated on a timer
		private Timer clockTimer;

		private DateTime? statusStartTime;

		private InGameStates state;
		private ChatMode currentChatMode;
		private Texture2D modeTexture;
		private bool modeTextureLoaded;
		/// <summary>
		/// the primary textbox for chat
		/// </summary>
		private readonly XNATextBox chatTextBox;

		//HP, SP, TP, TNL (in that order)
		private readonly HUDElement[] StatusBars = new HUDElement[4];
		
		public HUD(Game g)
			: base(g)
		{
			mainFrame = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 1, true);
			Texture2D mainButtonTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 25);
			mainBtn = new XNAButton[NUM_BTN];

			//set up panels
			Texture2D invBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 44);
			pnlInventory = new XNAPanel(new Rectangle(102, 330, invBG.Width, invBG.Height))
			{
				BackgroundImage = invBG,
				Visible = false
			};

			Texture2D spellsBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 62);
			pnlActiveSpells = new XNAPanel(new Rectangle(102, 330, spellsBG.Width, spellsBG.Height))
			{
				BackgroundImage = spellsBG,
				Visible = false
			};

			pnlPassiveSpells = new XNAPanel(new Rectangle(102, 330, spellsBG.Width, spellsBG.Height))
			{
				BackgroundImage = spellsBG,
				Visible = false
			};

			Texture2D chatBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 28);
			pnlChat = new XNAPanel(new Rectangle(102, 330, chatBG.Width, chatBG.Height))
			{
				BackgroundImage = chatBG,
				Visible = false
			};

			Texture2D statsBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 34);
			pnlStats = new XNAPanel(new Rectangle(102, 330, statsBG.Width, statsBG.Height))
			{
				BackgroundImage = statsBG,
				Visible = false
			};

			Texture2D onlineBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 36);
			pnlOnline = new XNAPanel(new Rectangle(102, 330, onlineBG.Width, onlineBG.Height))
			{
				BackgroundImage = onlineBG,
				Visible = false
			};

			Texture2D partyBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 42);
			pnlParty = new XNAPanel(new Rectangle(102, 330, partyBG.Width, partyBG.Height))
			{
				BackgroundImage = partyBG,
				Visible = false
			};

			Texture2D settingsBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 47);
			pnlSettings = new XNAPanel(new Rectangle(102, 330, settingsBG.Width, settingsBG.Height))
			{
				BackgroundImage = settingsBG,
				Visible = false
			};

			Texture2D helpBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 63);
			pnlHelp = new XNAPanel(new Rectangle(102, 330, helpBG.Width, helpBG.Height))
			{
				BackgroundImage = helpBG,
				Visible = false
			};

			Texture2D newsBG = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 48);
			pnlNews = new XNAPanel(new Rectangle(102, 330, newsBG.Width, newsBG.Height)) {BackgroundImage = newsBG};

			//for easy update of all panels via foreach
			List<XNAPanel> pnlCollection = new List<XNAPanel>(10)
			{
				pnlInventory,
				pnlActiveSpells,
				pnlPassiveSpells,
				pnlChat,
				pnlStats,
				pnlOnline,
				pnlParty,
				pnlSettings,
				pnlHelp,
				pnlNews
			};
			//pnlCollection.Add(pnlMacro); //if this ever happens...

			pnlCollection.ForEach(_pnl =>
			{
				_pnl.IgnoreDialog(typeof(EOPaperdollDialog));
				_pnl.IgnoreDialog(typeof(EOChestDialog));
			});

			for (int i = 0; i < NUM_BTN; ++i)
			{
				Texture2D _out = new Texture2D(g.GraphicsDevice, mainButtonTexture.Width / 2, mainButtonTexture.Height / NUM_BTN);
				Texture2D _ovr = new Texture2D(g.GraphicsDevice, mainButtonTexture.Width / 2, mainButtonTexture.Height / NUM_BTN);

				Rectangle _outRec = new Rectangle(0, i * _out.Height, _out.Width, _out.Height);
				Rectangle _ovrRec = new Rectangle(_ovr.Width, i * _ovr.Height, _ovr.Width, _ovr.Height);

				Color[] _outBuf = new Color[_outRec.Width * _outRec.Height];
				Color[] _ovrBuf = new Color[_ovrRec.Width * _ovrRec.Height];

				mainButtonTexture.GetData(0, _outRec, _outBuf, 0, _outBuf.Length);
				_out.SetData(_outBuf);

				mainButtonTexture.GetData(0, _ovrRec, _ovrBuf, 0, _ovrBuf.Length);
				_ovr.SetData(_ovrBuf);

				//0-5: left side, starting at 59, 327 with increments of 20
				//6-10: right side, starting at 587, 347
				Vector2 btnLoc = new Vector2(i < 6 ? 62 : 590, (i < 6 ? 330 : 350) + ((i < 6 ? i : i - 6) * 20));

				mainBtn[i] = new XNAButton(new [] { _out, _ovr }, btnLoc);
				mainBtn[i].IgnoreDialog(typeof(EOChestDialog));
				mainBtn[i].IgnoreDialog(typeof(EOPaperdollDialog));
				//mainBtn[i].ignoreDialog(typeof(EOTradingDialog)); //etc, etc for all other dialogs that should be ignored when they're top-most
			}

			//left button onclick events
			mainBtn[0].OnClick += (s,e) => _doStateChange(InGameStates.Inventory);
			mainBtn[1].OnClick += (s,e) => World.Instance.ActiveMapRenderer.ToggleMapView();
			mainBtn[2].OnClick += (s,e) => _doStateChange(InGameStates.Active);
			mainBtn[3].OnClick += (s, e) => _doStateChange(InGameStates.Passive);
			mainBtn[4].OnClick += (s, e) => _doStateChange(InGameStates.Chat);
			mainBtn[5].OnClick += (s, e) => _doStateChange(InGameStates.Stats);

			//right button onclick events
			mainBtn[6].OnClick += (s, e) => _doStateChange(InGameStates.Online);
			mainBtn[7].OnClick += (s, e) => _doStateChange(InGameStates.Party);
			//mainBtn[8].OnClick += OnViewMacro; //not implemented in EO client
			mainBtn[9].OnClick += (s, e) => _doStateChange(InGameStates.Settings);
			mainBtn[10].OnClick += (s, e) => _doStateChange(InGameStates.Help);

			SpriteBatch = new SpriteBatch(g.GraphicsDevice);

			state = InGameStates.News;

			chatRenderer = new EOChatRenderer();
			chatRenderer.SetParent(pnlChat);
			chatRenderer.AddTextToTab(ChatTabs.Global, "Server", "Begin your line with a '~' to send a message to everyone online!", ChatType.Note, ChatColor.Server);
			chatRenderer.AddTextToTab(ChatTabs.Global, "Server", "Do not curse, harass or flood on the global channel, this is not allowed.", ChatType.Note, ChatColor.Server);

			newsTab = new ChatTab(pnlNews);

			chatTextBox = new XNATextBox(new Rectangle(124, 308, 440, 19), g.Content.Load<Texture2D>("cursor"), "Microsoft Sans Serif", 8.0f)
			{
				Selected = true,
				Visible = true,
				MaxChars = 140
			};
			chatTextBox.IgnoreDialog(typeof(EOPaperdollDialog));
			chatTextBox.IgnoreDialog(typeof(EOChestDialog));
			chatTextBox.OnEnterPressed += _doTalk;
			chatTextBox.OnClicked += (s, e) =>
			{
				//make sure clicking on the textarea selects it (this is an annoying problem in the original client)
				if (((EOGame)g).Dispatcher.Subscriber != null)
					((XNATextBox) (g as EOGame).Dispatcher.Subscriber).Selected = false;

				(g as EOGame).Dispatcher.Subscriber = chatTextBox;
				chatTextBox.Selected = true;
			};
			chatTextBox.OnTextChanged += (s, e) =>
			{
				if (chatTextBox.Text.Length <= 0)
				{
					modeTextureLoaded = false;
					modeTexture.Dispose();
					modeTexture = null;

					currentChatMode = ChatMode.NoText;
					return;
				}

				switch (chatTextBox.Text[0])
				{
					case '!': currentChatMode = ChatMode.Private; break;
					case '~': currentChatMode = ChatMode.Global; break;
					case '@':
					{
						if (World.Instance.MainPlayer.ActiveCharacter.AdminLevel == AdminLevel.Player)
							goto default;
						currentChatMode = ChatMode.Admin;
					}
						break;
					case '\'': currentChatMode = ChatMode.Group; break;
					case '&':
					{
						if (World.Instance.MainPlayer.ActiveCharacter.GuildName == "")
							goto default;
						currentChatMode = ChatMode.Guild;
					}
						break;
					default: currentChatMode = ChatMode.Public; break;
				}
			};
			
			((EOGame)g).Dispatcher.Subscriber = chatTextBox;

			statusLabel = new XNALabel(new Rectangle(97, 455, 1, 1), "Microsoft Sans Serif", 7.0f);
			clockLabel = new XNALabel(new Rectangle(558, 455, 1, 1), "Microsoft Sans Serif", 7.0f);

			StatusBars[0] = new HudElementHP();
			StatusBars[1] = new HudElementTP();
			StatusBars[2] = new HudElementSP();
			StatusBars[3] = new HudElementTNL();
		}

		public override void Initialize()
		{
			World.Instance.ActiveMapRenderer.Visible = true;
			if (!Game.Components.Contains(World.Instance.ActiveMapRenderer))
				Game.Components.Add(World.Instance.ActiveMapRenderer);
			World.Instance.ActiveCharacterRenderer.Visible = true;

			DateTime usageTracking = DateTime.Now;
			clockTimer = new Timer(threadState =>
			{
				if ((DateTime.Now - usageTracking).TotalMinutes >= 1)
				{
					World.Instance.MainPlayer.ActiveCharacter.Stats.SetUsage(World.Instance.MainPlayer.ActiveCharacter.Stats.usage + 1);
					usageTracking = DateTime.Now;
				}

				string fmt = string.Format("{0,2:D2}:{1,2:D2}:{2,2:D2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
				lock(clockLock) clockLabel.Text = fmt;

				if (statusStartTime.HasValue && (DateTime.Now - statusStartTime.Value).TotalMilliseconds > 3000)
				{
					SetStatusLabel("");
				}

			}, null, 0, 1000);

			//the draw orders are adjusted for child items in the constructor.
			//calling SetParent will break this.
			inventory = new EOInventory(pnlInventory);

			stats = new EOCharacterStats(pnlStats);
			stats.Initialize();

			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			//polling loop to set status label for mouseover event for buttons
			//this is in lieu of creating a OnMouseOver/OnMouseEnter or whatever,
			//some kind of event-driven mechanism in XNAControls (which should be
			//done at some point since polling loops are bad and you should feel bad)
			for (int i = 0; i < mainBtn.Length; ++i)
			{
				XNAButton btn = mainBtn[i];
				if (btn.MouseOver)
				{
					SetStatusLabel(ButtonStatusStrings[i]);
					break;
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch.Begin();
			SpriteBatch.Draw(mainFrame, new Vector2(0, 0), Color.White);

			//show the little graphic next
			if (currentChatMode != ChatMode.NoText && !modeTextureLoaded)
			{
				Texture2D chatModeTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 31);
				int oneMode = chatModeTexture.Height/8;
				Color[] data = new Color[chatModeTexture.Width*oneMode]; //there are 8 chat mode graphics in the texture
				chatModeTexture.GetData(0, new Rectangle(0, (int) currentChatMode*oneMode, chatModeTexture.Width, oneMode), data, 0,
					data.Length);
				modeTexture = new Texture2D(Game.GraphicsDevice, chatModeTexture.Width, oneMode);
				modeTexture.SetData(data);
				modeTextureLoaded = true;
			}

			if(modeTextureLoaded && modeTexture != null)
				SpriteBatch.Draw(modeTexture, new Vector2(16, 309), Color.White);

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
					stats.Refresh();
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

		/// <summary>
		/// Event for enter keypress of primary textbox. Does the chat
		/// </summary>
		private void _doTalk(object sender, EventArgs e)
		{
			if (chatTextBox.Text.Length <= 0)
				return;

			string chatText = chatTextBox.Text;
			chatTextBox.Text = "";
			switch (chatText[0])
			{
				case '@':  //admin talk
					if (World.Instance.MainPlayer.ActiveCharacter.AdminLevel == AdminLevel.Player)
						goto default;
					break;
				case '\'': //group talk
					if (!Talk.Speak(TalkType.Party, chatText.Substring(1)))
					{
						_returnToLogin();
						break;
					}
					//note: more processing of colors/icons is needed here
					AddChat(ChatTabs.Group, World.Instance.MainPlayer.ActiveCharacter.Name, chatText.Substring(1));
					break;
				case '&':  //guild talk
					if (World.Instance.MainPlayer.ActiveCharacter.GuildName == "")
						goto default;
					if (!Talk.Speak(TalkType.Guild, chatText.Substring(1)))
					{
						_returnToLogin();
						break;
					}
					//note: more processing of colors/icons is needed here
					AddChat(ChatTabs.Group, World.Instance.MainPlayer.ActiveCharacter.Name, chatText.Substring(1));
					break;
				case '~':  //global talk
					if (!Talk.Speak(TalkType.Global, chatText.Substring(1)))
					{
						_returnToLogin();
						break;
					}
					AddChat(ChatTabs.Global, World.Instance.MainPlayer.ActiveCharacter.Name, chatText.Substring(1));
					break;
				case '!':  //private talk
				{
					string character, message;
					if (chatRenderer.SelectedTab.WhichTab == ChatTabs.Private1 || chatRenderer.SelectedTab.WhichTab == ChatTabs.Private2)
					{
						character = chatRenderer.SelectedTab.ChatCharacter;
						message = chatText.Substring(1);
					}
					else
					{
						int firstSpace = chatText.IndexOf(' ');
						if (firstSpace < 7) return; //character names should be 6, leading ! should be 1, 6+1=7 and THAT'S MATH
						character = chatText.Substring(1, firstSpace - 1);
						message = chatText.Substring(firstSpace + 1);
					}

					character = character.Substring(0, 1).ToUpper() + character.Substring(1).ToLower();

					if (!Talk.Speak(TalkType.PM, message, character))
					{
						_returnToLogin();
						break;
					}

					ChatTabs whichPrivateChat = chatRenderer.StartConversation(character);
					//the other player will have their messages rendered in Color.PM on scr
					//this player will have their messages rendered in Color.PM on the PM tab
					if(whichPrivateChat != ChatTabs.None)
						AddChat(whichPrivateChat, World.Instance.MainPlayer.ActiveCharacter.Name, message, ChatType.Note, ChatColor.PM);
				}
					break;
				case '#':  //local command
				{
					string cmd = chatText.Substring(1).ToLower().Trim();
					string[] args = cmd.Split(new[] {' '});
					
					if (args.Length == 1 && args[0] == "nowall")
					{
						World.Instance.ActiveCharacterRenderer.NoWall = !World.Instance.ActiveCharacterRenderer.NoWall;
					}
					else if (args.Length == 2 && args[0] == "find")
					{
						Players.Find(args[1]);
					}
					else if (args.Length == 1 && args[0] == "loc")
					{
						AddChat(ChatTabs.Local, "System", string.Format("Your current location is at Map {0}  x:{1}  y:{2}",
							World.Instance.ActiveMapRenderer.MapRef.MapID,
							World.Instance.MainPlayer.ActiveCharacter.X,
							World.Instance.MainPlayer.ActiveCharacter.Y),
							ChatType.LookingDude);
					}
					else if (args.Length == 1 && cmd == "usage")
					{
						int usage = World.Instance.MainPlayer.ActiveCharacter.Stats.usage;
						AddChat(ChatTabs.Local, "System", string.Format("[x] usage: {0}hrs. {1}min.", usage/60, usage%60));
					}
				}
					break;
				default:
				{
					//send packet to the server
					if (!Talk.Speak(TalkType.Local, chatText))
					{
						_returnToLogin();
						break;
					}
					//do the rendering
					World.Instance.ActiveMapRenderer.MakeSpeechBubble(null, chatText);
					AddChat(ChatTabs.Local, World.Instance.MainPlayer.ActiveCharacter.Name, chatText);
				}
					break;
			}
		}

		private void _returnToLogin()
		{
			//any other logic prior to disconnecting goes here
			EOGame.Instance.LostConnectionDialog();
		}
		#endregion

		#region Public Interface for classes outside HUD
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
						newsTab.AddText(null, " "); //line breaks between entries
				}
				newsTab.SetButtonFlash();
			}
			
			chatRenderer.AddTextToTab(ChatTabs.Local, "Server", lines[0], ChatType.Note, ChatColor.Server);
		}

		public void AddChat(ChatTabs whichTab, string who, string message, ChatType chatType = ChatType.None, ChatColor chatColor = ChatColor.Default)
		{
			chatRenderer.AddTextToTab(whichTab, who, message, chatType, chatColor);
		}

		public void PrivatePlayerNotFound(string character)
		{
			//add message to Sys and close the chat that was opened for 'character' (no good way to synchronize this because of how the packets are)
			chatRenderer.ClosePrivateChat(character);
			AddChat(ChatTabs.System, "", string.Format("{0} could not be found", character), ChatType.Error, ChatColor.Error);
		}

		public ChatTabs GetPrivateChatTab(string character)
		{
			return chatRenderer.StartConversation(character);
		}

		public void SetChatText(string text)
		{
			chatTextBox.Text = text;
		}

		public void SetStatusLabel(string text)
		{
			statusLabel.Text = text;
			statusStartTime = !string.IsNullOrEmpty(text) ? new DateTime?(DateTime.Now) : null;
		}

		public bool UpdateInventory(InventoryItem item)
		{
			if (item.amount == 0)
				inventory.RemoveItem(item.id);
			else
				return inventory.UpdateItem(item);
			return true;
		}

		public bool IsInventoryDragging()
		{
			return !inventory.NoItemsDragging();
		}

		public void RefreshStats()
		{
			if(inventory != null)
				inventory.UpdateWeightLabel();
			if(stats != null)
				stats.Refresh();
		}
		#endregion
		
		protected override void Dispose(bool disposing)
		{
			foreach (XNAButton btn in mainBtn)
				btn.Close();

			newsTab.Dispose();
			inventory.Dispose();
			chatRenderer.Dispose();
			stats.Dispose();

			if(modeTexture != null)
				modeTexture.Dispose();
			SpriteBatch.Dispose();

			pnlInventory.Close();
			pnlActiveSpells.Close();
			pnlPassiveSpells.Close();
			pnlChat.Close();
			pnlStats.Close();
			pnlOnline.Close();
			pnlParty.Close();
			pnlSettings.Close();

			chatTextBox.Dispose();
			statusLabel.Dispose();

			lock (clockLock)
			{
				clockTimer.Change(Timeout.Infinite, Timeout.Infinite);
				clockTimer.Dispose();
				clockLabel.Dispose();
			}

			base.Dispose(disposing);
		}

		public bool InventoryFits(short id)
		{
			return inventory.ItemFits(id);
		}
	}
}
