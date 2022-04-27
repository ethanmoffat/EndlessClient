using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.IO.Actions
{
    [MappedType(BaseType = typeof(IPubFileLoadActions))]
    public class PubFileLoadActions : IPubFileLoadActions
    {
        private readonly IPubFileRepository _pubFileRepository;
        private readonly IPubLoadService<EIFRecord> _itemFileLoadService;
        private readonly IPubLoadService<ENFRecord> _npcFileLoadService;
        private readonly IPubLoadService<ESFRecord> _spellFileLoadService;
        private readonly IPubLoadService<ECFRecord> _classFileLoadService;

        public PubFileLoadActions(IPubFileRepository pubFileRepository,
                                  IPubLoadService<EIFRecord> itemFileLoadService,
                                  IPubLoadService<ENFRecord> npcFileLoadService,
                                  IPubLoadService<ESFRecord> spellFileLoadService,
                                  IPubLoadService<ECFRecord> classFileLoadService)
        {
            _pubFileRepository = pubFileRepository;
            _itemFileLoadService = itemFileLoadService;
            _npcFileLoadService = npcFileLoadService;
            _spellFileLoadService = spellFileLoadService;
            _classFileLoadService = classFileLoadService;
        }

        public void LoadItemFile(IEnumerable<int> rangedWeaponIds)
        {
            var itemFile = _itemFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.EIFFile = OverrideRangedWeapons(itemFile, rangedWeaponIds);
        }

        public void LoadItemFileByName(string fileName, IEnumerable<int> rangedWeaponIds)
        {
            var itemFile = _itemFileLoadService.LoadPubFromExplicitFile(fileName);
            _pubFileRepository.EIFFile = OverrideRangedWeapons(itemFile, rangedWeaponIds);
        }

        public void LoadNPCFile()
        {
            var npcFile = _npcFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.ENFFile = npcFile;
        }

        public void LoadNPCFileByName(string fileName)
        {
            var npcFile = _npcFileLoadService.LoadPubFromExplicitFile(fileName);
            _pubFileRepository.ENFFile = npcFile;
        }

        public void LoadSpellFile()
        {
            var spellFile = _spellFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.ESFFile = spellFile;
        }

        public void LoadSpellFileByName(string fileName)
        {
            var spellFile = _spellFileLoadService.LoadPubFromExplicitFile(fileName);
            _pubFileRepository.ESFFile = spellFile;
        }

        public void LoadClassFile()
        {
            var classFile = _classFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.ECFFile = classFile;
        }

        public void LoadClassFileByName(string fileName)
        {
            var classFile = _classFileLoadService.LoadPubFromExplicitFile(fileName);
            _pubFileRepository.ECFFile = classFile;
        }

        private static IPubFile<EIFRecord> OverrideRangedWeapons(IPubFile<EIFRecord> inputFile, IEnumerable<int> rangedWeaponIds)
        {
            var rangedItemOverrides = inputFile.Where(x => x.Type == ItemType.Weapon && rangedWeaponIds.Contains(x.ID)).ToList();

            var retFile = inputFile;
            foreach (var item in rangedItemOverrides)
                retFile = retFile.WithUpdatedRecord((EIFRecord)item.WithProperty(PubRecordProperty.ItemSubType, (int)ItemSubType.Ranged));

            return retFile;
        }
    }
}