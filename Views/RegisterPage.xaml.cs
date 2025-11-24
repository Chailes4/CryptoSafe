using System.Text.RegularExpressions;
using CryptoSafe.Services;
namespace CryptoSafe.Views;

public partial class RegisterPage : ContentPage
{
    private UserService userService;
    public RegisterPage()
    {
        InitializeComponent();
        userService = new UserService();
    }

    private async void OnCreateAccountButtonClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;
        string confirmPassword = ConfirmPasswordEntry.Text;

        // Validate if all fields are filled
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            MessageLabel.Text = "Please fill in all fields.";
            MessageLabel.IsVisible = true;
            return;
        }

        // Validate email format
        if (!IsValidEmail(email))
        {
            MessageLabel.Text = "Please enter a valid email address.";
            MessageLabel.IsVisible = true;
            return;
        }

        // Validate password length (at least 6 characters)
        if (password.Length < 6)
        {
            MessageLabel.Text = "Password must be at least 6 characters long.";
            MessageLabel.IsVisible = true;
            return;
        }

        // Ensure passwords match
        if (password != confirmPassword)
        {
            MessageLabel.Text = "Passwords do not match.";
            MessageLabel.IsVisible = true;
            return;
        }

        // Try to create the account asynchronously
        bool accountCreated = await userService.CreateAccountAsync(email, password);

        if (accountCreated)
        {
            var newUser = await userService.GetUserByEmailAsync(email);
            await DisplayAlert("Success", $"Account created successfully! Your User ID is: {newUser.Id}", "OK");
            // Navigate back to the login page
            await Shell.Current.GoToAsync("//LoginPage");
        }
        else
        {
            MessageLabel.Text = "Email already exists.";
            MessageLabel.IsVisible = true;
        }
    }

    // Email validation method using regex
    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    // Event handler for Log In button
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}