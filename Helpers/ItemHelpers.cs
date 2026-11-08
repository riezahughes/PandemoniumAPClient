using Archipelago.Core;
using Archipelago.Core.Traps;
using Archipelago.Core.Util;
using Archipelago.Core.Models;
using Kokuban;
using PandemoniumArchipelago;
namespace Helpers
{
    public class ItemHelpers
    {
        public static bool TryGiveItem(Item item, ArchipelagoClient client)
        {
            //// Try to handle the item based on type
            if (item.Name.Contains("Progressive Health"))
                return PlayerStateHelpers.UpdatePlayerMaxHealth(client);
            else if (item.Name.Contains("Free Health"))
                return PlayerStateHelpers.UpdatePlayerCurrentHealth(client);
            else if (item.Name.Contains("Transformation Unlock"))
                return PlayerStateHelpers.UpdatePlayerTransformations(client);
            else if (item.Name.Contains("Extra Life"))
            {
                return PlayerStateHelpers.UpdatePlayerLives(client);
            }
            else if (item.Name.Contains("Fargus") || item.Name.Contains("Nikki"))
            {
                return PlayerStateHelpers.UpdatePlayerUnlocks(client);
            }
            else if (Weapons.Keys.FirstOrDefault(itm => item.Name.Contains(itm)) is string match)
            {
                GiveWeapon(match);
                return true;
            }

            else if (item.Name.Contains("Spellbook Page"))
            {
                Console.WriteLine("Recieved Spellbook Page!");
                return true;
            }
            else if (item.Name.Contains("Unlock"))
            {
                Console.WriteLine($"Recieved {item.Name} Key!");
                return true;
            }
            else
            {
                Console.WriteLine($"Item not recognised. ({item.Name}) Skipping");
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

