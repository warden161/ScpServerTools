using System.Diagnostics;

namespace ScpServerTools.Pages;

public partial class InstallScp : ContentPage
{
	public bool Installing;
	public InstallScp()
	{
		InitializeComponent();
	}

	public async void OnBeginClicked(object sender, EventArgs e)
	{
        await Launcher.OpenAsync("steam://install/996560");
        Installing = true;
        Description.Text = "Press the button again when installation is FULLY complete (100% downloaded).";
		CompletedBtn.IsVisible = true;
        await DisplayAlert("Alert", "ScpTools is opening Steam, please wait..", "OK");
    }

	public async void OnCompleted(object sender, EventArgs e)
	{
		if (!Installing)
			return;

        var folder = MainPage.Instance.ScpInstallPath();
        var localadmin = Path.Combine(folder, "LocalAdmin.exe");
        Debug.WriteLine(folder);
        if (!Directory.Exists(folder) || !File.Exists(localadmin))
        {
            await DisplayAlert("Error", "The game is not downloaded yet.", "OK");
            return;
        }

        MainPage.InstallFolder = folder;
        Process.Start(new ProcessStartInfo(localadmin) { Arguments = "7777", UseShellExecute = true, WorkingDirectory = folder });
        await DisplayAlert("Alert", "ScpTools is launching the server. Please close it when you see Waiting for players... in the server console & return to this window afterwards.", "OK");
        await Navigation.PopAsync();
    }
}