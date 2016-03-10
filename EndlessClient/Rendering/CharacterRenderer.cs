// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Sprites;
using EOLib;
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
			//todo: add checks for empty sprite sheet / null textures before creating renderers

			var rendererList = new List<ICharacterPropertyRenderer>();

			if (IsShieldBehindCharacter())
				rendererList.Add(new ShieldRenderer(_sb, RenderProperties, _textures.Shield));

			if (IsWeaponBehindCharacter())
				rendererList.Add(new WeaponRenderer(_sb, RenderProperties, _textures.Weapon));

			rendererList.Add(new SkinRenderer(_sb, RenderProperties, _textures.Skin));
			if (IsCharacterDoingEmote())
			{
				rendererList.Add(new FaceRenderer(_sb, RenderProperties, _textures.Face));

				//todo: the emote should probably be moved to a higher level, since this renderer should JUST be the character
				//however - the way things are segmented, this probably won't be possible (since emotes are contained within this class)
				rendererList.Add(new EmoteRenderer(_sb, RenderProperties, _textures.Emote));
			}

			rendererList.Add(new BootsRenderer(_sb, RenderProperties, _textures.Boots));
			rendererList.Add(new ArmorRenderer(_sb, RenderProperties, _textures.Armor));

			if (!rendererList.OfType<WeaponRenderer>().Any())
				rendererList.Add(new WeaponRenderer(_sb, RenderProperties, _textures.Weapon));

			var hairOnTopOfHat = new ICharacterPropertyRenderer[]
			{
				new HatRenderer(_sb, RenderProperties, _textures.Hat),
				new HairRenderer(_sb, RenderProperties, _textures.Hair)
			};
			rendererList.AddRange(IsHairOnTopOfHat() ? hairOnTopOfHat : hairOnTopOfHat.Reverse());

			if (!rendererList.OfType<ShieldRenderer>().Any())
				rendererList.Add(new ShieldRenderer(_sb, RenderProperties, _textures.Hair));

			return rendererList;
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

		private bool IsShieldBehindCharacter()
		{
			//todo: this may or may not work because boolean logic is confusing

			bool facingDownOrRight = RenderProperties.Direction == EODirection.Down ||
			                         RenderProperties.Direction == EODirection.Right;

			return (facingDownOrRight && IsShieldOnBack()) || !IsShieldOnBack();
		}

		private bool IsWeaponBehindCharacter()
		{
			//todo: this may or may not work because boolean logic is confusing

			var weaponInfo = _itemDataFile.Data.Single(
				x => x.Type == ItemType.Weapon &&
				     x.DollGraphic == RenderProperties.WeaponGraphic);

			// weapon will be drawn later if:
			//  - attack frame is 2 (or data is NULL in which case ignore)
			//      - When attack frame is 0, want it behind character's hand
			//  - Direction is right or down
			//  - Weapon subtype is not Ranged (or weapon info is NULL in which case ignore)
			var pass1 = RenderProperties.AttackFrame < 2;
			var pass2 = RenderProperties.Direction != EODirection.Down && RenderProperties.Direction != EODirection.Right;
			var pass3 = weaponInfo.SubType == ItemSubType.Ranged;

			return pass1 && pass2 && pass3;
		}

		private bool IsCharacterDoingEmote()
		{
			return RenderProperties.CurrentAction == CharacterActionState.Emote &&
			       RenderProperties.EmoteFrame > 0;
		}

		private bool IsHairOnTopOfHat()
		{
			//todo: i might have this backwards...
			
			var hatInfo = _itemDataFile.Data.SingleOrDefault(
				x => x.Type == ItemType.Hat &&
				     x.DollGraphic == RenderProperties.HatGraphic);

			return hatInfo != null && hatInfo.SubType == ItemSubType.FaceMask;
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
