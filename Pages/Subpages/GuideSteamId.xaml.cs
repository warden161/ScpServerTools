namespace ScpServerTools.Pages.Subpages;

public partial class GuideSteamId : ContentPage
{
	public GuideSteamId()
	{
		InitializeComponent();
	}

	public async void OnUrlTapped(object sender, EventArgs e)
	{
        await Launcher.OpenAsync("https://www.youtube.com/watch?v=FTuiA5k0R_k");
    }
}