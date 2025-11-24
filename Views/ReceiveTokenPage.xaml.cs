namespace CryptoSafe.Views;

public partial class ReceiveTokenPage : ContentPage
{
    public string UserId { get; set; }

    public ReceiveTokenPage()
    {
        InitializeComponent();
        UserId = App.CurrentUserId;

        UserIdLabel.Text = UserId;
    }

    private async void OnCopyButtonClicked(object sender, EventArgs e)
    {
        await Clipboard.SetTextAsync(UserId);
        await DisplayAlert("Copied", "User ID copied to clipboard.", "OK");
    }
}