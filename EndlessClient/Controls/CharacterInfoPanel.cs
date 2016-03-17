// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Data.BLL;
using EOLib.Graphics;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Controls
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
		//private readonly XNAButton _loginButton, _deleteButton;

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

			_characterControl.Update(gameTime);

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			SpriteBatch.Begin();

			SpriteBatch.Draw(_adminGraphic.SheetTexture, GetAdminGraphicLocation(), _adminGraphic.SourceRectangle, Color.White);

			SpriteBatch.End();

			base.Draw(gameTime);
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
			
			Rectangle adminRect;
			if (adminLevel == AdminLevel.Guide)
				adminRect = new Rectangle(252, 39, 17, 17);
			else if (adminLevel == AdminLevel.Guide ||
					 adminLevel == AdminLevel.GM ||
					 adminLevel == AdminLevel.HGM)
				adminRect = new Rectangle(233, 39, 17, 17);
			else
				return new SpriteSheet(adminGraphic);

			return new SpriteSheet(adminGraphic, adminRect);
		}
	}

	/// <summary>
	/// This is a no-op class that represents an empty character slot.
	/// </summary>
	public class EmptyCharacterInfoPanel : CharacterInfoPanel
	{
		public EmptyCharacterInfoPanel(int characterIndex)
			: base(characterIndex) { }

		public override void Initialize() { }

		public override void Update(GameTime gameTime) { }

		public override void Draw(GameTime gameTime) { }
	}
}
