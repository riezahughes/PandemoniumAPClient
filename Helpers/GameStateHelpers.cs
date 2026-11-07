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
            int maxCount = PlayerStateHelpers.GetPlayerOptionCounts(client.Options, "spellbook_pages");
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
                byte creditCheck = Memory.ReadByte(Addresses.CreditsAddress);
                if (APHelpers.isInTheGame() && creditCheck == 0x10)
                {
                    client.SendGoalCompletion();
                    return true;
                }
                return false;
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

        // A new game wipes the memory the items were written to, so every item received so far has to be given again.
        private static async Task ReplayReceivedItems(ArchipelagoClient client)
        {
            await client.ItemManager.StopReceiving();
            // re-queue everything Core already delivered, then zero Core's saved index so it counts as new again
            await client.ItemManager.ResetItems();
            await client.ItemManager.ForceReloadAllItems();
            // the loop in App.cs calls ReceiveReady() once the game is loaded and can take the items
            APHelpers.NeedsItemRetry = true;
        }

        // Pointsanity. The game's treasure total counts up over the whole game, so the client keeps its own count for the
        // current level: it adds every increase of the total, and starts again at 0 when the level changes. Dying keeps
        // the count (the level id doesn't change); a game over drops the total, which just becomes the new baseline.
        // points collected so far in the level the player is in (see SetUpPointsTracker), shown by the items command
        public static volatile int CurrentLevelPoints = 0;

        public static void SetUpPointsTracker(CancellationTokenSource cts, ArchipelagoClient client)
        {
            if (LocationHelpers.PointChecks.Count == 0) return;

            var pending = new List<LocationHelpers.PointCheck>(LocationHelpers.PointChecks);

            _ = Task.Run(async () =>
            {
                byte? lastLevel = null;
                uint? lastTotal = null;

                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (!APHelpers.isInTheGame())
                        {
                            // memory can't be trusted (loading, title, demo): start from a fresh baseline afterwards
                            lastLevel = null;
                            lastTotal = null;
                        }
                        else
                        {
                            byte level = Memory.ReadByte(Addresses.LevelId);
                            uint total = Memory.ReadUInt(Addresses.PlayerTotalTreasureCollected);

                            if (lastLevel != level || lastTotal == null)
                            {
                                CurrentLevelPoints = 0;
                                lastLevel = level;
                                lastTotal = total;
                            }
                            else if (total > lastTotal)
                            {
                                CurrentLevelPoints += (int)(total - lastTotal.Value);
                                lastTotal = total;

                                foreach (var check in pending.Where(c => c.LevelId == level && c.Points <= CurrentLevelPoints).ToList())
                                {
                                    await client.SendLocationAsync(new Location { Id = check.LocationId, Name = check.Name });
                                    pending.Remove(check);
                                }
                            }
                            else if (total < lastTotal)
                            {
                                lastTotal = total;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Points tracker failed");
                    }

                    await Task.Delay(50);
                }
            });
        }

        public static void SetUpMapListener(CancellationTokenSource cts, ArchipelagoClient client)
        {
            if (cts.Token.IsCancellationRequested) return;

            Memory.MonitorAddressForAction<byte>(
                Addresses.HasStartedGame,
                () =>
                {
                    try
                    {
                        // set all levels to open
                        Memory.WriteByte(Addresses.LevelMaxChoice, 0x15);
                        // allow quitting to the hub instead of the main menu
                        Memory.WriteByte(Addresses.LevelQuitState, 0x02);

                        // give back every unlock received so far so the new game's memory matches
                        ReplayReceivedItems(client).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "New game setup failed");
                    }
                    finally
                    {
                        // the value stays 0 for a while, so wait for it to clear before re-arming
                        // or the items would be replayed again straight away
                        while (!cts.Token.IsCancellationRequested
                               && Memory.ReadByte(Addresses.HasStartedGame) == 0x00)
                        {
                            Thread.Sleep(100);
                        }

                        SetUpMapListener(cts, client);
                    }

                }, value => value == 0x00);
        }
    }
}