using Archipelago.Core;
using Serilog;
using Helpers;
using Archipelago.Core.Models;
using Kokuban;
using System.Buffers;
using Archipelago.Core.Util;
using PandemoniumArchipelago;

namespace Helpers
{
    public class APHelpers
    {

        // set when Core's item delivery was halted (game not loaded / item couldn't be applied) and needs ReceiveReady() again
        public static volatile bool NeedsItemRetry = false;
        public static Boolean isInTheGame()
        {
            byte state = Memory.ReadByte(Addresses.BiosLoaded);
            byte demoCheck = Memory.ReadByte(Addresses.DemoCheck);
            uint titleScreenCheck = Memory.ReadUInt(Addresses.MainMenuCheck);

            if (state == 0x80 && demoCheck == 0x0000 && titleScreenCheck > 0x0002)
            {
                return true;

            }
            return false;
        }

        public static async void OnConnectedLogic(object? sender, EventArgs args, ArchipelagoClient client)
        {
            if (client.CurrentSession == null)
            {
                return;
            }
            Log.Logger.Information("Connected to Archipelago");
            Log.Logger.Information($"Playing {client.CurrentSession.ConnectionInfo.Game} as {client.CurrentSession.Players.GetPlayerName(client.CurrentSession.ConnectionInfo.Slot)}");

            // if deathlink goes here
            int deathlink = int.Parse(client.Options?.GetValueOrDefault("deathlink", "0").ToString() ?? "0");

            if (deathlink == 1)
            {
                var deathLink = client.EnableDeathLink();
                deathLink.OnDeathLinkReceived += (args) => PlayerStateHelpers.KillPlayer();
            }

            // a reconnect resets max health to what has actually been unlocked
            if (isInTheGame())
            {
                PlayerStateHelpers.ResetPlayerMaxHealth(client);
            }
        }

        public static async void OnDisconnectedLogic(object? sender, ConnectionChangedEventArgs args, ArchipelagoClient client, bool firstRun)
        {
            if (firstRun)
            {
                return;
            }
            if (!client.IsLoggedIn || !client.IsConnected)
            {
                Console.WriteLine("Disconnected from Archipelago. Please restart client.");
            }
        }

        // Core tracks which items have been delivered (saved index) and calls this once per new item.
        // Setting Success = false halts delivery until ReceiveReady() is called again.
        public static void ItemReceivedLogic(object? sender, ItemReceivedEventArgs args, ArchipelagoClient client)
        {
            if (client.CurrentSession == null || !isInTheGame())
            {
                args.Success = false;
                NeedsItemRetry = true;
                return;
            }

            bool success = ItemHelpers.TryGiveItem(args.Item, client);
            args.Success = success;
            if (!success)
            {
                NeedsItemRetry = true;
            }
        }

        public static void Client_MessageReceivedLogic(object? sender, MessageReceivedEventArgs e, ArchipelagoClient client, string slot)
        {

            if (client.CurrentSession == null)
            {
                return;
            }

            var message = string.Join("", e.Message.Parts.Select(p => p.Text));

            // sends the message to the in-game overlay. Only does anything if Program.OverlayEnabled is true (see App.cs)
            if (App.OverlayEnabled && OperatingSystem.IsWindows())
            {
                client.AddOverlayMessage(e.Message.ToString());
            }

            // adds coloured messages to terminal
            string prefix;
            Kokuban.AnsiEscape.AnsiStyle bg;
            Kokuban.AnsiEscape.AnsiStyle fg = Chalk.White;

            if (message.Contains($"{slot} found") || message.Contains($"{slot} sent"))
            {
                bg = message.Contains("Trap:") ? Chalk.BgRed : message.Contains("Congratulations") ? Chalk.Yellow : Chalk.BgBlue;
                prefix = " >> ";
            }
            else
            {
                bg = message.Contains("Trap:") ? Chalk.BgRed : message.Contains("Congratulations") ? Chalk.Yellow : Chalk.BgGreen;
                prefix = " << ";
            }

            Console.WriteLine(bg + (fg + $"{prefix} {message} "));
        }

        public static void Client_LocationCompletedLogic(object? sender, LocationCompletedEventArgs e, ArchipelagoClient client)
        {
            if (client.CurrentSession == null)
            {
                return;
            }
# if DEBUG            
            Console.WriteLine($"Location Completed: {e.CompletedLocation.Name}");
# endif
        }
        public static void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations)
        {
# if DEBUG                
            Console.WriteLine($"Location CheckedLocationsUpdated Firing.");
# endif            
        }
    }
}