using System;
using System.IO;
using System.Linq;
using AutomaticTypeMapper;
using EOLib;
using EOLib.IO.Actions;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using EOLib.IO.Services;

namespace BatchMap
{
    public static class Program
    {
        private static ITypeRegistry _typeRegistry;
        private static IPubFileProvider _pubProvider;
        private static IMapFileProvider _mapFileProvider;

        private static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: BatchMap.exe <srcmap|srcdir> <dstmap|dstdir> <pubdir>");
                return;
            }

            var srcFilePath = args[0];
            var dstFilePath = args[1];
            var pubFilePath = args[2];
            var singleFileProcess = false;

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

            SetupDependencies();

            try
            {
                var actions = _typeRegistry.Resolve<IPubFileLoadActions>();

                actions.LoadItemFileByName(Path.Combine(pubFilePath, "dat001.eif"));
                actions.LoadNPCFileByName(Path.Combine(pubFilePath, "dtn001.enf"));
            }
            catch
            {
                Console.WriteLine("Error loading pub files!");
                _typeRegistry.Dispose();
                return;
            }

            try
            {
                ProcessFiles(srcFilePath, dstFilePath, singleFileProcess);
            }
            catch (Exception ex)
            {
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception was thrown: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(ex.Message);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nCall stack: \n");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(ex.StackTrace);
            }

            _typeRegistry.Dispose();
        }

        private static void SetupDependencies()
        {
            _typeRegistry = new UnityRegistry("EOLib.IO");
            _typeRegistry.RegisterDiscoveredTypes();

            _pubProvider = _typeRegistry.Resolve<IPubFileProvider>();
            _mapFileProvider = _typeRegistry.Resolve<IMapFileProvider>();
        }

        private static void ProcessFiles(string src, string dst, bool singleFile)
        {
            var mapFileLoadActions = _typeRegistry.Resolve<IMapFileLoadActions>();
            var mapFileSaveService = _typeRegistry.Resolve<IMapFileSaveService>();

            var inFiles = singleFile ? new[] {src} : Directory.GetFiles(src, "*.emf");

            for (int mapIndex = 0; mapIndex < inFiles.Length; ++mapIndex)
            {
                var mapID = new MapPathToIDConverter().ConvertFromPathToID(inFiles[mapIndex]);

                mapFileLoadActions.LoadMapFileByName(inFiles[mapIndex]);
                var mapFile = _mapFileProvider.MapFiles[mapID];
                
                var changesMade = false;

                //todo: find way to store actual input data, since invalid tiles/warps will be auto-removed
                //for (int i = mapFile.TileRows.Count - 1; i >= 0; --i)
                //{
                //    var tr = mapFile.TileRows[i];
                //    for (int j = tr.EntityItems.Count - 1; j >= 0; --j)
                //    {
                //        var tt = tr.EntityItems[j];
                //        if (tt.X > mapFile.Properties.Width || tr.Y > mapFile.Properties.Height)
                //        {
                //            Console.WriteLine("[MAP {0}] Tile {1}x{2} ({3}) is out of map bounds. Removing.",
                //                              mapID, tt.X, tr.Y, Enum.GetName(typeof(TileSpec), tt.Value));
                //            mapFile.RemoveTileAt(tr.Y, tt.X);
                //            changesMade = true;
                //        }
                //    }
                //}

                //for (int i = mapFile.WarpRows.Count - 1; i >= 0; --i)
                //{
                //    var tr = mapFile.WarpRows[i];
                //    for (int j = tr.EntityItems.Count - 1; j >= 0; --j)
                //    {
                //        var tt = tr.EntityItems[j];
                //        if (tt.X > mapFile.Properties.Width || tr.Y > mapFile.Properties.Height)
                //        {
                //            Console.WriteLine("[MAP {0}] Warp {1}x{2} is out of map bounds. Removing.", mapID, tt.X, tr.Y);
                //            mapFile.RemoveWarpAt(tr.Y, tt.X);
                //            changesMade = true;
                //        }
                //    }
                //}

                for(int ndx = mapFile.NPCSpawns.Count - 1; ndx >= 0; --ndx)
                {
                    var npcSpawn = mapFile.NPCSpawns[ndx];
                    var npcRec = _pubProvider.ENFFile[npcSpawn.ID];
                    if (npcSpawn.ID > _pubProvider.ENFFile.Length || npcRec == null)
                    {
                        Console.WriteLine("[MAP {0}] NPC Spawn {1}x{2} uses non-existent NPC #{3}. Removing.", mapID, npcSpawn.X, npcSpawn.Y, npcSpawn.ID);
                        mapFile = mapFile.RemoveNPCSpawn(npcSpawn);
                        changesMade = true;
                        continue;
                    }

                    if (npcSpawn.X > mapFile.Properties.Width || npcSpawn.Y > mapFile.Properties.Height)
                    {
                        Console.WriteLine("[MAP {0}] NPC Spawn {1}x{2} ({3}) is out of map bounds. Removing.", mapID, npcSpawn.X, npcSpawn.Y, npcRec.Name);
                        mapFile = mapFile.RemoveNPCSpawn(npcSpawn);
                        changesMade = true;
                        continue;
                    }

                    if (!TileIsValidNPCSpawnPoint(mapFile, npcSpawn.X, npcSpawn.Y))
                    {
                        Console.WriteLine("[MAP {0}] NPC Spawn {1}x{2} ({3}) is invalid...", mapID, npcSpawn.X, npcSpawn.Y, npcRec.Name);
                        var found = false;
                        for (int row = npcSpawn.Y - 2; row < npcSpawn.Y + 2; ++row)
                        {
                            if (found) break;
                            for (int col = npcSpawn.X - 2; col < npcSpawn.X + 2; ++col)
                            {
                                if (found) break;
                                if (TileIsValidNPCSpawnPoint(mapFile, col, row))
                                {
                                    Console.WriteLine("[MAP {0}] Found valid spawn point. Continuing.", mapID);
                                    found = true;
                                }
                            }
                        }

                        if (!found)
                        {
                            Console.WriteLine("[MAP {0}] NPC couldn't spawn anywhere valid! Removing.", mapID);
                            mapFile = mapFile.RemoveNPCSpawn(npcSpawn);
                            changesMade = true;
                        }
                    }
                }

                for(int ndx = mapFile.Chests.Count - 1; ndx >= 0; --ndx)
                {
                    var chestSpawn = mapFile.Chests[ndx];
                    var rec = _pubProvider.EIFFile[chestSpawn.ItemID];
                    if (chestSpawn.ItemID > _pubProvider.EIFFile.Length || rec == null)
                    {
                        Console.WriteLine("[MAP {0}] Chest Spawn {1}x{2} uses non-existent Item #{3}. Removing.", mapID, chestSpawn.X, chestSpawn.Y, chestSpawn.ItemID);
                        mapFile = mapFile.RemoveChestSpawn(chestSpawn);
                        changesMade = true;
                        continue;
                    }

                    if (chestSpawn.X > mapFile.Properties.Width ||
                        chestSpawn.Y > mapFile.Properties.Height ||
                        mapFile.Tiles[chestSpawn.Y, chestSpawn.X] != TileSpec.Chest)
                    {
                        Console.WriteLine("[MAP {0}] Chest Spawn {1}x{2} points to a non-chest. Removing.", mapID, chestSpawn.X, chestSpawn.Y);
                        mapFile = mapFile.RemoveChestSpawn(chestSpawn);
                        changesMade = true;
                    }
                }

                if (!changesMade)
                {
                    Console.WriteLine("Map {0} processed without any errors. No changes made.", mapID);
                    continue;
                }

                if (mapIndex == 0 && singleFile && inFiles.Length == 1)
                {
                    mapFileSaveService.SaveFile(dst, mapFile);
                    break;
                }

                mapFileSaveService.SaveFile(
                    Path.Combine(dst, $"{mapID,5:D5}.emf"),
                    mapFile);
            }
        }

        private static bool TileIsValidNPCSpawnPoint(IMapFile EMF, int x, int y)
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
