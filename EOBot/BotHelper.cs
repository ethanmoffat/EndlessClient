using EOLib.Domain.Account;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Protocol;
using EOLib.IO.Actions;
using EOLib.Net.FileTransfer;
using Optional.Collections;
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
            var accParams = new CreateAccountParameters(name, password, password, name, name, name + "@eobot.net");
            var nameResult = await accountActions.CheckAccountNameWithServer(name);
            return nameResult >= AccountReply.OK_CodeRange
                ? await accountActions.CreateAccount(accParams, (int)nameResult)
                : nameResult;
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
            var createId = await characterActions.RequestCharacterCreation();
            var charParams = new CharacterCreateParameters(name, 0, 1, 0, 0);
            return await characterActions.CreateCharacter(charParams, createId);
        }

        public async Task LoginToCharacterAsync(string name)
        {
            var loginActions = DependencyMaster.TypeRegistry[_botIndex].Resolve<ILoginActions>();
            var characters = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterSelectorProvider>();
            var mapStateProvider = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICurrentMapStateProvider>();

            if (characters.Characters == null || !characters.Characters.Any())
                await CreateCharacterAsync(name);

            var character = characters.Characters.Single(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            var sessionID = await loginActions.RequestCharacterLogin(character);

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
                await fileRequestActions.GetMapFromServer(mapStateProvider.CurrentMapID, sessionID);

            if (fileRequestActions.NeedsFileForLogin(InitFileType.Item))
                await fileRequestActions.GetItemFileFromServer(sessionID);

            if (fileRequestActions.NeedsFileForLogin(InitFileType.Npc))
                await fileRequestActions.GetNPCFileFromServer(sessionID);

            if (fileRequestActions.NeedsFileForLogin(InitFileType.Spell))
                await fileRequestActions.GetSpellFileFromServer(sessionID);

            if (fileRequestActions.NeedsFileForLogin(InitFileType.Class))
                await fileRequestActions.GetClassFileFromServer(sessionID);

            await loginActions.CompleteCharacterLogin(sessionID);
        }

        public async Task<AccountReply> ChangePasswordAsync(string name, string oldPass, string newPass)
        {
            var accountActions = DependencyMaster.TypeRegistry[_botIndex].Resolve<IAccountActions>();
            var accParams = new ChangePasswordParameters(name, oldPass, newPass);
            return await accountActions.ChangePassword(accParams);
        }

        public async Task<CharacterReply> DeleteCharacterAsync(string name, bool force)
        {
            var characterSelectorRepository = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterSelectorRepository>();
            characterSelectorRepository.CharacterForDelete = characterSelectorRepository.Characters.SingleOrNone(x => x.Name == name);

            if (!characterSelectorRepository.CharacterForDelete.HasValue)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, $"Character {name} could not be deleted / does not exist");
                return CharacterReply.THIS_IS_WRONG;
            }

            var characterActions = DependencyMaster.TypeRegistry[_botIndex].Resolve<ICharacterManagementActions>();
            var deleteId = await characterActions.RequestCharacterDelete();

            if (!force)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Warning, "DELETING CHARACTER - ARE YOU SURE [Y/N]?", ConsoleColor.Yellow);
                if (Console.ReadLine().ToLower() != "y")
                    return CharacterReply.NotApproved;
            }

            return await characterActions.DeleteCharacter(deleteId);
        }
    }
}
