using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

using EOLib;

namespace EndlessClient
{
	public class GFXLoadException : Exception
	{
		public GFXLoadException(int resource, GFXTypes gfx)
			: base(string.Format("Unable to load graphic {0} from file gfx{1:000}.egf", resource + 100, (int)gfx)) { }
		public GFXLoadException(int resource, string imageDesc, GFXTypes gfx)
			: base(string.Format("Unable to load graphic {0} ({1}) from file gfx{2:000}.egf", resource + 100, imageDesc, (int)gfx)) { }
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
		MapOveraly,
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

	public class GFXLoader
	{
		/*** P/Invoke Stuff ***/
		//lpszName is a uint because egf files use MAKEINTRESOURCE which casts the uint resource value to a string pointer
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr LoadImage(IntPtr hinst, uint lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr LoadLibrary(string lpFileName);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool FreeLibrary(IntPtr hModule);
		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		private struct LibraryGraphicPair : IComparable
		{
			public int LibraryNumber;
			public int GraphicNumber;

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
		}

		private static Dictionary<LibraryGraphicPair, Texture2D> cache = new Dictionary<LibraryGraphicPair, Texture2D>();

		private static GraphicsDevice device = null;

		public static void Initialize(GraphicsDevice dev)
		{
			if (device != null)
				throw new ArgumentException("The GFX loader has already been initialized once.");
			device = dev;
		}

		/// <summary>
		/// Returns a byte array of image data from a single image within an endless online *.egf file
		/// Image is specified by the library file (GFXTypes) and the resourceName (number)
		/// </summary>
		/// <param name="resourceVal">Name (number) of the image resource</param>
		/// <param name="file">File type to load from</param>
		/// <param name="device">Graphics device in XNA game class</param>
		/// <returns>Texture2D containing the image from the *.egf file</returns>
		public static Texture2D TextureFromResource(GFXTypes file, int resourceVal, bool transparent = false)
		{
			Texture2D ret;

			LibraryGraphicPair key = new LibraryGraphicPair((int)file, 100 + resourceVal);
			if (cache.ContainsKey(key))
			{
				return cache[key];
			}

			using (System.IO.MemoryStream mem = new System.IO.MemoryStream())
			{
				using (Bitmap i = GFXLoader.BitmapFromResource(file, resourceVal, transparent))
					i.Save(mem, System.Drawing.Imaging.ImageFormat.Png);

				ret = Texture2D.FromStream(device, mem);
			}

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
			string fName = System.IO.Path.Combine(new string[] { "gfx", "gfx" + number + ".egf" });

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
			Bitmap ret = Bitmap.FromHbitmap(image);

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
	// SPRITE SHEET LAYER FOR CHARACTER RENDERING HELPER
	//---------------------------------------------------

	//enums are stored with values that are the actual numbers instead of being indexes.
	//so Standing=1 would refer to image number 101 (graphic no. 0 in game)

	public enum ArmorShieldSpriteType
	{
		Standing = 1, //1/2
		WalkFrame1 = 3, //3/7
		WalkFrame2 = 4, //4/8
		WalkFrame3 = 5, //5/9
		WalkFrame4 = 6, //6/10
		SpellCast = 11, //11/12
		PunchFrame1 = 13, //13/15
		PunchFrame2 = 14, //14/16
		SitChar = 17, //17/18
		SitGround = 19, //19/20
		Bow = 21, //21/22
	}

	public enum WeaponSpriteType //rangedweaponspritetype?
	{
		Standing = 1, //1/2
		WalkFrame1 = 3, //3/7
		WalkFrame2 = 4, //4/8
		WalkFrame3 = 5, //5/9
		WalkFrame4 = 6, //6/10
		SpellCast = 11, //11/12
		SwingFrame1 = 13, //13/15
		SwingFrame2 = 14, //14/16
		//Ranged = ???
	}

	public enum BootsSpriteType
	{
		Standing = 1,
		WalkFrame1 = 3,
		WalkFrame2 = 4,
		WalkFrame3 = 5,
		WalkFrame4 = 6,
		Attack = 11,
		SitChar = 13,
		SitGround = 15,
	}
	
	public static class EOSpriteSheet
	{
		public static Texture2D GetArmor(ArmorShieldSpriteType type, EODirection dir, short baseArmorValue, byte gender)
		{
			baseArmorValue = (short)((baseArmorValue - 1) * 50);
			GFXTypes gfxFile = (gender == 0) ? GFXTypes.FemaleArmor : GFXTypes.MaleArmor;
			int factor = (dir == EODirection.Down || dir == EODirection.Right) ? 0 : 1; //multiplier for the direction faced
			factor *= getFactor(type);
			int gfxNumber = baseArmorValue + (int)type + factor;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public static Texture2D GetShield(ArmorShieldSpriteType type, EODirection dir, short baseShieldValue, byte gender)
		{
			baseShieldValue = (short)((baseShieldValue - 1) * 50);
			GFXTypes gfxFile = gender == 0 ? GFXTypes.FemaleBack : GFXTypes.MaleBack;
			int factor = (dir == EODirection.Down || dir == EODirection.Right) ? 0 : 1;
			factor *= getFactor(type);
			int gfxNumber = baseShieldValue + (int)type + factor;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public static Texture2D GetWeapon(WeaponSpriteType type, EODirection dir, short baseWeaponValue, byte gender)
		{
			baseWeaponValue = (short)((baseWeaponValue - 1) * 100);
			GFXTypes gfxFile = gender == 0 ? GFXTypes.FemaleWeapons : GFXTypes.MaleWeapons;
			int factor = (dir == EODirection.Down || dir == EODirection.Right) ? 0 : 1;
			factor *= getFactor(type);
			int gfxNumber = baseWeaponValue + (int)type + factor;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public static Texture2D GetBoots(BootsSpriteType type, EODirection dir, short baseBootsValue, byte gender)
		{
			baseBootsValue = (short)((baseBootsValue - 1) * 40);
			GFXTypes gfxFile = gender == 0 ? GFXTypes.FemaleShoes : GFXTypes.MaleShoes;
			int factor = (dir == EODirection.Down || dir == EODirection.Right) ? 0 : 1;
			factor *= getFactor(type);
			int gfxNumber = baseBootsValue + (int)type + factor;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public static Texture2D GetHair(EODirection dir, short hairType, short hairColor, byte gender)
		{
			byte turnedOffset = (byte)((dir == EODirection.Left || dir == EODirection.Up) ? 2 : 0);
			GFXTypes gfxFile = (gender == 0) ? GFXTypes.FemaleHair : GFXTypes.MaleHair;
			int gfxNumber = 2 + ((hairType - 1) * 40) + (hairColor * 4) + turnedOffset;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public static Texture2D GetHat(EODirection dir, short baseHatValue, byte gender)
		{
			baseHatValue = (short)((baseHatValue - 1) * 10);
			GFXTypes gfxFile = gender == 0 ? GFXTypes.FemaleHat : GFXTypes.MaleHat;
			int factor = (dir == EODirection.Down || dir == EODirection.Right) ? 0 : 2;
			int gfxNumber = baseHatValue + factor + 1;
			return GFXLoader.TextureFromResource(gfxFile, gfxNumber, true);
		}

		public static Texture2D GetSkin(EODirection dir, short race, byte gender)
		{
			bool rotated = dir == EODirection.Left || dir == EODirection.Up;
			Texture2D sheet = GFXLoader.TextureFromResource(GFXTypes.SkinSprites, 1, true);
			int heightDelta = sheet.Height / 7;
			int widthDelta = sheet.Width / 4;
			Microsoft.Xna.Framework.Rectangle characterSkin = new Microsoft.Xna.Framework.Rectangle(gender * widthDelta * 2 + (rotated ? widthDelta : 0),
				race * heightDelta,
				widthDelta,
				heightDelta);

			XNA.Color[] data = new XNA.Color[characterSkin.Width * characterSkin.Height];
			sheet.GetData<XNA.Color>(0, characterSkin, data, 0, data.Length);

			Texture2D ret = new Texture2D(sheet.GraphicsDevice, characterSkin.Width, characterSkin.Height);
			ret.SetData(data);
			return ret;
		}

		private static int getFactor(ArmorShieldSpriteType type)
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

		private static int getFactor(WeaponSpriteType type)
		{
			return 1; //figure these out later
		}

		private static int getFactor(BootsSpriteType type)
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
}
