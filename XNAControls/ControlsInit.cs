
using System;
using Microsoft.Xna.Framework;

namespace XNAControls
{
	public static class XNAControls
	{
		/// <summary>
		/// Game reference. For internal use only.
		/// </summary>
		internal static Game Game { get; private set; }

		/// <summary>
		/// Determines state of library: true if all ok, false otherwise
		/// <para>For internal use only.</para>
		/// </summary>
		internal static bool IsInitialized { get; private set; }
		
		/// <summary>
		/// Initialize the XNAControls library
		/// </summary>
		/// <param name="mainInstance">The XNA Game instance being used for controls. This is automatically passed to each control that is constructed.</param>
		public static void Initialize(Game mainInstance)
		{
			if(mainInstance == null)
				throw new ArgumentNullException("mainInstance", "The game instance main not be null");

			Game = mainInstance;
			IsInitialized = true;
		}

		/// <summary>
		/// Re-initialize the XNAControls library with a new Game instance. All existing controls are closed and destroyed.
		/// </summary>
		/// <param name="newMainInstance"></param>
		public static void ReInitialize(Game newMainInstance)
		{
			if (!IsInitialized)
			{
				throw new InvalidOperationException("Unable to reinitialize library without first initializing it!");
			}

			if(newMainInstance == null)
				throw new ArgumentNullException("newMainInstance", "The game instance may not be null!");

			IsInitialized = false;

			try
			{
				//remove from game's components list so they aren't updated/drawn by framework while destroying them
				for (int i = Game.Components.Count - 1; i >= 0; i--)
				{
					XNAControl tmp = Game.Components[i] as XNAControl;
					if (tmp != null)
					{
						tmp.Close();
						tmp.Dispose();
					}
				}
			}
			catch(Exception ex)
			{
				throw new Exception("Error reinitializing the library!", ex);
			}

			IsInitialized = true;
			Game = newMainInstance;
		}
	}
}