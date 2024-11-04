using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Sprites;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Chat;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using XNAControls;

namespace EndlessClient.HUD.Chat
{
    public class ChatPanelTab : XNAControl
    {
        private readonly IChatProvider _chatProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IChatRenderableGenerator _chatRenderableGenerator;

        private readonly ChatPanel _parentPanel;
        private readonly ScrollBar _scrollBar;
        private readonly ISpriteSheet _tabSheetSelected, _tabSheetUnselected;
        private readonly BitmapFont _chatFont;

        private readonly IXNAButton _closeButton, _tab;
        private readonly IXNALabel _label;

        private bool _active;
        private int _cachedScrollOffset;
        private HashSet<ChatData> _cachedChat;
        private List<IChatRenderable> _renderables;
        private bool _closeButtonClicked;

        public ChatTab Tab { get; }

        public string Text { get => _label.Text; set => _label.Text = value; }

        public bool Active
        {
            get => _active;
            set
            {
                if (_active = value)
                {
                    _scrollBar.SetScrollOffset(_cachedScrollOffset);
                    _label.ForeColor = Color.White;
                }
                else
                {
                    _cachedScrollOffset = _scrollBar.ScrollOffset;
                    _label.ForeColor = Color.Black;
                }
            }
        }

        public event EventHandler OnClosed;

        public ChatPanelTab(IChatProvider chatProvider,
                            IHudControlProvider hudControlProvider,
                            IChatRenderableGenerator chatRenderableGenerator,
                            ChatPanel parentPanel,
                            ScrollBar scrollBar,
                            ChatTab whichTab,
                            ISpriteSheet tabSheetSelected,
                            ISpriteSheet tabSheetUnselected,
                            BitmapFont chatFont)
        {
            _chatProvider = chatProvider;
            _hudControlProvider = hudControlProvider;
            _chatRenderableGenerator = chatRenderableGenerator;

            _parentPanel = parentPanel;
            _scrollBar = scrollBar;
            Tab = whichTab;
            _tabSheetSelected = tabSheetSelected;
            _tabSheetUnselected = tabSheetUnselected;
            _chatFont = chatFont;

            DrawArea = GetTabClickableArea();

            _tab = new ClickableArea(new Rectangle(0, 0, DrawArea.Width, DrawArea.Height));
            _tab.OnMouseDown += (_, _) => SelectThisTab();
            _tab.SetParentControl(this);

            if (Tab == ChatTab.Private1)
            {
                _closeButton = new ClickableArea(new Rectangle(3, 3, 11, 11));
                _closeButton.OnMouseDown += (_, _) => CloseTab();
                _closeButton.SetParentControl(this);
            }
            else if (Tab == ChatTab.Private2)
            {
                _closeButton = new ClickableArea(new Rectangle(3, 3, 11, 11));
                _closeButton.OnMouseDown += (_, _) => CloseTab();
                _closeButton.SetParentControl(this);
            }

            _label = new XNALabel(Constants.FontSize08)
            {
                Text = GetTabTextLabel(),
                DrawPosition = new Vector2(16, 2)
            };
            _label.SetParentControl(this);

            _cachedChat = new HashSet<ChatData>();
            _renderables = new List<IChatRenderable>();
        }

        public override void Initialize()
        {
            _tab.Initialize();
            _closeButton?.Initialize();
            _label.Initialize();

            base.Initialize();
        }

        public void HandleRightClick(MouseEventArgs eventArgs)
        {
            var clickedYRelativeToTopOfPanel = eventArgs.Position.Y - _parentPanel.DrawAreaWithParentOffset.Y;
            var clickedChatRow = (int)Math.Round(clickedYRelativeToTopOfPanel / 13.0) - 1;

            if (clickedChatRow >= 0 && _scrollBar.ScrollOffset + clickedChatRow < _cachedChat.Count)
            {
                var who = _chatProvider.AllChat[Tab][_scrollBar.ScrollOffset + clickedChatRow].Who;
                if (!string.IsNullOrEmpty(who))
                {
                    _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox).Text = $"!{who} ";
                }
            }
        }

        public void CloseTab()
        {
            if (Tab != ChatTab.Private1 && Tab != ChatTab.Private2)
                throw new InvalidOperationException("Unable to close chat tab that isn't a PM tab");

            _closeButtonClicked = true;

            _parentPanel.SelectTab(ChatTab.Local);

            Visible = false;

            _cachedChat.Clear();
            _label.Text = string.Empty;
            _cachedScrollOffset = 0;

            OnClosed?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnUnconditionalUpdateControl(GameTime gameTime)
        {
            if (!Visible)
                return;

            if (!_cachedChat.SetEquals(_chatProvider.AllChat[Tab]))
            {
                _cachedChat = _chatProvider.AllChat[Tab].ToHashSet();
                _renderables = _chatRenderableGenerator.GenerateChatRenderables(_cachedChat).ToList();

                if (Active)
                {
                    _scrollBar.UpdateDimensions(_renderables.Count);
                    _scrollBar.ScrollToEnd();
                }
                else
                {
                    _cachedScrollOffset = Math.Max(0, _renderables.Count - 7);
                    _label.ForeColor = Color.White;
                }
            }

            base.OnUnconditionalUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();

            var sheet = Active ? _tabSheetSelected : _tabSheetUnselected;
            _spriteBatch.Draw(sheet.SheetTexture, DrawAreaWithParentOffset, sheet.SourceRectangle, Color.White);

            if (Active)
            {
                _spriteBatch.End();

                foreach (var (ndx, renderable) in _renderables.Skip(_scrollBar.ScrollOffset).Take(_scrollBar.LinesToRender).Select((r, i) => (i, r)))
                {
                    renderable.DisplayIndex = ndx;
                    renderable.Render(_parentPanel, _spriteBatch, _chatFont);
                }

                _spriteBatch.Begin();
            }

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        private void SelectThisTab()
        {
            if (!_closeButtonClicked)
                _parentPanel.SelectTab(Tab);
            _closeButtonClicked = false;
        }

        private Rectangle GetTabClickableArea()
        {
            return Tab switch
            {
                ChatTab.Private1 => new Rectangle(23, 102, 132, 16),
                ChatTab.Private2 => new Rectangle(156, 102, 132, 16),
                ChatTab.Local or
                ChatTab.Global or
                ChatTab.Group or
                ChatTab.System => new Rectangle(289 + 44 * ((int)Tab - 2), 102, 43, 16),
                _ => throw new ArgumentOutOfRangeException(nameof(Tab), Tab, null),
            };
        }

        private string GetTabTextLabel()
        {
            return Tab switch
            {
                ChatTab.Private1 or
                ChatTab.Private2 => string.Empty,
                ChatTab.Local => "scr",
                ChatTab.Global => "glb",
                ChatTab.Group => "grp",
                ChatTab.System => "sys",
                _ => throw new ArgumentOutOfRangeException(nameof(Tab), Tab, null),
            };
        }
    }
}
