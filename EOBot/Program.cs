using AutomaticTypeMapper;
using EOBot.Interpreter;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Domain.NPC;
using EOLib.Domain.Spells;
using EOLib.IO.Repositories;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot
{
    static class Program
    {
        private static BotFramework f;

        [AutoMappedType]
        class NpcWalkNotifier : INPCActionNotifier
        {
            private readonly ICurrentMapStateRepository _currentMapStateRepository;
            private readonly ICharacterProvider _characterProvider;
            private readonly IENFFileProvider _enfFileProvider;

            public NpcWalkNotifier(ICurrentMapStateRepository currentMapStateRepository,
                                   ICharacterProvider characterProvider,
                                   IENFFileProvider enfFileProvider)
            {
                _currentMapStateRepository = currentMapStateRepository;
                _characterProvider = characterProvider;
                _enfFileProvider = enfFileProvider;
            }

            public void NPCTakeDamage(int npcIndex, int fromPlayerId, int damageToNpc, int npcPctHealth, Option<int> spellId)
            {
                if (fromPlayerId != _characterProvider.MainCharacter.ID)
                    return;

                var npc = _currentMapStateRepository.NPCs.SingleOrDefault(x => x.Index == npcIndex);
                var npcName = _enfFileProvider.ENFFile.SingleOrDefault(x => npc != null && npc.ID == x.ID)?.Name;

                var color = npcPctHealth < 25
                    ? ConsoleColor.Red
                    : npcPctHealth < 50
                        ? ConsoleColor.Yellow
                        : ConsoleColor.Green;

                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Hit, $"{damageToNpc,7} - {npcPctHealth,3}% HP - {npcIndex,2} - {npcName ?? string.Empty}", color);
            }

            public void RemoveNPCFromView(int npcIndex, int playerId, Option<int> spellId, Option<int> damage, bool showDeathAnimation)
            {
            }

            public void ShowNPCSpeechBubble(int npcIndex, string message)
            {
            }

            public void StartNPCAttackAnimation(int npcIndex)
            {
            }

            public void StartNPCWalkAnimation(int npcIndex)
            {
                // immediately walk the NPC to the destination index
                var npc = _currentMapStateRepository.NPCs.SingleOrDefault(x => x.Index == npcIndex);
                if (npc == null) return;

                var newNpc = npc.WithX(npc.GetDestinationX()).WithY(npc.GetDestinationY()).WithFrame(NPCFrame.Standing);
                _currentMapStateRepository.NPCs.Remove(npc);
                _currentMapStateRepository.NPCs.Add(newNpc);
            }

            public void NPCDropItem(MapItem item)
            {
            }
        }

        [AutoMappedType]
        class CharacterTakeDamageNotifier : IMainCharacterEventNotifier
        {
            private readonly ICharacterProvider _characterProvider;
            private readonly IExperienceTableProvider _experienceTableProvider;
            private readonly ICharacterInventoryProvider _characterInventoryProvider;

            public CharacterTakeDamageNotifier(ICharacterProvider characterProvider,
                                               IExperienceTableProvider experienceTableProvider,
                                               ICharacterInventoryProvider characterInventoryProvider)
            {
                _characterProvider = characterProvider;
                _experienceTableProvider = experienceTableProvider;
                _characterInventoryProvider = characterInventoryProvider;
            }

            public void NotifyGainedExp(int expDifference)
            {
                var nextLevelExp = _experienceTableProvider.ExperienceByLevel[_characterProvider.MainCharacter.Stats[CharacterStat.Level] + 1];
                var tnl = nextLevelExp - _characterProvider.MainCharacter.Stats[CharacterStat.Experience] - expDifference;
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Experience, $"{expDifference,7} - {tnl} TNL", ConsoleColor.DarkCyan);
            }

            public void NotifyTakeDamage(int damageTaken, int playerPercentHealth, bool isHeal)
            {
                var type = isHeal ? ConsoleHelper.Type.Heal : ConsoleHelper.Type.Damage;
                var color = isHeal ? ConsoleColor.DarkGreen
                    : playerPercentHealth < 25
                        ? ConsoleColor.Red
                        : playerPercentHealth < 50
                            ? ConsoleColor.Yellow
                            : ConsoleColor.Green;

                ConsoleHelper.WriteMessage(type, $"{damageTaken,7} - {playerPercentHealth,3}% HP", color);

                var hp = _characterProvider.MainCharacter.Stats[CharacterStat.HP];
                if (!isHeal && hp - damageTaken <= 0)
                {
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Dead, "**** YOU DIED ****", ConsoleColor.DarkRed);
                }
            }

            public void TakeItemFromMap(int id, int amountTaken)
            {
                var inventoryCount = _characterInventoryProvider.ItemInventory.SingleOrDefault(x => x.ItemID == id);
                var weight = _characterProvider.MainCharacter.Stats[CharacterStat.Weight];
                var maxWeight = _characterProvider.MainCharacter.Stats[CharacterStat.MaxWeight];
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Item, $"{weight,3}/{maxWeight,3} - weight - {inventoryCount.Amount} in inventory");
            }

            public void DropItem(int id, int amountDropped)
            {
            }

            public void JunkItem(int id, int amountRemoved)
            {
                var inventoryCount = _characterInventoryProvider.ItemInventory.SingleOrDefault(x => x.ItemID == id);
                var weight = _characterProvider.MainCharacter.Stats[CharacterStat.Weight];
                var maxWeight = _characterProvider.MainCharacter.Stats[CharacterStat.MaxWeight];
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.JunkItem, $"{weight,3}/{maxWeight,3} - weight - {inventoryCount?.Amount ?? 0} in inventory");
            }

            public void MakeDrunk() { }
        }

        [AutoMappedType]
        class CharacterAnimationNotifier : IOtherCharacterAnimationNotifier
        {
            private readonly ICharacterProvider _characterProvider;

            public CharacterAnimationNotifier(ICharacterProvider characterProvider)
            {
                _characterProvider = characterProvider;
            }

            public void NotifySelfSpellCast(int playerId, int spellId, int spellHp, int percentHealth)
            {
                if (playerId == _characterProvider.MainCharacter.ID && spellHp > 0)
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Heal, $"{spellHp,7} - {percentHealth}% HP", ConsoleColor.DarkGreen);
            }

            public void NotifyStartSpellCast(int playerId, int spellId) { }
            public void NotifyTargetNpcSpellCast(int playerId) { }
            public void NotifyTargetOtherSpellCast(int sourcePlayerID, int targetPlayerID, int spellId, int recoveredHP, int targetPercentHealth) { }
            public void StartOtherCharacterAttackAnimation(int characterID, int noteIndex) { }
            public void StartOtherCharacterWalkAnimation(int characterID, MapCoordinate destination, EODirection direction) { }
            public void NotifyGroupSpellCast(int playerId, int spellId, int spellHp, List<GroupSpellTarget> spellTargets) { }
        }

        static async Task<int> Main(string[] args)
        {
            var assemblyNames = new[]
            {
                "EOBot",
                "EOLib",
                "EOLib.Config",
                "EOLib.IO",
                "EOLib.Localization",
                "EOLib.Logger"
            };

            Console.CancelKeyPress += HandleCtrlC;

            ArgumentsParser parsedArgs = new ArgumentsParser(args);

            if (parsedArgs.Error != ArgsError.NoError || parsedArgs.ExtendedHelp)
            {
                ShowError(parsedArgs);
                return 1;
            }

            DependencyMaster.TypeRegistry = new ITypeRegistry[parsedArgs.NumBots];
            for (int i = 0; i < parsedArgs.NumBots; ++i)
            {
                DependencyMaster.TypeRegistry[i] = new UnityRegistry(assemblyNames);
                DependencyMaster.TypeRegistry[i].RegisterDiscoveredTypes();
            }

            IBotFactory botFactory;
            if (parsedArgs.ScriptFile != null)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, $"Executing script {parsedArgs.ScriptFile}...");
                botFactory = new ScriptedBotFactory(parsedArgs);
            }
            else
            {
                botFactory = new TrainerBotFactory(parsedArgs);
            }

            ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, "Starting bots...");

            try
            {
                using (f = new BotFramework(parsedArgs))
                {
                    await f.InitializeAsync(botFactory, parsedArgs.InitDelay);
                    await f.RunAsync();
                }

                Console.WriteLine();
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, "All bots completed.");
            }
            catch (BotException bex)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, bex.Message, ConsoleColor.DarkRed);
                return 1;
            }
            catch (BotScriptErrorException bse)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, bse.Message, ConsoleColor.DarkRed);
                return 1;
            }
            catch (AggregateException ae)
            {
                var botExceptions = ae.InnerExceptions.OfType<BotScriptErrorException>().ToList();
                foreach (var ie in botExceptions)
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, ie.Message, ConsoleColor.DarkRed);

                var otherExceptions = ae.InnerExceptions.Except(botExceptions);
                foreach (var ie in otherExceptions)
                    ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, $"Unhandled error: {ie.Message}\nStack Trace:\n{ie.StackTrace}", ConsoleColor.DarkRed);

                return 1;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteMessage(ConsoleHelper.Type.Error, $"Unhandled error: {ex.Message}\nStack Trace:\n{ex.StackTrace}", ConsoleColor.DarkRed);
                return 1;
            }

            return 0;
        }

        static void HandleCtrlC(object sender, ConsoleCancelEventArgs e)
        {
            var name = Enum.GetName(e.SpecialKey.GetType(), e.SpecialKey);
            ConsoleHelper.WriteMessage(ConsoleHelper.Type.None, $"Exiting due to {name} event from system");

            f?.TerminateBots();
        }

        static void ShowError(ArgumentsParser args)
        {
            switch (args.Error)
            {
                case ArgsError.WrongNumberOfArgs:
                    Console.WriteLine("Invalid: specify host, port, and the number of bots to run");
                    break;
                case ArgsError.InvalidPort:
                    Console.WriteLine("Invalid: port number could not be parsed!");
                    break;
                case ArgsError.InvalidNumberOfBots:
                    Console.WriteLine("Invalid: specify an integer argument for number of bots.");
                    break;
                case ArgsError.TooManyBots:
                    Console.WriteLine("Invalid: unable to launch > 25 threads of execution. Please use 25 or less.");
                    break;
                case ArgsError.NotEnoughBots:
                    Console.WriteLine("Invalid: unable to launch < 1 thread of execution. Please use 1 or more.");
                    break;
                case ArgsError.BadFormat:
                    Console.WriteLine("Badly formatted argument. Enter args like \"argument=value\".");
                    break;
                case ArgsError.InvalidSimultaneousNumberOfBots:
                    Console.WriteLine("Invalid: Simultaneous number of bots must not be more than total number of bots.");
                    break;
                case ArgsError.InvalidWaitFlag:
                    Console.WriteLine("Invalid: Wait flag could not be parsed. Use 0, 1, false, true, no, or yes.");
                    break;
                case ArgsError.InvalidInitDelay:
                    Console.WriteLine("Invalid: specify an integer argument for delay between inits (> 1100ms).");
                    break;
                case ArgsError.InvalidPath:
                    Console.WriteLine("Invalid: Script file does not exist or is not a valid path.");
                    break;
                case ArgsError.InvalidScriptArgs:
                    Console.WriteLine("Invalid: User-defined arguments and disabling autoconnect require a script.");
                    break;
                case ArgsError.AutoConnectRequired:
                    Console.WriteLine("Invalid: AutoConnect is required when using a script with more than 1 bot due to eoserv connection throttling.");
                    break;
            }

            Console.WriteLine("\n\nUsage: (enter arguments in any order) (angle brackets is entry) (square brackets is optional)");
            Console.WriteLine("EOBot.exe host=<host>\n" +
                              "          port=<port>\n" +
                              "          bots=<numBots>[,<simultaneousBots>]\n" +
                              "          initDelay=<timeInMS>\n" +
                              "          account=<account>\n" +
                              "          password=<password>\n" +
                              "          character=<character>\n" +
                              "          script=<file>\n" +
                              "          autoconnect=<true|false>" +
                              "          [-- arg1, [arg2..argn]]");
            Console.WriteLine("\t host:           hostname or IP address");
            Console.WriteLine("\t port:           port to connect on (probably 8078)");
            Console.WriteLine("\t bots:           number of bots to execute.    \n\t       numBots is the total number, simultaneousBots is how many will run at once");
            Console.WriteLine("\t initDelay:      time in milliseconds to delay between doing the INIT handshake with the server");
            Console.WriteLine("\t account:        account to connect with (created if it does not exist)");
            Console.WriteLine("\t password:       password for account");
            Console.WriteLine("\t character:      character to use (created if it does not exist)");
            Console.WriteLine("\t script:         script file to execute\n\t         if script is not specified, default trainer bot will be used");
            Console.WriteLine("\t autoconnect:    (default true) true to automatically connect/disconnect to server with initDelay timeout between connection attempts for bots, false otherwise");
            Console.WriteLine("\t --: Any arguments passed after '--' will be available in a script under the '$args' array");

            if (!args.ExtendedHelp)
                return;

            Console.WriteLine(@"
===============================================================
                        Bot Script Info                        
===============================================================
GENERAL
---------------------------------------------------------------
Semicolons are not part of the script language grammar
Do not end statements with semicolons

---------------------------------------------------------------
VARIBLES
---------------------------------------------------------------
variables are prefixed with a $
ex: $var_1 = 1
datatypes supported: bool, int, string, array
variables are dynamically typed

---------------------------------------------------------------
KEYWORDS
---------------------------------------------------------------
if ($expression) $statement
if ($expression) { $statement_list }
while ($expression) $statement
while ($expression) { $statement_list }
goto LABEL
LABEL:

Labels are a separate set of identifiers from variables
Labels do not use a $ prefix and do not need to be all caps

---------------------------------------------------------------
FUNCTIONS
---------------------------------------------------------------
Functions are called like normal programming languages
ex: print(""my message"")
Functions can be used in any expression, e.g. an array access
    operation or a keyword evaluation (if/while)
Parameters to functions can be expressions, variables,
    or literals

---------------------------------------------------------------
BUILT-IN IDENTIFIERS AND FUNCTIONS
---------------------------------------------------------------
print($message) - returns nothing; writes $message to console
len($array) - returns int; gets length of $array
array($size) - returns array; creates a fixed-size array 
    of $size with all contents set to 'Undefined'
sleep($time_ms) - returns nothing; sleeps for $time_ms
    milliseconds
time() - returns string; gets the current system time in
    HH:MM:SS format

Connect($host, $port) - connect to a server
Disconnect() - disconnect from a server
CreateAccount($user, $pass) - returns AccountReply
Login($user, $pass) - returns LoginReply
CreateAndLogin($user, $pass) - returns LoginReply
ChangePassword($user, $oldpass, $newpass) - returns AccountReply
CreateCharacter($name) - returns CharacterReply
DeleteCharacter($name, $force) - returns CharacterReply
LoginToCharacter($name)

NOTE: IT IS HIGHLY RECOMMENDED TO SLEEP IF USING MULTIPLE BOTS
    AND CALLING ANY OF THE EO FUNCTIONS ABOVE, SINCE THEY
    RUN ASYNCHRONOUSLY THEY ARE NOT GUARANTEED TO FINISH BY THE
    TIME THE SCRIPT FINISHES EXECUTING

$host -     string : host passed via command-line
$port -     int    : port passed via command-line
$user -     string : account passed via command-line
$pass -     string : account password passed via command-line
$botindex - int    : index of this bot (for when numBots > 1)
$args -     array  : user-defined arguments passed after '--'

$version -  int    : allows setting client version
$result -   dynamic: get/set result of last function call
$account -  object : reserved
$character - object: reserved
$mapstate - object : reserved
");
        }
    }
}
