// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Data.BLL;
using EOLib.Graphics;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.UIControls
{
	//has login/delete buttons
	//has character display
	//has admin level icon
	//has character name label
	//has level label
	public class CharacterInfoPanel : XNAControl
	{
		private readonly CharacterControl _characterControl;
		private readonly ISpriteSheet _adminGraphic;

		protected CharacterInfoPanel(int characterIndex)
		{
			//todo: adjust so that the DrawLocation is the top-left of the display box
			DrawLocation = new Vector2(395, 60 + characterIndex*124);

			//create login/delete buttons
		}

		public CharacterInfoPanel(int characterIndex,
								  string characterName, 
								  int level,
								  AdminLevel adminLevel,
								  ICharacterRenderProperties renderProperties)
			: this(characterIndex)
		{
			_characterControl = new CharacterControl(renderProperties);

			var nameLabel = new XNALabel(GetNameLabelLocation(), Constants.FontSize08pt5)
			{
				ForeColor = Constants.BeigeText,
				Text = CapitalizeName(characterName),
				TextAlign = LabelAlignment.MiddleCenter,
				AutoSize = false
			};
			nameLabel.SetParent(this);

			var levelLabel = new XNALabel(GetLevelLabelLocation(), Constants.FontSize08pt75)
			{
				ForeColor = Constants.BeigeText,
				Text = level.ToString()
			};
			levelLabel.SetParent(this);

			_adminGraphic = CreateAdminGraphic(adminLevel);
		}

		public override void Initialize()
		{
			_characterControl.Initialize();

			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			if (!ShouldUpdate())
				return;

			DoUpdateLogic(gameTime);

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			DoDrawLogic(gameTime);

			base.Draw(gameTime);
		}

		protected virtual void LoginButtonClick(object sender, EventArgs e)
		{
			
		}

		protected virtual void DeleteButtonClick(object sender, EventArgs e)
		{

		}

		protected virtual void DoUpdateLogic(GameTime gameTime)
		{

			_characterControl.Update(gameTime);
		}

		protected virtual void DoDrawLogic(GameTime gameTime)
		{
			_characterControl.Draw(gameTime);

			if (_adminGraphic.HasTexture)
			{
				SpriteBatch.Begin();
				SpriteBatch.Draw(_adminGraphic.SheetTexture, GetAdminGraphicLocation(), _adminGraphic.SourceRectangle, Color.White);
				SpriteBatch.End();
			}
		}

		private static Rectangle GetNameLabelLocation()
		{
			return new Rectangle(104, 2, 89, 22);
		}

		private static Rectangle GetLevelLabelLocation()
		{
			return new Rectangle(-32, 75, 1, 1);
		}

		private Vector2 GetAdminGraphicLocation()
		{
			return new Vector2(DrawAreaWithOffset.X + 48, DrawAreaWithOffset.Y + 73);
		}

		private static string CapitalizeName(string name)
		{
			return (char)(name[0] - 32) + name.Substring(1);
		}

		private ISpriteSheet CreateAdminGraphic(AdminLevel adminLevel)
		{
			var adminGraphic = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 22);
			
			switch (adminLevel)
			{
				case AdminLevel.Player:
					return new EmptySpriteSheet();
				case AdminLevel.Guide:
					return new SpriteSheet(adminGraphic, new Rectangle(252, 39, 17, 17));
				case AdminLevel.Guardian:
				case AdminLevel.GM:
				case AdminLevel.HGM:
					return new SpriteSheet(adminGraphic, new Rectangle(233, 39, 17, 17));
				default:
					throw new ArgumentOutOfRangeException("adminLevel", adminLevel, null);
			}
		}
	}

	/// <summary>
	/// This is a no-op class that represents an empty character slot. The buttons don't do anything, and nothing is drawn / updated
	/// </summary>
	public class EmptyCharacterInfoPanel : CharacterInfoPanel
	{
		public EmptyCharacterInfoPanel(int characterIndex) : base(characterIndex)
		{
		}

		public override void Initialize()
		{
		}

		protected override void DoUpdateLogic(GameTime gameTime)
		{
		}

		protected override void DoDrawLogic(GameTime gameTime)
		{
		}

		protected override void LoginButtonClick(object sender, EventArgs e)
		{
		}

		protected override void DeleteButtonClick(object sender, EventArgs e)
		{
		}
	}
}
