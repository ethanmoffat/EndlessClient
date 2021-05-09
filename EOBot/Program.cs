using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Domain.NPC;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EOBot
{
    static class Program
    {
        private static BotFramework f;
        private static Win32.ConsoleCtrlDelegate consoleControlHandler;

        [AutoMappedType]
        class NpcWalkNotifier : INPCActionNotifier
        {
            private readonly ICurrentMapStateRepository _currentMapStateRepository;

            public NpcWalkNotifier(ICurrentMapStateRepository currentMapStateRepository)
            {
                _currentMapStateRepository = currentMapStateRepository;
            }

            public void NPCTakeDamage(short npcIndex, int fromPlayerId, int damageToNpc, short npcPctHealth, EOLib.Optional<int> spellId)
            {
            }

            public void RemoveNPCFromView(int npcIndex, int playerId, EOLib.Optional<short> spellId, EOLib.Optional<int> damage, bool showDeathAnimation)
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

                var newNpc = npc.WithX((byte)npc.GetDestinationX()).WithY((byte)npc.GetDestinationY()).WithFrame(NPCFrame.Standing);
                _currentMapStateRepository.NPCs.Remove(npc);
                _currentMapStateRepository.NPCs.Add(newNpc);
            }
        }

        [AutoMappedType]
        class CharacterTakeDamageNotifier : IMainCharacterEventNotifier
        {
            private readonly ICharacterProvider _characterProvider;
            private readonly IExperienceTableProvider _experienceTableProvider;

            public CharacterTakeDamageNotifier(ICharacterProvider characterProvider,
                                               IExperienceTableProvider experienceTableProvider)
            {
                _characterProvider = characterProvider;
                _experienceTableProvider = experienceTableProvider;
            }

            public void NotifyGainedExp(int expDifference)
            {
                var nextLevelExp = _experienceTableProvider.ExperienceByLevel[_characterProvider.MainCharacter.Stats[CharacterStat.Level] + 1];
                var tnl = nextLevelExp - _characterProvider.MainCharacter.Stats[CharacterStat.Experience] - expDifference;
                Console.WriteLine($"[EXP ] {expDifference,7} - {tnl} TNL");
            }

            public void NotifyTakeDamage(int damageTaken, int playerPercentHealth, bool isHeal)
            {
                Console.WriteLine($"[{(isHeal ? "HEAL" : "DMG ")}] {damageTaken,7} - {playerPercentHealth}% HP");

                var hp = _characterProvider.MainCharacter.Stats[CharacterStat.HP];
                if (!isHeal && hp - damageTaken <= 0)
                {
                    Console.WriteLine("[DEAD] **** YOU DIED ****");
                }
            }

            public void TakeItemFromMap(short id, int amountTaken)
            {
                var weight = _characterProvider.MainCharacter.Stats[CharacterStat.Weight];
                var maxWeight = _characterProvider.MainCharacter.Stats[CharacterStat.MaxWeight];
                Console.WriteLine($"[ITEM] {weight,3}/{maxWeight,3} weight");
            }
        }

        [AutoMappedType]
        class CharacterAnimationNotifier : IOtherCharacterAnimationNotifier
        {
            private readonly ICharacterProvider _characterProvider;

            public CharacterAnimationNotifier(ICharacterProvider characterProvider)
            {
                _characterProvider = characterProvider;
            }

            public void NotifySelfSpellCast(short playerId, short spellId, int spellHp, byte percentHealth)
            {
                if (playerId == _characterProvider.MainCharacter.ID && spellHp > 0)
                    Console.WriteLine($"[HEAL] {spellHp,7} - {percentHealth}% HP");
            }

            public void NotifyStartSpellCast(short playerId, short spellId) { }
            public void NotifyTargetOtherSpellCast(short sourcePlayerID, short targetPlayerID, short spellId, int recoveredHP, byte targetPercentHealth) { }
            public void StartOtherCharacterAttackAnimation(int characterID) { }
            public void StartOtherCharacterWalkAnimation(int characterID) { }
        }

        static async Task Main(string[] args)
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

            // this needs to be a delegate because it is getting garbage collected
            consoleControlHandler = new Win32.ConsoleCtrlDelegate(HandleCtrl);
            if (!Win32.SetConsoleCtrlHandler(consoleControlHandler, true))
            {
                Console.WriteLine("WARNING: Unable to set console control handler! CTRL+C will not terminate cleanly.");
            }

            ArgumentsParser parsedArgs = new ArgumentsParser(args);

            if (parsedArgs.Error != ArgsError.NoError)
            {
                ShowError(parsedArgs);
                return;
            }

            DependencyMaster.TypeRegistry = new ITypeRegistry[parsedArgs.NumBots];
            for (int i = 0; i < parsedArgs.NumBots; ++i)
            {
                DependencyMaster.TypeRegistry[i] = new UnityRegistry(assemblyNames);
                DependencyMaster.TypeRegistry[i].RegisterDiscoveredTypes();
            }

            Console.WriteLine("Starting bots...");

            try
            {
                using (f = new BotFramework(new BotConsoleOutputHandler(), parsedArgs))
                {
                    await f.InitializeAsync(new TrainerBotFactory(parsedArgs), parsedArgs.InitDelay);
                    f.Run(parsedArgs.WaitForTermination);
                    f.WaitForBotsToComplete();
                }

                Console.WriteLine("All bots completed.");
            }
            catch (BotException bex)
            {
                Console.WriteLine(bex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nUnhandled error: {0}", ex.Message);
            }
        }

        static bool HandleCtrl(Win32.CtrlTypes type)
        {
            var name = Enum.GetName(type.GetType(), type);
            Console.WriteLine("\nExiting due to {0} event from system!\n", name);

            if (f != null)
                f.TerminateBots();
            return true;
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
            }

            Console.WriteLine("\n\nUsage: (enter arguments in any order) (angle brackets is entry) (square brackets is optional)");
            Console.WriteLine("EOBot.exe host=<host>\n" +
                              "          port=<port>\n" +
                              "          bots=<numBots>[,<simultaneousBots>]\n" +
                              "          wait=<true/false>\n" +
                              "          initDelay=<timeInMS>\n");
            Console.WriteLine("\t host: hostname or IP address");
            Console.WriteLine("\t port: port to connect on (probably 8078)");
            Console.WriteLine("\t bots: number of bots to execute.    \n\t       numBots is the total number, simultaneousBots is how many will run at once");
            Console.WriteLine("\t wait: flag to wait for termination. \n\t       Set to true to wait for bots to be explicitly terminated via CTRL+C, false otherwise");
            Console.WriteLine("\t initDelay: Time in milliseconds to delay between doing the INIT handshake with the server");
        }
    }
}
