// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Controls;
using EndlessClient.HUD;
using EOLib;
using EOLib.Graphics;
using EOLib.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class QuestDialog : ScrollingListDialog
	{
		public static QuestDialog Instance { get; private set; }

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

			Instance = new QuestDialog(api);
		}

		private QuestState _stateInfo;
		private Dictionary<short, string> _dialogNames, _links;
		private List<string> _pages;
		private short _pageIndex;

		private static string NPCName { get; set; }

		private QuestDialog(PacketAPI api)
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

			_setBackgroundTexture(((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 67));

			caption = new XNALabel(new Rectangle(16, 16, 255, 18), Constants.FontSize08pt5)
			{
				AutoSize = false,
				TextAlign = LabelAlignment.MiddleLeft,
				ForeColor = Constants.LightGrayText
			};
			caption.SetParent(this);

			m_scrollBar.SetParent(null);
			m_scrollBar.Close();

			m_scrollBar = new ScrollBar(this, new Vector2(252, 44), new Vector2(16, 99), ScrollBarColors.LightOnMed);
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

			TextSplitter ts = new TextSplitter(_pages[_pageIndex], Game.Content.Load<SpriteFont>(Constants.FontSize08pt5));
			if (!ts.NeedsProcessing)
				rows.Add(_pages[_pageIndex]);
			else
				rows.AddRange(ts.SplitIntoLines());

			int index = 0;
			foreach (var row in rows)
			{
				ListDialogItem rowItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, index++)
				{
					Text = row
				};
				AddItemToList(rowItem, false);
			}

			if (_pageIndex < _pages.Count - 1)
				return;

			ListDialogItem item = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, index++) { Text = " " };
			AddItemToList(item, false);

			foreach (var link in _links)
			{
				ListDialogItem linkItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, index++)
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
}
