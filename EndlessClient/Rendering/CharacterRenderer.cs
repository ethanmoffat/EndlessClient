// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EndlessClient.Rendering.Sprites;
using EOLib.Data.BLL;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
	public class CharacterRenderer : DrawableGameComponent, ICharacterRenderer
	{
		private class CharacterTextures
		{
			private readonly ICharacterSpriteCalculator _calc;

			internal Texture2D Boots { get; private set; }
			internal Texture2D Armor { get; private set; }
			internal Texture2D Hat { get; private set; }
			internal Texture2D Shield { get; private set; }
			internal Texture2D Weapon { get; private set; }

			internal Texture2D Hair { get; private set; }
			internal ISpriteSheet Skin { get; private set; }

			internal ISpriteSheet Emote { get; private set; }
			internal ISpriteSheet Face { get; private set; }

			internal CharacterTextures(ICharacterSpriteCalculator calc)
			{
				_calc = calc;
			}

			internal void RefreshTextures(bool bowIsEquipped, bool shieldIsOnBack)
			{
				Boots = _calc.GetBootsTexture(bowIsEquipped).SheetTexture;
				Armor = _calc.GetArmorTexture(bowIsEquipped).SheetTexture;
				Hat = _calc.GetHatTexture().SheetTexture;
				Shield = _calc.GetShieldTexture(shieldIsOnBack).SheetTexture;
				Weapon = _calc.GetWeaponTexture(bowIsEquipped).SheetTexture;

				Hair = _calc.GetHairTexture().SheetTexture;
				Skin = _calc.GetSkinTexture(bowIsEquipped);
				Emote = _calc.GetEmoteTexture();
				Face = _calc.GetFaceTexture();
			}
		}

		private ICharacterSpriteCalculator _spriteCalculator;
		private ICharacterRenderProperties _characterRenderPropertiesPrivate;
		private CharacterTextures _textures;

		public ICharacterRenderProperties RenderProperties
		{
			get { return _characterRenderPropertiesPrivate; }
			set
			{
				if (_characterRenderPropertiesPrivate == value) return;
				_characterRenderPropertiesPrivate = value;
				_spriteCalculator = new CharacterSpriteCalculator(Game.GFXManager, _characterRenderPropertiesPrivate);
				_textures = new CharacterTextures(_spriteCalculator);
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
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			FigureOutTopPixel();
			ReloadTextures();

			base.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			//load textures for rendering from GFX if update required
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			//get textures based on render properties and draw
			base.Draw(gameTime);
		}

		#region Texture Loading

		private void FigureOutTopPixel()
		{
			var spriteForSkin = _spriteCalculator.GetSkinTexture(false);
			var skinData = spriteForSkin.GetSourceTextureData<Color>();

			int i = 0;
			while (i < skinData.Length && skinData[i].A == 0) i++;

			var firstPixelHeight = i == skinData.Length - 1 ? 0 : i/spriteForSkin.SourceRectangle.Height;
			var genderOffset = RenderProperties.Gender == 0 ? 12 : 13;

			TopPixel = genderOffset + firstPixelHeight;
		}

		private void ReloadTextures()
		{
			//todo: constructor inject EIF instead of using OldWorld in helpers
			_textures.RefreshTextures(IsBowEquipped(), IsShieldOnBack());
		}

		private bool IsBowEquipped()
		{
			var itemData = OldWorld.Instance.EIF.Data;
			var weaponInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Weapon &&
			                                               x.DollGraphic == RenderProperties.WeaponGraphic);

			return weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged;
		}

		private bool IsShieldOnBack()
		{
			var itemData = OldWorld.Instance.EIF.Data;
			var shieldInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Shield &&
			                                               x.DollGraphic == RenderProperties.ShieldGraphic);

			return shieldInfo != null &&
			       (shieldInfo.Name == "Bag" ||
			        shieldInfo.SubType == ItemSubType.Arrows ||
			        shieldInfo.SubType == ItemSubType.Wings);
		}

		#endregion
	}
}
