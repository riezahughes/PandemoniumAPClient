using Archipelago.Core;
using Archipelago.Core.Util;
using Kokuban;
using Kokuban.AnsiEscape;
using PandemoniumArchipelago;

namespace Helpers
{
    // Console commands that print information for the player (see the command loop in App.cs).
    public static class CliHelper
    {
        // heart containers: the player starts with BaseHearts and each "Progressive Health Upgrade" adds one, up to MaxHearts.
        // Adjust these if the apworld's pool differs.
        private const string Strike = "\u001b[9m";
        private const string StrikeOff = "\u001b[29m";

        private const int BaseHearts = PlayerStateHelpers.BaseHearts;
        private const int MaxHearts = 8;

        // the "items" command: which levels are unlocked, how many heart upgrades are left and, when spell pages are
        // part of the goal, how many pages have been found.
        public static void PrintItems(ArchipelagoClient client)
        {
            if (client.CurrentSession == null)
            {
                Console.WriteLine("Not connected to Archipelago yet.");
                return;
            }

            var received = client.CurrentSession.Items.AllItemsReceived;

            var checkedLocations = client.CurrentSession.Locations.AllLocationsChecked;
            bool inGame = APHelpers.isInTheGame();
            int currentLevel = inGame ? Memory.ReadByte(Addresses.LevelId) : 0;

            Console.WriteLine();
            Console.WriteLine("Levels (> = current level, break points: struck through once reached)");
            // every level with an unlock item, plus the Wishing Engine which opens once both bosses are cleared
            var levels = LocationHelpers.KeyToLevelNumber
                .Select(entry => (Name: entry.Key.Replace(" Unlock", ""), Number: entry.Value, Unlocked: received.Any(item => item.ItemName == entry.Key)))
                .Append((Name: "The Wishing Engine", Number: LocationHelpers.WishingEngineLevel, Unlocked: LocationHelpers.AreBossesCleared(client)))
                .OrderBy(level => level.Number);

            foreach (var (levelName, levelNumber, unlocked) in levels)
            {
                bool isCurrent = levelNumber == currentLevel;

                string line = $"{(isCurrent ? ">" : " ")} [{(unlocked ? "x" : " ")}] {levelNumber,2}. {levelName,-18}";

                // pointsanity break points for this level (empty when pointsanity is off)
                var breakPoints = LocationHelpers.PointChecks
                    .Where(check => check.LevelId == levelNumber)
                    .OrderBy(check => check.Points)
                    .ToList();

                if (breakPoints.Count > 0)
                {
                    line += isCurrent ? $" pts {GameStateHelpers.CurrentLevelPoints,3} |" : "         |";
                    foreach (var check in breakPoints)
                    {
                        // reached break points are struck through (ANSI has it, Kokuban doesn't)
                        line += checkedLocations.Contains(check.LocationId)
                            ? $" {Strike}{check.Points}{StrikeOff}"
                            : $" {check.Points}";
                    }
                }

                AnsiStyle style = isCurrent ? Chalk.Yellow : unlocked ? Chalk.Green : Chalk.Red;
                Console.WriteLine(style + line);
            }

            int heartUpgrades = received.Count(item => item.ItemName.Contains("Progressive Health Upgrade"));
            int heartsNeeded = Math.Max(0, MaxHearts - BaseHearts - heartUpgrades);
            Console.WriteLine();
            Console.WriteLine($"Heart upgrades: {heartUpgrades} received, {heartsNeeded} still needed for full health");

            int goal = PlayerStateHelpers.GetPlayerOptionCounts(client.Options, "goal");
            if (goal == PlayerVictoryConditions.SPELL_PAGES || goal == PlayerVictoryConditions.BOTH)
            {
                int pagesFound = received.Count(item => item.ItemName.Contains("Spellbook Page"));
                int pagesNeeded = PlayerStateHelpers.GetPlayerOptionCounts(client.Options, "spellbook_pages");
                Console.WriteLine($"Spell pages: {pagesFound}/{pagesNeeded}");
            }
            Console.WriteLine();
        }
    }
}
