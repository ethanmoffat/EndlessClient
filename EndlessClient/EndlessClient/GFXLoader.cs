using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

using EOLib;

namespace EndlessClient
{
	[Serializable]
	public class GFXLoadException : Exception
	{
		public GFXLoadException(int resource, GFXTypes gfx)
			: base(string.Format("Unable to load graphic {0} from file gfx{1:000}.egf", resource + 100, (int)gfx)) { }
	}

	public enum GFXTypes
	{
		/// <summary>
		/// 001
		/// </summary>
		PreLoginUI = 1,
		/// <summary>
		/// 002
		/// </summary>
		PostLoginUI,
		/// <summary>
		/// 003
		/// </summary>
		MapTiles,
		/// <summary>
		/// 004
		/// </summary>
		MapObjects,
		/// <summary>
		/// 005
		/// </summary>
		MapOverlay,
		/// <summary>
		/// 006
		/// </summary>
		MapWalls,
		/// <summary>
		/// 007
		/// </summary>
		MapWallTop,
		/// <summary>
		/// 008
		/// </summary>
		SkinSprites,
		/// <summary>
		/// 009
		/// </summary>
		MaleHair,
		/// <summary>
		/// 010
		/// </summary>
		FemaleHair,
		/// <summary>
		/// 011
		/// </summary>
		MaleShoes,
		/// <summary>
		/// 012
		/// </summary>
		FemaleShoes,
		/// <summary>
		/// 013
		/// </summary>
		MaleArmor,
		/// <summary>
		/// 014
		/// </summary>
		FemaleArmor,
		/// <summary>
		/// 015
		/// </summary>
		MaleHat,
		/// <summary>
		/// 016
		/// </summary>
		FemaleHat,
		/// <summary>
		/// 017
		/// </summary>
		MaleWeapons,
		/// <summary>
		/// 018
		/// </summary>
		FemaleWeapons,
		/// <summary>
		/// 019
		/// </summary>
		MaleBack,
		/// <summary>
		/// 020
		/// </summary>
		FemaleBack,
		/// <summary>
		/// 021
		/// </summary>
		NPC,
		/// <summary>
		/// 022
		/// </summary>
		Shadows,
		/// <summary>
		/// 023
		/// </summary>
		Items,
		/// <summary>
		/// 024
		/// </summary>
		Spells,
		/// <summary>
		/// 025
		/// </summary>
		SpellIcons
	}

	public static class GFXLoader
	{
		/*** P/Invoke Stuff ***/
		//lpszName is a uint because egf files use MAKEINTRESOURCE which casts the uint resource value to a string pointer
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr LoadImage(IntPtr hinst, uint lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr LoadLibrary(string lpFileName);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool FreeLibrary(IntPtr hModule);
		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		private struct LibraryGraphicPair : IComparable
		{
			private readonly int LibraryNumber;
			private readonly int GraphicNumber;

			public LibraryGraphicPair(int lib, int gfx)
			{
				LibraryNumber = lib;
				GraphicNumber = gfx;
			}

			public int CompareTo(object other)
			{
				if (!(other is LibraryGraphicPair))
					return -1;

				LibraryGraphicPair rhs = (LibraryGraphicPair)other;

				if (rhs.LibraryNumber == LibraryNumber && rhs.GraphicNumber == GraphicNumber)
					return 0;

				return -1;
			}

			public override bool Equals(object obj)
			{
				if (!(obj is LibraryGraphicPair)) return false;
				LibraryGraphicPair other = (LibraryGraphicPair) obj;
				return other.GraphicNumber == GraphicNumber && other.LibraryNumber == LibraryNumber;
			}

			public override int GetHashCode()
			{
				return (LibraryNumber << 16) | GraphicNumber;
			}
		}

		private static readonly Dictionary<LibraryGraphicPair, Texture2D> cache = new Dictionary<LibraryGraphicPair, Texture2D>();

		private static GraphicsDevice device;

		public static void Initialize(GraphicsDevice dev)
		{
			if (device != null)
				throw new ArgumentException("The GFX loader has already been initialized once.");
			device = dev;
		}

		/// <summary>
		/// Disposes all cached textures
		/// </summary>
		public static void Cleanup()
		{
			foreach (Texture2D text in cache.Values)
			{
				text.Dispose();
			}
			cache.Clear();
		}

		/// <summary>
		/// Returns a byte array of image data from a single image within an endless online *.egf file
		/// Image is specified by the library file (GFXTypes) and the resourceName (number)
		/// </summary>
		/// <param name="resourceVal">Name (number) of the image resource</param>
		/// <param name="file">File type to load from</param>
		/// <param name="transparent">Whether or not to make the background black color transparent</param>
		/// <param name="reloadFromFile">True to force reload the gfx from the gfx file, false to use the in-memory cache</param>
		/// <returns>Texture2D containing the image from the *.egf file</returns>
		public static Texture2D TextureFromResource(GFXTypes file, int resourceVal, bool transparent = false, bool reloadFromFile = false)
		{
			Texture2D ret;

			LibraryGraphicPair key = new LibraryGraphicPair((int)file, 100 + resourceVal);
			if (!reloadFromFile && cache.ContainsKey(key))
			{
				return cache[key];
			}
			
			if (cache.ContainsKey(key) && reloadFromFile)
			{
				if (cache[key] != null) cache[key].Dispose();
				cache.Remove(key);
			}

			using (System.IO.MemoryStream mem = new System.IO.MemoryStream())
			{
				using (Bitmap i = BitmapFromResource(file, resourceVal, transparent))
					i.Save(mem, System.Drawing.Imaging.ImageFormat.Png);

				ret = Texture2D.FromStream(device, mem);
			}
			
			//need to double-check that the key isn't already in the cache:
			//  	multiple threads can enter this method simultaneously
			//avoiding a lock because this method is used for every graphic
			if(!cache.ContainsKey(key))
				cache.Add(key, ret);

			return ret;
		}

		private static Bitmap BitmapFromResource(GFXTypes file, int resourceVal, bool transparent)
		{
			if (device == null)
			{
				throw new NullReferenceException("The GFX loader must be initialized with a graphics device by calling GFXLoader.Initialize() in your game class.");
			}

			string number = ((int)file).ToString("D3");
			string fName = System.IO.Path.Combine(new [] { "gfx", "gfx" + number + ".egf" });

			IntPtr library = LoadLibrary(fName);

			if (library == IntPtr.Zero)
			{
				int err = Marshal.GetLastWin32Error();
				throw new Exception(string.Format("Error {1} when loading library {0}\n{2}", number, err, new System.ComponentModel.Win32Exception(err).Message));
			}

			IntPtr image = LoadImage(library, (uint)(100 + resourceVal), 0 /*IMAGE_BITMAP*/, 0, 0, 0x00008000 | 0x00002000 /*LR_DEFAULT*/);

			if (image == IntPtr.Zero)
			{
				throw new GFXLoadException(resourceVal, file);
			}
			Bitmap ret = Image.FromHbitmap(image);

			if (transparent)
			{
				if (file != GFXTypes.FemaleHat && file != GFXTypes.MaleHat)
					ret.MakeTransparent(Color.Black);

				// for hats: 0x080000 is transparent, 0x000000 is supposed to clip hair below it
				if (file == GFXTypes.FemaleHat || file == GFXTypes.MaleHat)
				{
					//ret.MakeTransparent(Color.Black);
					ret.MakeTransparent(Color.FromArgb(0xFF, 0x08, 0x00, 0x00));
				}
			}

			FreeLibrary(library);
			DeleteObject(image);
			return ret;
		}
	}

	//---------------------------------------------------
	// SPRITE SHEET LAYER FOR CHARACTER RENDERING HELPER (and NPCs :) )
	//---------------------------------------------------

	//enums are stored with values that are the actual numbers instead of being indexes.
	//so Standing=1 would refer to image number 101 (graphic no. 0 in game)

	public enum ArmorShieldSpriteType
	{
							//dir1/dir2
		Standing = 1,		//1/2
		WalkFrame1 = 3,		//3/7
		WalkFrame2 = 4,		//4/8
		WalkFrame3 = 5,		//5/9
		WalkFrame4 = 6,		//6/10
		SpellCast = 11,		//11/12
		PunchFrame1 = 13,	//13/15
		PunchFrame2 = 14,	//14/16
		
		//not valid for shields:
		SitChair = 17,		//17/18
		SitGround = 19,		//19/20
		Bow = 21,			//21/22
	}

	public enum WeaponSpriteType
	{
		Standing = 1, //1/2
		WalkFrame1 = 3, //3/7
		WalkFrame2 = 4, //4/8
		WalkFrame3 = 5, //5/9
		WalkFrame4 = 6, //6/10
		SpellCast = 11, //11/12
		SwingFrame1 = 13, //13/15
		SwingFrame2 = 14, //14/16
		UnknownFrame = 17, //17 -- for melee i'm not sure what it does
		//invalid for non-ranged weapons:
		Shooting = 18, //18/19 AND 21/22 have same gfx
	}

	public enum BootsSpriteType
	{
		Standing = 1, //1/2
		WalkFrame1 = 3, //3/7
		WalkFrame2 = 4, //4/8
		WalkFrame3 = 5, //5/9
		WalkFrame4 = 6, //6/10
		Attack = 11, //11/12
		SitChair = 13, //13/14
		SitGround = 15, //15/16
	}
	
	public class EOSpriteSheet
	{
		private readonly Character charRef;
		public EOSpriteSheet(Character charToWatch)
		{
			charRef = charToWatch;
		}

		private CharRenderData _data
		{
			get { return charRef.RenderData; }
		}

		public Texture2D GetArmor(bool isBow = false)
		{
			ArmorShieldSpriteType type = ArmorShieldSpriteType.Standing;
			switch(charRef.State)
			{
				case CharacterActionState.Walking:
					switch (_data.walkFrame)
					{
						case 1: type = ArmorShieldSpriteType.WalkFrame1; break;
						case 2: type = ArmorShieldSpriteType.WalkFrame2; break;
						case 3: type = ArmorShieldSpriteType.WalkFrame3; break;
						case 4: type = ArmorShieldSpriteType.WalkFrame4; break;
					}
					break;
				case CharacterActionState.Attacking:
					if(isBow)
					{
						switch (_data.attackFrame)
						{
							case 1: type = ArmorShieldSpriteType.Bow; break;
							case 2: type = ArmorShieldSpriteType.Standing; break;
						}
					}
					else
					{
						switch (_data.attackFrame)
						{
							case 1: type = ArmorShieldSpriteType.PunchFrame1; break;
							case 2: type = ArmorShieldSpriteType.PunchFrame2; break;
						}
					}
					break;
				case CharacterActionState.SpellCast:
					type = ArmorShieldSpriteType.SpellCast;
					break;
				case CharacterActionState.Sitting:
					switch (_data.sitting)
					{
						case SitState.Chair: 
							type = ArmorShieldSpriteType.SitChair;
							break;
						case SitState.Floor:
							type = ArmorShieldSpriteType.SitGround;
							break;
					}
					break;
			}

			short baseArmorValue = (short)((_data.armor - 1) * 50);
			GFXTypes gfxFile = (_data.gender == 0) ? GFXTypes.FemaleArmor : GFXTypes.MaleArmor;
			int factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 1; //multiplier for the direction faced
			factor *= getFactor(type);
			int gfxNumber = baseArmorValue + (int)type + factor;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public Texture2D GetShield(bool shieldIsOnBack)
		{
			//front shields have one size gfx, back arrows/wings have another size.
			ArmorShieldSpriteType type = ArmorShieldSpriteType.Standing;
			int factor;
			if (!shieldIsOnBack)
			{
				if(charRef.State == CharacterActionState.Walking)
				{
					switch (_data.walkFrame)
					{
						case 1: type = ArmorShieldSpriteType.WalkFrame1; break;
						case 2: type = ArmorShieldSpriteType.WalkFrame2; break;
						case 3: type = ArmorShieldSpriteType.WalkFrame3; break;
						case 4: type = ArmorShieldSpriteType.WalkFrame4; break;
					}
				}
				else if(charRef.State == CharacterActionState.Attacking)
				{
					switch (_data.attackFrame)
					{
						case 1: type = ArmorShieldSpriteType.PunchFrame1; break;
						case 2: type = ArmorShieldSpriteType.PunchFrame2; break;
					}
				}
				else if (charRef.State == CharacterActionState.SpellCast)
				{
					type = ArmorShieldSpriteType.SpellCast;
				}
				else
				{
					//hide shield graphic when sitting
					return null;
				}

				factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 1;
				factor *= getFactor(type);
			}
			else
			{
				//sitting is valid for arrows and wings and bag
				//Standing = 1/2
				//Attacking = 3/4
				//Extra = 5 (unused?)
				if (charRef.State == CharacterActionState.Attacking && _data.attackFrame == 1)
				{
					type = (ArmorShieldSpriteType)3;
				}
				factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 1;
			}

			short baseShieldValue = (short)((_data.shield - 1) * 50);
			GFXTypes gfxFile = _data.gender == 0 ? GFXTypes.FemaleBack : GFXTypes.MaleBack;
			int gfxNumber = baseShieldValue + (int)type + factor;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public Texture2D GetWeapon(bool isBow = false)
		{
			WeaponSpriteType type = WeaponSpriteType.Standing;
			switch(charRef.State)
			{
				case CharacterActionState.Walking:
					switch (_data.walkFrame)
					{
						case 1: type = WeaponSpriteType.WalkFrame1; break;
						case 2: type = WeaponSpriteType.WalkFrame2; break;
						case 3: type = WeaponSpriteType.WalkFrame3; break;
						case 4: type = WeaponSpriteType.WalkFrame4; break;
					}
					break;
				case CharacterActionState.Attacking:
					if (isBow)
					{
						switch (_data.attackFrame)
						{
							case 1: type = WeaponSpriteType.Shooting; break;
							case 2: type = WeaponSpriteType.Standing; break;
						}
					}
					else
					{
						switch (_data.attackFrame)
						{
							case 1: type = WeaponSpriteType.SwingFrame1; break;
							case 2: type = WeaponSpriteType.SwingFrame2; break;
						}
					}
					break;
				case CharacterActionState.SpellCast:
					type = WeaponSpriteType.SpellCast;
					break;
				case CharacterActionState.Sitting: return null; //no weapon when sitting
			}

			short baseWeaponValue = (short)((_data.weapon - 1) * 100);
			GFXTypes gfxFile = _data.gender == 0 ? GFXTypes.FemaleWeapons : GFXTypes.MaleWeapons;
			int factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 1;
			factor *= getFactor(type);
			int gfxNumber = baseWeaponValue + (int)type + factor;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public Texture2D GetBoots(bool isBow = false)
		{
			BootsSpriteType type = BootsSpriteType.Standing;
			switch(charRef.State)
			{
				case CharacterActionState.Walking:
					switch (_data.walkFrame)
					{
						case 1: type = BootsSpriteType.WalkFrame1; break;
						case 2: type = BootsSpriteType.WalkFrame2; break;
						case 3: type = BootsSpriteType.WalkFrame3; break;
						case 4: type = BootsSpriteType.WalkFrame4; break;
					}
					break;
				case CharacterActionState.Attacking:
					if(!isBow && _data.attackFrame > 0 || 
						isBow && _data.attackFrame == 1)
						type = BootsSpriteType.Attack;
					break;
				case CharacterActionState.Sitting:
					switch (_data.sitting)
					{
						case SitState.Chair: type = BootsSpriteType.SitChair; break;
						case SitState.Floor: type = BootsSpriteType.SitGround; break;
					}
					break;
			}
			short baseBootsValue = (short)((_data.boots - 1) * 40);
			GFXTypes gfxFile = _data.gender == 0 ? GFXTypes.FemaleShoes : GFXTypes.MaleShoes;
			int factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 1;
			factor *= getFactor(type);
			int gfxNumber = baseBootsValue + (int)type + factor;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		/// <summary>
		/// Gets the hair texture from the GFX file based on gender, direction, style, and color
		/// </summary>
		/// <param name="refresh">True to refresh from the GFX file, false to use the hair texture cached in this EOSpriteSheet instance</param>
		/// <returns>Texture2D with the hair data</returns>
		public Texture2D GetHair(bool refresh)
		{
			byte turnedOffset = (byte)((_data.facing == EODirection.Left || _data.facing == EODirection.Up) ? 2 : 0);
			GFXTypes gfxFile = (_data.gender == 0) ? GFXTypes.FemaleHair : GFXTypes.MaleHair;
			int gfxNumber = 2 + ((_data.hairstyle - 1) * 40) + (_data.haircolor * 4) + turnedOffset;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true, refresh);
		}

		public Texture2D GetHat()
		{
			short baseHatValue = (short)((_data.hat - 1) * 10);
			GFXTypes gfxFile = _data.gender == 0 ? GFXTypes.FemaleHat : GFXTypes.MaleHat;
			int factor = (_data.facing == EODirection.Down || _data.facing == EODirection.Right) ? 0 : 2;
			int gfxNumber = baseHatValue + factor + 1;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public Texture2D GetSkin(bool isBow, out Microsoft.Xna.Framework.Rectangle skinRect)
		{
			const byte sheetRows = 7;
			byte sheetColumns = 4;
			byte gfxNum = 1;

			//change up which gfx resource to load, and the size of the resource, based on the _data
			if (charRef.State == CharacterActionState.Walking && _data.walkFrame > 0)
			{
				//walking
				gfxNum = 2;
				sheetColumns = 16;
			}
			else if (charRef.State == CharacterActionState.Attacking && _data.attackFrame > 0)
			{
				if (!isBow)
				{
					//attacking
					gfxNum = 3;
					sheetColumns = 8;
				}
				else if(_data.attackFrame == 1) //only 1 frame of bow/gun animation
				{
					gfxNum = 7; //4 columns in this one too
				}
			}
			else if (charRef.State == CharacterActionState.SpellCast)
			{
				gfxNum = 4;
			}
			else if (charRef.State == CharacterActionState.Sitting)
			{
				if (_data.sitting == SitState.Floor) gfxNum = 6;
				else if (_data.sitting == SitState.Chair) gfxNum = 5;
			}
			//similar if statements for spell, emote, etc

			bool rotated = _data.facing == EODirection.Left || _data.facing == EODirection.Up;
			Texture2D sheet = GFXLoader.TextureFromResource(GFXTypes.SkinSprites, gfxNum, true);
			int heightDelta = sheet.Height / sheetRows; //the height of one 'row' in the sheet
			int widthDelta = sheet.Width / sheetColumns; //the width of one 'column' in the sheet
			int section = sheet.Width/4; //each 'section' for a different set of graphics

			int walkExtra = _data.walkFrame > 0 ? widthDelta * (_data.walkFrame - 1) : 0;
			walkExtra = !isBow && _data.attackFrame > 0 ? widthDelta*(_data.attackFrame - 1) : walkExtra;

			skinRect = new Microsoft.Xna.Framework.Rectangle(
				_data.gender * widthDelta * (sheetColumns / 2) + (rotated ? section : 0) + walkExtra,
				_data.race * heightDelta,
				widthDelta,
				heightDelta);

			return sheet;
		}

		private int getFactor(ArmorShieldSpriteType type)
		{
			switch (type)
			{
				case ArmorShieldSpriteType.WalkFrame1:
				case ArmorShieldSpriteType.WalkFrame2:
				case ArmorShieldSpriteType.WalkFrame3:
				case ArmorShieldSpriteType.WalkFrame4:
					return 4;
				case ArmorShieldSpriteType.PunchFrame1:
				case ArmorShieldSpriteType.PunchFrame2:
					return 2;
			}
			return 1;
		}

		private int getFactor(WeaponSpriteType type)
		{
			switch (type)
			{
				case WeaponSpriteType.WalkFrame1:
				case WeaponSpriteType.WalkFrame2:
				case WeaponSpriteType.WalkFrame3:
				case WeaponSpriteType.WalkFrame4:
					return 4;
				case WeaponSpriteType.SwingFrame1:
				case WeaponSpriteType.SwingFrame2:
					return 2;
			}
			return 1;
		}

		private int getFactor(BootsSpriteType type)
		{
			switch (type)
			{
				case BootsSpriteType.WalkFrame1:
				case BootsSpriteType.WalkFrame2:
				case BootsSpriteType.WalkFrame3:
				case BootsSpriteType.WalkFrame4:
					return 4;
			}
			return 1;
		}
	}

	public enum NPCFrame
	{
		//comments are example graphic numbers for goat
		Standing, //341, 343
		StandingFrame1, //DNE for goat - for witch/smith, 942/1062
		WalkFrame1, //345, 349
		WalkFrame2, //346, 350
		WalkFrame3, //347, 351
		WalkFrame4, //348, 352
		Attack1, //353, 355
		Attack2, //354, 356
		//there may be an Attack3 frame, there are 2 extra graphics at this point
	}

	public class EONPCSpriteSheet
	{
		private readonly NPC npc;

		public EONPCSpriteSheet(NPC npcToWatch)
		{
			npc = npcToWatch;
		}

		public Texture2D GetNPCTexture()
		{
			EODirection dir = npc.Direction;
			int baseGfx = (npc.Data.Graphic - 1)*40;
			int offset;
			switch (npc.Frame)
			{
				case NPCFrame.Standing:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 1 : 3;
					break;
				case NPCFrame.StandingFrame1:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 2 : 4;
					break;
				case NPCFrame.WalkFrame1:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 5 : 9;
					break;
				case NPCFrame.WalkFrame2:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 6 : 10;
					break;
				case NPCFrame.WalkFrame3:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 7 : 11;
					break;
				case NPCFrame.WalkFrame4:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 8 : 12;
					break;
				case NPCFrame.Attack1:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 13 : 15;
					break;
				case NPCFrame.Attack2:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 14 : 16;
					break;
				default:
					return null;
			}

			return GFXLoader.TextureFromResource(GFXTypes.NPC, baseGfx + offset, true);
		}
	}
}
