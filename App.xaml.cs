namespace ScpServerTools
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            const int newWidth = 560;
            const int newHeight = 800;

            window.Width = newWidth;
            window.Height = newHeight;

            window.MinimumHeight = newHeight;
            window.MaximumHeight = newHeight;

            window.MinimumWidth = newWidth;
            window.MaximumWidth = newWidth;
            return window;
        }
    }
}
