using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class CharacterControl : XNAControl
    {
        public CharacterRenderProperties RenderProperties
        {
            get { return _characterRenderer.Character.RenderProperties; }

            protected set
            {
                _characterRenderer.Character = _characterRenderer.Character.WithRenderProperties(value);
            }
        }

        protected readonly ICharacterRenderer _characterRenderer;

        public CharacterControl(Character character,
                                ICharacterRendererFactory characterRendererFactory)
        {
            _characterRenderer = characterRendererFactory.CreateCharacterRenderer(character, isUiControl: true);
        }

        public override void Initialize()
        {
            _characterRenderer.Initialize();
            _characterRenderer.SetAbsoluteScreenPosition(DrawAreaWithParentOffset.X, DrawAreaWithParentOffset.Y);

            base.Initialize();
        }

        protected override bool ShouldUpdate()
        {
            return Visible;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            _characterRenderer.Update(gameTime);
            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _characterRenderer.Draw(gameTime);
            base.OnDrawControl(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _characterRenderer.Dispose();

            base.Dispose(disposing);
        }
    }
}