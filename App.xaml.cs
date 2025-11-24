namespace CryptoSafe
{
    public partial class App : Application
    {
        public static string CurrentUserId { get; set; } 
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
