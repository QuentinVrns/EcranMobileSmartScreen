namespace EcranMobileSmartScreen
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Utiliser NavigationPage pour activer la navigation entre les pages
            MainPage = new NavigationPage(new MainPage());
        }
    }
}
