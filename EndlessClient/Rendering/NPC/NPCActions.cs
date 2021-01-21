using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EOLib;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.NPC
{
    [MappedType(BaseType = typeof(INPCActionNotifier))]
    public class NPCActions : INPCActionNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly INPCStateCache _npcStateCache;
        private readonly INPCRendererRepository _npcRendererRepository;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly IChatBubbleRepository _chatBubbleRepository;
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;
        private readonly IESFFileProvider _esfFileProvider;

        public NPCActions(IHudControlProvider hudControlProvider,
                          INPCStateCache npcStateCache,
                          INPCRendererRepository npcRendererRepository,
                          ICharacterRendererRepository characterRendererRepository,
                          IChatBubbleRepository chatBubbleRepository,
                          IChatBubbleTextureProvider chatBubbleTextureProvider,
                          IESFFileProvider esfFileProvider)
        {
            _hudControlProvider = hudControlProvider;
            _npcStateCache = npcStateCache;
            _npcRendererRepository = npcRendererRepository;
            _characterRendererRepository = characterRendererRepository;
            _chatBubbleRepository = chatBubbleRepository;
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

        public void RemoveNPCFromView(int npcIndex, int playerId, Optional<short> spellId, Optional<int> damage, bool showDeathAnimation)
        {
            //possible that the server might send a packet for the npc to be removed by the map switch is completed
            if (!_hudControlProvider.IsInGame || !_npcRendererRepository.NPCRenderers.ContainsKey(npcIndex))
                return;

            _npcStateCache.RemoveStateByIndex(npcIndex);

            // todo: show damage counter

            if (!showDeathAnimation)
            {
                _npcRendererRepository.NPCRenderers[npcIndex].Dispose();
                _npcRendererRepository.NPCRenderers.Remove(npcIndex);
            }
            else
            {
                _npcRendererRepository.NPCRenderers[npcIndex].StartDying();

                if (spellId.HasValue)
                {
                    var graphic = _esfFileProvider.ESFFile[spellId.Value].Graphic;
                    _npcRendererRepository.NPCRenderers[npcIndex].ShowSpellAnimation(graphic);
                    ShoutSpellCast(playerId);
                }
            }
        }

        public void ShowNPCSpeechBubble(int npcIndex, string message)
        {
            IChatBubble chatBubble;
            if (_chatBubbleRepository.NPCChatBubbles.TryGetValue(npcIndex, out chatBubble))
                chatBubble.SetMessage(isGroupChat: false, message: message);
            else if (_npcRendererRepository.NPCRenderers.ContainsKey(npcIndex))
            {
                chatBubble = new ChatBubble(message,
                                            _npcRendererRepository.NPCRenderers[npcIndex],
                                            _chatBubbleTextureProvider);

                _chatBubbleRepository.NPCChatBubbles.Add(npcIndex, chatBubble);
            }
        }

        public void NPCTakeDamage(short npcIndex, int fromPlayerId, int damageToNpc, short npcPctHealth, Optional<int> spellId)
        {
            // todo: show damage counter

            if (spellId.HasValue)
            {
                var renderer = _npcRendererRepository.NPCRenderers[npcIndex];

                var graphic = _esfFileProvider.ESFFile[spellId.Value].Graphic;
                renderer.ShowSpellAnimation(graphic);
                ShoutSpellCast(fromPlayerId);
            }
        }

        private void ShoutSpellCast(int playerId)
        {
            if (_characterRendererRepository.MainCharacterRenderer.Character.ID == playerId)
                _characterRendererRepository.MainCharacterRenderer.ShoutSpellCast();
            else
                _characterRendererRepository.CharacterRenderers[playerId].ShoutSpellCast();
        }

        private INPCAnimator Animator => _hudControlProvider.GetComponent<INPCAnimator>(HudControlIdentifier.NPCAnimator);
    }
}
