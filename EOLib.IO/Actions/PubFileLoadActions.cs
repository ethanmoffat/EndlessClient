using AutomaticTypeMapper;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using System.IO;
using System.Linq;

namespace EOLib.IO.Actions
{
    [AutoMappedType]
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
            var itemFiles = _itemFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.EIFFiles = itemFiles.ToList();
            _pubFileRepository.EIFFile = PubFileExtensions.Merge(_pubFileRepository.EIFFiles);
        }

        public void LoadItemFileByName(string fileName)
        {
            var itemFiles = _itemFileLoadService.LoadPubFromExplicitFile(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
            _pubFileRepository.EIFFiles = itemFiles.ToList();
            _pubFileRepository.EIFFile = PubFileExtensions.Merge(_pubFileRepository.EIFFiles);
        }

        public void LoadNPCFile()
        {
            var npcFiles = _npcFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.ENFFiles = npcFiles.ToList();
            _pubFileRepository.ENFFile = PubFileExtensions.Merge(_pubFileRepository.ENFFiles);
        }

        public void LoadNPCFileByName(string fileName)
        {
            var npcFiles = _npcFileLoadService.LoadPubFromExplicitFile(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
            _pubFileRepository.ENFFiles = npcFiles.ToList();
            _pubFileRepository.ENFFile = PubFileExtensions.Merge(_pubFileRepository.ENFFiles);
        }

        public void LoadSpellFile()
        {
            var spellFiles = _spellFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.ESFFiles = spellFiles.ToList();
            _pubFileRepository.ESFFile = PubFileExtensions.Merge(_pubFileRepository.ESFFiles);
        }

        public void LoadSpellFileByName(string fileName)
        {
            var spellFiles = _spellFileLoadService.LoadPubFromExplicitFile(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
            _pubFileRepository.ESFFiles = spellFiles.ToList();
            _pubFileRepository.ESFFile = PubFileExtensions.Merge(_pubFileRepository.ESFFiles);
        }

        public void LoadClassFile()
        {
            var classFiles = _classFileLoadService.LoadPubFromDefaultFile();
            _pubFileRepository.ECFFiles = classFiles.ToList();
            _pubFileRepository.ECFFile = PubFileExtensions.Merge(_pubFileRepository.ECFFiles);
        }

        public void LoadClassFileByName(string fileName)
        {
            var classFiles = _classFileLoadService.LoadPubFromExplicitFile(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
            _pubFileRepository.ECFFiles = classFiles.ToList();
            _pubFileRepository.ECFFile = PubFileExtensions.Merge(_pubFileRepository.ECFFiles);
        }
    }
}