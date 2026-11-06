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
            else if (item.ItemName.Contains("Extra Life"))
            {
                return PlayerStateHelpers.UpdatePlayerLives(client);
            }
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

