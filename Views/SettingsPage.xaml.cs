using CryptoSafe.Services;
namespace CryptoSafe.Views;

public partial class SettingsPage : ContentPage
{

    public SettingsPage()
    {
        InitializeComponent();
        // Default initialization
    }
    private readonly UserService _userService;
    public SettingsPage(UserService userService)
    {
        InitializeComponent();
        _userService = userService;
    }

    private async void OnAccountTapped(object sender, EventArgs e)
    {
        //var accountPage = new AccountPage(_userService);
        //await Navigation.PushAsync(accountPage);
    }

    private async void OnAboutTapped(object sender, EventArgs e)
    {
       // await Shell.Current.GoToAsync(nameof(AboutPage));
    }

    private async void OnLogOutTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}