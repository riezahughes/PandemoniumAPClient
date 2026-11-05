using Archipelago.Core;
using Serilog;
using Helpers;
using Archipelago.Core.Models;
using Kokuban;

namespace Helpers
{
    public class APHelpers
    {

        public static bool PROCESSING_ITEM_LIST = false;
        public static Boolean isInTheGame()
        {
            return true;
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

            if (isInTheGame())
            {
                return;
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

        public static void ItemReceivedLogic(object? sender, ItemReceivedEventArgs args, ArchipelagoClient client)
        {
            if (client.CurrentSession == null)
            {
                return;
            }
            PROCESSING_ITEM_LIST = true;
            ItemHelpers.ProcessPendingItems(client);
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
            Console.WriteLine("Location Completed");
        }
        public static void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations)
        {
            Console.WriteLine($"Location CheckedLocationsUpdated Firing.");
        }
    }
}