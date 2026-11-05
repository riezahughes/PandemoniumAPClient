using Archipelago.Core;
using Archipelago.Core.Traps;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.Models;
using Kokuban;
using PandemoniumArchipelago;
namespace Helpers
{
    public class ItemHelpers
    {
        private static bool TryGiveItem(ItemInfo item, ArchipelagoClient client)
        {
            //// Try to handle the item based on type
            if (item.ItemName.Contains("Progressive Health Upgrade"))
                return PlayerStateHelpers.UpdatePlayerMaxHealth(client);
            else if (item.ItemName.Contains("Free Health"))
                return PlayerStateHelpers.UpdatePlayerCurrentHealth(client);
            else if (item.ItemName.Contains("Transformation Unlock"))
                return PlayerStateHelpers.UpdatePlayerTransformations(client);
            else if (item.ItemName.Contains("Fargus") || item.ItemName.Contains("Nikki"))
            {
                return PlayerStateHelpers.UpdatePlayerUnlocks(client);
            }
            else if (WeaponBitflags.Keys.FirstOrDefault(itm => item.ItemName.Contains(itm)) is string match)
            {
                GiveWeapon(match);
                return true;
            }

            else if (item.ItemName.Contains("Spellbook Page"))
            {
                Console.WriteLine("Recieved Shard!");
                return true;
            }
            else if (item.ItemName.Contains("Unlock"))
            {
                Console.WriteLine($"Recieved {item.ItemName} Key!");
                return true;
            }
            else
            {
                Console.WriteLine($"Item not recognised. ({item.ItemName}) Skipping");
                return true; // Skip unrecognized items
            }
        }



        public struct ItemLocation
        {
            public uint Address;
            public int Bit;

            public ItemLocation(uint address, int bit)
            {
                Address = address;
                Bit = bit;
            }
        }

        public static bool GiveWeapon(string itemName)
        {
            ItemLocation itemData = WeaponBitflags.GetValueOrDefault(itemName);
            Memory.WriteBit(itemData.Address, itemData.Bit, true);
            return true;
        }

        public static void ProcessPendingItems(ArchipelagoClient client)
        {
            if (client.CurrentSession == null)
            {
                return;
            }

            var allItems = client.CurrentSession.Items.AllItemsReceived;

            // Process items from our last saved index up to what we've received
            while (App.ProcessedItemIndex < allItems.Count)
            {
                var itemToProcess = allItems[App.ProcessedItemIndex];

                // Try to give the item
                bool success = TryGiveItem(itemToProcess, client);

                if (success)
                {
                    // Only increment if we successfully gave the item
                    App.ProcessedItemIndex++;
                    Memory.Write(Addresses.ItemIndexStorage, (ushort)App.ProcessedItemIndex);
#if DEBUG
                    Console.WriteLine($"Successfully processed item {App.ProcessedItemIndex}/{allItems.Count}");
#endif
                }
                else
                {
                    // Inventory full - stop processing and wait
                    Kokuban.AnsiEscape.AnsiStyle bg = Chalk.BgMagenta;
                    Kokuban.AnsiEscape.AnsiStyle fg = Chalk.White;
                    Console.WriteLine(bg + (fg + $"⚠️ Cannot process item {itemToProcess.ItemName}"));
                    break; // Exit the loop, we'll try again later
                }
            }

            APHelpers.PROCESSING_ITEM_LIST = false;
        }

        public static readonly Dictionary<int, string> LevelToKeyName = new Dictionary<int, string>
        {
            { 1, "Skull Fortress Unlock"},
            { 2, "Hollow Stairway Unlock"},
            { 3, "Dungeon Tower Unlock"},
            { 4, "Lost Caves Unlock"},
            { 5, "Fungus Grotto Unlock"},
            { 6, "Acid Pools Unlock"},
            { 7, "Shroom Lord Unlock"},
            { 8, "Burning Desert Unlock"},
            { 9, "Branky Wastes Unlock"},
            { 10, "Spider Forest Unlock"},
            { 11, "Canopy Village Unlock"},
            { 12, "Soldier Barracks Unlock"},
            { 13, "Honcho's Logmill Unlock"},
            { 14, "Goon Honcho Unlock"},
            { 15, "Honcho's Airship Unlock"},
            { 16, "Dragoon Skyfort Unlock"},
            { 17, "Cloud Citadel Unlock"},
            { 18, "Efreet Palace Unlock"},
            { 19, "Frozen Cavern Unlock"},
            { 20, "Storm Temple Unlock"}
        };

        public static readonly Dictionary<string, int> KeyToLevelNumber = new Dictionary<string, int>
        {
            {"Skull Fortress Unlock", 1 },
            {"Hollow Stairway Unlock", 2 },
            {"Dungeon Tower Unlock", 3 },
            {"Lost Caves Unlock", 4 },
            {"Fungus Grotto Unlock", 5 },
            {"Acid Pools Unlock", 6 },
            {"Shroom Lord Unlock", 7 },
            {"Burning Desert Unlock", 8 },
            {"Branky Wastes Unlock", 9 },
            { "Spider Forest Unlock", 10},
            { "Canopy Village Unlock", 11},
            { "Soldier Barracks Unlock", 12},
            { "Honcho's Logmill Unlock", 13},
            { "Goon Honcho Unlock", 14},
            { "Honcho's Airship Unlock", 15},
            { "Dragoon Skyfort Unlock", 16},
            { "Cloud Citadel Unlock", 17},
            { "Efreet Palace Unlock", 18},
            { "Frozen Cavern Unlock", 19},
            { "Storm Temple Unlock", 20}
        };
        public static Dictionary<string, ItemLocation> WeaponBitflags = new Dictionary<string, ItemLocation>
        {
            // Address 0x12DA3DA
            { "Shaman's Mask",        new ItemLocation(Addresses.WeaponArray1, 1) },
            { "Mask of Strength",     new ItemLocation(Addresses.WeaponArray1, 2) },
            { "Mask of Power",        new ItemLocation(Addresses.WeaponArray1, 3) },
            { "Shaman's Cloak",       new ItemLocation(Addresses.WeaponArray1, 5) },
            { "Cloak of Force",       new ItemLocation(Addresses.WeaponArray1, 6) },
            { "Cloak of Deception",   new ItemLocation(Addresses.WeaponArray1, 7) },
        };
    }
}

