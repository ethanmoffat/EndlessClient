using System.Collections.Generic;
using System.Linq;
using EndlessClient.Content;
using EndlessClient.HUD.Party;
using EndlessClient.Rendering;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Party;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Optional.Collections;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class PartyPanel : DraggableHudPanel
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IPartyActions _partyActions;
        private readonly IContentProvider _contentProvider;
        private readonly IPartyDataProvider _partyDataProvider;
        private readonly ICharacterProvider _characterProvider;

        private readonly ScrollBar _scrollBar;
        private readonly IXNALabel _numMembers;
        private readonly List<PartyPanelMember> _children;

        private HashSet<PartyMember> _cachedParty;
        private int _cachedScrollOffset;

        public PartyPanel(INativeGraphicsManager nativeGraphicsManager,
                          IPartyActions partyActions,
                          IContentProvider contentProvider,
                          IPartyDataProvider partyDataProvider,
                          ICharacterProvider characterProvider,
                          IClientWindowSizeProvider clientWindowSizeProvider)
            : base(clientWindowSizeProvider.Resizable)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _partyActions = partyActions;
            _contentProvider = contentProvider;
            _partyDataProvider = partyDataProvider;
            _characterProvider = characterProvider;
            _scrollBar = new ScrollBar(new Vector2(467, 20), new Vector2(16, 97), ScrollBarColors.LightOnMed, _nativeGraphicsManager)
            {
                LinesToRender = 7,
                Visible = true,
            };
            _scrollBar.SetParentControl(this);
            _scrollBar.UpdateDimensions(0);
            SetScrollWheelHandler(_scrollBar);

            _numMembers = new XNALabel(Constants.FontSize09)
            {
                AutoSize = false,
                DrawArea = new Rectangle(455, 0, 27, 14),
                ForeColor = ColorConstants.LightGrayText,
                TextAlign = LabelAlignment.MiddleRight
            };
            _numMembers.SetParentControl(this);

            _children = new List<PartyPanelMember>();

            _cachedParty = new HashSet<PartyMember>();

            BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 42);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);
        }

        public override void Initialize()
        {
            _scrollBar.Initialize();
            _numMembers.Initialize();

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (!_cachedParty.SetEquals(_partyDataProvider.Members))
            {
                var added = _partyDataProvider.Members.Where(x => !_cachedParty.Any(y => y.CharacterID == x.CharacterID)).ToList();
                var removed = _cachedParty.Where(x => !_partyDataProvider.Members.Any(y => y.CharacterID == x.CharacterID)).ToList();
                var updated = _partyDataProvider.Members.Where(x => _cachedParty.Any(y => y.CharacterID == x.CharacterID && !y.Equals(x))).ToList();
                _cachedParty = _partyDataProvider.Members.ToHashSet();

                var mainCharacterIsLeader = _cachedParty.Any(x => x.IsLeader && x.CharacterID == _characterProvider.MainCharacter.ID);

                foreach (var member in added)
                {
                    var next = new PartyPanelMember(
                        _nativeGraphicsManager,
                        _contentProvider,
                        mainCharacterIsLeader || member.CharacterID == _characterProvider.MainCharacter.ID)
                    {
                        PartyMember = member,
                    };
                    next.RemoveAction += (_, _) => _partyActions.RemovePartyMember(member.CharacterID);
                    next.SetParentControl(this);
                    next.Initialize();

                    _children.Add(next);
                }

                foreach (var member in removed)
                {
                    _children.SingleOrNone(x => x.PartyMember.CharacterID == member.CharacterID)
                        .MatchSome(x =>
                        {
                            x.SetControlUnparented();
                            x.Dispose();
                            _children.Remove(x);
                        });
                }

                foreach (var member in updated)
                {
                    _children.SingleOrNone(x => x.PartyMember.CharacterID == member.CharacterID)
                        .MatchSome(x => x.PartyMember = member);
                }

                _numMembers.Text = $"{_cachedParty.Count}";
                _scrollBar.UpdateDimensions(_cachedParty.Count);

                UpdateChildMemberVisibility();
            }

            if (_cachedScrollOffset != _scrollBar.ScrollOffset)
            {
                _cachedScrollOffset = _scrollBar.ScrollOffset;
                UpdateChildMemberVisibility();
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnVisibleChanged(object sender, System.EventArgs args)
        {
            if (Visible)
            {
                // request party list when viewing the party panel
                _partyActions.ListParty();
            }

            base.OnVisibleChanged(sender, args);
        }

        private void UpdateChildMemberVisibility()
        {
            foreach (var child in _children)
                child.Visible = false;

            for (int i = 0; i < _scrollBar.LinesToRender; i++)
            {
                if (_scrollBar.ScrollOffset + i >= _children.Count)
                    break;
                _children[_scrollBar.ScrollOffset + i].DisplayIndex = i;
                _children[_scrollBar.ScrollOffset + i].Visible = true;
            }
        }
    }
}