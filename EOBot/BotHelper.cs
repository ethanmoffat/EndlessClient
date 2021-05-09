using EOLib;
using EOLib.Domain.Account;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Protocol;
using EOLib.IO.Actions;
using EOLib.Net.API;
using EOLib.Net.FileTransfer;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot
{
    delegate void DisplayMessageFunc(string message = "");

    public class BotHelper
    {
        private readonly int _botIndex;

        public BotHelper(int botIndex)
        {
            _botIndex = botIndex;
        }

        public async Task<AccountReply> CreateAccountAsync(string name, string password)
        {
            var accountActions = DependencyMaster.TypeRegistry[_botIndex].Resolve<IAccountActions>();
            var accParams = new CreateAccountParameters(name, password, password, name, name, name + "@test.com");
            return await accountActions.CreateAccount(accParams);
        }

        public async Task<LoginReply> LoginToAccountAsync(string name, string password)
        {
            var loginActions = DependencyMaster.TypeRegistry[_botIndex].Resolve<ILoginActions>();
            var loginParameters = new LoginParameters(name, password);
            return await loginActions.LoginToServer(loginParameters);
        }

        public async Task<CharacterReply> CreateCharacterAsync(string name)
        {
            var characterActions = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterManagementActions>();
            await characterActions.RequestCharacterCreation();
            var charParams = new CharacterCreateParameters(name, 0, 1, 0, 0);
            return await characterActions.CreateCharacter(charParams);
        }

        public async Task LoginToCharacterAsync(string name)
        {
            var loginActions = DependencyMaster.TypeRegistry[_botIndex].Resolve<ILoginActions>();
            var characters = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterSelectorProvider>();
            var mapStateProvider = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICurrentMapStateProvider>();

            if (characters.Characters == null || !characters.Characters.Any())
                await CreateCharacterAsync(name);

            var character = characters.Characters.Single(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            await loginActions.RequestCharacterLogin(character);

            var unableToLoadMap = false;
            try
            {
                var mapLoadActions = DependencyMaster.TypeRegistry[_botIndex].Resolve<IMapFileLoadActions>();
                mapLoadActions.LoadMapFileByID(mapStateProvider.CurrentMapID);
            }
            catch (IOException)
            {
                unableToLoadMap = true;
            }

            var fileRequestActions = DependencyMaster.TypeRegistry[_botIndex].Resolve<IFileRequestActions>();
            if (unableToLoadMap || fileRequestActions.NeedsFileForLogin(InitFileType.Map, mapStateProvider.CurrentMapID))
                await fileRequestActions.GetMapFromServer(mapStateProvider.CurrentMapID);

            if (fileRequestActions.NeedsFileForLogin(InitFileType.Item))
                await fileRequestActions.GetItemFileFromServer();

            if (fileRequestActions.NeedsFileForLogin(InitFileType.Npc))
                await fileRequestActions.GetNPCFileFromServer();

            if (fileRequestActions.NeedsFileForLogin(InitFileType.Spell))
                await fileRequestActions.GetSpellFileFromServer();

            if (fileRequestActions.NeedsFileForLogin(InitFileType.Class))
                await fileRequestActions.GetClassFileFromServer();

            await loginActions.CompleteCharacterLogin();
        }
    }
}
