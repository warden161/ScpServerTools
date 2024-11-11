using Microsoft.Maui.Controls;
using ScpServerTools.Pages.Subpages;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace ScpServerTools.Pages;

public partial class ServerManager : ContentPage
{
	public ServerManager()
	{
		InitializeComponent();
		ServerPort.Text = $"Server Port: {MainPage.ServerPort.ToString()}";
		InstallationPath.Text = $"SCPSL Path: {MainPage.InstallFolder.ToString()}";
        ConfigPath.Text = $"Config Path: {MainPage.ConfigFolder.ToString()}";
    }

    private async void OnAddAdmin(object sender, EventArgs e)
    {
        var text = AdminInput.Text;
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (!text.Contains("@"))
            text += "@steam";

        var path = Path.Combine(MainPage.ConfigFolder, MainPage.ServerPort.ToString(), "config_remoteadmin.txt");
        string fileContent = File.ReadAllText(path);
        if (fileContent.Contains(text))
            fileContent = Regex.Replace(fileContent, $@"({text}@steam: )\S+", $"{text}@steam: owner");
        else
        {
            string newEntry = $" - {text}: owner\n";

            int membersIndex = fileContent.IndexOf("Members:");
            if (membersIndex != -1)
            {
                int endOfMembersIndex = fileContent.IndexOf("\n", membersIndex);
                fileContent = fileContent.Insert(endOfMembersIndex + 1, newEntry);
            }
        }

        File.WriteAllText(path, fileContent);
        await DisplayAlert("Done!", $"Successfully added owner rank to \"{text}\". To apply these changes, restart the server or write \"reload remoteadmin\" in the server console.", "OK");
    }

    private async void GuideSteamID64(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GuideSteamId());
    }

    private async void InfoButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UpnpExplanation());
    }

    private async void StartServer(object sender, EventArgs e)
    {
        Process.Start(new ProcessStartInfo(Path.Combine(MainPage.InstallFolder, "LocalAdmin.exe")) { Arguments = MainPage.ServerPort.ToString(), UseShellExecute = true, WorkingDirectory = MainPage.InstallFolder });
        await DisplayAlert("Alert", "ScpTools is launching the server, this may take a bit...", "OK");

    }

    private async void GetIp(object sender, EventArgs e)
    {
        var ip = $"{GetIpAddress()}:{MainPage.ServerPort}";
        await Clipboard.SetTextAsync(ip);
        await DisplayAlert("Alert", $"The IP your friends can join on is {ip} in the Direct Connect button in SCPSL. This IP has been copied to clipboard!", "OK");
    }

    private async void ReturnHome(object sender, EventArgs e)
    {
        MainPage.ServerPort = 0;
        await Navigation.PopToRootAsync();
    }

    public static string GetIpAddress()
    {
        return new WebClient().DownloadString("https://api.ipify.org");
    }

    public string PluginPath = Path.Combine(MainPage.ScpFolder, "PluginAPI", "plugins", MainPage.ServerPort.ToString());
    private const string GitHubApiUrl = "https://api.github.com/repos/warden161/NWAPI-UPnP/releases/latest";
    private static readonly HttpClient client = new HttpClient();
    private async void InstallUPnP(object sender, EventArgs e)
    {
        await DownloadLatestReleaseFile("NWAPI_UPnP.dll", PluginPath);
        await DownloadLatestReleaseFile("Open.Nat.dll", Path.Combine(PluginPath, "dependencies"));
        await DisplayAlert("Alert", "Downloaded files & installed plugin.", "OK");
    }

    public async Task DownloadLatestReleaseFile(string assetName, string savePath)
    {
        client.DefaultRequestHeaders.Add("User-Agent", "ScpServerTools");

        string response = await client.GetStringAsync(GitHubApiUrl);

        using (JsonDocument doc = JsonDocument.Parse(response))
        {
            JsonElement root = doc.RootElement;
            if (!root.TryGetProperty("assets", out JsonElement assetsElement))
            {
                Console.WriteLine("No assets found in the release.");
                return;
            }

            string assetUrl = null;
            foreach (JsonElement asset in assetsElement.EnumerateArray())
            {
                string assetNameInRelease = asset.GetProperty("name").GetString();
                if (assetNameInRelease == assetName)
                {
                    assetUrl = asset.GetProperty("browser_download_url").GetString();
                    break;
                }
            }

            if (assetUrl == null)
            {
                Debug.WriteLine($"Asset '{assetName}' not found in the latest release.");
                return;
            }

            await DownloadFile(assetUrl, Path.Combine(savePath, assetName));
            Debug.WriteLine($"File '{assetName}' downloaded successfully to {savePath}.");
        }
    }

    private async Task DownloadFile(string url, string savePath)
    {
        // Download the file and save it locally
        byte[] fileBytes = await client.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(savePath, fileBytes);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var folderUri = new Uri($"file:///{PluginPath}");
        await Launcher.OpenAsync(folderUri);
    }
}