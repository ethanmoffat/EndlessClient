// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EOLib;
using EOLib.Domain.Character;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
            _allDisplayStates = ((DisplayState[]) Enum.GetValues(typeof (DisplayState))).ToList();
        }

        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IEIFFileProvider _eifFileProvider;

        private ICharacterRenderProperties _baseProperties;
        private readonly List<ICharacterRenderer> _renderersForDifferentStates;

        private KeyboardState _previousState, _currentState;

        private bool _isBowEquipped;
        private short _lastGraphic;

        private DateTime _lastWalk, _lastAttack, _lastSpell;

        public CharacterStateTest(IEndlessGame baseGame,
                                  ICharacterRendererFactory characterRendererFactory,
                                  IEIFFileProvider eifFileProvider)
            : base((Game)baseGame)
        {
            _characterRendererFactory = characterRendererFactory;
            _eifFileProvider = eifFileProvider;

            _renderersForDifferentStates = new List<ICharacterRenderer>(12);
        }

        public override void Initialize()
        {
            DrawOrder = 0;

            _baseProperties = new CharacterRenderProperties();
            foreach (var displayState in _allDisplayStates)
            {
                var props = GetRenderPropertiesForState(displayState);
                _renderersForDifferentStates.Add(_characterRendererFactory.CreateCharacterRenderer(props));
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

            var update = false;
            if (KeyPressed(Keys.D1))
            {
                _baseProperties = _baseProperties.WithGender((byte) ((_baseProperties.Gender + 1)%2));
                update = true;
            }
            else if (KeyPressed(Keys.D2))
            {
                _baseProperties = _baseProperties.WithHairStyle((byte) ((_baseProperties.HairStyle + 1)%21));
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
                _baseProperties = _baseProperties.WithWeaponGraphic(GetNextItemGraphicMatching(ItemType.Weapon, _baseProperties.WeaponGraphic));
                update = true;
            }
            else if (KeyPressed(Keys.D7))
            {
                _baseProperties = _baseProperties.WithShieldGraphic(GetNextItemGraphicMatching(ItemType.Shield, _baseProperties.ShieldGraphic));
                update = true;
            }
            else if (KeyPressed(Keys.D8))
            {
                _baseProperties = _baseProperties.WithDirection((EODirection)(((int)_baseProperties.Direction + 1) % 4));
                update = true;
            }
            else if (KeyPressed(Keys.Space))
            {
                if (!_isBowEquipped)
                {
                    _lastGraphic = _baseProperties.WeaponGraphic;
                    var firstBowWeapon = EIFFile.Data.First(x => x.Type == ItemType.Weapon && x.SubType == ItemSubType.Ranged);
                    _baseProperties = _baseProperties.WithWeaponGraphic((short)firstBowWeapon.DollGraphic);
                }
                else
                    _baseProperties = _baseProperties.WithWeaponGraphic(_lastGraphic);
                
                _isBowEquipped = !_isBowEquipped;
                update = true;
            }

            if(update)
                RefreshDisplayedCharacters();

            _previousState = _currentState;

            var now = DateTime.Now;
            if ((now - _lastWalk).TotalMilliseconds > CharacterAnimator.WALK_FRAME_TIME_MS)
            {
                var rend = _renderersForDifferentStates[(int) DisplayState.WalkingAnimation];
                rend.RenderProperties = rend.RenderProperties.WithNextWalkFrame();
                _lastWalk = now;
            }

            if ((now - _lastAttack).TotalMilliseconds > 285)
            {
                var rend = _renderersForDifferentStates[(int)DisplayState.AttackingAnimation];
                rend.RenderProperties = rend.RenderProperties.WithNextAttackFrame();
                _lastAttack = now;
            }

            if ((now - _lastSpell).TotalMilliseconds > 280)
            {
                var rend = _renderersForDifferentStates[(int)DisplayState.SpellCastAnimation];
                rend.RenderProperties = rend.RenderProperties.WithNextSpellCastFrame();
                _lastSpell = now;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            base.Draw(gameTime);
        }

        private ICharacterRenderProperties GetRenderPropertiesForState(DisplayState displayState)
        {
            switch (displayState)
            {
                case DisplayState.Standing:
                    return _baseProperties;
                case DisplayState.SitChair:
                    return _baseProperties.WithSitState(SitState.Chair);
                case DisplayState.SitFloor:
                    return _baseProperties.WithSitState(SitState.Floor);
                case DisplayState.Attack1:
                    return _baseProperties.WithNextAttackFrame();
                case DisplayState.Attack2:
                    return _baseProperties.WithNextAttackFrame().WithNextAttackFrame();
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
                    return (ICharacterRenderProperties)_baseProperties.Clone();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RefreshDisplayedCharacters()
        {
            foreach (var displayState in _allDisplayStates)
            {
                var props = GetRenderPropertiesForState(displayState);
                _renderersForDifferentStates[(int) displayState].RenderProperties = props;
            }
        }

        private bool KeyPressed(Keys key)
        {
            return _previousState.IsKeyDown(key) && _currentState.IsKeyUp(key);
        }

        private short GetNextItemGraphicMatching(ItemType type, short currentGraphic)
        {
            var shiftPressed = _previousState.IsKeyDown(Keys.LeftShift) ||
                               _previousState.IsKeyDown(Keys.RightShift);
            var increment = shiftPressed ? -1 : 1;

            var matchingItems = EIFFile.Data.Where(x => x.Type == type).OrderBy(x => x.ID).ToList();
            var matchingIndex = matchingItems.FindIndex(x => x.DollGraphic == currentGraphic);
            var ndx = (matchingIndex + increment) % matchingItems.Count;
            if (ndx < 0)
                return 0;
            return (short) matchingItems[ndx].DollGraphic;
        }

        private IPubFile<EIFRecord> EIFFile { get { return _eifFileProvider.EIFFile; } }

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
