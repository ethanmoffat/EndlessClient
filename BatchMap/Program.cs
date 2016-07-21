// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.IO;
using System.Linq;
using EOLib.IO.Map;
using EOLib.IO.OldMap;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using IMapFile = EOLib.IO.OldMap.IMapFile;

namespace BatchMap
{
    public static class Program
    {
        private static EIFFile EIF;
        private static ENFFile ENF;

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

            EIF = new EIFFile();
            ENF = new ENFFile();
            try
            {
                EIF.DeserializeFromByteArray(File.ReadAllBytes(Path.Combine(pubFilePath, "dat001.eif")), new NumberEncoderService());
                ENF.DeserializeFromByteArray(File.ReadAllBytes(Path.Combine(pubFilePath, "dtn001.enf")), new NumberEncoderService());
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
                IMapFile EMF = new MapFile();
                EMF.Load(inFiles[map]);
                var changesMade = false;

                var startOfMapID = inFiles[map].Contains('\\') ? inFiles[map].LastIndexOf('\\') + 1 : 0;
                var lengthOfMapID = inFiles[map].Length - inFiles[map].LastIndexOf('\\') - 1;
                var mapID = inFiles[map].Substring(startOfMapID, lengthOfMapID);

                for (int i = EMF.TileRows.Count - 1; i >= 0; --i)
                {
                    var tr = EMF.TileRows[i];
                    for (int j = tr.EntityItems.Count - 1; j >= 0; --j)
                    {
                        var tt = tr.EntityItems[j];
                        if (tt.X > EMF.Properties.Width || tr.Y > EMF.Properties.Height)
                        {
                            Console.WriteLine("[MAP {3}] Tile {0}x{1} ({2}) is out of map bounds. Removing.",
                                tt.X, tr.Y, Enum.GetName(typeof(TileSpec), tt.Value), mapID);
                            EMF.RemoveTileAt(tr.Y, tt.X);
                            changesMade = true;
                        }
                    }
                }

                for (int i = EMF.WarpRows.Count - 1; i >= 0; --i)
                {
                    var tr = EMF.WarpRows[i];
                    for (int j = tr.EntityItems.Count - 1; j >= 0; --j)
                    {
                        var tt = tr.EntityItems[j];
                        if (tt.X > EMF.Properties.Width || tr.Y > EMF.Properties.Height)
                        {
                            Console.WriteLine("[MAP {2}] Warp {0}x{1} is out of map bounds. Removing.", tt.X, tr.Y, mapID);
                            EMF.RemoveWarpAt(tr.Y, tt.X);
                            changesMade = true;
                        }
                    }
                }

                for(int i = EMF.NPCSpawns.Count - 1; i >= 0; --i)
                {
                    var npc = EMF.NPCSpawns[i];
                    var npcRec = ENF[npc.NpcID];
                    if (npc.NpcID > ENF.Data.Count || npcRec == null)
                    {
                        Console.WriteLine("[MAP {0}] NPC Spawn {1}x{2} uses non-existent NPC #{3}. Removing.", mapID, npc.X, npc.Y, npc.NpcID);
                        EMF.NPCSpawns.RemoveAt(i);
                        changesMade = true;
                        continue;
                    }

                    if (npc.X > EMF.Properties.Width || npc.Y > EMF.Properties.Height)
                    {
                        Console.WriteLine("[MAP {0}] NPC Spawn {1}x{2} ({3}) is out of map bounds. Removing.", mapID, npc.X, npc.Y, npcRec.Name);
                        EMF.NPCSpawns.RemoveAt(i);
                        changesMade = true;
                        continue;
                    }

                    if (!CheckTile(EMF, npc.X, npc.Y))
                    {
                        Console.WriteLine("[MAP {0}] NPC Spawn {1}x{2} ({3}) is invalid...", mapID, npc.X, npc.Y, npcRec.Name);
                        var found = false;
                        for (int row = npc.Y - 2; row < npc.Y + 2; ++row)
                        {
                            if (found) break;
                            for (int col = npc.X - 2; col < npc.X + 2; ++col)
                            {
                                if (found) break;
                                if (CheckTile(EMF, col, row))
                                {
                                    Console.WriteLine("[MAP {0}] Found valid spawn point. Continuing.", mapID);
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
                    var chest = EMF.Chests[i];
                    var rec = EIF[chest.ItemID];
                    if (chest.ItemID > EIF.Data.Count || rec == null)
                    {
                        Console.WriteLine("[MAP {0}] Chest Spawn {1}x{2} uses non-existent Item #{3}. Removing.", mapID, chest.X, chest.Y, chest.ItemID);
                        EMF.Chests.RemoveAt(i);
                        changesMade = true;
                        continue;
                    }

                    if (chest.X > EMF.Properties.Width ||
                        chest.Y > EMF.Properties.Height ||
                        EMF.Tiles[chest.Y, chest.X] != TileSpec.Chest)
                    {
                        Console.WriteLine("[MAP {0}] Chest Spawn {1}x{2} points to a non-chest. Removing.", mapID, chest.X, chest.Y);
                        EMF.Chests.RemoveAt(i);
                        changesMade = true;
                    }
                }

                if (!changesMade)
                {
                    Console.WriteLine("Map {0} processed without any errors. No changes made.", mapID);
                    continue;
                }

                if (map == 0 && singleFile && inFiles.Length == 1)
                {
                    EMF.Save(dst);
                    break;
                }

                EMF.Save(Path.Combine(dst, mapID));
            }
        }

        private static bool CheckTile(IMapFile EMF, int x, int y)
        {
            if (EMF.Warps[y, x] != null)
                return false;

            switch (EMF.Tiles[y, x])
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

            return true;
        }
    }
}
