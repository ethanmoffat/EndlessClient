// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.Old;
using EOLib;
using EOLib.Graphics;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class QuestProgressDialog : ScrollingListDialog
    {
        public static QuestProgressDialog Instance { get; private set; }

        public static void Show(PacketAPI api)
        {
            if (Instance != null)
                Instance.Close(null, XNADialogResult.NO_BUTTON_PRESSED);

            Instance = new QuestProgressDialog(api);

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

        private QuestProgressDialog(PacketAPI api)
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

            dlgButtons.AddRange(new[] { m_history, ok });

            m_titleText = new XNALabel(new Rectangle(18, 14, 452, 19), Constants.FontSize08pt5)
            {
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText,
                Text = " "
            };
            m_titleText.SetParent(this);

            m_scrollBar.DrawLocation = new Vector2(449, 44);
            SmallItemStyleMaxItemDisplay = 10;
            ListItemType = ListDialogItem.ListItemStyle.Small;
        }

        private void _setupBGTexture()
        {
            Texture2D wholeBgText = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 59);
            Texture2D bgText = new Texture2D(Game.GraphicsDevice, wholeBgText.Width, wholeBgText.Height / 2);
            Color[] data = new Color[bgText.Width * bgText.Height];

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
            m_titleText.Text = string.Format("{0}'s {1}", OldWorld.Instance.MainPlayer.ActiveCharacter.Name, OldWorld.GetString(EOResourceID.QUEST_PROGRESS));
        }

        private void _setTitleHistory()
        {
            m_titleText.Text = string.Format("{0}'s {1}", OldWorld.Instance.MainPlayer.ActiveCharacter.Name, OldWorld.GetString(EOResourceID.QUEST_HISTORY));
        }

        private void _setMessageProgress()
        {
            if (_questInfo.Count == 0)
            {
                AddItemToList(new QuestProgressDialogListItem(this, 0)
                {
                    QuestName = OldWorld.GetString(EOResourceID.QUEST_DID_NOT_START_ANY),
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
                    QuestName = OldWorld.GetString(EOResourceID.QUEST_DID_NOT_FINISH_ANY),
                    QuestStep = " ",
                    ShowIcons = false,
                    QuestProgress = " "
                }, false);

                return;
            }

            int ndx = 0;
            foreach (string quest in _completedQuests)
            {
                AddItemToList(new QuestHistoryDialogListItem(this, ndx++) { QuestName = quest }, false);
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
                    ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
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
