using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Chat;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Sprites;
using EndlessClient.UIControls;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class ChatPanel : XNAPanel, IHudPanel
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IChatActions _chatActions;
        private readonly IChatRenderableGenerator _chatRenderableGenerator;
        private readonly IChatProvider _chatProvider;
        private readonly IHudControlProvider _hudControlProvider;

        private readonly ScrollBar _scrollBar;
        private readonly Dictionary<ChatTab, ChatPanelTab> _tabs;

        public ChatTab CurrentTab => _tabs.Single(x => x.Value.Active).Key;

        public ChatPanel(INativeGraphicsManager nativeGraphicsManager,
                         IChatActions chatActions,
                         IChatRenderableGenerator chatRenderableGenerator,
                         IChatProvider chatProvider,
                         IHudControlProvider hudControlProvider,
                         BitmapFont chatFont)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _chatActions = chatActions;
            _chatRenderableGenerator = chatRenderableGenerator;
            _chatProvider = chatProvider;
            _hudControlProvider = hudControlProvider;

            //abs coordiantes: 568 309
            _scrollBar = new ScrollBar(new Vector2(467, 2), new Vector2(16, 97), ScrollBarColors.LightOnMed, _nativeGraphicsManager)
            {
                LinesToRender = 7,
                Visible = true
            };
            _scrollBar.SetParentControl(this);
            SetScrollWheelHandler(_scrollBar);

            var tabTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 35);
            var smallSelected = new SpriteSheet(tabTexture, new Rectangle(307, 16, 43, 16));
            var smallUnselected = new SpriteSheet(tabTexture, new Rectangle(264, 16, 43, 16));
            var largeSelected = new SpriteSheet(tabTexture, new Rectangle(132, 16, 132, 16));
            var largeUnselected = new SpriteSheet(tabTexture, new Rectangle(0, 16, 132, 16));

            BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 28);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);

            _tabs = new Dictionary<ChatTab, ChatPanelTab>
            {
                {ChatTab.Local, new ChatPanelTab(_chatProvider, _hudControlProvider, _chatRenderableGenerator, this, _scrollBar, ChatTab.Local, smallSelected, smallUnselected, chatFont) { Visible = true, Active = true } },
                {ChatTab.Global, new ChatPanelTab(_chatProvider, _hudControlProvider, _chatRenderableGenerator, this, _scrollBar, ChatTab.Global, smallSelected, smallUnselected, chatFont) { Visible = true, } },
                {ChatTab.Group, new ChatPanelTab(_chatProvider, _hudControlProvider, _chatRenderableGenerator, this, _scrollBar, ChatTab.Group, smallSelected, smallUnselected, chatFont) { Visible = true, } },
                {ChatTab.System, new ChatPanelTab(_chatProvider, _hudControlProvider, _chatRenderableGenerator, this, _scrollBar, ChatTab.System, smallSelected, smallUnselected, chatFont) { Visible = true, } },
                {ChatTab.Private1, new ChatPanelTab(_chatProvider, _hudControlProvider, _chatRenderableGenerator, this, _scrollBar, ChatTab.Private1, largeSelected, largeUnselected, chatFont) {Visible = false} },
                {ChatTab.Private2, new ChatPanelTab(_chatProvider, _hudControlProvider, _chatRenderableGenerator, this, _scrollBar, ChatTab.Private2, largeSelected, largeUnselected, chatFont) {Visible = false } },
            };

            foreach (var tab in _tabs.Values)
            {
                tab.SetParentControl(this);
                tab.OnClosed += (s, _) => _chatActions.ClosePMTab(((ChatPanelTab)s).Tab);
            }
        }

        public override void Initialize()
        {
            _scrollBar.Initialize();

            foreach (var tab in _tabs.Values)
                tab.Initialize();

            base.Initialize();
        }

        public void TryStartNewPrivateChat(string targetCharacter)
        {
            if (_tabs[ChatTab.Private1].Visible && _tabs[ChatTab.Private2].Visible)
                return;

            if (!string.Equals(_chatProvider.PMTarget1, targetCharacter, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(_chatProvider.PMTarget2, targetCharacter, StringComparison.OrdinalIgnoreCase))
            {
                if (_tabs[ChatTab.Private1].Visible)
                {
                    SelectTab(ChatTab.Private2);
                    _tabs[ChatTab.Private2].Text = char.ToUpper(targetCharacter[0]) + targetCharacter[1..];
                }
                else
                {
                    SelectTab(ChatTab.Private1);
                    _tabs[ChatTab.Private1].Text = char.ToUpper(targetCharacter[0]) + targetCharacter[1..];
                }
            }
        }

        public void SelectTab(ChatTab clickedTab)
        {
            if (CurrentTab == ChatTab.Global && clickedTab != ChatTab.Global)
            {
                _chatActions.SetGlobalActive(false);
            }
            else if (CurrentTab != ChatTab.Global && clickedTab == ChatTab.Global)
            {
                _chatActions.SetGlobalActive(true);
            }

            _tabs[CurrentTab].Active = false;

            _tabs[clickedTab].Visible = true;
            _tabs[clickedTab].Active = true;

            _scrollBar.UpdateDimensions(_chatProvider.AllChat[clickedTab].Count);
        }

        public void ClosePMTab(ChatTab whichTab)
        {
            _tabs[whichTab].CloseTab();
        }
    }
}