using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Rendering;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class SettingsPanel : DraggableHudPanel
    {
        private enum KeyboardLayout
        {
            English,
            Dutch,
            Swedish,
            Azerty
        }

        private enum WhichSetting
        {
            Sfx,
            Mfx,
            Keyboard,
            Language,
            HearWhispers,
            ShowBalloons,
            ShowShadows,
            CurseFilter,
            LogChat,
            Interaction,
        }

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IChatActions _chatActions;
        private readonly IAudioActions _audioActions;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ISfxPlayer _sfxPlayer;

        private readonly Dictionary<WhichSetting, IXNALabel> _labels;
        private readonly Dictionary<WhichSetting, IXNAButton> _buttons;

        private bool _soundChanged, _musicChanged;
        private KeyboardLayout _keyboardLayout;

        public SettingsPanel(INativeGraphicsManager nativeGraphicsManager,
                             IChatActions chatActions,
                             IAudioActions audioActions,
                             IStatusLabelSetter statusLabelSetter,
                             ILocalizedStringFinder localizedStringFinder,
                             IEOMessageBoxFactory messageBoxFactory,
                             IConfigurationRepository configurationRepository,
                             ISfxPlayer sfxPlayer,
                             IClientWindowSizeProvider clientWindowSizeProvider)
            : base(clientWindowSizeProvider.Resizable)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _chatActions = chatActions;
            _audioActions = audioActions;
            _statusLabelSetter = statusLabelSetter;
            _localizedStringFinder = localizedStringFinder;
            _messageBoxFactory = messageBoxFactory;
            _configurationRepository = configurationRepository;
            _sfxPlayer = sfxPlayer;

            BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 47);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);

            var values = Enum.GetValues<WhichSetting>();
            _labels = values.ToDictionary(k => k,
                v =>
                {
                    var ndx = (int)v;
                    return (IXNALabel)new XNALabel(Constants.FontSize09)
                    {
                        DrawArea = new Rectangle(117 + (ndx / 5) * 239, 25 + 18 * (ndx % 5), 100, 15),
                        AutoSize = false,
                        ForeColor = ColorConstants.LightGrayText,
                    };
                });

            UpdateDisplayText();

            _buttons = values.ToDictionary(k => k,
                v =>
                {
                    var ndx = (int)v;
                    return (IXNAButton)new XNAButton(
                        _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27, true),
                        new Vector2(215 + (ndx / 5) * 239, 25 + 18 * (ndx % 5)),
                        new Rectangle(0, 0, 19, 15),
                        new Rectangle(19, 0, 19, 15));
                });
        }

        public override void Initialize()
        {
            foreach (var label in _labels.Values)
            {
                label.SetParentControl(this);
                label.Initialize();
            }

            foreach (var pair in _buttons)
            {
                var button = pair.Value;
                button.OnMouseDown += (_, _) => SettingChange(pair.Key);
                button.OnMouseEnter += (_, _) => _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_BUTTON, EOResourceID.STATUS_LABEL_SETTINGS_CLICK_TO_CHANGE);
                button.SetParentControl(this);
                button.Initialize();
            }

            if (!_configurationRepository.HearWhispers)
                _chatActions.SetHearWhispers(_configurationRepository.HearWhispers);

            base.Initialize();
        }

        private void SettingChange(WhichSetting setting)
        {
            _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);

            switch (setting)
            {
                case WhichSetting.Sfx:
                    {
                        // this alert is emulated even though it isn't needed
                        if (!_soundChanged && !_configurationRepository.SoundEnabled)
                        {
                            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SETTINGS_SOUND_DISABLED, EODialogButtons.OkCancel);
                            dlg.DialogClosing += (_, e) =>
                            {
                                if (e.Result != XNADialogResult.OK)
                                    return;

                                _soundChanged = true;
                                _configurationRepository.SoundEnabled = !_configurationRepository.SoundEnabled;
                                _audioActions.ToggleSound();

                                UpdateDisplayText();
                            };
                            dlg.ShowDialog();
                        }
                        else
                        {
                            _soundChanged = true;
                            _configurationRepository.SoundEnabled = !_configurationRepository.SoundEnabled;
                            _audioActions.ToggleSound();
                        }
                    }
                    break;
                case WhichSetting.Mfx:
                    {
                        // this alert is emulated even though it isn't needed
                        if (!_musicChanged && !_configurationRepository.MusicEnabled)
                        {
                            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SETTINGS_MUSIC_DISABLED, EODialogButtons.OkCancel);
                            dlg.DialogClosing += (_, e) =>
                            {
                                if (e.Result != XNADialogResult.OK)
                                    return;

                                _musicChanged = true;
                                _configurationRepository.MusicEnabled = !_configurationRepository.MusicEnabled;
                                _audioActions.ToggleBackgroundMusic();

                                UpdateDisplayText();
                            };
                            dlg.ShowDialog();
                        }
                        else
                        {
                            _musicChanged = true;
                            _configurationRepository.MusicEnabled = !_configurationRepository.MusicEnabled;
                            _audioActions.ToggleBackgroundMusic();
                        }
                    }
                    break;
                case WhichSetting.Keyboard:
                    {
                        // this doesn't actually change anything...
                        _keyboardLayout++;
                        if (_keyboardLayout > KeyboardLayout.Azerty)
                            _keyboardLayout = 0;
                    }
                    break;
                case WhichSetting.Language:
                    {
                        _configurationRepository.Language++;
                        if (_configurationRepository.Language > EOLanguage.Portuguese)
                            _configurationRepository.Language = 0;
                    }
                    break;
                case WhichSetting.HearWhispers:
                    {
                        _configurationRepository.HearWhispers = !_configurationRepository.HearWhispers;
                        _chatActions.SetHearWhispers(_configurationRepository.HearWhispers);
                    }
                    break;
                case WhichSetting.ShowBalloons:
                    _configurationRepository.ShowChatBubbles = !_configurationRepository.ShowChatBubbles;
                    break;
                case WhichSetting.ShowShadows:
                    _configurationRepository.ShowShadows = !_configurationRepository.ShowShadows;
                    break;
                case WhichSetting.CurseFilter:
                    {
                        if (_configurationRepository.StrictFilterEnabled)
                        {
                            _configurationRepository.StrictFilterEnabled = false;
                        }
                        else if (_configurationRepository.CurseFilterEnabled)
                        {
                            _configurationRepository.CurseFilterEnabled = false;
                            _configurationRepository.StrictFilterEnabled = true;
                        }
                        else
                        {
                            _configurationRepository.CurseFilterEnabled = true;
                        }
                    }
                    break;
                case WhichSetting.LogChat:
                    _configurationRepository.LogChatToFile = !_configurationRepository.LogChatToFile;
                    break;
                case WhichSetting.Interaction:
                    // todo: block trade requests when this is true
                    _configurationRepository.Interaction = !_configurationRepository.Interaction;
                    break;
            }

            UpdateDisplayText();
        }

        private void UpdateDisplayText()
        {
            _labels[WhichSetting.Sfx].Text = _localizedStringFinder.GetString(_configurationRepository.SoundEnabled ? EOResourceID.SETTING_ENABLED : EOResourceID.SETTING_DISABLED);
            _labels[WhichSetting.Mfx].Text = _localizedStringFinder.GetString(_configurationRepository.MusicEnabled ? EOResourceID.SETTING_ENABLED : EOResourceID.SETTING_DISABLED);
            _labels[WhichSetting.Keyboard].Text = _localizedStringFinder.GetString(EOResourceID.SETTING_KEYBOARD_ENGLISH);
            _labels[WhichSetting.Language].Text = _localizedStringFinder.GetString(EOResourceID.SETTING_LANG_CURRENT);
            _labels[WhichSetting.HearWhispers].Text = _localizedStringFinder.GetString(_configurationRepository.HearWhispers ? EOResourceID.SETTING_ENABLED : EOResourceID.SETTING_DISABLED);

            _labels[WhichSetting.ShowBalloons].Text = _localizedStringFinder.GetString(_configurationRepository.ShowChatBubbles ? EOResourceID.SETTING_ENABLED : EOResourceID.SETTING_DISABLED);
            _labels[WhichSetting.ShowShadows].Text = _localizedStringFinder.GetString(_configurationRepository.ShowShadows ? EOResourceID.SETTING_ENABLED : EOResourceID.SETTING_DISABLED);
            if (_configurationRepository.StrictFilterEnabled)
                _labels[WhichSetting.CurseFilter].Text = _localizedStringFinder.GetString(EOResourceID.SETTING_EXCLUSIVE);
            else if (_configurationRepository.CurseFilterEnabled)
                _labels[WhichSetting.CurseFilter].Text = _localizedStringFinder.GetString(EOResourceID.SETTING_NORMAL);
            else
                _labels[WhichSetting.CurseFilter].Text = _localizedStringFinder.GetString(EOResourceID.SETTING_DISABLED);

            _labels[WhichSetting.LogChat].Text = _localizedStringFinder.GetString(_configurationRepository.LogChatToFile ? EOResourceID.SETTING_ENABLED : EOResourceID.SETTING_DISABLED);
            _labels[WhichSetting.Interaction].Text = _localizedStringFinder.GetString(_configurationRepository.Interaction ? EOResourceID.SETTING_ENABLED : EOResourceID.SETTING_DISABLED);
        }
    }
}
