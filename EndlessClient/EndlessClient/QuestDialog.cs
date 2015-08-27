
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using EOLib;
using EOLib.Net;
using Microsoft.Xna.Framework;
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
			if (Instance != null)
				return;

			SetupInstance(api);

			NPCName = name;

			if (!api.TalkToQuestNPC(npcIndex, questID))
				EOGame.Instance.LostConnectionDialog();
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

		//click npc - quest_use
		//click link in dialog - quest_accept

		private EOQuestDialog(PacketAPI api)
			: base(api)
		{
			DialogClosing += (o, e) =>
			{
				if (e.Result == XNADialogResult.OK)
				{
					if (!m_api.RespondToQuestDialog(_stateInfo, DialogReply.Ok))
						((EOGame) Game).LostConnectionDialog();
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
				((EOGame)Game).LostConnectionDialog();
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

	public class EOQuestProgressDialog : EODialogBase
	{

	}
}
