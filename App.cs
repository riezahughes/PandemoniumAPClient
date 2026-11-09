// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Archipelago.Core;
using Archipelago.Core.Helpers;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.Core.Util.Overlay;
using Archipelago.Core.Util.PlatformMemory;
using PandemoniumArchipelago;
using Helpers;
using Kokuban;
using Microsoft.Extensions.Configuration;
using Serilog;

public class App
{

    // must match the game name in your ap world
    public const string GameName = "Pandemonium";

    // OVERLAY: shows Archipelago messages (items sent/received, etc.) on top of the emulator window.
    // It is on by default; set this to false to disable it. Players can also toggle it with the "overlay" command. Things to know:
    //  - It only works on Windows (Core only ships WindowsOverlayService). On macOS this flag is ignored.
    //  - It needs libcimgui.dylib / the imgui native libraries to be shipped alongside the client when
    //    publishing (add libcimgui.dylib to the macOS zip line in .github/workflows/release.yml if you ever use it there).
    //  - Position and font are set in the OverlayOptions block further down in Main.
    //  - Once it is enabled it can be switched on and off while the client runs with the "overlay" command.
    public static bool OverlayEnabled = true;

    [DllImport("libc")]
    private static extern uint geteuid();

    private static async Task Main(string[] args)
    {
        // macOS only lets root read another process's memory, so relaunch under sudo in the same terminal
        if (OperatingSystem.IsMacOS() && geteuid() != 0)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine();
            Console.WriteLine(Chalk.BgMagenta.White.Bold[$"  {GameName} Archipelago Client · macOS  "]);
            Console.WriteLine();
            Console.WriteLine(Chalk.Yellow.Bold["  ⚠  Administrator access needed"]);
            Console.WriteLine(Chalk.Gray["     macOS only lets admin apps read DuckStation's memory."]);
            Console.WriteLine();
            Console.WriteLine(Chalk.Cyan["  🔑 Enter your Mac password below."]);
            Console.WriteLine(Chalk.Gray["     Nothing will appear as you type, that's normal."]);
            Console.WriteLine();

            var startInfo = new ProcessStartInfo("/usr/bin/sudo") { UseShellExecute = false };
            startInfo.ArgumentList.Add("-p");
            startInfo.ArgumentList.Add(Chalk.Magenta.Bold["  🔒 Password: "].ToString());
            startInfo.ArgumentList.Add(Environment.ProcessPath!);
            // when launched through the dotnet host (e.g. the VS Code debugger), ProcessPath is "dotnet",
            // so the app's .dll has to be passed along too or dotnet just prints its usage text
            if (Path.GetFileNameWithoutExtension(Environment.ProcessPath!) == "dotnet")
            {
                startInfo.ArgumentList.Add(typeof(App).Assembly.Location);
            }
            foreach (var arg in args) startInfo.ArgumentList.Add(arg);

            using var elevated = Process.Start(startInfo)!;
            await elevated.WaitForExitAsync();
            Environment.Exit(elevated.ExitCode);
        }

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = $"🧙‍♂️{GameName} Archipelago Client";

        // Connection details
        string url;
        string port;
        string slot;
        string? password;

        bool firstRun = true;

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        List<ILocation> GameLocations;

        ////////////////////////////
        //
        // Main Program Flow
        //
        ////////////////////////////

        // Make sure the emulator connection is initialised

        GameClient? gameClient = null;
        bool clientInitializedAndConnected = false;
        int retryAttempt = 0;

        while (!clientInitializedAndConnected)
        {
            Console.Clear();
            retryAttempt++;
            Console.WriteLine($"\nAttempt #{retryAttempt}:");

            try
            {
                // process name of the emulator. Change this if you're using something other than Duckstation
                gameClient = new GameClient(OperatingSystem.IsMacOS() ? "DuckStation" : "duckstation-qt-x64-ReleaseLTCG");
                clientInitializedAndConnected = true;
            }
            catch (Exception)
            {
                Console.WriteLine("Could not find Duckstation open.");

                // Wait for 5 seconds before the next retry
                Thread.Sleep(5000);
            }
        }

#if DEBUG
#else
        Console.Clear();
#endif

        gameClient!.Connect();

        var archipelagoClient = new ArchipelagoClient(gameClient);

        Console.WriteLine("Successfully connected to Duckstation.");

        // get the duckstation offset
        if (OperatingSystem.IsMacOS())
        {
            // macOS memory restrictions stop Core from finding the offset, so the user has to provide it
            PlatformMemory.GlobalOffset = PromptForDuckstationOffset();
            Console.WriteLine($"Duckstation Memory Offset found at: 0x{PlatformMemory.GlobalOffset:X}");
        }
        else
        {
            try
            {
                PlatformMemory.GlobalOffset = PlatformMemory.GetDuckstationOffset();
                Console.WriteLine($"Duckstation Memory Offset found at: 0x{PlatformMemory.GlobalOffset:X}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while getting Duckstation memory offset: {ex.Message}");
                Console.WriteLine(ex); // Print full exception for debugging
            }
        }

#if DEBUG
        // auto logs in with appsettings.Local.json settings in debug builds (because laziness)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .Build();

        Console.WriteLine("Logging in using settings in appsettings.Local.json");
        url = "wss://archipelago.gg";
        port = configuration["port"] ?? "";
        slot = configuration["slot"] ?? "";
        password = configuration["pass"];
#else
        // start AP Login

        Console.WriteLine("Enter AP Domain: (archipelago.gg)");
        string? lineUrl = Console.ReadLine();

        url = string.IsNullOrWhiteSpace(lineUrl) ? "archipelago.gg" : lineUrl;

        Console.WriteLine("Enter Port: eg, 80001");
        port = Console.ReadLine() ?? "";

        Console.WriteLine("Enter Slot Name:");
        slot = Console.ReadLine() ?? "";

        Console.WriteLine("Room Password:");
        string? linePassword = Console.ReadLine();
        password = string.IsNullOrWhiteSpace(linePassword) ? null : linePassword;

        Console.WriteLine("Details:");
        Console.WriteLine($"URL:{url}:{port}");
        Console.WriteLine($"Slot: {slot}");
        Console.WriteLine($"Password: {password}");

        if (string.IsNullOrWhiteSpace(slot))
        {
            Console.WriteLine("Slot name cannot be empty. Please provide a valid slot name.");
            return;
        }
#endif

        Console.WriteLine("Got the details! Attempting to connect to Archipelagos main server");

        try
        {
            await archipelagoClient.Connect(url + ":" + port, GameName);

            Thread.Sleep(1000);

            await archipelagoClient.Login(slot, password!);

            int retryCount = 0;
            Console.WriteLine("Waiting for connection...");
            while (!archipelagoClient.IsLoggedIn)
            {
                if (retryCount >= 10)
                {
                    throw new Exception("The client was unable to log into Archipelago. Please make sure your room is running, that you are putting in the correct details and that you are online.");
                }
                await archipelagoClient.Login(slot, password!);
                retryCount++;
                Console.Write(".");
                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn error occurred while connecting to Archipelago: {ex.Message}");
#if DEBUG
            Console.WriteLine(ex); // Print full exception for debugging
#endif
            Console.ReadKey();
            Environment.Exit(1);
        }

        // Register event handlers (must be after Connect so ItemManager/LocationManager are initialized)
        archipelagoClient.Connected += (sender, args) => APHelpers.OnConnectedLogic(sender, args, archipelagoClient);
        archipelagoClient.Disconnected += (sender, args) => APHelpers.OnDisconnectedLogic(sender, args, archipelagoClient, firstRun);
        archipelagoClient.ItemManager.ItemReceived += (sender, args) => APHelpers.ItemReceivedLogic(sender, args, archipelagoClient);
        archipelagoClient.MessageReceived += (sender, args) => APHelpers.Client_MessageReceivedLogic(sender, args, archipelagoClient, slot);
        archipelagoClient.LocationManager.LocationCompleted += (sender, args) => APHelpers.Client_LocationCompletedLogic(sender, args, archipelagoClient);
        archipelagoClient.LocationManager.EnableLocationsCondition = () => APHelpers.isInTheGame();

        try
        {
            while (archipelagoClient.CurrentSession == null)
            {
                Console.WriteLine("Waiting for current session");
                Thread.Sleep(1000);
            }

            archipelagoClient.CurrentSession.Locations.CheckedLocationsUpdated += APHelpers.Locations_CheckedLocationsUpdated;

            GameLocations = LocationHelpers.BuildLocationList(archipelagoClient.Options);

#if DEBUG
#else
            Console.Clear();
#endif

            // OVERLAY: this block only runs when OverlayEnabled is true (see the top of this class).
            ToggleableOverlayService? gameOverlay = null;
            if (OverlayEnabled && OperatingSystem.IsWindows())
            {
                var overlayOptions = new OverlayOptions();

                overlayOptions.XOffset = 50; // pixels from the left of the emulator window
                overlayOptions.YOffset = 160; // pixels from the top of the emulator window
                overlayOptions.FontSize = 12;
                overlayOptions.DefaultTextColor = Archipelago.Core.Util.Overlay.Color.White;

                gameOverlay = new ToggleableOverlayService(() => new WindowsOverlayService(overlayOptions));
                archipelagoClient.InitializeOverlayService(gameOverlay);

                // optional: use a custom font by putting a .ttf in an Assets folder, copying it to the output
                // directory in the csproj and uncommenting these lines
                // var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "MyFont.ttf");
                // gameOverlay.CreateFont(fontPath, 12);
            }

            // Core only delivers items once told the game is ready. Wait for the game to be loaded so first-run
            // items aren't written into memory that gets wiped, then keep retrying if delivery is halted.
            _ = Task.Run(async () =>
            {
                APHelpers.NeedsItemRetry = true;
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (APHelpers.NeedsItemRetry && APHelpers.isInTheGame())
                    {
                        APHelpers.NeedsItemRetry = false;
                        await archipelagoClient.ReceiveReady();
                    }
                    await Task.Delay(2000);
                }
            });

            firstRun = false;

            gameOverlay?.AddTextPopup($"{GameName} is ready to go.");

            GameStateHelpers.SetUpLevelSelectListener(cancellationTokenSource, archipelagoClient);
            GameStateHelpers.SetUpMapListener(cancellationTokenSource, archipelagoClient);
            GameStateHelpers.SetUpPointsTracker(cancellationTokenSource, archipelagoClient);
            GameStateHelpers.SetUpGoalTracker(cancellationTokenSource, archipelagoClient);

            // underscore runs the monitor locations task in the background. You can change _= to await if you want to watch it more directly.
            _ = archipelagoClient.MonitorLocationsAsync(GameLocations);

            // Simple commands for interacting can go here
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                var input = Console.ReadLine();
                if (input?.Trim().ToLower() == "exit")
                {
                    cancellationTokenSource.Cancel();
                    break;
                }
                else if (input?.Trim().ToLower() == "release")
                {
                    Console.WriteLine("Manually sending goal completion ping to Archipelago server...");
                    archipelagoClient.SendGoalCompletion();
                }
                else if (input?.Trim().ToLower().Contains("hint") == true)
                {
                    string hintString = input.Trim().ToLower() == "hint" ? "!hint" : $"!hint {input.Substring(5).Trim()}";
                    _ = archipelagoClient.SendMessage(hintString);
                }
                else if (input?.Trim().ToLower() == "items")
                {
                    CliHelper.PrintItems(archipelagoClient);
                }
                else if (input?.Trim().ToLower() == "overlay")
                {
                    if (gameOverlay == null)
                    {
                        Console.WriteLine("The game overlay isn't available (see OverlayEnabled at the top of Program).");
                    }
                    else
                    {
                        gameOverlay.Enabled = !gameOverlay.Enabled;
                        Console.WriteLine($"Game overlay {(gameOverlay.Enabled ? "enabled" : "disabled")}.");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine($"Unknown command: '{input}'");
                }
            }
        }
        catch (Exception ex)
        {
            cancellationTokenSource.Cancel();
            Console.WriteLine($"An unexpected error occurred while connecting to Archipelago: {ex.Message}");
            Console.WriteLine(ex); // Print full exception for debugging
        }
        finally
        {
            // Perform any necessary cleanup here
            Console.WriteLine("Shutting down...");
        }
    }

    private static ulong PromptForDuckstationOffset()
    {
        Console.WriteLine();
        Console.WriteLine(Chalk.BgMagenta.White.Bold["  DuckStation Memory Address  "]);
        Console.WriteLine();
        Console.WriteLine(Chalk.Yellow.Bold["  ⚠  macOS needs the DuckStation memory address entered manually"]);
        Console.WriteLine();
        Console.WriteLine(Chalk.Cyan["  1. "] + "In DuckStation, open " + Chalk.Bold["Tools > Memory Scanner"]);
        Console.WriteLine(Chalk.Cyan["  2. "] + "Find this line in the bottom right:");
        Console.WriteLine(Chalk.Gray["       Address of RAM for HxD Usage: "] + Chalk.Green.Bold["0x000000011c400000"]);
        Console.WriteLine(Chalk.Cyan["  3. "] + "Paste the " + Chalk.Green.Bold["0x..."] + " address (or the whole line) below");
        Console.WriteLine();

        while (true)
        {
            Console.Write(Chalk.Magenta.Bold["  📍 Address: "]);
            string input = Console.ReadLine() ?? "";

            // accept the full "Address of RAM for HxD Usage: 0x..." line as well as just the address
            int colonIndex = input.LastIndexOf(':');
            if (colonIndex >= 0)
            {
                input = input.Substring(colonIndex + 1);
            }

            input = input.Trim();
            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(2);
            }

            if (ulong.TryParse(input, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong offset) && offset != 0)
            {
                return offset;
            }

            Console.WriteLine(Chalk.Red.Bold["  ✖  That doesn't look like a valid address. "] + Chalk.Gray["It should look like: "] + Chalk.Green["0x000000011c400000"]);
        }
    }
}
