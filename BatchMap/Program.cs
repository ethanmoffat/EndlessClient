// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.IO;
using System.Linq;
using EOLib.IO;

namespace BatchMap
{
	public static class Program
	{
		private static ItemFile EIF;
		private static NPCFile ENF;

		private static void Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine("Usage: BatchMap.exe <srcmap|srcdir> <dstmap|dstdir> <pubdir>");
				return;
			}

			string srcFilePath = args[0];
			string dstFilePath = args[1];
			string pubFilePath = args[2];
			bool singleFileProcess = false;

			if (srcFilePath.ToLower().EndsWith(".emf") && !dstFilePath.ToLower().EndsWith(".emf"))
			{
				Console.WriteLine("Invalid: single map cannot be processed into output directory. Specify destination emf file.");
				return;
			}
			
			if (dstFilePath.ToLower().EndsWith(".emf") && !srcFilePath.ToLower().EndsWith(".emf"))
			{
				Console.WriteLine("Invalid: map directory cannot be processed into single output map. Specify destination output directory.");
				return;
			}

			if (srcFilePath.ToLower().EndsWith(".emf") && dstFilePath.ToLower().EndsWith(".emf"))
			{
				singleFileProcess = true;
				if (!File.Exists(srcFilePath))
				{
					Console.WriteLine("Invalid input: input file does not exist!");
					return;
				}

				if (File.Exists(dstFilePath))
				{
					char inp;
					do
					{
						Console.Write("Destination file exists. Overwrite? [y/n] ");
						string input = Console.ReadLine() ?? "";
						inp = input.Length > 0 ? input[0] : ' ';
					} while (inp != 'y' && inp != 'n' && inp != 'Y' && inp != 'N');

					if (inp == 'n' || inp == 'N')
					{
						Console.WriteLine("Will not overwrite existing file. Exiting.");
						return;
					}
				}
			}
			else
			{
				if (!Directory.Exists(srcFilePath) || Directory.GetFiles(srcFilePath, "*.emf").Length == 0)
				{
					Console.WriteLine("Invalid input: source directory does not exist or is missing maps!");
					return;
				}

				if (Directory.Exists(dstFilePath) && Directory.GetFiles(dstFilePath, "*.emf").Length > 0)
				{
					char inp;
					do
					{
						Console.WriteLine("Destination directory contains emf files. Overwrite? [y/n] ");
						string input = Console.ReadLine() ?? "";
						inp = input.Length > 0 ? input[0] : ' ';
					} while (inp != 'y' && inp != 'n' && inp != 'Y' && inp != 'N');

					if (inp == 'n' || inp == 'N')
					{
						Console.WriteLine("Will not overwrite existing files. Exiting.");
						return;
					}
				}
				else if (!Directory.Exists(dstFilePath))
				{
					Directory.CreateDirectory(dstFilePath);
				}
			}

			try
			{
				EIF = new ItemFile(Path.Combine(pubFilePath, "dat001.eif"));
				ENF = new NPCFile(Path.Combine(pubFilePath, "dtn001.enf"));
			}
			catch
			{
				Console.WriteLine("Error loading pub files!");
				return;
			}

			ProcessFiles(srcFilePath, dstFilePath, singleFileProcess);
		}

		private static void ProcessFiles(string src, string dst, bool singleFile)
		{
			string[] inFiles = singleFile ? new[] {src} : Directory.GetFiles(src, "*.emf");

			for (int map = 0; map < inFiles.Length; ++map)
			{
				MapFile EMF = new MapFile(inFiles[map]);
				bool changesMade = false;

				string lastPart = inFiles[map].Substring(inFiles[map].Contains('\\') ? inFiles[map].LastIndexOf('\\') + 1 : 0,
					inFiles[map].Length - inFiles[map].LastIndexOf('\\') - 1);

				for (int i = EMF.TileRows.Count - 1; i >= 0; --i)
				{
					TileRow tr = EMF.TileRows[i];
					for (int j = tr.tiles.Count - 1; j >= 0; --j)
					{
						Tile tt = tr.tiles[j];
						if (tt.x > EMF.Width || tr.y > EMF.Height)
						{
							Console.WriteLine("[MAP {3}] Tile {0}x{1} ({2}) is out of map bounds. Removing.", tt.x, tr.y, Enum.GetName(typeof(TileSpec), tt.spec), lastPart);
							tr.tiles.RemoveAt(j);
							changesMade = true;
						}
					}
				}

				for (int i = EMF.WarpRows.Count - 1; i >= 0; --i)
				{
					WarpRow tr = EMF.WarpRows[i];
					for (int j = tr.tiles.Count - 1; j >= 0; --j)
					{
						Warp tt = tr.tiles[j];
						if (tt.x > EMF.Width || tr.y > EMF.Height)
						{
							Console.WriteLine("[MAP {2}] Warp {0}x{1} is out of map bounds. Removing.", tt.x, tr.y, lastPart);
							tr.tiles.RemoveAt(j);
							changesMade = true;
						}
					}
				}

				for(int i = EMF.NPCSpawns.Count - 1; i >= 0; --i)
				{
					NPCSpawn npc = EMF.NPCSpawns[i];
					NPCRecord npcRec = (NPCRecord)ENF.Data.Find(rec => ((NPCRecord) rec).ID == npc.id);
					if (npc.id > ENF.Data.Count || npcRec == null)
					{
						Console.WriteLine("[MAP {0}] NPC Spawn {1}x{2} uses non-existent NPC #{3}. Removing.", lastPart, npc.x, npc.y, npc.id);
						EMF.NPCSpawns.RemoveAt(i);
						changesMade = true;
						continue;
					}

					if (npc.x > EMF.Width || npc.y > EMF.Height)
					{
						Console.WriteLine("[MAP {0}] NPC Spawn {1}x{2} ({3}) is out of map bounds. Removing.", lastPart, npc.x, npc.y, npcRec.Name);
						EMF.NPCSpawns.RemoveAt(i);
						changesMade = true;
						continue;
					}

					if (!CheckTile(EMF, npc.x, npc.y))
					{
						Console.WriteLine("[MAP {0}] NPC Spawn {1}x{2} ({3}) is invalid...", lastPart, npc.x, npc.y, npcRec.Name);
						bool found = false;
						for (int row = npc.y - 2; row < npc.y + 2; ++row)
						{
							if (found) break;
							for (int col = npc.x - 2; col < npc.x + 2; ++col)
							{
								if (found) break;
								if (CheckTile(EMF, col, row))
								{
									Console.WriteLine("[MAP {0}] Found valid spawn point. Continuing.", lastPart);
									found = true;
								}
							}
						}

						if (!found)
						{
							Console.WriteLine("[MAP {0}] NPC couldn't spawn anywhere valid! Removing.");
							EMF.NPCSpawns.RemoveAt(i);
							changesMade = true;
						}
					}
				}

				for (int i = EMF.Chests.Count - 1; i >= 0; --i)
				{
					MapChest chest = EMF.Chests[i];
					ItemRecord rec = EIF.GetItemRecordByID(chest.item);
					if (chest.item > EIF.Data.Count || rec == null)
					{
						Console.WriteLine("[MAP {0}] Chest Spawn {1}x{2} uses non-existent Item #{3}. Removing.", lastPart, chest.x, chest.y, chest.item);
						EMF.Chests.RemoveAt(i);
						changesMade = true;
						continue;
					}

					if (chest.x > EMF.Width || chest.y > EMF.Height ||
						(EMF.TileLookup[chest.y, chest.x] ?? new Tile { spec = TileSpec.Wall }).spec != TileSpec.Chest)
					{
						Console.WriteLine("[MAP {0}] Chest Spawn {1}x{2} points to a non-chest. Removing.", lastPart, chest.x, chest.y);
						EMF.Chests.RemoveAt(i);
						changesMade = true;
					}
				}

				if (!changesMade)
				{
					Console.WriteLine("Map {0} processed without any errors. No changes made.", lastPart);
					continue;
				}

				if (map == 0 && singleFile && inFiles.Length == 1)
				{
					EMF.Save(dst);
					break;
				}

				EMF.Save(Path.Combine(dst, lastPart));
			}
		}

		private static bool CheckTile(MapFile EMF, int x, int y)
		{
			if (EMF.WarpLookup[y, x] != null)
				return false;

			if (EMF.TileLookup[y, x] != null)
			{
				switch (EMF.TileLookup[y, x].spec)
				{
					case TileSpec.Wall:
					case TileSpec.ChairDown:
					case TileSpec.ChairLeft:
					case TileSpec.ChairRight:
					case TileSpec.ChairUp:
					case TileSpec.ChairDownRight:
					case TileSpec.ChairUpLeft:
					case TileSpec.ChairAll:
					case TileSpec.Chest:
					case TileSpec.BankVault:
					case TileSpec.NPCBoundary:
					case TileSpec.MapEdge:
					case TileSpec.Board1:
					case TileSpec.Board2:
					case TileSpec.Board3:
					case TileSpec.Board4:
					case TileSpec.Board5:
					case TileSpec.Board6:
					case TileSpec.Board7:
					case TileSpec.Board8:
					case TileSpec.Jukebox:
						return false;
				}
			}

			return true;
		}
	}
}
