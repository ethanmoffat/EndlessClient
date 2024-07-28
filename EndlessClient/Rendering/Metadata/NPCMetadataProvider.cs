using AutomaticTypeMapper;
using EndlessClient.Rendering.Metadata.Models;
using System.Collections.Generic;

namespace EndlessClient.Rendering.Metadata;

[AutoMappedType(IsSingleton = true)]
public class NPCMetadataProvider : IMetadataProvider<NPCMetadata>
{
    public IReadOnlyDictionary<int, NPCMetadata> DefaultMetadata => _metadata;

    private readonly Dictionary<int, NPCMetadata> _metadata;
    private readonly IGFXMetadataLoader _metadataLoader;

    public NPCMetadataProvider(IGFXMetadataLoader metadataLoader)
    {
        // source: https://docs.google.com/spreadsheets/d/1GMo3c2xcPW5Uv3pOsaIS2EwtVA_N0Qfwo9TCTDibUoI/edit#gid=0
        _metadata = new Dictionary<int, NPCMetadata>
        {
            { 1, new NPCMetadata(0, 16, 0, 0, false, 4) }, // crow
            { 2, new NPCMetadata(0, 18, -6, -3, false, 0) }, // rat
            { 3, new NPCMetadata(0, 16, -8, -4, false, 0) }, // slime
            { 4, new NPCMetadata(0, 18, -8, -4, false, 50) }, // mummy
            { 5, new NPCMetadata(-2, 7, -8, -4, false, 20) }, // fox
            { 6, new NPCMetadata(0, 8, -6, -3, false, 10) }, // snake
            { 7, new NPCMetadata(-2, 6, -4, -2, false, 12) }, // goat
            { 8, new NPCMetadata(0, 0, -2, -1, false, 75) }, // centaur
            { 9, new NPCMetadata(-4, 6, -4, -2, false, 30) }, // undeath
            { 10, new NPCMetadata(1, -1, -6, -3, false, 8) }, // spider
            { 11, new NPCMetadata(0, 5, -4, -2, false, 40) }, // barbarian
            { 12, new NPCMetadata(0, 15, -6, -3, false, 33) }, // cactus
            { 13, new NPCMetadata(0, 13, 2, -2, false, 10) }, // penguin
            { 14, new NPCMetadata(-4, 10, -2, -4, false, 53) }, // cyclops
            { 15, new NPCMetadata(-1, 17, 0, 0, false, 40) }, // shop bob
            { 16, new NPCMetadata(0, 14, -6, -3, false, 45) }, // skeleton
            { 17, new NPCMetadata(0, 14, -6, -3, false, 45) }, // skeleton captain
            { 18, new NPCMetadata(0, 14, -6, -3, false, 45) }, // skeleton sword
            { 19, new NPCMetadata(0, 11, -6, -3, false, 49) }, // wingo
            { 20, new NPCMetadata(0, 7, 0, 0, false, 40) }, // juweller
            { 21, new NPCMetadata(-2, 17, -10, -5, false, 40) }, // pirate npc 1
            { 22, new NPCMetadata(0, 11, 0, 0, true, 45) }, // witch
            { 23, new NPCMetadata(0, 17, 0, 0, false, 40) }, // babs
            { 24, new NPCMetadata(0, 17, 0, 0, false, 40) }, // pirate npc 2
            { 25, new NPCMetadata(0, 6, 0, 0, true, 50) }, // blacksmith
            { 26, new NPCMetadata(-2, 11, -6, -3, false, 0) }, // crab🦀
            { 27, new NPCMetadata(0, 9, 0, 0, true, 12) }, // remi
            { 28, new NPCMetadata(-4, 16, 0, 0, true, 42) }, // guild bob
            { 29, new NPCMetadata(0, 7, -6, -3, false, 0) }, // vyercil
            { 30, new NPCMetadata(-10, 13, -8, -4, false, 42) }, // reaper
            { 31, new NPCMetadata(0, 13, -8, -4, false, 42) }, // chaos spawn
            { 32, new NPCMetadata(0, 17, 0, 0, false, 40) }, // blue hair npc
            { 33, new NPCMetadata(0, 17, 0, 0, false, 40) }, // beard npc
            { 34, new NPCMetadata(0, 17, 0, 0, false, 40) }, // long hair npc
            { 35, new NPCMetadata(-5, 17, -6, -3, false, 40) }, // aeven guard red
            { 36, new NPCMetadata(-5, 17, -6, -3, false, 40) }, // aeven guard purple
            { 37, new NPCMetadata(-5, 17, -6, -3, false, 40) }, // aeven guard green
            { 38, new NPCMetadata(-1, 12, -6, -3, false, 0) }, // green blob
            { 39, new NPCMetadata(-1, 12, -6, -3, false, 0) }, // pink blob
            { 40, new NPCMetadata(-1, 12, -6, -3, false, 0) }, // red blob
            { 41, new NPCMetadata(-1, 12, -6, -3, false, 0) }, // cyan blob
            { 42, new NPCMetadata(-1, 12, -6, -3, false, 0) }, // orange blob
            { 43, new NPCMetadata(-1, 12, -6, -3, false, 0) }, // yellow blob
            { 44, new NPCMetadata(-1, 12, -6, -3, false, 0) }, // blue blob
            { 45, new NPCMetadata(-1, 12, -6, -3, false, 40) }, // horse
            { 46, new NPCMetadata(-3, 10, -6, -3, false, 42) }, // unicorn
            { 47, new NPCMetadata(0, 10, -6, -3, false, 66) }, // birdman
            { 48, new NPCMetadata(6, 10, -6, -3, false, 66) }, // birdman winged
            { 49, new NPCMetadata(-9, 13, -6, -3, false, 68) }, // birdman captain
            { 50, new NPCMetadata(4, 10, -6, -3, true, 82) }, // flying birdman
            { 51, new NPCMetadata(0, 13, -10, -5, true, 59) }, // hell guardian
            { 52, new NPCMetadata(0, 1, -4, -2, true, 48) }, // wolfman
            { 53, new NPCMetadata(-2, 17, 0, 0, false, 40) }, // mommy npc
            { 54, new NPCMetadata(0, 14, 0, 0, false, 28) }, // kid npc
            { 55, new NPCMetadata(-3, 4, -3, -2, true, 13) }, // bullfrog green
            { 56, new NPCMetadata(-3, 4, -3, -2, true, 13) }, // bullfrog turqoise
            { 57, new NPCMetadata(-3, 4, -3, -2, true, 13) }, // bullfrog red
            { 58, new NPCMetadata(0, 18, -8, -4, true, 42) }, // hornet
            { 59, new NPCMetadata(0, 16, -8, -4, true, 44) }, // bat
            { 60, new NPCMetadata(-3, 6, -6, -3, false, 10) }, // wurm
            { 61, new NPCMetadata(0, 0, -12, -6, false, 10) }, // worm
            { 62, new NPCMetadata(-3, 11, -8, -4, false, 10) }, // ant
            { 63, new NPCMetadata(-3, 11, -8, -4, false, 10) }, // ant soldier
            { 64, new NPCMetadata(0, 15, -12, -6, true, 41) }, // teawk
            { 65, new NPCMetadata(3, 12, -8, -4, false, 23) }, // batmaso
            { 66, new NPCMetadata(-1, 8, -10, -5, false, 53) }, // jesaur
            { 67, new NPCMetadata(0, 12, -4, -2, false, 28) }, // hedgehog
            { 68, new NPCMetadata(0, 6, -4, -2, false, 93) }, // ice golem
            { 69, new NPCMetadata(0, 20, 0, 0, true, 48) }, // ice gem
            { 70, new NPCMetadata(-1, 17, -8, -4, false, 18) }, // gnome
            { 71, new NPCMetadata(-2, 10, -6, -3, false, 24) }, // gnome rider
            { 72, new NPCMetadata(1, 14, 0, 0, false, 20) }, // rock
            { 73, new NPCMetadata(-3, 13, -8, -4, false, 55) }, // golem
            { 74, new NPCMetadata(0, 16, -4, -2, false, 22) }, // mushroom
            { 75, new NPCMetadata(-1, 17, -6, -3, true, 37) }, // teawcus
            { 76, new NPCMetadata(-1, 16, -4, -2, false, 41) }, // hula man
            { 77, new NPCMetadata(-2, 10, 0, 0, false, 14) }, // snail
            { 78, new NPCMetadata(-2, 10, 0, 0, false, 14) }, // bowtie snail
            { 79, new NPCMetadata(0, 15, 0, 0, true, 41) }, // geggime
            { 80, new NPCMetadata(0, 5, -8, -4, false, 31) }, // ewak
            { 81, new NPCMetadata(-3, 15, -6, -3, true, 44) }, // azuorph
            { 82, new NPCMetadata(-2, 18, 0, 0, false, 40) }, // sword guy npc
            { 83, new NPCMetadata(-2, 17, 0, 0, false, 40) }, // purple hair npc
            { 84, new NPCMetadata(-2, 18, 0, 0, false, 40) }, // green hair girl npc
            { 85, new NPCMetadata(0, 18, -10, -5, true, 50) }, // wraith
            { 86, new NPCMetadata(-2, 14, 0, 0, true, 43) }, // priest
            { 87, new NPCMetadata(-4, 11, -8, -4, false, 40) }, // ninja
            { 88, new NPCMetadata(1, 12, -10, -5, false, 54) }, // orc
            { 89, new NPCMetadata(0, -4, 0, 0, true, 43) }, // octo
            { 90, new NPCMetadata(1, 6, -8, -4, true, 54) }, // octo tentacle
            { 91, new NPCMetadata(-14, 14, -3, -2, false, 64) }, // anundo leader
            { 92, new NPCMetadata(0, 13, -6, -4, false, 21) }, // carnivo
            { 93, new NPCMetadata(-9, 14, 0, 0, true, 55) }, // old king
            { 94, new NPCMetadata(-9, 14, 0, 0, true, 50) }, // green hair king
            { 95, new NPCMetadata(-7, 9, 0, 0, false, 32) }, // goblin
            { 96, new NPCMetadata(-1, 18, -6, -3, true, 46) }, // optica
            { 97, new NPCMetadata(-2, 14, -4, -2, false, 46) }, // bogo man
            { 98, new NPCMetadata(-1, 20, -8, -4, true, 50) }, // butterfly
            { 99, new NPCMetadata(0, 17, -6, -3, true, 68) }, // cursed mask
            { 100, new NPCMetadata(0, 17, -6, -3, false, 30) }, // red imp
            { 101, new NPCMetadata(0, 17, -6, -3, false, 30) }, // grey imp
            { 102, new NPCMetadata(0, 15, 0, 0, true, 84) }, // vine tentacle
            { 103, new NPCMetadata(0, 16, -10, -5, false, 48) }, // headless hunter
            { 104, new NPCMetadata(0, 15, -8, -4, false, 60) }, // swamp monster
            { 105, new NPCMetadata(0, 15, -8, -4, true, 70) }, // twin demons
            { 106, new NPCMetadata(0, 13, -6, -3, false, 30) }, // dwarf
            { 107, new NPCMetadata(-31, 4, -4, -2, false, 138) }, // apozen
            { 108, new NPCMetadata(0, 6, -10, -5, false, 12) }, // mimic
            { 109, new NPCMetadata(-2, 16, 0, 0, false, 43) }, // panda master npc
            { 110, new NPCMetadata(-2, 17, -6, -3, true, 59) }, // crane
            { 111, new NPCMetadata(0, 17, -6, -3, true, 17) }, // cyto 053
            { 112, new NPCMetadata(4, 6, -6, -3, true, 30) }, // sav 109
            { 113, new NPCMetadata(1, 7, 0, 0, false, 6) }, // robo tile
            { 114, new NPCMetadata(0, 17, -4, -2, true, 44) }, // proto
            { 115, new NPCMetadata(-6, 16, -6, -3, true, 42) }, // princess
            { 116, new NPCMetadata(0, 18, 0, 0, true, 40) }, // scientist npc
            { 117, new NPCMetadata(-2, 15, 0, 0, true, 32) }, // mechanic npc
            { 118, new NPCMetadata(-6, 15, -4, -2, true, 47) }, // wise man
            { 119, new NPCMetadata(-1, 13, -6, -3, false, 15) }, // sheep
            { 120, new NPCMetadata(-3, 19, -8, -4, false, 19) }, // biter
            { 121, new NPCMetadata(-1, 16, -2, -1, true, 10) }, // blocto
            { 122, new NPCMetadata(0, 18, -8, -4, true, 58) }, // puppet
            { 123, new NPCMetadata(0, 4, 0, 0, false, 65) }, // king wurm
            { 124, new NPCMetadata(-4, 9, -10, -5, false, 71) }, // gnoll
            { 125, new NPCMetadata(0, 0, 0, 0, false, 88) }, // bone spider
            { 126, new NPCMetadata(0, 20, -10, -5, true, 58) }, // hell flyer
            { 127, new NPCMetadata(-4, 12, -14, -5, true, 36) }, // shaman
            { 128, new NPCMetadata(-5, 17, -4, -2, false, 36) }, // war bear
            { 129, new NPCMetadata(0, 7, 0, 0, true, 32) }, // shark
            { 130, new NPCMetadata(0, 15, -10, -5, false, 5) }, // turtle
            { 131, new NPCMetadata(-1, 16, -8, -4, false, 16) }, // doll
            { 132, new NPCMetadata(-2, 5, -8, -4, false, 40) }, // dark magician
            { 133, new NPCMetadata(-2, 5, -6, -3, false, 47) }, // yeti
            { 134, new NPCMetadata(0, 18, -10, -5, true, 80) }, // banshee
            { 135, new NPCMetadata(0, 18, 0, 5, false, 41) }, // drone flyer
            { 136, new NPCMetadata(-2, 19, -4, -2, false, 22) }, // butter
            { 137, new NPCMetadata(0, 18, -8, -4, false, 45) }, // funky hair npc
            { 138, new NPCMetadata(0, 18, -6, -3, false, 26) }, // flombie
            { 139, new NPCMetadata(0, 17, -6, -3, false, 32) }, // gator
            { 140, new NPCMetadata(0, 18, -2, -1, true, 65) }, // phoenix
            { 141, new NPCMetadata(0, 16, -6, -3, false, 25) }, // bale
            { 142, new NPCMetadata(0, 18, -2, -1, true, 20) }, // espring
            { 143, new NPCMetadata(0, 16, -6, -3, false, 40) }, // old italian man npc
            { 144, new NPCMetadata(-3, 8, 0, 0, false, 52) }, // rotveig
            { 145, new NPCMetadata(0, 19, -6, -3, false, 48) }, // booba amazon npc
            { 146, new NPCMetadata(0, 19, -6, -3, false, 69) }, // cacadem
            { 147, new NPCMetadata(0, 10, 0, 0, false, 32) }, // flowie
            { 148, new NPCMetadata(-4, 16, -6, -3, true, 37) }, // nutviper
            { 149, new NPCMetadata(2, 6, 0, 0, false, 98) }, // dragon
            { 150, new NPCMetadata(-1, 18, -6, -3, false, 48) }, // flyman
            { 151, new NPCMetadata(-2, 1, -2, 1, false, 18) }, // vitamin
            { 152, new NPCMetadata(0, 10, -8, -2, false, 27) }, // onigiri
            { 153, new NPCMetadata(-2, 13, -6, -9, false, 39) }, // rabther
            { 154, new NPCMetadata(0, 15, -6, -3, false, 13) }, // piglet
            { 155, new NPCMetadata(0, 16, -6, -3, false, 18) }, // piglet with baby
            { 156, new NPCMetadata(0, 15, -6, -3, false, 32) }, // tenba
            { 157, new NPCMetadata(0, 6, -8, -4, true, 58) }, // funky hair cyborg
            { 158, new NPCMetadata(0, 15, 0, 0, true, 74) }, // drummer
            { 159, new NPCMetadata(-11, 18, 0, 0, true, 57) }, // guitarist
            { 160, new NPCMetadata(-4, 18, 0, 0, true, 45) }, // wise man guitar
            { 161, new NPCMetadata(0, 0, 0, 0, true, 65) }, // piano panda
            { 162, new NPCMetadata(-1, 10, -8, -4, false, 15) }, // mini army
            { 163, new NPCMetadata(0, 8, -6, -3, false, 12) }, // artist
            { 164, new NPCMetadata(0, 15, -8, -4, false, 10) }, // hamster
            { 165, new NPCMetadata(0, 13, 0, 0, false, 10) }, // mole
            { 166, new NPCMetadata(0, 18, 0, 0, true, 32) }, // fish
            { 167, new NPCMetadata(0, 14, -6, -3, false, 22) }, // lizzy
            { 168, new NPCMetadata(0, 15, -8, -4, false, 15) }, // ape
            { 169, new NPCMetadata(0, 4, -8, -4, false, 19) }, // taraduda
            { 170, new NPCMetadata(0, 19, -8, -4, false, 17) }, // monkey
            { 171, new NPCMetadata(0, 0, 0, 0, false, 0) }, // ancient wraith
        };
        _metadataLoader = metadataLoader;
    }

    public NPCMetadata GetValueOrDefault(int graphic)
    {
        return _metadataLoader.GetMetadata<NPCMetadata>(graphic)
            .ValueOr(DefaultMetadata.TryGetValue(graphic, out var ret) ? ret : NPCMetadata.Default);
    }
}