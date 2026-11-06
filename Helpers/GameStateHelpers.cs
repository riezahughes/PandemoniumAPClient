using Archipelago.Core;
using Serilog;
using Helpers;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using PandemoniumArchipelago;
using Archipelago.MultiClient.Net.Models;

namespace Helpers
{
    public class GameStateHelpers
    {
        internal static bool CheckCreditsCondition(ArchipelagoClient client)
        {
            ushort creditsFind = Memory.ReadByte(Addresses.CreditsAddress);
            if (creditsFind == 10)
            {
                return true;
            }

            return false;
        }

        internal static bool CheckSpellbookPageCondition(ArchipelagoClient client)
        {

            int tailCount = client.CurrentSession.Items.AllItemsReceived.Count(item => item.ItemName.Contains("Spellbook Pages"));
            int maxCount = PlayerStateHelpers.GetPlayerOptionCounts(client.Options, "spellpages");
            return tailCount >= maxCount;
        }

        public static bool CheckGoalCondition(ArchipelagoClient client)
        {

            if (client?.CurrentSession == null)
            {
                return false;
            }

            // TODO Victory logic goes into each of these goal conditions

            int goalCondition = Int32.Parse(client.Options?.GetValueOrDefault("goal", "0").ToString() ?? "0");

            if (goalCondition == PlayerVictoryConditions.CREDITS)
            {
                Console.WriteLine("Cleared 1");
                return true;
            }
            else if (goalCondition == PlayerVictoryConditions.SPELL_PAGES)
            {
                bool cleared = CheckSpellbookPageCondition(client);

                if (cleared == true)
                {
                    client.SendGoalCompletion();
                    return true;
                }
                return false;
            }
            else if (goalCondition == PlayerVictoryConditions.BOTH)
            {
                Console.WriteLine("Cleared 1 and 2");
                return true;
            }
            return false;
        }

        public static void SetUpLevelSelectListener(CancellationTokenSource cts, ArchipelagoClient client)
        {
            if (cts.Token.IsCancellationRequested) return;

            Console.WriteLine("!!!");

            // Halfword at LevelSelectState: 0x0015 = no choice made (max 0x15, choice 0).
            Memory.MonitorAddressForAction<ushort>(
                Addresses.LevelSelectState,
                () =>
                {
                    try
                    {
                        // The chosen level is the high byte of the halfword (0x0115: 01 = level, 15 = max choice)
                        int currentLevel = Memory.ReadByte(Addresses.LevelChoice);

                        var received = client.CurrentSession?.Items.AllItemsReceived;

                        bool hasKey = false;
                        if (LocationHelpers.LevelToKeyName.TryGetValue(currentLevel, out string? keyName))
                        {
                            Console.WriteLine($"Level {currentLevel} needs key: {keyName}");
                            hasKey = client.CurrentSession?.Items.AllItemsReceived
                                .Any(item => item.ItemName == keyName) ?? false;
                        }

                        if (!hasKey)
                        {
                            // Locked: cancel the choice. Must be a ushort write, a bare int literal writes 4 bytes.
                            Memory.Write(Addresses.LevelSelectState, 0x0015);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Level select check failed");
                    }
                    finally
                    {
                        // An unlocked choice stays non-zero, so wait for it to clear before re-arming
                        // or the monitor would retrigger straight away.
                        while (!cts.Token.IsCancellationRequested
                               && Memory.ReadShort(Addresses.LevelSelectState) != 0x0015)
                        {
                            Thread.Sleep(100);
                        }

                        SetUpLevelSelectListener(cts, client);
                    }

                }, value => value != 0x0015);
        }

        public static void SetUpMapListener(CancellationTokenSource cts, ArchipelagoClient client)
        {
            if (cts.Token.IsCancellationRequested) return;

            Memory.MonitorAddressForAction<byte>(
                Addresses.HasStartedGame,
                () =>
                {
                    // Console.WriteLine("Checking level");
                    Memory.WriteByte(Addresses.LevelMaxChoice, 0x15);
                    Thread.Sleep(1000);
                    SetUpMapListener(cts, client);

                }, value => value == 0x00);
        }
    }
}