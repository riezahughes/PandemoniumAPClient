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
            else if (Weapons.Keys.FirstOrDefault(itm => item.ItemName.Contains(itm)) is string match)
            {
                GiveWeapon(match);
                return true;
            }

            else if (item.ItemName.Contains("Spellbook Page"))
            {
                Console.WriteLine("Recieved Spellbook Page!");
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



        // Pandemonium has no weapon inventory: there is one active power up slot (PowerUpType + PowerUpActive),
        // and the shield is its own value. Giving a weapon writes its value to its address.
        public struct WeaponPowerUp
        {
            public uint Address;
            public ushort Value;
            public bool IsShort; // false = byte write
            public bool SetsPowerUpActive; // power ups also need PowerUpActive set to 01

            public WeaponPowerUp(uint address, ushort value, bool isShort = false, bool setsPowerUpActive = true)
            {
                Address = address;
                Value = value;
                IsShort = isShort;
                SetsPowerUpActive = setsPowerUpActive;
            }
        }

        public static bool GiveWeapon(string itemName)
        {
            WeaponPowerUp weapon = Weapons[itemName];

            if (weapon.IsShort)
                Memory.Write(weapon.Address, weapon.Value);
            else
                Memory.WriteByte(weapon.Address, (byte)weapon.Value);

            // set the type before switching the power up on, so the game never sees the old type as active
            if (weapon.SetsPowerUpActive)
                Memory.WriteByte(Addresses.PowerUpActive, 0x01);

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

        // the weapon items in the apworld (Items.py, PandemoniumItemCategory.WEAPON)
        public static Dictionary<string, WeaponPowerUp> Weapons = new Dictionary<string, WeaponPowerUp>
        {
            // the code notes only list one fireball value (0x00), so both fireballs use it for now
            { "Eldritch Fireball",    new WeaponPowerUp(Addresses.PowerUpType, 0x00) },
            { "Phoenix Fireball",     new WeaponPowerUp(Addresses.PowerUpType, 0x00) },
            { "Freeze Ray",           new WeaponPowerUp(Addresses.PowerUpType, 0x0b) },
            { "Shrink Ray",           new WeaponPowerUp(Addresses.PowerUpType, 0x0c) },
            // the shield isn't part of the power up slot; 0xfa68 is what the notes show while it's active
            { "Invincibility Shield", new WeaponPowerUp(Addresses.ShieldState, 0xfa68, isShort: true, setsPowerUpActive: false) },
        };
    }
}

