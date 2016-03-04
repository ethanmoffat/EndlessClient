// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
	public enum TileSpec : byte
	{
		Wall = 0,
		ChairDown = 1,
		ChairLeft = 2,
		ChairRight = 3,
		ChairUp = 4,
		ChairDownRight = 5,
		ChairUpLeft = 6,
		ChairAll = 7,
		JammedDoor = 8,
		Chest = 9,
		BankVault = 16,
		NPCBoundary = 17,
		MapEdge = 18,
		FakeWall = 19,
		Board1 = 20,
		Board2 = 21,
		Board3 = 22,
		Board4 = 23,
		Board5 = 24,
		Board6 = 25,
		Board7 = 26,
		Board8 = 27,
		Jukebox = 28,
		Jump = 29,
		Water = 30,
		Arena = 32,
		AmbientSource = 33,
		SpikesTimed = 34,
		SpikesStatic = 35,
		SpikesTrap = 36,

		None = 255
	}
}