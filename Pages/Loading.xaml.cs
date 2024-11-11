namespace ScpServerTools.Pages;

public partial class Loading : ContentPage
{
	public Loading(string text)
	{
		InitializeComponent();
		LoadText.Text = text;
	}
}