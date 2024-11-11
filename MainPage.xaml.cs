using Microsoft.Win32;
using ScpServerTools.Pages;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ScpServerTools
{
    public partial class MainPage : ContentPage
    {
        public static MainPage Instance { get; private set; }
        public static ushort ServerPort { get; internal set; } = 0;
        public static string AppData { get; private set; }
        public static string ScpFolder { get; private set; }
        public static string ConfigFolder { get; private set; }
        public static string InstallFolder { get; internal set; }

        //todo: check scpsl installation
        // find folder: https://chatgpt.com/share/67313d1c-6320-8005-9e13-e0db0fc185ea
        // launch game installer: 
        public MainPage()
        {
            InitializeComponent();
            Instance = this;
        }

        public async Task Load(string text)
        {
            var load = new Loading(text);
            NavigationPage.SetHasBackButton(load, false);
            await Navigation.PushAsync(load);
        }

        public async Task<bool> Check()
        {
            AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ScpFolder = Path.Combine(AppData, "SCP Secret Laboratory");
            ConfigFolder = Path.Combine(ScpFolder, "config");
            InstallFolder = ScpInstallPath();

            if (InstallFolder != "none" && InstallFolder != null)
                return true;

            var page = new InstallScp();
            NavigationPage.SetHasBackButton(page, false);
            await Navigation.PushAsync(page);
            return false;
        }

        public async void OnBeginClicked(object sender, EventArgs e)
        {
            var valid = await Check();
            if (valid)
            {
                if (ServerPort == 0)
                {
                    List<ushort> ports = new();
                    var portFolders = Directory.GetDirectories(ConfigFolder);
                    foreach (var portPath in portFolders)
                    {
                        var port = Path.GetFileName(Path.TrimEndingDirectorySeparator(portPath));

                        if (port == "nondedicated" || port == "0")
                            continue;

                        try
                        {
                            ports.Add(Convert.ToUInt16(port));
                        }
                        catch (FormatException ex) { Debug.WriteLine(ex); };
                    }

                    if (ports.Count > 1)
                    {
                        var page = new ServerChoice(ports);
                        NavigationPage.SetHasBackButton(page, false);
                        await Navigation.PushAsync(page);
                        return;
                    }

                    if (ports.Count == 0)
                    {
                        await DisplayAlert("Error", "You haven't run the SCP:SL dedicated server at least once. Please do that before attempting setup.", "OK");
                        return;
                    }

                    ServerPort = ports.First();
                }
                var manager = new ServerManager();
                NavigationPage.SetHasBackButton(manager, false);
                await Navigation.PushAsync(manager);
            }
        }
 
        public string ScpInstallPath()
        {
            string steamPath = GetSteamPath();

            if (steamPath == null)
                return "";

            string libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

            if (!File.Exists(libraryFoldersPath))
                return "";

            Debug.WriteLine("Found libraryfolders.vdf at: " + libraryFoldersPath);
            var libraryPaths = GetSteamLibraryFolders(libraryFoldersPath);

            if (libraryPaths == null)
                return "";

            foreach (var libraryPath in libraryPaths)
            {
                string gameFolderPath = Path.Combine(libraryPath, "steamapps", "common", "SCP Secret Laboratory Dedicated Server");

                Debug.WriteLine(gameFolderPath);
                if (Directory.Exists(gameFolderPath))
                    return gameFolderPath;
            }

            return "none";
        }

        public static string GetSteamPath()
        {
            string text = Environment.Is64BitOperatingSystem ? "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam" : "HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam";
            var key = Registry.GetValue(text, "InstallPath", null);
            return key == null ? throw new InvalidOperationException("Steam isn't installed") : (string)key;
        }

        public static List<string> GetSteamLibraryFolders(string libraryFoldersPath)
        {
            try
            {
                List<string> folders = new();
                var lines = File.ReadAllLines(libraryFoldersPath);
                foreach (var line in lines)
                {
                    var match = Regex.Match(line, "\"path\"\\s+\"([^\"]+)\"");

                    if (!match.Success)
                        continue;

                    var newLine = match.Groups[1].Value.Replace(@"\\", @"\");
                    Debug.WriteLine(newLine);
                    folders.Add(newLine);
                }

                return folders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error reading libraryfolders.vdf: " + ex.Message);
                return null;
            }
        }
    }
}
