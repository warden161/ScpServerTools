namespace ScpServerTools.Pages;

public partial class ServerChoice : ContentPage
{
	List<ushort> Ports;
	public ServerChoice(List<ushort> ports)
	{
		InitializeComponent();
		Ports = ports;

        foreach (var port in ports)
		{
            var button = new Button
            {
                Text = $"{port}",
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };

            // Handle button click
            button.Clicked += (sender, args) =>
            {
                OnPortSelected(port);
            };

            ButtonLayout.Children.Add(button);
        }
	}

    public async void OnPortSelected(ushort port)
    {
        MainPage.ServerPort = port;
        var manager = new ServerManager();
        NavigationPage.SetHasBackButton(manager, false);
        await Navigation.PushAsync(manager);
    }
}