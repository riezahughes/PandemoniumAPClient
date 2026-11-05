using Archipelago.Core;
using Serilog;
using Helpers;
using Archipelago.Core.Models;

namespace Helpers
{
    public class GameStateHelpers
    {
        internal static bool CheckCreditsCondition(ArchipelagoClient client)
        {
            return true;
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

    }
}