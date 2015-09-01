
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EOLib;
using EOLib.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;
using Color = System.Drawing.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace EndlessClient
{
	public class EOQuestDialog : EOScrollingListDialog
	{
		public static EOQuestDialog Instance { get; private set; }

		public static void Show(PacketAPI api, short npcIndex, short questID, string name)
		{
			NPCName = name;

			//note: dialog is created in packet callback! sometimes talking to the quest NPC does nothing (if you already completed)!

			if (!api.TalkToQuestNPC(npcIndex, questID))
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
		}

		public static void SetupInstance(PacketAPI api)
		{
			if(Instance != null)
				Instance.Close(null, XNADialogResult.NO_BUTTON_PRESSED);

			Instance = new EOQuestDialog(api);
		}

		private QuestState _stateInfo;
		private Dictionary<short, string> _dialogNames, _links;
		private List<string> _pages;
		private short _pageIndex;

		private static string NPCName { get; set; }

		private EOQuestDialog(PacketAPI api)
			: base(api)
		{
			DialogClosing += (o, e) =>
			{
				if (e.Result == XNADialogResult.OK)
				{
					if (!m_api.RespondToQuestDialog(_stateInfo, DialogReply.Ok))
						((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
				}
				Instance = null;
			};

			_dialogNames = new Dictionary<short, string>();
			_links = new Dictionary<short, string>();
			_pages = new List<string>();

			_setBackgroundTexture(GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 67));

			caption = new XNALabel(new Rectangle(16, 16, 255, 18), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				ForeColor = Color.FromArgb(255, 0xc8, 0xc8, 0xc8)
			};
			caption.SetParent(this);

			m_scrollBar.SetParent(null);
			m_scrollBar.Close();

			m_scrollBar = new EOScrollBar(this, new Vector2(252, 44), new Vector2(16, 99), EOScrollBar.ScrollColors.LightOnMed);
			m_scrollBar.SetParent(this);
			SmallItemStyleMaxItemDisplay = 6;
		}

		public override void Update(GameTime gt)
		{
			try
			{
				base.Update(gt);
			}
			catch
			{
				//Running up against weird thread synchronization error. The call to base XNAControl.Update throws an exception because
				//	the draw area is 0, but the debugger has the correct values. This is a temporary workaround. Basically re-throws
				//	the exception when the draw area is ACTUALLY invalid.
				if (DrawArea.Width*DrawArea.Height == 0 || children.Any(x => x.DrawArea.Width*x.DrawArea.Height == 0))
					throw;

				base.Update(gt);
			}
		}

		public void SetDisplayData(QuestState stateinfo, Dictionary<short, string> dialognames, List<string> pages, Dictionary<short, string> links)
		{
			if(dialognames.Count == 0)
				throw new ArgumentException("Invalid quest dialog data received from server", "dialognames");

			_stateInfo = stateinfo;
			_dialogNames = dialognames;
			_pages = pages;
			_links = links;

			_pageIndex = 0;

			_setDialogTitle();
			_setDialogText();
			_setDialogButtons();
		}

		private void _setDialogTitle()
		{
			string title = NPCName;
			if (!_dialogNames.ContainsKey(_stateInfo.VendorID) && _dialogNames.Count == 1)
				title += " - " + _dialogNames.First();
			else if(_dialogNames.ContainsKey(_stateInfo.VendorID))
				title += " - " + _dialogNames[_stateInfo.VendorID];

			caption.Text = title;
			caption.ResizeBasedOnText();
		}

		private void _setDialogText()
		{
			ClearItemList();

			List<string> rows = new List<string>();
			using (Font f = new Font("Microsoft Sans Serif", 8.5f))
			{
				TextSplitter ts = new TextSplitter(_pages[_pageIndex], f);
				if (!ts.NeedsProcessing)
					rows.Add(_pages[_pageIndex]);
				else
				{
					rows.AddRange(ts.SplitIntoLines());
				}
			}

			int index = 0;
			foreach (var row in rows)
			{
				EODialogListItem rowItem = new EODialogListItem(this, EODialogListItem.ListItemStyle.Small, index++)
				{
					Text = row
				};
				AddItemToList(rowItem, false);
			}

			if (_pageIndex < _pages.Count - 1)
				return;

			EODialogListItem item = new EODialogListItem(this, EODialogListItem.ListItemStyle.Small, index++) { Text = " " };
			AddItemToList(item, false);

			foreach (var link in _links)
			{
				EODialogListItem linkItem = new EODialogListItem(this, EODialogListItem.ListItemStyle.Small, index++)
				{
					Text = link.Value
				};

				var linkIndex = (byte)link.Key;
				linkItem.SetPrimaryTextLink(() => _clickLink(linkIndex));
				AddItemToList(linkItem, false);
			}
		}

		private void _setDialogButtons()
		{
			dlgButtons.ForEach(btn =>
			{
				btn.SetParent(null);
				btn.Close();
			});
			dlgButtons.Clear();

			bool morePages = _pageIndex < _pages.Count - 1;
			bool firstPage = _pageIndex == 0;

			Vector2 firstLoc = new Vector2(89, 153), secondLoc = new Vector2(183, 153);

			if (firstPage && morePages)
			{
				//show cancel/next
				XNAButton cancel = new XNAButton(smallButtonSheet, firstLoc, _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
				cancel.OnClick += (o, e) => Close(cancel, XNADialogResult.Cancel);
				cancel.SetParent(this);
				dlgButtons.Add(cancel);

				XNAButton next = new XNAButton(smallButtonSheet, secondLoc, _getSmallButtonOut(SmallButton.Next), _getSmallButtonOver(SmallButton.Next));
				next.OnClick += (o, e) => _nextPage();
				next.SetParent(this);
				dlgButtons.Add(next);
			}
			else if (!firstPage && morePages)
			{
				//show back/next
				XNAButton back = new XNAButton(smallButtonSheet, firstLoc, _getSmallButtonOut(SmallButton.Back), _getSmallButtonOver(SmallButton.Back));
				back.OnClick += (o, e) => _prevPage();
				back.SetParent(this);
				dlgButtons.Add(back);

				XNAButton next = new XNAButton(smallButtonSheet, secondLoc, _getSmallButtonOut(SmallButton.Next), _getSmallButtonOver(SmallButton.Next));
				next.OnClick += (o, e) => _nextPage();
				next.SetParent(this);
				dlgButtons.Add(next);
			}
			else if (firstPage)
			{
				//show cancel/ok
				XNAButton cancel = new XNAButton(smallButtonSheet, firstLoc, _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
				cancel.OnClick += (o, e) => Close(cancel, XNADialogResult.Cancel);
				cancel.SetParent(this);
				dlgButtons.Add(cancel);

				XNAButton ok = new XNAButton(smallButtonSheet, secondLoc, _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
				ok.OnClick += (o, e) => Close(ok, XNADialogResult.OK);
				ok.SetParent(this);
				dlgButtons.Add(ok);
			}
			else
			{
				//show back/ok
				XNAButton back = new XNAButton(smallButtonSheet, firstLoc, _getSmallButtonOut(SmallButton.Back), _getSmallButtonOver(SmallButton.Back));
				back.OnClick += (o, e) => _prevPage();
				back.SetParent(this);
				dlgButtons.Add(back);

				XNAButton ok = new XNAButton(smallButtonSheet, secondLoc, _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
				ok.OnClick += (o, e) => Close(ok, XNADialogResult.OK);
				ok.SetParent(this);
				dlgButtons.Add(ok);
			}
		}

		private void _clickLink(byte linkID)
		{
			//send to server with linkID
			if (!m_api.RespondToQuestDialog(_stateInfo, DialogReply.Link, linkID))
			{
				Close(null, XNADialogResult.NO_BUTTON_PRESSED);
				((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
			}

			Close(null, XNADialogResult.Cancel);
		}

		private void _nextPage()
		{
			_pageIndex++;
			_setDialogText();
			_setDialogButtons();
		}

		private void _prevPage()
		{
			_pageIndex--;
			_setDialogText();
			_setDialogButtons();
		}
	}

	public class QuestProgressDialogListItem : EODialogListItem
	{
		public int QuestContextIcon { get; set; }

		public string QuestName
		{
			get { return Text; }
			set { Text = value; }
		}

		public string QuestStep
		{
			get { return SubText; }
			set { SubText = value; }
		}

		private readonly XNALabel m_progress;
		public string QuestProgress
		{
			get { return m_progress.Text; }
			set { m_progress.Text = value; }
		}

		public bool ShowIcons { private get; set; }

		private readonly Texture2D m_iconTexture;

		private static readonly Vector2 m_firstIconLocation = new Vector2(6, 0);
		private static readonly Vector2 m_secondIconLocation = new Vector2(151, 0);
		private static readonly Vector2 m_signalLocation = new Vector2(334, 0);

		private readonly bool _constructorFinished;

		public QuestProgressDialogListItem(EOScrollingListDialog parent, int index = -1)
			: base(parent, ListItemStyle.Small, index)
		{
			m_iconTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 68, true);
			ShowIcons = true;

			_setSize(427, 16);

			m_primaryText.DrawLocation = new Vector2(m_primaryText.DrawLocation.X + 25, m_primaryText.DrawLocation.Y);
			m_secondaryText = new XNALabel(new Rectangle(169, (int) m_primaryText.DrawLocation.Y, 1, 1), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = true,
				BackColor = m_primaryText.BackColor,
				ForeColor = m_primaryText.ForeColor,
				Text = " "
			};
			m_secondaryText.SetParent(this);
			m_progress = new XNALabel(new Rectangle(353, (int) m_primaryText.DrawLocation.Y, 1, 1), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = true,
				BackColor = m_primaryText.BackColor,
				ForeColor = m_primaryText.ForeColor,
				Text = " "
			};
			m_progress.SetParent(this);

			_constructorFinished = true;
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible || !_constructorFinished) return;


			if (ShowIcons)
			{
				SpriteBatch.Begin();

				SpriteBatch.Draw(m_iconTexture,
					m_firstIconLocation + new Vector2(17 + OffsetX + xOff, OffsetY + yOff + (DrawArea.Height*Index)),
					GetIconSourceRectangle(0), Microsoft.Xna.Framework.Color.White);
				SpriteBatch.Draw(m_iconTexture,
					m_secondIconLocation + new Vector2(17 + OffsetX + xOff, OffsetY + yOff + (DrawArea.Height*Index)),
					GetIconSourceRectangle(QuestContextIcon), Microsoft.Xna.Framework.Color.White);
				SpriteBatch.Draw(m_iconTexture,
					m_signalLocation + new Vector2(17 + OffsetX + xOff, OffsetY + yOff + (DrawArea.Height*Index)),
					GetSignalSourceRectangle(), Microsoft.Xna.Framework.Color.White);

				SpriteBatch.End();
			}

			base.Draw(gameTime);
		}

		private static Rectangle GetIconSourceRectangle(int index)
		{
			return new Rectangle(index * 15, 0, 15, 15);
		}

		private static Rectangle GetSignalSourceRectangle()
		{
			return new Rectangle(0, 15, 15, 15);
		}
	}

	public class QuestHistoryDialogListItem : EODialogListItem
	{
		public string QuestName
		{
			get { return Text; }
			set { Text = value; }
		}

		private readonly Texture2D m_iconTexture;

		private static readonly Vector2 m_firstIconLocation = new Vector2(6, 0);
		private static readonly Vector2 m_signalLocation = new Vector2(270, 0);

		private readonly bool _constructorFinished;

		public QuestHistoryDialogListItem(EOScrollingListDialog parent, int index = -1)
			: base(parent, ListItemStyle.Small, index)
		{
			m_iconTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 68, true);

			_setSize(427, 16);

			m_primaryText.DrawLocation = new Vector2(m_primaryText.DrawLocation.X + 25, m_primaryText.DrawLocation.Y);
			m_secondaryText = new XNALabel(new Rectangle(290, (int)m_primaryText.DrawLocation.Y, 1, 1), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = true,
				BackColor = m_primaryText.BackColor,
				ForeColor = m_primaryText.ForeColor,
				Text = World.GetString(DATCONST2.QUEST_COMPLETED)
			};
			m_secondaryText.SetParent(this);

			_constructorFinished = true;
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible || !_constructorFinished) return;

			SpriteBatch.Begin();

			SpriteBatch.Draw(m_iconTexture,
				m_firstIconLocation + new Vector2(17 + OffsetX + xOff, OffsetY + yOff + (DrawArea.Height*Index)),
				GetIconSourceRectangle(), Microsoft.Xna.Framework.Color.White);
			SpriteBatch.Draw(m_iconTexture,
				m_signalLocation + new Vector2(17 + OffsetX + xOff, OffsetY + yOff + (DrawArea.Height*Index)),
				GetSignalSourceRectangle(), Microsoft.Xna.Framework.Color.White);

			SpriteBatch.End();

			base.Draw(gameTime);
		}

		private static Rectangle GetIconSourceRectangle()
		{
			//always show 'completed' icon
			return new Rectangle(75, 0, 15, 15);
		}

		private static Rectangle GetSignalSourceRectangle()
		{
			return new Rectangle(0, 15, 15, 15);
		}
	}

	public class EOQuestProgressDialog : EOScrollingListDialog
	{
		public static EOQuestProgressDialog Instance { get; private set; }

		public static void Show(PacketAPI api)
		{
			if (Instance != null)
				Instance.Close(null, XNADialogResult.NO_BUTTON_PRESSED);

			Instance = new EOQuestProgressDialog(api);

			if (!api.RequestQuestHistory(QuestPage.Progress))
			{
				Instance.Close(null, XNADialogResult.NO_BUTTON_PRESSED);
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			}
		}

		//controls
		private readonly XNAButton m_history, m_progress;

		//state fields
		private bool _historyRequested;
		private short _numQuests;
		private List<InProgressQuestData> _questInfo;
		private List<string> _completedQuests;

		private EOQuestProgressDialog(PacketAPI api) 
			: base(api)
		{
			DialogClosing += (o, e) =>
			{
				Instance = null;
			};

			_setupBGTexture();

			m_history = new XNAButton(smallButtonSheet, new Vector2(288, 252), _getSmallButtonOut(SmallButton.History), _getSmallButtonOver(SmallButton.History));
			m_history.SetParent(this);
			m_history.OnClick += _historyClick;

			m_progress = new XNAButton(smallButtonSheet, new Vector2(288, 252), _getSmallButtonOut(SmallButton.Progress), _getSmallButtonOver(SmallButton.Progress))
			{
				Visible = false
			};
			m_progress.SetParent(this);
			m_progress.OnClick += _progressClick;

			var ok = new XNAButton(smallButtonSheet, new Vector2(380, 252), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
			ok.SetParent(this);
			ok.OnClick += (o, e) => Close(ok, XNADialogResult.OK);

			dlgButtons.AddRange(new[] {m_history, ok});

			m_titleText = new XNALabel(new Rectangle(18, 14, 452, 19), "Microsoft Sans Serif", 8.5f)
			{
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				ForeColor = Color.FromArgb(255, 0xc8, 0xc8, 0xc8),
				Text = " "
			};
			m_titleText.SetParent(this);

			m_scrollBar.DrawLocation = new Vector2(449, 44);
			SmallItemStyleMaxItemDisplay = 10;
			ListItemType = EODialogListItem.ListItemStyle.Small;
		}

		private void _setupBGTexture()
		{
			Texture2D wholeBgText = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 59);
			Texture2D bgText = new Texture2D(Game.GraphicsDevice, wholeBgText.Width, wholeBgText.Height / 2);
			Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[bgText.Width * bgText.Height];

			wholeBgText.GetData(0, new Rectangle(0, 0, bgText.Width, bgText.Height), data, 0, data.Length);
			bgText.SetData(data);

			_setBackgroundTexture(bgText);
		}

		public void SetInProgressDisplayData(short numQuests, List<InProgressQuestData> questInfo)
		{
			ClearItemList();
			_numQuests = numQuests;
			_questInfo = questInfo;
			_setTitleProgress();
			_setMessageProgress();
		}

		public void SetHistoryDisplayData(short numQuests, List<string> completedQuestNames)
		{
			ClearItemList();
			_numQuests = numQuests;
			_completedQuests = completedQuestNames;
			_setTitleHistory();
			_setMessageHistory();
		}

		private void _setTitleProgress()
		{
			m_titleText.Text = string.Format("{0}'s {1}", World.Instance.MainPlayer.ActiveCharacter.Name, World.GetString(DATCONST2.QUEST_PROGRESS));
		}

		private void _setTitleHistory()
		{
			m_titleText.Text = string.Format("{0}'s {1}", World.Instance.MainPlayer.ActiveCharacter.Name, World.GetString(DATCONST2.QUEST_HISTORY));
		}

		private void _setMessageProgress()
		{
			if (_questInfo.Count == 0)
			{
				AddItemToList(new QuestProgressDialogListItem(this, 0)
				{
					QuestName= World.GetString(DATCONST2.QUEST_DID_NOT_START_ANY),
					QuestStep = " ",
					ShowIcons = false,
					QuestProgress = " "
				}, false);

				return;
			}

			int ndx = 0;
			foreach (var quest in _questInfo)
			{
				var nextItem = new QuestProgressDialogListItem(this, ndx++)
				{
					QuestName = quest.Name,
					QuestStep = quest.Description,
					QuestContextIcon = quest.IconIndex,
					QuestProgress = quest.Target > 0 ? string.Format("{0} / {1}", quest.Progress, quest.Target) : "n / a"
				};
				AddItemToList(nextItem, false);
			}
		}

		private void _setMessageHistory()
		{
			if (_completedQuests.Count == 0)
			{
				AddItemToList(new QuestProgressDialogListItem(this, 0) 
				{
					 QuestName = World.GetString(DATCONST2.QUEST_DID_NOT_FINISH_ANY),
					 QuestStep = " ",
					 ShowIcons = false,
					 QuestProgress = " "
				}, false);

				return;
			}

			int ndx = 0;
			foreach (string quest in _completedQuests)
			{
				AddItemToList(new QuestHistoryDialogListItem(this, ndx++){QuestName = quest}, false);
			}
		}

		private void _historyClick(object sender, EventArgs e)
		{
			m_progress.Visible = true;
			m_history.Visible = false;

			dlgButtons.Remove(m_history);
			dlgButtons.Insert(0, m_progress);

			m_scrollBar.ScrollToTop();

			if (!_historyRequested)
			{
				if (!m_api.RequestQuestHistory(QuestPage.History))
				{
					Close(null, XNADialogResult.NO_BUTTON_PRESSED);
					((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
				}
				_historyRequested = true;
			}
			else
				SetHistoryDisplayData(_numQuests, _completedQuests);
		}

		private void _progressClick(object sender, EventArgs e)
		{
			m_progress.Visible = false;
			m_history.Visible = true;

			dlgButtons.Remove(m_progress);
			dlgButtons.Insert(0, m_history);

			m_scrollBar.ScrollToTop();

			SetInProgressDisplayData(_numQuests, _questInfo);
		}
	}
}
