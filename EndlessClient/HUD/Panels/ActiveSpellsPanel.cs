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
using Optional;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using XNAControls;

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

        private readonly Dictionary<int, int> _spellSlotMap;
        private readonly List<ISpellPanelItem> _childItems;

        private readonly Texture2D _functionKeyLabelSheet;
        private readonly Rectangle _functionKeyRow1Source, _functionKeyRow2Source;

        private XNALabel _selectedSpellName, _selectedSpellLevel, _totalSkillPoints;
        private XNAButton _levelUpButton1, _levelUpButton2;
        private ScrollBar _scrollBar;

        private HashSet<IInventorySpell> _cachedSpells;
        private bool _confirmedTraining;

        public INativeGraphicsManager NativeGraphicsManager { get; }

        public ActiveSpellsPanel(INativeGraphicsManager nativeGraphicsManager,
                                 ITrainingController trainingController,
                                 IEOMessageBoxFactory messageBoxFactory,
                                 IStatusLabelSetter statusLabelSetter,
                                 IPlayerInfoProvider playerInfoProvider,
                                 ICharacterProvider characterProvider,
                                 ICharacterInventoryProvider characterInventoryProvider,
                                 IESFFileProvider esfFileProvider)
        {
            NativeGraphicsManager = nativeGraphicsManager;
            _trainingController = trainingController;
            _messageBoxFactory = messageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
            _playerInfoProvider = playerInfoProvider;
            _characterProvider = characterProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _esfFileProvider = esfFileProvider;

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
                    });
                }

                foreach (var spell in updated)
                {
                    var matchedSpell = _childItems.SingleOrNone(x => x.InventorySpell.ID == spell.ID);
                    matchedSpell.MatchSome(childControl =>
                    {
                        childControl.InventorySpell = spell;
                    });
                }

                foreach (var spell in added)
                {
                    var spellData = _esfFileProvider.ESFFile[spell.ID];

                    var preferredSlot = _spellSlotMap.SingleOrNone(x => x.Value == spell.ID).Map(x => x.Key);
                    var actualSlot = preferredSlot.Match(
                        some: x =>
                        {
                            return _childItems.Any(ci => ci.Slot == x)
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

                        newChild.Selected += SetSpellStatusLabelSelected;
                        newChild.OnMouseOver += SetSpellStatusLabelHover;
                        //newItem.DoubleClick += HandleItemDoubleClick;
                        //newItem.DoneDragging += HandleItemDoneDragging;

                        _childItems.Add(newChild);
                    });
                }

                _cachedSpells = _characterInventoryProvider.SpellInventory.ToHashSet();
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);
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
                //var selectedSpell = _childItems.Single(x => x.Selected);
                // todo: implement in training controller
                //_trainingController.LevelUpSpell(selectedSpell.SpellData.ID);
            }
        }

        private void SetSpellStatusLabelSelected(object sender, EventArgs e)
        {
            var spell = ((SpellPanelItem)sender).SpellData;

            if (spell.Target == EOLib.IO.SpellTarget.Normal)
                _statusLabelSetter.SetStatusLabel(EOResourceID.SKILLMASTER_WORD_SPELL, spell.Name, EOResourceID.SPELL_WAS_SELECTED);
            else if (spell.Target == EOLib.IO.SpellTarget.Group /*&& not in party*/) // todo: parties
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.SPELL_ONLY_WORKS_ON_GROUP);
        }

        private void SetSpellStatusLabelHover(object sender, EventArgs e)
        {
            var spell = ((SpellPanelItem)sender).SpellData;
            _statusLabelSetter.SetStatusLabel(EOResourceID.SKILLMASTER_WORD_SPELL, spell.Name);
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
            emptyItem.Selected += (_, _) => _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.SPELL_NOTHING_WAS_SELECTED);
            emptyItem.Clicked += (_, _) =>
            {
                //if (DoEmptySpellIconUpdateLogic)
                //{
                //    if (MouseOver && MouseOverPreviously &&
                //        _parentSpellContainer.AnySpellsSelected() &&
                //        !_parentSpellContainer.AnySpellsDragging() &&
                //        Mouse.GetState().LeftButton == ButtonState.Released &&
                //        PreviousMouseState.LeftButton == ButtonState.Pressed)
                //    {
                //        _parentSpellContainer.ClearSelectedSpell();
                //    }
                //}
            };
            emptyItem.Initialize();
            return emptyItem;
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !File.Exists(Constants.InventoryFile))
            {
                using var inventoryKey = TryGetCharacterRegistryKey(accountName, characterName);
                if (inventoryKey != null)
                {
                    for (int i = 0; i < SpellRowLength * 4; ++i)
                    {
                        if (int.TryParse(inventoryKey.GetValue($"item{i}")?.ToString() ?? string.Empty, out var id))
                            map[i] = id;
                    }
                }
            }

            var inventory = new IniReader(Constants.InventoryFile);
            if (inventory.Load() && inventory.Sections.ContainsKey(accountName))
            {
                var section = inventory.Sections[accountName];
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
            var inventory = new IniReader(Constants.InventoryFile);

            var section = inventory.Load() && inventory.Sections.ContainsKey(_playerInfoProvider.LoggedInAccountName)
                ? inventory.Sections[_playerInfoProvider.LoggedInAccountName]
                : new SortedList<string, string>();

            var existing = section.Where(x => x.Key.Contains(_characterProvider.MainCharacter.Name)).Select(x => x.Key).ToList();
            foreach (var key in existing)
                section.Remove(key);

            foreach (var item in _childItems)
                section[$"{_characterProvider.MainCharacter.Name}.{item.Slot}"] = $"{item.InventorySpell.ID}";

            inventory.Sections[_playerInfoProvider.LoggedInAccountName] = section;

            inventory.Save();
        }

        #endregion
    }
}