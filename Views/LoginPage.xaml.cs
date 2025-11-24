using CryptoSafe.Services;
namespace CryptoSafe.Views;

public partial class LoginPage : ContentPage
{
    private readonly UserService userService;

    public LoginPage()
    {
        InitializeComponent();
        userService = new UserService();
    }

    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter both email and password.", "OK");
            return;
        }

        // Validate login credentials
        bool isValidUser = await userService.ValidateLoginAsync(email, password);

        if (isValidUser)
        {
            // Retrieve the user from the database
            var user = await userService.GetUserByEmailAsync(email);

            if (user != null)
            {
                // Set the global user ID after retrieving the user object
                App.CurrentUserId = user.Id.ToString();

                // Navigate to the homepage
                await DisplayAlert("Success", "Login successful!", "OK");
                await Shell.Current.GoToAsync("//HomePage");

                EmailEntry.Text = string.Empty;
                PasswordEntry.Text = string.Empty;
            }
            else
            {
                await DisplayAlert("Error", "Unable to retrieve user details.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error", "Invalid email or password. Please try again.", "OK");
        }
    }

    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        // Navigate to the registration page
        await Shell.Current.GoToAsync("//RegisterPage");
    }
}