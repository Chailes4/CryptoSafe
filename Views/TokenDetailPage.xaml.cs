using CryptoSafe.Services;
using CryptoSafe.Models;
using CommunityToolkit.Maui.Views; 

namespace CryptoSafe.Views;

public partial class TokenDetailPage : ContentPage
{
    public string UserId { get; set; }

    public TokenDetailPage(CryptoToken token)
    {
        InitializeComponent();
        BindingContext = token;
        UserId = App.CurrentUserId;

        UserIdLabel.Text = UserId;
    }

    private async void OnSendReceiveButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SendTokenPage());

    }

    private async void OnReceiveButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ReceiveTokenPage());
    }

    private async void OnSwapButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SwapPage());
    }
}