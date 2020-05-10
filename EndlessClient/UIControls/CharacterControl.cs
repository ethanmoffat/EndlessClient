using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class CharacterControl : XNAControl
    {
        public ICharacterRenderProperties RenderProperties
        {
            get { return _characterRenderer.RenderProperties; }
            protected set { _characterRenderer.RenderProperties = value; }
        }

        protected readonly ICharacterRenderer _characterRenderer;

        public CharacterControl(ICharacterRenderProperties initialProperties,
                                ICharacterRendererFactory characterRendererFactory)
        {
            _characterRenderer = characterRendererFactory.CreateCharacterRenderer(initialProperties);
        }

        public override void Initialize()
        {
            _characterRenderer.Initialize();
            _characterRenderer.SetAbsoluteScreenPosition(DrawAreaWithParentOffset.X, DrawAreaWithParentOffset.Y);

            base.Initialize();
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
