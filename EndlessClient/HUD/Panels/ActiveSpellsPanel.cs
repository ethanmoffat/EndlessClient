using EndlessClient.Controllers;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD.Spells;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Optional;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using XNAControls;

using static EndlessClient.HUD.Spells.SpellPanelItem;

namespace EndlessClient.HUD.Panels
{
    public class ActiveSpellsPanel : XNAPanel, IHudPanel
    {
        public const int SpellRows = 4;
        public const int SpellRowLength = 8;

        private readonly ITrainingController _trainingController;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IESFFileProvider _esfFileProvider;
        private readonly ISpellSlotDataRepository _spellSlotDataRepository;

        private readonly Dictionary<int, int> _spellSlotMap;
        private readonly List<ISpellPanelItem> _childItems;

        private readonly Texture2D _functionKeyLabelSheet;
        private Rectangle _functionKeyRow1Source, _functionKeyRow2Source;

        private readonly XNALabel _selectedSpellName, _selectedSpellLevel, _totalSkillPoints;
        private readonly XNAButton _levelUpButton1, _levelUpButton2;
        private readonly ScrollBar _scrollBar;

        private HashSet<IInventorySpell> _cachedSpells;
        private CharacterStats _cachedStats;
        private Option<int> _cachedSelectedSpell;

        private bool _confirmedTraining;
        private int _lastScrollOffset;
        private Texture2D _activeSpellIcon;

        public INativeGraphicsManager NativeGraphicsManager { get; }

        public ActiveSpellsPanel(INativeGraphicsManager nativeGraphicsManager,
                                 ITrainingController trainingController,
                                 IEOMessageBoxFactory messageBoxFactory,
                                 IStatusLabelSetter statusLabelSetter,
                                 IPlayerInfoProvider playerInfoProvider,
                                 ICharacterProvider characterProvider,
                                 ICharacterInventoryProvider characterInventoryProvider,
                                 IESFFileProvider esfFileProvider,
                                 ISpellSlotDataRepository spellSlotDataRepository)
        {
            NativeGraphicsManager = nativeGraphicsManager;
            _trainingController = trainingController;
            _messageBoxFactory = messageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
            _playerInfoProvider = playerInfoProvider;
            _characterProvider = characterProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _esfFileProvider = esfFileProvider;
            _spellSlotDataRepository = spellSlotDataRepository;

            _spellSlotMap = GetSpellSlotMap(_playerInfoProvider.LoggedInAccountName, _characterProvider.MainCharacter.Name);
            _childItems = new List<ISpellPanelItem>();
            ResetChildItems();

            _cachedSpells = new HashSet<IInventorySpell>();

            _functionKeyLabelSheet = NativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 58, true);
            _functionKeyRow1Source = new Rectangle(148, 51, 18, 13);
            _functionKeyRow2Source = new Rectangle(148 + 18 * 8, 51, 18, 13);

            _selectedSpellName = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(9, 50, 81, 13),
                Visible = false,
                Text = "",
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleCenter,
                ForeColor = ColorConstants.LightGrayText,
            };

            _selectedSpellLevel = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(32, 78, 42, 15),
                Visible = true,
                Text = "0",
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText,
            };

            _totalSkillPoints = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(32, 96, 42, 15),
                Visible = true,
                Text = _characterProvider.MainCharacter.Stats[CharacterStat.SkillPoints].ToString(),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText,
            };

            var buttonSheet = NativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27, true);
            _levelUpButton1 = new XNAButton(buttonSheet, new Vector2(71, 77), new Rectangle(215, 386, 19, 15), new Rectangle(234, 386, 19, 15))
            {
                FlashSpeed = 500,
                Visible = false
            };
            _levelUpButton1.OnClick += LevelUp_Click;

            _levelUpButton2 = new XNAButton(buttonSheet, new Vector2(71, 95), new Rectangle(215, 386, 19, 15), new Rectangle(234, 386, 19, 15))
            {
                FlashSpeed = 500,
                Visible = false
            };
            _levelUpButton2.OnClick += LevelUp_Click;

            _scrollBar = new ScrollBar(new Vector2(467, 2), new Vector2(16, 115), ScrollBarColors.LightOnMed, NativeGraphicsManager)
            {
                LinesToRender = 2
            };
            _scrollBar.UpdateDimensions(4);

            BackgroundImage = NativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 62);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);

            Game.Exiting += SaveSpellsFile;
        }

        public override void Initialize()
        {
            _selectedSpellName.Initialize();
            _selectedSpellName.SetParentControl(this);

            _selectedSpellLevel.Initialize();
            _selectedSpellLevel.SetParentControl(this);

            _totalSkillPoints.Initialize();
            _totalSkillPoints.SetParentControl(this);

            _levelUpButton1.Initialize();
            _levelUpButton1.SetParentControl(this);

            _levelUpButton2.Initialize();
            _levelUpButton2.SetParentControl(this);

            _scrollBar.Initialize();
            _scrollBar.SetParentControl(this);

            base.Initialize();
        }

        public bool AnySpellsDragging() => _childItems.Any(x => x.IsBeingDragged);

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (!_cachedSpells.SetEquals(_characterInventoryProvider.SpellInventory))
            {
                var added = _characterInventoryProvider.SpellInventory.Where(i => !_cachedSpells.Any(j => i.ID == j.ID));
                var removed = _cachedSpells.Where(i => !_characterInventoryProvider.SpellInventory.Any(j => i.ID == j.ID));
                var updated = _characterInventoryProvider.SpellInventory.Except(added)
                    .Where(i => _cachedSpells.Any(j => i.ID == j.ID && i.Level != j.Level));

                foreach (var spell in removed)
                {
                    var matchedSpell = _childItems.SingleOrNone(x => x.InventorySpell.ID == spell.ID);
                    matchedSpell.MatchSome(childControl =>
                    {
                        childControl.SetControlUnparented();
                        childControl.Dispose();
                        _childItems.Remove(childControl);

                        _childItems.Add(CreateEmptySpell(childControl.Slot));
                        _spellSlotDataRepository.SpellSlots[childControl.Slot] = Option.None<IInventorySpell>();
                    });
                }

                foreach (var spell in updated)
                {
                    var matchedSpell = _childItems.SingleOrNone(x => x.InventorySpell.ID == spell.ID);
                    matchedSpell.MatchSome(childControl =>
                    {
                        childControl.InventorySpell = spell;
                        _spellSlotDataRepository.SelectedSpellSlot.MatchSome(s =>
                        {
                            if (childControl.Slot == s)
                                _selectedSpellLevel.Text = spell.Level.ToString();
                        });
                    });
                }

                foreach (var spell in added)
                {
                    var spellData = _esfFileProvider.ESFFile[spell.ID];

                    var preferredSlot = _spellSlotMap.SingleOrNone(x => x.Value == spell.ID).Map(x => x.Key);
                    var actualSlot = preferredSlot.Match(
                        some: x =>
                        {
                            return _childItems.OfType<SpellPanelItem>().Any(ci => ci.Slot == x)
                                ? GetNextOpenSlot(_childItems)
                                : Option.Some(x);
                        },
                        none: () => GetNextOpenSlot(_childItems));

                    actualSlot.MatchSome(slot =>
                    {
                        _childItems.SingleOrNone(ci => ci.Slot == slot)
                            .MatchSome(ci =>
                            {
                                ci.SetControlUnparented();
                                ci.Dispose();
                                _childItems.Remove(ci);
                            });

                        var newChild = new SpellPanelItem(this, slot, spell, spellData);
                        newChild.Initialize();

                        newChild.Clicked += (sender, _) => _spellSlotDataRepository.SelectedSpellSlot = Option.Some(((SpellPanelItem)sender).Slot);
                        newChild.OnMouseOver += SetSpellStatusLabelHover;
                        newChild.DoneDragging += ItemDraggingCompleted;

                        _childItems.Add(newChild);
                        _spellSlotDataRepository.SpellSlots[slot] = Option.Some(spell);
                    });
                }

                if (added.Any())
                    UpdateSpellItemsForScroll();

                _cachedSpells = _characterInventoryProvider.SpellInventory.ToHashSet();
            }

            if ((CurrentKeyState.IsKeyDown(Keys.RightShift) && PreviousKeyState.IsKeyUp(Keys.RightShift)) ||
                (CurrentKeyState.IsKeyDown(Keys.LeftShift) && PreviousKeyState.IsKeyUp(Keys.LeftShift)) ||
                (CurrentKeyState.IsKeyUp(Keys.RightShift) && PreviousKeyState.IsKeyDown(Keys.RightShift)) ||
                (CurrentKeyState.IsKeyUp(Keys.LeftShift) && PreviousKeyState.IsKeyDown(Keys.LeftShift)))
            {
                SwapFunctionKeySourceRectangles();
            }

            if (_lastScrollOffset != _scrollBar.ScrollOffset)
            {
                UpdateSpellItemsForScroll();
            }

            if (_cachedStats != _characterProvider.MainCharacter.Stats && _activeSpellIcon != null)
            {
                _cachedStats = _characterProvider.MainCharacter.Stats;
                var skillPoints = _cachedStats[CharacterStat.SkillPoints];
                _totalSkillPoints.Text = skillPoints.ToString();

                _levelUpButton1.Visible = skillPoints > 0;
                _levelUpButton2.Visible = skillPoints > 0;
            }

            _cachedSelectedSpell.Match(
                some: cached => _spellSlotDataRepository.SelectedSpellSlot.Match(
                    some: repoSlot =>
                    {
                        if (cached != repoSlot)
                        {
                            SetSelectedSpell(repoSlot);
                            _cachedSelectedSpell = _spellSlotDataRepository.SelectedSpellSlot;
                        }
                    },
                    none: () =>
                    {
                        ClearSelectedSpell();
                        _cachedSelectedSpell = Option.None<int>();
                    }),
                none: () => _spellSlotDataRepository.SelectedSpellSlot.MatchSome(
                    some: repoSlot =>
                    {
                        SetSelectedSpell(repoSlot);
                        _cachedSelectedSpell = _spellSlotDataRepository.SelectedSpellSlot;
                    }));

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);

            _spriteBatch.Begin();

            DrawFunctionKeyLabels();
            DrawActiveSpell();

            _spriteBatch.End();
        }

        private void DrawFunctionKeyLabels()
        {
            if (_scrollBar.ScrollOffset >= 2)
                return;

            for (int i = 0; i < 8; ++i)
            {
                var offset = _functionKeyRow1Source.Width * i;

                if (_scrollBar.ScrollOffset == 0)
                {
                    _spriteBatch.Draw(_functionKeyLabelSheet,
                        new Vector2(202 + 45 * i, 338),
                        _functionKeyRow1Source.WithPosition((_functionKeyRow1Source.Location + new Point(offset, 0)).ToVector2()),
                        Color.White);
                }

                if (_scrollBar.ScrollOffset < 2)
                {
                    var yCoord = _scrollBar.ScrollOffset == 0 ? 390 : 338;
                    _spriteBatch.Draw(_functionKeyLabelSheet,
                        new Vector2(202 + 45 * i, yCoord),
                        _functionKeyRow2Source.WithPosition((_functionKeyRow2Source.Location + new Point(offset, 0)).ToVector2()),
                        Color.White);
                }
            }
        }

        private void DrawActiveSpell()
        {
            if (_activeSpellIcon == null)
                return;

            var srcRect = new Rectangle(0, 0, _activeSpellIcon.Width / 2, _activeSpellIcon.Height);
            var dstRect = new Rectangle(DrawAreaWithParentOffset.X + 32, DrawAreaWithParentOffset.Y + 14, srcRect.Width, srcRect.Height);
            _spriteBatch.Draw(_activeSpellIcon, dstRect, srcRect, Color.White);
        }

        private void LevelUp_Click(object sender, EventArgs args)
        {
            if (!_confirmedTraining)
            {
                //apparently this is NOT stored in the edf files...
                //NOTE: copy-pasted from EOCharacterStats button event handler. Should probably be in some shared function somewhere.
                var dialog = _messageBoxFactory.CreateMessageBox("Do you want to train?",
                    "Skill training",
                    EODialogButtons.OkCancel);
                dialog.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                        _confirmedTraining = true;
                };
                dialog.ShowDialog();
            }
            else
            {
                _cachedSelectedSpell.MatchSome(slot =>
                {
                    _childItems.SingleOrNone(x => x.Slot == slot)
                        .MatchSome(x => _trainingController.AddSkillPoint(x.SpellData.ID));
                });
            }
        }

        private void SetSelectedSpell(int slot)
        {
            _childItems.OfType<SpellPanelItem>().SingleOrNone(x => x.Slot == slot)
                .MatchSome(spell =>
                {
                    var spellData = spell.SpellData;

                    _activeSpellIcon = NativeGraphicsManager.TextureFromResource(GFXTypes.SpellIcons, spellData.Icon);

                    _selectedSpellName.Text = spellData.Name;
                    _selectedSpellName.Visible = true;

                    _selectedSpellLevel.Text = spell.InventorySpell.Level.ToString();

                    _levelUpButton1.Visible = _levelUpButton2.Visible = _characterProvider.MainCharacter.Stats[CharacterStat.SkillPoints] > 0;
                });
        }

        private void ClearSelectedSpell()
        {
            _activeSpellIcon = null;

            _selectedSpellName.Text = string.Empty;
            _selectedSpellName.Visible = false;

            _selectedSpellLevel.Text = "0";

            _levelUpButton1.Visible = _levelUpButton2.Visible = false;
        }

        private void SetSpellStatusLabelHover(object sender, EventArgs e)
        {
            var spell = ((SpellPanelItem)sender).SpellData;
            _statusLabelSetter.SetStatusLabel(EOResourceID.SKILLMASTER_WORD_SPELL, spell.Name);
        }

        private void ItemDraggingCompleted(object sender, SpellDragCompletedEventArgs e)
        {
            var item = (SpellPanelItem)sender;

            _childItems.SingleOrNone(x => x.MouseOver)
                .Match(child =>
                {
                    if (child != item)
                    {
                        var oldSlot = item.Slot;
                        var oldDisplaySlot = item.DisplaySlot;

                        var newSlot = child.Slot;
                        var newDisplaySlot = child.DisplaySlot;

                        item.Slot = newSlot;
                        item.DisplaySlot = newDisplaySlot;

                        child.Slot = oldSlot;
                        child.DisplaySlot = oldDisplaySlot;

                        _spellSlotDataRepository.SpellSlots[oldSlot] = Option.None<IInventorySpell>();
                        _spellSlotDataRepository.SpellSlots[newSlot] = Option.Some(item.InventorySpell);
                        _spellSlotDataRepository.SelectedSpellSlot = Option.Some(newSlot);
                        _spellSlotDataRepository.SpellIsPrepared = false;
                    }
                },
                () => e.ContinueDragging = true);
        }

        private static Option<int> GetNextOpenSlot(IEnumerable<ISpellPanelItem> childItems)
        {
            // get the minimum slot when there is an Empty space
            return childItems.OfType<EmptySpellPanelItem>()
                .SomeWhen(x => x.Any())
                .Map(x => x.Min(y => y.Slot));
        }

        private void ResetChildItems()
        {
            for (int slot = 0; slot < SpellRows * SpellRowLength; slot++)
            {
                _childItems.Add(CreateEmptySpell(slot));
            }
        }

        private ISpellPanelItem CreateEmptySpell(int slot)
        {
            var emptyItem = new EmptySpellPanelItem(this, slot);
            emptyItem.Clicked += (_, _) => _spellSlotDataRepository.SelectedSpellSlot = Option.None<int>();
            emptyItem.Initialize();
            return emptyItem;
        }

        private void SwapFunctionKeySourceRectangles()
        {
            var tmpRect = _functionKeyRow2Source;
            _functionKeyRow2Source = _functionKeyRow1Source;
            _functionKeyRow1Source = tmpRect;
        }

        private void UpdateSpellItemsForScroll()
        {
            var firstValidSlot = _scrollBar.ScrollOffset * SpellRowLength;
            var lastValidSlot = firstValidSlot + 2 * SpellRowLength;

            var itemsToHide = _childItems.Where(x => x.Slot < firstValidSlot || x.Slot >= lastValidSlot).ToList();
            foreach (var item in itemsToHide)
            {
                if (!item.IsBeingDragged)
                    ((XNAControl)item).Visible = false;

                item.DisplaySlot = GetDisplaySlotFromSlot(item.Slot);
            }

            foreach (var item in _childItems.Except(itemsToHide))
            {
                ((XNAControl)item).Visible = true;
                item.DisplaySlot = item.Slot - firstValidSlot;
            }

            _lastScrollOffset = _scrollBar.ScrollOffset;
        }

        private int GetDisplaySlotFromSlot(int newSlot)
        {
            var offset = _scrollBar.ScrollOffset;
            return newSlot - SpellRowLength * offset;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Game.Exiting -= SaveSpellsFile;
                SaveSpellsFile(null, EventArgs.Empty);
            }

            base.Dispose(disposing);
        }

        #region Slot loading

        private static Dictionary<int, int> GetSpellSlotMap(string accountName, string characterName)
        {
            var map = new Dictionary<int, int>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !File.Exists(Constants.SpellsFile))
            {
                using var spellsKey = TryGetCharacterRegistryKey(accountName, characterName);
                if (spellsKey != null)
                {
                    for (int i = 0; i < SpellRowLength * SpellRows; ++i)
                    {
                        if (int.TryParse(spellsKey.GetValue($"item{i}")?.ToString() ?? string.Empty, out var id))
                            map[i] = id;
                    }
                }
            }

            var spells = new IniReader(Constants.SpellsFile);
            if (spells.Load() && spells.Sections.ContainsKey(accountName))
            {
                var section = spells.Sections[accountName];
                foreach (var key in section.Keys.Where(x => x.Contains(characterName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!key.Contains("."))
                        continue;

                    var slot = key.Split(".")[1];
                    if (!int.TryParse(slot, out var slotIndex))
                        continue;

                    if (int.TryParse(section[key], out var id))
                        map[slotIndex] = id;
                }
            }

            return map;
        }

        [SupportedOSPlatform("Windows")]
        private static RegistryKey TryGetCharacterRegistryKey(string accountName, string characterName)
        {
            using RegistryKey currentUser = Registry.CurrentUser;

            var pathSegments = $"Software\\EndlessClient\\{accountName}\\{characterName}\\spells".Split('\\');
            var currentPath = string.Empty;

            RegistryKey retKey = null;
            foreach (var segment in pathSegments)
            {
                retKey?.Dispose();

                currentPath = Path.Combine(currentPath, segment);
                retKey = currentUser.CreateSubKey(currentPath, RegistryKeyPermissionCheck.ReadSubTree);
            }

            return retKey;
        }

        private void SaveSpellsFile(object sender, EventArgs e)
        {
            var spells = new IniReader(Constants.SpellsFile);

            var section = spells.Load() && spells.Sections.ContainsKey(_playerInfoProvider.LoggedInAccountName)
                ? spells.Sections[_playerInfoProvider.LoggedInAccountName]
                : new SortedList<string, string>();

            var existing = section.Where(x => x.Key.Contains(_characterProvider.MainCharacter.Name)).Select(x => x.Key).ToList();
            foreach (var key in existing)
                section.Remove(key);

            foreach (var item in _childItems.OfType<SpellPanelItem>())
                section[$"{_characterProvider.MainCharacter.Name}.{item.Slot}"] = $"{item.InventorySpell.ID}";

            spells.Sections[_playerInfoProvider.LoggedInAccountName] = section;

            spells.Save();
        }

        #endregion
    }
}