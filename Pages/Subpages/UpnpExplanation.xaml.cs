namespace ScpServerTools.Pages.Subpages;

public partial class UpnpExplanation : ContentPage
{
	public UpnpExplanation()
	{
		InitializeComponent();
	}

    public async void OnUrlTapped(object sender, EventArgs e)
    {
        await Launcher.OpenAsync("https://www.lifewire.com/enable-upnp-on-a-router-5206124");
    }
}