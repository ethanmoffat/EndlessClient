using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Metadata;
using EndlessClient.Rendering.Metadata.Models;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Test
{
    public class CharacterStateTest : DrawableGameComponent
    {
        private enum DisplayState
        {
            Standing,
            SitChair,
            SitFloor,
            Attack1,
            Attack2,
            Walk1,
            Walk2,
            Walk3,
            SpellCast,
            AttackingAnimation,
            WalkingAnimation,
            SpellCastAnimation
        }

        private static readonly List<DisplayState> _allDisplayStates;

        static CharacterStateTest()
        {
            _allDisplayStates = ((DisplayState[])Enum.GetValues(typeof(DisplayState))).ToList();
        }

        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IMetadataProvider<WeaponMetadata> _weaponMetadataProvider;

        private CharacterRenderProperties _baseProperties;
        private readonly Dictionary<ItemType, int> _itemIndices;
        private readonly List<ICharacterRenderer> _renderersForDifferentStates;

        private KeyboardState _previousState, _currentState;

        private bool _isBowEquipped;
        private int _lastGraphic;

        private DateTime _lastWalk, _lastAttack, _lastSpell;

        public CharacterStateTest(IEndlessGame baseGame,
                                  ICharacterRendererFactory characterRendererFactory,
                                  IEIFFileProvider eifFileProvider,
                                  IMetadataProvider<WeaponMetadata> weaponMetadataProvider)
            : base((Game)baseGame)
        {
            _characterRendererFactory = characterRendererFactory;
            _eifFileProvider = eifFileProvider;
            _weaponMetadataProvider = weaponMetadataProvider;

            _itemIndices = ((ItemType[])Enum.GetValues(typeof(ItemType))).ToDictionary(k => k, v => 0);
            _renderersForDifferentStates = new List<ICharacterRenderer>(12);
        }

        public override void Initialize()
        {
            DrawOrder = 0;

            _baseProperties = new CharacterRenderProperties.Builder().ToImmutable();
            foreach (var displayState in _allDisplayStates)
            {
                var props = GetRenderPropertiesForState(displayState);
                _renderersForDifferentStates.Add(_characterRendererFactory.CreateCharacterRenderer(Character.Default.WithRenderProperties(props)));
                _renderersForDifferentStates.OfType<DrawableGameComponent>().Last().DrawOrder = 10;
            }

            _renderersForDifferentStates.ForEach(Game.Components.Add);

            _currentState = _previousState = Keyboard.GetState();
            _lastWalk = _lastAttack = _lastSpell = DateTime.Now;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            RefreshDisplayedCharacters();

            foreach (var displayState in _allDisplayStates)
            {
                var characterRenderer = _renderersForDifferentStates[(int) displayState];
                characterRenderer.SetAbsoluteScreenPosition(50 + 640 / 4 * ((int) displayState % 4),
                                                            30 + 480 / 3 * ((int) displayState / 4));
            }

            base.LoadContent();
        }

        //standing = 0
        //sitting: chair floor
        //attacking: 0 1 2   +animated (0-1-2-0)
        //walking:   0 1 2 3 +animated (0-1-2-3-0)
        //spellcast: 0 1     +animated (0-1-0)

        public override void Update(GameTime gameTime)
        {
            _currentState = Keyboard.GetState();

            var increment = ShiftPressed ? -1 : 1;

            var update = false;
            if (KeyPressed(Keys.D1))
            {
                _baseProperties = _baseProperties.WithGender((_baseProperties.Gender + increment) % 2);
                update = true;
            }
            else if (KeyPressed(Keys.D2))
            {
                if (CtrlPressed)
                {
                    const int NUM_HAIR_COLORS = 10;
                    if (_baseProperties.HairColor + increment < 0) _baseProperties = _baseProperties.WithHairColor(NUM_HAIR_COLORS);
                    _baseProperties = _baseProperties.WithHairColor((_baseProperties.HairColor + increment) % NUM_HAIR_COLORS);
                }
                else
                {
                    const int NUM_HAIR_STYLES = 21;
                    if (_baseProperties.HairStyle + increment < 0) _baseProperties = _baseProperties.WithHairColor(NUM_HAIR_STYLES);
                    _baseProperties = _baseProperties.WithHairStyle((_baseProperties.HairStyle + increment) % NUM_HAIR_STYLES);
                }
                update = true;
            }
            else if (KeyPressed(Keys.D3))
            {
                _baseProperties = _baseProperties.WithHatGraphic(GetNextItemGraphicMatching(ItemType.Hat, _baseProperties.HatGraphic));
                update = true;
            }
            else if (KeyPressed(Keys.D4))
            {
                _baseProperties = _baseProperties.WithArmorGraphic(GetNextItemGraphicMatching(ItemType.Armor, _baseProperties.ArmorGraphic));
                update = true;
            }
            else if (KeyPressed(Keys.D5))
            {
                _baseProperties = _baseProperties.WithBootsGraphic(GetNextItemGraphicMatching(ItemType.Boots, _baseProperties.BootsGraphic));
                update = true;
            }
            else if (KeyPressed(Keys.D6) && !_isBowEquipped)
            {
                var nextGraphic = GetNextItemGraphicMatching(ItemType.Weapon, _baseProperties.WeaponGraphic);
                _baseProperties = _baseProperties.WithWeaponGraphic(nextGraphic);
                update = true;
            }
            else if (KeyPressed(Keys.D7))
            {
                _baseProperties = _baseProperties.WithShieldGraphic(GetNextItemGraphicMatching(ItemType.Shield, _baseProperties.ShieldGraphic));
                update = true;
            }
            else if (KeyPressed(Keys.D8))
            {
                if ((int)_baseProperties.Direction + increment < 0) _baseProperties = _baseProperties.WithDirection((EODirection)4);
                _baseProperties = _baseProperties.WithDirection((EODirection)(((int)_baseProperties.Direction + increment) % 4));
                update = true;
            }
            else if (KeyPressed(Keys.Space))
            {
                if (!_isBowEquipped)
                {
                    _lastGraphic = _baseProperties.WeaponGraphic;
                    var firstBowWeapon = EIFFile.First(x => x.Type == ItemType.Weapon && x.SubType == ItemSubType.Ranged);
                    _baseProperties = _baseProperties.WithWeaponGraphic(firstBowWeapon.DollGraphic);
                }
                else
                {
                    _baseProperties = _baseProperties.WithWeaponGraphic(_lastGraphic);
                }
                
                _isBowEquipped = !_isBowEquipped;
                update = true;
            }

            if(update)
                RefreshDisplayedCharacters();

            _previousState = _currentState;

            var now = DateTime.Now;
            if ((now - _lastWalk).TotalMilliseconds > 500)
            {
                var rend = _renderersForDifferentStates[(int) DisplayState.WalkingAnimation];
                rend.Character = rend.Character.WithRenderProperties(rend.Character.RenderProperties.WithNextWalkFrame(false));
                _lastWalk = now;
            }

            if ((now - _lastAttack).TotalMilliseconds > 500)
            {
                var rend = _renderersForDifferentStates[(int)DisplayState.AttackingAnimation];
                var isRanged = _weaponMetadataProvider.GetValueOrDefault(rend.Character.RenderProperties.WeaponGraphic).Ranged;
                rend.Character = rend.Character.WithRenderProperties(rend.Character.RenderProperties.WithNextAttackFrame(isRanged));
                _lastAttack = now;
            }

            if ((now - _lastSpell).TotalMilliseconds > 500)
            {
                var rend = _renderersForDifferentStates[(int)DisplayState.SpellCastAnimation];
                rend.Character = rend.Character.WithRenderProperties(rend.Character.RenderProperties.WithNextSpellCastFrame());
                _lastSpell = now;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            base.Draw(gameTime);
        }

        private CharacterRenderProperties GetRenderPropertiesForState(DisplayState displayState)
        {
            var isRanged = _weaponMetadataProvider.GetValueOrDefault(_baseProperties.WeaponGraphic).Ranged;
            switch (displayState)
            {
                case DisplayState.Standing:
                    return _baseProperties;
                case DisplayState.SitChair:
                    return _baseProperties.WithSitState(SitState.Chair);
                case DisplayState.SitFloor:
                    return _baseProperties.WithSitState(SitState.Floor);
                case DisplayState.Attack1:
                    return _baseProperties.WithNextAttackFrame(isRanged);
                case DisplayState.Attack2:
                    return _baseProperties.WithNextAttackFrame(isRanged).WithNextAttackFrame(isRanged);
                case DisplayState.Walk1:
                    return _baseProperties.WithNextWalkFrame();
                case DisplayState.Walk2:
                    return _baseProperties.WithNextWalkFrame().WithNextWalkFrame();
                case DisplayState.Walk3:
                    return _baseProperties.WithNextWalkFrame().WithNextWalkFrame().WithNextWalkFrame();
                case DisplayState.SpellCast:
                    return _baseProperties.WithNextSpellCastFrame();
                //create a clone of the properties for animation
                case DisplayState.WalkingAnimation:
                case DisplayState.SpellCastAnimation:
                case DisplayState.AttackingAnimation:
                    return _baseProperties.ToBuilder().ToImmutable();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RefreshDisplayedCharacters()
        {
            foreach (var displayState in _allDisplayStates)
            {
                var props = GetRenderPropertiesForState(displayState);
                _renderersForDifferentStates[(int) displayState].Character = Character.Default.WithRenderProperties(props);
            }
        }

        private bool KeyPressed(Keys key)
        {
            return _previousState.IsKeyDown(key) && _currentState.IsKeyUp(key);
        }

        private bool ShiftPressed => _previousState.IsKeyDown(Keys.LeftShift) || _previousState.IsKeyDown(Keys.RightShift);

        private bool CtrlPressed => _previousState.IsKeyDown(Keys.LeftControl) || _previousState.IsKeyDown(Keys.RightControl);

        private int GetNextItemGraphicMatching(ItemType type, int currentGraphic)
        {
            var increment = ShiftPressed ? -1 : 1;
            var matchingItems = EIFFile.Where(x => x.Type == type).OrderBy(x => x.ID).ToList();
            _itemIndices[type] = (_itemIndices[type] + increment) % matchingItems.Count;

            if (_itemIndices[type] + increment < 0)
            {
                _itemIndices[type] = 0;
                return 0;
            }

            return matchingItems[_itemIndices[type]].DollGraphic;
        }

        private IPubFile<EIFRecord> EIFFile => _eifFileProvider.EIFFile;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _renderersForDifferentStates.ForEach(x => x.Dispose());
            }
            base.Dispose(disposing);
        }
    }
}
