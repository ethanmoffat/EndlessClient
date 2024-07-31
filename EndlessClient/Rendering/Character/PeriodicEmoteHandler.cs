using EndlessClient.Audio;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Chat;
using EndlessClient.Input;
using EOLib.Domain.Character;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Optional;
using System;
using System.Diagnostics;

namespace EndlessClient.Rendering.Character
{
    public class PeriodicEmoteHandler : GameComponent, IPeriodicEmoteHandler
    {
        // Time before periodic AFK emotes start firing (5 minutes)
        private const int AFK_TIME_MS = 300000;
        // Time between each emote once period emotes start firing (30 seconds)
        private const int AFK_TIME_BETWEEN_EMOTES_MS = 30000;

        // Time before periodic alert messages start firing (30 minutes)
        private const int AFK_TIME_ALERT_MS = 1800000;
        // Time between each periodic alert message fires (60 seconds)
        private const int AFK_TIME_BETWEEN_ALERTS_MS = 60000;
        // Time between first alert and alternate alert (3.6 seconds)
        private const int AFK_TIME_ALT_ALERT_MS = 3600;

        // Time from arena launch until the block messages start showing
        private const int ARENA_BLOCK_INITIAL_TIME_MS = 6000;
        // Time between subsequent messages
        private const int ARENA_BLOCK_WARNING_INTERVAL_MS = 1200;
        // Intervals before disconnect
        private const int ARENA_BLOCK_MAX_WARNINGS = 6;

        private readonly ICharacterActions _characterActions;
        private readonly IChatBubbleActions _chatBubbleActions;
        private readonly IUserInputTimeProvider _userInputTimeProvider;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterAnimator _animator;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IMainButtonController _mainButtonController;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ISfxPlayer _sfxPlayer;

        private readonly Random _random;

        // drunk stuff
        private Option<DateTime> _drunkStart;
        private Option<Stopwatch> _drunkTimeSinceLastEmote;
        private int _drunkIntervalSeconds;
        private double _drunkTimeoutSeconds;

        // afk stuff
        private Option<Stopwatch> _afkTimeSinceLastEmote;
        private Option<Stopwatch> _afkTimeSinceLastAlert;
        private bool _altAlert;

        // arena block stuff
        private Option<Stopwatch> _arenaTimer;
        private int _arenaWarningCounter;

        public PeriodicEmoteHandler(IEndlessGameProvider endlessGameProvider,
                                    ICharacterActions characterActions,
                                    IChatBubbleActions chatBubbleActions,
                                    IUserInputTimeProvider userInputTimeProvider,
                                    ICharacterRepository characterRepository,
                                    ICharacterAnimator animator,
                                    IStatusLabelSetter statusLabelSetter,
                                    IMainButtonController mainButtonController,
                                    ILocalizedStringFinder localizedStringFinder,
                                    ISfxPlayer sfxPlayer)
            : base((Game)endlessGameProvider.Game)
        {
            _characterActions = characterActions;
            _chatBubbleActions = chatBubbleActions;
            _userInputTimeProvider = userInputTimeProvider;
            _characterRepository = characterRepository;
            _animator = animator;
            _statusLabelSetter = statusLabelSetter;
            _mainButtonController = mainButtonController;
            _localizedStringFinder = localizedStringFinder;
            _sfxPlayer = sfxPlayer;
            _random = new Random();
        }

        public override void Update(GameTime gameTime)
        {
            _drunkStart.Match(
                some: ds =>
                {
                    if ((DateTime.Now - ds).TotalSeconds > _drunkTimeoutSeconds)
                    {
                        _drunkStart = Option.None<DateTime>();
                        _drunkTimeSinceLastEmote = Option.None<Stopwatch>();
                        _drunkIntervalSeconds = 0;

                        _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(
                            _characterRepository.MainCharacter.RenderProperties.WithIsDrunk(false));
                    }
                    else
                    {
                        _drunkTimeSinceLastEmote.MatchSome(dt =>
                        {
                            if (dt.Elapsed.TotalSeconds > _drunkIntervalSeconds)
                            {
                                _drunkIntervalSeconds = _random.Next(4, 7);
                                _drunkTimeSinceLastEmote = Option.Some(Stopwatch.StartNew());

                                if (_animator.Emote(_characterRepository.MainCharacter.ID, Emote.Drunk))
                                    _characterActions.Emote(Emote.Drunk);
                            }
                        });
                    }
                },
                none: () =>
                {
                    if (_characterRepository.MainCharacter.RenderProperties.IsDrunk)
                    {
                        _drunkStart = Option.Some(DateTime.Now);
                        _drunkIntervalSeconds = _random.Next(4, 7);
                        _drunkTimeSinceLastEmote = Option.Some(Stopwatch.StartNew());

                        if (_animator.Emote(_characterRepository.MainCharacter.ID, Emote.Drunk))
                            _characterActions.Emote(Emote.Drunk);
                    }
                });

            var now = DateTime.Now;

            if ((now - _userInputTimeProvider.LastInputTime).TotalMilliseconds >= AFK_TIME_MS)
            {
                if ((now - _userInputTimeProvider.LastInputTime).TotalMilliseconds >= AFK_TIME_MS + AFK_TIME_ALERT_MS)
                {
                    _mainButtonController.GoToInitialStateAndDisconnect(showLostConnection: true);
                }
                else
                {
                    _afkTimeSinceLastEmote.Match(
                        some: at =>
                        {
                            if (at.ElapsedMilliseconds >= AFK_TIME_BETWEEN_EMOTES_MS)
                            {
                                _animator.Emote(_characterRepository.MainCharacter.ID, Emote.Moon);
                                _characterActions.Emote(Emote.Moon);
                                _afkTimeSinceLastEmote = Option.Some(Stopwatch.StartNew());
                            }
                        },
                        none: () =>
                        {
                            _animator.Emote(_characterRepository.MainCharacter.ID, Emote.Moon);
                            _characterActions.Emote(Emote.Moon);
                            _afkTimeSinceLastEmote = Option.Some(Stopwatch.StartNew());
                        });

                    if ((DateTime.Now - _userInputTimeProvider.LastInputTime).TotalMilliseconds >= AFK_TIME_ALERT_MS)
                    {
                        _afkTimeSinceLastAlert.Match(
                            some: at =>
                            {
                                if (at.ElapsedMilliseconds >= AFK_TIME_BETWEEN_ALERTS_MS)
                                {
                                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.IDLE_TOO_LONG);
                                    _afkTimeSinceLastAlert = Option.Some(Stopwatch.StartNew());
                                    _altAlert = false;
                                }
                            },
                            none: () =>
                            {
                                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.IDLE_TOO_LONG);
                                _afkTimeSinceLastAlert = Option.Some(Stopwatch.StartNew());
                                _altAlert = false;
                            });

                        _afkTimeSinceLastAlert.Match(
                            some: at =>
                            {
                                if (at.ElapsedMilliseconds >= AFK_TIME_ALT_ALERT_MS && !_altAlert)
                                {
                                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.IDLE_PLEASE_START_MOVING);
                                    _altAlert = true;
                                }
                            },
                            none: () => _altAlert = false);
                    }
                }
            }
            else
            {
                _afkTimeSinceLastEmote = Option.None<Stopwatch>();
                _afkTimeSinceLastAlert = Option.None<Stopwatch>();
            }

            _arenaTimer.MatchSome(
                some: a =>
                {
                    if ((_arenaWarningCounter == 0 && a.ElapsedMilliseconds >= ARENA_BLOCK_INITIAL_TIME_MS) ||
                        (_arenaWarningCounter > 0 && a.ElapsedMilliseconds >= ARENA_BLOCK_WARNING_INTERVAL_MS))
                    {
                        _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.ARENA_DO_NOT_BLOCK_LINE);
                        _sfxPlayer.PlaySfx(SoundEffectID.ArenaTickSound);

                        if (_arenaWarningCounter == 1)
                        {
                            var resource = _localizedStringFinder.GetString(EOResourceID.ARENA_PLEASE_MOVE_FROM_PLACE);
                            _chatBubbleActions.ShowChatBubbleForMainCharacter(resource);
                        }
                        else if (_arenaWarningCounter == ARENA_BLOCK_MAX_WARNINGS)
                        {
                            _arenaTimer = Option.None<Stopwatch>();
                            _mainButtonController.GoToInitialStateAndDisconnect(showLostConnection: true);
                        }

                        StartArenaBlockTimer();
                        _arenaWarningCounter++;
                    }

                });

            base.Update(gameTime);
        }

        public void SetDrunkTimeout(int beerPotency)
        {
            _drunkTimeoutSeconds = (100 + (beerPotency * 10)) / 8.0;
        }

        public void StartArenaBlockTimer()
        {
            _arenaTimer = Option.Some(Stopwatch.StartNew());
        }

        public void CancelArenaBlockTimer()
        {
            _arenaTimer = Option.None<Stopwatch>();
            _arenaWarningCounter = 0;
        }
    }

    public interface IPeriodicEmoteHandler : IGameComponent, IUpdateable
    {
        void SetDrunkTimeout(int beerPotency);

        void StartArenaBlockTimer();

        void CancelArenaBlockTimer();
    }
}