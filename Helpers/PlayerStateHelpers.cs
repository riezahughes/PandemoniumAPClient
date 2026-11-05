using Archipelago.Core;
using Archipelago.MultiClient.Net.Models;

namespace Helpers
{
    public class PlayerStateHelpers
    {
        public static void KillPlayer()
        {
            //TODO: Kill the player logic goes here
            Console.WriteLine("Ur ded kiddo");
        }

        public static void UpdatePlayerState(System.Collections.ObjectModel.ReadOnlyCollection<ItemInfo> itemsCollected)
        {
            //TODO: Player update logic
            Console.WriteLine("Player Status updated");
        }

        public static T GetPlayerOption<T>(Dictionary<string, object> options, string optionKey, T defaultValue = default) where T : struct, Enum
        {
            string optionValue = options?.GetValueOrDefault(optionKey, "0").ToString();

            if (Enum.TryParse<T>(optionValue, out T result))
            {
                return result;
            }

            return defaultValue;
        }

        public static int GetPlayerOptionCounts(Dictionary<string, object> options, string optionKey, int defaultValue = 0)
        {
            string optionValue = options?.GetValueOrDefault(optionKey, "0")?.ToString() ?? "0";

            if (Int32.TryParse(optionValue, out int result))
            {
                return result;
            }

            return defaultValue;
        }

        public static bool UpdatePlayerMaxHealth(ArchipelagoClient client)
        {
            return true;
        }

        public static bool UpdatePlayerCurrentHealth(ArchipelagoClient client)
        {
            return true;
        }

        public static bool UpdatePlayerCurrentWeapon(ArchipelagoClient client)
        {
            return true;
        }
        public static bool UpdatePlayerTransformations(ArchipelagoClient client)
        {
            return true;
        }

        public static bool UpdatePlayerUnlocks(ArchipelagoClient client)
        {
            return true;
        }

        public static bool UpdatePlayerKeys(ArchipelagoClient client)
        {
            return true;
        }
        public static bool UpdatePlayerLives(ArchipelagoClient client)
        {
            return true;
        }
        public static bool UpdatePlayerSpellPages(ArchipelagoClient client)
        {
            return true;
        }

    }

}
