using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Chat;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Optional;

namespace EndlessClient.Rendering.NPC
{
    [AutoMappedType]
    public class NPCActions : INPCActionNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly INPCStateCache _npcStateCache;
        private readonly INPCRendererRepository _npcRendererRepository;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly IChatBubbleActions _chatBubbleActions;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IESFFileProvider _esfFileProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public NPCActions(IHudControlProvider hudControlProvider,
                          INPCStateCache npcStateCache,
                          INPCRendererRepository npcRendererRepository,
                          ICharacterRendererRepository characterRendererRepository,
                          IChatBubbleActions chatBubbleActions,
                          IChatRepository chatRepository,
                          ILocalizedStringFinder localizedStringFinder,
                          IEIFFileProvider eifFileProvider,
                          IESFFileProvider esfFileProvider,
                          ISfxPlayer sfxPlayer)
        {
            _hudControlProvider = hudControlProvider;
            _npcStateCache = npcStateCache;
            _npcRendererRepository = npcRendererRepository;
            _characterRendererRepository = characterRendererRepository;
            _chatBubbleActions = chatBubbleActions;
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
            _eifFileProvider = eifFileProvider;
            _esfFileProvider = esfFileProvider;
            _sfxPlayer = sfxPlayer;
        }

        public void StartNPCWalkAnimation(int npcIndex)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartWalkAnimation(npcIndex);
        }

        public void StartNPCAttackAnimation(int npcIndex)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartAttackAnimation(npcIndex);

            _sfxPlayer.PlaySfx(SoundEffectID.PunchAttack);
        }

        public void RemoveNPCFromView(int npcIndex, int playerId, Option<int> spellId, Option<int> damage, bool showDeathAnimation)
        {
            //possible that the server might send a packet for the npc to be removed by the map switch is completed
            if (!_hudControlProvider.IsInGame || !_npcRendererRepository.NPCRenderers.ContainsKey(npcIndex))
                return;

            _npcStateCache.RemoveStateByIndex(npcIndex);

            if (!showDeathAnimation)
            {
                _npcRendererRepository.NPCRenderers[npcIndex].Dispose();
                _npcRendererRepository.NPCRenderers.Remove(npcIndex);
            }
            else
            {
                _npcRendererRepository.NPCRenderers[npcIndex].StartDying();

                spellId.MatchSome(spell =>
                {
                    var graphic = _esfFileProvider.ESFFile[spell].Graphic;
                    _npcRendererRepository.NPCRenderers[npcIndex].PlayEffect(graphic);
                    ShoutSpellCast(playerId);
                });

                damage.MatchSome(d => _npcRendererRepository.NPCRenderers[npcIndex].ShowDamageCounter(d, 0, isHeal: false));
            }
        }

        public void ShowNPCSpeechBubble(int npcIndex, string message)
        {
            _chatBubbleActions.ShowChatBubbleForNPC(npcIndex, message);
        }

        public void NPCTakeDamage(int npcIndex, int fromPlayerId, int damageToNpc, int npcPctHealth, Option<int> spellId)
        {
            if (_npcRendererRepository.NPCRenderers.ContainsKey(npcIndex))
                _npcRendererRepository.NPCRenderers[npcIndex].ShowDamageCounter(damageToNpc, npcPctHealth, isHeal: false);

            spellId.MatchSome(spell =>
            {
                var renderer = _npcRendererRepository.NPCRenderers[npcIndex];

                var graphic = _esfFileProvider.ESFFile[spell].Graphic;
                renderer.PlayEffect(graphic);
                ShoutSpellCast(fromPlayerId);
            });
        }

        public void NPCDropItem(MapItem item)
        {
            // todo: not sure if it is better to do this here in a notifier or modify the chat repository in the packet handler
            //         however, I don't want to introduce a dependency on localized text in the packet handler
            var itemName = _eifFileProvider.EIFFile[item.ItemID].Name;
            var chatData = new ChatData(ChatTab.System,
                string.Empty,
                $"{_localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_THE_NPC_DROPPED)} {item.Amount} {itemName}",
                ChatIcon.DownArrow);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);
        }

        private void ShoutSpellCast(int playerId)
        {
            _characterRendererRepository.MainCharacterRenderer.Match(
                some: r =>
                {
                    if (r.Character.ID == playerId)
                        r.ShoutSpellCast();
                    else if (_characterRendererRepository.CharacterRenderers.ContainsKey(playerId))
                        _characterRendererRepository.CharacterRenderers[playerId].ShoutSpellCast();
                },
                none: () =>
                {
                    if (_characterRendererRepository.CharacterRenderers.ContainsKey(playerId))
                        _characterRendererRepository.CharacterRenderers[playerId].ShoutSpellCast();
                });
        }

        private INPCAnimator Animator => _hudControlProvider.GetComponent<INPCAnimator>(HudControlIdentifier.NPCAnimator);
    }
}
