// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.IO.Services;

namespace EOLib.IO.Actions
{
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

        public void LoadItemFile()
        {
            var itemFile = _itemFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.ItemFile = itemFile;
        }

        public void LoadItemFileByName(string fileName)
        {
            var itemFile = _itemFileLoadService.LoadPubFromExplicitFile(fileName);
            _pubFileRepository.ItemFile = itemFile;
        }

        public void LoadNPCFile()
        {
            var npcFile = _npcFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.NPCFile = npcFile;
        }

        public void LoadNPCFileByName(string fileName)
        {
            var npcFile = _npcFileLoadService.LoadPubFromExplicitFile(fileName);
            _pubFileRepository.NPCFile = npcFile;
        }

        public void LoadSpellFile()
        {
            var spellFile = _spellFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.SpellFile = spellFile;
        }

        public void LoadSpellFileByName(string fileName)
        {
            var spellFile = _spellFileLoadService.LoadPubFromExplicitFile(fileName);
            _pubFileRepository.SpellFile = spellFile;
        }

        public void LoadClassFile()
        {
            var classFile = _classFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.ClassFile = classFile;
        }

        public void LoadClassFileByName(string fileName)
        {
            var classFile = _classFileLoadService.LoadPubFromExplicitFile(fileName);
            _pubFileRepository.ClassFile = classFile;
        }
    }
}