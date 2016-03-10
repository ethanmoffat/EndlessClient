// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Sprites;
using EOLib.Data.BLL;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
	public class CharacterRenderer : DrawableGameComponent, ICharacterRenderer
	{
		private ICharacterSpriteCalculator _spriteCalculator;
		private ICharacterRenderProperties _characterRenderPropertiesPrivate;
		private ICharacterTextures _textures;
		private bool _textureUpdateRequired;

		private SpriteBatch _sb;
		private RenderTarget2D _charRenderTarget;
		private readonly IDataFile<ItemRecord> _itemDataFile;

		public ICharacterRenderProperties RenderProperties
		{
			get { return _characterRenderPropertiesPrivate; }
			set
			{
				if (_characterRenderPropertiesPrivate == value) return;
				_characterRenderPropertiesPrivate = value;
				_textureUpdateRequired = true;
			}
		}

		public Rectangle DrawArea { get; private set; }

		public int TopPixel { get; private set; }
		public new EOGame Game { get; private set; }

		public CharacterRenderer(EOGame game, ICharacterRenderProperties renderProperties)
			: base(game)
		{
			Game = game;
			RenderProperties = renderProperties;

			_itemDataFile = OldWorld.Instance.EIF; //todo: constructor inject
		}

		public override void Initialize()
		{
			_charRenderTarget = new RenderTarget2D(Game.GraphicsDevice,
				Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
				false,
				SurfaceFormat.Color,
				DepthFormat.None);
			_sb = new SpriteBatch(Game.GraphicsDevice);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			FigureOutTopPixel();
			ReloadTextures();
			//todo: set draw area

			base.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			if (!Game.IsActive || !Visible)
				return;

			if (_textureUpdateRequired)
			{
				ReloadTextures();
				DrawToRenderTarget();

				_textureUpdateRequired = false;
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			//todo: check if this is the renderer for the main player
			//		if hidden, draw if: they are not active character and active character is admin

			_sb.Begin();
			DrawToSpriteBatch(_sb);
			_sb.End();

			//todo: draw effect over character

			base.Draw(gameTime);
		}

		public void DrawToSpriteBatch(SpriteBatch spriteBatch)
		{
			_sb.Draw(_charRenderTarget, Vector2.Zero, GetAlphaColor());
		}

		#region Texture Loading

		private void FigureOutTopPixel()
		{
			var spriteForSkin = _spriteCalculator.GetSkinTexture(isBow: false);
			var skinData = spriteForSkin.GetSourceTextureData<Color>();

			int i = 0;
			while (i < skinData.Length && skinData[i].A == 0) i++;

			var firstPixelHeight = i == skinData.Length - 1 ? 0 : i/spriteForSkin.SourceRectangle.Height;
			var genderOffset = RenderProperties.Gender == 0 ? 12 : 13;

			TopPixel = genderOffset + firstPixelHeight;
		}

		private void ReloadTextures()
		{
			_spriteCalculator = new CharacterSpriteCalculator(Game.GFXManager, _characterRenderPropertiesPrivate);
			_textures = new CharacterTextures(_spriteCalculator);

			_textures.RefreshTextures(IsBowEquipped(), IsShieldOnBack());
		}

		#endregion

		#region Drawing

		private void DrawToRenderTarget()
		{
			GraphicsDevice.SetRenderTarget(_charRenderTarget);
			GraphicsDevice.Clear(Color.Transparent);
			_sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

			var characterPropertyRenderers = GetOrderedRenderers();
			foreach (var renderer in characterPropertyRenderers)
				renderer.Render(DrawArea);

			_sb.End();
			GraphicsDevice.SetRenderTarget(null);
		}

		private IEnumerable<ICharacterPropertyRenderer> GetOrderedRenderers()
		{
			var propertyListBuilder = new CharacterPropertyRendererBuilder(_sb, RenderProperties, _textures, _itemDataFile);
			return propertyListBuilder.BuildList(isBowEquipped: IsBowEquipped(),
												 isShieldOnBack: IsShieldOnBack());
		}

		private Color GetAlphaColor()
		{
			return RenderProperties.IsHidden || RenderProperties.IsDead
				? Color.FromNonPremultiplied(255, 255, 255, 128)
				: Color.White;
		}

		#endregion

		#region Conditional Rendering

		private bool IsBowEquipped()
		{
			var itemData = _itemDataFile.Data;
			var weaponInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Weapon &&
														   x.DollGraphic == RenderProperties.WeaponGraphic);

			return weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged;
		}

		private bool IsShieldOnBack()
		{
			var itemData = _itemDataFile.Data;
			var shieldInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Shield &&
														   x.DollGraphic == RenderProperties.ShieldGraphic);

			return shieldInfo != null &&
				   (shieldInfo.Name == "Bag" ||
					shieldInfo.SubType == ItemSubType.Arrows ||
					shieldInfo.SubType == ItemSubType.Wings);
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_charRenderTarget.Dispose();
				_sb.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
