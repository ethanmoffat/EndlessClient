using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Chat;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EOLib;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;
using Optional;

namespace EndlessClient.Rendering.NPC
{
    [MappedType(BaseType = typeof(INPCActionNotifier))]
    public class NPCActions : INPCActionNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly INPCStateCache _npcStateCache;
        private readonly INPCRendererRepository _npcRendererRepository;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly IChatBubbleActions _chatBubbleActions;
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;
        private readonly IESFFileProvider _esfFileProvider;

        public NPCActions(IHudControlProvider hudControlProvider,
                          INPCStateCache npcStateCache,
                          INPCRendererRepository npcRendererRepository,
                          ICharacterRendererRepository characterRendererRepository,
                          IChatBubbleActions chatBubbleActions,
                          IChatBubbleTextureProvider chatBubbleTextureProvider,
                          IESFFileProvider esfFileProvider)
        {
            _hudControlProvider = hudControlProvider;
            _npcStateCache = npcStateCache;
            _npcRendererRepository = npcRendererRepository;
            _characterRendererRepository = characterRendererRepository;
            _chatBubbleActions = chatBubbleActions;
            _chatBubbleTextureProvider = chatBubbleTextureProvider;
            _esfFileProvider = esfFileProvider;
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
        }

        public void RemoveNPCFromView(int npcIndex, int playerId, Option<short> spellId, Option<int> damage, bool showDeathAnimation)
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
                    _npcRendererRepository.NPCRenderers[npcIndex].ShowSpellAnimation(graphic);
                    ShoutSpellCast(playerId);
                });

                damage.MatchSome(d => _npcRendererRepository.NPCRenderers[npcIndex].ShowDamageCounter(d, 0, isHeal: false));
            }
        }

        public void ShowNPCSpeechBubble(int npcIndex, string message)
        {
            _chatBubbleActions.ShowChatBubbleForNPC(npcIndex, message);
        }

        public void NPCTakeDamage(short npcIndex, int fromPlayerId, int damageToNpc, short npcPctHealth, Option<int> spellId)
        {
            _npcRendererRepository.NPCRenderers[npcIndex].ShowDamageCounter(damageToNpc, npcPctHealth, isHeal: false);

            spellId.MatchSome(spell =>
            {
                var renderer = _npcRendererRepository.NPCRenderers[npcIndex];

                var graphic = _esfFileProvider.ESFFile[spell].Graphic;
                renderer.ShowSpellAnimation(graphic);
                ShoutSpellCast(fromPlayerId);
            });
        }

        private void ShoutSpellCast(int playerId)
        {
            _characterRendererRepository.MainCharacterRenderer.Match(
                some: r =>
                {
                    if (r.Character.ID == playerId)
                        r.ShoutSpellCast();
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
