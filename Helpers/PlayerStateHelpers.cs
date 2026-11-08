using Archipelago.Core;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.Models;
using PandemoniumArchipelago;

namespace Helpers
{
    public class PlayerStateHelpers
    {
        public static void KillPlayer()
        {
            Memory.WriteByte(Addresses.PlayerCurrentHealth, 0x00);
            Console.WriteLine("Ur ded kiddo");
        }

        public static void UpdatePlayerState(System.Collections.ObjectModel.ReadOnlyCollection<ItemInfo> itemsCollected)
        {
            // for each item in itemsCollected, run it through the whole 
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
            byte currentMaxHealth = Memory.ReadByte(Addresses.PlayerMaxHealth);
            int newMaxHealthTotal = currentMaxHealth + 1;

            int maxHealthContainers = GetPlayerOptionCounts(client.Options, "max_health_containers", 8);

            if (newMaxHealthTotal >= maxHealthContainers)
            {
                newMaxHealthTotal = maxHealthContainers;
            }

            Memory.WriteByte(Addresses.PlayerMaxHealth, (byte)newMaxHealthTotal);
            return true;
        }

        public static bool UpdatePlayerCurrentHealth(ArchipelagoClient client)
        {
            byte currentHealth = Memory.ReadByte(Addresses.PlayerCurrentHealth);
            byte currentMaxHealth = Memory.ReadByte(Addresses.PlayerMaxHealth);
            int newHealthTotal = currentHealth + 1;

            if (newHealthTotal >= currentMaxHealth)
            {
                newHealthTotal = currentMaxHealth;
            }
            Memory.WriteByte(Addresses.PlayerCurrentHealth, (byte)newHealthTotal);
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
            byte currentLives = Memory.ReadByte(Addresses.PlayerLives);

            int newLifeTotal = currentLives + 1;

            if (newLifeTotal >= 32)
            {
                Console.WriteLine("Lives have capped out!");

                newLifeTotal = 31;
            }

            Memory.WriteByte(Addresses.PlayerLives, (byte)newLifeTotal);
            return true;
        }

        public static bool UpdatePlayerSpellPages(ArchipelagoClient client)
        {
            return true;
        }

    }

}
